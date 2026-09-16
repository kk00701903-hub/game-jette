# -*- coding: utf-8 -*-
"""검증으로 남은 실제 문제 컷만 모은 대조 시트."""
import io, json, os
from PIL import Image, ImageDraw, ImageFont

OUT = r"C:\dev\game\Tools\KlingGen\out\shape_mismatch"
REF = r"C:\dev\game\Tools\KlingGen\ref87"
rep = {r["id"]: r for r in json.load(io.open(os.path.join(OUT, "report.json"), encoding="utf-8"))}

ROWS = [
    ("DAD anchor", os.path.join(REF, "DAD.png"), "Cut_T_OP_03", "aboji wonkyeong - face/coat unreadable"),
    ("D12 anchor", os.path.join(REF, "D12.png"), "Cut_T_N6_05", "script says hospital room, art = city desk"),
    ("tower: lattice (N3_06)", rep["Cut_T_N3_06"]["path"], "Cut_T_E4_03", "same place drawn as red/white comm tower"),
    ("tower: lattice (N7_09)", rep["Cut_T_N7_09"]["path"], "Cut_T_N8_12", "comm tower again"),
]

CW, CH, PAD, LAB = 250, 440, 10, 34
W = 2 * CW + 3 * PAD
H = len(ROWS) * (CH + LAB + PAD) + 28
sheet = Image.new("RGB", (W, H), (252, 251, 248))
d = ImageDraw.Draw(sheet)
try:
    f = ImageFont.truetype("malgun.ttf", 15)
    fs = ImageFont.truetype("malgun.ttf", 13)
except Exception:
    f = fs = ImageFont.load_default()


def cell(p):
    im = Image.open(p).convert("RGB")
    im.thumbnail((CW, CH), Image.Resampling.LANCZOS)
    c = Image.new("RGB", (CW, CH), (242, 240, 235))
    c.paste(im, ((CW - im.width) // 2, (CH - im.height) // 2))
    return c


d.text((PAD, 6), "reference | cutscene", fill=(20, 20, 20), font=f)
y = 28
for left_label, left_path, cid, note in ROWS:
    sheet.paste(cell(left_path), (PAD, y))
    sheet.paste(cell(rep[cid]["path"]), (PAD * 2 + CW, y))
    d.text((PAD, y + CH + 3), left_label, fill=(60, 60, 60), font=fs)
    d.text((PAD * 2 + CW, y + CH + 3), "%s  mm=%.2f" % (cid, rep[cid]["mismatch"]), fill=(30, 30, 30), font=fs)
    d.text((PAD * 2 + CW, y + CH + 18), note[:52], fill=(160, 40, 40), font=fs)
    y += CH + LAB + PAD

dest = os.path.join(OUT, "_sheets", "findings.jpg")
sheet.save(dest, quality=90)
print(dest, sheet.size)
