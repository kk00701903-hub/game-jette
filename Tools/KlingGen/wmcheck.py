# -*- coding: utf-8 -*-
"""110차: 워터마크 제거 확인 — 최종 컷 jpg 의 우하단 코너만 모아 시트로."""
import os, glob
from PIL import Image, ImageDraw
ROOT = r'C:\dev\game'
CUT = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun', '컷씬이미지', '97_v7신규')
ids = ["V7_OPB_01","V7_OPB_03","V7_OPB_04","V7_E2_08","V7_N4_10","V7_E1_06","V7_N4_14","V7_E8_05","V7_E2_04","V7_N1_05","V7_N5_07","V7_N5_08"]
cw, ch = 300, 110
sheet = Image.new('RGB', (cw * 3, ch * ((len(ids) + 2) // 3) + 18 * ((len(ids) + 2) // 3)), (20, 20, 20))
d = ImageDraw.Draw(sheet)
for i, k in enumerate(ids):
    p = os.path.join(CUT, 'Cut_T_' + k + '.jpg')
    if not os.path.exists(p): continue
    im = Image.open(p).convert('RGB'); w, h = im.size
    c = im.crop((w - 230, h - 90, w, h)).resize((cw, ch), Image.LANCZOS)
    x = (i % 3) * cw; y = (i // 3) * (ch + 18)
    sheet.paste(c, (x, y + 18)); d.text((x + 4, y + 2), k, fill=(255, 255, 255))
out = os.path.join(ROOT, 'Claude outputs', 'r110_wm.jpg')
sheet.save(out, quality=88); print('sheet', out)
