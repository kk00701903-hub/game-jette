using UnityEngine;

namespace CoastRun
{
    /// 53차(사용자): 육성 **레벨·경험치**. 젤리·행동·러닝·미니게임·이야기가 경험치를 준다.
    ///   레벨업마다 스탯이 조금씩 오르고(체력 +2·순발력 +1·매력 +1·감성 +1), 러닝 코인 +1 %/Lv(최대 +30 %).
    ///   스토리 컷씬/엔딩은 레벨로 잠그지 않는다 — K-POP 은 돈·아이템 파밍용.
    ///   필요 경험치 Need(L) = 80 + 40·L (Lv1→2 120, Lv10→11 480, Lv20→21 880). 상한 MaxLevel.
    public static class LevelSystem
    {
        public const int MaxLevel = 40;
        // 경험치 표
        public const int ExpJelly = 5, ExpBigJelly = 15, ExpAction = 10, ExpActionGreat = 20, ExpActionFail = 4;
        public const int ExpStoryRun = 120, ExpKpopFinish = 60, ExpBoss = 20, ExpMinigame = 40, ExpChapterRead = 50, ExpRubMax = 2;

        public static int Need(int level) => 80 + 40 * Mathf.Max(1, level);

        private static SaveData S => GameManager.I != null ? GameManager.I.Save : null;
        public static int Level => S != null ? Mathf.Max(1, S.level) : 1;
        public static int Exp => S != null ? S.exp : 0;
        public static float Progress01 => S == null ? 0f : Mathf.Clamp01(S.exp / (float)Need(Level));

        /// 경험치 추가(레벨업 처리·토스트). 세이브가 없으면(순수 K-POP 러닝만 하는 유저) PlayerPrefs 에 쌓아 두었다가 세이브가 생기면 합친다.
        public static void Add(int amount, string reasonKo = null, string reasonEn = null)
        {
            if (amount <= 0) return;
            var s = S;
            if (s == null) { PlayerPrefs.SetInt(PendingKey, PlayerPrefs.GetInt(PendingKey, 0) + amount); return; }
            s.exp += amount;
            int ups = 0;
            while (s.level < MaxLevel && s.exp >= Need(s.level)) { s.exp -= Need(s.level); s.level++; ups++; OnLevelUp(s); }
            if (s.level >= MaxLevel) s.exp = Mathf.Min(s.exp, Need(s.level) - 1);
            if (ups > 0)
            {
                CoastToast.Show(Loc.T($"레벨 업! Lv {s.level}  · 체력 +{2 * ups} 순발력 +{ups} 매력 +{ups}", $"LEVEL UP! Lv {s.level}"));
                CoastAudioManager.PlayAnywhere(CoastSfx.RankS, 0.7f);
                if (GameManager.I != null) GameManager.I.Persist();
            }
        }
        public const string PendingKey = "CoastRun.PendingExp";
        /// 세이브 로드/새 회차 때 — 세이브 없이 모은 경험치 합치기.
        public static void FlushPending()
        {
            int p = PlayerPrefs.GetInt(PendingKey, 0);
            if (p <= 0 || S == null) return;
            PlayerPrefs.SetInt(PendingKey, 0);
            Add(p);
        }

        private static void OnLevelUp(SaveData s)
        {
            var st = s.stats;
            st.stamina = Mathf.Min(PlayerStats.StatMax, st.stamina + 2);
            st.agility = Mathf.Min(PlayerStats.StatMax, st.agility + 1);
            st.charm = Mathf.Min(PlayerStats.StatMax, st.charm + 1);
            st.sense = Mathf.Min(PlayerStats.StatMax, st.sense + 1);
        }

        /// 러닝 코인 배수: +1 %/Lv, 최대 +30 %.
        public static float CoinMul(SaveData s) => 1f + Mathf.Min(0.30f, 0.01f * ((s != null ? Mathf.Max(1, s.level) : 1) - 1));

        /// (호환) 예전 롱컷 레벨표 — 스토리는 더 이상 레벨로 잠기지 않는다.
        public static int LevelForLongCut(int chapter) => 1;
        public static bool LongCutOpen(int chapter) => true;

        /// 상태창 힌트 — 레벨 게이트 없음.
        public static string NextUnlockHint() =>
            Loc.T("K-POP 런으로 돈·아이템을 모아 스토리에 쓰세요", "Farm money & items in K-POP for story mode");

        /// 칭호(5레벨마다).
        public static string Title(int level)
        {
            if (level >= 30) return Loc.T("주파수의 주인", "Master of Frequency");
            if (level >= 25) return Loc.T("송전탑 러너", "Tower Runner");
            if (level >= 20) return Loc.T("해안도로 스타", "Coast Road Star");
            if (level >= 15) return Loc.T("스무 살 준비", "Ready for Twenty");
            if (level >= 10) return Loc.T("제주 소녀", "Jeju Girl");
            if (level >= 5) return Loc.T("초보 러너", "Rookie Runner");
            return Loc.T("새내기", "Newcomer");
        }

        /// 1000 단위는 k, 1,000,000 단위는 M — 육성 화면 돈·코인 표기(53차).
        public static string FormatK(long n)
        {
            if (n >= 1000000) return (n / 1000000f).ToString(n >= 10000000 ? "0" : "0.0") + "M";
            if (n >= 1000) return (n / 1000f).ToString(n >= 100000 ? "0" : "0.0") + "k";
            return n.ToString();
        }
    }
}
