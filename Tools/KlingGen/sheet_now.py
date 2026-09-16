# -*- coding: utf-8 -*-
"""상위 문제 컷을 앵커와 나란히 붙인 대조 시트 — 육안 확인용."""
import io, json, os, sys
from PIL import Image, ImageDraw, ImageFont

OUT = r"C:\dev\game\Tools\KlingGen\out\shape_mismatch"
REF = r"C:\dev\game\Tools\KlingGen\ref87"
HARD = ("missing_orange", "missing_sky", "missing_pale", "missing_navy", "missing_grey",
        "hair_looks_long", "hair_looks_short")

rep = json.load(io.open(os.path.join(OUT, "report.json"), encoding="utf-8"))
mode = sys.argv[3] if len(sys.argv) > 3 else "hard"
rows = []
for r in rep:
    hard = [f for f in r["flags"] if any(h in f for h in HARD)]
    if mode == "hard" and hard:
        rows.append((len(hard), r["mismatch"], r, hard))
    elif mode == "soft" and not hard:
        rows.append((0, r["mismatch"], r, r["flags"][:2]))
rows.sort(key=lambda x: (-x[0], -x[1]))

start = int(sys.argv[1]) if len(sys.argv) > 1 else 0
count = int(sys.argv[2]) if len(sys.argv) > 2 else 6
rows = rows[start:start + count]

CW, CH = 230, 405
PAD, LAB = 10, 30
PER = 2  # 한 줄에 pair 2개


def cell(path):
    im = Image.open(path).convert("RGB")
    im.thumbnail((CW, CH), Image.Resampling.LANCZOS)
    c = Image.new("RGB", (CW, CH), (242, 240, 235))
    c.paste(im, ((CW - im.width) // 2, (CH - im.height) // 2))
    return c


pair_w = CW * 2 + PAD
lines = (len(rows) + PER - 1) // PER
W = PER * pair_w + PAD * (PER + 1)
H = lines * (CH + LAB + PAD) + PAD + 24
sheet = Image.new("RGB", (W, H), (252, 251, 248))
d = ImageDraw.Draw(sheet)
try:
    f = ImageFont.truetype("malgun.ttf", 15)
except Exception:
    f = ImageFont.load_default()

d.text((PAD, 5), "anchor | cutscene  (%d-%d)" % (start + 1, start + len(rows)), fill=(20, 20, 20), font=f)
for i, (_, mm, r, hard) in enumerate(rows):
    col, row = i % PER, i // PER
    x = PAD + col * (pair_w + PAD)
    y = 24 + PAD + row * (CH + LAB + PAD)
    key = r["best_key"] or r["keys"][0]
    ap = os.path.join(REF, key + ".png")
    if os.path.exists(ap):
        sheet.paste(cell(ap), (x, y))
    sheet.paste(cell(r["path"]), (x + CW + PAD, y))
    d.text((x, y + CH + 4), "%s  %s  mm=%.2f" % (key, r["id"], mm), fill=(30, 30, 30), font=f)
    d.text((x, y + CH + 18), ", ".join(h.split(":")[-1] for h in hard)[:56], fill=(160, 40, 40), font=f)

dest = os.path.join(OUT, "_sheets", "now_%s%02d.jpg" % ("" if mode == "hard" else "soft", start // max(1, count) + 1))
sheet.save(dest, quality=88)
print(dest, sheet.size)
