# 컷씬 대본 v4.8 (83차, 2026-09-14 · 감정선 점검 + 백화점·사투리·CS7 이전 유령 티 제거) — 「너와 나의 주파수」 서사 재구성판

v3(77·80차)에 사용자 제공 「서사 중심 컷씬 시나리오 재구성안」을 입혀 전편을 다시 썼다. **코드는 아직 안 건드렸다** — 이 문서는 대본·그림 지시서이고, 적용(`Tools/Story/script/*.txt` → `cutscene_txt.py import`, 시네마 카드, 새 스틸 생성)은 다음 회차.

## v3 → v4 무엇이 달라졌나

| | v3 | v4 |
|---|---|---|
| 구성 | OPEN 11컷 + 컷씬 8편(12컷) + 엔딩 3편(8컷) = 131컷 | OPEN 14컷 + 컷씬 8편(13컷, CS1·CS6은 14·CS8은 16컷) + **보조 컷씬 10편(4컷, 스토리 모드 이벤트)** + 엔딩 3편(9·9·11컷) = **192컷** |
| 아빠의 죽음 | 「한 마리만 더」 태풍 출항 | **백화점 보석 머리띠를 사려고 빌린 돈** 때문에 나간 것(하늘의 죄책감이 서사의 뼈대) + 엄마가 우비를 빼앗음 + 방수 통(머리띠)이 장롱에 숨겨짐 |
| 꼬마 | 환생한 아빠(엔딩 B에서 확정) | **「민재」라는 살아 있는 여섯 살 아이의 몸에 남은 아빠의 마지막 기억.** 딸이 죽던 순간 하트 부표를 보고 깨어났고, 해경에 구조를 청한 뒤 약속 장소까지 곁을 지킨다. 컷씬 8에서 이름이 불리는 순간 기억이 빠져나간다(=두 번째 이별). |
| 하늘의 상태 | 유령(확증은 컷씬 8) | **실종자.** 「닿는 물건의 규칙」(오래 써서 몸이 기억하는 것·감정이 깊게 남은 것만 잡힌다)으로 유령 확증을 끝까지 미룬다. 컷씬 7에서 「고하늘 · 실종 1년」 카드를 보고도 하늘은 죽음을 받아들이지 않는다. 확정은 컷씬 8(시신 발견 → 마지막 기억). |
| 도윤 | 병원 팔찌·약 → 「저 사람이 죽어 가나」 | 유지 + **새벽 물때마다 갯바위를 도는 수색·병원 경고 메시지·아버지가 감춘 편지 열아홉 통.** 하늘이 「죽어 가는 쪽이 도윤이어야 한다」고 *선택*하는 것까지 밀어붙인다(컷씬 6·7). |
| 편지 | 우편함 스무 통 | **열아홉 통 + 스무 번째는 도윤이 제주로 돌아와 직접 꽂은 것.** 스무 통이 모이면 수신인 이름이 잠시 선명해진다. |
| 팔찌 | 병원 팔찌에 「하늘」 | **수색용 방수 카드**(사진 + 고하늘 + 실종 1년). 도윤의 물건인데 하늘의 이름이 새겨져 있어 경계를 통과한다. |
| 세 가지 규칙 | 없음 | **죽은 사람이 산 사람에게 닿으려면 — 불러 줄 진짜 이름 · 둘이 함께 기억하는 물건 · 끝까지 지킨 약속.** 세 개가 맞으면 91.9의 노래 한 곡 동안만. 엔딩 분기의 근거. |
| 엔딩 | A 만난다 / B 못 만난다 / TRUE 라디오 | **A 「엇갈린 정류장」(못 만남) / B 「우유 두 병」(목소리만) / TRUE 「맞닿은 주파수 91.9」(한 곡 동안 만남 + 이듬해 라디오국에서 도윤이 돌아봄 + 넷째 돌 「내일」 + **마지막 컷은 손잡은 둘의 사진** — 도윤도 죽어서 만난 건지, 하늘이 살아난 건지 일부러 열어 둔다).** ⚠ A·B의 뜻이 v3와 뒤집혔다 — `CinemaSelect` 카드 제목·해금 규칙 갱신 필요. |

## 챕터 배치 (스토리 모드 20챕터)

| 챕터 | 씬 | 길이 | 계절·잔여일 |
|---|---|---|---|
| 새 게임 | **OPEN** 「너와 나의 주파수」 | 14컷 × 10.6s ≈ 2:28 | — |
| CH1 | **CS1** 「이름」 | 14컷 × 8.5s ≈ 1:59 | 봄 · 365 |
| CH2 | EV1 「손가락 사이로」 | 4컷 × 7s | 봄 |
| CH3 | **CS2** 「하트」 | 13컷 | 봄 |
| CH4 | EV2 「열일곱 번」 | 4컷 | 늦봄 |
| CH5 | **CS3** 「우리 기지」 | 13컷 | 초여름 |
| CH6 | EV3 「우유 한 팩」 | 4컷 | 여름 |
| CH7 | **CS4** 「열두 개의 초」 | 13컷 | 여름 · 196 |
| CH8 | EV4 「달력」 | 4컷 | 늦여름 |
| CH9 | EV5 「찢어진 소매」 | 4컷 | 가을 |
| CH10 | **CS5** 「그 밤」 | 13컷 | 가을 · 104 |
| CH11 | EV6 「물때」 | 4컷 | 가을 |
| CH12 | **CS6** 「스무 살」 | 14컷 | 늦가을 |
| CH13 | EV7 「첫눈」 | 4컷 | 겨울 · 32 |
| CH14 | EV8 「국화」 | 4컷 | 겨울 |
| CH15 | EV9 「스무 번째」 | 4컷 | 겨울 |
| CH17 | **CS7** 「둘 중 하나」 | 13컷 | 겨울 · 7 |
| CH19 | EV10 「전날 밤」 | 4컷 | 생일 전날 |
| CH20 | **CS8** 「주파수」 | 16컷 | 생일 |
| 엔딩 | END_A / END_B 9컷 · END_TRUE 11컷 × 8.5s | 다음 날 아침 |

CH16·CH18은 컷씬 없음(육성·러닝만). 보조 컷씬은 `ChapterVN.PlayChapterOpening` 자리에서 4컷짜리 짧은 이벤트로 튼다(채도는 앞 컷씬 값을 이어받음).

## 분기 조건 (엔딩)

플레이 중 모이는 「단서」 여섯 개 — `이름`(CS7 카드) · `편지 스무 통`(EV9) · `하트 흔적`(CS2 부표 페인트) · `보석 머리띠`(CS8 방수 통) · `돌 세 개`(CS4, 무너진 걸 한 번이라도 다시 쌓음) · `91.9 라디오`(CS3 복원). 컷씬은 항상 재생되므로 단서는 **컷씬 직후 육성 화면에 「단서 획득」 팝업으로 확정**하고, 미니게임·게이트 실패 등으로 챕터를 놓치면(예: CH15 이전에 편지를 다 못 읽음) 단서가 빠진다.

- **END_A** 「엇갈린 정류장」: `이름` 없음 **또는** `91.9 라디오` 없음.
- **END_B** 「우유 두 병」: `이름`+`라디오`는 있으나 `보석 머리띠` 또는 `편지 스무 통`·`하트 흔적`·`돌 세 개` 중 하나라도 없음.
- **END_TRUE** 「맞닿은 주파수 91.9」: 여섯 개 전부.

## 캐릭터 바이블 — v3 표 그대로 + 추가

v3의 H19 / H12 / D12 / D20 / KID / DAD / MOM / GUARD / BOAT 고정. 추가·수정:

| 키 | 인물 | 비고 |
|---|---|---|
| H11 | 하늘(열한 살, 오프닝 앞부분) | H12와 같은 옷, 머리는 조금 더 짧게. 앵커는 H12 재사용 |
| KID | 꼬마(민재) | 주황 우비 고정. **왼쪽 눈이 웃을 때 접힌다**(아빠와 같음 — DAD 앵커 문장에도 넣을 것). 손톱 밑 붉은 페인트(CS2) |
| DAD | 아빠 | 왼쪽 눈 접힘 추가. 백화점 컷에선 우비 벗고 깨끗하지만 어울리지 않는 낡은 셔츠 |
| MOM | 엄마 | **마흔둘(40대 초반)**. **갸름하고 단정한 얼굴, 날씬하고 단단한 체격, 키 160.** 볕에 그은 건강한 피부, 검은 머리를 낮게 묶음, **흰머리·깊은 주름 없음**, 지친 듯 온화한 눈. 해녀 컷: 검은 네오프렌 해녀복 + 검은 두건 + 이마의 물안경 + 주황 태왁. 평상복 컷: 옅은 색 블라우스에 짙은 바지, 앞치마. CS8: 짙은 남색 평상복에 흰 국화. **⚠ 엄마가 나오는 컷은 전부 신규 생성** — 옛 그림은 뚱뚱하고 늙게 나와 폐기. 앵커 `ref77/MOM.png`도 이 문장으로 먼저 다시 만든다 |
| MIN_MOM | 민재 엄마 | 30대 · 평범한 마을 사람 · CS8 마지막에 한 컷만 |
| GRANNY | 마을 할머니 | 행인. CS1·EV2에 한 컷씩 |
| HEADBAND | 보석 머리띠 | **푸른 보석 세 알**이 박힌 얇은 은빛 머리띠. 시내 백화점 유리 진열장 → 방수 통 → CS8 하늘 머리 |
| CASE | 방수 통 | 손바닥만 한 노란 플라스틱 방수 통, 뚜껑에 붉은 하트 |
| CARD | 수색 카드 | 손바닥만 한 코팅 카드, 열아홉 살 사진 + 이름 + 「실종 1년」. 도윤이 코트 안주머니에 넣고 다니며 초소 앞에서 꺼내 봄 |

화풍·부정 프롬프트는 v3와 같다. 어린이 컷은 `adult, teenager, grown man`을 부정에 추가.

## 인물 고정 규칙 (그림 생성 전 반드시)

**앵커 사용법(Kling 웹)**: 인물이 나오는 모든 컷은 `Tools/KlingGen/ref77/<키>.png`를 **subject(얼굴) 참조, 강도 0.35~0.5**로 건다. 77차에 참조가 구도를 깼던 건 강도 1.0 + 전신 참조였기 때문 — 얼굴만, 약하게. 구도·옷·배경은 텍스트로만. 두 인물이 같이 나오면 앵커 두 장을 함께 걸고 프롬프트 첫 문장에 두 사람의 나이·성별·키 차이를 적는다. 한 컷에 세 사람 이상이면 앵커는 주인공 둘만.

**나이·성별 태그를 프롬프트 맨 앞에**: `a 19-year-old young WOMAN` / `a 12-year-old GIRL` / `a 20-year-old young MAN, tall` / `a 12-year-old BOY` / `a 6-year-old small CHILD (boy)` / `a woman in her early forties` / `a man in his forties`. 부정 프롬프트에 반대 성별·반대 나이대를 넣는다(어린이 컷: `adult, teenager, grown man, grown woman` / 어른 컷: `child, kid, elderly, old`).

**키 기준(같은 컷 안에서 절대 어긋나지 않게)**:

| 키 | 인물 | 키(cm) | 같은 컷의 상대 기준 |
|---|---|---|---|
| H11 / H12 | 하늘 11·12세 | 140 | D12보다 반 뼘 작다 |
| D12 | 도윤 12세 | 145 | H12보다 반 뼘 크다, 마른 체형 |
| KID | 꼬마 6세 | 110 | H19의 허리~배꼽 높이 |
| H19 | 하늘 19세 | 162 | D20 어깨 아래, 턱이 D20 어깨선 |
| D20 | 도윤 20세 | 180 | H19보다 머리 하나 크다, 마르고 길다 |
| DAD | 아빠 40대 | 172 | MOM보다 반 뼘 크다, 어깨 넓다 |
| MOM | 엄마 42세 | 160 | H19와 거의 같다, 날씬 |
| GRANNY | 마을 할머니 | 150 | 허리 굽음 |
| MIN_MOM | 민재 엄마 30대 | 158 | 한 컷 |

**얼굴 고정 문장(프롬프트 앞에 그대로 복사)**:
- H19: `19-year-old Korean young woman HANEUL, oval face, long straight black hair below the shoulders with a small yellow hairpin, dark clear eyes, no makeup, sky-blue hoodie`
- H12: `12-year-old Korean girl HANEUL, round face, chin-length black bob with a small yellow hairpin, faded pale-blue t-shirt` (OPEN 1~8은 `11-year-old`)
- D12: `12-year-old Korean boy DOYUN, very pale, neat short black hair, thin, white shirt and navy knit vest`
- D20: `20-year-old Korean young man DOYUN, tall and thin, pale, soft black hair, grey hooded coat over navy hood, white hospital bracelet on left wrist`
- KID: `6-year-old small Korean boy in an oversized orange rain coat with the hood up, round cheeks and big eyes visible under the hood, yellow rain boots` (얼굴이 드러나는 컷 CS8-11·EV7-4만 `hood pushed back, left eye creasing when smiling`)
- DAD: `Korean fisherman in his forties, sun-weathered, short black hair with a little grey, stubble, kind face, left eye creasing when he smiles`
- MOM: `slim Korean woman in her early forties (42), neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, gentle tired eyes, average-slim build — NOT overweight, NOT elderly`

**금지**: 같은 인물의 헤어 길이·색 변화, 하늘의 주황색 옷, 도윤(20)의 병원 팔찌 누락, 꼬마 후드 벗김(예외 두 컷), 엄마의 흰머리·주름·비만, 아이 컷에 어른 비율(머리 크기 1/5 이상 유지).

## 닿는 물건의 규칙 (연출 지침)

하늘의 손에 잡히는 것: 스케이트보드 · 고장 난 라디오 · 편지 · 돌 세 개 · 리어카 손잡이 · 도시락 · 우비 · 부표 · 그리고 CS7의 수색 카드(도윤의 물건이지만 하늘의 이름이 새겨져 있어서). 잡히지 않는 것: 문고리 · 가게 물건 · 사람 · 국화. **CS7 전에는 「못 잡는다」를 절대 유령처럼 그리지 않는다** — 자막은 「손에 힘이 안 들어갔다 / 손끝이 저렸다 / 미끄러졌다」, 그림은 힘이 빠진 손·굴러가는 병·떨리는 바늘로, 투명·흐릿한 손은 금지. 독자는 CS6~7의 「둘 중 누가 아픈가」에서 오히려 **하늘이 병약한 쪽일 수도** 있다고 느껴야 한다. 손이 「통과」하는 그림은 CS7-8(카드를 돌려주는 손)부터, 확정은 CS8-6·7. **단 하나의 예외 = CS6-13(오도 컷)**: 하늘이 도윤의 소매를 잡는데 쥐어지지 않는다 — 이때 흐리게 그리는 쪽은 **도윤**이다(하늘의 손은 또렷). 자막도 「사라져 가는 쪽은 그 애였다」로 독자가 도윤이 죽어 간다고 믿게 만든 뒤, CS7-5의 카드에서 뒤집는다.

## 떡밥·복선 체크리스트

- OPEN 1·2·4·7 → CS8 10~12·15: 백화점 머리띠 · 「한 마리만 더 잡으민 뒈어」 · 우비 소매 · 방수 통.
- OPEN 13 / CS1 9·12 / CS4 12·13 / CS8 6: 돌 세 개(아래 하늘·가운데 도윤·위 아빠). 엔딩 TRUE에서 넷째 돌 「내일」 + 빈 우유 두 병 + 사진 한 장(END_TRUE 9~11: 도윤이 부스에서 일어선 뒤 「방송은 이어지지 않았다」— 도윤의 죽음으로도, 하늘의 귀환으로도 읽히게. 사진은 두 사람이 똑같이 또렷해야 하고, 찍은 사람·날짜를 절대 밝히지 않는다).
- CS1 5 / EV2 3 / CS8 14: 할머니가 꼬마를 「민재야」라고 부름 → 민재 엄마의 「민재야」로 기억이 빠져나감.
- CS1 12 / EV3 4 / CS6 13·14 / CS7 3: 새벽의 약 → 「저 사람이 죽어 간다」는 오해.
- CS2 1 / CS7 4·5 / EV9: 편지 열아홉 통 → 스무 번째 → 이름이 선명해짐.
- CS2 9~11 / CS4 1 / CS8 8: 하트 부표의 새 페인트, 꼬마 손톱의 붉은 물.
- CS6 8·9 / OPEN 12: 우비가 세 번 떨어짐(아빠의 경고).
- CS6 10 / CS8 7: 잠수의 조각(폐그물·주황 형체·사이렌) → 마지막 기억(칼이 없던 허리·푸른 조개껍데기).
- EV5 / CS5: 트럭 — 「트럭이 바로 옆을 스쳤고 나는 길 옆으로 넘어졌다」(80차 규칙 유지, 통과 아님).
- 잡음(회수 안 됨): OPEN 5 우유 한 팩의 상표, CS3 11 라디오 옆 유리구슬 개수, EV4 도윤의 버스 시간표 메모.

## 목소리 카드 (자막 문체)

- **하늘(서술, 1인칭)**: 짧고 건조하게 가다가 가슴이 달리는 자리에서만 한 문장이 길어진다. 감정을 가끔 그냥 말한다(「무서웠다」「미안했다」). 격언으로 닫지 않는 컷이 편당 절반 이상.
- **꼬마**: 재치가 없다. 아이 말을 두 번 한다. 가끔 아이가 할 리 없는 말이 튀어나오면 후드를 눌러쓴다. 사투리 한 마디(「~뒈어」)는 아빠의 것.
- **도윤(12)**: 짧은 확정문. 「반은 진짜고 반은 거짓말이야」. **도윤(20)**: 혼잣말. 질문으로 끝난다.
- **엄마·아빠·할머니**: 제주 말. 짧다. 아빠는 오프닝과 CS8에서 같은 문장 한 줄.

---

## OPEN 「너와 나의 주파수」 · BGM_M5 · 14컷 × 10.6s · 채도 1.0

1. 열한 살 생일을 하루 앞둔 봄. 처음 가 본 시내 백화점, 유리 진열장 안에 푸른 보석 머리띠가 있었다. 나는 오래 봤다. 갖고 싶다는 말은 안 했다.  
   `Cut_T_OP_01` (앵커 H12 · **신규**)  
   _a bright city department store accessory counter, an 11-year-old girl HANEUL in a faded pale-blue t-shirt and worn sandals standing very still before a glass display case, staring at a thin silver headband set with three small blue gems on a velvet stand, her hands behind her back, polished floor and warm shop lights, shoppers blurred_
2. 해녀 엄마와 작은 배 한 척이 전부인 아빠. 그걸 사 줄 형편이 아니라는 건 열한 살도 알았다. 아빠는 내 눈을 놓치지 않았다.  
   `Cut_T_OP_02` (앵커 DAD · **신규**)  
   _weathered Jeju fisherman DAD in an old clean shirt that does not fit the department store, standing a few steps behind his 11-year-old daughter HANEUL at the glass counter, watching her look at the headband, his hand touching his thin wallet in his pocket, gentle expression, the left eye creasing as he smiles_
3. 아빠는 이웃에게 돈을 빌려 머리띠를 샀다. 빌린 돈은 갚아야 했다. 태풍이 온다는 밤, 아빠는 한 번 더 바다로 나가기로 했다.  
   `Cut_T_OP_03` (앵커 DAD · **신규**)  
   _dusk at a small Jeju harbor before a storm, DAD in an orange rain coat checking the nets on his small white-and-blue fishing boat, dark clouds piling over the sea, a small yellow waterproof case on the deck beside him, wind_
4. 엄마가 우비를 빼앗았다. 나는 대문 앞에서 아빠의 젖은 소매를 잡았다. 머리띠 필요 없어. 생일도 안 해도 돼. 「한 마리만 더 잡으민 뒈어.」  
   `Cut_T_OP_04` (앵커 DAD · **신규**)  
   _rainy night at a Jeju stone-house gate, DAD in the orange rain coat with the hood up half turned toward the lane, 11-year-old HANEUL in a pale-blue t-shirt gripping his wet sleeve with both hands, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) in a pale blouse and dark trousers standing in the doorway holding the orange rain coat she has just taken from him, one porch lamp_
5. 소매가 내 손에서 빠져나갔다. 그게 아빠와 나눈 마지막 인사였다.  
   `Cut_T_OP_05` (앵커 H12 · **신규**)  
   _close-up of a child's small wet hands opening as an orange rain coat sleeve slips out of them, rain, the lane beyond out of focus, low lamp light_
6. 다음 날 아침은 거짓말처럼 맑았다. 송전탑 아래 바다에서 아빠 배가 뒤집힌 채 발견됐다. 삐뚤어진 하트가 그려진 부표 하나만 떠 있었다.  
   `Cut_T_OP_06` (앵커 - · **유지** `Cut_S_N2_11`)  
7. 며칠 뒤 구조대가 바위 사이에서 닫힌 방수 통을 건졌다. 엄마는 그걸 열어 보지 못했다. 장롱 깊은 곳에 넣고 문을 닫았다.  
   `Cut_T_OP_07` (앵커 MOM · **신규**)  
   _dim Jeju room, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) in a pale blouse kneeling before an old wooden wardrobe pushing a small yellow waterproof case with a red heart on its lid deep behind folded blankets, her face turned away, afternoon light through paper door_
8. 아빠는 내 선물 때문에 죽었다. 나는 그렇게 믿었다. 그날부터 생일과 송전탑과 주황색 우비는 전부 같은 공포였다.  
   `Cut_T_OP_08` (앵커 H12 · **신규**)  
   _11-year-old HANEUL in a pale-blue t-shirt sitting alone under the eaves of the gate in bright daylight after the storm, hugging her knees, an orange rain coat hanging on a nail on the wall beside her, the steel transmission tower on the hill far behind, harsh sun_
9. 1년 뒤. 서울서 요양을 온 창백한 전학생. 검은 차와 경호원을 달고 다니는 이상한 애. 그 애는 나를 불쌍하게 보지 않은 첫 사람이었다.  
   `Cut_T_OP_09` (앵커 D12 · **유지** `Cut_S_OP_4`)  
10. 길을 알려 준 값이라며 우유 한 팩을 줬다. 아이들이 내 가방을 도랑에 던진 날엔 흰 셔츠가 진흙투성이가 되도록 직접 건졌다. 뒤늦게 그 애 그림자에서 경호원이 나왔다. 아이들은 도망쳤다.  
   `Cut_T_OP_10` (앵커 D12 · **유지** `Cut_S_OP_5`)  
11. 그 애는 탑 아래 움푹한 자리를 우리 비밀 기지로 만들었다. 담요 한 장, 고장 난 라디오, 유리구슬, 우유 두 병. 라디오에서 잡힌 91.9는 둘만의 주파수가 됐다.  
   `Cut_T_OP_11` (앵커 H12 · **유지** `Cut_S_OP_7`)  
12. 폭우의 밤, 엄마가 마당에서 쓰러졌다. 우리는 바퀴 빠진 리어카를 고쳐 병원까지 2킬로를 밀었다. 엄마는 살았고 그 애는 복도에서 쓰러졌다.  
   `Cut_T_OP_12` (앵커 H12+D12+MOM · **신규**)  
   _heavy night rain on a coastal road, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) lying unconscious on a wooden handcart under a boy's coat, two small 12-year-old children — girl HANEUL in a pale-blue t-shirt and boy DOYUN in a navy vest — straining to push the cart together, town lights far ahead_
13. 작별은 5분. 우유 두 병. 하나는 오늘, 하나는 내일. 스무 살 네 생일에 송전탑 아래서 기다릴게. 나는 차를 따라 달리며 소리쳤다. 안 늦을게. 입 모양은 「알아」였다.  
   `Cut_T_OP_13` (앵커 H12 · **유지** `Cut_S_N6_05`)  
14. 열아홉 살 생일 아침, 아빠의 하트 부표를 들고 바다로 갔다. 기억은 거기서 끊겼다. 눈을 뜬 곳은 낯선 방. 달력에는 한 줄. 스무 살 생일, 송전탑 아래. 1년 남았다.  
   `Cut_T_OP_14` (앵커 H19 · **유지** `Cut_S_OP_11`)  

## CS1 「이름」 · BGM_M1 · 14컷 × 8.5s · 채도 0.85

1. 눈을 떴다. 문가에 주황 우비를 뒤집어쓴 여섯 살쯤의 꼬마가 앉아 있었다. 내가 깰 걸 알고 있었던 것처럼, 놀라지 않았다.  
   `Cut_T_N1_01` (앵커 KID · **유지**)  
2. 이름을 물었다. 입을 열었는데 아무것도 안 나왔다. 내가 누군지 모른다는 것보다, 그게 이상하게 낯설지 않다는 게 더 무서웠다.  
   `Cut_T_N1_02` (앵커 H19 · **유지**)  
3. 벽 달력에 연필로 한 줄이 씌어 있었다. 내년 내 생일, 송전탑 아래서 보자. 누가 썼는지, 누구를 보자는 건지 몰랐다. 잘은 모르지만 그 날짜만은 꼭 지켜야 할 것 같았다. 그 한 줄이 나를 일으켰다.  
   `Cut_T_N1_03` (앵커 H19 · **유지** `Cut_S_N1_03`)  
4. 꼬마는 나를 바다처럼 파랗다며 「바다누나」라고 불렀다. 촌스럽다고 했다. 그런데 그 이름으로 불릴 때마다 가슴 한쪽이 저렸다. 오래전에도 누가 그렇게 불렀던 것처럼.  
   `Cut_T_N1_04` (앵커 KID · **유지** `Cut_S_N1_05`)  
5. 꼬마의 이름을 물었다. 대답 대신, 이름을 말하면 다른 사람이 돼 버릴 것 같다고 했다. 골목에서 할머니가 소리쳤다. 「민재야, 찻길로 댕기지 말라.」 꼬마는 잘못 들었다며 후드를 더 눌러썼다.  
   `Cut_T_N1_05` (앵커 KID · **신규**)  
   _Jeju village lane with black basalt walls, the tiny child in the oversized orange rain coat pulling the hood down over its face, an old GRANNY in a flowered work shirt calling from her gate in the background, 19-year-old HANEUL in a sky-blue hoodie looking back and forth between them, morning haze_
6. 마을 사람들은 내 인사에 대답하지 않았다. 무시하는 건지 목소리가 작은 건지 알 수 없었다. 사람들 시선은 늘 내 어깨 조금 옆을 지나갔다.  
   `Cut_T_N1_06` (앵커 H19 · **유지** `Cut_S_N1_04`)  
7. 운행이 끊긴 정류장. 꼬마가 멀리 탑을 가리켰다. 4.2킬로. 노을 전에 닿으면 내가 이기는 놀이라고 했다. 누구랑 하는 게임이냐고 물었다. 「기다리는 사람이랑.」  
   `Cut_T_N1_07` (앵커 H19 · **유지**)  
8. 집 앞에 낡은 스케이트보드가 있었다. 처음 보는 물건인데 발이 먼저 균형을 잡았다. 돌담 사이 굽은 길과 바람의 방향까지 몸이 알고 있었다. 꼬마가 무심코 말했다. 「매일 탔잖아.」 되묻자 앞서 달아났다.  
   `Cut_T_N1_08` (앵커 H19 · **유지** `Cut_S_N1_10`)  
9. 문고리를 잡는데 손에 힘이 안 들어갔다. 손끝이 저렸다. 꼬마가 내 손을 보더니 물었다. 「보드는 잘 잡히지?」 그건 잘 잡혔다. 왜 그런 걸 묻느냐고 하니 그냥이라고 했다. 기억상실 뒤끝이겠거니 했다.  
   `Cut_T_N1_09` (앵커 H19 · **신규**)  
   _close-up of 19-year-old HANEUL's hand gripping the iron ring handle of an old wooden Jeju gate, knuckles pale, the ring not turning, her other hand rubbing her wrist, soft morning light, sky-blue hoodie sleeve, no transparency_
10. 탑 아래 돌 세 개. 손을 대려 하자 꼬마가 처음으로 다급해졌다. 세 번째 돌은 한 번 잃으면 못 찾을 수도 있다고. 옆에 우유 두 병. 한 병은 차갑고 한 병은 미지근했다.  
   `Cut_T_N1_10` (앵커 KID · **유지** `Cut_S_N1_08`)  
11. 「하나는 오늘, 하나는 내일.」 이유도 모르고 그렇게 중얼거렸다. 꼬마가 나를 올려다봤다. 아무 말도 안 했다.  
    `Cut_T_N1_11` (앵커 H19 · **유지** `Cut_S_N1_11`)  
12. 멀리 키 큰 남자가 약을 삼키고 있었다. 억새와 철골에 가려 얼굴은 안 보였다. 아는 사람이었다. 왜인지는 몰라도 확실했다. 달려가려는 나를 꼬마가 잡았다. 「지금 놀라게 하면 안 돼.」  
    `Cut_T_N1_12` (앵커 D20 · **신규**)  
    _seen through tall silver grass and the steel lattice of the transmission tower, a tall pale young man D20 in a grey hooded coat tipping pills from a small bottle into his mouth, face hidden by the hood and the grass, sunset backlight, 19-year-old HANEUL's hand held back by a small orange sleeve in the foreground_
13. 남자는 우유 한 병을 마시고 한 병은 그대로 두고 갔다. 나는 처음으로 생각했다. 이 약속이 나 혼자만의 것이 아닐지도 모른다고.  
    `Cut_T_N1_13` (앵커 D20 · **유지** `Cut_S_N7_09`)  
14. 돌아오는 길, 꼬마가 내 손을 잡았다. 작고 차가운 손. 「내일도 가자.」 나는 고개를 끄덕였다.  
    `Cut_T_N1_14` (앵커 KID · **유지** `Cut_S_N1_12`)  

## EV1 「손가락 사이로」 · CH2 · 4컷 × 7s · 채도 0.85

1. 가게에 갔다. 우유를 집으려는데 손에 힘이 안 들어갔다. 병이 미끄러져 선반 위를 굴렀다. 두 번, 세 번. 주인 할머니는 라디오만 듣고 있었다.  
   `Cut_T_E1_01` (앵커 H19 · **신규**)  
   _inside a tiny Jeju corner shop, 19-year-old HANEUL in a sky-blue hoodie catching at a glass milk bottle that is tipping over on the shelf, her fingers stiff and clumsy, an old shopkeeper behind the counter turned toward a small radio, warm bulb light, no transparency_
2. 밖에 나와 꼬마에게 화를 냈다. 왜 이런 건 안 되냐고. 꼬마는 대답 대신 내 주머니를 가리켰다. 유리구슬 한 알. 그건 잡혔다.  
   `Cut_T_E1_02` (앵커 KID · **신규**)  
   _outside the shop, 19-year-old HANEUL holding up a single blue glass marble between two fingers with a puzzled face, the tiny child in the orange rain coat pointing at it, black basalt wall behind, midday_
3. 「누나 거였던 것만 돼. 오래 갖고 있던 거.」 그럼 우유는 왜 안 돼. 「아직 누나 거 아니니까.」 아이 말은 늘 반쯤 이상하다.  
   `Cut_T_E1_03` (앵커 KID · **신규**)  
   _close two-shot on a stone wall at noon, the tiny child in the orange rain coat sitting with legs dangling explaining with both hands, 19-year-old HANEUL in a sky-blue hoodie listening with her chin on her knees, sea behind_
4. 그날 밤 탑 아래 우유 두 병 중 미지근한 쪽을 들어 봤다. 잡혔다. 누군가 매일 두고 가는 것이니까, 라고 생각하다 말았다.  
   `Cut_T_E1_04` (앵커 H19 · **신규**)  
   _night at the foot of the transmission tower, 19-year-old HANEUL in a sky-blue hoodie holding one glass milk bottle firmly in both hands, the other bottle still on the rock, the red tower lamp above, stars_

## CS2 「하트」 · BGM_M6 · 13컷 × 8.5s · 채도 0.78

1. 이 집엔 누가 급하게 떠난 흔적이 가득했다. 빨랫줄에 크기가 다른 해녀복 두 벌. 방문 옆에 낡은 주황 우비. 현관 앞 양철 상자엔 우표만 붙고 소인이 없는 편지 열아홉 통.  
   `Cut_T_N2_01` (앵커 H19 · **신규**)  
   _yard of an old Jeju stone house, two black haenyeo wetsuits of different sizes on a clothesline, an old orange rain coat on a hook by the door, a dented tin box on the step holding a stack of white envelopes, 19-year-old HANEUL in a sky-blue hoodie standing over the box_
2. 같은 사람이 여러 해에 걸쳐 쓴 글씨. 받는 사람 이름만 물에 번진 것처럼 읽히지 않았다. 첫 통을 뜯으려 하면 이유 없이 손이 떨렸다.  
   `Cut_T_N2_02` (앵커 H19 · **유지**)  
3. 꼬마가 말했다. 이름을 찾으면 잊고 있던 것도 같이 돌아온다고. 나는 편지를 내려놓았다. 기억이 돌아온 뒤에 지금의 내가 없어질 것 같아서. 그게 더 무서웠다.  
   `Cut_T_N2_03` (앵커 KID · **신규**)  
   _dim doorway of the Jeju house, 19-year-old HANEUL in a sky-blue hoodie putting the tin box of letters back down on the step, the tiny child in the orange rain coat squatting beside it looking up at her, the letters untouched, grey afternoon_
4. 바람이 한 통을 채 갔다. 쫓다가 탑의 쇠기둥에 손을 댔다. 낮게 웅웅거리는 소리. 그리고 귓속으로 해녀의 숨비소리가 밀려왔다.  
   `Cut_T_N2_04` (앵커 H19 · **유지** `Cut_S_N2_03`)  
5. **— 회상 —** 물 위로 올라온 엄마가 길게 숨을 내쉬고, 어린 나는 바위 위에서 그 횟수를 셌다. 열일곱 번 숨을 쉰 게 아니라 열일곱 번 돌아온 거라고. 내 목소리였다.  
   `Cut_T_N2_05` (앵커 MOM+H12 · **신규**)  
   _morning sea, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) as a haenyeo diver breaking the surface with her black neoprene hood pushed back and goggles on her forehead, exhaling a long whistling breath, an orange tewak float beside her, in the foreground 12-year-old girl HANEUL sitting on black rocks counting on her fingers_
6. **— 회상 · 아홉 해 전 —** 엄마가 해녀였다는 것과 함께 아빠도 돌아왔다. 작은 배, 낡은 그물, 부표마다 그려 놓은 삐뚤어진 붉은 하트.  
   `Cut_T_N2_06` (앵커 DAD · **유지** `Cut_S_N2_07`)  
7. **— 회상 · 아홉 해 전 —** 큰 하트는 아빠, 작은 하트는 나. 큰 건 언제나 작은 걸 지켜야 한다고 내가 말했다. 아빠는 웃었다. 왼쪽 눈이 접혔다. 「그러주. 꼭.」  
   `Cut_T_N2_07` (앵커 DAD · **신규**)  
   _close-up of a white buoy with one large and one small red heart, DAD's weathered hand holding it up, 11-year-old HANEUL's small finger pointing at the small heart, DAD laughing with his left eye creasing, harbor light_
8. 절벽 아래 바위에 그 부표가 걸려 있었다. 지워진 데를 누가 새 페인트로 덧칠해 놓았다. 냄새가 아직 났다.  
   `Cut_T_N2_08` (앵커 H19 · **유지** `Cut_S_N4_01`)  
9. 꼬마 손톱 밑에 붉은 게 묻어 있었다. 젖은 부표를 만졌을 뿐이라고 했다. 우리 아빠 하트를 어떻게 아느냐고 물었다. 대답이 없었다.  
   `Cut_T_N2_09` (앵커 KID · **유지** `Cut_S_N4_02`)  
10. **— 회상 · 태풍 전날 밤 —** 아빠가 생일 전날 바다로 나갔고 돌아오지 못했다는 것까지 돌아왔다. 머리띠와 마지막 말은 아직 안개 속이었다.  
    `Cut_T_N2_10` (앵커 DAD · **유지** `Cut_S_N2_09`)  
11. 지켜 주겠다던 약속은 거짓말이었다. 나는 부표에 대고 화를 냈다. 꼬마는 한참 하트를 보다가 말했다. 「어른도 못 지키는 약속이 있어.」  
    `Cut_T_N2_11` (앵커 KID · **신규**)  
    _black rocks below the cliff at dusk, 19-year-old HANEUL in a sky-blue hoodie shouting at the heart buoy with tears, the tiny child in the orange rain coat sitting a little apart on a rock looking at the buoy not at her, sea spray_
12. 여섯 살이 할 말이 아니었다. 어떻게 아느냐고 물었다. 「기다려 본 애는 알아.」  
    `Cut_T_N2_12` (앵커 KID · **신규**)  
    _close-up under the orange hood, the child's round face half in shadow, big eyes looking straight at the camera, calm and much older than six for one moment, dusk light on the wet cheek_
13. 탑이 무섭고, 그리웠다. 오늘도 돌 세 개는 그대로였다. 누군가 매일 다녀가는 것처럼.  
    `Cut_T_N2_13` (앵커 H19 · **유지** `Cut_S_N2_12`)  

## EV2 「열일곱 번」 · CH4 · 4컷 × 7s · 채도 0.78

1. 바닷가 바위에 앉아 해녀들의 숨비소리를 셌다. 열일곱 번. 엄마는 없었다. 다른 해녀들이었다.  
   `Cut_T_E2_01` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie sitting on black rocks by the sea, three haenyeo divers far out in the water with orange tewak floats, one surfacing with a whistling breath, morning glare_
2. 물에서 나온 할머니 해녀들이 태왁을 내려놓고 쉬었다. 「하늘이는 아직도 못 찾았주?」 「1년 다 되감서.」 처음 듣는 이름이었다. 아마.  
   `Cut_T_E2_02` (앵커 GRANNY · **신규**)  
   _two elderly haenyeo women in black wetsuits resting on the rocks with their tewak floats, talking, 19-year-old HANEUL in a sky-blue hoodie sitting a few meters away with her back half turned, listening, sea behind_
3. 꼬마가 내 소매를 당겼다. 「가자.」 「왜.」 「가자.」 두 번 말하면 진짜 가야 하는 거였다.  
   `Cut_T_E2_03` (앵커 KID · **신규**)  
   _the tiny child in the orange rain coat tugging the sleeve of 19-year-old HANEUL's sky-blue hoodie with both hands, HANEUL still looking back toward the divers, rocks and foam_
4. 돌아오는 길에 그 이름을 몇 번 발음해 봤다. 하늘. 파란 건 나였다. 바다누나.  
   `Cut_T_E2_04` (앵커 H19 · **신규**)  
   _coastal road at noon, 19-year-old HANEUL in a sky-blue hoodie walking with her skateboard under her arm, mouth slightly open as if saying a word, the tiny child in the orange rain coat far ahead, bright sky_

## CS3 「우리 기지」 · BGM_M2 · 13컷 × 8.5s · 채도 0.7

1. 부엌 깊은 데서 낡은 도시락이 나왔다. 하트 모양 계란말이의 흔적. 뚜껑 안쪽에 어린 글씨. 반은 네 거. 열두 살의 봄이 열렸다.  
   `Cut_T_N3_01` (앵커 H19 · **유지**)  
2. **— 회상 · 여덟 해 전 —** 나는 학교에서 도시락을 못 먹는 애였다. 생선 냄새가 난다고, 물만 마시는 것까지 구경거리였다.  
   `Cut_T_N3_02` (앵커 H12 · **유지**)  
3. **— 회상 —** 운동장 구석으로 도망친 내 옆에 그 애가 도시락을 들고 앉았다. 동정은 필요 없다고 했다. 「동정이면 반이나 안 줘. 친구는 반씩 먹는 거야.」 그러고 한가운데에 선을 그었다.  
   `Cut_T_N3_03` (앵커 D12 · **유지**)  
4. **— 회상 —** 싸움도 못하면서 왜 앞을 막느냐고 물었다. 「잘하는 것만 하고 살면 네 앞에 설 사람이 없잖아.」 그 애가 강해서가 아니었다. 무서워하면서 안 물러나서였다.  
   `Cut_T_N3_04` (앵커 D12 · **유지** `Cut_S_N3_10`)  
5. **— 회상 —** 그 애는 탑 아래 움푹한 자리를 기지로 꾸몄다. 처음엔 싫었다. 담요를 펴고 우유를 나눠 마시는 날이 늘면서 아빠가 죽은 자리와 우리 기지가 조금씩 갈라졌다.  
   `Cut_T_N3_05` (앵커 H12 · **유지** `Cut_S_N3_08`)  
6. **— 회상 —** 그 애는 무서운 장소를 잊게 하려 하지 않았다. 내가 무서워할 때 옆에 앉아 있었다. 그게 전부였고, 그게 됐다.  
   `Cut_T_N3_06` (앵커 D12 · **신규**)  
   _two 12-year-old children HANEUL in a pale-blue t-shirt and DOYUN in a navy vest sitting side by side in the grassy hollow under the tower at dusk, not talking, HANEUL hugging her knees looking at the sea below, DOYUN just sitting next to her, an old blanket, one milk bottle_
7. **— 회상 —** 정류장에서 탑까지 4.2킬로 놀이도 그때 시작됐다. 나는 보드로 앞서갔고 그 애는 헐떡이며 따라왔다. 힘들어 보이면 길을 잘못 들었다는 핑계로 되돌아갔다. 그 애는 늘 자기가 졌다고 했다. 내가 돌아올 거라는 내기에선 자기가 이겼다고 우겼다.  
   `Cut_T_N3_07` (앵커 H12 · **유지** `Cut_S_N3_09`)  
8. **— 회상 —** 고장 난 라디오를 같이 뜯다가 91.9에서 희미한 방송이 잡혔다. 다이얼 옆에 붉은 하트를 그렸다. 헤어지면 잡음 속에서라도 이름을 부르기로 했다.  
   `Cut_T_N3_08` (앵커 H12 · **신규**)  
   _two 12-year-old children HANEUL and DOYUN crouched over an old radio with its back open, screws and a screwdriver on the blanket, HANEUL drawing a small red heart beside the dial with a marker, DOYUN holding the antenna, golden afternoon in the hollow under the tower_
9. **— 회상 —** 「꼭 알아들을게.」 「반은 진짜고 반은 거짓말이지.」 내가 웃었다. 그 애도 웃었다.  
   `Cut_T_N3_09` (앵커 D12 · **유지** `Cut_S_N3_07`)  
10. 지금의 기지에도 그 라디오가 있었다. 다이얼을 돌리자 잡음 사이로 어린 목소리가 내 이름 비슷한 걸 불렀다. 꼬마가 라디오를 꺼 버렸다. 「아직 들을 때 아니야.」  
    `Cut_T_N3_10` (앵커 KID · **신규**)  
    _dusk in the grassy hollow, 19-year-old HANEUL in a sky-blue hoodie holding the old radio with its dial glowing 91.9, the tiny child in the orange rain coat reaching over and pressing the switch off, rust and grass, a few glass marbles beside the radio_
11. 유리구슬이 몇 알 남아 있었다. 세어 보지는 않았다.  
    `Cut_T_N3_11` (앵커 H19 · **유지** `Cut_S_N3_11`)  
12. 저녁에 남자가 기지에 다시 왔다. 나는 기억 속 아이의 이름을 처음으로 불렀다. 도윤.  
    `Cut_T_N3_12` (앵커 D20 · **유지** `Cut_S_N2_06`)  
13. 남자는 걸음을 멈췄다. 끝내 돌아보지는 않았다. 나는 그 등을 오래 봤다.  
    `Cut_T_N3_13` (앵커 D20 · **신규**)  
    _night under the transmission tower, 20-year-old DOYUN in a grey hooded coat stopped mid-step with his back to the camera, head slightly turned but not looking back, 19-year-old HANEUL in a sky-blue hoodie a few meters behind him with her hand half raised, the red tower lamp_

## EV3 「우유 한 팩」 · CH6 · 4컷 × 7s · 채도 0.7

1. **— 회상 · 전학 첫날 —** 그 애가 길을 물었다. 학교까지 알려 줬더니 우유 한 팩을 내밀었다. 「빚지는 거 싫어서.」 상표가 처음 보는 거였다. 서울 우유였나.  
   `Cut_T_E3_01` (앵커 D12 · **신규**)  
   _spring lane in a Jeju village, 12-year-old DOYUN in a white shirt and navy vest holding out a small carton of milk to 12-year-old HANEUL in a pale-blue t-shirt, a black sedan and a bodyguard waiting at the end of the lane, cherry petals_
2. **— 회상 —** 나는 안 받았다. 그 애는 담장 위에 올려놓고 갔다. 저녁에 가 보니 없었다. 고양이가 가져갔을 것이다.  
   `Cut_T_E3_02` (앵커 H12 · **신규**)  
   _a small milk carton standing on top of a black basalt stone wall in evening light, 12-year-old HANEUL in a pale-blue t-shirt peeking at it from behind the gate post, a cat's tail at the far end of the wall_
3. 가게 앞에서 남자를 봤다. 우유 두 병. 딸깍, 딸깍. 주인 할머니가 물었다. 「오늘도 두 개라?」 「예.」  
   `Cut_T_E3_03` (앵커 D20 · **유지** `Cut_S_EB_08`)  
4. 새벽엔 약을 먹고 저녁엔 우유를 산다. 그 사이엔 뭘 하는지 몰랐다. 꼬마는 「따라가지 마」라고만 했다.  
   `Cut_T_E3_04` (앵커 KID · **신규**)  
   _coastal road at dusk, 20-year-old DOYUN in a grey hooded coat walking away far ahead carrying two milk bottles, 19-year-old HANEUL in a sky-blue hoodie about to follow, the tiny child in the orange rain coat standing in front of her with arms spread_

## CS4 「열두 개의 초」 · BGM_M3 · 13컷 × 8.5s · 채도 0.6

1. 돌 위에 초 하나가 켜져 있었다. 다가가자 저절로 꺼졌다. 바람은 없었다.  
   `Cut_T_N4_01` (앵커 H19 · **유지** `Cut_S_N4_04`)  
2. **— 회상 · 열두 살 생일 —** 그 애가 탑 아래에 작은 케이크와 초 열두 개를 준비했다. 내게 생일이 뭔지, 이 바다가 뭘 빼앗아 갔는지 모른 채.  
   `Cut_T_N4_02` (앵커 D12 · **유지** `Cut_S_N4_05`)  
3. **— 회상 —** 불붙은 초를 본 순간 폭풍우와 뒤집힌 배가 돌아왔다. 나는 케이크를 엎었다. 여기서 아빠가 죽었어. 모르는 주제에 왜 자꾸 끼어들어. 친구도 아니면서.  
   `Cut_T_N4_03` (앵커 H12 · **유지** `Cut_S_N4_06`)  
4. **— 회상 —** 그 애는 변명하지 않았다. 「몰랐어.」 우유 두 병을 두고 돌아갔다.  
   `Cut_T_N4_04` (앵커 D12 · **신규**)  
   _night under the tower, 12-year-old DOYUN in a navy vest walking away down the dark slope with his head down, two glass milk bottles left on the grass beside the ruined cake, 12-year-old HANEUL standing with her fists clenched, tower lamp_
5. **— 회상 · 열흘 —** 그 뒤 열흘 동안 그 애는 오지 않았다. 대신 담장 위에 매일 도시락이 놓였다. 첫날엔 따뜻했다. 열흘째엔 얼어 있었다.  
   `Cut_T_N4_05` (앵커 H12 · **유지** `Cut_S_N4_07`)  
6. **— 회상 —** 마지막 뚜껑 안에 글씨가 있었다. 친구가 아니라도 반은 네 거. 나는 도시락을 들고 탑으로 달렸다.  
   `Cut_T_N4_06` (앵커 H12 · **신규**)  
   _close-up of a frosted tin lunchbox lid held open by small hands, childish handwriting scratched inside the lid, 12-year-old HANEUL's breath fogging in the cold, grey winter morning in the yard_
7. **— 회상 —** 반대편 길에서 그 애가 헐떡이며 뛰어오고 있었다. 사과 대신 서로 왜 뛰었느냐고 나무랐다. 「올 것 같아서.」 「다음부턴 내가 오기 전에 먼저 와 있어.」  
   `Cut_T_N4_07` (앵커 D12 · **유지** `Cut_S_N4_08`)  
8. **— 회상 —** 그렇게 우리는 제일 큰 상처를 설명하지 않은 채 다시 나란히 앉았다.  
   `Cut_T_N4_08` (앵커 H12 · **신규**)  
   _two 12-year-old children sitting side by side on the stone edge of the hollow under the tower in thin winter light, the tin lunchbox open between them with the line drawn down the middle, HANEUL in a thin cardigan over her pale-blue t-shirt, DOYUN in his navy vest, both looking at the sea_
9. **— 회상 —** 그 애가 기지 앞에 돌 세 개를 쌓았다. 맨 아래는 제일 잘 버티는 너. 가운데는 나. 맨 위는 너희 아빠. 무너지면 다시 쌓고, 잃어버리면 내가 기억할게.  
   `Cut_T_N4_09` (앵커 H12 · **유지** `Cut_S_N4_10`)  
10. **— 회상 —** 마을 사람들은 그 집 아빠 죽은 자리라고 불렀다. 그 애만 우리 기지라고 불렀다. 그날부터 나도 그렇게 불렀다.  
    `Cut_T_N4_10` (앵커 D12 · **유지** `Cut_S_N4_09`)  
11. 같은 돌 세 개 앞에서 꼬마와 여름을 맞았다. 봄의 365일은 196일이 됐다. 나는 매일 4.2킬로를 달렸고 도윤은 매일 우유 두 병을 두고 갔다. 기억이 돌아오는 속도보다 약속의 날이 오는 속도가 빨랐다.  
    `Cut_T_N4_11` (앵커 H19 · **신규**)  
    _summer noon under the transmission tower, three stacked stones on the grass, 19-year-old HANEUL in a sky-blue hoodie with sleeves pushed up sitting beside them with her skateboard, the tiny child in the orange rain coat (hood still up despite the heat) lying on its back in the grass, cicadas implied, bright green and blue_
12. 돌이 무너져 있던 아침이 있었다. 내가 다시 쌓았다. 세 번째 돌이 손에 잡혀서, 그게 이상하게 고마웠다.  
    `Cut_T_N4_12` (앵커 H19 · **신규**)  
    _close-up of 19-year-old HANEUL's hands placing the third small stone on top of two others on the grass, morning dew, the tiny orange boots of the child standing beside, soft light_
13. 누가 이걸 매번 다시 쌓아 두느냐고 물었다. 「기다리는 사람이 무너질 때마다.」 꼬마는 그렇게만 말했다.  
    `Cut_T_N4_13` (앵커 KID · **신규**)  
    _the tiny child in the orange rain coat crouching beside the three stacked stones at sunset with one hand resting on the top stone, looking down the slope toward the coastal road where a distant tall figure in a grey coat walks away, tower above_

## EV4 「달력」 · CH8 · 4컷 × 7s · 채도 0.6

1. 달력의 빈 칸에 매일 X를 그었다. 연필은 잘 잡혔다. 손에 힘이 붙는 물건이 늘고 있었다. 좋은 일인지 몰랐다.  
   `Cut_T_E4_01` (앵커 H19 · **신규**)  
   _close-up of the old wall calendar with rows of pencil X marks leading toward one circled date with a tiny heart, 19-year-old HANEUL's hand holding a short pencil, late-summer light_
2. 남자의 방 창가에 종이 한 장이 붙어 있었다. 버스 시간표. 서울행 첫차에 동그라미. 날짜는 안 적혀 있었다.  
   `Cut_T_E4_02` (앵커 D20 · **신규**)  
   _a small rented room seen through an open window from outside, a bus timetable taped to the wall with one departure circled in pen, a grey coat on a chair, two empty milk bottles on the sill, 19-year-old HANEUL's hand on the window frame from outside_
3. 꼬마는 여름에도 우비를 벗지 않았다. 덥지 않냐고 물었다. 「이거 벗으면 못 찾아.」 누가 누굴.  
   `Cut_T_E4_03` (앵커 KID · **신규**)  
   _hot late-summer road, the tiny child in the oversized orange rain coat with the hood up walking ahead, sweat on the visible round cheek, 19-year-old HANEUL in a sky-blue hoodie tied around her waist over a white t-shirt walking behind holding a skateboard, heat haze_
4. 4.2킬로를 처음으로 노을 전에 끝냈다. 탑 아래엔 아무도 없었다. 우유 두 병만 있었다. 이긴 건지 몰랐다.  
   `Cut_T_E4_04` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie arriving under the transmission tower on her skateboard with the sun still above the sea, out of breath, two milk bottles on the rock, nobody else, long shadow_

## EV5 「찢어진 소매」 · CH9 · 4컷 × 7s · 채도 0.5

1. 빗속, 탑 아래 우산도 없이 선 뒷모습. 뛰어가면 사라지고 멈추면 다시 있었다. 나는 찻길을 봤어야 했다.  
   `Cut_T_E5_01` (앵커 D20 · **유지** `Cut_S_N5_02`)  
2. 트럭이 바로 옆을 스쳤다. 나는 길 옆으로 넘어졌다. 밀어낸 건 꼬마였다. 우비 소매가 찢어져 있었다.  
   `Cut_T_E5_02` (앵커 KID · **유지** `Cut_S_N5_03`)  
3. 다쳤냐고 물었다. 꼬마는 소매를 뒤로 감추고 웃기만 했다. 나 때문이라는 걸 그땐 몰랐다. 미안했다.  
   `Cut_T_E5_03` (앵커 KID · **유지** `Cut_S_N5_04`)  
4. 그날 밤 소매를 기워 주려고 바늘을 잡았다. 손이 떨려서 실이 안 꿰어졌다. 꼬마가 대신 실을 이로 끊었다.  
   `Cut_T_E5_04` (앵커 KID · **신규**)  
   _night inside the Jeju room by a small lamp, the tiny child in the orange rain coat biting off a thread with its teeth, the torn sleeve roughly stitched, 19-year-old HANEUL in a sky-blue hoodie sitting beside holding a needle with a trembling hand, thread still unthreaded_

## CS5 「그 밤」 · BGM_M6 · 13컷 × 8.5s · 채도 0.5

1. 길가에 낡은 리어카가 있었다. 바퀴 하나가 새것이었다. 손잡이엔 작은 손톱이 파고든 오래된 흠집.  
   `Cut_T_N5_01` (앵커 H19 · **유지** `Cut_S_N5_11`)  
2. 그 자국 위에 손가락을 올렸다. 잡혔다. 여덟 해 전 폭우의 밤이 통째로 돌아왔다.  
   `Cut_T_N5_02` (앵커 H19 · **신규**)  
   _close-up of 19-year-old HANEUL's fingertip resting exactly in an old small fingernail scratch on the worn wooden handle of a handcart, grey afternoon, the image edges beginning to blur into rain_
3. **— 회상 · 여덟 해 전 —** 마당에 엎어진 엄마를 먼저 본 건 그 애였다. 심장병. 전화선도 휴대전화도 끊겼다. 열두 살 둘이 할 수 있는 건 거의 없었다.  
   `Cut_T_N5_03` (앵커 D12+MOM · **신규**)  
   _pouring night rain in a Jeju stone-house yard, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) in a pale blouse collapsed face down on the wet ground, 12-year-old boy DOYUN in a navy vest shouting, 12-year-old girl HANEUL in a pale-blue t-shirt frozen at the door, porch lamp_
4. **— 회상 —** 내가 굳어 있자 그 애가 말했다. 「살 수 있어.」 확신해서가 아니었다. 그렇게 말해야 내가 움직이니까.  
   `Cut_T_N5_04` (앵커 D12 · **신규**)  
   _pouring night rain, 12-year-old DOYUN in a soaked navy vest gripping the shoulders of 12-year-old HANEUL in a pale-blue t-shirt and looking straight into her face, her eyes wide and blank, MOM lying behind them on the wet ground, lamp light_
5. **— 회상 —** 길가 리어카의 바퀴를 진흙에서 다시 끼웠다. 엄마를 눕히고 그 애 코트를 덮었다. 병원까지 2킬로.  
   `Cut_T_N5_05` (앵커 H12+D12+MOM · **신규**)  
   _heavy night rain, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) lying unconscious on a wooden handcart covered with a boy's coat, her face visible and calm, two small 12-year-old children — girl HANEUL in a pale-blue t-shirt and boy DOYUN in a navy vest — pushing the cart together, a dead phone on the wet ground_
6. **— 회상 —** 빗길 중간에서 그 애 무릎이 꺾였다. 엄마부터 데려가라고 했다. 싫다고 했다. 셋이 같이 간다. 규칙은 거기서 정했다.  
   `Cut_T_N5_06` (앵커 D12 · **유지** `Cut_S_N5_08`)  
7. **— 회상 —** 그 애는 몇 번이나 손을 놓칠 뻔했다. 내가 겹쳐 잡은 손 때문에 다시 일어났다. 병원 불빛이 보일 때 그 애가 물었다. 무서워? 처음으로 무섭다고 했다.  
   `Cut_T_N5_07` (앵커 H12 · **신규**)  
   _rain-soaked night road, close-up of two small hands overlapping on the wet wooden handle of the handcart, one hand pale and slipping, the other gripping over it, hospital lights blurred far ahead_
8. **— 회상 —** 「나도.」 그 애도 무섭다고 했다. 엄마가 죽을까 봐가 아니라, 내가 다시 혼자 될까 봐.  
   `Cut_T_N5_08` (앵커 D12 · **신규**)  
   _rain, close two-shot of the two 12-year-old children pushing the cart side by side, DOYUN's face turned to HANEUL saying something, both drenched, the town lights reflected in the puddles_
9. **— 회상 —** 엄마는 살았다. 그 애는 응급실 문이 닫히는 걸 보고 복도에서 쓰러졌다. 그 밤에 나눈 제일 진짜 말은 길지 않았다. 무서웠어. 응. 나도.  
   `Cut_T_N5_09` (앵커 H12 · **유지** `Cut_S_N5_10`)  
10. 리어카 손잡이를 잡고 한참 울었다. 왜 우는지 이번엔 알았다.  
    `Cut_T_N5_10` (앵커 H19 · **신규**)  
    _grey afternoon on the coastal road, 19-year-old HANEUL in a sky-blue hoodie crouched with her forehead against the wooden handle of the old handcart, shoulders down, the new wheel bright against the old wood, no one else on the road_
11. 꼬마가 리어카 위에 올라앉았다. 「밀어 줘.」 웃으면서 밀었다. 새 바퀴가 부드럽게 굴렀다.  
    `Cut_T_N5_11` (앵커 KID · **유지** `Cut_S_N5_12`)  
12. 밤사이 키 큰 남자가 바퀴를 갈아 줬다고 꼬마가 말했다. 왜 그랬을까. 이 리어카가 누구 건지 그 사람은 알고 있었다.  
    `Cut_T_N5_12` (앵커 KID · **유지** `Cut_S_N5_05`)  
13. 멀리서 도윤이 우리 쪽은 보지 않고 같은 길을 걸어갔다. 리어카 바퀴 소리가 그 사람에게 닿았는지는 모르겠다.  
    `Cut_T_N5_13` (앵커 D20 · **신규**)  
    _sunset coastal road, in the foreground the tiny child riding the handcart pushed by 19-year-old HANEUL in a sky-blue hoodie, far ahead on the same road 20-year-old DOYUN in a grey hooded coat walking away with his back turned, long shadows, tower on the hill_

## EV6 「물때」 · CH11 · 4컷 × 7s · 채도 0.5

1. 새벽마다 남자는 물때에 맞춰 갯바위를 걸었다. 손전등으로 바위 틈을 하나씩 비추면서. 낚시도 아니고 해녀 일도 아니었다. 뭘 찾는지는 몰랐다.  
   `Cut_T_E6_01` (앵커 D20 · **신규**)  
   _pre-dawn, 20-year-old DOYUN in a grey hooded coat walking alone along black rocks at low tide with a small flashlight, bending to shine it into a crevice between the rocks, grey autumn sea, seen from a distance up the shore_
2. 따라갔다. 꼬마는 오지 않았다. 남자는 폐그물 더미 앞에서 오래 서 있었다. 나는 거기 서고 싶지 않았다. 이유는 몰랐다.  
   `Cut_T_E6_02` (앵커 H19 · **신규**)  
   _a pile of tangled discarded fishing nets on black rocks below the cliff, 20-year-old DOYUN standing before it with his head down, 19-year-old HANEUL in a sky-blue hoodie stopped several meters behind him with one hand on the rock, unwilling to go closer, cold light_
3. 해경 초소 앞에서 남자가 명단을 봤다. 「아직요?」 「아직.」 남자는 고개를 숙였고, 초소 아저씨는 괜히 담배를 찾았다.  
   `Cut_T_E6_03` (앵커 D20 · **신규**)  
   _a small coast guard post with a glass door on the shore road, 20-year-old DOYUN in a grey hooded coat standing at the door looking at a notice board, a middle-aged coast guard in uniform inside patting his pockets, overcast_
4. 그 사람이 찾는 게 사람이라는 건 알았다. 누군지는 묻지 않았다. 물을 수 있는 사람이 없기도 했다.  
   `Cut_T_E6_04` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie standing alone on the shore road at dusk looking at the coast guard post's notice board from a distance, the papers on it unreadable, wind in her hair_

## CS6 「스무 살」 · BGM_M4 · 14컷 × 8.5s · 채도 0.42

1. **— 회상 · 여덟 해 전 —** 폭우 다음 날 그 애는 서울로 실려 갔다. 우리에게 주어진 시간은 5분이었다. 검은 차가 시동을 건 채 서 있었다.  
   `Cut_T_N6_01` (앵커 D12 · **유지** `Cut_S_N6_02`)  
2. **— 회상 —** 스무 살 네 생일에 송전탑 아래서 기다릴게. 내가 제일 무서워하는 곳이라서 고른 게 아니었다. 내가 처음으로 「우리 기지」라고 불러 준 곳이라서였다.  
   `Cut_T_N6_02` (앵커 D12 · **유지** `Cut_S_N6_03`)  
3. **— 회상 —** 우유 두 병. 하나는 오늘, 하나는 내일. 여덟 해가 지나도 알아볼 수 있게 두 병을 들고 있을게.  
   `Cut_T_N6_03` (앵커 D12 · **유지** `Cut_S_N6_04`)  
4. **— 회상 —** 그날 밤 달력에 처음으로 글씨를 썼다. 스무 살 생일, 송전탑 아래, 우유 두 개, 늦지 말 것.  
   `Cut_T_N6_04` (앵커 H12 · **유지** `Cut_S_N6_06`)  
5. **— 회상 · 서울 —** 그 애는 병실에서 편지를 썼다. 열세 살의 나에게, 열다섯의, 열일곱의, 열여덟의 나에게. 한 통도 부쳐지지 않았다. 아버지가 치료에 방해된다며 전부 서랍에 넣었다.  
   `Cut_T_N6_05` (앵커 D12 · **신규**)  
   _a Seoul hospital room at night, a thin teenage boy (DOYUN around 15, pale, short black hair, hospital gown) writing a letter at a bedside table by a small lamp, a stern man in a dark suit standing at the door holding a stack of sealed envelopes, city lights in the window_
6. 답장이 안 오는 건 그 애가 나를 잊어서라고 생각했다. 그래도 약속은 안 버렸다. 그게 내가 가진 전부여서.  
   `Cut_T_N6_06` (앵커 H12 · **신규**)  
   _a teenage HANEUL (about 16, longer hair, pale-blue t-shirt) standing at the rusty red mailbox by the gate opening it and finding it empty, evening, the calendar visible through the open door behind her_
7. 열아홉 되던 해 엄마 폐가 나빠져 도시 병원으로 갔다. 나는 엄마 해녀복을 입었다. 아빠 없는 집과 엄마 없는 바다를 혼자 지켜야 한다고 믿었다.  
   `Cut_T_N6_07` (앵커 H19 · **유지**)  
8. 출근하는 아침마다 벽의 우비가 떨어져 있었다. 올려놨다. 다음 날 또. 못은 멀쩡했다.  
   `Cut_T_N6_08` (앵커 H19 · **유지**)  
9. 열아홉 생일 아침, 우비가 세 번째로 떨어졌다. 한 번 안았다가 다시 걸었다. 어디선가 아이 목소리가 가지 말라고 했다. 나는 못 들었다.  
   `Cut_T_N6_09` (앵커 H19 · **유지**)  
10. 엄마 말을 뒤로하고 아빠 부표를 들고 바다로 갔다. 물속부터 기억이 심하게 흔들린다. 발목 근처의 폐그물, 수면 위를 달리는 주황색, 멀리 초소 사이렌. 조각뿐이다.  
    `Cut_T_N6_10` (앵커 H19 · **유지** `Cut_S_N6_10`)  
11. 그 뒤 내가 어떻게 됐는지는 안 떠오른다. 그냥 살아 돌아온 줄로만 알았다. 아니면, 그렇게 알고 싶었다.  
    `Cut_T_N6_11` (앵커 - · **유지** `Cut_S_N6_11`)  
12. 도윤은 내가 사라진 지 보름 만에 병원에서 뛰쳐나와 제주로 왔다. 아버지가 감춰 둔 편지 열아홉 통을 그제야 돌려받았다. 그걸 빈집 앞에 두고 갔다. 돌아온 내가 한꺼번에 읽고 화내기를 바라면서.  
    `Cut_T_N6_12` (앵커 D20 · **신규**)  
    _dawn at the gate of the empty Jeju stone house, 20-year-old DOYUN in a grey hooded coat setting a dented tin box full of white envelopes on the step, his hand lingering on the lid, the orange rain coat visible on its hook through the open door, first light_
13. 그날 저녁 그 애가 난간 없는 절벽 길로 걷기에 소매를 잡았다. 손이 옷감을 쥐지 못했다. 그 애 쪽이 흐렸다. 안개 속 사람처럼. 약을 먹는 사람, 우유를 두고 가는 사람, 국화를 사는 사람. 사라져 가는 쪽은 그 애였다. 그렇게 보였다.  
    `Cut_T_N6_13` (앵커 D20 · **신규**)  
    _dusk on a cliff path above the sea, 19-year-old HANEUL in a sky-blue hoodie grabbing at the sleeve of 20-year-old DOYUN's grey coat, her hand fully solid and closing on nothing, DOYUN himself drawn faded and slightly translucent like a figure in mist, his face turned toward the sea, cold wind, HANEUL's face frightened for him — the only cut where DOYUN is the one who looks unreal_
14. 바람 사이로 병원 메시지가 들렸다. 또 쓰러지면 다음은 장담 못 한다고. 나는 그 반만 들었다. 도윤의 시간이 얼마 안 남았다고 믿었다. 죽어 가는 사람이 그 애여야 나는 약속을 지킬 수 있었다. 그걸 바랐다는 게, 지금도 미안하다.  
    `Cut_T_N6_14` (앵커 D20 · **신규**)  
    _20-year-old DOYUN in a grey hooded coat sitting on the bus shelter bench at dusk with a phone to his ear and his other hand pressed to his chest, 19-year-old HANEUL in a sky-blue hoodie standing at the far end of the shelter with her back to him, looking at the sea, both faces in shadow_

## EV7 「첫눈」 · CH13 · 4컷 × 7s · 채도 0.42

1. 첫눈. 꼬마와 창가 성에에 얼굴을 그렸다. 둘을 그리다 손이 멈췄다. 나머지 하나가 누군지 몰라서.  
   `Cut_T_E7_01` (앵커 KID · **유지** `Cut_S_N3_05`)  
2. 눈길을 나란히 걸었다. 꼬마가 자꾸 뒤를 돌아봤다. 뭘 보냐고 물으니 아무것도 아니라고 했다. 두 번 물어도 아무것도 아니라고 했다.  
   `Cut_T_E7_02` (앵커 KID · **신규**)  
   _snowy coastal road, the tiny child in the orange rain coat walking beside 19-year-old HANEUL in a sky-blue hoodie and looking back over its shoulder at the road behind, HANEUL looking down at the child, snow falling, the road behind them out of focus_
3. 달력엔 32일. 겨울은 짧고 밤은 길었다. 손에 힘이 붙는 물건은 더 늘지 않았다.  
   `Cut_T_E7_03` (앵커 H19 · **신규**)  
   _the old wall calendar now almost entirely crossed out in pencil, the circled date close, 19-year-old HANEUL in a sky-blue hoodie sitting on the floor below it with a blanket around her shoulders, snow light through the window_
4. 꼬마가 잠든 사이 후드를 살짝 들춰 봤다. 왼쪽 눈가가 접혀 있었다. 웃으면서 자는 애였다.  
   `Cut_T_E7_04` (앵커 KID · **신규**)  
   _night, the tiny child asleep on the floor of the Jeju room in the orange rain coat, 19-year-old HANEUL's hand gently lifting the edge of the hood, the child's round sleeping face with a faint smile and the left eye creased, lamp light_

## EV8 「국화」 · CH14 · 4컷 × 7s · 채도 0.42

1. 남자의 방 창가에 흰 국화 한 다발이 있었다. 리본에 이름이 없었다. 누구 것인지 물을 사람이 없었다.  
   `Cut_T_E8_01` (앵커 D20 · **유지** `Cut_S_N7_10`)  
2. 꼬마는 그를 보면 늘 한 걸음 물러났다. 「저 사람은 아직 몰라.」 뭘 모른다는 건지 나도 몰랐다.  
   `Cut_T_E8_02` (앵커 KID · **유지** `Cut_S_N7_11`)  
3. 국화는 아빠 것일 수 있었다. 기일이 생일 전날이니까. 그런데 두 다발이었다. 하나는 누구 것인지, 그날 밤엔 생각하지 않기로 했다.  
   `Cut_T_E8_03` (앵커 H19 · **신규**)  
   _close-up of two bundles of white chrysanthemums wrapped in plain paper lying side by side on a windowsill, one ribbon with a small pencilled name that cannot be read, dark night glass above them, 19-year-old HANEUL's sleeve at the edge of frame_
4. 이름 없는 꽃은 그 사람을 위한 꽃이라고 정했다. 그러면 내가 살아 있는 쪽이 되니까.  
   `Cut_T_E8_04` (앵커 H19 · **신규**)  
   _night, 19-year-old HANEUL in a sky-blue hoodie sitting on the stone wall outside DOYUN's lit window, her back to it, hugging herself against the cold, the steel tower's red lamp far off_

## EV9 「스무 번째」 · CH15 · 4컷 × 7s · 채도 0.32

1. 편지를 하나씩 열었다. 열세 살의 하늘에게. 병실 창에서 제주랑 닮은 송전탑을 찾았어. 약이 써서 우유를 마셔. 네 생일엔 초 대신 우유 두 병을 놨어. 나 아직 살아 있어. 너도 그러길.  
   `Cut_T_E9_01` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie sitting on the floor of the Jeju room surrounded by opened white envelopes and unfolded letters, reading one close to her face, the tin box open beside her, winter afternoon light_
2. 편지마다 그 애는 자기가 아직 살아 있다고 적었다. 그 문장이 과거의 도윤이 미래의 자기에게 남긴 유서처럼 읽혔다. 나는 그렇게 읽고 싶었던 것이다.  
   `Cut_T_E9_02` (앵커 H19 · **신규**)  
   _close-up of a handwritten letter in neat boyish Korean handwriting (illegible in the image), one line underlined by the reader's thumb, 19-year-old HANEUL's other hand pressed flat on the floor, the paper slightly yellowed_
3. 우체국 앞에서 도윤이 혼잣말을 했다. 이번에도 답장이 없으면 만나서 왜 안 왔냐고 묻겠다고. 여덟 해를 버틴 건 그 밤 내가 「셋이 같이 간다」고 말해 줘서였다고.  
   `Cut_T_E9_03` (앵커 D20 · **유지** `Cut_S_N7_08`)  
4. 그 애는 편지를 우체통에 넣지 않았다. 빈집으로 돌아와 양철 상자의 마지막 자리에 직접 꽂았다. 스무 통. 흐려져 있던 이름이 잠깐 선명해졌다. 하늘. 그 애가 떠난 뒤 나는 마지막 편지를 열었다.  
   `Cut_T_E9_04` (앵커 D20 · **신규**)  
   _dusk at the gate step of the Jeju house, 20-year-old DOYUN in a grey hooded coat kneeling to slide a twentieth envelope into the last empty slot of the tin box, the name on the top envelope momentarily sharp and legible, 19-year-old HANEUL standing in the doorway behind him, he has not noticed her_

## CS7 「둘 중 하나」 · BGM_M1 · 13컷 × 8.5s · 채도 0.32

1. 생일까지 이레. 기억은 더 돌아오지 않았다. 대신 도윤이 점점 가까운 데 나타났다. 이름을 불러도 방향을 못 찾았다.  
   `Cut_T_N7_01` (앵커 D20 · **유지**)  
2. 그런데 내가 욕을 하거나 울음을 삼킬 때마다 그 애는 찬바람을 맞은 것처럼 잠깐 멈췄다. 보이지도 들리지도 않는데 무언가 하나는 끊어지지 않고 남아 있었다.  
   `Cut_T_N7_02` (앵커 D20 · **신규**)  
   _winter coastal road, 20-year-old DOYUN in a grey hooded coat stopped mid-step with his hand half raised to his cheek as if touched by wind, 19-year-old HANEUL in a sky-blue hoodie a step beside him with her face turned away wiping her eyes, bare branches_
3. 새벽마다 약을 한 움큼. 병원 팔찌는 끊긴 적이 없었다. 여전히 아픈 사람이었다. 그리고 매일 우유 두 병.  
   `Cut_T_N7_03` (앵커 D20 · **유지**)  
4. 도윤이 초소 앞에서 꺼내 보던 코팅 카드가 바람에 날아갔다. 반사적으로 손을 뻗었다. 잡혔다. 처음으로 지금의 그 애 것이 손에 들어왔다.  
   `Cut_T_N7_04` (앵커 H19 · **신규**)  
   _windy winter shore road, a small laminated card spinning in the air, 19-year-old HANEUL in a sky-blue hoodie lunging with her hand outstretched, 20-year-old DOYUN a few steps ahead turning at the sound, grey sky_
5. 카드에는 열아홉 살 여자애 사진과 이름. 고하늘. 실종 1년. 내 이름과 그 애의 기다림이 같이 새겨진 물건이라서 경계를 넘어온 거였다.  
   `Cut_T_N7_05` (앵커 H19 · **신규**)  
   _close-up of 19-year-old HANEUL's open palm holding a small laminated search card showing a passport-style photo of a 19-year-old girl and a printed name and the words for 'missing 1 year' (illegible), sky-blue hoodie sleeve, winter light, no bracelet_
6. 이름은 찾았다. 죽었다는 건 받아들이지 않았다. 카드에는 사망자가 아니라 실종자라고 적혀 있었다. 어딘가에 내 몸이 살아 있고 지금의 나는 기억만 빠져나온 것일 수도 있었다.  
   `Cut_T_N7_06` (앵커 H19 · **유지** `Cut_S_N8_04`)  
7. 꼬마는 그 믿음을 부정하지 않았다. 진실은 남이 말해 준다고 받아들일 수 있는 게 아니니까. 대신 내 옆에 앉아 있었다. 그 애가 늘 그랬던 것처럼.  
   `Cut_T_N7_07` (앵커 KID · **신규**)  
   _dusk under the transmission tower in winter, 19-year-old HANEUL in a sky-blue hoodie sitting with the laminated card in her lap staring at the sea, the tiny child in the orange rain coat sitting close beside her with its shoulder against her arm, saying nothing_
8. 카드를 도윤의 손바닥에 올려놓았다. 허공에서 떨어진 카드가 다시 손에 들어오자 그 애는 처음으로 내가 가까이 있다는 걸 확신했다.  
   `Cut_T_N7_08` (앵커 D20 · **신규**)  
   _close-up of 20-year-old DOYUN's open pale palm with a laminated search card lying on it, his fingers slowly closing, 19-year-old HANEUL's hand withdrawing at the edge of frame almost transparent, cold light_
9. 「생일에 꼭 와.」 그 애가 허공에 대고 말했다. 「늦어도 돼. 살아서만 와.」 대답을 못 했다. 그 애가 내 이름을 처음으로 소리 내 불렀다. 하늘아.  
   `Cut_T_N7_09` (앵커 D20 · **신규**)  
   _20-year-old DOYUN in a grey hooded coat standing on the shore road speaking to empty air with the card pressed to his chest, eyes wet, 19-year-old HANEUL in a sky-blue hoodie right in front of him with her mouth open and no sound, both faces close, winter dusk_
10. 비에 젖은 상점 유리 앞에 나란히 섰다. 그 애는 희미하게 비쳤다. 내가 선 자리는 바다의 반사광에 가려 있었다. 둘 중 하나는 여기 없다.  
    `Cut_T_N7_10` (앵커 D20 · **유지** `Cut_S_N7_12`)  
11. 꼬마에게 물었다. 나랑 도윤 중에 누가 돌아갈 사람이냐고. 꼬마는 되물었다. 「어느 쪽이라고 믿고 싶어?」  
    `Cut_T_N7_11` (앵커 KID · **신규**)  
    _rain-wet shop window at night reflecting street light, the tiny child in the orange rain coat standing beside 19-year-old HANEUL in a sky-blue hoodie, the child's reflection clear in the glass, hers a blur of sea light, the child looking up at her_
12. 도윤이 죽어 가는 사람이어야 한다고 말했다. 소리 내서. 꼬마는 아무 말도 안 했다. 내가 살려고 고른 대답이라는 걸 아는 얼굴이었다.  
    `Cut_T_N7_12` (앵커 KID · **신규**)  
    _close-up of the child's face under the orange hood looking down, not at HANEUL, small mouth pressed shut, rain drops on the hood, street light behind_
13. 그날 밤 달력에 내 이름을 썼다. 약속 옆에. 하늘. 손은 조금 흐릿했다. 연필은 잡혔다.  
    `Cut_T_N7_13` (앵커 H19 · **유지** `Cut_S_N8_05`)  

## EV10 「전날 밤」 · CH19 · 4컷 × 7s · 채도 0.25

1. 생일 전날은 아빠 기일이다. 도윤과 나란히 4.2킬로를 걸었다. 국화 두 다발. 그 애는 몰랐다. 나도 절반만 알았다.  
   `Cut_T_E10_01` (앵커 D20 · **유지** `Cut_S_N8_01`)  
2. 남자의 창가 시간표에 날짜가 적혀 있었다. 모레 새벽 첫차. 병원이 있는 도시로. 이틀. 손가락으로 세어 봤다. 어느 쪽이든 짧았다.  
   `Cut_T_E10_02` (앵커 D20 · **신규**)  
   _night, the bus timetable taped to the wall of the small room now with a date written beside the circled first departure, a packed duffel bag on the floor, 20-year-old DOYUN asleep sitting up against the wall with two milk bottles beside him, lamp on_
3. 꼬마가 라디오를 내 무릎에 올려놨다. 「내일은 들어도 돼.」 「뭘.」 「내일.」  
   `Cut_T_E10_03` (앵커 KID · **신규**)  
   _night in the Jeju room, the tiny child in the orange rain coat placing the old radio on the lap of 19-year-old HANEUL in a sky-blue hoodie, the dial dark, the child's face serious under the hood, the calendar on the wall with one uncrossed day_
4. 잠이 안 왔다. 우비를 벽에서 내려 한 번 안았다. 이번엔 걸지 않았다. 옆에 두고 잤다.  
   `Cut_T_E10_04` (앵커 H19 · **신규**)  
   _19-year-old HANEUL lying on the floor of the dim Jeju room in her sky-blue hoodie with the old orange rain coat folded beside her pillow, one hand resting on it, eyes open, the empty hook on the wall, the tower's red lamp through the window_

## CS8 「주파수」 · BGM_M5 · 16컷 × 8.5s · 채도 0.25

1. 스무 번째 생일의 노을이 탑 아래로 내려앉았다. 도윤은 우유 두 병을 들고 약속 장소에 서 있었다. 나는 늦지 않았다. 그 애에겐 보이지 않았다.  
   `Cut_T_N8_01` (앵커 D20 · **유지** `Cut_S_N8_12`)  
2. 5분 지나면 화낼 거야. 10분 지나면 더 오래 기다릴 거야. 혼잣말하는 그 애 앞으로 엄마가 흰 국화를 안고 올라왔다.  
   `Cut_T_N8_02` (앵커 MOM+D20 · **신규**)  
   _sunset under the steel transmission tower, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) in dark navy everyday clothes walking up the grassy slope holding a bundle of white chrysanthemums, 20-year-old young man DOYUN (tall, grey hooded coat) standing ahead with two milk bottles, turning toward her_
3. 엄마가 말했다. 오늘 새벽에 나를 찾았다고. 주황 우비 꼬마가 해경 초소 문을 두드리며 송전탑 남쪽 폐그물 아래를 봐 달라고 울었다고. 거기 있었다고. 1년 동안.  
   `Cut_T_N8_03` (앵커 MOM · **신규**)  
   _sunset under the transmission tower, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) in dark navy everyday clothes standing before 20-year-old young man DOYUN holding white chrysanthemums, speaking with her eyes down, 19-year-old HANEUL in a sky-blue hoodie standing between them unseen with her hand over her mouth_
4. 찾았으면 데려와야 하는 거 아니냐고 그 애가 되물었다. 살아 있느냐는 물음에 엄마가 대답하지 못했다. 그 애는 우유 두 병을 끌어안은 채 무너졌다.  
   `Cut_T_N8_04` (앵커 D20 · **신규**)  
   _20-year-old DOYUN on his knees in the grass under the tower at sunset hugging two glass milk bottles to his chest, head down, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly)'s hand on his shoulder, chrysanthemums fallen on the grass, the three stacked stones beside them_
5. 「하나는 오늘, 하나는 내일이라며.」 그 애가 병을 보며 말했다. 「내일 건 누가 마셔.」 엄마가 그 애 등을 쓸었다. 나를 안는 것처럼.  
   `Cut_T_N8_05` (앵커 D20 · **신규**)  
   _close-up, 20-year-old DOYUN's wet face looking down at the two milk bottles in his hands, MOM's slim hand rubbing his back, 19-year-old HANEUL's translucent shoulder at the edge of frame, sunset_
6. 엄마가 돌 세 개 옆에 국화를 놓았다. 「하늘아. 어멍 왔져. 미역국은 못 끓여 줬져.」 나는 엄마를 안으려고 했다. 두 팔이 엄마를 지나갔다. 엄마는 바람이 분다고 옷깃을 여몄다.  
   `Cut_T_N8_06` (앵커 MOM · **신규**)  
   _sunset, MOM (slim Korean woman in her early forties, neat oval face, healthy sun-kissed skin, black hair tied low, no grey hair, no deep wrinkles, NOT overweight, NOT elderly) in dark navy everyday clothes kneeling to lay white chrysanthemums beside three stacked stones at the foot of the tower and pulling her collar closed as if against wind, 19-year-old HANEUL in a sky-blue hoodie kneeling behind her with both arms around her mother's shoulders passing through like mist, HANEUL's face pressed to where her mother's back would be_
7. 나 왔어. 약속 지켰어. 그 애 앞에 무릎을 꿇고 외쳤다. 손은 그 애 얼굴을 통과했다. 목소리는 닿지 않았다. 여기까지 왔는데.  
   `Cut_T_N8_07` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie kneeling face to face with the collapsed DOYUN, her hands passing through his cheeks like mist, her face crying and shouting, his eyes closed not seeing her, sunset_
8. 마지막 기억이 돌아왔다. 발목을 조이던 폐그물. 허리에 없던 칼. 보석처럼 빛나던 푸른 조개껍데기. 수면 위에서 도움을 청하던 주황 우비. 마지막 기포까지 나는 내년의 약속을 생각했다.  
   `Cut_T_N8_08` (앵커 - · **신규**)  
   _underwater, a young woman in a black haenyeo wetsuit tangled at the ankle in a discarded net, her hand reaching toward a small blue shell glinting on the rock, far above on the bright surface the tiny blurred orange shape of a child, rising bubbles slowing, deep blue_
9. 나는 낯선 방에서 살아 돌아온 게 아니었다. 약속을 못 지킨 마음만 1년 동안 이 마을에 남아 있었던 거다.  
   `Cut_T_N8_09` (앵커 H19 · **유지** `Cut_S_N8_06`)  
10. 꼬마가 돌 세 개 앞에 낡은 방수 통을 내려놨다. 안에 아빠가 못 준 보석 머리띠. 엄마가 장롱에 숨긴 걸 꿈에서 본 열쇠로 찾아왔다고 했다. 그리고 아빠 목소리로. 「한 마리만 더 잡으민 뒈어.」  
    `Cut_T_N8_10` (앵커 KID · **신규**)  
    _dusk under the tower, the tiny child in the orange rain coat kneeling before the three stacked stones opening a small yellow waterproof case with a red heart on the lid, inside a thin silver headband with three blue gems catching the last light, 19-year-old HANEUL's tear-streaked face above_
11. 왼쪽 눈이 웃을 때 접히는 모양. 삐뚤어진 하트. 주황 우비. 나를 「바다」라고 부른 이유. 한꺼번에 알아봤다. 아빠였다. 바다에서 죽은 아빠가 어떤 아이의 삶에 남긴 마지막 기억.  
    `Cut_T_N8_11` (앵커 KID · **신규**)  
    _close-up of the child's face with the hood pushed back for the first time, round cheeks, left eye creasing in a small smile exactly like DAD's, dusk light, 19-year-old HANEUL's hand on the child's cheek_
12. 머리띠를 갖고 싶어 해서 아빠가 죽었다고 고백했다. 아홉 해 동안 아무한테도 못 한 말이었다. 「선물 갖고 싶다고 한 건 죄 아니라. 바다 나간 건 아방이 고른 거주.」 소매를 놓은 게 아니라 놓게 한 거라고. 어린 딸이 어른의 선택을 평생 대신 지지 않아도 된다고.  
    `Cut_T_N8_12` (앵커 KID · **신규**)  
    _the tiny child in the orange rain coat holding both hands of 19-year-old HANEUL in a sky-blue hoodie, looking up at her, speaking, her face breaking, the sea turning gold behind them_
13. 꼬마를 안았다. 이번엔 손이 통과하지 않았다. 「늦게 와서 미안.」 아빠가 말했다. 「기다려 줘서 고맙고.」 열한 살 이후로 처음 듣는 말이었다. 죽은 사람이 산 사람에게 닿으려면 세 가지가 같은 자리에 모여야 한대. 불러 줄 진짜 이름. 둘이 같이 기억하는 물건. 끝까지 지킨 약속. 그러면 91.9의 노래 한 곡 동안만.  
    `Cut_T_N8_13` (앵커 KID · **신규**)  
    _19-year-old HANEUL in a sky-blue hoodie hugging the tiny child in the orange rain coat tightly at the foot of the tower, the child's small arms around her neck, both solid, the three stones and the open yellow case in the grass, sunset_
14. 멀리서 여자가 불렀다. 「민재야.」 꼬마의 눈에서 아빠가 빠르게 빠져나갔다. 울고 있는 나를 못 알아보고, 엄마가 부른다며 평범한 아이의 얼굴로 유채밭을 달려갔다. 안 잡았다. 아빠가 또 나 때문에 삶을 포기하게 할 순 없었다.  
    `Cut_T_N8_14` (앵커 KID · **신규**)  
    _wide shot, the tiny child in the orange rain coat running away down the slope through a yellow rapeseed field toward a young woman (MIN_MOM, 30s, plain clothes) waving at the road, 19-year-old HANEUL standing at the top of the slope with her arm half raised and then lowered, sunset_
15. 잘 가, 아빠. 이번엔 내가 소매를 놓았다.  
    `Cut_T_N8_15` (앵커 H19 · **신규**)  
    _19-year-old HANEUL alone at the foot of the tower at the last light, holding the silver headband with three blue gems in both hands against her chest, eyes closed, the open yellow case and three stones at her feet, the rapeseed field below empty, a small torn orange thread caught on the grass_
16. 머리띠를 쓰고 라디오를 안았다. 도윤이 떠나는 새벽까지 하루. 마지막 4.2킬로. 이번엔 기억을 찾으려고 달리는 게 아니다. 닿으려고 달린다.  
    `Cut_T_N8_16` (앵커 H19 · **신규**)  
    _19-year-old HANEUL wearing the silver headband with three blue gems, the old radio strapped to her back, pushing off on her skateboard along the coastal road at first light toward the bus stop, hair flying, determined, the tower behind her_
## END_A 「엇갈린 정류장」 · BGM_M3 · 9컷 × 8.5s · 채도 0.35
분기: `이름` 없음 또는 `91.9 라디오` 없음.

1. 서울행 버스가 낡은 정류장 앞에 섰다. 도윤은 계단에 발을 올리기 전에 마지막으로 탑이 있는 언덕을 돌아봤다.  
   `Cut_T_EA_01` (앵커 D20 · **신규**)  
   _early morning, a country bus stopped at the stone bus shelter with its door open, 20-year-old DOYUN in a grey hooded coat with a duffel bag standing at the step looking back over his shoulder at the hill with the transmission tower, pale sky_
2. 나는 그 애 바로 옆에 서 있었다. 가지 말라고 하는 대신 이제 그만 기다리라고 말했다. 뺨을 만지려던 손끝은 아무것도 못 잡았다.  
   `Cut_T_EA_02` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie standing beside DOYUN at the bus step reaching for his cheek, her fingertips fading to nothing where they meet his skin, his eyes on the hill not on her, morning_
3. 기사 아저씨가 혼자냐고 물었다. 「오늘부터는요.」 그 애가 대답했다.  
   `Cut_T_EA_03` (앵커 D20 · **신규**)  
   _inside the bus doorway, the driver glancing back from his seat, 20-year-old DOYUN on the top step answering with a small nod, the empty bus seats behind_
4. 유리창에 손바닥을 댄 그 애는 여덟 해 전 검은 차 안의 아이와 같았다. 나도 유리 반대편에 손을 댔다. 두 손 사이엔 차갑고 두꺼운 유리가 있었다.  
   `Cut_T_EA_04` (앵커 D20 · **신규**)  
   _bus window from outside, 20-year-old DOYUN's palm pressed against the glass from inside, 19-year-old HANEUL's palm on the same spot from outside, her hand slightly translucent, their faces not aligned, reflection of the sea on the glass_
5. 버스가 떠났다. 정류장에 우유 한 병이 남았다. 하나는 오늘. 내일 것은 이제 필요 없다고 중얼거렸다.  
   `Cut_T_EA_05` (앵커 H19 · **신규**)  
   _empty stone bus shelter after the bus has gone, one glass milk bottle standing on the bench, 19-year-old HANEUL in a sky-blue hoodie sitting at the other end of the bench looking at it, dust settling on the road_
6. 라디오를 켰다. 끝내 주파수는 맞지 않았다. 잡음만 흘렀다. 한참 들었다.  
   `Cut_T_EA_06` (앵커 H19 · **신규**)  
   _close-up of the old radio on the bench with its dial lit between stations, static implied by a soft glow, 19-year-old HANEUL's hand on the dial not turning it, morning light_
7. 도윤은 현실의 삶으로 돌아갔다. 병원에 가고, 약을 먹고, 가끔 창밖의 송전탑을 찾았다. 그런 걸 나는 알 수 없었다.  
   `Cut_T_EA_07` (앵커 D20 · **신규**)  
   _a city hospital room window at dusk, 20-year-old DOYUN in a hospital gown sitting on the bed looking out at a distant steel transmission tower among apartment blocks, one milk bottle on the sill, muted colors_
8. 나는 탑 아래 남았다. 돌 세 개가 무너질 때마다 다시 쌓았다. 왜 기다리는지는 조금씩 잊어 갔다.  
   `Cut_T_EA_08` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie kneeling at the foot of the transmission tower restacking three stones, her figure faint against tall grass, the season unclear, grey light, sea below_
9. 약속은 지켰다. 목소리는 끝내 닿지 않았다.  
   `Cut_T_EA_09` (앵커 - · **신규**)  
   _wide shot of the transmission tower on the Jeju cliff at dusk with no one under it, three small stones at its foot, the sea flat and grey, a single red lamp lit_

## END_B 「우유 두 병」 · BGM_M6 · 9컷 × 8.5s · 채도 0.5
분기: `이름`·`라디오`는 있음, `보석 머리띠`·`편지 스무 통`·`하트 흔적`·`돌 세 개` 중 하나 이상 없음.

1. 버스 문이 열리는 순간 나는 91.9에 맞춘 라디오를 켰다. 모습은 안 보였고 손도 안 닿았다. 잡음 사이로 내 목소리가 그 애에게 갔다. 「도윤아.」  
   `Cut_T_EB_01` (앵커 H19 · **신규**)  
   _early morning at the stone bus shelter, the bus door open, 19-year-old HANEUL in a sky-blue hoodie holding the old radio up with both hands, its dial glowing 91.9, 20-year-old DOYUN on the bus step frozen with his back half turned_
2. 그 애가 계단에서 내려와 정류장 한가운데 섰다. 어디 있느냐고 묻는 대신 정말 너냐고 물었다. 「반은 진짜고 반은 거짓말.」 그 애가 울면서 그런 말은 하지 말라고 했다.  
   `Cut_T_EB_02` (앵커 D20 · **신규**)  
   _20-year-old DOYUN standing in the middle of the empty shelter turning slowly with tears, speaking to the air, the bus waiting with its door open behind him, 19-year-old HANEUL a step away half transparent holding the radio_
3. 서로를 볼 수 없는 채로 여덟 해 동안 못 한 말을 짧게 나눴다. 늦어서 미안해. 안 늦었어. 어디 있었어. 길을 잃었어.  
   `Cut_T_EB_03` (앵커 D20 · **신규**)  
   _close two-shot in the shelter, 20-year-old DOYUN's face turned slightly the wrong way, 19-year-old HANEUL's face right beside his almost touching, her eyes on him, his on nothing, morning light through the shelter_
4. 나는 이미 죽었다는 것과 약속 때문에 1년을 더 머물렀다는 걸 고백했다. 그 애는 다시 기다리겠다고 했다. 이번엔 기다리지 말라고 부탁했다.  
   `Cut_T_EB_04` (앵커 H19 · **신규**)  
   _19-year-old HANEUL in a sky-blue hoodie speaking into the radio's microphone grille with her eyes closed, the dial glowing, DOYUN's grey sleeve at the edge of frame_
5. 내가 못 산 내일까지 네가 대신 살아. 병원에 가고, 사람을 만나고, 가끔은 나를 잊어. 그 애는 대답 대신 우유 두 병을 벤치에 올려놓았다.  
   `Cut_T_EB_05` (앵커 D20 · **유지** `Cut_S_EA_08`)  
6. 방송의 노래가 끝났다. 내 목소리가 잡음 속으로 멀어졌다. 그 애 손이 허공을 한 번 더듬었다. 두 손 사이에 작은 하트 하나가 켜졌다가 꺼졌다.  
   `Cut_T_EB_06` (앵커 D20 · **유지** `Cut_S_EB_03`)  
7. 「가.」 「응.」 「살아.」 「응.」 마지막 대답은 라디오가 아니라 바람이 했을지도 모른다.  
   `Cut_T_EB_07` (앵커 D20 · **신규**)  
   _20-year-old DOYUN stepping onto the bus with his hand on the door rail, looking back one last time at the empty shelter bench with two milk bottles on it, the radio gone, morning_
8. 다음 날 아침, 마을 가게 종이 두 번 울렸다. 도윤은 우유 두 병을 샀다. 탑 아래엔 한 병만 두었다. 나머지 한 병은 자기가 마셨다.  
   `Cut_T_EB_08` (앵커 D20 · **유지** `Cut_S_EB_08`)  
9. 나는 모습을 못 보여 줬다. 작별은 전했다. 우리 주파수는 목소리까지만 닿았다.  
   `Cut_T_EB_09` (앵커 - · **신규**)  
   _sunrise under the transmission tower, one glass milk bottle on the rock beside three stacked stones, the empty coastal road below, warm light, no people_

## END_TRUE 「맞닿은 주파수 91.9」 · BGM_M1 · 11컷 × 8.5s · 채도 0.25 → 1.0
분기: 단서 여섯 개 전부.

1. 버스 문이 열리고 그 애가 첫 계단에 발을 올리는 순간, 파도와 엔진 소리가 전부 사라졌다. 나는 머리띠를 쓰고 다이얼을 정확히 91.9에 맞췄다. 「도윤아.」  
   `Cut_T_ET_01` (앵커 H19 · **신규**)  
   _early morning at the stone bus shelter in near-monochrome, 19-year-old HANEUL wearing the silver headband with three blue gems holding the old radio, the only color in the frame the glowing dial and the blue gems, DOYUN on the bus step with his back turned_
2. 그 애가 돌아봤다. 빛바랜 마을에 색이 돌아오기 시작했다. 내가 선 자리에서 유채의 노란색이 번지고, 바다의 파랑, 주황 우비, 삐뚤어진 하트의 빨강이 차례로 살아났다.  
   `Cut_T_ET_02` (앵커 H19 · **신규**)  
   _the same shelter, color spreading outward from 19-year-old HANEUL's feet across the grey scene: yellow rapeseed, blue sea, an orange coat on a fence, a red heart on a buoy, 20-year-old DOYUN turned fully around with his mouth open, a milk bottle slipping from his hand_
3. 열아홉 살 그대로의 내가 정류장 앞에 서 있었다. 그 애는 내 이름을 몇 번이나 확인하고 나서 물었다. 왜 이렇게 늦었어. 길을 잃었어. 4.2킬로밖에 안 되는 길을. 이름을 잃어버리면 짧은 길도 아주 멀어져.  
   `Cut_T_ET_03` (앵커 D20 · **신규**)  
   _full color, 19-year-old HANEUL in a sky-blue hoodie and the gem headband standing solid in front of the shelter, 20-year-old DOYUN a step away staring at her with tears and a broken laugh, a milk bottle rolling on the road, morning gold_
4. 그 애가 손을 내밀었다. 통과할지도 모른다고 했다. 「그럼 다시 잡을게.」 몇 번이고 통과하면 몇 번이고 내밀겠다는 말이었다. 손을 포갰다. 두 손 사이에 아빠가 그리던 삐뚤어진 하트 모양의 빛이 켜졌다.  
   `Cut_T_ET_04` (앵커 D20 · **유지** `Cut_S_EB_03`)  
5. 허락된 시간은 91.9에서 흐르는 노래 한 곡. 나는 죽었다는 걸 숨기지 않았고 그 애는 붙잡을 수 없는 걸 붙잡겠다고 고집하지 않았다. 대신 물었다. 「앞으로도 너 생각해도 돼?」  
   `Cut_T_ET_05` (앵커 D20 · **유지** `Cut_S_EA_05`)  
6. 내 기억 때문에 네 내일을 버리지만 마. 그 애가 한참 있다가 말했다. 「살아 볼게. 아주 오래.」 그 약속이라면 믿기로 했다. 여덟 해 전 5분보다 짧은 시간에, 우리는 제일 긴 약속을 했다.  
   `Cut_T_ET_06` (앵커 D20 · **신규**)  
   _full color, close two-shot at the shelter, 19-year-old HANEUL in the gem headband and 20-year-old DOYUN forehead to forehead with eyes closed, their clasped hands between them glowing faintly, the radio on the bench, morning gold_
7. 노래가 끝나며 내 모습이 빛 속으로 흐려졌다. 그 애 손엔 더 이상 내 손이 없었지만 온기는 남았다. 길 위에 우유 두 병. 하나는 함께한 오늘, 하나는 그 애가 살아갈 내일.  
   `Cut_T_ET_07` (앵커 D20 · **신규**)  
   _20-year-old DOYUN standing alone on the coastal road at full morning holding his open hand up to the light, two glass milk bottles on the road at his feet, the bus gone, the tower on the hill, color fully returned_
8. 이듬해 봄. 도윤은 제주 지역 방송국에서 91.9 심야 프로그램을 만들고 있었다. 잃어버린 사람의 목소리를 기다리는 사연을 읽고, 남은 사람이 살아도 되느냐는 편지에 답했다. 먼저 간 사람이 제일 듣고 싶은 소식은 남은 사람이 잘 산다는 소식일 거라고.  
   `Cut_T_ET_08` (앵커 D20 · **유지** `Cut_S_ET_03`)  
9. 아무도 말하지 않았는데 레벨 미터가 잠깐 움직였다. 잡음 속에서 아주 짧게. 「안 늦었네.」 그 애가 돌아봤다. 부스 유리 너머, 정류장 앞에 서 있던 그날의 내가 있었다. 「응. 오늘은 안 늦었어.」 그 애가 마이크를 내려놓고 일어섰다.  
   `Cut_T_ET_09` (앵커 D20 · **신규**)  
   _small radio studio at night, 20-year-old DOYUN standing up from the microphone with wet eyes and a smile, turned toward the glass of the booth, beyond the glass 19-year-old HANEUL in a sky-blue hoodie and the gem headband standing in the dark corridor lit only by the ON AIR sign, the audio level meter still lit, a glass milk bottle beside the console_
10. 그날 방송은 노래가 끝난 뒤로 이어지지 않았다. 새벽의 송전탑 아래, 돌 세 개 옆에 「내일」이라고 적힌 네 번째 돌. 그 옆에 낡은 라디오와 우유 두 병. 둘 다 비어 있었다. 유채꽃 한 송이가 계절보다 하루 먼저 피었다.  
    `Cut_T_ET_10` (앵커 - · **신규**)  
    _dawn at the foot of the transmission tower, four small stones stacked on the grass with the top one marked with a single pencilled word, the old radio and two empty glass milk bottles beside them, one yellow rapeseed flower blooming between the stones before the rest of the field, first sunlight, no people_
11. 사진 한 장. 정류장 앞에서 손을 잡고 웃는 열아홉의 나와 스무 살의 그 애. 누가 찍었는지, 언제인지는 적혀 있지 않았다. 우리의 주파수. 91.9.  
    `Cut_T_ET_11` (앵커 - · **신규**)  
    _a single instant photograph lying on the grass against the fourth stone in the morning light: in the photo 19-year-old HANEUL in a sky-blue hoodie with the gem headband and 20-year-old DOYUN in a grey hooded coat standing hand in hand in front of the stone bus shelter, both smiling at the camera, both in full color and equally solid, the photo slightly bent at one corner, nothing written on it_

---

## 적용 메모 (다음 회차)

1. **그림 ID 규칙**: v4 컷은 `Cut_T_*`(T=v4)로 새 이름을 받는다 — v3 `Cut_S_*`와 번호가 어긋나는 컷이 많아 같은 접두어로는 덮어쓰기 충돌이 난다. `**유지** <옛 id>`(또는 옛 id 표기 없는 `**유지**`는 같은 번호의 v3 파일)는 그 `Cut_S_*` 파일을 `Cut_T_*` 이름으로 복사하면 되고, `**신규**`만 Kling 웹으로 생성한다. 신규 컷 수: OPEN 8 · CS1 3 · EV1 4 · CS2 6 · EV2 4 · CS3 4 · EV3 3 · CS4 6 · EV4 4 · EV5 1 · CS5 8 · EV6 4 · CS6 5 · EV7 3 · EV8 2 · EV9 3 · CS7 8 · EV10 3 · CS8 14 · END_A 9 · END_B 6 · END_TRUE 8 = **116장**(9:16, 720×1280). Kling 웹 크레딧 6 남음 → 크레딧 충전 또는 회차 분할. **생성 순서: ① `ref77/MOM.png` 재생성(새 바이블 문장) → ② 엄마 컷 11장 → ③ 나머지.**
2. **대본 파일**: `Tools/Story/script/`에 `PRO`(=OPEN), `CH01_Open`(CS1) … 위 배치표대로. EV는 `CHnn_Open`에 4컷짜리로 들어가며, 컷씬 없는 CH16·CH18은 파일을 비운다(`ChapterVN.Play`가 건너뜀). 컷 길이: 메인 8.5s, EV 7s, OPEN 10.6s.
3. **단서 시스템**: `SaveData`에 6비트 플래그(`clueMask`). 컷씬 종료 콜백에서 세팅하되 `편지 스무 통`은 EV9까지 편지 읽기(육성 행동 「편지 읽기」 × 20)를 끝냈을 때, `돌 세 개`는 CS4 이후 러닝 결과 화면에서 「돌 다시 쌓기」 탭 1회로. 엔딩 분기는 `CinemaSelect`/`SceneFlowController`에서 `clueMask`로.
4. **CinemaSelect 카드**: 엔딩 A·B 제목·설명 교체(A=엇갈린 정류장 / B=우유 두 병 / TRUE=맞닿은 주파수). `OpenAllEndings` 임시 해금은 유지.
5. **채도**: CS8까지 0.25로 내려갔다가 END_TRUE 2컷에서 1.0으로 복귀하는 연출은 `CinematicPlayer` 채도 셰이더에 컷별 키프레임(현재는 컷씬 단위) 필요.
6. **음악 큐**: BGM은 v3와 같게 두었다. END_TRUE 5컷 「노래 한 곡」은 M1을 그대로 쓰고, 8컷 방송국은 M1 라디오 필터(로우패스) 버전 하나 추가하면 좋다.
7. **자막 길이**: 8.5초 컷에 자막이 세 문장을 넘는 컷(CS3-7, CS6-13·14, CS8-12·13, END_TRUE-3·9)은 두 페이지로 나누거나 컷을 10초로. 적용 때 결정.

---

## 씬별 그림 지시 (앵커·샷·핵심)

샷: W 와이드 · M 미디엄 · C 클로즈 · 2S 투샷 · OTS 어깨너머. 「유지(옛 id)」는 v3 그림을 `Cut_T_*`로 복사. 앵커는 위 「인물 고정 규칙」의 얼굴 문장 + `ref77/<키>.png`(강도 0.35~0.5). 두 인물 이상이면 프롬프트 첫 문장에 키 차이를 적는다.


### OPEN 「너와 나의 주파수」
H11(=H12 앵커, 11세 표기)·DAD·MOM·D12·H19 · 채도 1.0 · 봄 햇빛 → 폭풍 밤 → 맑은 아침 → 1년 뒤 봄

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H11 | M | 백화점 유리 진열장 앞, 낡은 샌들·바랜 티셔츠. 얼굴은 진열장 유리에 살짝 비침. 어른 손님은 흐리게 |
| 2 | DAD+H11 | M/OTS | 아빠 어깨 너머로 딸의 뒷모습. 아빠 얼굴 정면, 왼쪽 눈 접힘. 키 차이: 딸 머리가 아빠 가슴 |
| 3 | DAD | W | 폭풍 전 항구. 주황 우비, 배(BOAT 규격), 갑판의 노란 방수 통이 보이게 |
| 4 | DAD+H11+MOM | M | 대문. 아빠(우비) 뒤돌아 반쯤, 딸이 소매 잡음, 엄마(42, 날씬)는 문간에서 우비 하나를 든 채. 세 사람 키 순서 DAD>MOM>H11 |
| 5 | H11 | C | 아이 손 클로즈업만. 손이 작아야 함(어른 손 금지). 소매 끝이 손에서 빠져나가는 순간 |
| 6 | - | W | 유지(N2_11). 인물 없음 |
| 7 | MOM | M | 장롱 앞 무릎. 얼굴 옆모습 3/4, 40대 초반, 눈물 없이 꾹 다문 입. 방수 통은 손바닥 크기 |
| 8 | H11 | M | 처마 밑 무릎 안은 아이. 뒤 벽에 주황 우비. 강한 햇빛. 탑은 멀리 작게 |
| 9 | D12 | M | 유지(OP_4). 경호원 키 190 이상, 도윤은 H12보다 반 뼘 큼 |
| 10 | D12 | M | 유지(OP_5) |
| 11 | H12+D12 | W | 유지(OP_7). 두 아이 키 차 유지 |
| 12 | H12+D12+MOM | W | 신규. 리어카 위 엄마 얼굴 보임(42, 날씬). 아이 둘은 리어카 손잡이보다 조금 큰 정도 |
| 13 | H12 | M | 유지(N6_05). 차창 속 도윤 얼굴은 작게라도 D12 앵커 |
| 14 | H19 | C | 유지(OP_11). 달력 + 손. 손은 성인 여성 손 |

### CS1 「이름」
H19·KID·GRANNY·D20 · 채도 0.85 · 아침 안개 → 노을

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | KID+H19 | M | 유지(N1_01). 꼬마는 문가에 앉아 있어 머리가 침대 높이 |
| 2 | H19 | M | 유지(N1_02). 거울 속 얼굴은 흐리게(반사 금지 규칙과 별개로 뿌연 거울) |
| 3 | H19 | C | 유지(N1_03). 달력·손 |
| 4 | KID+H19 | M | 유지(N1_05). 하늘이 쪼그려 앉아 눈높이 맞춤 |
| 5 | KID+GRANNY+H19 | W | 골목. 할머니는 뒤 대문에서 작게, 허리 굽음. 꼬마는 후드 눌러쓰는 손동작 |
| 6 | H19 | M | 유지(N1_04). 행인 둘은 얼굴 없이 |
| 7 | H19+KID | W | 유지(N1_06). 꼬마 손가락이 탑을 가리킴 |
| 8 | H19 | M | 유지(N1_10). 보드 위 전신, 머리카락 뒤로 |
| 9 | H19 | C | 문고리 쥔 손. 손마디 하얗게, 손목을 다른 손으로 문지름. 투명 금지 |
| 10 | KID | C | 유지(N1_08). 돌 세 개 + 꼬마 무릎 |
| 11 | H19 | C | 유지(N1_11). 우유 두 병, 한 병에 물방울(차가움) |
| 12 | D20 | W | 억새·철골 사이로 본 남자. 얼굴 가림, 키 큼이 드러나게 전신. 앞쪽에 주황 소매(꼬마 손) |
| 13 | D20+H19 | M | 유지(N7_09). 벤치, 병 두 개, 하늘은 그를 보고 그는 바다를 봄 |
| 14 | KID+H19 | W | 유지(N1_12). 뒷모습, 손 잡음, 키 차이 명확 |

### EV1 「손가락 사이로」
H19·KID·가게 할머니 · 채도 0.85

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | M | 가게 안. 넘어지는 병을 잡으려는 뻣뻣한 손. 할머니는 등 돌림. 투명 금지 |
| 2 | H19+KID | M | 유리구슬 든 손가락 + 꼬마가 가리킴. 파란 구슬 한 알 |
| 3 | KID+H19 | 2S | 돌담 위 꼬마(다리 흔듦)와 턱 괸 하늘. 정오 |
| 4 | H19 | C | 밤, 탑 아래. 병 하나를 두 손으로 꼭 쥠. 다른 병은 바위 위 |

### CS2 「하트」
H19·KID·MOM(해녀)·DAD·H11 · 채도 0.78

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | W | 마당. 해녀복 두 벌(크기 다름)·우비·양철 상자. 하늘은 상자 앞 |
| 2 | H19 | C | 유지(N2_02). 봉투 든 떨리는 손, 이름은 번짐 |
| 3 | KID+H19 | M | 문간. 상자를 내려놓는 하늘, 쪼그린 꼬마 |
| 4 | H19 | M | 유지(N2_03). 쇠기둥에 손, 은은한 빛 |
| 5 | MOM+H12 | M | 신규. 수면 위 엄마 얼굴(42, 두건 벗김, 물안경 이마) + 앞쪽 바위의 12세 하늘. 엄마 얼굴 또렷 |
| 6 | DAD | M | 유지(N2_07). 아빠 배 갑판, 하트 칠하기 |
| 7 | DAD+H11 | C | 부표 든 아빠 손 + 작은 하트 가리키는 아이 손가락 + 아빠 웃는 얼굴(왼눈 접힘) |
| 8 | H19 | W | 유지(N4_01). 절벽 아래 부표 |
| 9 | KID | C | 유지(N4_02). 손톱 밑 붉은 페인트 |
| 10 | DAD+H11 | M | 유지(N2_09). 대문 밤비 |
| 11 | H19+KID | M | 바위. 하늘이 부표에 대고 소리침, 꼬마는 떨어져 앉아 부표를 봄 |
| 12 | KID | C | 후드 아래 얼굴 클로즈업. 6세 얼굴이되 눈빛만 어른. 후드는 그대로 |
| 13 | H19 | W | 유지(N2_12). 탑 아래 노을 |

### EV2 「열일곱 번」
H19·해녀 할머니 둘(얼굴 앵커 없음, 60대) · 채도 0.78

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | W | 바위 위 하늘, 멀리 해녀 셋. 엄마 아님 — 얼굴 안 보이게 멀리 |
| 2 | GRANNY×2+H19 | M | 쉬는 해녀 할머니 둘(60대, 두건). 하늘은 몇 미터 떨어져 등 반쯤 |
| 3 | KID+H19 | M | 소매 당기는 꼬마. 하늘은 아직 해녀 쪽을 봄 |
| 4 | H19 | M | 정오 해안도로, 보드 옆에 끼고 입 살짝 벌림. 꼬마는 멀리 |

### CS3 「우리 기지」
H19·H12·D12·KID·D20 · 채도 0.7 · 회상은 따뜻한 오후

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | C | 유지(N3_01). 도시락·손 |
| 2 | H12 | W | 유지(N3_02). 운동장 구석 |
| 3 | D12+H12 | 2S | 유지(N3_03). 도시락 가운데 선 |
| 4 | D12+H12 | M | 유지(N3_10). 도윤이 앞에 서고 하늘이 뒤. 경호원은 골목 끝 작게 |
| 5 | H12+D12 | W | 유지(N3_08). 기지 |
| 6 | H12+D12 | 2S | 기지 노을, 말 없이 나란히. 하늘은 무릎 안고 바다, 도윤은 그냥 옆. 키 차 유지 |
| 7 | H12+D12 | W | 유지(N3_09). 달리기 |
| 8 | H12+D12 | C/2S | 라디오 뒷판 열림, 나사·드라이버, 하늘이 마커로 하트, 도윤은 안테나 |
| 9 | D12+H12 | 2S | 유지(N3_07). 유채밭 마주 봄 |
| 10 | KID+H19 | C | 라디오 스위치 끄는 작은 손 + 하늘 손. 다이얼 91.9 불빛 |
| 11 | H19 | C | 유지(N3_11). 라디오와 구슬 몇 알 |
| 12 | D20+H19 | W | 유지(N2_06). 뒷모습 남자, 손 든 하늘 |
| 13 | D20+H19 | W | 멈춰 선 남자 뒷모습, 고개만 살짝. 하늘은 몇 미터 뒤 손 반쯤 듦. 탑 램프 |

### EV3 「우유 한 팩」
D12·H12·D20·가게 할머니 · 채도 0.7

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D12+H12 | M | 봄 골목. 우유 팩 내미는 도윤, 골목 끝 검은 차·경호원 작게 |
| 2 | H12 | M | 담장 위 우유 팩, 대문 뒤에서 엿보는 하늘, 담장 끝 고양이 꼬리 |
| 3 | D20 | M | 유지(EB_08). 가게 카운터 |
| 4 | KID+H19+D20 | W | 노을 길. 멀리 병 두 개 든 남자, 앞에 팔 벌린 꼬마, 따라가려는 하늘 |

### CS4 「열두 개의 초」
H19·H12·D12·KID·D20(원경) · 채도 0.6

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | C | 유지(N4_04). 꺼지는 초 |
| 2 | D12+H12 | M | 유지(N4_05). 케이크·초 열두 개 |
| 3 | H12+D12 | M | 유지(N4_06). 엎어진 케이크 |
| 4 | D12+H12 | W | 밤 비탈 내려가는 도윤 뒷모습, 풀밭에 병 두 개, 주먹 쥔 하늘 |
| 5 | H12 | M | 유지(N4_07). 서리 낀 도시락 |
| 6 | H12 | C | 도시락 뚜껑 안 글씨 + 입김. 아이 손 |
| 7 | D12+H12 | M | 유지(N4_08). 비 속 달려오는 도윤 |
| 8 | H12+D12 | 2S | 겨울 빛, 기지 돌 턱에 나란히, 도시락 가운데 선. 하늘은 얇은 가디건 |
| 9 | H12+D12 | C | 유지(N4_10). 돌 쌓는 작은 손들 |
| 10 | D12+H12 | W | 유지(N4_09). 마을 어른들 원경 |
| 11 | H19+KID | W | 여름 정오. 소매 걷은 하늘, 풀밭에 누운 꼬마(후드 그대로), 돌 세 개 |
| 12 | H19+KID | C | 세 번째 돌 올리는 성인 손, 옆에 노란 장화 |
| 13 | KID+D20 | W | 노을. 돌 위에 손 얹은 꼬마, 비탈 아래 멀어지는 회색 코트 원경 |

### EV4 「달력」
H19·KID·D20(방·원경) · 채도 0.6

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | C | X 표시 달력 + 짧은 연필 든 손 |
| 2 | H19 | M | 창밖에서 본 방. 시간표·코트·빈 병 둘. 창틀에 하늘 손. 반사 금지 |
| 3 | KID+H19 | M | 한여름 길. 후드 쓴 꼬마 뺨에 땀, 하늘은 후드티를 허리에 묶음(흰 티) |
| 4 | H19 | W | 노을 전 탑 아래 도착. 해가 아직 바다 위. 병 두 개, 아무도 없음 |

### EV5 「찢어진 소매」
D20(원경)·H19·KID · 채도 0.5

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D20+H19 | W | 유지(N5_02). 빗속 뒷모습 |
| 2 | KID+H19 | M | 유지(N5_03). 트럭은 옆을 지나감, 하늘은 갓길에 넘어짐, 꼬마 소매 찢김. 통과 금지 |
| 3 | KID+H19 | M | 유지(N5_04). 소매 감추는 꼬마 |
| 4 | KID+H19 | M | 밤 방. 실 끊는 꼬마, 바늘 든 떨리는 성인 손(실 안 꿰임) |

### CS5 「그 밤」
H19·H12·D12·MOM·KID·D20 · 채도 0.5 · 회상은 폭우

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | C | 유지(N5_11). 손잡이 흠집 + 손 |
| 2 | H19 | C | 흠집에 손가락 끝. 화면 가장자리부터 비로 번짐 |
| 3 | D12+H12+MOM | M | 신규. 마당 엎어진 엄마(얼굴 보임, 42). 소리치는 도윤, 굳은 하늘 |
| 4 | D12+H12 | C/2S | 빗속 어깨 잡고 마주 봄. 도윤이 반 뼘 큼 |
| 5 | H12+D12+MOM | W | 신규. 리어카 위 엄마(얼굴 차분), 아이 둘이 밈 |
| 6 | D12+H12 | M | 유지(N5_08). 무릎 꺾인 도윤 |
| 7 | H12+D12 | C | 겹쳐 잡은 작은 손 둘. 병원 불빛 원경 |
| 8 | D12+H12 | 2S | 빗속 나란히 밀며 도윤이 하늘 쪽으로 얼굴 |
| 9 | H12+D12 | 2S | 유지(N5_10). 병원 계단 담요 |
| 10 | H19 | M | 리어카 손잡이에 이마 댄 하늘. 새 바퀴 밝게 |
| 11 | KID+H19 | W | 유지(N5_12). 리어카 위 꼬마 |
| 12 | KID+H19 | M | 유지(N5_05). 새 바퀴 |
| 13 | KID+H19+D20 | W | 앞 리어카·하늘, 같은 길 멀리 회색 코트 뒷모습 |

### EV6 「물때」
D20·H19·해경(50대 제복) · 채도 0.5

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D20 | W | 새벽 갯바위, 손전등, 키 큰 실루엣. 얼굴은 멀어서 작게 |
| 2 | D20+H19 | M | 폐그물 더미 앞 고개 숙인 도윤, 몇 미터 뒤 멈춘 하늘 |
| 3 | D20 | M | 초소 유리문 앞 도윤, 안의 해경 아저씨 |
| 4 | H19 | W | 해 질 녘 초소 게시판을 멀리서 보는 하늘 |

### CS6 「스무 살」
D12·H12·D15(도윤 15세: D12 앵커, 병원복)·H16(하늘 16세: H19 앵커, 머리 어깨선)·H19·D20 · 채도 0.42

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D12+H12 | M | 유지(N6_02). 검은 차 |
| 2 | D12+H12 | C | 유지(N6_03) |
| 3 | D12+H12 | 2S | 유지(N6_04). 우유 두 병 |
| 4 | H12 | M | 유지(N6_06). 달력에 글씨 |
| 5 | D15 | M | 서울 병실 밤. 15세 도윤(D12 앵커, 조금 더 큼, 병원복), 문가 아버지는 정장·얼굴 그늘 |
| 6 | H16 | M | 16세 하늘(H19 앵커, 머리 어깨선). 빈 우편함. 저녁 |
| 7 | H19 | M | 유지(N6_07). 해녀복 하늘. 얼굴은 H19 |
| 8 | H19 | M | 유지(N6_08). 떨어진 우비 |
| 9 | H19 | M | 유지(N6_09). 우비 안음 |
| 10 | H19 | W | 유지(N6_10). 새벽 바다 입수 |
| 11 | - | W | 유지(N6_11). 수중 손 |
| 12 | D20 | M | 새벽 대문 앞 양철 상자 내려놓는 도윤. 문 안쪽 우비 |
| 13 | H19+D20 | M | **오도 컷.** 절벽 길. 하늘 손·팔은 또렷, 소매 쥐어지지 않음. **도윤만 안개처럼 흐리게**(전 편 유일) |
| 14 | D20+H19 | W | 정류장 벤치. 전화 든 도윤, 반대편 끝에 등 돌린 하늘. 둘 다 얼굴 그늘 |

### EV7 「첫눈」
KID·H19 · 채도 0.42

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | KID+H19 | M | 유지(N3_05). 성에 얼굴 그림 |
| 2 | KID+H19 | M | 눈길. 뒤 돌아보는 꼬마, 내려다보는 하늘. 발자국 화면에 안 잡히게 프레이밍 |
| 3 | H19 | M | X 가득한 달력 밑 담요 두른 하늘 |
| 4 | KID | C | 잠든 꼬마, 후드 살짝 들춘 손. **왼쪽 눈가 접힘** 보이게(예외 컷 1) |

### EV8 「국화」
D20(물건)·KID·H19 · 채도 0.42

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D20 | C | 유지(N7_10). 창가 국화. 유리에 반사 없음 |
| 2 | KID+H19+D20 | M | 유지(N7_11). 한 걸음 물러난 꼬마 |
| 3 | - | C | 국화 두 다발 나란히, 읽히지 않는 이름. 하늘 소매 끝만 |
| 4 | H19 | M | 밤 담장 위 하늘, 등 뒤 불 켜진 창 |

### EV9 「스무 번째」
H19·D20 · 채도 0.32

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19 | M | 편지 펼쳐진 방바닥, 편지를 얼굴 가까이 |
| 2 | H19 | C | 편지 클로즈업, 엄지가 한 줄 누름. 글씨는 읽히지 않게 |
| 3 | D20 | M | 유지(N7_08). 우체통 앞 |
| 4 | D20+H19 | M | 문간 상자에 스무 번째 봉투 꽂는 도윤, 뒤 문 안 하늘. 봉투 이름은 이 컷만 선명 |

### CS7 「둘 중 하나」
D20·H19·KID · 채도 0.32 · 겨울

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D20 | M | 유지(N7_01). 유채밭 편지 |
| 2 | D20+H19 | M | 겨울 길. 찬바람 맞은 듯 뺨에 손 든 도윤, 옆에서 눈 닦는 하늘 |
| 3 | D20 | C | 유지(N7_03). 약 한 움큼·팔찌 |
| 4 | H19+D20 | M | 바람에 도는 코팅 카드, 손 뻗는 하늘, 돌아보는 도윤 |
| 5 | H19 | C | 유지(N8_03)를 **카드로 재생성 권장**: 손바닥 위 코팅 카드(사진+이름). 팔찌 아님 |
| 6 | H19 | C | 유지(N8_04). 눈물 고인 얼굴 |
| 7 | H19+KID | 2S | 겨울 노을 탑 아래, 카드 무릎에, 꼬마가 어깨 붙이고 앉음 |
| 8 | D20+H19 | C | 도윤 손바닥 위 카드, 물러나는 하늘 손은 **여기서 처음 살짝 투명** |
| 9 | D20+H19 | C/2S | 카드 가슴에 댄 도윤, 바로 앞 하늘. 둘 다 눈물 |
| 10 | D20+H19 | M | 유지(N7_12). 창에 도윤만 비침 |
| 11 | KID+H19 | M | 젖은 상점 유리. 꼬마는 또렷이 비침, 하늘 자리는 바다 빛 |
| 12 | KID | C | 후드 아래 내려 본 얼굴, 입 다묾. 빗방울 |
| 13 | H19 | C | 유지(N8_05). 달력에 이름, 손 살짝 흐림 |

### EV10 「전날 밤」
D20·H19·KID · 채도 0.25

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D20+H19 | W | 유지(N8_01). 국화 두 다발 나란히 걷기 |
| 2 | D20 | M | 밤 방. 날짜 적힌 시간표, 짐 가방, 벽에 기대 잠든 도윤 |
| 3 | KID+H19 | M | 라디오를 무릎에 올려 주는 꼬마 |
| 4 | H19 | M | 바닥에 누운 하늘, 베개 옆 우비, 빈 못 |

### CS8 「주파수」
D20·MOM·H19·KID(후드 벗는 컷 11)·MIN_MOM · 채도 0.25 · 노을 → 황혼 → 새벽

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D20+H19 | W | 유지(N8_12). 병 두 개 든 도윤, 도착한 하늘 뒷모습 |
| 2 | MOM+D20 | W | 신규. 비탈 오르는 엄마(42, 남색 평상복, 국화), 앞의 도윤 |
| 3 | MOM+D20+H19 | M | 엄마가 눈 내리깔고 말함, 도윤 마주 봄, 사이에 하늘(입 막음) |
| 4 | D20+MOM | M | 무릎 꿇고 병 껴안은 도윤, 어깨 위 엄마 손 |
| 5 | D20+MOM | C | 도윤 젖은 얼굴 + 병 두 개 + 등 쓸어 주는 엄마 손 |
| 6 | MOM+H19 | M | 국화 놓는 엄마(옷깃 여밈), 뒤에서 안는 하늘 팔이 **엄마를 통과**(안개). 엄마 얼굴 정면 또렷 |
| 7 | H19+D20 | C/2S | 무릎 꿇고 마주 본 둘, 하늘 손이 뺨을 통과. 도윤 눈 감음 |
| 8 | - | W | 수중. 발목 그물, 푸른 조개, 수면의 주황 점 |
| 9 | H19 | M | 유지(N8_06). 빈 방 병 하나 |
| 10 | KID+H19 | C | 방수 통 여는 꼬마 손, 머리띠 보석 빛, 위에 하늘 얼굴 |
| 11 | KID+H19 | C | **후드 벗은 꼬마 얼굴**(예외 컷 2), 왼눈 접힘, 하늘 손이 뺨에 |
| 12 | KID+H19 | M | 두 손 맞잡음, 꼬마가 올려다봄, 하늘 얼굴 무너짐 |
| 13 | H19+KID | M | 꼭 안음. 둘 다 또렷 |
| 14 | KID+MIN_MOM+H19 | W | 유채밭 뛰어 내려가는 꼬마, 길가의 민재 엄마(30대) 작게, 언덕 위 하늘 |
| 15 | H19 | M | 혼자, 머리띠 가슴에, 눈 감음. 풀에 걸린 주황 실 한 올 |
| 16 | H19 | W | 머리띠 쓰고 라디오 메고 새벽 도로 출발 |

### END_A 「엇갈린 정류장」
D20·H19 · 채도 0.35

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | D20 | M | 버스 계단 앞 돌아보는 도윤 |
| 2 | H19+D20 | C/2S | 뺨을 향한 손끝만 사라짐 |
| 3 | D20 | M | 버스 문 안, 기사 뒤돌아봄 |
| 4 | D20+H19 | C | 유리 양쪽 손바닥, 얼굴 어긋남 |
| 5 | H19 | M | 빈 정류장, 병 하나, 벤치 끝 하늘 |
| 6 | H19 | C | 라디오 다이얼, 안 돌리는 손 |
| 7 | D20 | M | 도시 병원 창, 멀리 탑 |
| 8 | H19 | W | 탑 아래 돌 쌓는 흐릿한 하늘 |
| 9 | - | W | 아무도 없는 탑 |

### END_B 「우유 두 병」
H19·D20 · 채도 0.5

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19+D20 | M | 라디오 든 하늘, 등 돌린 도윤 |
| 2 | D20+H19 | M | 정류장 한가운데 도는 도윤, 반투명 하늘 |
| 3 | D20+H19 | C/2S | 얼굴 나란히, 시선 어긋남 |
| 4 | H19 | C | 라디오에 대고 말하는 하늘, 눈 감음 |
| 5 | D20 | M | 유지(EA_08). 벤치 병 두 개 |
| 6 | D20+H19 | C | 유지(EB_03). 손 사이 하트 |
| 7 | D20 | M | 버스 오르며 돌아봄, 벤치에 병 둘 |
| 8 | D20 | M | 유지(EB_08). 가게 |
| 9 | - | W | 새벽 탑 아래 병 하나 |

### END_TRUE 「맞닿은 주파수 91.9」
H19(머리띠)·D20 · 채도 0.25→1.0 · 마지막 3컷은 방송국 밤 → 새벽 → 사진

| 컷 | 앵커 | 샷 | 지시 |
|---|---|---|---|
| 1 | H19+D20 | M | 흑백에 가까움. 머리띠 보석·다이얼만 색 |
| 2 | H19+D20 | W | 하늘 발밑부터 색 번짐. 도윤 돌아봄, 병 떨어짐 |
| 3 | H19+D20 | 2S | 풀컬러. 또렷한 하늘, 웃다 우는 도윤 |
| 4 | D20+H19 | C | 유지(EB_03). 손 사이 하트 빛 |
| 5 | D20+H19 | C | 유지(EA_05). 웃는 투샷 |
| 6 | H19+D20 | C/2S | 이마 맞댐, 눈 감음, 맞잡은 손 빛 |
| 7 | D20 | M | 혼자 남은 도윤, 손을 빛에, 발밑 병 둘 |
| 8 | D20 | M | 유지(ET_03). 방송국 마이크 |
| 9 | D20+H19 | M | 부스에서 일어선 도윤, 유리 너머 복도의 하늘(ON AIR 불빛). 둘 다 또렷 |
| 10 | - | W | 새벽 탑, 돌 넷, 라디오, 빈 병 둘, 유채 한 송이 |
| 11 | H19+D20 | C | 풀밭의 사진 한 장. 사진 속 둘은 똑같이 또렷, 손 잡고 웃음. 글씨 없음 |
