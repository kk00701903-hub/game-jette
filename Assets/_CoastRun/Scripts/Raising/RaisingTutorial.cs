using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 31차: 육성 화면 튜토리얼 — 버튼을 하나씩 하이라이트하며 소개한다.
    /// 화면 위에 어두운 막(4장)으로 대상만 밝게 남기고, 대상 둘레에 맥동하는 테두리, 화자 그림 + 말풍선 + [다음]/[건너뛰기].
    /// 처음 육성 화면에 들어왔을 때 1회(PlayerPrefs CoastRun_RaisingTut). ★ 옆 「도움」 버튼과 에디터 F1 로 다시 볼 수 있다.
    /// 95차(사용자): 설명하는 사람을 집사에서 **꼬마**(주황 우비)로 바꿨다 — 이름은 아직 안 밝히는 시점이라 이름표는 「꼬마」.
    public class RaisingTutorial : MonoBehaviour
    {
        public const string PrefKey = "CoastRun_RaisingTut";

        public class Step { public string target; public string[] targets; public string ko, en; public bool speakerTop; }

        private RectTransform _uiRoot, _root;
        private Canvas _canvas;
        private List<Step> _steps;
        private int _i = -1;
        private Action _onDone;
        private readonly Image[] _dim = new Image[4];
        private Image _ring, _ring2;
        private RectTransform _speaker, _bubble;
        private Text _text, _count;
        private float _t;

        public static RaisingTutorial Open(RectTransform uiRoot, List<Step> steps, Action onDone)
        {
            var go = new GameObject("RaisingTutorial");
            var t = go.AddComponent<RaisingTutorial>();
            t._uiRoot = uiRoot; t._steps = steps; t._onDone = onDone;
            t.Build();
            t.Next();
            return t;
        }

        public static List<Step> DefaultSteps()
        {
            return TamaSteps();
        }

        /// TamaRaisingUI 대상 이름에 맞춘 튜토리얼 — 화면 위에서 아래로, 버튼을 하나씩.
        /// 95차(사용자): 꼬마가 설명한다. 여섯 살 말투(짧게, 같은 말을 두 번), 하늘이를 「바다누나」라고 부른다.
        public static List<Step> TamaSteps()
        {
            return new List<Step>
            {
                new Step { target = null, ko = "바다누나, 처음이지?\n내가 내가 알려 줄게. 하나씩 눌러 볼 거야.", en = "First time, sis?\nI'll show you. One by one, one by one." },
                new Step { target = "Week", ko = "여기 <b>몇 주째</b>인지, 무슨 계절인지 나와.\n한 주에 세 번 움직이면 다음 주로 가.", en = "This says the <b>week</b> and the season.\nThree things each week, then the week ends." },
                new Step { target = "GoalRibbon", ko = "이번에 <b>뭘 해야 하는지</b> 여기 써 있어.\n모르면 여기 봐. 여기 보면 돼.", en = "This ribbon says <b>what you need</b> this chapter.\nWhen you're lost, look here." },
                new Step { target = "Money", ko = "<b>G</b>는 돈이야. 쌀 사고 옷 사고.\n다 쓰면 밥을 못 먹어. 못 먹어.", en = "<b>G</b> is money — rice, clothes, stuff.\nSpend it all and there's no dinner." },
                new Step { target = "StatusBtn", ko = "별을 누르면 <b>상태창</b>이야.\n체력이랑 기운이랑, 누나 몸 상태 다 보여.", en = "The star opens your <b>status</b>.\nStamina, energy, how you're holding up." },
                new Step { target = "TutorialBtn", ko = "별 옆에 <b>전구</b>. 이거 나야.\n또 모르겠으면 여기 눌러. 내가 또 올게.", en = "The <b>bulb</b> next to the star is me.\nTap it anytime and I'll come back." },
                new Step { target = "RoomBtn", ko = "<b>마이룸</b>. 방 꾸미고 텃밭에 씨 심고.\n놀이도 있어. 놀이 하면 돈도 생겨.", en = "<b>My Room</b> — decorate, plant seeds, play games.\nGames give you a little money too." },
                new Step { target = "ShopBtn", ko = "<b>장보기</b>야. 쌀, 반찬, 옷.\n쌀 없으면 큰일 나. 큰일 나.", en = "<b>Shop</b> — rice, side dishes, clothes.\nNo rice is bad. Really bad." },
                new Step { target = "BagBtn", ko = "<b>가방</b>엔 산 게 들어 있어.\n뭐가 남았는지 여기서 세어 봐.", en = "Your <b>bag</b> holds what you bought.\nCheck here for what's left." },
                new Step { targets = new[] { "HpTrack", "StressTrack" }, ko = "빨간 건 <b>♥ 체력</b>, 보라는 <b>스트레스</b>야.\n보라 막대 <b>노란 줄</b> 넘으면 누나가 지쳐서 자꾸 실패해.", en = "Red is <b>♥ stamina</b>, purple is <b>stress</b>.\nPast the <b>yellow tick</b> you're worn out and things start failing." },
                new Step { target = "Girl", ko = "누나를 <b>쓰다듬으면</b> 스트레스가 조금 내려가.\n근데 한 주에 세 번만이야. 세 번만.", en = "<b>Pet</b> her and stress drops a bit.\nOnly three times a week though. Three." },
                new Step { targets = new[] { "Act0", "Act1", "Act2" }, ko = "아래 <b>밥 · 놀기 · 알바</b>.\n누르면 카드 두 장 나와. 하나 골라.", en = "<b>Feed · Play · Work</b> down here.\nTap one, then pick from two cards.", speakerTop = true },
                new Step { targets = new[] { "ActRing0", "ActRing1", "ActRing2" }, ko = "동그라미 세 개가 <b>이번 주</b>야.\n하면 초록 ✓. 세 개 다 차면 끝.", en = "Three circles are <b>this week</b>.\nEach one turns green. Three and you're done.", speakerTop = true },
                new Step { target = "Auto", ko = "<b>자동</b>을 켜면 누나가 알아서 해.\n근데 누나가 막 골라. 나는 누나가 고르는 게 좋아.", en = "<b>Auto</b> lets her decide on her own.\nShe picks fast, though. I like it when you pick.", speakerTop = true },
                new Step { target = "NextTurn", ko = "<b>다음 턴</b> 누르면 한 주가 지나가.\n밥 먹었는지 잠 잤는지 다 세어 줘.", en = "<b>Next turn</b> ends the week.\nIt counts your meals, sleep, everything.", speakerTop = true },
                new Step { target = "Home", ko = "<b>홈</b>은 처음 화면으로 나가는 거야.\n여기까지 한 건 저장돼. 걱정 마.", en = "<b>Home</b> goes back to the title.\nYour week is saved. Don't worry.", speakerTop = true },
                new Step { target = null, ko = "다 알려 줬어. 밥부터 눌러 볼래?\n또 모르면 전구 눌러. 전구.", en = "That's everything. Try Feed first?\nIf you forget, tap the bulb. The bulb." },
            };
        }

        private void Build()
        {
            _canvas = CoastUiCanvas.Create("TutorialCanvas", 140, transform);
            _root = CoastUiCanvas.Root(_canvas);
            // 입력 막(투명) — 튜토리얼 중엔 아래 UI를 못 누른다
            var block = CoastHudLayout.MakeImage(_root, "Block", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-2000f, -3000f), new Vector2(2000f, 3000f), new Color(0f, 0f, 0f, 0.03f));   // 알파 0.03: RaycastWatchdog(투명 막 자동 해제)에 안 걸리게
            block.raycastTarget = true;
            for (int i = 0; i < 4; i++)
            {
                _dim[i] = CoastHudLayout.MakeImage(_root, "Dim" + i, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.04f, 0.10f, 0.72f));
                _dim[i].rectTransform.pivot = Vector2.zero; _dim[i].raycastTarget = true;
            }
            _ring = CoastUiArt.Panel(_root, "Ring", new Color(1f, 0.85f, 0.35f, 0.95f), 18);
            _ring.rectTransform.anchorMin = _ring.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); _ring.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _ring2 = CoastUiArt.Panel(_ring.transform, "Hole", new Color(0.08f, 0.04f, 0.10f, 0f), 14);
            _ring2.rectTransform.anchorMin = Vector2.zero; _ring2.rectTransform.anchorMax = Vector2.one; _ring2.rectTransform.offsetMin = new Vector2(5f, 5f); _ring2.rectTransform.offsetMax = new Vector2(-5f, -5f);
            // 링 안쪽은 어두운 막이 없어야 하므로 링은 '테두리만' — 안쪽 Hole 을 투명이 아니라 '막을 뚫는' 용도로는 못 쓰니, 링 자체를 얇은 액자 4장으로 만든다.
            _ring2.enabled = false;

            // 꼬마 + 말풍선 — 95차: 전신(UI_Kid_Bust = Raise_Kid 의 여백을 자른 422×824)이라 세로로 길게.
            _speaker = new GameObject("Speaker", typeof(RectTransform)).GetComponent<RectTransform>();
            _speaker.SetParent(_root, false);
            _speaker.sizeDelta = new Vector2(176f, 344f);
            var tex = ArtAssets.LoadTexture("UI_Kid_Bust") ?? ArtAssets.LoadTexture("Raise_Kid")
                      ?? ArtAssets.LoadTexture("UI_Butler_Bust") ?? ArtAssets.LoadTexture("UI_Butler_Boy");
            var img = new GameObject("Img", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            img.transform.SetParent(_speaker, false);
            img.rectTransform.anchorMin = Vector2.zero; img.rectTransform.anchorMax = Vector2.one; img.rectTransform.offsetMin = img.rectTransform.offsetMax = Vector2.zero;
            img.preserveAspect = true; img.raycastTarget = false;
            if (tex != null) img.sprite = CoastUiArt.AsSprite(RaisingUI.ChromaKeyed(tex)); else img.color = new Color(0.25f, 0.2f, 0.35f);

            var pill = CoastUiArt.CutePill(_root, "Bubble", new Color(1f, 0.99f, 0.95f), 20, 4);
            _bubble = pill.rectTransform;
            _bubble.sizeDelta = new Vector2(440f, 210f);   // 95차: 470 이면 꼬마(176) 와 합쳐 인셋 664 를 넘어 오른쪽이 삐져나왔다
            var tag = CoastHudLayout.MakeText(_bubble, "Tag", Loc.T("꼬마", "The kid"), 13, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -30f), new Vector2(-12f, -8f));
            tag.color = new Color(0.94f, 0.48f, 0.15f); tag.fontStyle = FontStyle.Bold;   // 95차: 주황 우비 색
            _count = CoastHudLayout.MakeText(_bubble, "Count", "", 12, TextAnchor.UpperRight, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -30f), new Vector2(-16f, -8f));
            _count.color = new Color(0.55f, 0.5f, 0.6f);
            _text = CoastHudLayout.MakeText(_bubble, "Text", "", 15, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(18f, 60f), new Vector2(-14f, -34f));
            _text.color = new Color(0.25f, 0.15f, 0.12f); _text.horizontalOverflow = HorizontalWrapMode.Wrap; _text.supportRichText = true;
            _text.resizeTextForBestFit = true; _text.resizeTextMinSize = 14; _text.resizeTextMaxSize = CoastHudLayout.Scaled(15);
            Btn(_bubble, "Next", Loc.T("다음", "Next"), new Color(1f, 0.44f, 0.57f), new Vector2(1f, 0f), new Vector2(-12f, 10f), new Vector2(130f, 44f), Next);
            Btn(_bubble, "Skip", Loc.T("건너뛰기", "Skip"), new Color(0.62f, 0.63f, 0.70f), new Vector2(0f, 0f), new Vector2(12f, 10f), new Vector2(130f, 44f), Finish);
        }

        private static Button Btn(Transform parent, string name, string label, Color color, Vector2 anchor, Vector2 pos, Vector2 size, Action onClick)
        {
            var pill = CoastUiArt.CutePill(parent, name, color, 14, 3);
            pill.rectTransform.anchorMin = pill.rectTransform.anchorMax = anchor; pill.rectTransform.pivot = anchor;
            pill.rectTransform.anchoredPosition = pos; pill.rectTransform.sizeDelta = size; pill.raycastTarget = true;
            var b = pill.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None;
            b.onClick.AddListener(() => { CoastPrefs.Vibrate(); onClick?.Invoke(); });
            var t = CoastHudLayout.MakeText(pill.transform, "T", label, 14, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CoastUiArt.OutlineText(t, new Color(0f, 0f, 0f, 0.35f), 1.5f);
            return b;
        }

        private void Next()
        {
            _i++;
            if (_i >= _steps.Count) { Finish(); return; }
            Show(_steps[_i]);
        }

        private void Finish()
        {
            PlayerPrefs.SetInt(PrefKey, 1); PlayerPrefs.Save();
            var cb = _onDone; _onDone = null;
            Destroy(gameObject);
            cb?.Invoke();
        }

        private RectTransform FindTarget(string name)
        {
            if (string.IsNullOrEmpty(name) || _uiRoot == null) return null;
            foreach (var rt in _uiRoot.GetComponentsInChildren<RectTransform>(true))
                if (rt.name == name) return rt;
            return null;
        }

        /// 대상 RectTransform 의 사각형을 튜토리얼 캔버스 루트 로컬 좌표로.
        private Rect LocalRect(RectTransform target)
        {
            var c = new Vector3[4]; target.GetWorldCorners(c);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                var sp = RectTransformUtility.WorldToScreenPoint(null, c[i]);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, sp, null, out var l);
                min = Vector2.Min(min, l); max = Vector2.Max(max, l);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private void Show(Step s)
        {
            _count.text = $"{_i + 1} / {_steps.Count}";
            _text.text = Loc.T(s.ko, s.en);
            Rect? hi = null;
            var names = s.targets ?? (s.target != null ? new[] { s.target } : null);
            if (names != null)
                foreach (var n in names)
                {
                    var t = FindTarget(n); if (t == null) continue;
                    var r = LocalRect(t);
                    hi = hi.HasValue ? Rect.MinMaxRect(Mathf.Min(hi.Value.xMin, r.xMin), Mathf.Min(hi.Value.yMin, r.yMin), Mathf.Max(hi.Value.xMax, r.xMax), Mathf.Max(hi.Value.yMax, r.yMax)) : r;
                }
            const float big = 3000f, pad = 8f;
            if (hi.HasValue)
            {
                var r = hi.Value; r.xMin -= pad; r.yMin -= pad; r.xMax += pad; r.yMax += pad;
                // 위 / 아래 / 왼 / 오 — 네 장으로 구멍
                SetDim(0, -big, r.yMax, 2f * big, big);            // 위
                SetDim(1, -big, -big, 2f * big, big + r.yMin);      // 아래
                SetDim(2, -big, r.yMin, big + r.xMin, r.height);    // 왼
                SetDim(3, r.xMax, r.yMin, big, r.height);           // 오
                _ring.enabled = true;
                _ring.rectTransform.anchoredPosition = r.center; _ring.rectTransform.sizeDelta = r.size + new Vector2(10f, 10f);
                // 링은 액자: 안쪽을 다시 밝히려면 안쪽에 같은 크기의 투명 구멍이 필요 → sliced 스프라이트의 테두리만 남기는 대신 4각 띠로
                BuildRingBands(r);
                // 화자·말풍선 위치: 대상이 화면 위쪽이면 꼬마는 아래, 아래쪽이면 위. Step.speakerTop 이 있으면 우선.
                bool top = s.speakerTop || r.center.y < -80f;
                PlaceSpeaker(top);
            }
            else
            {
                SetDim(0, -big, -big, 2f * big, 2f * big); SetDim(1, 0, 0, 0, 0); SetDim(2, 0, 0, 0, 0); SetDim(3, 0, 0, 0, 0);
                _ring.enabled = false; ClearBands();
                PlaceSpeaker(false, center: true);
            }
            _t = 0f;
        }

        private readonly List<Image> _bands = new List<Image>();
        private void ClearBands() { foreach (var b in _bands) if (b != null) Destroy(b.gameObject); _bands.Clear(); }
        private void BuildRingBands(Rect r)
        {
            ClearBands(); _ring.enabled = false;
            const float w = 5f;
            var col = new Color(1f, 0.85f, 0.35f, 0.95f);
            _bands.Add(Band(new Rect(r.xMin - w, r.yMax, r.width + 2f * w, w), col));
            _bands.Add(Band(new Rect(r.xMin - w, r.yMin - w, r.width + 2f * w, w), col));
            _bands.Add(Band(new Rect(r.xMin - w, r.yMin, w, r.height), col));
            _bands.Add(Band(new Rect(r.xMax, r.yMin, w, r.height), col));
        }
        private Image Band(Rect r, Color c)
        {
            var im = CoastUiArt.Panel(_root, "Band", c, 3);
            im.rectTransform.anchorMin = im.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); im.rectTransform.pivot = Vector2.zero;
            im.rectTransform.anchoredPosition = r.min; im.rectTransform.sizeDelta = r.size;
            return im;
        }

        private void SetDim(int i, float x, float y, float w, float h)
        {
            var rt = _dim[i].rectTransform;
            rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(Mathf.Max(0f, w), Mathf.Max(0f, h));
        }

        private void PlaceSpeaker(bool top, bool center = false)
        {
            var rr = _root.rect;
            _speaker.anchorMin = _speaker.anchorMax = new Vector2(0.5f, 0.5f); _speaker.pivot = new Vector2(0f, 0f);
            _bubble.anchorMin = _bubble.anchorMax = new Vector2(0.5f, 0.5f); _bubble.pivot = new Vector2(0f, 0f);
            // 95차: 화자 그림이 240 → 344 로 길어져, 위쪽에 세울 때 머리가 화면 밖으로 나가던 것(yMax−300)을 내렸다.
            float y = center ? -110f : top ? rr.yMax - 400f : rr.yMin + 30f;
            _speaker.anchoredPosition = new Vector2(rr.xMin + 10f, y);
            _bubble.anchoredPosition = new Vector2(rr.xMin + 196f, y + 20f);
            _speaker.SetAsLastSibling(); _bubble.SetAsLastSibling();
        }

        private void Update()
        {
            _t += Time.unscaledDeltaTime;
            float p = 0.75f + 0.25f * Mathf.Abs(Mathf.Sin(_t * 3f));
            foreach (var b in _bands) if (b != null) b.color = new Color(1f, 0.85f, 0.35f, p);
            if (_speaker != null) _speaker.localScale = new Vector3(1f, 1f + 0.02f * Mathf.Sin(_t * 2.2f), 1f);
            // 엔터가 EventSystem Submit 으로 아래 화면의 마지막 선택 버튼을 누르지 않게 선택을 비운다
            var es = UnityEngine.EventSystems.EventSystem.current;
            if (es != null && es.currentSelectedGameObject != null) es.SetSelectedGameObject(null);
            if (CoastRemoteKeys.Down(KeyCode.Return) || CoastRemoteKeys.Down(KeyCode.Space)) Next();
        }
    }
}
