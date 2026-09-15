using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 53차(사용자): 육성 **상태창** — 레벨·칭호·경험치 바, 스탯 6개(체력·순발력·매력·감성·평판·스트레스), 돈/코인(k 표기),
    ///   레벨 보너스(코인 +%), K-POP 파밍 안내, 경험치 얻는 법. TamaRaisingUI 의 [상태창] 버튼에서.
    public static class StatusUI
    {
        private static Canvas _canvas;
        private static Action _onClose;
        public static bool IsOpen => _canvas != null;
        private static readonly Color Navy = new Color(0.16f, 0.14f, 0.30f);
        private static readonly Color Cream = new Color(0.99f, 0.96f, 0.88f);

        /// 86차(사용자 시안 「캐릭터 정보」): 전체 화면 페이지 — 시안 그림(UI_Status_BG: 꽃 테두리·제목·얼굴·스탯 아이콘/라벨·「생활」·닫기 버튼을 그대로 구움) 위에
        ///   레벨·이름·칭호 / EXP 바 / 대회 배너 / 스탯 막대 7개 + 숫자 / 돈·코인 알약 / 생활 막대 2개 / 바닥 재고 줄을 시안 좌표(720×1280)에 올린다.
        private static RectTransform Place(RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f); rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x0, -y0); rt.sizeDelta = new Vector2(x1 - x0, y1 - y0);
            return rt;
        }
        private static readonly Color BarBg = new Color(0.808f, 0.78f, 0.718f);   // 206,199,183
        /// 시안 막대: 둥근 받침 + 색 채움(오른쪽 끝 ✦ 반짝이) + 오른쪽 숫자
        private static void Bar(Transform page, string name, float cy, float v01, Color fill, string number)
        {
            float h = 30f;
            var bg = CoastUiArt.Panel(page, name + "Bg", BarBg, 15); Place(bg.rectTransform, 247f, cy - h * 0.5f, 633f, cy + h * 0.5f);
            var fl = CoastUiArt.Panel(bg.transform, "Fill", fill, 13); fl.raycastTarget = false;
            fl.rectTransform.anchorMin = Vector2.zero; fl.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(v01), 1f); fl.rectTransform.offsetMin = new Vector2(2f, 2f); fl.rectTransform.offsetMax = new Vector2(-2f, -2f);
            var hl = CoastUiArt.Panel(fl.transform, "Hl", new Color(1f, 1f, 1f, 0.28f), 8); hl.raycastTarget = false;
            hl.rectTransform.anchorMin = new Vector2(0f, 0.5f); hl.rectTransform.anchorMax = Vector2.one; hl.rectTransform.offsetMin = new Vector2(8f, 0f); hl.rectTransform.offsetMax = new Vector2(-8f, -4f);
            if (v01 > 0.06f)
            {
                var sp = CoastHudLayout.MakeText(fl.transform, "Spark", "✦", 20, TextAnchor.MiddleCenter, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-30f, -16f), new Vector2(2f, 16f));
                sp.color = Color.white; sp.raycastTarget = false;
                var sp2 = CoastHudLayout.MakeText(fl.transform, "Spark2", "✦", 12, TextAnchor.MiddleCenter, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(4f, -12f), new Vector2(24f, 12f));
                sp2.color = new Color(1f, 1f, 1f, 0.8f); sp2.raycastTarget = false;
            }
            var n = CoastHudLayout.MakeText(page, name + "V", number, 24, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(n.rectTransform, 600f, cy - 22f, 682f, cy + 22f); n.color = Navy; n.fontStyle = FontStyle.Bold; n.raycastTarget = false;
        }

        public static void Open(GameManager gm, Action onClose = null)
        {
            Close();
            _onClose = onClose;
            var save = gm != null ? gm.Save : null;
            if (save == null) return;
            _canvas = CoastUiCanvas.Create("StatusCanvas", 462);
            var root = CoastUiCanvas.Root(_canvas);
            var pad = CoastUiCanvas.HudPad;
            var bgTex = ArtAssets.LoadTexture("UI_Status_BG");
            var dim = CoastHudLayout.MakeImage(root, "Dim", Vector2.zero, Vector2.one, new Vector2(-pad - 400f, -pad - 400f), new Vector2(pad + 400f, pad + 400f), bgTex != null ? new Color(0.996f, 0.957f, 0.925f) : new Color(0.05f, 0.04f, 0.10f, 0.72f));
            dim.raycastTarget = true;
            var db = dim.gameObject.AddComponent<Button>(); db.transition = Selectable.Transition.None; db.onClick.AddListener(Close);
            var page = new GameObject("Page", typeof(RectTransform)).GetComponent<RectTransform>();
            page.SetParent(root, false); page.anchorMin = page.anchorMax = new Vector2(0.5f, 0.5f); page.pivot = new Vector2(0.5f, 0.5f); page.sizeDelta = new Vector2(720f, 1280f); page.anchoredPosition = Vector2.zero;
            if (bgTex != null)
            {
                var art = CoastHudLayout.MakeImage(page, "Art", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
                art.sprite = CoastUiArt.AsSprite(bgTex); art.preserveAspect = false; art.raycastTarget = true;
            }
            else
            {
                var card = CoastUiArt.CutePill(page, "Card", Cream, 28, 5); Place(card.rectTransform, 30f, 20f, 690f, 1260f); card.raycastTarget = true;
                var face = ArtAssets.LoadTexture("UI_Face_Girl");
                var fimg = CoastHudLayout.MakeImage(page, "Face", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, Color.white); Place(fimg.rectTransform, 80f, 128f, 215f, 263f);
                if (face != null) { fimg.sprite = CoastUiArt.AsSprite(face); fimg.preserveAspect = true; } else fimg.color = new Color(1f, 0.85f, 0.7f);
            }
            // 86차-2 규칙: 시안 그림(UI_Status_BG)에는 프레임·아이콘·알약 틀만 굽고, 글자는 전부 코드에서 Loc.T로 그린다(다국어).
            {
                var ttl = CoastHudLayout.MakeText(page, "Title", Loc.T("캐릭터 정보", "Character"), 26, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                Place(ttl.rectTransform, 250f, 58f, 490f, 108f); ttl.color = new Color(0.26f, 0.14f, 0.05f); ttl.fontStyle = FontStyle.Bold; ttl.raycastTarget = false;
                ttl.resizeTextForBestFit = true; ttl.resizeTextMinSize = 14; ttl.resizeTextMaxSize = CoastHudLayout.Scaled(26);
                var hs = CoastHudLayout.MakeText(page, "HdrStats", Loc.T("스탯", "Stats"), 26, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                Place(hs.rectTransform, 106f, 428f, 236f, 486f); hs.color = new Color(0.26f, 0.14f, 0.05f); hs.fontStyle = FontStyle.Bold; hs.raycastTarget = false;
                var hl2 = CoastHudLayout.MakeText(page, "HdrLife", Loc.T("생활", "Life"), 26, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                Place(hl2.rectTransform, 340f, 970f, 470f, 1016f); hl2.color = new Color(0.26f, 0.14f, 0.05f); hl2.fontStyle = FontStyle.Bold; hl2.raycastTarget = false;
                string[] lblKo = { "체력", "순발력", "매력", "감성", "평판", "말썽", "스트레스", "배부름", "컨디션" };
                string[] lblEn = { "Stamina", "Agility", "Charm", "Sense", "Trust", "Trouble", "Stress", "Hunger", "Cond." };
                float[] ly = { 523f, 582f, 642f, 702f, 761f, 821f, 881f, 1053f, 1112f };
                for (int i = 0; i < lblKo.Length; i++)
                {
                    var l = CoastHudLayout.MakeText(page, "L" + i, Loc.T(lblKo[i], lblEn[i]), 20, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
                    Place(l.rectTransform, 122f, ly[i] - 22f, 240f, ly[i] + 22f); l.color = new Color(0.26f, 0.14f, 0.05f); l.fontStyle = FontStyle.Bold; l.raycastTarget = false;
                    l.resizeTextForBestFit = true; l.resizeTextMinSize = 11; l.resizeTextMaxSize = CoastHudLayout.Scaled(20);
                }
            }

            int lv = Mathf.Max(1, save.level);
            // 레벨 · 이름 · 칭호 (+ 오른쪽 칭호 칩)
            var nm = CoastHudLayout.MakeText(page, "Name", Loc.T($"Lv {lv}  하늘 · {LevelSystem.Title(lv)}", $"Lv {lv}  Haneul · {LevelSystem.Title(lv)}"), 26, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(nm.rectTransform, 220f, 151f, 560f, 211f); nm.color = Navy; nm.fontStyle = FontStyle.Bold; nm.horizontalOverflow = HorizontalWrapMode.Overflow;
            nm.resizeTextForBestFit = true; nm.resizeTextMinSize = 14; nm.resizeTextMaxSize = CoastHudLayout.Scaled(26);
            var chip = CoastUiArt.Panel(page, "TitleChip", new Color(0.62f, 0.56f, 0.92f), 16); Place(chip.rectTransform, 574f, 163f, 682f, 199f);
            var chipT = CoastHudLayout.MakeText(chip.transform, "T", LevelSystem.Title(lv), 13, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(4f, 1f), new Vector2(-4f, 0f));
            chipT.color = Color.white; chipT.fontStyle = FontStyle.Bold; chipT.resizeTextForBestFit = true; chipT.resizeTextMinSize = 9; chipT.resizeTextMaxSize = CoastHudLayout.Scaled(13);
            // EXP
            int need = LevelSystem.Need(lv);
            var el = CoastHudLayout.MakeText(page, "ExpL", "EXP", 14, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero); Place(el.rectTransform, 220f, 214f, 320f, 240f); el.color = Navy; el.fontStyle = FontStyle.Bold;
            var er = CoastHudLayout.MakeText(page, "ExpR", lv >= LevelSystem.MaxLevel ? "MAX" : $"EXP {save.exp} / {need}", 14, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero); Place(er.rectTransform, 420f, 214f, 682f, 240f); er.color = new Color(0.45f, 0.30f, 0.75f); er.fontStyle = FontStyle.Bold;
            {
                float v01 = lv >= LevelSystem.MaxLevel ? 1f : Mathf.Clamp01(save.exp / (float)need);
                var bg = CoastUiArt.Panel(page, "ExpBg", new Color(0.92f, 0.90f, 0.96f), 16); Place(bg.rectTransform, 220f, 244f, 682f, 280f);
                var fl = CoastUiArt.Panel(bg.transform, "Fill", new Color(0.55f, 0.40f, 0.95f), 14); fl.raycastTarget = false;
                fl.rectTransform.anchorMin = Vector2.zero; fl.rectTransform.anchorMax = new Vector2(Mathf.Max(0.04f, v01), 1f); fl.rectTransform.offsetMin = new Vector2(2f, 2f); fl.rectTransform.offsetMax = new Vector2(-2f, -2f);
                var hl = CoastUiArt.Panel(fl.transform, "Hl", new Color(1f, 1f, 1f, 0.25f), 8); hl.raycastTarget = false;
                hl.rectTransform.anchorMin = new Vector2(0f, 0.5f); hl.rectTransform.anchorMax = Vector2.one; hl.rectTransform.offsetMin = new Vector2(8f, 0f); hl.rectTransform.offsetMax = new Vector2(-8f, -4f);
                foreach (var side in new[] { 0f, 1f })
                {
                    var sp = CoastHudLayout.MakeText(fl.transform, "Spark", "✦", 18, TextAnchor.MiddleCenter, new Vector2(side, 0.5f), new Vector2(side, 0.5f), new Vector2(side < 0.5f ? 6f : -30f, -16f), new Vector2(side < 0.5f ? 30f : -6f, 16f));
                    sp.color = Color.white; sp.raycastTarget = false;
                }
            }
            // 대회 배너(시안: 깃발 + 노랑 리본 + 흰 칩)
            {
                var ban = CoastUiArt.Panel(page, "Banner", new Color(0.98f, 0.80f, 0.25f), 22); Place(ban.rectTransform, 36f, 296f, 689f, 410f);
                var inner = CoastUiArt.Panel(ban.transform, "Inner", new Color(1f, 0.93f, 0.50f), 18); inner.raycastTarget = false;
                inner.rectTransform.anchorMin = Vector2.zero; inner.rectTransform.anchorMax = Vector2.one; inner.rectTransform.offsetMin = new Vector2(4f, 4f); inner.rectTransform.offsetMax = new Vector2(-4f, -4f);
                var rib = CoastUiArt.Panel(ban.transform, "Rib", new Color(0.55f, 0.85f, 1f, 0.55f), 14); rib.raycastTarget = false;
                rib.rectTransform.anchorMin = new Vector2(0f, 0f); rib.rectTransform.anchorMax = new Vector2(1f, 0.32f); rib.rectTransform.offsetMin = new Vector2(8f, 6f); rib.rectTransform.offsetMax = new Vector2(-8f, 0f);
                var ftex = ArtAssets.LoadTexture("UI_Status_Flag") ?? ArtAssets.LoadTexture("UI_Contest_Flag");
                var flag = CoastHudLayout.MakeImage(ban.transform, "Flag", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, -50f), new Vector2(96f, 50f), Color.white);
                if (ftex != null) { flag.sprite = CoastUiArt.AsSprite(ftex); flag.preserveAspect = true; } else flag.color = Color.clear;
                flag.raycastTarget = false;
                string title, sub; GoalParts(save, out title, out sub);
                var bt = CoastHudLayout.MakeText(ban.transform, "T", title.Replace("「", "").Replace("」", ""), 26, TextAnchor.MiddleCenter, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(100f, -4f), new Vector2(-24f, -8f));
                bt.color = new Color(0.10f, 0.14f, 0.45f); bt.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(bt, new Color(1f, 1f, 1f, 0.9f), 1.6f);
                bt.resizeTextForBestFit = true; bt.resizeTextMinSize = 12; bt.resizeTextMaxSize = CoastHudLayout.Scaled(26); bt.horizontalOverflow = HorizontalWrapMode.Wrap;
                var sc = CoastUiArt.Panel(ban.transform, "SubChip", new Color(1f, 1f, 1f, 0.85f), 16); sc.raycastTarget = false;
                sc.rectTransform.anchorMin = new Vector2(0.5f, 0f); sc.rectTransform.anchorMax = new Vector2(0.5f, 0f); sc.rectTransform.pivot = new Vector2(0.5f, 0f); sc.rectTransform.anchoredPosition = new Vector2(40f, 10f); sc.rectTransform.sizeDelta = new Vector2(400f, 34f);
                var st2 = CoastHudLayout.MakeText(sc.transform, "T", sub, 15, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8f, 1f), new Vector2(-8f, 0f));
                st2.color = new Color(0.10f, 0.14f, 0.45f); st2.fontStyle = FontStyle.Bold; st2.resizeTextForBestFit = true; st2.resizeTextMinSize = 10; st2.resizeTextMaxSize = CoastHudLayout.Scaled(15);
                foreach (var (x, y, c) in new[] { (0.72f, 0.82f, new Color(0.35f, 0.8f, 0.5f)), (0.9f, 0.7f, new Color(1f, 0.55f, 0.3f)), (0.2f, 0.15f, new Color(0.4f, 0.6f, 1f)), (0.82f, 0.18f, new Color(1f, 0.4f, 0.6f)) })
                {
                    var conf = CoastUiArt.Panel(ban.transform, "Conf", c, 4); conf.raycastTarget = false;
                    conf.rectTransform.anchorMin = conf.rectTransform.anchorMax = new Vector2(x, y); conf.rectTransform.sizeDelta = new Vector2(14f, 7f); conf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, x * 90f);
                }
            }

            // 스탯 7개 (시안 순서: 체력·순발력·매력·감성·평판·말썽·스트레스)
            var stt = save.stats;
            (int v, int max, Color c)[] rows =
            {
                (stt.stamina, PlayerStats.StatMax, new Color(0.945f, 0.35f, 0.33f)),
                (stt.agility, PlayerStats.StatMax, new Color(0.37f, 0.75f, 0.96f)),
                (stt.charm, PlayerStats.StatMax, new Color(1f, 0.565f, 0.757f)),
                (stt.sense, PlayerStats.StatMax, new Color(0.67f, 0.55f, 0.96f)),
                (stt.trust, 100, new Color(1f, 0.80f, 0.31f)),
                (stt.trouble, 100, new Color(0.78f, 0.67f, 0.96f)),
                (stt.stress, 100, new Color(0.59f, 0.59f, 0.62f)),
            };
            float[] ry = { 523f, 582f, 642f, 702f, 761f, 821f, 881f };
            for (int i = 0; i < rows.Length; i++) Bar(page, "S" + i, ry[i], rows[i].v / (float)rows[i].max, rows[i].c, rows[i].v.ToString());
            // 돈 · 코인 알약(보라)
            {
                var mp = CoastUiArt.Panel(page, "MoneyPill", new Color(0.49f, 0.42f, 0.89f), 26); Place(mp.rectTransform, 50f, 911f, 678f, 961f);
                var lip = CoastUiArt.Panel(mp.transform, "Lip", new Color(0.36f, 0.30f, 0.72f), 22); lip.raycastTarget = false;
                lip.rectTransform.anchorMin = Vector2.zero; lip.rectTransform.anchorMax = new Vector2(1f, 0.3f); lip.rectTransform.offsetMin = new Vector2(3f, 3f); lip.rectTransform.offsetMax = new Vector2(-3f, 0f);
                string money = Loc.T($"돈 {LevelSystem.FormatK(stt.money)}G   ·   코인 {LevelSystem.FormatK(CoinWallet.TotalStatic)}", $"Money {LevelSystem.FormatK(stt.money)}G   ·   Coins {LevelSystem.FormatK(CoinWallet.TotalStatic)}");
                var mt = CoastHudLayout.MakeText(mp.transform, "T", money, 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, 0f));
                mt.color = new Color(1f, 0.93f, 0.55f); mt.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(mt, new Color(0.18f, 0.12f, 0.40f, 0.9f), 1.6f);
            }
            // 생활
            Bar(page, "Life0", 1053f, Mathf.Clamp01(save.hunger / 100f), save.hunger < 30 ? new Color(0.95f, 0.35f, 0.35f) : new Color(1f, 0.70f, 0.30f), save.hunger.ToString());
            Bar(page, "Life1", 1112f, Mathf.Clamp01(save.condition / 100f), save.condition < 30 ? new Color(0.95f, 0.35f, 0.35f) : new Color(0.40f, 0.80f, 0.55f), save.condition.ToString());
            LifeItems.Ensure(save);
            int dishes = LifeItems.CountCat(save, LifeItemCat.BasicDish) + LifeItems.CountCat(save, LifeItemCat.PremiumDish);
            int ings = LifeItems.CountCat(save, LifeItemCat.Ingredient);
            int meds = LifeItems.CountCat(save, LifeItemCat.Medicine);
            string stock = Loc.T($"요리 {dishes}  ·  재료 {ings}  ·  약 {meds}  ·  옷 {(save.clothesWeeks <= 0 ? "낡음!" : save.clothesWeeks + "주 남음")}  ·  {(save.ateThisWeek ? "이번 주 식사함" : "이번 주 아직 안 먹음")}  ·  {(save.restedThisWeek ? "잠 잤음" : "아직 안 잠")}",
                $"Meals {dishes}  ·  Ing {ings}  ·  Med {meds}  ·  Clothes {(save.clothesWeeks <= 0 ? "worn!" : save.clothesWeeks + "w")}  ·  {(save.ateThisWeek ? "ate this week" : "not eaten")}  ·  {(save.restedThisWeek ? "rested" : "not rested")}");
            var stT = CoastHudLayout.MakeText(page, "Stock", stock, 13, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            Place(stT.rectTransform, 50f, 1144f, 678f, 1180f); stT.color = new Color(0.40f, 0.27f, 0.12f); stT.fontStyle = FontStyle.Bold; stT.horizontalOverflow = HorizontalWrapMode.Wrap;
            stT.resizeTextForBestFit = true; stT.resizeTextMinSize = 10; stT.resizeTextMaxSize = CoastHudLayout.Scaled(13);

            // 닫기 — 보라 버튼 틀은 시안에 구워져 있고(없으면 코드로), 글자 「닫기」는 항상 코드에서(Loc.T)
            var close = bgTex != null ? CoastUiArt.Panel(page, "Close", Color.clear, 20) : CoastUiArt.GlossyPill(page, "Close", new Color(0.49f, 0.42f, 0.89f), 24, 8);
            Place(close.rectTransform, 198f, 1180f, 520f, 1246f); close.raycastTarget = true;
            {
                var ct = CoastHudLayout.MakeText(close.transform, "T", Loc.T("닫기", "Close"), 26, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 6f), new Vector2(0f, -4f));
                ct.color = Color.white; ct.fontStyle = FontStyle.Bold; ct.raycastTarget = false; CoastUiArt.OutlineText(ct, new Color(0.22f, 0.16f, 0.55f, 0.9f), 1.6f);
            }
            var cb = close.gameObject.AddComponent<Button>(); cb.transition = Selectable.Transition.None; cb.onClick.AddListener(() => { CoastPrefs.Vibrate(); Close(); });
        }

        /// 86차: 배너용 — 제목(대회/미니게임/이야기)과 아래 줄(게이트·S컷)
        private static void GoalParts(SaveData save, out string title, out string sub)
        {
            string d = GoalDetail(save);
            int nl = d.IndexOf('\n');
            title = nl >= 0 ? d.Substring(0, nl) : d; sub = nl >= 0 ? d.Substring(nl + 1) : "";
        }

        /// 상태창용 목표 상세 — HUD 리본은 「대회/미니게임」만 표시.
        private static string GoalDetail(SaveData save)
        {
            if (save == null) return "";
            var st = save.stats;
            int need = StoryGate.Required(save);
            int have = st.stamina;
            var rec = save.CurrentChapter;
            int target = rec != null
                ? (rec.heartsTarget > 0 ? rec.heartsTarget : ChapterGrading.HeartTarget(save.chapter))
                : ChapterGrading.HeartTarget(save.chapter);
            if (!StoryProgress.IsRunChapter(save.chapter))
                target = Mathf.Max(1, target - RunTuning.HeartsPerStage);
            int sCut = Mathf.CeilToInt(target * ChapterGrading.S_Ratio);
            string gate = have >= need
                ? Loc.T($"게이트 {have}/{need} ✓", $"Gate {have}/{need} ✓")
                : Loc.T($"게이트 {have}/{need} (−{need - have})", $"Gate {have}/{need} (−{need - have})");
            string heart = Loc.T($"♥ {save.chapterHearts}/{sCut} S컷", $"♥ {save.chapterHearts}/{sCut} for S");
            var contest = StoryContest.Get(save.chapter);
            if (contest != null)
                return Loc.T($"대회 「{contest.Name}」\n{gate}  ·  {heart}", $"Contest \"{contest.Name}\"\n{gate}  ·  {heart}");
            if (StoryProgress.WeeklyMinigame(save.week, out var mk))
            {
                var md = ChapterMission.Get(mk);
                string name = Loc.T(md.nameKo, md.nameEn);
                bool done = save.weekMiniDone >= save.week;
                return Loc.T($"미니게임 「{name}」{(done ? " · 완료" : "")}\n{gate}  ·  {heart}",
                    $"Mini-game \"{name}\"{(done ? " · done" : "")}\n{gate}  ·  {heart}");
            }
            return Loc.T($"챕터 {save.chapter} 이야기\n{gate}  ·  {heart}", $"Chapter {save.chapter} story\n{gate}  ·  {heart}");
        }

        public static void Close()
        {
            if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null;
            var cb = _onClose; _onClose = null; cb?.Invoke();
        }
    }
}
