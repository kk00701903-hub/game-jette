# -*- coding: utf-8 -*-
"""110차: 돌발이벤트 그림 8장 내려받기 + 대조 시트. --apply 면 Resources 로 넣는다."""
import json, sys, urllib.request
from pathlib import Path
from PIL import Image

ROOT = Path(r"C:\dev\game")
RAW = ROOT / "Tools" / "KlingGen" / "out" / "r110"
RAW.mkdir(parents=True, exist_ok=True)
DST = ROOT / "Assets" / "Resources" / "CoastRun"
SHEET = ROOT / "Claude outputs" / "r110_events.jpg"
URLS = json.loads((ROOT / "Tools" / "KlingGen" / "urls110.json").read_text(encoding="utf-8"))

def fetch(url, out):
    if out.exists() and out.stat().st_size > 10000:
        return out
    req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0"})
    with urllib.request.urlopen(req, timeout=120) as r:
        out.write_bytes(r.read())
    return out

got = []
for k, u in URLS.items():
    p = RAW / (k + ".png")
    try:
        fetch(u, p)
        got.append(k)
    except Exception as e:
        print("FAIL", k, e)

# 대조 시트 (2열)
cols, w = 2, 640
ims = [(k, Image.open(RAW / (k + ".png")).convert("RGB")) for k in got]
h = int(ims[0][1].height * w / ims[0][1].width) if ims else 360
rows = (len(ims) + cols - 1) // cols
sheet = Image.new("RGB", (cols * w, rows * (h + 22)), (255, 255, 255))
from PIL import ImageDraw
d = ImageDraw.Draw(sheet)
for i, (k, im) in enumerate(ims):
    x, y = (i % cols) * w, (i // cols) * (h + 22)
    d.text((x + 6, y + 5), k, fill=(0, 0, 0))
    sheet.paste(im.resize((w, h), Image.LANCZOS), (x, y + 22))
SHEET.parent.mkdir(parents=True, exist_ok=True)
sheet.save(SHEET, quality=88)
print("got", len(got), got)
print("sheet", SHEET)

if "--apply" in sys.argv:
    for k in got:
        im = Image.open(RAW / (k + ".png")).convert("RGB")
        im = im.resize((1280, 722), Image.LANCZOS)
        im.save(DST / ("UI_Ev_" + k + ".jpg"), quality=92)
    print("applied", len(got))
