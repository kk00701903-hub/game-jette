using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 60차(사용자 시안 「대회 미달…」): 육성 이벤트 팝업 공통 카드 — 크림 카드(은은한 흰 테두리·모서리 ✦), 젤리 글씨 제목,
    ///   점·✦ 구분선, 동그란 아이콘 + 글 줄(오른쪽 알약 배지), 흰 안내 상자, 아이콘 달린 큰 버튼 두 개.
    ///   ContestResultUI·ContestIntroUI·GameOverUI 가 쓴다.
    internal static class EventCardKit
    {
        public static readonly Color Navy = new Color(0.16f, 0.14f, 0.30f);
        public static readonly Color Cream = new Color(0.99f, 0.96f, 0.90f);
        public static readonly Color Ink = new Color(0.22f, 0.18f, 0.16f);

        /// 캔버스 + 어둠 + 카드. 되돌림: 카드 RectTransform.
        public static RectTransform Card(string canvasName, int order, Vector2 size, out Canvas canvas, float y = 0f)
        {
            canvas = CoastUiCanvas.Create(canvasName, order);
            var root = CoastUiCanvas.Root(canvas);
            float pad = CoastUiCanvas.HudPad;
            var dim = CoastHudLayout.MakeImage(root, "Dim", Vector2.zero, Vector2.one, new Vector2(-pad - 400f, -pad - 400f), new Vector2(pad + 400f, pad + 400f), new Color(0.04f, 0.03f, 0.08f, 0.76f));
            dim.raycastTarget = true;
            // 바깥 빛 테두리(크림보다 밝은 흰 알약을 살짝 크게)
            var glow = CoastUiArt.Panel(root, "Glow", new Color(1f, 1f, 1f, 0.35f), 34); glow.raycastTarget = false;
            var grt = glow.rectTransform; grt.anchorMin = grt.anchorMax = new Vector2(0.5f, 0.5f); grt.pivot = new Vector2(0.5f, 0.5f); grt.anchoredPosition = new Vector2(0f, y); grt.sizeDelta = size + new Vector2(16f, 16f);
            var card = CoastUiArt.CutePill(root, "Card", Cream, 30, 5);
            var crt = card.rectTransform; crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f); crt.pivot = new Vector2(0.5f, 0.5f); crt.anchoredPosition = new Vector2(0f, y); crt.sizeDelta = size; card.raycastTarget = true;
            Sparkle(crt, new Vector2(0f, 1f), new Vector2(26f, -26f), 22, new Color(1f, 1f, 1f, 0.95f));
            Sparkle(crt, new Vector2(1f, 1f), new Vector2(-30f, -40f), 14, new Color(1f, 1f, 1f, 0.9f));
            Sparkle(crt, new Vector2(0f, 0f), new Vector2(28f, 60f), 12, new Color(1f, 1f, 1f, 0.9f));
            Sparkle(crt, new Vector2(1f, 0f), new Vector2(-26f, 30f), 20, new Color(1f, 1f, 1f, 0.95f));
            return crt;
        }

        public static void Sparkle(RectTransform parent, Vector2 anchor, Vector2 pos, int size, Color col)
        {
            var t = CoastHudLayout.MakeText(parent, "Sp", "✦", size, TextAnchor.MiddleCenter, anchor, anchor, new Vector2(pos.x - size, pos.y - size), new Vector2(pos.x + size, pos.y + size));
            t.color = col; t.raycastTarget = false; CoastUiArt.OutlineText(t, new Color(1f, 0.85f, 0.5f, 0.5f), 1f);
        }

        /// 젤리 글씨 제목(두꺼운 진한 테두리 + 위쪽 하이라이트 겹 글자). yTop = 카드 위에서 내려온 거리(양수).
        public static Text JellyTitle(RectTransform card, string text, Color fill, Color edge, float yTop, float height, int size = 52)
        {
            // 그림자
            var sh = CoastHudLayout.MakeText(card, "TitleShadow", text, size, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -yTop - height - 4f), new Vector2(4f, -yTop - 4f));
            sh.color = new Color(edge.r * 0.6f, edge.g * 0.6f, edge.b * 0.6f, 0.55f); sh.fontStyle = FontStyle.Bold; sh.raycastTarget = false;
            sh.resizeTextForBestFit = true; sh.resizeTextMinSize = 20; sh.resizeTextMaxSize = CoastHudLayout.Scaled(size);
            var t = CoastHudLayout.MakeText(card, "Title", text, size, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -yTop - height), new Vector2(0f, -yTop));
            t.color = fill; t.fontStyle = FontStyle.Bold; t.raycastTarget = false; CoastUiArt.OutlineText(t, edge, 3.2f);
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 20; t.resizeTextMaxSize = CoastHudLayout.Scaled(size);
            // 하이라이트(윗부분만 살짝 밝게 — 같은 글자를 위로 2px 올려 반투명 흰색)
            var hl = CoastHudLayout.MakeText(card, "TitleHl", text, size, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -yTop - height + 3f), new Vector2(0f, -yTop + 3f));
            hl.color = new Color(1f, 1f, 1f, 0.22f); hl.fontStyle = FontStyle.Bold; hl.raycastTarget = false;
            hl.resizeTextForBestFit = true; hl.resizeTextMinSize = 20; hl.resizeTextMaxSize = CoastHudLayout.Scaled(size);
            return t;
        }

        /// 「· · · ✦ ✦ ✦ · · ·」 구분선.
        public static void Divider(RectTransform card, float yTop)
        {
            var t = CoastHudLayout.MakeText(card, "Div", "· · · · · · ✦ ✦ ✦ · · · · · ·", 16, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -yTop - 22f), new Vector2(-20f, -yTop));
            t.color = new Color(0.95f, 0.75f, 0.35f); t.raycastTarget = false;
            var l = CoastUiArt.Panel(card, "DivLine", new Color(0.95f, 0.85f, 0.55f, 0.5f), 2); l.raycastTarget = false;
            l.rectTransform.anchorMin = new Vector2(0.1f, 1f); l.rectTransform.anchorMax = new Vector2(0.9f, 1f); l.rectTransform.offsetMin = new Vector2(0f, -yTop - 13f); l.rectTransform.offsetMax = new Vector2(0f, -yTop - 11f);
        }

        /// 동그란 아이콘 + 글 한 줄(+ 오른쪽 알약 배지). 카드 위에서 yTop 만큼 내려온 자리, 높이 h.
        public static Text IconRow(RectTransform parent, string icon, Color iconBg, string text, float yTop, float h, int size = 24, string badge = null, Color? badgeCol = null, float left = 36f, float right = 36f)
        {
            var circle = CoastUiArt.Panel(parent, "IcBg", iconBg, (int)(h * 0.5f)); circle.raycastTarget = false;
            var cr = circle.rectTransform; cr.anchorMin = cr.anchorMax = new Vector2(0f, 1f); cr.pivot = new Vector2(0f, 1f); cr.anchoredPosition = new Vector2(left, -yTop); cr.sizeDelta = new Vector2(h, h);
            var sp = CoastUiArt.Art(icon);
            if (sp != null)
            {
                var im = new GameObject("Ic", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                im.transform.SetParent(cr, false); im.sprite = sp; im.preserveAspect = true; im.raycastTarget = false;
                im.rectTransform.anchorMin = im.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); im.rectTransform.sizeDelta = new Vector2(h * 0.68f, h * 0.68f);
            }
            float badgeW = badge != null ? 96f : 0f;
            var t = CoastHudLayout.MakeText(parent, "Row", text, size, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(left + h + 16f, -yTop - h), new Vector2(-right - badgeW, -yTop));
            t.color = Ink; t.fontStyle = FontStyle.Bold; t.raycastTarget = false; t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 11; t.resizeTextMaxSize = CoastHudLayout.Scaled(size);
            if (badge != null)
            {
                var pill = CoastUiArt.GlossyPill(parent, "Badge", badgeCol ?? new Color(0.98f, 0.45f, 0.35f), 16, 5); pill.raycastTarget = false;
                var pr = pill.rectTransform; pr.anchorMin = pr.anchorMax = new Vector2(1f, 1f); pr.pivot = new Vector2(1f, 1f); pr.anchoredPosition = new Vector2(-right, -yTop - (h - 36f) * 0.5f); pr.sizeDelta = new Vector2(88f, 36f);
                var bt = CoastHudLayout.MakeText(pr, "T", badge, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
                bt.color = Color.white; bt.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(bt, new Color(0f, 0f, 0f, 0.35f), 1.2f);
            }
            return t;
        }

        /// 흰 안내 상자(둥근 흰 패널). 되돌림: 상자 RectTransform — 안에 IconRow 를 쌓는다.
        public static RectTransform InfoBox(RectTransform card, float yTop, float height, float side = 28f)
        {
            var box = CoastUiArt.Panel(card, "Info", new Color(1f, 1f, 1f, 0.92f), 22); box.raycastTarget = false;
            var r = box.rectTransform; r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f); r.pivot = new Vector2(0.5f, 1f);
            r.offsetMin = new Vector2(side, -yTop - height); r.offsetMax = new Vector2(-side, -yTop);
            return r;
        }

        /// 아이콘 달린 큰 버튼(젤리 알약 + ✦).
        public static Button IconButton(RectTransform parent, string name, string icon, string label, Color col, Vector2 anchor, Vector2 pos, Vector2 size, Action onClick, int font = 24)
        {
            var b = CoastUiArt.GlossyPill(parent, name, col, 26, 9);
            var rt = b.rectTransform; rt.anchorMin = rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = size; b.raycastTarget = true;
            float textLeft = 10f;
            var sp = CoastUiArt.Art(icon);
            if (sp != null)
            {
                var im = new GameObject("Ic", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                im.transform.SetParent(rt, false); im.sprite = sp; im.preserveAspect = true; im.raycastTarget = false;
                im.rectTransform.anchorMin = im.rectTransform.anchorMax = new Vector2(0f, 0.5f); im.rectTransform.pivot = new Vector2(0f, 0.5f);
                im.rectTransform.anchoredPosition = new Vector2(16f, 1f); im.rectTransform.sizeDelta = new Vector2(size.y * 0.5f, size.y * 0.5f);
                textLeft = 16f + size.y * 0.5f + 6f;
            }
            var t = CoastHudLayout.MakeText(rt, "T", label, font, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(textLeft, 3f), new Vector2(-10f, 0f));
            t.color = Color.white; t.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(t, new Color(0f, 0f, 0f, 0.35f), 1.4f); t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 11; t.resizeTextMaxSize = CoastHudLayout.Scaled(font);
            Sparkle(rt, new Vector2(1f, 1f), new Vector2(-14f, -10f), 14, new Color(1f, 1f, 1f, 0.95f));
            Sparkle(rt, new Vector2(0f, 0f), new Vector2(14f, 10f), 10, new Color(1f, 1f, 1f, 0.85f));
            var bt = b.gameObject.AddComponent<Button>(); bt.transition = Selectable.Transition.None;
            bt.onClick.AddListener(() => { CoastPrefs.Vibrate(); onClick?.Invoke(); });
            return bt;
        }

        // ── 스토리 모드 선택 팝업 (시안: 헬섬 이벤트 / 어디 알바?) ──

        public static readonly Color SoftPink = new Color(0.99f, 0.90f, 0.93f);
        public static readonly Color SoftBlue = new Color(0.89f, 0.95f, 0.99f);
        public static readonly Color TagPink = new Color(0.95f, 0.45f, 0.62f);
        public static readonly Color BrownInk = new Color(0.30f, 0.20f, 0.18f);
        public static readonly Color GoldJob = new Color(1f, 0.82f, 0.38f);
        public static readonly Color LavenderJob = new Color(0.72f, 0.62f, 0.95f);

        /// 상단 분홍 알약 태그 (예: 「✨ 헬섬 이벤트」).
        public static Text HellsumTag(RectTransform card, string label, float yTop = 18f)
        {
            var pill = CoastUiArt.CutePill(card, "Tag", TagPink, 16, 0);
            var pr = pill.rectTransform; pr.anchorMin = pr.anchorMax = new Vector2(0.5f, 1f); pr.pivot = new Vector2(0.5f, 1f);
            pr.anchoredPosition = new Vector2(0f, -yTop); pr.sizeDelta = new Vector2(220f, 36f); pill.raycastTarget = false;
            var t = CoastHudLayout.MakeText(pr, "T", label, 15, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(4f, 1f), new Vector2(-4f, -1f));
            t.color = Color.white; t.fontStyle = FontStyle.Bold; t.raycastTarget = false;
            return t;
        }

        /// A/B 본문 블록 (연분홍·연파랑 + 아이콘 + 「A:」 헤더).
        public static RectTransform ChoiceBlock(RectTransform card, string name, bool isA, string icon, string body, float yTop, float height)
        {
            Color bg = isA ? SoftPink : SoftBlue;
            Color head = isA ? new Color(0.85f, 0.28f, 0.48f) : new Color(0.28f, 0.48f, 0.82f);
            var box = CoastUiArt.CutePill(card, name, bg, 18, 0);
            var r = box.rectTransform; r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f); r.pivot = new Vector2(0.5f, 1f);
            r.offsetMin = new Vector2(28f, -yTop - height); r.offsetMax = new Vector2(-28f, -yTop); box.raycastTarget = false;

            var circle = CoastUiArt.Panel(r, "IcBg", head, 14); circle.raycastTarget = false;
            var cr = circle.rectTransform; cr.anchorMin = cr.anchorMax = new Vector2(0f, 1f); cr.pivot = new Vector2(0f, 1f);
            cr.anchoredPosition = new Vector2(14f, -12f); cr.sizeDelta = new Vector2(28f, 28f);
            var sp = CoastUiArt.Art(icon);
            if (sp != null)
            {
                var im = new GameObject("Ic", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                im.transform.SetParent(cr, false); im.sprite = sp; im.preserveAspect = true; im.color = Color.white; im.raycastTarget = false;
                im.rectTransform.anchorMin = im.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); im.rectTransform.sizeDelta = new Vector2(18f, 18f);
            }
            var letter = CoastHudLayout.MakeText(r, "Letter", isA ? "A:" : "B:", 18, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(50f, -40f), new Vector2(-14f, -10f));
            letter.color = head; letter.fontStyle = FontStyle.Bold; letter.raycastTarget = false;

            var bodyT = CoastHudLayout.MakeText(r, "Body", body, 15, TextAnchor.UpperLeft,
                Vector2.zero, Vector2.one, new Vector2(16f, 12f), new Vector2(-16f, -44f));
            bodyT.color = BrownInk; bodyT.horizontalOverflow = HorizontalWrapMode.Wrap;
            bodyT.verticalOverflow = VerticalWrapMode.Truncate;
            bodyT.resizeTextForBestFit = true; bodyT.resizeTextMinSize = CoastHudLayout.MinFontSize;
            bodyT.resizeTextMaxSize = CoastHudLayout.Scaled(15); bodyT.raycastTarget = false;
            return r;
        }

        /// 하단 나란히 A/B 버튼 — 연한 배경 + 갈색 글씨(시안).
        public static Button SoftChoiceButton(RectTransform parent, string name, string icon, string label, bool isA, Vector2 pos, Vector2 size, Action onClick)
        {
            Color fill = isA ? SoftPink : SoftBlue;
            Color edge = isA ? new Color(0.92f, 0.55f, 0.68f) : new Color(0.55f, 0.72f, 0.92f);
            var glow = CoastUiArt.Panel(parent, name + "Edge", edge, 22); glow.raycastTarget = false;
            var ge = glow.rectTransform; ge.anchorMin = ge.anchorMax = new Vector2(0.5f, 0f); ge.pivot = new Vector2(0.5f, 0f);
            ge.anchoredPosition = pos; ge.sizeDelta = size + new Vector2(6f, 6f);

            var b = CoastUiArt.CutePill(parent, name, fill, 20, 3);
            var rt = b.rectTransform; rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = pos; rt.sizeDelta = size; b.raycastTarget = true;

            float textLeft = 8f;
            var sp = CoastUiArt.Art(icon);
            if (sp != null)
            {
                var im = new GameObject("Ic", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                im.transform.SetParent(rt, false); im.sprite = sp; im.preserveAspect = true; im.color = BrownInk; im.raycastTarget = false;
                im.rectTransform.anchorMin = im.rectTransform.anchorMax = new Vector2(0f, 0.5f); im.rectTransform.pivot = new Vector2(0f, 0.5f);
                im.rectTransform.anchoredPosition = new Vector2(14f, 1f); im.rectTransform.sizeDelta = new Vector2(size.y * 0.42f, size.y * 0.42f);
                textLeft = 14f + size.y * 0.42f + 4f;
            }
            var t = CoastHudLayout.MakeText(rt, "T", label, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(textLeft, 2f), new Vector2(-10f, 0f));
            t.color = BrownInk; t.fontStyle = FontStyle.Bold; t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 11; t.resizeTextMaxSize = CoastHudLayout.Scaled(18);
            var bt = b.gameObject.AddComponent<Button>(); bt.transition = Selectable.Transition.None;
            bt.onClick.AddListener(() => { CoastPrefs.Vibrate(); onClick?.Invoke(); });
            return bt;
        }

        /// 「어디 알바?」식 젤리 배너 제목.
        public static Text PickBanner(RectTransform parent, string text, Color fill, Color edge, float yTop = 70f)
        {
            var bar = CoastUiArt.GlossyPill(parent, "Banner", fill, 28, 10);
            var br = bar.rectTransform; br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f); br.pivot = new Vector2(0.5f, 1f);
            br.anchoredPosition = new Vector2(0f, -yTop); br.sizeDelta = new Vector2(420f, 64f); bar.raycastTarget = false;
            return JellyTitle(br, text, Color.white, edge, 4f, 56f, 34);
        }

        /// 알바/쉼/놀기 선택 카드 (탭 + 큰 제목 + 워터마크 ✦).
        /// note: 카드 아래 작은 흰 알약(효과 요약). 74차 — 「이 카드가 스트레스를 쌓는지 푸는지」를 고를 때 보이게.
        public static Button ThemedPickCard(RectTransform parent, string name, string tab, string title, Color fill, Color tabCol, Vector2 pos, Vector2 size, Action onClick, string note = null)
        {
            var card = CoastUiArt.GlossyPill(parent, name, fill, 26, 10);
            var rt = card.rectTransform; rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size; card.raycastTarget = true;

            var tabPill = CoastUiArt.CutePill(rt, "Tab", tabCol, 12, 0);
            var tr = tabPill.rectTransform; tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 1f); tr.pivot = new Vector2(0.5f, 0.5f);
            tr.anchoredPosition = new Vector2(0f, 6f); tr.sizeDelta = new Vector2(88f, 28f); tabPill.raycastTarget = false;
            var tt = CoastHudLayout.MakeText(tr, "T", tab, 14, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            tt.color = Color.white; tt.fontStyle = FontStyle.Bold; tt.raycastTarget = false;

            Sparkle(rt, new Vector2(0.12f, 0.78f), Vector2.zero, 18, new Color(1f, 1f, 1f, 0.35f));
            Sparkle(rt, new Vector2(0.85f, 0.25f), Vector2.zero, 14, new Color(1f, 1f, 1f, 0.28f));
            Sparkle(rt, new Vector2(0.2f, 0.2f), Vector2.zero, 12, new Color(1f, 1f, 1f, 0.22f));

            var nm = CoastHudLayout.MakeText(rt, "N", title, 26, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(18f, 20f), new Vector2(-18f, -24f));
            nm.color = Color.white; nm.fontStyle = FontStyle.Bold; nm.horizontalOverflow = HorizontalWrapMode.Wrap;
            nm.resizeTextForBestFit = true; nm.resizeTextMinSize = 14; nm.resizeTextMaxSize = CoastHudLayout.Scaled(28);
            CoastUiArt.OutlineText(nm, new Color(0.12f, 0.10f, 0.28f, 0.85f), 2.4f);

            if (!string.IsNullOrEmpty(note))
            {
                nm.rectTransform.offsetMin = new Vector2(18f, 86f);   // 제목을 위로 밀고 아래에 효과 알약
                var pill = CoastUiArt.CutePill(rt, "Note", new Color(1f, 1f, 1f, 0.94f), 14, 0);
                var prt = pill.rectTransform; prt.anchorMin = new Vector2(0f, 0f); prt.anchorMax = new Vector2(1f, 0f); prt.pivot = new Vector2(0.5f, 0f);
                prt.anchoredPosition = new Vector2(0f, 16f); prt.sizeDelta = new Vector2(-28f, 62f); pill.raycastTarget = false;
                var nt = CoastHudLayout.MakeText(prt, "T", note, 15, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f));
                nt.color = new Color(0.24f, 0.20f, 0.34f); nt.fontStyle = FontStyle.Bold; nt.raycastTarget = false;
                nt.horizontalOverflow = HorizontalWrapMode.Wrap;
                nt.resizeTextForBestFit = true; nt.resizeTextMinSize = 10; nt.resizeTextMaxSize = CoastHudLayout.Scaled(15);
            }

            var btn = card.gameObject.AddComponent<Button>(); btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { CoastPrefs.Vibrate(); onClick?.Invoke(); });
            return btn;
        }
    }
}
