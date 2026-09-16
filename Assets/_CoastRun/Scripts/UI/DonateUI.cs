using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 52차(사용자): 「기부부탁」 — 타이틀 우상단에 반짝이며 떠 있는 커피잔 아이콘(다른 페이지에선 숨김) + 팝업.
    ///   첫 로딩엔 팝업이 저절로 한 번 뜬다(Donation.PopupSeen). 팝업: 개발자 메시지 → 선물 고르기(히든 트랙 / 모든 게임 열림 패스코드 / 안 받기) → 구글 결제 $3.
    public static class DonateUI
    {
        private static Canvas _canvas;
        private static RectTransform _root;
        private static Action _onClose;
        private static Donation.Gift _gift = Donation.Gift.HiddenTrack;
        private static readonly Image[] _giftPills = new Image[4];
        private static Text _cups, _status;
        private static Button _payBtn;
        public static bool IsOpen => _canvas != null;

        private static readonly Color Ink = new Color(0.20f, 0.14f, 0.10f);
        private static readonly Color Cream = new Color(0.99f, 0.96f, 0.90f);
        private static readonly Color Coffee = new Color(0.45f, 0.28f, 0.16f);
        private static readonly Color Rose = new Color(0.93f, 0.22f, 0.52f);
        private static readonly Color PillOff = new Color(0.945f, 0.894f, 0.804f);
        private static readonly Color PillOn = new Color(1f, 0.80f, 0.35f);

        // ── 우상단 떠 있는 아이콘 ──
        public static GameObject AttachIcon(Transform titleUi, Func<bool> visible, Action onTap)
        {
            var go = new GameObject("DonateIcon", typeof(RectTransform), typeof(Image), typeof(Button), typeof(DonateIconAnim));
            go.transform.SetParent(titleUi, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-78f, -286f); rt.sizeDelta = new Vector2(124f, 124f);
            var img = go.GetComponent<Image>();
            var art = ArtAssets.LoadTexture("UI_Donate_Cup");
            if (art != null) { img.sprite = CoastUiArt.AsSprite(art); img.preserveAspect = true; img.color = Color.white; }
            else { img.sprite = CoastUiArt.RoundedRect(40); img.type = Image.Type.Sliced; img.color = new Color(1f, 0.86f, 0.45f, 0.95f); }
            img.raycastTarget = true;
            if (art == null)
            {
                var glyph = CoastHudLayout.MakeText(rt, "Glyph", "☕", 46, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 10f), new Vector2(0f, 0f));
                glyph.color = Coffee;
            }
            // 라벨 「기부부탁」
            var tag = CoastUiArt.CutePill(rt, "Tag", Rose, 12, 2); tag.raycastTarget = false;
            var trt = tag.rectTransform; trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0f); trt.pivot = new Vector2(0.5f, 0.5f); trt.anchoredPosition = new Vector2(0f, 2f); trt.sizeDelta = new Vector2(104f, 30f);
            var tl = CoastHudLayout.MakeText(trt, "T", Loc.T("기부부탁", "Donate?"), 15, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
            tl.color = Color.white; tl.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(tl, new Color(0f, 0f, 0f, 0.35f), 1.2f);
            // 반짝이 4개
            var anim = go.GetComponent<DonateIconAnim>();
            anim.visible = visible;
            for (int i = 0; i < 4; i++)
            {
                var sp = CoastHudLayout.MakeText(rt, "Spark" + i, "✦", 18 + (i % 2) * 8, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-16f, -16f), new Vector2(16f, 16f));
                sp.color = new Color(1f, 0.95f, 0.6f); sp.raycastTarget = false;
                anim.sparks[i] = sp;
            }
            var b = go.GetComponent<Button>(); b.transition = Selectable.Transition.None;
            b.onClick.AddListener(() => { CoastPrefs.Vibrate(); onTap?.Invoke(); });
            return go;
        }

        /// 둥실 + 맥동 + 반짝이 회전. visible() 이 false 면 숨긴다(다른 페이지).
        public class DonateIconAnim : MonoBehaviour
        {
            public Func<bool> visible;
            public readonly Text[] sparks = new Text[4];
            private CanvasGroup _cg; private float _t;
            private void Awake() { _cg = gameObject.AddComponent<CanvasGroup>(); }
            private void Update()
            {
                bool on = visible == null || visible();
                _cg.alpha = Mathf.MoveTowards(_cg.alpha, on ? 1f : 0f, Time.unscaledDeltaTime * 6f);
                _cg.blocksRaycasts = on; _cg.interactable = on;
                if (!on) return;
                _t += Time.unscaledDeltaTime;
                float pulse = 1f + Mathf.Sin(_t * 3.2f) * 0.05f;
                transform.localScale = Vector3.one * pulse;
                var rt = (RectTransform)transform;
                rt.anchoredPosition = new Vector2(-78f, -286f + Mathf.Sin(_t * 1.6f) * 6f);
                for (int i = 0; i < sparks.Length; i++)
                {
                    var s = sparks[i]; if (s == null) continue;
                    float a = _t * 1.1f + i * Mathf.PI * 0.5f;
                    s.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(a) * 66f, 10f + Mathf.Sin(a) * 52f);
                    float tw = 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(_t * 4f + i * 1.3f));
                    s.color = new Color(1f, 0.95f, 0.6f, tw);
                    s.transform.localScale = Vector3.one * (0.7f + 0.5f * tw);
                }
            }
        }

        // ── 팝업 ── 86차(사용자 시안): 전체 화면 페이지 — 시안 그림(UI_Donate_BG: 꽃 테두리·갈색 카드·컵·제목을 그대로 구움) 위에
        //    본문 상자 · 「★ 선물 고르기」 · 알약 4개(선택 = 노랑+반짝) · 분홍 결제 버튼 · 「다음에 할게요」 · 바닥 안내를 시안 좌표(720×1280)에 올린다.
        private static RectTransform _page;
        private static readonly Text[] _giftSparks = new Text[8];
        private static readonly Color BoxFill = new Color(0.945f, 0.894f, 0.804f);      // 241,228,205
        private static readonly Color BoxLine = new Color(0.635f, 0.478f, 0.353f);      // 162,122,90
        private static readonly Color PillLine = new Color(0.337f, 0.184f, 0.094f);     // 86,47,24
        private static readonly Color PillOnFill = new Color(1f, 0.906f, 0.51f);         // 255,231,130
        private static readonly Color PinkFill = new Color(0.941f, 0.369f, 0.596f);      // 240,94,152
        private static readonly Color PinkLine = new Color(0.62f, 0.16f, 0.36f);
        private static readonly Color LaterFill = new Color(0.824f, 0.796f, 0.753f);     // 210,203,192

        /// 시안 좌표(720×1280, 왼쪽 위 원점)로 놓기.
        private static RectTransform Place(RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x0, -y0); rt.sizeDelta = new Vector2(x1 - x0, y1 - y0);
            return rt;
        }
        /// 테두리 있는 둥근 상자(바깥 = 테두리색, 안쪽 = 채움).
        private static Image Box(Transform parent, string name, Color fill, Color line, int width, int radius)
        {
            var outer = CoastUiArt.Panel(parent, name, line, radius);
            var inner = CoastUiArt.Panel(outer.transform, "Fill", fill, Mathf.Max(2, radius - width));
            var r = inner.rectTransform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(width, width); r.offsetMax = new Vector2(-width, -width);
            return outer;
        }

        public static void Open(Action onClose = null)
        {
            Close();
            _onClose = onClose;
            Donation.MarkPopupSeen();
            _canvas = CoastUiCanvas.Create("DonateCanvas", 472);
            _root = CoastUiCanvas.Root(_canvas);
            var pad = CoastUiCanvas.HudPad;
            // 74차(사용자: 「기부부탁은 pop-up 형태로 닫을 수 있어야 한다」): 전체 화면 페이지 → **팝업**.
            //   ① 어두운 딤 — 카드 바깥을 누르면 닫힌다  ② 카드는 부모(안전 영역) 안으로 통째 축소(PopupFit)해서
            //   어떤 화면 비율에서도 오른쪽 위 ✕ 와 아래 「다음에 할게요」가 잘리지 않는다  ③ Esc·안드로이드 뒤로 키.
            var dim = CoastHudLayout.MakeImage(_root, "Dim", Vector2.zero, Vector2.one, new Vector2(-pad - 400f, -pad - 400f), new Vector2(pad + 400f, pad + 400f), new Color(0.05f, 0.04f, 0.10f, 0.72f));
            dim.raycastTarget = true;
            var dimBtn = dim.gameObject.AddComponent<Button>(); dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(() => { CoastPrefs.Vibrate(); Close(); });

            var page = new GameObject("Page", typeof(RectTransform), typeof(PopupFit), typeof(CloseKeys)).GetComponent<RectTransform>();
            page.SetParent(_root, false); page.anchorMin = page.anchorMax = new Vector2(0.5f, 0.5f); page.pivot = new Vector2(0.5f, 0.5f); page.sizeDelta = new Vector2(720f, 1280f); page.anchoredPosition = Vector2.zero;
            _page = page;
            // 시안 배경(카드 전체) — 없으면 크림 카드. 카드는 레이캐스트를 먹어 딤(닫기)으로 탭이 새지 않게 한다.
            var bgTex = ArtAssets.LoadTexture("UI_Donate_BG");
            // 86차-2 규칙: 시안 그림(UI_Donate_BG)에는 프레임·컵·꽃만 굽고, 글자는 전부 코드에서 Loc.T로 그린다(다국어).
            if (bgTex != null)
            {
                var art = CoastHudLayout.MakeImage(page, "Art", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
                art.sprite = CoastUiArt.AsSprite(bgTex); art.preserveAspect = false; art.raycastTarget = true;
            }
            else
            {
                // 그림이 없을 때: 크림 카드 + 컵을 따로
                var card = Box(page, "Card", new Color(0.99f, 0.93f, 0.84f), BoxLine, 4, 40);
                var crt = card.rectTransform; crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one; crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;
                card.raycastTarget = true;
                var cup = ArtAssets.LoadTexture("UI_Donate_Cup");
                var head = CoastHudLayout.MakeImage(page, "Cup", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.white);
                Place(head.rectTransform, 300f, 112f, 420f, 250f);
                if (cup != null) { head.sprite = CoastUiArt.AsSprite(cup); head.preserveAspect = true; } else head.color = new Color(1f, 0.86f, 0.45f);
                head.raycastTarget = false;
            }

            // 제목(항상 글자로) — 시안: 금색 굵은 글씨 + 갈색 외곽선, 커피콩 ☕
            var title = CoastHudLayout.MakeText(page, "Title", Loc.T("커피 한 잔 값, 기부 부탁드려요 ☕", "A coffee's worth — please donate ☕"), 27, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(title.rectTransform, 70f, 258f, 650f, 326f); title.color = new Color(1f, 0.80f, 0.22f); title.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(title, new Color(0.40f, 0.18f, 0.05f), 3f);
            title.resizeTextForBestFit = true; title.resizeTextMinSize = 16; title.resizeTextMaxSize = CoastHudLayout.Scaled(27); title.raycastTarget = false;

            // 본문 상자
            var box = Box(page, "BodyBox", BoxFill, BoxLine, 3, 26); Place(box.rectTransform, 101f, 343f, 620f, 660f);
            string bodyKo = "이 게임은 잠깐 잠깐 하는 러닝 게임이에요!\n광고와 연결이 끊기는 게 짜증나서 기부만으로 만들었어요.\n그래서 광고도, 강제 결제도 없어요.\n\n" +
                            "또 하나의 목적은 K-POP을 전 세계에 널리 알리는 것.\n가끔 듀오 우리히히의 노래를 잠깐만 들려줄게요.\n\n" +
                            "다만 꾸준한 업데이트를 위해 커피 한 잔 값(" + Donation.PriceLabel + ") 정도 기부해 주시면 더 감사하겠습니다. 기부하신 분께는 작은 선물이 있어요 — 여러 번 기부하셔도 좋아요.";
            string bodyEn = "This is a quick pick-up-and-play runner!\nAds and dropped connections were annoying, so it runs on donations only.\nNo ads, no forced purchases.\n\n" +
                            "The other goal is to spread K-POP worldwide.\nNow and then you'll hear a bit of the duo Woohee & Heesi.\n\n" +
                            "To keep the updates coming, a coffee's worth (" + Donation.PriceLabel + ") would mean a lot. Donors get a small gift — and you can donate more than once.";
            var body = CoastHudLayout.MakeText(box.transform, "Body", Loc.T(bodyKo, bodyEn), 14, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(28f, 16f), new Vector2(-24f, -18f));
            body.color = new Color(0.24f, 0.16f, 0.10f); body.fontStyle = FontStyle.Bold; body.horizontalOverflow = HorizontalWrapMode.Wrap; body.verticalOverflow = VerticalWrapMode.Truncate; body.lineSpacing = 1.15f;
            body.resizeTextForBestFit = true; body.resizeTextMinSize = 12; body.resizeTextMaxSize = CoastHudLayout.Scaled(14);

            // 선물 고르기
            var gl = CoastHudLayout.MakeText(page, "GiftLabel", Loc.T("★ 선물 고르기", "★ Pick your gift"), 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(gl.rectTransform, 101f, 680f, 620f, 728f); gl.color = new Color(0.22f, 0.13f, 0.06f); gl.fontStyle = FontStyle.Bold;
            string[] giftKo = { "♫  히든 트랙 2곡 (레코드 + K-POP 편)", "★  모든 게임 열림 (히든 패스코드)", "♥  아무것도 안 받을래요", "♫  OST 잠금해제 (레코드 전곡)" };
            string[] giftEn = { "♫  2 hidden tracks (records + K-POP run)", "★  Everything unlocked (hidden passcode)", "♥  Nothing, thanks", "♫  Unlock the OST (all records)" };
            Donation.Gift[] kinds = { Donation.Gift.HiddenTrack, Donation.Gift.UnlockAll, Donation.Gift.None, Donation.Gift.Ost };   // 74차: ④ OST
            float[] py = { 735f, 813f, 888f, 963f }; float[] ph = { 62f, 59f, 59f, 59f };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                var pill = Box(page, "Gift" + i, PillOff, PillLine, 3, 30); Place(pill.rectTransform, 101f, py[i], 620f, py[i] + ph[i]); pill.raycastTarget = true;
                var b = pill.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None;
                b.onClick.AddListener(() => { CoastPrefs.Vibrate(); _gift = kinds[idx]; RefreshGifts(); });
                var t = CoastHudLayout.MakeText(pill.transform, "T", Loc.T(giftKo[i], giftEn[i]), 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(16f, 3f), new Vector2(-16f, 0f));
                t.color = new Color(0.16f, 0.10f, 0.05f); t.fontStyle = FontStyle.Bold; t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = CoastHudLayout.Scaled(22);
                _giftPills[i] = pill;
                // 선택 반짝이(왼쪽 위·오른쪽 아래)
                for (int k = 0; k < 2; k++)
                {
                    var sp = CoastHudLayout.MakeText(pill.transform, "Spark" + k, "✦", 22, TextAnchor.MiddleCenter, k == 0 ? new Vector2(0f, 1f) : new Vector2(1f, 0f), k == 0 ? new Vector2(0f, 1f) : new Vector2(1f, 0f), k == 0 ? new Vector2(-22f, -12f) : new Vector2(-2f, -28f), k == 0 ? new Vector2(10f, 20f) : new Vector2(30f, 4f));
                    sp.color = new Color(1f, 0.93f, 0.45f); sp.raycastTarget = false; CoastUiArt.OutlineText(sp, new Color(0.6f, 0.4f, 0.05f, 0.8f), 1.2f);
                    _giftSparks[i * 2 + k] = sp;
                }
            }
            _gift = Donation.Gift.HiddenTrack; RefreshGifts();

            // 결제 버튼(분홍)
            var pay = Box(page, "Pay", PinkFill, PinkLine, 3, 30); Place(pay.rectTransform, 98f, 1051f, 623f, 1126f); pay.raycastTarget = true;
            var lip = CoastUiArt.Panel(pay.transform, "Lip", new Color(0.72f, 0.20f, 0.44f), 26); lip.raycastTarget = false;
            var lrt0 = lip.rectTransform; lrt0.anchorMin = Vector2.zero; lrt0.anchorMax = new Vector2(1f, 0.28f); lrt0.offsetMin = new Vector2(3f, 3f); lrt0.offsetMax = new Vector2(-3f, 0f);
            var gloss = CoastUiArt.Panel(pay.transform, "Gloss", new Color(1f, 1f, 1f, 0.22f), 22); gloss.raycastTarget = false;
            var grt = gloss.rectTransform; grt.anchorMin = new Vector2(0f, 0.55f); grt.anchorMax = new Vector2(1f, 1f); grt.offsetMin = new Vector2(10f, 0f); grt.offsetMax = new Vector2(-10f, -6f);
            _payBtn = pay.gameObject.AddComponent<Button>(); _payBtn.transition = Selectable.Transition.None;
            _payBtn.onClick.AddListener(Pay);
            var pt = CoastHudLayout.MakeText(pay.transform, "T", Loc.T($"☕ 커피 한 잔 기부하기 · {Donation.PriceLabel}", $"☕ Buy me a coffee · {Donation.PriceLabel}"), 28, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 4f), new Vector2(0f, 2f));
            pt.color = Color.white; pt.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(pt, new Color(0.45f, 0.05f, 0.22f, 0.9f), 2.2f);
            pt.resizeTextForBestFit = true; pt.resizeTextMinSize = 16; pt.resizeTextMaxSize = CoastHudLayout.Scaled(28);

            // 다음에 / 바닥 안내 / 결제 상태
            var later = Box(page, "Later", LaterFill, new Color(0.62f, 0.58f, 0.52f), 2, 18); Place(later.rectTransform, 248f, 1185f, 473f, 1221f); later.raycastTarget = true;
            var lb = later.gameObject.AddComponent<Button>(); lb.transition = Selectable.Transition.None; lb.onClick.AddListener(() => { CoastPrefs.Vibrate(); Close(); });
            var lt = CoastHudLayout.MakeText(later.transform, "T", Loc.T("다음에 할게요", "Maybe later"), 16, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
            lt.color = new Color(0.16f, 0.10f, 0.05f); lt.fontStyle = FontStyle.Bold;
            _cups = CoastHudLayout.MakeText(page, "Cups", "", 12, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(_cups.rectTransform, 60f, 1234f, 660f, 1260f); _cups.color = new Color(0.30f, 0.20f, 0.12f); _cups.fontStyle = FontStyle.Bold;
            _cups.resizeTextForBestFit = true; _cups.resizeTextMinSize = 10; _cups.resizeTextMaxSize = CoastHudLayout.Scaled(12);
            _status = CoastHudLayout.MakeText(page, "Status", "", 13, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(_status.rectTransform, 80f, 1130f, 640f, 1182f); _status.color = new Color(0.1f, 0.5f, 0.25f); _status.fontStyle = FontStyle.Bold; _status.horizontalOverflow = HorizontalWrapMode.Wrap;
            RefreshCups();

            // 74차: 오른쪽 위 ✕ — 맨 마지막에 붙여 언제나 제일 위에 그려지고 제일 먼저 눌린다.
            var x = CoastUiArt.GlossyPill(page, "X", new Color(0.55f, 0.58f, 0.66f), 18, 5);
            Place(x.rectTransform, 612f, 22f, 690f, 100f);
            x.raycastTarget = true;
            var xb = x.gameObject.AddComponent<Button>(); xb.transition = Selectable.Transition.None; xb.targetGraphic = x;
            xb.onClick.AddListener(() => { CoastPrefs.Vibrate(); Close(); });
            var xt = CoastHudLayout.MakeText(x.rectTransform, "T", "✕", 34, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 3f), Vector2.zero);
            xt.color = Color.white; xt.fontStyle = FontStyle.Bold; xt.raycastTarget = false;
            CoastUiArt.OutlineText(xt, new Color(0f, 0f, 0f, 0.4f), 1.5f);
        }

        private static void RefreshGifts()
        {
            Donation.Gift[] kinds = { Donation.Gift.HiddenTrack, Donation.Gift.UnlockAll, Donation.Gift.None, Donation.Gift.Ost };
            for (int i = 0; i < 4; i++)
            {
                if (_giftPills[i] == null) continue;
                bool on = kinds[i] == _gift;
                var fill = _giftPills[i].transform.Find("Fill"); var fi = fill != null ? fill.GetComponent<Image>() : null;
                if (fi != null) fi.color = on ? PillOnFill : PillOff;
                for (int k = 0; k < 2; k++) if (_giftSparks[i * 2 + k] != null) _giftSparks[i * 2 + k].gameObject.SetActive(on);
            }
        }

        private static void RefreshCups()
        {
            if (_cups == null) return;
            int n = Donation.Cups;
            string extra = "";
            if (Donation.HiddenTrack) extra += Loc.T(" · 히든 트랙 열림", " · hidden tracks on");
            if (Donation.AllOpen) extra += Loc.T($" · 패스코드 {Donation.DonorPasscode}", $" · passcode {Donation.DonorPasscode}");
            _cups.text = n > 0 ? Loc.T($"Google Play 결제 · 자율 기부 · 지금까지 {n}잔 ☕ 고마워요{extra}", $"Google Play billing · voluntary · {n} cup(s) so far ☕ thank you{extra}") : Loc.T("Google Play 결제 · 자율 기부 · 아직 0잔 — 끊김 없이 안전해요", "Google Play billing · voluntary · 0 cups so far — safe and seamless");
        }

        private static void Pay()
        {
            if (_payBtn == null) return;
            CoastPrefs.Vibrate();
            _payBtn.interactable = false;
            if (_status != null) _status.text = Loc.T("결제 창을 여는 중…", "Opening store…");
            var gift = _gift;
            Donation.Donate(gift, ok =>
            {
                if (_canvas == null) return;
                _payBtn.interactable = true;
                if (!ok) { _status.text = Loc.T("결제가 취소됐거나 실패했어요. 괜찮아요!", "Payment cancelled or failed — that's okay!"); _status.color = new Color(0.6f, 0.2f, 0.2f); return; }
                _status.color = new Color(0.1f, 0.5f, 0.25f);
                switch (gift)
                {
                    case Donation.Gift.HiddenTrack: _status.text = Loc.T("고마워요! ♪ 히든 트랙 2곡이 레코드와 K-POP 런에 열렸어요", "Thank you! ♪ 2 hidden tracks unlocked in Records and the K-POP run"); break;
                    case Donation.Gift.UnlockAll: _status.text = Loc.T($"고마워요! ★ 모든 게임이 열렸어요\n히든 패스코드 {Donation.DonorPasscode} — 다른 기기에선 설정 › 비밀코드에 입력", $"Thank you! ★ Everything unlocked\nHidden passcode {Donation.DonorPasscode} — enter it in Settings › Secret code on another device"); break;
                    case Donation.Gift.Ost: _status.text = Loc.T("고마워요! ♫ 레코드의 OST 전곡이 열렸어요", "Thank you! ♫ The whole OST is unlocked in Records"); break;
                    default: _status.text = Loc.T("고마워요! ♥ 그 마음만으로 충분해요", "Thank you! ♥ That means a lot"); break;
                }
                CoastAudioManager.PlayAnywhere(CoastSfx.RankS, 0.7f);
                RefreshCups();
            });
        }

        /// 74차: 시안 카드(720×1280)를 부모 사각형 안에 통째로 넣는다 — 세로가 짧은 16:9에서도 ✕·「다음에 할게요」가 화면 안.
        ///   0.94 를 곱해 딤이 테두리처럼 보이게(= 팝업으로 읽힌다) + 바깥을 눌러 닫을 자리를 남긴다.
        private class PopupFit : MonoBehaviour
        {
            private static readonly Vector2 Design = new Vector2(720f, 1280f);
            private RectTransform _self, _parent;
            private float _lw = -1f, _lh = -1f;
            private void OnEnable() { _self = (RectTransform)transform; _parent = _self.parent as RectTransform; Apply(); }
            private void LateUpdate() { Apply(); }
            private void Apply()
            {
                if (_self == null || _parent == null) return;
                float rw = _parent.rect.width, rh = _parent.rect.height;
                if (rw < 8f || rh < 8f) return;
                if (Mathf.Abs(rw - _lw) < 0.25f && Mathf.Abs(rh - _lh) < 0.25f) return;
                _lw = rw; _lh = rh;
                float s = Mathf.Min(rw / Design.x, rh / Design.y) * 0.94f;
                _self.localScale = new Vector3(s, s, 1f);
            }
        }

        /// 74차: Esc(안드로이드 뒤로 키도 Escape 로 들어온다)·Backspace 로 닫기.
        private class CloseKeys : MonoBehaviour
        {
            private void Update()
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace) || CoastRemoteKeys.Down(KeyCode.Backspace)) Close();
            }
        }

        public static void Close()
        {
            if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null; _root = null; _page = null; _cups = null; _status = null; _payBtn = null;
            var cb = _onClose; _onClose = null; cb?.Invoke();
        }
    }
}
