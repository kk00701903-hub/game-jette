# -*- coding: utf-8 -*-
"""플래그된 컷을 대본(v6)·plan84 프롬프트와 나란히 놓아, 앵커 키 오탐을 걸러낸다."""
import io, json, os, re, sys

OUT = r"C:\dev\game\Tools\KlingGen\out\shape_mismatch"
DOC = r"C:\dev\game\Docs\CUTSCENE_SCRIPTS_v6.md"
PLAN = r"C:\dev\game\Tools\KlingGen\plan84.json"
HARD = ("missing_orange", "missing_sky", "missing_pale", "missing_navy", "missing_grey",
        "hair_looks_long", "hair_looks_short")

rep = json.load(io.open(os.path.join(OUT, "report.json"), encoding="utf-8"))
plan = {e["id"]: e for e in json.load(io.open(PLAN, encoding="utf-8"))}
doc = io.open(DOC, encoding="utf-8").read().split("\n")

# 대본에서 컷 id 가 적힌 줄 → 바로 위 서술 문장
script = {}
for i, line in enumerate(doc):
    for m in re.finditer(r"`(Cut_T_[A-Z0-9]+_\d+)`", line):
        body = ""
        for j in range(i - 1, max(0, i - 4), -1):
            if doc[j].strip():
                body = doc[j].strip()
                break
        script[m.group(1)] = {"anchor_note": line.strip(), "text": body}

rows = []
for r in rep:
    hard = [f for f in r["flags"] if any(h in f for h in HARD)]
    if hard:
        rows.append((len(hard), r["mismatch"], r, hard))
rows.sort(key=lambda x: (-x[0], -x[1]))

n = int(sys.argv[1]) if len(sys.argv) > 1 else 24
dest = os.path.join(OUT, "_crosscheck.md")
L = ["# 플래그 컷 · 대본 교차 확인", ""]
for k, (_, mm, r, hard) in enumerate(rows[:n], 1):
    s = script.get(r["id"], {})
    p = plan.get(r["id"], {})
    L.append("## %d. %s  (mm %.2f · keys %s)" % (k, r["id"], mm, ",".join(r["keys"])))
    L.append("- 플래그: %s" % ", ".join(x.split(":")[-1] for x in hard))
    L.append("- 대본: %s" % (s.get("text", "(대본에서 못 찾음 — v6 에서 빠진 컷?)")[:230]))
    L.append("- 앵커표기: %s" % s.get("anchor_note", "—")[:120])
    pr = (p.get("prompt") or "")[:230]
    L.append("- plan 프롬프트: %s" % pr)
    L.append("")
io.open(dest, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("wrote", dest, "cuts", min(n, len(rows)), "| 대본 매칭", sum(1 for _, _, r, _ in rows[:n] if r["id"] in script))
