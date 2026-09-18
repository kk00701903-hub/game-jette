using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 112차(사용자 시안): 스토리모드 팝업 표준 — 나무 액자 + 크림 속지 + 점선 테두리 +
    ///   모서리 새싹/꽃 + 제목(양옆 아이콘) + 파스텔 알약 버튼(갈색 글씨·테두리).
    public static class StoryPopupKit
    {
        public static readonly Color WoodOuter = new Color(0.72f, 0.55f, 0.38f);       // 바깥 갈색 윤곽
        public static readonly Color WoodFrame = new Color(0.93f, 0.82f, 0.62f);       // 두꺼운 나무 테
        public static readonly Color WoodFrameDark = new Color(0.82f, 0.68f, 0.48f);
        public static readonly Color Cream = new Color(0.996f, 0.97f, 0.90f);
        public static readonly Color Ink = new Color(0.32f, 0.22f, 0.14f);
        public static readonly Color Dash = new Color(0.55f, 0.40f, 0.28f, 0.85f);
        public static readonly Color Leaf = new Color(0.45f, 0.72f, 0.38f);
        public static readonly Color LeafDark = new Color(0.32f, 0.58f, 0.28f);
        public static readonly Color FlowerYellow = new Color(0.98f, 0.82f, 0.28f);
        public static readonly Color FlowerGreen = new Color(0.55f, 0.78f, 0.42f);
        public static readonly Color SoftBlue = new Color(0.72f, 0.82f, 0.92f);
        public static readonly Color SoftPink = new Color(0.95f, 0.72f, 0.78f);
        public static readonly Color BadgeOrange = new Color(0.98f, 0.62f, 0.42f);
        public static readonly Color Sparkle = new Color(1f, 0.86f, 0.28f);

        /// 딤 + 나무 액자. 되돌림: 크림 속지(콘텐츠 부모).
        public static RectTransform Frame(string canvasName, int order, Vector2 size, out Canvas canvas, float y = 0f)
        {
            canvas = CoastUiCanvas.Create(canvasName, order);
            var root = CoastUiCanvas.Root(canvas);
            float pad = CoastUiCanvas.HudPad;
            var dim = CoastHudLayout.MakeImage(root, "Dim", Vector2.zero, Vector2.one,
                new Vector2(-pad - 400f, -pad - 400f), new Vector2(pad + 400f, pad + 400f),
                new Color(0.05f, 0.04f, 0.08f, 0.72f));
            dim.raycastTarget = true;

            // 바깥 갈색 윤곽
            var outline = CoastUiArt.Panel(root, "Outline", WoodOuter, 36); outline.raycastTarget = false;
            var ort = outline.rectTransform;
            ort.anchorMin = ort.anchorMax = new Vector2(0.5f, 0.5f); ort.pivot = new Vector2(0.5f, 0.5f);
            ort.anchoredPosition = new Vector2(0f, y); ort.sizeDelta = size + new Vector2(10f, 10f);

            // 두꺼운 나무 테(시안: 밝은 목재 링이 분명히 보이게)
            var frame = CoastUiArt.Panel(root, "Frame", WoodFrame, 34); frame.raycastTarget = true;
            var frt = frame.rectTransform;
            frt.anchorMin = frt.anchorMax = new Vector2(0.5f, 0.5f); frt.pivot = new Vector2(0.5f, 0.5f);
            frt.anchoredPosition = new Vector2(0f, y); frt.sizeDelta = size;
            var woodIn = CoastUiArt.Panel(frt, "WoodIn", WoodFrameDark, 30); woodIn.raycastTarget = false;
            Stretch(woodIn.rectTransform, 4f);
            var woodMid = CoastUiArt.Panel(woodIn.transform, "WoodMid", WoodFrame, 28); woodMid.raycastTarget = false;
            Stretch(woodMid.rectTransform, 3f);

            // 크림 속지 — 안쪽 여백을 넓혀 나무 테가 두껍게 보임
            var card = CoastUiArt.Panel(woodMid.transform, "Card", Cream, 24); card.raycastTarget = true;
            Stretch(card.rectTransform, 22f);
            var crt = card.rectTransform;

            DashedBorder(crt, 10f);
            EventCardKit.Sparkle(crt, new Vector2(0.14f, 0.62f), Vector2.zero, 14, Sparkle);
            EventCardKit.Sparkle(crt, new Vector2(0.86f, 0.55f), Vector2.zero, 11, Sparkle);

            // 모서리 장식 — 위 새싹, 아래 꽃(시안)
            Sprout(frt, new Vector2(0.02f, 0.98f), -1f);
            Sprout(frt, new Vector2(0.98f, 0.98f), 1f);
            Flower(frt, new Vector2(0.03f, 0.02f), FlowerYellow);
            Flower(frt, new Vector2(0.97f, 0.02f), FlowerGreen);

            return crt;
        }

        /// 식사 팝업 제목 — 왼쪽 수저·포크, 오른쪽 김 나는 그릇(이모지 폰트 의존 없음).
        public static Text TitleMeal(RectTransform card, string text, float yTop = 22f)
        {
            float h = 56f;
            DrawCutleryIcon(card, new Vector2(0f, 1f), new Vector2(52f, -yTop - h * 0.5f));
            DrawBowlIcon(card, new Vector2(1f, 1f), new Vector2(-52f, -yTop - h * 0.5f));
            var t = CoastHudLayout.MakeText(card, "Title", text, 30, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(100f, -yTop - h), new Vector2(-100f, -yTop));
            t.color = Ink; t.fontStyle = FontStyle.Bold; t.raycastTarget = false;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 16; t.resizeTextMaxSize = CoastHudLayout.Scaled(30);
            return t;
        }

        public static Text Title(RectTransform card, string text, string leftIcon, string rightIcon, float yTop = 28f)
        {
            float h = 56f;
            if (!string.IsNullOrEmpty(leftIcon))
            {
                var L = CoastHudLayout.MakeText(card, "TitleL", leftIcon, 30, TextAnchor.MiddleCenter,
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -yTop - h), new Vector2(88f, -yTop));
                L.color = new Color(0.55f, 0.55f, 0.58f); L.raycastTarget = false;
            }
            if (!string.IsNullOrEmpty(rightIcon))
            {
                var R = CoastHudLayout.MakeText(card, "TitleR", rightIcon, 28, TextAnchor.MiddleCenter,
                    new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-88f, -yTop - h), new Vector2(-28f, -yTop));
                R.color = new Color(0.55f, 0.40f, 0.28f); R.raycastTarget = false;
            }
            var t = CoastHudLayout.MakeText(card, "Title", text, 30, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(90f, -yTop - h), new Vector2(-90f, -yTop));
            t.color = Ink; t.fontStyle = FontStyle.Bold; t.raycastTarget = false;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 16; t.resizeTextMaxSize = CoastHudLayout.Scaled(30);
            return t;
        }

        /// 파스텔 알약 — 갈색 테두리 + 광택 + 갈색 글씨(시안 취소/먹기).
        public static Button SoftPill(RectTransform parent, string name, string label, Color fill, Vector2 pos, Vector2 size, Action onClick, bool backIcon = false, bool forkIcon = false)
        {
            var edge = CoastUiArt.Panel(parent, name + "Edge", Ink, 24); edge.raycastTarget = false;
            var ert = edge.rectTransform; ert.anchorMin = ert.anchorMax = new Vector2(0.5f, 0f); ert.pivot = new Vector2(0.5f, 0f);
            ert.anchoredPosition = pos; ert.sizeDelta = size + new Vector2(5f, 5f);

            var pill = CoastUiArt.CutePill(parent, name, fill, 22, 0);
            var rt = pill.rectTransform; rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f); rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = pos; rt.sizeDelta = size; pill.raycastTarget = true;
            var gloss = CoastUiArt.Panel(rt, "Gloss", new Color(1f, 1f, 1f, 0.42f), 10); gloss.raycastTarget = false;
            var gr = gloss.rectTransform; gr.anchorMin = new Vector2(0.08f, 0.55f); gr.anchorMax = new Vector2(0.92f, 0.92f);
            gr.offsetMin = gr.offsetMax = Vector2.zero;

            float textLeft = 12f;
            if (backIcon || forkIcon)
            {
                var icBg = CoastUiArt.Panel(rt, "IcBg", new Color(1f, 1f, 1f, 0.85f), 14); icBg.raycastTarget = false;
                var ibr = icBg.rectTransform; ibr.anchorMin = ibr.anchorMax = new Vector2(0f, 0.5f); ibr.pivot = new Vector2(0f, 0.5f);
                ibr.anchoredPosition = new Vector2(10f, 0f); ibr.sizeDelta = new Vector2(32f, 32f);
                if (backIcon)
                {
                    var ar = CoastHudLayout.MakeText(ibr, "Ar", "←", 20, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 1f), Vector2.zero);
                    ar.color = Ink; ar.fontStyle = FontStyle.Bold; ar.raycastTarget = false;
                }
                else
                    DrawMiniFork(ibr);
                textLeft = 48f;
            }
            var t = CoastHudLayout.MakeText(rt, "T", label, 22, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(textLeft, 2f), new Vector2(-12f, 0f));
            t.color = Ink; t.fontStyle = FontStyle.Bold; t.raycastTarget = false;

            var btn = pill.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.targetGraphic = pill;
            btn.onClick.AddListener(() => { CoastPrefs.Vibrate(); onClick?.Invoke(); });
            return btn;
        }

        /// 「이름」 + 주황 원형 ×N 배지.
        public static void NameWithCount(RectTransform parent, string name, int count, float yTop, float height = 56f)
        {
            string label = name;
            var t = CoastHudLayout.MakeText(parent, "Name", label, 36, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(40f, -yTop - height), new Vector2(-120f, -yTop));
            t.color = Ink; t.fontStyle = FontStyle.Bold; t.raycastTarget = false;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 18; t.resizeTextMaxSize = CoastHudLayout.Scaled(36);

            var badge = CoastUiArt.Panel(parent, "Badge", BadgeOrange, 22); badge.raycastTarget = false;
            var br = badge.rectTransform; br.anchorMin = br.anchorMax = new Vector2(0.5f, 1f); br.pivot = new Vector2(0.5f, 1f);
            br.anchoredPosition = new Vector2(Mathf.Min(140f, 28f + name.Length * 14f), -yTop - 6f);
            br.sizeDelta = new Vector2(48f, 48f);
            var bt = CoastHudLayout.MakeText(br, "T", "x" + count, 18, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
            bt.color = Color.white; bt.fontStyle = FontStyle.Bold; bt.raycastTarget = false;
            CoastUiArt.OutlineText(bt, new Color(0.45f, 0.2f, 0.1f, 0.45f), 1.2f);

            // 왼쪽 반짝이(시안)
            EventCardKit.Sparkle(parent, new Vector2(0.5f, 1f), new Vector2(-150f, -yTop - height * 0.55f), 14, Sparkle);
            EventCardKit.Sparkle(parent, new Vector2(0.5f, 1f), new Vector2(-175f, -yTop - height * 0.35f), 10, Sparkle);
        }

        public static Text EffectLine(RectTransform parent, string text, float yTop)
        {
            var t = CoastHudLayout.MakeText(parent, "Fx", "🌱 " + text, 16, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -yTop - 36f), new Vector2(-28f, -yTop));
            t.color = Ink; t.raycastTarget = false;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 11; t.resizeTextMaxSize = CoastHudLayout.Scaled(16);
            return t;
        }

        private static void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset); rt.offsetMax = new Vector2(-inset, -inset);
        }

        private static void DashedBorder(RectTransform card, float inset)
        {
            var host = new GameObject("DashHost", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(card, false);
            Stretch(host, inset);
            void EdgeH(float yNorm, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    float t0 = (i + 0.15f) / count;
                    float t1 = (i + 0.75f) / count;
                    var im = CoastUiArt.Panel(host, "DH", Dash, 2); im.raycastTarget = false;
                    var r = im.rectTransform;
                    r.anchorMin = new Vector2(t0, yNorm); r.anchorMax = new Vector2(t1, yNorm);
                    r.offsetMin = new Vector2(0f, -1.2f); r.offsetMax = new Vector2(0f, 1.2f);
                }
            }
            void EdgeV(float xNorm, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    float t0 = (i + 0.15f) / count;
                    float t1 = (i + 0.75f) / count;
                    var im = CoastUiArt.Panel(host, "DV", Dash, 2); im.raycastTarget = false;
                    var r = im.rectTransform;
                    r.anchorMin = new Vector2(xNorm, t0); r.anchorMax = new Vector2(xNorm, t1);
                    r.offsetMin = new Vector2(-1.2f, 0f); r.offsetMax = new Vector2(1.2f, 0f);
                }
            }
            EdgeH(1f, 14); EdgeH(0f, 14); EdgeV(0f, 10); EdgeV(1f, 10);
        }

        private static void Sprout(RectTransform frame, Vector2 anchor, float side)
        {
            // 두 잎 새싹 — 액자 위 모서리 밖으로
            for (int i = 0; i < 2; i++)
            {
                float ang = side * (32f + i * 40f);
                var leaf = CoastUiArt.Panel(frame, "Leaf", i == 0 ? Leaf : LeafDark, 12); leaf.raycastTarget = false;
                var r = leaf.rectTransform;
                r.anchorMin = r.anchorMax = anchor; r.pivot = new Vector2(0.5f, 0.1f);
                r.anchoredPosition = new Vector2(side * (6f + i * 6f), 14f + i * 3f);
                r.sizeDelta = new Vector2(20f, 34f);
                r.localRotation = Quaternion.Euler(0f, 0f, ang);
            }
            var stem = CoastUiArt.Panel(frame, "Stem", LeafDark, 3); stem.raycastTarget = false;
            var s = stem.rectTransform; s.anchorMin = s.anchorMax = anchor; s.pivot = new Vector2(0.5f, 0f);
            s.anchoredPosition = new Vector2(0f, -2f); s.sizeDelta = new Vector2(5f, 16f);
        }

        private static void Flower(RectTransform frame, Vector2 anchor, Color col)
        {
            for (int i = 0; i < 5; i++)
            {
                float ang = i * 72f * Mathf.Deg2Rad + 0.2f;
                var p = CoastHudLayout.MakeImage(frame, "Petal", anchor, anchor,
                    new Vector2(Mathf.Cos(ang) * 10f - 7f, Mathf.Sin(ang) * 10f - 7f),
                    new Vector2(Mathf.Cos(ang) * 10f + 7f, Mathf.Sin(ang) * 10f + 7f), col);
                p.raycastTarget = false; p.sprite = CoastUiArt.RoundedRect(20);
            }
            var c = CoastHudLayout.MakeImage(frame, "Center", anchor, anchor,
                new Vector2(-5f, -5f), new Vector2(5f, 5f), new Color(1f, 0.92f, 0.45f));
            c.raycastTarget = false; c.sprite = CoastUiArt.RoundedRect(16);
        }

        private static void DrawCutleryIcon(RectTransform parent, Vector2 anchor, Vector2 pos)
        {
            var host = new GameObject("Cutlery", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(parent, false);
            host.anchorMin = host.anchorMax = anchor; host.pivot = new Vector2(0.5f, 0.5f);
            host.anchoredPosition = pos; host.sizeDelta = new Vector2(48f, 42f);
            var steel = new Color(0.62f, 0.66f, 0.72f);
            var dark = new Color(0.38f, 0.40f, 0.46f);
            var sb = CoastUiArt.Panel(host, "SpoonB", steel, 10); sb.raycastTarget = false;
            Rect(sb.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(2f, -18f), new Vector2(18f, -2f));
            var ss = CoastUiArt.Panel(host, "SpoonS", steel, 3); ss.raycastTarget = false;
            Rect(ss.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(6f, 2f), new Vector2(14f, 26f));
            var fs = CoastUiArt.Panel(host, "ForkS", dark, 3); fs.raycastTarget = false;
            Rect(fs.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-16f, 2f), new Vector2(-8f, 24f));
            for (int i = 0; i < 3; i++)
            {
                var ti = CoastUiArt.Panel(host, "T" + i, dark, 2); ti.raycastTarget = false;
                Rect(ti.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f + i * 6f, -18f), new Vector2(-16f + i * 6f, -2f));
            }
        }

        private static void DrawBowlIcon(RectTransform parent, Vector2 anchor, Vector2 pos)
        {
            var host = new GameObject("Bowl", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(parent, false);
            host.anchorMin = host.anchorMax = anchor; host.pivot = new Vector2(0.5f, 0.5f);
            host.anchoredPosition = pos; host.sizeDelta = new Vector2(44f, 40f);
            var bowl = new Color(0.62f, 0.42f, 0.28f);
            var b = CoastUiArt.Panel(host, "B", bowl, 14); b.raycastTarget = false;
            Rect(b.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
            b.rectTransform.pivot = new Vector2(0.5f, 0f); b.rectTransform.anchoredPosition = new Vector2(0f, 2f); b.rectTransform.sizeDelta = new Vector2(34f, 18f);
            for (int i = 0; i < 3; i++)
            {
                var steam = CoastUiArt.Panel(host, "S" + i, new Color(0.85f, 0.88f, 0.92f, 0.9f), 4); steam.raycastTarget = false;
                var sr = steam.rectTransform; sr.anchorMin = sr.anchorMax = new Vector2(0.5f, 0f); sr.pivot = new Vector2(0.5f, 0f);
                sr.anchoredPosition = new Vector2(-8f + i * 8f, 20f + (i % 2) * 4f); sr.sizeDelta = new Vector2(5f, 10f);
            }
        }

        private static void DrawMiniFork(RectTransform parent)
        {
            var dark = Ink;
            var fs = CoastUiArt.Panel(parent, "F", dark, 2); fs.raycastTarget = false;
            Rect(fs.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
            fs.rectTransform.pivot = new Vector2(0.5f, 0f); fs.rectTransform.anchoredPosition = new Vector2(0f, 4f); fs.rectTransform.sizeDelta = new Vector2(3f, 14f);
            for (int i = 0; i < 3; i++)
            {
                var ti = CoastUiArt.Panel(parent, "T" + i, dark, 2); ti.raycastTarget = false;
                Rect(ti.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
                ti.rectTransform.pivot = new Vector2(0.5f, 1f); ti.rectTransform.anchoredPosition = new Vector2(-6f + i * 6f, -4f); ti.rectTransform.sizeDelta = new Vector2(3f, 10f);
            }
        }

        private static void Rect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = offMin; rt.offsetMax = offMax;
        }
    }
}
