using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 보유 아이템 가방 — 장바구니 옆 버튼. 조리·사용·목록.
    public static class InventoryUI
    {
        private static Canvas _canvas;
        private static RectTransform _root;
        private static GameManager _gm;
        private static Action _onClose;
        private static LifeItemCat? _filter;
        private static bool _cookMode;
        public static bool IsOpen => _canvas != null;

        private static readonly Color Navy = new Color(0.16f, 0.14f, 0.34f);

        public static void Open(GameManager gm, Action onClose = null, bool cookMode = false)
        {
            Close();
            _gm = gm; _onClose = onClose; _cookMode = cookMode; _filter = cookMode ? LifeItemCat.Ingredient : null;
            if (gm?.Save == null) return;
            LifeItems.Ensure(gm.Save);
            _canvas = CoastUiCanvas.Create("InvCanvas", 464);
            _root = CoastUiCanvas.Root(_canvas);
            var pad = CoastUiCanvas.HudPad;
            var dim = CoastHudLayout.MakeImage(_root, "Dim", Vector2.zero, Vector2.one, new Vector2(-pad - 400f, -pad - 400f), new Vector2(pad + 400f, pad + 400f), new Color(0.05f, 0.04f, 0.10f, 0.72f));
            dim.raycastTarget = true;
            dim.gameObject.AddComponent<Button>().onClick.AddListener(Close);
            Build();
        }

        private static void Build()
        {
            var save = _gm?.Save; if (save == null || _root == null) return;
            for (int i = _root.childCount - 1; i >= 0; i--)
            {
                var c = _root.GetChild(i);
                if (c != null && c.name == "Card") UnityEngine.Object.Destroy(c.gameObject);
            }
            LifeItems.Ensure(save);

            var card = CoastUiArt.Panel(_root, "Card", new Color(0.98f, 0.80f, 0.32f), 34); card.raycastTarget = true;
            var crt = card.rectTransform; crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f); crt.pivot = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = new Vector2(0f, 20f); crt.sizeDelta = new Vector2(660f, 980f);
            var inner = CoastUiArt.Panel(crt, "Inner", new Color(0.996f, 0.96f, 0.88f), 30); inner.raycastTarget = false;
            inner.rectTransform.anchorMin = Vector2.zero; inner.rectTransform.anchorMax = Vector2.one;
            inner.rectTransform.offsetMin = new Vector2(6f, 6f); inner.rectTransform.offsetMax = new Vector2(-6f, -6f);

            string title = _cookMode ? Loc.T("🍳 방 조리 — 재료를 요리로", "🍳 Cook — ingredients → meals") : Loc.T("보유 아이템", "Inventory");
            var head = CoastHudLayout.MakeText(crt, "Head", title, 28, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -70f), new Vector2(-12f, -18f));
            head.color = Navy; head.fontStyle = FontStyle.Bold;

            // 필터 칩
            LifeItemCat[] cats = { LifeItemCat.Ingredient, LifeItemCat.BasicDish, LifeItemCat.PremiumDish, LifeItemCat.Medicine, LifeItemCat.Care };
            float chipW = 112f;
            for (int i = 0; i < cats.Length; i++)
            {
                var cat = cats[i];
                bool on = _filter.HasValue && _filter.Value == cat;
                var pill = CoastUiArt.GlossyPill(crt, "F" + i, on ? new Color(1f, 0.86f, 0.35f) : new Color(0.90f, 0.86f, 0.78f), 14, 4);
                var prt = pill.rectTransform; prt.anchorMin = prt.anchorMax = new Vector2(0f, 1f); prt.pivot = new Vector2(0f, 1f);
                prt.anchoredPosition = new Vector2(18f + i * (chipW + 6f), -78f); prt.sizeDelta = new Vector2(chipW, 40f); pill.raycastTarget = true;
                var t = CoastHudLayout.MakeText(prt, "T", LifeItems.CatLabel(cat), 13, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                t.color = Navy; t.fontStyle = FontStyle.Bold; t.resizeTextForBestFit = true; t.resizeTextMinSize = 10; t.resizeTextMaxSize = 13;
                var b = pill.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None;
                LifeItemCat pick = cat;
                b.onClick.AddListener(() => { _filter = _filter == pick ? null : pick; if (_cookMode) _filter = LifeItemCat.Ingredient; Build(); });
            }

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
            scrollGo.transform.SetParent(crt, false);
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0f, 0f); srt.anchorMax = new Vector2(1f, 1f);
            srt.offsetMin = new Vector2(16f, 70f); srt.offsetMax = new Vector2(-16f, -130f);
            scrollGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            var sr = scrollGo.GetComponent<ScrollRect>(); sr.horizontal = false; sr.vertical = true; sr.movementType = ScrollRect.MovementType.Clamped;
            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(srt, false);
            content.anchorMin = new Vector2(0f, 1f); content.anchorMax = new Vector2(1f, 1f); content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(0f, 0f);
            sr.content = content; sr.viewport = srt;

            var owned = LifeItems.ListOwned(save, _filter);
            if (_cookMode) owned = LifeItems.ListOwned(save, LifeItemCat.Ingredient);
            float y = 0f; const float rowH = 108f;
            if (owned.Count == 0)
            {
                var empty = CoastHudLayout.MakeText(content, "Empty",
                    _cookMode ? Loc.T("재료가 없어. 상점에서 쌀·채소·고기를 사 와.", "No ingredients. Buy rice/veg/meat at the shop.")
                              : Loc.T("가방이 비었어. 상점에서 채워 보자.", "Bag empty. Stock up at the shop."),
                    18, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -80f), new Vector2(-8f, -20f));
                empty.color = new Color(0.40f, 0.36f, 0.42f); empty.horizontalOverflow = HorizontalWrapMode.Wrap;
                y = 100f;
            }
            for (int i = 0; i < owned.Count; i++)
            {
                var def = owned[i].def; int n = owned[i].n;
                Row(content, def, n, -y); y += rowH + 8f;
            }
            content.sizeDelta = new Vector2(0f, Mathf.Max(y + 20f, 200f));

            var close = CoastUiArt.GlossyPill(crt, "Close", new Color(0.55f, 0.57f, 0.64f), 18, 6);
            var cl = close.rectTransform; cl.anchorMin = cl.anchorMax = new Vector2(0.5f, 0f); cl.pivot = new Vector2(0.5f, 0f);
            cl.anchoredPosition = new Vector2(0f, 16f); cl.sizeDelta = new Vector2(280f, 52f); close.raycastTarget = true;
            var ct = CoastHudLayout.MakeText(cl, "T", Loc.T("닫기", "Close"), 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 3f), Vector2.zero);
            ct.color = Color.white; ct.fontStyle = FontStyle.Bold;
            close.gameObject.AddComponent<Button>().onClick.AddListener(Close);
        }

        private static void Row(RectTransform content, LifeItemDef def, int n, float y)
        {
            var row = CoastUiArt.Panel(content, "R_" + def.id, Color.white, 16); row.raycastTarget = false;
            var rt = row.rectTransform; rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f); rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y); rt.sizeDelta = new Vector2(600f, 108f);
            Color fill = def.cat switch
            {
                LifeItemCat.Ingredient => new Color(0.92f, 0.96f, 0.84f),
                LifeItemCat.BasicDish => new Color(1f, 0.94f, 0.82f),
                LifeItemCat.PremiumDish => new Color(1f, 0.88f, 0.92f),
                LifeItemCat.Medicine => new Color(0.86f, 0.92f, 1f),
                _ => new Color(0.94f, 0.90f, 0.98f)
            };
            var fin = CoastUiArt.Panel(row.transform, "F", fill, 14); fin.raycastTarget = false;
            fin.rectTransform.anchorMin = Vector2.zero; fin.rectTransform.anchorMax = Vector2.one;
            fin.rectTransform.offsetMin = new Vector2(3f, 3f); fin.rectTransform.offsetMax = new Vector2(-3f, -3f);

            var frame = CoastUiArt.Panel(row.transform, "Art", Color.white, 12); frame.raycastTarget = false;
            var frt = frame.rectTransform; frt.anchorMin = frt.anchorMax = new Vector2(0f, 0.5f); frt.pivot = new Vector2(0f, 0.5f);
            frt.anchoredPosition = new Vector2(10f, 0f); frt.sizeDelta = new Vector2(78f, 78f);
            var tex = ArtAssets.LoadTexture(def.art);
            if (tex != null)
            {
                var im = new GameObject("I", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                im.transform.SetParent(frame.transform, false); im.sprite = CoastUiArt.AsSprite(tex); im.preserveAspect = true; im.raycastTarget = false;
                var irt = im.rectTransform; irt.anchorMin = Vector2.zero; irt.anchorMax = Vector2.one; irt.offsetMin = new Vector2(4f, 4f); irt.offsetMax = new Vector2(-4f, -4f);
            }

            var nm = CoastHudLayout.MakeText(row.transform, "N", $"{LifeItems.Name(def)} ×{n}", 22, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(100f, -48f), new Vector2(-150f, -8f));
            nm.color = Navy; nm.fontStyle = FontStyle.Bold;
            string sub = LifeItems.EffectText(def); if (string.IsNullOrEmpty(sub)) sub = LifeItems.Blurb(def);
            var bl = CoastHudLayout.MakeText(row.transform, "B", sub, 13, TextAnchor.UpperLeft, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(100f, 8f), new Vector2(-150f, -50f));
            bl.color = new Color(0.28f, 0.24f, 0.36f); bl.horizontalOverflow = HorizontalWrapMode.Wrap;
            bl.resizeTextForBestFit = true; bl.resizeTextMinSize = 10; bl.resizeTextMaxSize = 13;

            string actLabel = null; Action act = null;
            if (def.cat == LifeItemCat.Ingredient && LifeItems.CanCook(_gm.Save, def.id))
            {
                actLabel = Loc.T("조리", "Cook");
                string id = def.id;
                act = () =>
                {
                    if (LifeItems.Cook(_gm.Save, id, 1))
                    {
                        var src = LifeItems.Get(id);
                        var to = src.HasValue ? LifeItems.Get(src.Value.cookTo) : null;
                        CoastToast.Show(Loc.T($"조리 완료! → {(to.HasValue ? to.Value.nameKo : "?")}", $"Cooked → {(to.HasValue ? to.Value.nameEn : "?")}"));
                        CoastAudioManager.PlayAnywhere(CoastSfx.Coin, 0.45f); _gm.Persist(); Build();
                    }
                };
            }
            else if (def.edible)
            {
                actLabel = Loc.T("먹기", "Eat");
                string id = def.id;
                act = () =>
                {
                    if (LifeItems.Eat(_gm.Save, id))
                    {
                        CoastToast.Show(Loc.T($"{LifeItems.Name(def)} 먹음!", $"Ate {LifeItems.Name(def)}!"));
                        CoastAudioManager.PlayAnywhere(CoastSfx.Coin, 0.45f); _gm.Persist(); Build();
                    }
                };
            }
            else if (def.usable)
            {
                actLabel = Loc.T("사용", "Use");
                string id = def.id;
                act = () =>
                {
                    if (LifeItems.Use(_gm.Save, id))
                    {
                        CoastToast.Show(Loc.T($"{LifeItems.Name(def)} 사용!", $"Used {LifeItems.Name(def)}!"));
                        CoastAudioManager.PlayAnywhere(CoastSfx.Coin, 0.45f); _gm.Persist(); Build();
                    }
                };
            }

            if (act != null)
            {
                var btn = CoastUiArt.GlossyPill(row.transform, "Act", new Color(0.35f, 0.70f, 0.55f), 16, 5);
                var brt = btn.rectTransform; brt.anchorMin = brt.anchorMax = new Vector2(1f, 0.5f); brt.pivot = new Vector2(1f, 0.5f);
                brt.anchoredPosition = new Vector2(-12f, 0f); brt.sizeDelta = new Vector2(120f, 48f); btn.raycastTarget = true;
                var lt = CoastHudLayout.MakeText(brt, "T", actLabel, 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 3f), Vector2.zero);
                lt.color = Color.white; lt.fontStyle = FontStyle.Bold;
                var b = btn.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None;
                b.onClick.AddListener(() => { CoastPrefs.Vibrate(); act(); });
            }
        }

        public static void Close()
        {
            if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null; _root = null;
            var cb = _onClose; _onClose = null; cb?.Invoke();
        }
    }
}
