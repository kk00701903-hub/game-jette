using System.Collections.Generic;
using UnityEngine;

namespace CoastRun
{
    /// 109차(사용자: 「마이룸에서 기존 간식 바꾸기 등 다 없애고, 펫이 있으면 펫 명령 — 펫마다 특성에 따라 2개 선택지」).
    ///   펫 한 마리당 명령 2개, **주 1회**(SaveData.petCmdWeek) 하나만 고를 수 있다. 효과는 바로 적용되고 결과 문장을 돌려준다.
    ///   참새(돈 감각): 먹이 찾기 = 재료 자동 조리 / 노래 = 스트레스 −10%
    ///   팡찌(배달 오토바이): 장보기 심부름 = 쌀 한 봉 사 오기(18G) / 드라이브 = 스트레스 −10%
    ///   기러기(자석): 편지 물어오기 = 하트 +1 / 같이 산책 = 스트레스 −10% · 체력 +1
    ///   흑돼지(버티기): 같이 낮잠 = 체력 +3 / 밥 나눠 먹기 = 재료 하나 자동 조리 + 스트레스 −5%
    public static class PetCommands
    {
        public enum Effect { AutoCook, Stress10, Grocery, Heart, Walk, Nap, ShareMeal }

        public struct Cmd
        {
            public string nameKo, nameEn, blurbKo, blurbEn, icon; public Effect effect;
            public string Name => Loc.T(nameKo, nameEn);
            public string Blurb => Loc.T(blurbKo, blurbEn);
        }

        private static readonly Dictionary<PetKind, Cmd[]> Table = new Dictionary<PetKind, Cmd[]>
        {
            { PetKind.Sparrow, new[] {
                new Cmd { nameKo = "먹이 찾기", nameEn = "Find food", blurbKo = "가진 재료를 요리로 (자동 조리)", blurbEn = "Cook owned ingredients", icon = "Icon_Cart", effect = Effect.AutoCook },
                new Cmd { nameKo = "노래 불러 줘", nameEn = "Sing", blurbKo = "스트레스 −10%", blurbEn = "Stress −10%", icon = "Icon_Heart", effect = Effect.Stress10 } } },
            { PetKind.BikerThug, new[] {
                new Cmd { nameKo = "장보기 심부름", nameEn = "Grocery run", blurbKo = "쌀 한 봉 사 오기 (18G)", blurbEn = "Buys a bag of rice (18G)", icon = "Icon_Cart", effect = Effect.Grocery },
                new Cmd { nameKo = "드라이브", nameEn = "Ride", blurbKo = "스트레스 −10%", blurbEn = "Stress −10%", icon = "Icon_Speed", effect = Effect.Stress10 } } },
            { PetKind.WildGoose, new[] {
                new Cmd { nameKo = "편지 물어오기", nameEn = "Fetch a letter", blurbKo = "하트 +1", blurbEn = "Hearts +1", icon = "Icon_Heart", effect = Effect.Heart },
                new Cmd { nameKo = "같이 산책", nameEn = "Walk together", blurbKo = "스트레스 −10% · 체력 +1", blurbEn = "Stress −10% · Stamina +1", icon = "Icon_Speed", effect = Effect.Walk } } },
            { PetKind.BlackPig, new[] {
                new Cmd { nameKo = "같이 낮잠", nameEn = "Nap together", blurbKo = "체력 +3", blurbEn = "Stamina +3", icon = "Icon_Refresh", effect = Effect.Nap },
                new Cmd { nameKo = "밥 나눠 먹기", nameEn = "Share a meal", blurbKo = "재료 하나 자동 조리 · 스트레스 −5%", blurbEn = "Cook one ingredient · Stress −5%", icon = "Icon_Cart", effect = Effect.ShareMeal } } },
        };

        public static Cmd[] For(PetKind k) => Table.TryGetValue(k, out var c) ? c : null;

        public static bool UsedThisWeek(SaveData s) => s != null && s.petCmdWeek == s.week;

        /// 명령 실행 — 성공하면 결과 문장, 못 하면 이유(null 이면 성공).
        public static string Run(GameManager gm, PetKind k, int idx, out bool ok)
        {
            ok = false;
            var s = gm != null ? gm.Save : null; var cmds = For(k);
            if (s == null || cmds == null || idx < 0 || idx >= cmds.Length) return Loc.T("펫이 없어.", "No pet.");
            if (UsedThisWeek(s)) return Loc.T("이번 주 명령은 이미 했어 — 다음 주에 또!", "Already commanded this week — next week!");
            string petName = PetCompanion.Names[Mathf.Clamp((int)k, 0, PetCompanion.Names.Length - 1)];
            string result;
            switch (cmds[idx].effect)
            {
                case Effect.AutoCook:
                {
                    int n = AutoCook(s, 3);
                    if (n == 0) return Loc.T("조리할 재료가 없어 — 장보기부터.", "No ingredients to cook — shop first.");
                    result = Loc.T($"{petName}가 재료를 물어 와서 요리 {n}개가 완성됐어!", $"{petName} brought ingredients — {n} dishes cooked!");
                    break;
                }
                case Effect.ShareMeal:
                {
                    int n = AutoCook(s, 1);
                    s.stats.stress = Mathf.Max(0, s.stats.stress - Mathf.CeilToInt(s.stats.stress * 0.05f)); s.stats.Clamp();
                    result = n > 0 ? Loc.T($"{petName}랑 나눠 먹었어 — 요리 1개 완성, 스트레스 −5%", $"Shared with {petName} — 1 dish cooked, stress −5%")
                                   : Loc.T($"{petName}랑 나눠 먹었어 — 스트레스 −5% (재료는 없었어)", $"Shared with {petName} — stress −5% (no ingredients)");
                    break;
                }
                case Effect.Stress10:
                {
                    int d = Mathf.Max(1, Mathf.CeilToInt(s.stats.stress * 0.10f));
                    s.stats.stress = Mathf.Max(0, s.stats.stress - d); s.stats.Clamp();
                    result = Loc.T($"{petName} 덕에 마음이 풀렸어 — 스트레스 −{d}", $"{petName} cheered me up — stress −{d}");
                    break;
                }
                case Effect.Grocery:
                {
                    if (!LifeItems.CanBuy(s, "ing_rice", 1)) return Loc.T("18G 가 모자라.", "Need 18G.");
                    LifeItems.Buy(s, "ing_rice", 1);
                    result = Loc.T($"{petName}가 쌀 한 봉을 사 왔어! (−18G)", $"{petName} brought a bag of rice! (−18G)");
                    break;
                }
                case Effect.Heart:
                    s.chapterHearts += 1; s.stats.hearts += 1;
                    result = Loc.T($"{petName}가 편지를 물어왔어 — 하트 +1", $"{petName} fetched a letter — hearts +1");
                    break;
                case Effect.Walk:
                {
                    int d = Mathf.Max(1, Mathf.CeilToInt(s.stats.stress * 0.10f));
                    s.stats.stress = Mathf.Max(0, s.stats.stress - d); s.stats.stamina += 1; s.stats.Clamp();
                    result = Loc.T($"{petName}랑 바닷길을 걸었어 — 스트레스 −{d} · 체력 +1", $"Walked the coast with {petName} — stress −{d} · stamina +1");
                    break;
                }
                default:
                    s.stats.stamina += 3; s.stats.Clamp();
                    result = Loc.T($"{petName}랑 낮잠 잤어 — 체력 +3", $"Napped with {petName} — stamina +3");
                    break;
            }
            s.petCmdWeek = s.week; ok = true;
            gm.Persist();
            return result;
        }

        /// 가진 재료 중 조리 가능한 것을 앞에서부터 최대 max 개 요리로.
        private static int AutoCook(SaveData s, int max)
        {
            int n = 0;
            foreach (var d in LifeItems.All)
            {
                if (n >= max) break;
                if (d.cat != LifeItemCat.Ingredient || string.IsNullOrEmpty(d.cookTo)) continue;
                while (n < max && LifeItems.CanCook(s, d.id) && LifeItems.Cook(s, d.id, 1)) n++;
            }
            return n;
        }
    }
}
