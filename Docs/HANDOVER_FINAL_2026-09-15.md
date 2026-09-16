# 최종 인수인계 — 「너와 나의 주파수」(Coast Run) · 2026-09-15

> 상세 이력(1~87차)은 `Docs/HANDOVER_2026-09-07.md`(회차별 「세션 마무리」 절). 이 문서는 **지금 상태·규칙·절차·남은 일**만 한 장으로 정리한 것.

---

## 1. 프로젝트 한눈에
- Unity 6000.5.10f1 · URP 17.5 · uGUI · Purchasing · glTFast. **모바일 세로(9:16) 게임**, 패키지 `com.jette.coastrun`. 코드 `Assets/_CoastRun/`(C# 약 150개), 리소스 `Assets/Resources/CoastRun/`.
- 저장소 `C:\dev\game` = GitHub `kk00701903-hub/game-costrunner`(main). **커밋·푸시는 사용자 담당** — Claude 는 파일만 쓰고 커밋 대상 목록을 남긴다.
- 모드: **스토리 모드**(육성 턴제: 3행동 = 1주 → 결산 → 챕터 경계에서 컷씬/이야기/대회 러닝) + **K-POP 러닝**(곡 단위 아케이드, 미션 3개·보스) + 더보기(시네마·컬렉션·레코드·기부·약관).
- 이야기: 20챕터, 컷씬 v4(`CinematicTable` 192컷 `Cut_T_*`), 이벤트 EV1~10, 단서 6개(`ClueSystem`) → 엔딩 A/B/진엔딩. 컷씬 챕터 1·3·5·7·10·12·17·20, 이벤트 2·4·6·8·9·11·13·14·15·19, 러닝(대회) 1·4·7·10·13·15·18·20.

## 2. 절대 규칙(사용자 지시)
1. **모든 에셋은 세로 9:16** — 스틸 1080×1920 원본 → 720×1280 처리본. 가로 에셋 금지.
2. **UI 시안 그림에 글자를 굽지 않는다** — 배경 PNG 는 프레임·아이콘·틀만, 모든 문자열은 코드에서 `Loc.T(ko, en)` 으로 시안 좌표(`Place`, 720×1280)에 그린다(86차-2). 예: `DonateUI`·`StatusUI`.
3. **게임 폴더 작업은 PC 링크 세션에서**, 텔레그램 보고는 매 작업 시작/50%/100%(`Tools/Telegram/outbox.txt` → Unity 메뉴 `Coast Run/Telegram/Send outbox`).
4. 비밀값(`.env`, 봇 토큰, API 키)은 문서·보고에 절대 쓰지 않는다. `.env` 는 gitignore, 원격 도구로는 쓰기 불가.
5. 컷씬 그림 화풍 = **clean soft anime / Korean webtoon key-visual**(v3 `Cut_S_*` 기준, 수채 아님). 인물은 앵커(`Tools/KlingGen/ref87`) + 바이블 문장으로 고정.

## 3. 원격(Cowork) 세션 작업 절차
- `device_bash` 는 9/8 이후 `C:\dev\game` 마운트 실패 → **파일은 `device_stage_files`(읽기) / `device_commit_files`(쓰기, 50개씩)**, PC 실행은 **RunBat**: bat 를 매번 **새 staged 파일명**으로 `Tools/_clip/run.bat` 에 커밋 → `unity_cmd "menu Coast Run/Dev/Run - Tools/_clip/run.bat"`. 같은 staged 파일명을 다시 커밋하면 옛 내용이 써질 수 있다(outbox·py 모두).
- Unity MCP: `unity_compile`(stop 뒤), `unity_cmd play|stop|scene <name>|menu <path>|tap nx ny(ny 는 아래에서)|key|log`, `unity_shot <name>` → `Tools/_shots/<name>.png`. 플레이 중 컴파일하면 `GameManager.I` 가 비어 Dev 메뉴가 안 먹음 → stop → play. PNG 는 만든 직후 stage 가 「hardlinked」로 거부 → PC 파이썬으로 JPG 변환 후 20~30초 뒤 stage(`Tools/KlingGen/out/shots8x/`).
- 캡쳐 기준: 1080×1920, UI 설계 단위 720×1280(CanvasScaler 1080×1920 · `CoastUiCanvas.HudPad=28` · 텍스트 ×1.4).
- 텔레그램: `Tools/Telegram/outbox_<n>.txt` 작성 → `outbox.txt` 로 커밋 → `menu Coast Run/Telegram/Send outbox` → `unity_log` 에 `[Telegram] outbox exit=0`.
- Dev 메뉴(`Editor/MiniGameDevMenu.cs`): UI - Donate/Status, Cine EV*/END_*, Clue Card, Kpop Start/Log pet, Contest HUD test, Life/Contest 테스트, UI - Overflow audit.

## 4. 외부 도구·MCP 연결 상태
| 도구 | 상태·요령 |
|---|---|
| **Unity** | `.cursor/mcp.json` + Claude 앱 구성의 `unity` 서버(`Tools/Mcp`). 브릿지 `Editor/CoastRemote.cs`. |
| **Kling 웹**(컷·앵커 생성) | Claude 앱 내장 브라우저 탭 `seed`, kling.ai 로그인은 사용자. IMAGE 3.0 Omni · **1K SD · 9:16 · 2장**(새로고침마다 재설정). 헬퍼는 `localStorage.__helpers84`(`eval` 로 복구) + 87차 수정(`__clearRefs` 는 `a.close` 에 pointerdown→…→click 순서 dispatch). 참조는 GitHub raw(`Tools/KlingGen/ref77|ref84|ref87`, `Assets/Resources/CoastRun/Cut_S_*.jpg`) fetch → `input.el-upload__input`. 결과 수집 = `__collect()`(sig 매칭, 「Back to Top」 후). 다운로드는 PC 파이썬(`dl84.py`/`dl87.py`) — CDN 이 샌드박스에서 막힘. 크레딧 잔액 **448**. |
| **Kling API** | `Tools/KlingGen/*.py`, PC 터미널에서만(샌드박스·VM 403). |
| **Figma MCP** | Claude 앱 커넥터로 연결됨(계정 songmj, `(주)제때` org Full 시트 · `songmj의 팀` starter View). `whoami` 로 확인. 스킬 인덱스 `skill://index.json`(figma-use 등 14개) — `get_design_context`/`use_figma` 전에 해당 SKILL.md 필수. |
| **Photoshop MCP** | 연결 OK(`photoshop_ping`). 시안 정리·키잉·텍스트 지우기 가능(원격에선 PIL 로 대체해 옴). |
| **Blender MCP** | 애드온 서버(9876) 자동 시작되도록 userpref 저장됨(82차-2). `get_scene_info` 로 확인. |
| **android MCP** | `Tools/Mcp/android_mcp_server.py`(adb/emulator 13도구). Claude 앱 구성에 `android` 항목 붙여넣기(`Tools/Mcp/claude_desktop_mcp_android.json`) **아직 사용자 미완**. 에뮬로는 Unity ARM APK 못 봄 → 실기기 adb. |
| **Telegram** | 봇 API 는 샌드박스 403 → outbox 경로. |

## 5. 파일 지도(자주 만지는 것)
- 이야기: `Docs/CUTSCENE_SCRIPTS_v4.md`(대본 원본) → `python Tools/Story/gen_cinematic_v4.py [--txt]` → `Story/CinematicTable.cs` + `Tools/Story/script/*.txt` → `python Tools/Story/cutscene_txt.py import` → `Story/ChapterScript.Data.cs/En.cs`. 진행 규칙 `Meta/StoryProgress.cs`, 단서 `Story/ClueSystem.cs`, 엔딩 `GameManager.ResolveEnding`, 시네마 목록 `UI/CinemaSelect.cs`, 플레이어 `Story/CinematicPlayer.cs`.
- 스틸: `Assets/Resources/CoastRun/Cut_T_*.jpg`(192, v4) · `Cut_S_*.jpg`(131, v3) · 앵커 `Tools/KlingGen/ref87/`(현행 7개, ref77/ref84 에도 복사됨) · 계획/다운로드 `Tools/KlingGen/plan84.json·plan87.json·dl84.py·dl87.py`.
- 러닝: `Core/GameSession.cs`, `Meta/ArcadeRun.cs`(K-POP 정산, 미션 3개 → 돈·젤리 ×2), `Meta/BossDirector.cs`(BossBody 3D 움직임), `Meta/ContestRivals.cs`(NPC 러너 회피·꽈당), `Meta/StoryContest.cs`(대회·배너), `Visual/JuiceDirector.cs`, `Player/PetCompanion.cs`.
- 육성: `Raising/TamaRaisingUI.cs`(턴·경계 루틴), `Meta/Survival.cs`, `Raising/MissionMiniGames*.cs`, `UI/StatusUI.cs`, `UI/WeekPassUI.cs`, `Raising/HomeUI.cs`.
- UI 배경 그림(글자 없음): `Resources/CoastRun/UI_Donate_BG.png`, `UI_Status_BG.png`, `UI_Status_Flag.png`, `UI_Contest_Banner.png`, `UI_Contest_Flag.png`.
- 오디오: `Audio/CoastAudioManager.cs`(절차 SFX: `CoastSfx.Shutter` 등), `CoastBgmLibrary.cs`(M1~M12 만).
- 문서: `Docs/HANDOVER_2026-09-07.md`(전체 이력), `Docs/DESIGN_RAISING_LIFE_v1.md`, `Docs/STORY_MODE_v6.md`, `Tools/Mcp/README.md`.

## 6. 사용자가 할 것(미커밋·미완)
1. **커밋**(84~87차분): `Assets/Resources/CoastRun/Cut_T_*.jpg`(192+meta) · 85차 코드(Story/Meta/Raising/UI/Editor + `Tools/Story/*`) · 86차 코드(`Meta/{ArcadeRun,BossDirector,ContestRivals,PetShop,StoryContest}.cs`, `UI/{RunHudChrome,CollectionUI,DonateUI,StatusUI}.cs`, `Audio/CoastAudioManager.cs`, `Player/PetCompanion.cs`, `Visual/JuiceDirector.cs`, `Editor/MiniGameDevMenu.cs`, `Resources/CoastRun/UI_*.png`) · 87차(`Tools/KlingGen/ref87/*`, `ref77/{H12,H19,D12,D20,KID,DAD}.png`, `ref84/MOM.png`, `plan87.json`, `dl87.py`, `out/web87/urls.json`) · `Docs/HANDOVER*`. `Tools/KlingGen/out/web8x/*.png` 원본은 제외 권장. Unity 를 한 번 열어 .meta 생성 후 커밋.
2. Claude 앱 **설정 → 개발자 → 구성 편집** 에 `android` 항목 붙여넣고 앱 완전 재시작.
3. 실기기 APK 검증(오프닝 1:37, 컷씬 v4, 대회 러닝 NPC·보스, 기부·캐릭터 정보 화면 영어 모드).

## 7. 알려진 이슈·다음 후보
- 보스 3D 움직임(`BossBody`)은 코드·컴파일만 확인, K-POP 6챕터 이상 플레이 캡쳐 미확인 — 과하면 `FollowBoss` 의 roll/pitch 계수만 줄이면 됨.
- 옛 시안 배경 중 글자가 구워진 것(더보기 포토카드·뮤직 컬렉션 버튼, 타이틀 목업)은 규칙 2 대로 글자를 지우고 코드로 옮기는 일이 남음.
- NPC 러너 회피는 장애물 콜라이더 기준(전 레인 장애물은 못 피함 → 꽈당).
- 컷씬 리더 txt 는 EN 줄 없음(영어 모드에도 한국어). 단서 획득은 카드 두 버튼으로 단순화(실제 행동으로 바꾸려면 `ClueSystem.ShowAfterScene`).
- APK 230~240 MB(Resources 342 MB) → AAB/PAD 검토. 카드 사진 실물 교체, README/COMPLETION 문서 현행화.
- 새 컷씬 그림을 뽑을 때는 **ref87 앵커**(아빠 40대 초반)로 — 아빠가 나오는 컷(OP_02·03·04, N2_07 등)은 옛 앵커로 뽑힌 것이라 재생성 후보.

## 8. 새 세션 시작 체크리스트
1. `unity_status`/`unity_cmd log` → 에디터 연결 확인. 2. 텔레그램 「작업 시작」 보고. 3. `Docs/HANDOVER_2026-09-07.md` 의 최근 회차 절만 읽기. 4. Kling 이 필요하면 내장 브라우저 `seed` 탭에서 로그인 상태·크레딧 확인 후 `eval(localStorage.__helpers84)` + 87차 패치. 5. 끝나면 HANDOVER 에 회차 절 추가 + 커밋 대상 + 텔레그램 100%.

---

## 9. 88~95차 이후 달라진 것 (2026-09-16 갱신) — 상세는 `Docs/HANDOVER_2026-09-15_88-90.md`
- **컷씬은 v6.4**(`Docs/CUTSCENE_SCRIPTS_v6_읽기용.md`, 188컷 + MV = `CinematicTable` 194컷). 오프닝 4컷(손자국·소매·물속·달력「내년 내 생일, 송전탑 아래서 만나자」), CS1 은 기억을 잃은 하늘, CS8 은 엄마 얼굴이 마지막 기억·두 번 안으려다 통과, 「빌린 1년」은 신의 선물이라는 뜻을 직접 말하지 않고 꼬마 대사·유채꽃 컷으로만 암시. 그림 ID 는 폴더 번호와 무관한 리소스 이름(`Cut_T_V6_*` 4장 신규). 생성기는 `Tools/Story/gen_cinematic_v6.py --txt` → `cutscene_txt.py import`. §1 의 「컷씬 v4 192컷」은 이 절로 대체.
- **엄마 앵커**: 40세(하늘 12살 때, 해녀복 `ref87/ff_MOM40.png`) / 48세(현재 평상복, Kling 생성 `ANCHOR_MOM48B` — 웃지 않는 얼굴, URL 은 `localStorage.__mom48url`). 아빠는 ref87 40대 초반.
- **육성(스토리모드) 주인공 그림**은 수채 치비 세트로 통일(검은 머리·노란 핀): `Raise_Girl_S{1..4}_{Normal|Happy|Tired}`(주차 1~13/14~26/27~39/40~ 성장 단계) + `Raise_Girl_Pose_*` 20종 + `Sched_job_*` 카드. 옛 52차 갈색 머리 포즈 이름은 `TamaRaisingUI.RefreshGirl` 에서 새 세트로 치환. 쓰러짐 화면은 `Raise_Girl_Pose_SleepLying`.
- **스토리모드 8페이지 UI 시안 정합 완료**(94차: HomeUI·ShopUI·GameOver·WeekPass·알바 선택). 남은 에셋: 상점 재료 아이콘 25종(`UI_Goods_<id>`), 알바 카드 소품, `Sched_job_{hall,salon,dangsan,night_delivery}`.
- **도구 변화**: Kling 헬퍼는 `eval(localStorage.__helpers84); eval(localStorage.__helpers95)` 두 줄로 복원(§8-4 대체). 생성 설정(1K SD·9:16·2장)은 리로드마다 초기화. Firefly(포토샵 `syntheticFill`)는 「현재 사용할 수 없습니다」로 막힘 → 아이콘·소품도 Kling 마젠타 배경 + `Tools/KlingGen/dl95r.py` 크로마키. RunBat 가 안 돌면 `Tools/_clip/run.log` 를 잡은 옛 `kg_server.py` 파이썬 창을 닫을 것(95차에 `RunBatMenu` 가 run2.log 로 우회하도록 고침). Unity 브릿지(47001)가 재시작 뒤 안 붙는 경우가 있었음 — `CoastRemote` 리슨 로그 확인.
- **§8 체크리스트 3번**은 「`Docs/HANDOVER_2026-09-15_88-90.md` 의 '한눈에' 절과 마지막 회차 절 읽기」로 바꿔 읽을 것. 95차 커밋 대상은 그 문서 95차 절 끝.
