using System.Collections.Generic;
using UnityEngine;

namespace CoastRun
{
    /// 상점: 펫을 사고 장착한다. 소유는 SaveData.ownedPetMask 비트, 장착은 equippedPet.
    /// 52차: 펫은 스토리 주차·코인. 53차: 레벨 조건. Phase2: **8주차 또는 대회 2회 클리어** 해금.
    public static class PetShop
    {
        public const int UnlockWeek = 10;
        public const int UnlockContests = 3;
        private const string UnlockToastKey = "CoastRun_PetShopUnlockToast";
        public static readonly PetKind[] ForSale = { PetKind.Sparrow, PetKind.BlackPig, PetKind.BikerThug, PetKind.WildGoose };

        public static readonly Dictionary<PetKind, int> Price = new Dictionary<PetKind, int>
        {
            // 챕터 ~10·러닝 코인 축적 후에야 가장 싼 펫(참새)을 살 수 있는 수준
            { PetKind.Sparrow, 12000 },
            { PetKind.BlackPig, 24000 },
            { PetKind.BikerThug, 40000 },
            { PetKind.WildGoose, 65000 },
        };
        public static readonly Dictionary<PetKind, int> LevelReq = new Dictionary<PetKind, int>
        {
            { PetKind.Sparrow, 10 },
            { PetKind.BlackPig, 13 },
            { PetKind.BikerThug, 16 },
            { PetKind.WildGoose, 20 },
        };

        /// 러닝 챕터(대회) 중 cleared 개수.
        public static int ContestsCleared(SaveData s)
        {
            if (s?.chapters == null) return 0;
            int n = 0;
            foreach (int c in StoryProgress.RunChapters)
            {
                int i = c - 1;
                if (i >= 0 && i < s.chapters.Length && s.chapters[i] != null && s.chapters[i].cleared) n++;
            }
            return n;
        }

        public static string LockReason(SaveData s)
        {
            if (s == null) return Loc.T("세이브 없음", "No save");
            if (Unlocked(s)) return null;
            return Loc.T($"{UnlockWeek}주차 또는 대회 {UnlockContests}회 클리어 (지금 {s.week}주 · 대회 {ContestsCleared(s)}회)",
                $"Week {UnlockWeek} or {UnlockContests} contests cleared (now wk {s.week} · {ContestsCleared(s)} contests)");
        }

        /// 8주차 또는 대회 클리어 또는 비밀코드(devUnlockAll).
        public static bool Unlocked(SaveData s) =>
            s != null && (s.week >= UnlockWeek || ContestsCleared(s) >= UnlockContests
                || (GameManager.I != null && GameManager.I.DevUnlockAll));

        /// 해금 직후 첫 오픈 시 토스트 한 번.
        public static void NotifyUnlockToast(SaveData s)
        {
            if (!Unlocked(s) || PlayerPrefs.GetInt(UnlockToastKey, 0) != 0) return;
            PlayerPrefs.SetInt(UnlockToastKey, 1);
            PlayerPrefs.Save();
            CoastToast.Show(Loc.T("펫 상점 해금!", "Pet shop unlocked!"));
        }

        public static bool Owns(SaveData s, PetKind k) =>
            k == PetKind.None || (s != null && (s.ownedPetMask & (1 << (int)k)) != 0);

        public static bool CanAfford(SaveData s, PetKind k) =>
            s != null && Price.TryGetValue(k, out int p) && CoinWallet.TotalStatic >= p && Mathf.Max(1, s.level) >= LevelReq[k];

        public static bool TryBuy(SaveData s, PetKind k)
        {
            if (s == null || Owns(s, k) || !Unlocked(s) || !Price.TryGetValue(k, out int price)) return false;
            if (CoinWallet.TotalStatic < price || Mathf.Max(1, s.level) < LevelReq[k]) return false;
            if (!CoinWallet.TrySpendStatic(price)) return false;
            s.ownedPetMask |= 1 << (int)k;
            s.equippedPet = k;   // 사면 바로 장착(펫 없음 해제 없음)
            return true;
        }

        public static bool Equip(SaveData s, PetKind k)
        {
            if (s == null || k == PetKind.None || !Owns(s, k)) return false;
            s.equippedPet = k;
            return true;
        }

        /// 비밀코드 등 — 판매 펫 전부 소유 + 장착 보장. 변경 시 true.
        public static bool UnlockAllPets(SaveData s)
        {
            if (s == null) return false;
            int before = s.ownedPetMask;
            var prevEq = s.equippedPet;
            foreach (var k in ForSale)
                s.ownedPetMask |= 1 << (int)k;
            EnsureEquipped(s);
            return s.ownedPetMask != before || s.equippedPet != prevEq;
        }

        /// 86차: 보유 펫 중 첫 번째(없으면 None) — 장착이 비어 있을 때 K-POP 러닝 폴백.
        public static PetKind FirstOwned(SaveData s)
        {
            if (s == null) return PetKind.None;
            foreach (var k in ForSale) if (Owns(s, k)) return k;
            return PetKind.None;
        }

        /// 보유 펫이 있는데 장착이 비어 있으면 첫 보유 펫을 끼운다. 변경 시 true.
        public static bool EnsureEquipped(SaveData s)
        {
            if (s == null) return false;
            if (s.equippedPet != PetKind.None && Owns(s, s.equippedPet)) return false;
            foreach (var k in ForSale)
                if (Owns(s, k)) { s.equippedPet = k; return true; }
            if (s.equippedPet == PetKind.None) return false;
            s.equippedPet = PetKind.None;
            return true;
        }
    }
}
