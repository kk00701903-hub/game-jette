using System.Collections.Generic;
using UnityEngine;

namespace CoastRun
{
    /// 55차(사용자): 육성 **생존 생태계**(다마고치) — 재료를 사서 방에서 조리 → 「밥」으로 먹고, 옷·잠·컨디션을 관리한다.
    ///   한 주(행동 3번)가 끝날 때 WeekTick 이 소비·악화를 계산하고 WeekPassUI 가 그 결과를 보여 준다.
    public static class Survival
    {
        public const int ClothesWeeks = 12;
        public const int RicePrice = 60, SidePrice = 40, ClothesPrice = 300; // 레거시 상수(새 단가는 LifeItems)
        public const int DangerWeeksToDie = 2;

        public class WeekReport
        {
            public bool ateRice, ateSide, slept, clothesWorn;
            public int hungerBefore, hungerAfter, condBefore, condAfter, riceLeft, sideLeft, clothesLeft, harvested;
            public bool died;
            public readonly List<string> lines = new List<string>();
            public readonly List<string> harvestNames = new List<string>();
        }

        public static WeekReport WeekTick(SaveData s)
        {
            var r = new WeekReport();
            if (s == null) return r;
            LifeItems.Ensure(s);
            r.hungerBefore = s.hunger; r.condBefore = s.condition;
            r.harvested = HomeData.WeeklyGrow(s);
            HomeData.EnsurePots(s);
            for (int i = 0; i < s.pots.Length; i++)
            {
                var p = s.pots[i]; var sd = HomeData.Seed(p.seed);
                if (sd == null) continue;
                if (p.growth >= sd.weeks) r.harvestNames.Add(sd.Name);
            }

            // 식사: 「밥」으로 요리를 먹었으면 유지, 아니면 굶주림(재료만 있어도 안 먹힌 상태)
            r.ateRice = s.ateThisWeek;
            r.ateSide = s.ateThisWeek;
            if (s.ateThisWeek)
            {
                s.starveWeeks = 0;
                s.hunger = Mathf.Max(0, s.hunger - 18);
            }
            else
            {
                s.hunger -= 40;
                s.starveWeeks++;
                s.condition -= 5;
            }
            s.ateThisWeek = false;
            s.hunger = Mathf.Clamp(s.hunger, 0, 100);

            r.slept = s.restedThisWeek;
            if (s.restedThisWeek) s.sleepDebt = 0; else s.sleepDebt++;
            s.restedThisWeek = false;
            if (s.sleepDebt >= 2) { s.condition -= 15; s.stats.stress = Mathf.Min(PlayerStats.StatMax, s.stats.stress + 10); }

            s.clothesWeeks = Mathf.Max(0, s.clothesWeeks - 1);
            r.clothesWorn = s.clothesWeeks <= 0;
            if (r.clothesWorn) { s.condition -= 8; s.stats.charm = Mathf.Max(0, s.stats.charm - 1); }

            if (s.hunger >= 60) s.condition += 8;
            else if (s.hunger >= 30) s.condition -= 8;
            else s.condition -= 22;
            s.condition = Mathf.Clamp(s.condition, 0, 100);
            if (s.condition < 30) s.stats.stamina = Mathf.Max(0, s.stats.stamina - 3);

            if (s.condition <= 0) s.dangerWeeks++; else s.dangerWeeks = 0;
            r.died = s.dangerWeeks >= DangerWeeksToDie || s.starveWeeks >= 4;
            if (r.died) s.deaths++;

            LifeItems.SyncLegacy(s);
            r.hungerAfter = s.hunger; r.condAfter = s.condition;
            r.riceLeft = s.rice; r.sideLeft = s.sideDish; r.clothesLeft = s.clothesWeeks;

            int dishes = LifeItems.CountCat(s, LifeItemCat.BasicDish) + LifeItems.CountCat(s, LifeItemCat.PremiumDish);
            int ings = LifeItems.CountCat(s, LifeItemCat.Ingredient);
            r.lines.Add(r.ateRice
                ? Loc.T($"이번 주 식사함 · 남은 요리 {dishes} · 재료 {ings}", $"Ate this week · dishes {dishes} · ingredients {ings}")
                : Loc.T($"식사를 안 했다… 요리 {dishes} · 재료 {ings} (방에서 조리 후 「밥」)", $"No meal… dishes {dishes} · ingredients {ings} (cook then Feed)"));
            if (!r.ateRice && ings > 0 && dishes <= 0)
                r.lines.Add(Loc.T("재료만 있어 — 마이룸에서 조리해야 먹어", "Ingredients only — cook in My Room first"));
            if (r.harvested > 0)
            {
                string crops = r.harvestNames.Count > 0 ? string.Join(", ", r.harvestNames) : $"{r.harvested}";
                r.lines.Add(Loc.T($"텃밭에 다 자람: {crops} — 마이룸에서 수확", $"Garden ready: {crops} — harvest in My Room"));
            }
            r.lines.Add(r.slept ? Loc.T("잘 잤다", "Slept well") : Loc.T($"잠을 못 잤다 ({s.sleepDebt}주째)", $"No sleep ({s.sleepDebt} wk)"));
            r.lines.Add(r.clothesWorn ? Loc.T("옷이 낡아서 못 입겠다 — 새 옷을 사자", "Clothes worn out — buy new") : Loc.T($"옷 {s.clothesWeeks}주 남음", $"Clothes {s.clothesWeeks} wk left"));
            r.lines.Add(Loc.T($"배부름 {r.hungerBefore} → {s.hunger}  ·  컨디션 {r.condBefore} → {s.condition}", $"Fullness {r.hungerBefore} → {s.hunger}  ·  Condition {r.condBefore} → {s.condition}"));
            if (!r.died && s.condition <= 0) r.lines.Add(Loc.T("!! 컨디션 0 — 한 주 더 이러면 쓰러진다", "!! Condition 0 — one more week and she collapses"));
            return r;
        }

        /// 밥/휴식 — 잠 표시. 배부름은 Eat()에서 오른다.
        public static void OnRestAction(SaveData s)
        {
            if (s == null) return;
            s.restedThisWeek = true;
        }

        public static bool BuyRice(SaveData s, int weeks = 1)
        {
            LifeItems.Ensure(s);
            return LifeItems.Buy(s, "ing_rice", weeks);
        }
        public static bool BuySide(SaveData s, int n = 1)
        {
            LifeItems.Ensure(s);
            return LifeItems.Buy(s, "ing_veg", n);
        }
        public static bool BuyClothes(SaveData s) => LifeItems.Buy(s, "clothes_set", 1);

        public static void Revive(SaveData s)
        {
            if (s == null) return;
            LifeItems.Ensure(s);
            s.stats.money /= 2;
            s.condition = 50; s.hunger = 50; s.dangerWeeks = 0; s.starveWeeks = 0; s.sleepDebt = 0;
            if (!LifeItems.HasEdible(s)) LifeItems.Add(s, "dish_rice", 1);
            if (s.clothesWeeks <= 0) s.clothesWeeks = 2;
            s.stats.stress = Mathf.Max(0, s.stats.stress - 30);
            LifeItems.SyncLegacy(s);
        }

        public static string Summary(SaveData s)
        {
            if (s == null) return "";
            LifeItems.Ensure(s);
            int dishes = LifeItems.CountCat(s, LifeItemCat.BasicDish) + LifeItems.CountCat(s, LifeItemCat.PremiumDish);
            int ings = LifeItems.CountCat(s, LifeItemCat.Ingredient);
            return Loc.T($"요리 {dishes} · 재료 {ings} · 옷 {s.clothesWeeks}주 · 배부름 {s.hunger} · 컨디션 {s.condition}",
                         $"Meals {dishes} · Ing {ings} · Clothes {s.clothesWeeks}w · Full {s.hunger} · Cond {s.condition}");
        }

        public static string Warning(SaveData s)
        {
            if (s == null) return null;
            LifeItems.Ensure(s);
            if (s.condition <= 0) return Loc.T("컨디션 0! 다음 주에 쓰러진다 — 식사·잠·약", "Condition 0! Collapses next week");
            if (!LifeItems.HasEdible(s) && LifeItems.CountCat(s, LifeItemCat.Ingredient) > 0)
                return Loc.T("재료만 있어 — 마이룸에서 조리", "Ingredients only — cook in My Room");
            if (!LifeItems.HasEdible(s)) return Loc.T("먹을 요리가 없어 — 장보기·조리", "No meals — shop & cook");
            if (s.hunger < 30) return Loc.T("배고파… 「밥」으로 식사하자", "Hungry… use Feed to eat");
            if (s.clothesWeeks <= 0) return Loc.T("옷이 낡았어 — 새 옷", "Clothes worn out");
            if (s.sleepDebt >= 1) return Loc.T("이번 주엔 꼭 자자(밥/휴식)", "Rest this week");
            if (s.condition < 30) return Loc.T("컨디션이 나빠 — 약·보양식", "Poor condition — medicine / stew");
            return null;
        }
    }
}
