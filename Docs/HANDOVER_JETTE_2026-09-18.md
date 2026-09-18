# 「빨리가자 제때 런」(jette) 인수인계 — 2026-09-18 (1차)

**이 폴더(`C:\dev\game-jette`)는 본편 레포의 git worktree 다.** 브랜치 `jette`, 리모트 `jette` = https://github.com/kk00701903-hub/game-jette.git (아직 빈 레포).
본편(`C:\dev\game`, main, origin=game-costrunner)에서 `6d5ff03`(109~112차)을 기준으로 갈라졌다. 본편 main 의 미커밋분(113~118차)은 안 들어 있다 — 스토리 쪽이라 이 앱엔 필요 없다.

사용자 지시(원문): 「jette 레포로 브랜치 분리해서 메인페이지와 버튼이미지는 첨부 확인하고, kpop 러닝만 start 로 해서 다시 만들어주고, 나머지 스토리모드하고 더보기등은 소스 제외해줘, 첨부된 캐릭터 이미지로 모델링해서 start 눌렀을때 러닝할수 있게 해줘, 러닝관련 에셋은 기존에셋 그대로 사용해줘」

## 앱이 하는 일
Boot → 타이틀(시안 한 장 + START) → START = K-POP 한 곡 달리기(`ArcadeRun.StartKpop`, 챕터는 마지막 클리어 다음이 자동) → 결과 카드(한 곡 더 / 나가기) → 타이틀.
그게 전부다. 스토리·육성·컷씬·엔딩·더보기(컬렉션·레코드·시네마·미니게임·보스전·설정·약관)·기부·IAP·펫샵은 **소스째 없다.**

## 유니티 여는 법 (주의)
- 두 프로젝트를 동시에 열지 말 것: 에디터 브릿지(`CoastRemote`, 127.0.0.1:47001)가 같은 포트라 둘째 것은 못 붙는다.
- MCP `unity_launch` 는 `C:\dev\game` 만 연다. 이 프로젝트는 직접: `"C:\Program Files\Unity\Hub\Editor\6000.5.10f1\Editor\Unity.exe" -projectPath C:\dev\game-jette`
  (본편을 먼저 닫아야 한다 — `unity_cmd menu File/Exit` 로 닫힌다). 열리면 `unity_status`/`unity_cmd`/`unity_shot` 은 그대로 이 프로젝트를 본다.
- 첫 임포트는 Library 가 없어 몇 분 걸린다(1회).
- Cowork 의 연결 폴더는 `C:\dev\game` 뿐이라 이 폴더의 파일은 device_stage/commit 이 안 된다 → PC 파이썬(블렌더 MCP subprocess)으로 옮겨 다룬다. `C:\dev\game` 은 Google Drive 가 동기화 중이라 새 파일이 잠깐 하드링크(nlink 2)가 되어 stage 가 거부된다 — 몇 초 뒤 재시도.

## 이번에 한 것

### 1. 타이틀 — `Scripts/UI/JetteTitleController.cs` (신규)
- 시안(첨부 1008×2240, 9:20)을 **그대로** `Resources/CoastRun/UI_Title_JetteGate.jpg` 로(Meta AI 워터마크만 cv2 inpaint 로 지움). `CoastUiCanvas.FullBleedBackground` 로 화면 전체(20:9 딱, 16:9 위아래만 잘림).
- START 는 시안의 버튼 픽셀을 알파로 오려 낸 `UI_Jette_StartBtn.png`(763×266)를 **같은 자리**(시안 px 122,1905 → 제작 좌표 변환)에 겹친 진짜 Button. 숨쉬기 펄스, 누르면 0.93 눌림 → `ArcadeRun.StartKpop(gm)`. Enter/Space 도 시작.
- `TitleSceneDriver` 가 `MainMenuController` 대신 이걸 붙인다. 스플래시 영상(Title_Bus) 없음.
- 캡쳐 확인: `res 1080 2400` 뒤 `tap 0.5 0.091` 로 START 가 눌린다(ny 는 **아래에서** 잰다).

### 2. 캐릭터 — JETTE 곰 (`Tools/blender/jette_bear_rig.py` → `Resources/CoastRun/Rig/JetteBear.fbx`)
- 첨부 6장(정면·측면·후면·박스 든 모습)을 보고 **블렌더 프로시저럴**로 만든 치비 비닐토이 곰: 큰 머리, 노란 안전모(JETTE 글자·챙·턱끈·버클), 귀, 주둥이·코·볼터치·하이라이트 눈, 하늘색 작업복(흰 플래킷·단추·가슴 주머니·(주)제때 명찰·팔 흰띠·커프스·허리띠·등 흰 줄), 흰 부츠.
- **Mixamo 이름의 휴머노이드 뼈대**(Hips/Spine/Spine1/Spine2/Neck/Head, Left/RightShoulder·Arm·ForeArm·Hand, UpLeg·Leg·Foot·ToeBase), T-포즈, 파츠별 **강체 스키닝**(관절은 캡슐 구로 겹쳐 굽혀도 안 벌어짐). `MixamoImportSettings` 가 Rig/ 아래 FBX 를 Humanoid 로 자동 임포트 → 아바타 자동 매핑 16뼈 성공 → 기존 `RunnerAnimator`(Anim_Run/Jump/Hit/Collect/DoubleJump) 가 그대로 리타겟된다.
- `SkaterRig.ModelPath` = `Rig/JetteBear`, 가방(AttachBackpack) 생략, 키 1.42 m(`CoastPlayerVisual`), 그늘색 옅게. `RunTuning.Configure`/`ArcadeRun.StartKpop` 은 늘 `RunMode.Running`(보드 없음).
- 스크립트를 고치면: `blender -b --python Tools/blender/jette_bear_rig.py -- <fbx> <preview.png>` (미리보기 2장 Workbench). 실행 시간 ~10초.
- UI 렌더(`Tools/blender/jette_bear_renders.py`, Eevee, 투명): `UI_RunOver_Sad`(주저앉은 곰, 실패 카드) · `UI_Face_Girl`/`UI_Face_Butler`/`UI_Face_Ring`(얼굴 초상 — 완주 카드 금테·피버 버튼). 포즈는 `rot_world(bone, axis, deg)` 로 세계축 회전 — **나중 호출이 먼저 적용**된다(총회전 = R_a @ R_b).
- 앱 아이콘(`Art/Brand/AppIcon*.png`) = 곰 얼굴 + 하늘 그라데이션. 스플래시 로고는 스튜디오 것 그대로.

### 3. 소스 제외 (211 + 16 파일 삭제, 리소스 ~600 MB 삭제)
삭제: `Scripts/Story/`, `Scripts/Raising/`, `_Recovery/`, 씬 03·04·05, UI(CinemaSelect·StoryReaderUI·LifeUI·MealPickUI·UI_Memory/FinalDestination/PhoneOverlay·Calendar·Donate·Inventory·PetShop·Shop·UpgradeShop·Collection*·Status·MainMenuController·TitleWorldBackdrop·Records·Policy·StageClearUI·KpopChapterSelect·KpopBarPulse), Meta(StoryContest·ContestRivals·HomeData·Schedule*·RandomEvent·LifeItems·RoomDeco·PetShop·Donation·StoryProgress·StoryGate·Festival·Survival·RaisingFun·PetCommands·ChapterMission·ChapterGrading·Collection·RecordTable·IapBridge·NativeShare), Content(AlbumTable·PhotocardTable), EmuSmokeBoot, 에디터(MiniGameDevMenu·RaisingDevMenu·CutsceneTableMenu·StoryAssetsRebuildMenu·CanvasCaptureRunner·BoardProbe113·Telegram·VideoFetch·TransmissionTowerBuilder·ClipboardDump·PlayerMaterialDump·WebGLBuild·VisualCompare/OfflineCapture/VisualIterate/TestBuild·Pexels/3D 임포터).
축약: `GameManager`(세이브 없음 — `Save` 늘 null, Profile/Persist 만), `SceneFlowController`(Boot/Title/Run/StageClear 만), `GameDirector`(Story/Memory 서비스 제거, FlowState 4개), `GameSession`(스토리 바인딩·대회·프롤로그 핸드오프 제거), `StageManager`(StageClearUI 없음), `RunHudChrome`(일시정지 육성 분기 제거), `ArcadeRun`(RoomDeco·EnterRaising 제거, 곡 제목표 `KpopTitles` 내장), `SaveData`(방·화분·가방 필드 제거), `JellyPickup`(포토카드 = 코인 +50), `BossDirector`, `CoastConfigRegistry`, `DynamicEnvironmentManager`(DayPhase 자체 정의), `CoastScenes`(3씬), `SceneDriverInstaller`, `CoastRemote`(vn 명령 제거), `SceneFlowSetupMenu`(3씬), 빌드 세팅 3씬.
리소스: 컷씬이미지/·Cut_*·BG_*·Raise_*·Sched_*·Card/·Video/·FanArt/·Scene/·Album/·Rival_*·Watch_*·MG_*·Title_Bus·story_data·StreamingAssets/Opening(126 MB)·안 쓰는 UI_/Icon_/Fx_·BGM M1·M3·M6·M9·M10·M13(K-POP 풀 M2·M4·M7·M8·M11·M12 + 메뉴 M14/M5 만 남김)·Config 의 CutsceneTable/StoryConfig. `Resources/CoastRun` 840 → 231 MB.
남긴 것 중 이름이 옛것: `StoryPopupKit`(EventCardKit 이 쓰는 나무 프레임 — 범용 UI 킷), `GirlSkater*`(CoastPlayerVisual 의 보드/폴백 경로 — 곰이 뜨는 한 안 쓰임), `Timeline`(챕터 20 = 계절 4), `AchievementTable`·`LevelSystem`·`PetCompanion`(프로필에 쌓임).

### 4. 빌드 — `Editor/BuildMenu.cs`
- 번들 `com.jette.jetterun`, productName 「빨리가자 제때 런」, APK 이름 `JetteRun*.apk`. `Coast Run/Build/Apply jette identity` 메뉴로 에디터에도 적용(ProjectSettings 도 같이 고쳐 둠 — 재시작 뒤 persistentDataPath 가 본편과 갈라진다. 그 전엔 profile.json(코인)을 본편과 공유한다).

### 확인한 것
컴파일 에러 0 · 타이틀 → START → 02_Run 곰 달리기 → 피격 4회 사망 → 「아쉽지만 다음에!」 곰 카드 → 나가기 → 타이틀, 예외 없음. 곡 완주 경로(KpopFinishCo → EndKpopRun)는 코드상 그대로이나 3분 완주는 안 돌려 봤다.

## 남은 것 / 아이디어
- `git push -u jette jette:main` — 첫 푸시는 사용자가. (커밋도 아직 안 했다: `git -C C:\dev\game-jette status` 로 2,000여 변경 확인 후 커밋.)
- 곰 모델 다듬기: 입 벌린 웃음, 등 흰 줄 위치, 안전모 챙 두께, 달릴 때 머리 숙임(Anim_Run 의 목 회전이 큰 머리에서 과함 → SkaterRig 에서 Head 회전 감쇠 가능).
- 러닝 HUD 하단 「NOW PLAYING · 우히&히시」·결과 카드의 「돈 +16G·젤리」 문구는 본편 것 그대로 — 이 앱 톤에 맞게 손볼 여지.
- 피버 버튼 얼굴(UI_Face_Butler)·완주 카드 초상 모두 같은 곰 정면 렌더 — 표정 변형(윙크·웃음)을 renders.py 에 추가하면 좋다.
- APK 빌드 검증: `Coast Run/Build/Android APK — quick (Mono, ARMv7, dev)` → `Builds/JetteRun_quick.apk` 209 MB 빌드 성공(Mono dev 라 큼 — 릴리스 IL2CPP+ASTC 는 훨씬 작다). 빌드 중엔 에디터 브릿지가 안 받는다(수 분).

## 2차 (같은 날) — 결과 카드 시안 + 안전모 JETTE

사용자 지시: 「첨부처럼 제때 수정해줘, 헬멧에 jette 잘 보이게 해줘」 (파스텔 「아쉽지만 다음에!」 카드 시안)

- `RunHudChrome.ShowRunOver`(실패 카드) 를 시안대로: 라벤더→분홍→살구 그라데이션(코드 생성 1×4 텍스처) + 색종이(별·하트 아이콘·♪·알약), 크림/노랑 리본 제목, 크림 부제 띠, 「오늘 도전 N / 3」 하트 알약(미달성은 회색), **쿠키 받침**(갈색 타원 2겹 + 스프링클) 위 주저앉은 곰, 크림 카드(라벤더 테두리) — 「★ 결과 ★」 알약, 거리/점수 큰 칸(둥근 아이콘 Icon_R_Shoe/Trophy), 작은 칸 4개(Icon_R_Coin/Star/Combo/Heart), 라벤더 안내 상자, 노랑 「♪ 한판 더!」 / 라벤더 「나가기」. 완주 카드(ShowSongComplete)는 손대지 않았다.
- `Icon_Star`·`Icon_Heart` 는 `CoastUiArt.Icon("Star")` 처럼 접두어 결합으로 쓰여서 1차 정리 때 잘못 지웠다 → 되살림. (문자열 결합 로드는 grep 으로 안 잡힌다 — 리소스 지울 때 `Icon("` 패턴도 볼 것.)
- 곰 안전모 「JETTE」: 앞(0.125) + **양옆(0.10) + 뒤(0.115)** 네 곳에 새겼다 — 러닝은 뒤에서 보므로 달릴 때도 글자가 보인다. 실패 카드 렌더는 정면 조금 위 카메라 + 고개 9° 숙임으로 앞글자와 얼굴이 같이 보이게.
- 폰트에 없는 글리프 주의: 💜·✦·▶ 는 안 그려져서 ♥·★·♪ 로 썼다(🌱 도 안 나옴 — 117차 메모와 같다).

## 3차 (같은 날) — 고개 감쇠 + 릴리스 APK

- `SkaterRig.ApplyJuiceLate`: Mixamo 러닝 클립의 고개 회전을 **45% 만** 남긴다(`_headRestLocalRot` 과 Slerp). 큰 안전모 머리가 앞으로 푹 숙여지던 것이 서고, 달릴 때 뒤통수의 JETTE 가 읽힌다.
- 릴리스 빌드 `Coast Run/Build/Android APK (IL2CPP, ARM64+ARMv7)` → `Builds/JetteRun.apk` (아래 결과 참조). 빌드 중엔 브릿지가 멈춘다(10~20분).
- 첫 푸시(`git push -u jette jette:main`)는 세션 정책상 에이전트가 못 한다 — 사용자가 직접.
- 1차 릴리스 빌드(`JetteRun_v1_withGirlAssets.apk`, 207 MB, 22.5분)에 옛 소녀 모델(Rig/Skater.fbx + Ch46 텍스처)·GirlSkater 스프라이트/프리팹이 그대로 들어 있어 삭제(약 22 MB 비압축). CoastPlayerVisual 의 GirlSkater 폴백 코드는 남았지만 리소스가 없어 곰 경로만 탄다. 그 뒤 `JetteRun.apk` 재빌드.
- 남은 용량은 러닝 월드 텍스처(하늘 16 MB×4, 원경, 파사드…) — 사용자 지시대로 그대로 둠. 줄이려면 Sky_/Far_ 계절 변형 정리가 첫 후보.
