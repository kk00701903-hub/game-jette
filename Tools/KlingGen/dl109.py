# -*- coding: utf-8 -*-
"""109차: Kling 웹 결과 다운로드 → out/web109/<id>.png. --apply 면 컷 그림은 720x1280 jpg 로 컷씬이미지/97_v7신규/Cut_T_<id>.jpg,
   RAISE_S1_HAPPY 는 배경을 지워 Resources/CoastRun/Raise_Girl_S1_Happy.png 로. 시트 Claude outputs/r109_cuts.jpg"""
import json, os, sys, time, shutil, urllib.request
ROOT = r'C:\dev\game'; KG = os.path.join(ROOT, 'Tools', 'KlingGen'); OUT = os.path.join(KG, 'out', 'web109')
CUT = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun', '컷씬이미지', '97_v7신규'); RES = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun')
os.makedirs(OUT, exist_ok=True)
from PIL import Image, ImageDraw, ImageFilter
import numpy as np
urls = json.load(open(os.path.join(KG, 'urls109.json'), encoding='utf-8'))
def fetch(url, path, tries=4):
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'Referer': 'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path + '.part', 'wb') as f: shutil.copyfileobj(r, f)
            os.replace(path + '.part', path); return True
        except Exception as e: print('   retry', t + 1, e); time.sleep(2 + 2 * t)
    return False
for k, lst in urls.items():
    u = lst[0] if isinstance(lst, list) else lst
    u = u.split(':360x')[0].split(':100x')[0] + '.origin'
    p = os.path.join(OUT, k + '.png')
    if os.path.exists(p) and os.path.getsize(p) > 1000: continue
    print('dl', k, fetch(u, p))
apply = '--apply' in sys.argv; done = []
def cutout_white(im, tol=28):
    from scipy import ndimage
    a = np.array(im.convert('RGB')).astype(float); h, w = a.shape[:2]
    d = np.sqrt(((a - 255) ** 2).sum(2)); reach = d < tol
    lab, n = ndimage.label(reach); border = set(np.unique(np.concatenate([lab[0], lab[-1], lab[:, 0], lab[:, -1]]))) - {0}
    bg = np.isin(lab, list(border)); fg = ~bg
    lab2, n2 = ndimage.label(fg); sizes = ndimage.sum(fg, lab2, range(1, n2 + 1)); keep = [i + 1 for i, s in enumerate(sizes) if s >= 0.02 * fg.sum()]
    fg = np.isin(lab2, keep); fg = ndimage.binary_fill_holes(fg)
    A = Image.fromarray((fg * 255).astype(np.uint8)).filter(ImageFilter.MinFilter(3)).filter(ImageFilter.GaussianBlur(0.8))
    out = im.convert('RGBA'); out.putalpha(A); return out
for k in urls:
    src = os.path.join(OUT, k + '.png')
    if not os.path.exists(src): print('MISSING', src); continue
    im = Image.open(src)
    if k == 'RAISE_S1_HAPPY':
        # 배경 제거는 scipy 가 있는 쪽(컨테이너)에서 — Raise_Girl_S1_Happy.png 는 따로 커밋
        continue
    im = im.convert('RGB'); w, h = im.size; tw = int(h * 9 / 16)
    if w > tw: x0 = (w - tw) // 2; im = im.crop((x0, 0, x0 + tw, h))
    elif w < tw: th = int(w * 16 / 9); y0 = (h - th) // 2; im = im.crop((0, y0, w, y0 + th))
    im = im.resize((720, 1280), Image.LANCZOS); fin = os.path.join(OUT, k + '_final.jpg'); im.save(fin, quality=90)
    if apply: os.makedirs(CUT, exist_ok=True); shutil.copy2(fin, os.path.join(CUT, 'Cut_T_' + k + '.jpg')); done.append(k)
print('applied', len(done), done)
ks = [k for k in urls if os.path.exists(os.path.join(OUT, k + '.png'))]
cols = 6; tw, th = 225, 400; rows = (len(ks) + cols - 1) // cols
sheet = Image.new('RGB', (cols * tw, rows * (th + 18)), (20, 20, 20)); d = ImageDraw.Draw(sheet)
for i, k in enumerate(ks):
    im = Image.open(os.path.join(OUT, k + '.png')).convert('RGB'); im.thumbnail((tw, th)); x = (i % cols) * tw; y = (i // cols) * (th + 18)
    sheet.paste(im, (x + (tw - im.width) // 2, y + 18)); d.text((x + 4, y + 2), k, fill=(255, 255, 255))
sp = os.path.join(ROOT, 'Claude outputs', 'r109_cuts.jpg'); sheet.save(sp, quality=85); print('sheet', sp)
