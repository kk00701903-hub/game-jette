# -*- coding: utf-8 -*-
"""지금 기준 앵커-컷씬 형상 차이 순위 — 어제 큐레이션(curated.json)·요청서(final.json) 대비 변화까지."""
import io, json, os, datetime

OUT = r"C:\dev\game\Tools\KlingGen\out\shape_mismatch"
REPORT = os.path.join(OUT, "report.json")
BASE = os.path.join(OUT, "curated", "curated.json")
FINAL = os.path.join(OUT, "final", "final.json")
DEST = os.path.join(OUT, "README_지금기준.md")

HARD = ("missing_orange", "missing_sky", "missing_pale", "missing_navy", "missing_grey",
        "hair_looks_long", "hair_looks_short")


def load(p):
    return json.load(io.open(p, encoding="utf-8"))


cur = {r["id"]: r for r in load(REPORT)}
base = {r["id"]: r for r in load(BASE)}
req = {r["id"]: r for r in load(FINAL)}


def hard_of(r):
    return [f for f in r["flags"] if any(h in f for h in HARD)]


def mtime(p):
    try:
        return datetime.datetime.fromtimestamp(os.path.getmtime(p)).strftime("%m-%d %H:%M")
    except OSError:
        return "?"


rows = []
for rid, r in cur.items():
    hard = hard_of(r)
    # 심각도: 하드 플래그 개수를 우선, 그다음 mismatch
    rows.append({
        "id": rid,
        "keys": ",".join(r["keys"]),
        "mm": r["mismatch"],
        "torso": r["sim_torso"],
        "hair": r["sim_hair"],
        "hard": hard,
        "soft": [f for f in r["flags"] if f not in hard],
        "path": r["path"],
        "mtime": mtime(r["path"]),
        "prev": base.get(rid, {}).get("mismatch"),
        "req": req.get(rid, {}).get("severity"),
    })
rows.sort(key=lambda x: (-len(x["hard"]), -x["mm"]))

L = []
L.append("# 지금 기준 · 앵커 대비 형상 차이가 심한 컷")
L.append("")
L.append("- 앵커: `Tools/KlingGen/ref87/` · 컷씬: `Assets/Resources/CoastRun/컷씬이미지/`")
L.append("- 채점 대상 Cut_T: %d컷 · 시그니처 의상/머리 위반(하드 플래그)이 잡힌 컷: %d"
         % (len(rows), sum(1 for r in rows if r["hard"])))
L.append("- mm = 형상 불일치(0=동일, 1=완전 다름) · torso/hair = 앵커와의 의상·머리 유사도")
L.append("")

top = [r for r in rows if r["hard"]][:14]
L.append("## A. 지금도 심한 컷 — 시그니처 위반 + 불일치 상위 %d" % len(top))
L.append("")
L.append("| # | id | 인물 | mm | 어제 | torso | hair | 위반 | 파일 갱신 |")
L.append("|---|---|---|---:|---:|---:|---:|---|---|")
for i, r in enumerate(top, 1):
    prev = ("%.2f" % r["prev"]) if r["prev"] is not None else "—"
    L.append("| %d | `%s` | %s | %.2f | %s | %.2f | %.2f | %s | %s |"
             % (i, r["id"], r["keys"], r["mm"], prev, r["torso"], r["hair"],
                ", ".join(x.split(":")[-1] for x in r["hard"]), r["mtime"]))
L.append("")

L.append("## B. 어제 재생성 요청(HIGH/MID 15컷) 처리 결과")
L.append("")
L.append("| id | 등급 | 초점 | 어제 mm | 지금 mm | 변화 | 위반 남음 | 파일 갱신 |")
L.append("|---|---|---|---:|---:|---|---|---|")
for rid, rq in sorted(req.items(), key=lambda kv: (kv[1].get("severity") != "high", kv[0])):
    c = cur.get(rid)
    b = base.get(rid, {}).get("mismatch")
    if c is None:
        L.append("| `%s` | %s | %s | %s | 채점 제외 | — | — | %s |"
                 % (rid, rq.get("severity"), rq.get("focus"),
                    ("%.2f" % b) if b else "—", mtime(rq.get("src", ""))))
        continue
    hard = hard_of(c)
    delta = "—"
    if b:
        d = c["mismatch"] - b
        delta = ("개선 %.2f" % -d) if d < -0.03 else ("악화 %.2f" % d) if d > 0.03 else "거의 같음"
    L.append("| `%s` | %s | %s | %s | %.2f | %s | %s | %s |"
             % (rid, rq.get("severity"), rq.get("focus"), ("%.2f" % b) if b else "—",
                c["mismatch"], delta,
                ", ".join(x.split(":")[-1] for x in hard) if hard else "없음", mtime(c["path"])))
L.append("")

fixed = [r for r in rows if r["id"] in base and not r["hard"]]
L.append("## C. 어제 지적됐다가 지금은 시그니처 위반이 사라진 컷 (%d)" % len(fixed))
L.append("")
for r in sorted(fixed, key=lambda x: -x["mm"]):
    L.append("- `%s` (%s) mm %.2f ← 어제 %.2f · 갱신 %s"
             % (r["id"], r["keys"], r["mm"], base[r["id"]]["mismatch"], r["mtime"]))
L.append("")

L.append("## D. 경로")
L.append("")
for r in top:
    rel = r["path"].replace("c:\\dev\\game\\", "").replace("\\", "/")
    L.append("- `%s` → `%s`" % (r["id"], rel))
L.append("")

io.open(DEST, "w", encoding="utf-8").write("\n".join(L) + "\n")
print("hard=%d of %d" % (sum(1 for r in rows if r["hard"]), len(rows)))
print("wrote", DEST)
