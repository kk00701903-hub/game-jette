using System.Collections.Generic;
using UnityEngine;

namespace CoastRun
{
    /// 105차(사용자: 스토리모드 재미요소 — Docs/STORY_MODE_FUN_REVIEW_v1.md).
    ///   ① 카드 미리보기(오를 수치·실패율) ② 다음 대회 목표 띠(추천 스탯, 게이트는 그대로) ③ 같은 행동 반복 = 숙련 ★(효과·수입 +10%/★)
    ///   ④ 자동 모드 효율 −15% ⑤ 성장 순간 토스트. 규칙은 전부 여기 — 화면(TamaRaisingUI)은 문자열만 받아 쓴다.
    public static class RaisingFun
    {
        // ── 숙련 ──
        public const int MaxMastery = 5;
        public const int UsesPerStar = 3;          // 3회마다 ★ 하나(★1 은 기본)
        public const float MasteryStep = 0.10f;    // ★ 하나당 +10%
        public const float AutoEfficiency = 0.85f; // 자동 모드는 손으로 할 때의 85%

        public static int MasteryUses(SaveData s, string id)
        {
            if (s == null || string.IsNullOrEmpty(id) || s.masteryIds == null) return 0;
            for (int i = 0; i < s.masteryIds.Length; i++) if (s.masteryIds[i] == id) return i < s.masteryCounts.Length ? s.masteryCounts[i] : 0;
            return 0;
        }
        public static int MasteryLevel(int uses) => Mathf.Clamp(1 + uses / UsesPerStar, 1, MaxMastery);
        public static int MasteryLevel(SaveData s, string id) => MasteryLevel(MasteryUses(s, id));
        public static float MasteryMul(int level) => 1f + MasteryStep * (Mathf.Clamp(level, 1, MaxMastery) - 1);
        public static string Stars(int level) { string t = ""; for (int i = 0; i < level; i++) t += "★"; return t; }

        /// 행동 1회 기록. ★이 올랐으면 새 레벨을, 아니면 0 을 돌려준다.
        public static int NoteUse(SaveData s, string id)
        {
            if (s == null || string.IsNullOrEmpty(id)) return 0;
            if (s.masteryIds == null) { s.masteryIds = new string[0]; s.masteryCounts = new int[0]; }
            int before = MasteryLevel(s, id);
            int idx = System.Array.IndexOf(s.masteryIds, id);
            if (idx < 0)
            {
                var ids = new List<string>(s.masteryIds) { id }; var cs = new List<int>(s.masteryCounts) { 1 };
                s.masteryIds = ids.ToArray(); s.masteryCounts = cs.ToArray();
            }
            else s.masteryCounts[idx]++;
            int after = MasteryLevel(s, id);
            return after > before ? after : 0;
        }

        // ── 카드 미리보기 ──
        /// 카드 아래 한 줄: 「체력 +3 · 순발력 +2 · 60G · 기운 −15 · 실패 12%」. 실패 30% 이상이면 warn = true.
        public static string Preview(ScheduleDef d, SaveData s, out bool warn)
        {
            warn = false;
            if (d == null || s == null) return "";
            var st = s.stats;
            int lv = MasteryLevel(s, d.id);
            float mul = MasteryMul(lv);
            var parts = new List<string>();
            void Add(string name, int v, bool scaled)
            {
                if (v == 0) return;
                int vv = scaled && v > 0 ? Mathf.RoundToInt(v * mul) : v;
                parts.Add($"{name} {vv:+#;-#}");
            }
            Add(Loc.T("체력", "STA"), d.dStamina, true);
            Add(Loc.T("순발력", "AGI"), d.dAgility, true);
            Add(Loc.T("매력", "CHA"), d.dCharm, true);
            Add(Loc.T("감성", "SEN"), d.dSense, true);
            if (d.dMoney != 0) parts.Add(d.dMoney > 0 ? $"{Mathf.RoundToInt(d.dMoney * mul)}G" : $"{d.dMoney}G");
            if (d.dStress != 0) parts.Add(Loc.T("기운", "ENG") + $" {-d.dStress:+#;-#}");
            ScheduleJudge.Condition = s.condition;
            float p = ScheduleJudge.SuccessChance(d, st);
            if (p < 1f)
            {
                int fail = Mathf.RoundToInt((1f - p) * 100f);
                warn = fail >= 30;
                parts.Add(Loc.T($"실패 {fail}%", $"fail {fail}%"));
            }
            return string.Join(" · ", parts);
        }

        // ── 다음 대회 목표 ──
        public static StoryContest.Def NextContest(SaveData s)
        {
            if (s == null) return null;
            foreach (var c in StoryProgress.RunChapters) if (c >= s.chapter) return StoryContest.Get(c);
            return null;
        }
        /// 대회 종류별 「추천」 스탯 — 러닝에 실제로 들어가는 값(RunTuning)을 기준으로. 게이트는 체력 그대로.
        public static StatKind RecommendedStat(StoryContest.Def d)
        {
            if (d == null) return StatKind.Stamina;
            switch (d.goal)
            {
                case StoryContest.Goal.Coins: return StatKind.Agility;
                case StoryContest.Goal.Photos: return StatKind.Charm;
                case StoryContest.Goal.Boss: return StatKind.Stamina;
                default: return StatKind.Agility;
            }
        }
        public static int RecommendedValue(StoryContest.Def d) => d == null ? 30 : Mathf.Clamp(28 + 5 * d.chapter, 30, 140);
        public static string StatName(StatKind k)
        {
            switch (k)
            {
                case StatKind.Stamina: return Loc.T("체력", "Stamina");
                case StatKind.Agility: return Loc.T("순발력", "Agility");
                case StatKind.Charm: return Loc.T("매력", "Charm");
                case StatKind.Sense: return Loc.T("감성", "Sense");
                default: return "";
            }
        }
        public static int WeeksUntil(SaveData s, StoryContest.Def d)
        {
            if (s == null || d == null) return 0;
            var rec = s.chapters != null && d.chapter - 1 < s.chapters.Length ? s.chapters[d.chapter - 1] : null;
            int end = rec != null && rec.weekEnd > 0 ? rec.weekEnd : Timeline.WeekEnd(d.chapter);
            return Mathf.Max(0, end - s.week);
        }
        /// HUD 목표 띠 한 줄.
        public static string GoalRibbon(SaveData s)
        {
            var d = NextContest(s);
            if (s == null) return "";
            if (d == null) return Loc.T("이야기", "Story");
            var k = RecommendedStat(d); int have = s.stats.Get(k), want = RecommendedValue(d);
            int w = WeeksUntil(s, d);
            string when = s.boundaryPending && d.chapter == s.chapter ? Loc.T("이번 턴", "now") : w == 0 ? Loc.T("이번 주", "this week") : Loc.T($"{w}주 뒤", $"in {w}w");
            string stat = have >= want ? $"{StatName(k)} {have} ✓" : $"{StatName(k)} {have}/{want}";
            return Loc.T($"{when} 대회 · {d.ShortGoal} {d.target} · {stat}", $"{d.ShortGoal} {d.target} {when} · {stat}");
        }
        /// 대회 안내 카드: 내가 키운 스탯이 러닝에서 어떻게 쓰이는지(RunTuning 공식 그대로).
        public static string ContestStatLine(SaveData s)
        {
            if (s == null) return "";
            var st = s.stats;
            float stamina01 = Mathf.Clamp01(st.stamina / (float)PlayerStats.StatMax);
            float agility01 = Mathf.Clamp01(st.agility / (float)PlayerStats.StatMax);
            float charm01 = Mathf.Clamp01(st.charm / (float)PlayerStats.StatMax);
            int hp = Mathf.RoundToInt(100f + st.stamina * 0.5f);
            int lane = Mathf.RoundToInt(38f * agility01);
            int near = Mathf.RoundToInt(100f * charm01);
            return Loc.T($"체력 {st.stamina} → HP {hp} · 순발력 {st.agility} → 레인 {lane}% 빠름 · 매력 {st.charm} → 니어미스 +{near}%",
                         $"STA {st.stamina} → HP {hp} · AGI {st.agility} → lanes {lane}% faster · CHA {st.charm} → near-miss +{near}%");
        }

        // ── 성장 순간(10 단위 돌파) ──
        public static string Milestone(PlayerStats before, PlayerStats after)
        {
            if (before == null || after == null) return null;
            if (after.stamina / 10 > before.stamina / 10) return Loc.T($"체력 {after.stamina / 10 * 10} 돌파 — 오름 정상까지 안 쉬고 간다.", $"Stamina {after.stamina / 10 * 10}!");
            if (after.agility / 10 > before.agility / 10) return Loc.T($"순발력 {after.agility / 10 * 10} 돌파 — 보드가 발에 붙는다.", $"Agility {after.agility / 10 * 10}!");
            if (after.charm / 10 > before.charm / 10) return Loc.T($"매력 {after.charm / 10 * 10} 돌파 — 삼춘들이 먼저 인사한다.", $"Charm {after.charm / 10 * 10}!");
            if (after.sense / 10 > before.sense / 10) return Loc.T($"감성 {after.sense / 10 * 10} 돌파 — 라디오 소리가 다르게 들린다.", $"Sense {after.sense / 10 * 10}!");
            return null;
        }
        /// 행동 뒤 새로 열린 카드(잠금 해제) 이름들.
        public static List<string> NewlyUnlocked(PlayerStats before, PlayerStats after, SeasonKind season)
        {
            var list = new List<string>();
            if (before == null || after == null) return list;
            foreach (var d in ScheduleTable.All)
            {
                if (d == null || !d.AvailableIn(season)) continue;
                if (d.LockReason(before) != null && d.LockReason(after) == null) list.Add(d.Name);
            }
            return list;
        }
    }
}
