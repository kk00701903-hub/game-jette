# 74차 스트레스·체력 밸런스 점검 — Survival.WeekTick + ScheduleJudge 규칙을 그대로 옮긴 시늉 시뮬레이션.
#   목적: 「몇 주 만에 번아웃에 닿는가 / 한 주 쉬면 회복되는가」만 본다(운은 평균값으로).
WEEKLY_BASE = 8
RUB = 3          # 쓰다듬기 주당 상한
MINI = 0         # 놀이 미니게임 보상(최대 9) — 보수적으로 0

def limit(stamina):
    return max(70, min(90, 70 + stamina // 10))

def stage(stress, stamina):
    lim = limit(stamina)
    if stress >= lim + 15: return 4
    if stress >= lim: return 3
    if stress >= 55: return 2
    if stress >= 30: return 1
    return 0

ACT = {   # (스트레스, 돈, 체력)
    "밥":      (-10 - 2, 0, 1),
    "알바":    (+18, 40, 2),
    "밤알바":  (+30, 80, -1),
    "연습":    (+12, 0, 1),
    "푸는놀이": (-7, -5, 1),
    "수영":    (-12, 0, 1),
}

def week(plan, stress, stamina, money, cond=80, hunger=80, slept=True):
    for a in plan:
        ds, dm, dst = ACT[a]
        stress += ds; money += dm; stamina += dst
    ds = WEEKLY_BASE
    ds += -5 if slept else 5
    ds += -2 if hunger >= 60 else 0
    ds += -2 if cond >= 70 else 0
    if stage(stress, stamina) >= 3 and ds < 0: ds = 0
    stress += ds - RUB - MINI
    stress = max(0, min(100, stress))
    return stress, stamina, money

def run(name, plan, weeks=14):
    stress, stamina, money = 0, 30, 300
    print(f"\n[{name}]  주차별 스트레스(한계) / 체력 / 돈")
    for w in range(1, weeks + 1):
        stress, stamina, money = week(plan, stress, stamina, money)
        tag = ["평온", "피곤", "지침", "번아웃", "위기"][stage(stress, stamina)]
        print(f"  {w:2d}주  스트레스 {stress:3d}(한계 {limit(stamina)})  {tag:4s}  체력 {stamina:3d}  돈 {money:5d}")
        if stage(stress, stamina) >= 3:
            print("   → 번아웃: 다음 한 주를 회복(밥+수영+푸는놀이)으로 써 보면")
            s2, st2, m2 = week(["밥", "수영", "푸는놀이"], stress, stamina, money)
            print(f"     회복 후 스트레스 {s2} ({['평온','피곤','지침','번아웃','위기'][stage(s2, st2)]})")
            break

run("돈 벌기 집중: 밥1 + 알바2", ["밥", "알바", "알바"])
run("밤알바 강행: 밥1 + 밤알바2", ["밥", "밤알바", "밤알바"])
run("균형: 밥1 + 알바1 + 푸는놀이1", ["밥", "알바", "푸는놀이"])
run("성장 집중: 밥1 + 연습2", ["밥", "연습", "연습"])
