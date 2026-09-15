using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 「밥」 직 먹을 요리 고르기 — 선택 시 1개 소진 후 onPicked.
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

            _canvas = CoastUiCanvas.Create("MealPick", 466);
            var root = CoastUiCanvas.Root(_canvas);
            var pad = CoastUiCanvas.HudPad;
            var dim = CoastHudLayout.MakeImage(root, "Dim", Vector2.zero, Vector2.one, new Vector2(-pad - 400f, -pad - 400f), new Vector2(pad + 400f, pad + 400f), new Color(0.05f, 0.04f, 0.10f, 0.75f));
            dim.raycastTarget = true;

            var card = CoastUiArt.Panel(root, "Card", new Color(0.98f, 0.80f, 0.32f), 28); card.raycastTarget = true;
            var crt = card.rectTransform; crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.sizeDelta = new Vector2(620f, Mathf.Min(820f, 160f + list.Count * 100f)); crt.anchoredPosition = new Vector2(0f, 40f);
            var inner = CoastUiArt.Panel(crt, "In", new Color(0.996f, 0.96f, 0.88f), 24); inner.raycastTarget = false;
            inner.rectTransform.anchorMin = Vector2.zero; inner.rectTransform.anchorMax = Vector2.one;
            inner.rectTransform.offsetMin = new Vector2(5f, 5f); inner.rectTransform.offsetMax = new Vector2(-5f, -5f);

            var head = CoastHudLayout.MakeText(crt, "H", Loc.T("🍽 오늘 뭐 먹을까?", "🍽 What to eat?"), 26, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -58f), new Vector2(-12f, -12f));
            head.color = new Color(0.18f, 0.14f, 0.32f); head.fontStyle = FontStyle.Bold;

            float y = -70f;
            for (int i = 0; i < list.Count; i++)
            {
                var def = list[i].def; int n = list[i].n;
                var row = CoastUiArt.GlossyPill(crt, "M" + i, def.cat == LifeItemCat.PremiumDish ? new Color(1f, 0.82f, 0.88f) : new Color(1f, 0.93f, 0.75f), 16, 5);
                var rt = row.rectTransform; rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, y); rt.sizeDelta = new Vector2(560f, 88f); row.raycastTarget = true;
                var nm = CoastHudLayout.MakeText(rt, "N", $"{LifeItems.Name(def)} ×{n}", 22, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(18f, 4f), new Vector2(-140f, 0f));
                nm.color = new Color(0.18f, 0.14f, 0.32f); nm.fontStyle = FontStyle.Bold;
                var ef = CoastHudLayout.MakeText(rt, "E", LifeItems.EffectText(def), 13, TextAnchor.LowerLeft, Vector2.zero, Vector2.one, new Vector2(18f, 8f), new Vector2(-140f, 36f));
                ef.color = new Color(0.35f, 0.30f, 0.40f);
                var eat = CoastUiArt.GlossyPill(rt, "Eat", new Color(1f, 0.45f, 0.55f), 14, 4);
                var ert = eat.rectTransform; ert.anchorMin = ert.anchorMax = new Vector2(1f, 0.5f); ert.pivot = new Vector2(1f, 0.5f);
                ert.anchoredPosition = new Vector2(-12f, 0f); ert.sizeDelta = new Vector2(120f, 48f); eat.raycastTarget = true;
                var et = CoastHudLayout.MakeText(ert, "T", Loc.T("먹기", "Eat"), 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 3f), Vector2.zero);
                et.color = Color.white; et.fontStyle = FontStyle.Bold;
                string id = def.id;
                void Pick()
                {
                    CoastPrefs.Vibrate();
                    Close();
                    onPicked?.Invoke(id);
                }
                eat.gameObject.AddComponent<Button>().onClick.AddListener(Pick);
                row.gameObject.AddComponent<Button>().onClick.AddListener(Pick);
                y -= 96f;
            }

            var cancel = CoastUiArt.GlossyPill(crt, "Cancel", new Color(0.55f, 0.57f, 0.64f), 16, 5);
            var crt2 = cancel.rectTransform; crt2.anchorMin = crt2.anchorMax = new Vector2(0.5f, 0f); crt2.pivot = new Vector2(0.5f, 0f);
            crt2.anchoredPosition = new Vector2(0f, 14f); crt2.sizeDelta = new Vector2(240f, 48f); cancel.raycastTarget = true;
            var ct = CoastHudLayout.MakeText(crt2, "T", Loc.T("취소", "Cancel"), 20, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 3f), Vector2.zero);
            ct.color = Color.white; ct.fontStyle = FontStyle.Bold;
            cancel.gameObject.AddComponent<Button>().onClick.AddListener(() => { Close(); onCancel?.Invoke(); });
        }

        public static void Close()
        {
            if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null;
        }
    }
}
