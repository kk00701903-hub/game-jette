using System.Collections.Generic;
using UnityEngine;

namespace CoastRun
{
    /// 육성 화면 복귀 시 30% 확률로 뜨는 돌발 이벤트. 플레이어가 A/B 중 고른다.
    [System.Serializable]
    public class RandomEventDef
    {
        public string id;
        public string title;
        public string body;          // 선택 A 본문
        public string altBody;       // 선택 B 본문
        public string choiceA = "그래";   // 버튼 라벨 A
        public string choiceB = "아니";   // 버튼 라벨 B
        public float weight = 1f;
        public bool hasSeason;
        public SeasonKind season;
        /// 74차: 스트레스가 이 값 이상일 때만 뜨는 사건(프메의 반항·가출 계열). 0 이면 항상 후보.
        public int condStressMin;
        // Legacy condition fields kept for data; choices replace auto-branching.
        public StatKind condStat = StatKind.None;
        public int condMin;
        public bool condMoneyBelow;
        public int dStamina, dMoney, dHearts, dStress;
        public int altStamina, altMoney, altHearts, altStress;

        public bool Eval(PlayerStats s)
        {
            if (condMoneyBelow) return s.money < condMin;
            if (condStat == StatKind.None) return true;
            return s.Get(condStat) >= condMin;
        }

        public string ChoiceALabel => Loc.T(choiceA, choiceA);
        public string ChoiceBLabel => Loc.T(choiceB, choiceB);
    }

    public struct RandomEventResult
    {
        public RandomEventDef def;
        public bool conditionMet;   // true = chose A
        public int dStamina, dMoney, dHearts, dStress;
        public string Body => conditionMet || string.IsNullOrEmpty(def.altBody) ? def.body : def.altBody;
    }

    public static class RandomEventTable
    {
        public const float Chance = 0.30f;

        private static List<RandomEventDef> _all;

        public static IReadOnlyList<RandomEventDef> All
        {
            get { Ensure(); return _all; }
        }

        private static void Ensure()
        {
            if (_all != null) return;
            // 본편 v4 톤: 하늘 / 도윤 / 꼬마. 선택 A = 적극 / B = 소극·다른 길.
            _all = new List<RandomEventDef>
            {
                new RandomEventDef { id = "ev_orange", title = "이웃의 귤 선물", weight = 1.2f, hasSeason = true, season = SeasonKind.Autumn,
                    body = "옆집 삼춘이 귤 한 봉지를 건넸다. \"먹으멍 힘내라.\" 달콤해서 기운이 났다.",
                    altBody = "예의를 차리며 사양했다. 삼춘은 아쉽다는 듯 웃었다.",
                    choiceA = "감사히 받는다", choiceB = "사양한다",
                    dStamina = 2, dStress = -5, altStress = -1 },
                new RandomEventDef { id = "ev_radio", title = "주파수 질문", weight = 1.0f, condStat = StatKind.Charm, condMin = 40,
                    body = "꼬마가 고장 난 라디오를 가리켰다. \"몇이야?\" 91.9라고 했다. 눈이 동그래졌다.",
                    altBody = "라디오 주파수를 물었는데 얼버무렸다. 꼬마가 입을 다물었다.",
                    choiceA = "91.9라고 말한다", choiceB = "얼버무린다",
                    dHearts = 2, altHearts = 0, altStress = 2 },
                new RandomEventDef { id = "ev_rain", title = "오름에서 소나기", weight = 1.0f,
                    body = "갑자기 소나기. 뛰어서 비를 맞으며 내려왔다 — 상쾌하지만 지친다.",
                    altBody = "바위에서 잠깐 비를 피했다. 옷은 덜 젖었지만 시간이 좀 갔다.",
                    choiceA = "뛰어 내려간다", choiceB = "비를 피한다",
                    dStamina = -2, dStress = 4, altStamina = -1, altStress = 1 },
                new RandomEventDef { id = "ev_money", title = "길에서 주운 돈", weight = 0.8f,
                    body = "해안도로 벤치 밑에서 지폐를 주웠다. 주인을 못 찾아 일단 챙겼다.",
                    altBody = "주인을 찾아 파출소에 맡겼다. 마음은 편해졌다.",
                    choiceA = "일단 챙긴다", choiceB = "맡긴다",
                    dMoney = 30, altStress = -3, altHearts = 1 },
                new RandomEventDef { id = "ev_scooter", title = "스쿠터 펑크", weight = 0.8f, condMoneyBelow = true, condMin = 40,
                    body = "스쿠터 타이어가 터졌다. 수리비를 아끼려 끌고 왔다… 어깨가 아프다.",
                    altBody = "스쿠터 타이어가 터졌다. 수리비를 냈다.",
                    choiceA = "끌고 간다", choiceB = "수리한다",
                    dStress = 10, altMoney = -40 },
                new RandomEventDef { id = "ev_tower", title = "송전탑 아래서", weight = 0.9f, condStat = StatKind.Stamina, condMin = 45,
                    body = "송전탑까지 뛰어 올라갔더니 도윤이 멀리 서 있었다. 우유 병 두 개. 같이 바다를 봤다.",
                    altBody = "숨이 차서 중간에 앉았다. 멀리 송전탑만 바라봤다.",
                    choiceA = "끝까지 오른다", choiceB = "중간에 쉰다",
                    dHearts = 2, dStress = -3, altStress = 3 },
                new RandomEventDef { id = "ev_hospital", title = "약 먹는 뒷모습", weight = 0.9f,
                    body = "정류장에서 약을 삼키는 도윤을 봤다. 말없이 옆에 섰다. 바람이 찼다.",
                    altBody = "멀리서 배웅만 했다. 혼자 병을 여는 등이 작아 보였다.",
                    choiceA = "옆에 선다", choiceB = "멀리서 본다",
                    dHearts = 1, dStress = 2, altHearts = 0, altStress = 1 },
                new RandomEventDef { id = "ev_snow", title = "첫눈", weight = 1.0f, hasSeason = true, season = SeasonKind.Winter,
                    body = "첫눈. 꼬마가 우비 모자를 젖히고 웃었다. 성에 창에 얼굴이 둘 그려져 있었다.",
                    altBody = "창문으로만 첫눈을 봤다. 나가기엔 너무 추웠다.",
                    choiceA = "밖에 나간다", choiceB = "창문으로 본다",
                    dHearts = 1, dStress = -4, altStress = -1 },
                new RandomEventDef { id = "ev_sea", title = "여름 바다", weight = 1.0f, hasSeason = true, season = SeasonKind.Summer,
                    body = "갯바위에 발만 담갔다. 물때가 빠지며 짠내가 났다. 피로가 풀렸다.",
                    altBody = "바다를 보기만 하고 돌아왔다. 발은 안 적셨다.",
                    choiceA = "발을 담근다", choiceB = "구경만",
                    dStress = -8, dStamina = 1, altStress = -2 },
                new RandomEventDef { id = "ev_yuchae", title = "유채꽃밭", weight = 1.0f, hasSeason = true, season = SeasonKind.Spring,
                    body = "유채꽃밭에서 관광객이 사진을 부탁했다. 찍어주고 귤 하나 받았다.",
                    altBody = "바쁘다고 손을 흔들며 지나쳤다.",
                    choiceA = "사진을 찍어준다", choiceB = "그냥 지나간다",
                    dStress = -3, dMoney = 5, altStress = 0 },
                new RandomEventDef { id = "ev_milk", title = "가게 앞 우유", weight = 1.1f,
                    body = "주인 할머니가 \"오늘도 두 개라?\" 물었다. 도윤이 짧게 대답하고 나갔다. 따라가진 않았다.",
                    altBody = "도윤이 우유를 집는 걸 보고 발걸음을 돌렸다. 가슴이 뛰었다.",
                    choiceA = "자리를 지킨다", choiceB = "발길을 돌린다",
                    dHearts = 1, dStress = -2, altStress = 2 },
                new RandomEventDef { id = "ev_name", title = "바다누나", weight = 1.0f,
                    body = "꼬마가 또 「바다누나」라고 불렀다. 촌스럽다고 했지만 가슴이 저렸다.",
                    altBody = "이름을 묻자 입을 다물었다. 「말하면 다른 사람이 돼.」",
                    choiceA = "그대로 받아 준다", choiceB = "진짜 이름을 묻는다",
                    dHearts = 1, dStress = -2, altStress = 1 },
                // ── 74차: 스트레스가 쌓였을 때만 뜨는 사건 — 다마고치처럼 상태가 이야기로 터져 나온다.
                new RandomEventDef { id = "ev_cry", title = "새벽 세 시", weight = 2.0f, condStressMin = PlayerStats.StressWorn,
                    body = "이유도 없이 눈물이 났다. 베개에 얼굴을 묻고 울다 잠들었다. 아침엔 눈이 부었다.",
                    altBody = "참고 라디오를 켰다. 낯선 사연 하나가 밤을 데워 줬다.",
                    choiceA = "울다 잠든다", choiceB = "라디오를 켠다",
                    dStress = -12, dStamina = -1, altStress = -6, altHearts = 1 },
                new RandomEventDef { id = "ev_snap", title = "말이 먼저 나갔다", weight = 1.8f, condStressMin = 62,
                    body = "삼춘의 농담에 날카롭게 받아쳤다. 돌아서서 귤 한 봉지를 사 들고 사과하러 갔다.",
                    altBody = "모른 척 지나갔다. 며칠 동안 그 얼굴이 떠올랐다.",
                    choiceA = "사과하러 간다", choiceB = "모른 척한다",
                    dStress = -8, dMoney = -20, altStress = 8 },
                new RandomEventDef { id = "ev_runaway", title = "아무 버스나", weight = 2.2f, condStressMin = 72,
                    body = "번호도 안 보고 버스를 탔다. 종점 바다까지 갔다가 막차로 돌아왔다. 아무도 몰랐다.",
                    altBody = "정류장에 앉아 있다가 그냥 집으로 걸었다. 발이 무거웠다.",
                    choiceA = "종점까지 간다", choiceB = "돌아선다",
                    dStress = -25, dMoney = -30, dStamina = -2, altStress = -5 },
            };
        }

        /// stress: 스트레스 조건이 붙은 사건을 걸러 내기 위한 현재 값(기본 999 = 제한 없음).
        public static RandomEventDef Pick(SeasonKind season, double roll, int stress = 999)
        {
            Ensure();
            System.Func<RandomEventDef, bool> ok = e =>
                (!e.hasSeason || e.season == season) && stress >= e.condStressMin;
            float total = 0f;
            foreach (var e in _all)
                if (ok(e)) total += e.weight;
            if (total <= 0f) return _all[0];
            float r = (float)roll * total;
            foreach (var e in _all)
            {
                if (!ok(e)) continue;
                r -= e.weight;
                if (r <= 0f) return e;
            }
            return _all[0];
        }

        /// choice 0 = A (body + d*), choice 1 = B (altBody + alt*).
        public static RandomEventResult ApplyChoice(RandomEventDef ev, PlayerStats stats, int choice)
        {
            bool a = choice <= 0;
            var res = new RandomEventResult { def = ev, conditionMet = a };
            if (a)
            {
                res.dStamina = ev.dStamina;
                res.dMoney = ev.dMoney;
                res.dHearts = ev.dHearts;
                res.dStress = ev.dStress;
            }
            else
            {
                res.dStamina = ev.altStamina;
                res.dMoney = ev.altMoney;
                res.dHearts = ev.altHearts;
                res.dStress = ev.altStress;
            }
            stats.stamina += res.dStamina;
            stats.money += res.dMoney;
            stats.hearts += res.dHearts;
            stats.stress += res.dStress;
            stats.Clamp();
            return res;
        }

        /// Legacy: auto-pick branch by Eval (unused by Tama choice UI).
        public static RandomEventResult Apply(RandomEventDef ev, PlayerStats stats) =>
            ApplyChoice(ev, stats, ev.Eval(stats) ? 0 : 1);
    }
}
