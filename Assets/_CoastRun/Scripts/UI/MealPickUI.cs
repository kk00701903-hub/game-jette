using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 「밥」 직 먹을 요리 고르기 — 112차 시안(나무 액자·점선·새싹/꽃·취소/먹기 알약) 표준.
    public static class MealPickUI
    {
        private static Canvas _canvas;
        public static bool IsOpen => _canvas != null;

        public static void Open(GameManager gm, Action<string> onPicked, Action onCancel = null)
        {
            Close();
            if (gm?.Save == null) { onCancel?.Invoke(); return; }
            LifeItems.Ensure(gm.Save);
            var list = LifeItems.ListEdible(gm.Save);
            if (list.Count == 0) { onCancel?.Invoke(); return; }

            int sel = 0;
            float listH = list.Count <= 1 ? 0f : Mathf.Min(360f, list.Count * 84f + 8f);
            float h = list.Count <= 1 ? 420f : 280f + listH;
            var crt = StoryPopupKit.Frame("MealPick", 466, new Vector2(620f, h), out _canvas, 40f);

            StoryPopupKit.TitleMeal(crt, Loc.T("오늘 뭐 먹을까?", "What shall we eat?"), 20f);

            Text nameT = null; Text fxT = null; Text badgeT = null;
            RectTransform badgeRt = null;
            Image[] rowImgs = null;

            void ShowDish(int idx)
            {
                sel = idx;
                var item = list[sel];
                string nm = LifeItems.Name(item.def);
                string fx = LifeItems.EffectText(item.def);
                if (string.IsNullOrEmpty(fx)) fx = Loc.T("든든한 한 끼", "A good meal");
                if (nameT != null) nameT.text = "✨ " + nm;
                if (fxT != null) fxT.text = "🌱 " + fx;
                if (badgeT != null) badgeT.text = "x" + item.n;
                // 이름 길이에 맞춰 배지를 이름 오른쪽에
                if (badgeRt != null && nameT != null)
                {
                    float approx = Mathf.Clamp(nm.Length * 18f + 40f, 80f, 220f);
                    badgeRt.anchoredPosition = new Vector2(approx * 0.5f + 28f, badgeRt.anchoredPosition.y);
                }
                if (rowImgs != null)
                    for (int i = 0; i < rowImgs.Length; i++)
                        if (rowImgs[i] != null)
                            rowImgs[i].color = i == sel
                                ? new Color(1f, 0.93f, 0.82f)
                                : new Color(1f, 0.98f, 0.94f, 0.65f);
            }

            // 본문 — 시안처럼 가운데 이름 + 배지 + 효과 줄
            float nameY = list.Count <= 1 ? -150f : -120f;
            nameT = CoastHudLayout.MakeText(crt, "Name", "", 34, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-160f, nameY - 50f), new Vector2(120f, nameY));
            nameT.color = StoryPopupKit.Ink; nameT.fontStyle = FontStyle.Bold; nameT.raycastTarget = false;
            nameT.resizeTextForBestFit = true; nameT.resizeTextMinSize = 18; nameT.resizeTextMaxSize = CoastHudLayout.Scaled(34);

            var badge = CoastUiArt.Panel(crt, "Badge", StoryPopupKit.BadgeOrange, 22); badge.raycastTarget = false;
            badgeRt = badge.rectTransform; badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(0.5f, 1f); badgeRt.pivot = new Vector2(0.5f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(110f, nameY - 25f); badgeRt.sizeDelta = new Vector2(48f, 48f);
            badgeT = CoastHudLayout.MakeText(badgeRt, "T", "x1", 18, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
            badgeT.color = Color.white; badgeT.fontStyle = FontStyle.Bold;
            CoastUiArt.OutlineText(badgeT, new Color(0.45f, 0.2f, 0.1f, 0.45f), 1.2f);

            EventCardKit.Sparkle(crt, new Vector2(0.5f, 1f), new Vector2(-170f, nameY - 20f), 14, StoryPopupKit.Sparkle);
            EventCardKit.Sparkle(crt, new Vector2(0.5f, 1f), new Vector2(-195f, nameY - 8f), 10, StoryPopupKit.Sparkle);

            fxT = CoastHudLayout.MakeText(crt, "Fx", "", 15, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, nameY - 95f), new Vector2(-24f, nameY - 55f));
            fxT.color = StoryPopupKit.Ink; fxT.raycastTarget = false;
            fxT.resizeTextForBestFit = true; fxT.resizeTextMinSize = 11; fxT.resizeTextMaxSize = CoastHudLayout.Scaled(15);

            if (list.Count > 1)
            {
                // 여러 요리 — 아래 목록으로 선택
                nameT.gameObject.SetActive(false);
                badge.gameObject.SetActive(false);
                fxT.gameObject.SetActive(false);
                var listHost = new GameObject("List", typeof(RectTransform)).GetComponent<RectTransform>();
                listHost.SetParent(crt, false);
                listHost.anchorMin = new Vector2(0f, 0f); listHost.anchorMax = new Vector2(1f, 1f);
                listHost.offsetMin = new Vector2(28f, 100f); listHost.offsetMax = new Vector2(-28f, -96f);
                rowImgs = new Image[list.Count];
                float y = 0f;
                for (int i = 0; i < list.Count; i++)
                {
                    int idx = i;
                    var def = list[i].def; int n = list[i].n;
                    var row = CoastUiArt.CutePill(listHost, "R" + i, new Color(1f, 0.98f, 0.94f, 0.65f), 14, 0);
                    rowImgs[i] = row;
                    var rt = row.rectTransform; rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f); rt.anchoredPosition = new Vector2(0f, y); rt.sizeDelta = new Vector2(0f, 76f);
                    row.raycastTarget = true;
                    var nm = CoastHudLayout.MakeText(rt, "N", "✨ " + LifeItems.Name(def), 20, TextAnchor.MiddleLeft,
                        Vector2.zero, Vector2.one, new Vector2(14f, 18f), new Vector2(-64f, 0f));
                    nm.color = StoryPopupKit.Ink; nm.fontStyle = FontStyle.Bold;
                    var b = CoastUiArt.Panel(rt, "B", StoryPopupKit.BadgeOrange, 14); b.raycastTarget = false;
                    var brr = b.rectTransform; brr.anchorMin = brr.anchorMax = new Vector2(1f, 0.5f); brr.pivot = new Vector2(1f, 0.5f);
                    brr.anchoredPosition = new Vector2(-8f, 8f); brr.sizeDelta = new Vector2(40f, 40f);
                    var bt = CoastHudLayout.MakeText(brr, "T", "x" + n, 15, TextAnchor.MiddleCenter,
                        Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
                    bt.color = Color.white; bt.fontStyle = FontStyle.Bold;
                    var ef = CoastHudLayout.MakeText(rt, "E", "🌱 " + LifeItems.EffectText(def), 12, TextAnchor.LowerLeft,
                        Vector2.zero, Vector2.one, new Vector2(14f, 6f), new Vector2(-14f, 34f));
                    ef.color = StoryPopupKit.Ink;
                    row.gameObject.AddComponent<Button>().onClick.AddListener(() => ShowDish(idx));
                    y -= 80f;
                }
            }

            ShowDish(0);

            void Pick()
            {
                CoastPrefs.Vibrate();
                string id = list[sel].def.id;
                Close();
                onPicked?.Invoke(id);
            }

            StoryPopupKit.SoftPill(crt, "Cancel", Loc.T("취소", "Cancel"), StoryPopupKit.SoftBlue,
                new Vector2(-130f, 22f), new Vector2(200f, 56f), () => { Close(); onCancel?.Invoke(); }, backIcon: true);
            StoryPopupKit.SoftPill(crt, "Eat", Loc.T("먹기", "Eat"), StoryPopupKit.SoftPink,
                new Vector2(130f, 22f), new Vector2(200f, 56f), Pick, forkIcon: true);
        }

        /// Dev/캡쳐용 — 세이브와 무관하게 시안 단일 카드(삼계탕 ×2)를 띄운다.
        public static void OpenDemo(Action onClose = null)
        {
            Close();
            var crt = StoryPopupKit.Frame("MealPick", 466, new Vector2(620f, 420f), out _canvas, 40f);
            StoryPopupKit.TitleMeal(crt, Loc.T("오늘 뭐 먹을까?", "What shall we eat?"), 20f);

            const float nameY = -150f;
            var nameT = CoastHudLayout.MakeText(crt, "Name", "✨ " + Loc.T("삼계탕", "Ginseng Chicken Soup"), 34, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-160f, nameY - 50f), new Vector2(100f, nameY));
            nameT.color = StoryPopupKit.Ink; nameT.fontStyle = FontStyle.Bold; nameT.raycastTarget = false;

            var badge = CoastUiArt.Panel(crt, "Badge", StoryPopupKit.BadgeOrange, 22); badge.raycastTarget = false;
            var badgeRt = badge.rectTransform; badgeRt.anchorMin = badgeRt.anchorMax = new Vector2(0.5f, 1f); badgeRt.pivot = new Vector2(0.5f, 0.5f);
            badgeRt.anchoredPosition = new Vector2(120f, nameY - 25f); badgeRt.sizeDelta = new Vector2(48f, 48f);
            var badgeT = CoastHudLayout.MakeText(badgeRt, "T", "x2", 18, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
            badgeT.color = Color.white; badgeT.fontStyle = FontStyle.Bold;
            CoastUiArt.OutlineText(badgeT, new Color(0.45f, 0.2f, 0.1f, 0.45f), 1.2f);

            EventCardKit.Sparkle(crt, new Vector2(0.5f, 1f), new Vector2(-175f, nameY - 18f), 14, StoryPopupKit.Sparkle);
            EventCardKit.Sparkle(crt, new Vector2(0.5f, 1f), new Vector2(-198f, nameY - 6f), 10, StoryPopupKit.Sparkle);

            var fx = CoastHudLayout.MakeText(crt, "Fx",
                "🌱 " + Loc.T("배부름+40 · 컨디션+18 · 스트레스-5 · 체력+5", "Full+40 · Cond+18 · Stress-5 · Sta+5"),
                15, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, nameY - 95f), new Vector2(-24f, nameY - 55f));
            fx.color = StoryPopupKit.Ink; fx.raycastTarget = false;

            StoryPopupKit.SoftPill(crt, "Cancel", Loc.T("취소", "Cancel"), StoryPopupKit.SoftBlue,
                new Vector2(-130f, 22f), new Vector2(200f, 56f), () => { Close(); onClose?.Invoke(); }, backIcon: true);
            StoryPopupKit.SoftPill(crt, "Eat", Loc.T("먹기", "Eat"), StoryPopupKit.SoftPink,
                new Vector2(130f, 22f), new Vector2(200f, 56f), () => { Close(); onClose?.Invoke(); }, forkIcon: true);
        }

        public static void Close()
        {
            if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null;
        }
    }
}
