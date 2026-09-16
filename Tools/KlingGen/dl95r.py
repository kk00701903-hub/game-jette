# -*- coding: utf-8 -*-
"""95차: 육성 그림 — Kling 웹 결과(urls95r.json id->[url...], picks95r.json id->index) 다운로드 →
   Raise_Girl_* : 마젠타 크로마키 → RGBA 컷아웃(긴 변 1024) → Assets/Resources/CoastRun/<id>.png
   Sched_*      : 4:3 센터 크롭 1024x768 → Assets/Resources/CoastRun/<id>.png
   UI_Goods_*   : 마젠타 키 → RGBA 256x256 아이콘
   python Tools/KlingGen/dl95r.py [--apply]"""
import json, os, sys, time, shutil, urllib.request
import numpy as np
from PIL import Image, ImageFilter, ImageDraw
ROOT = r'C:\dev\game'; KG = os.path.join(ROOT, 'Tools', 'KlingGen'); OUT = os.path.join(KG, 'out', 'web95r')
RES = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun'); OLD = os.path.join(KG, 'out', 'shape_mismatch', 'final', '_old95r')
os.makedirs(OUT, exist_ok=True); os.makedirs(OLD, exist_ok=True)
urls = json.load(open(os.path.join(OUT, 'urls95r.json'), encoding='utf-8'))
picks = json.load(open(os.path.join(OUT, 'picks95r.json'), encoding='utf-8'))

def fetch(url, path, tries=4):
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'Referer': 'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path + '.part', 'wb') as f: shutil.copyfileobj(r, f)
            os.replace(path + '.part', path); return True
        except Exception as e: print('   retry', t + 1, e); time.sleep(2 + 2 * t)
    return False

def cutout(im, size=1024):
    im = im.convert('RGB'); a = np.asarray(im).astype(np.int16)
    key = np.median(np.concatenate([a[:8, :8].reshape(-1, 3), a[-8:, -8:].reshape(-1, 3), a[:8, -8:].reshape(-1, 3), a[-8:, :8].reshape(-1, 3)]), axis=0)
    d = np.abs(a - key).sum(axis=2); r, g, b = a[..., 0], a[..., 1], a[..., 2]
    pinkish = (r > 150) & (g < 110) & (b > 110) & (r - g > 90)
    m = ~((d < 100) | pinkish)
    # 95차b: 배경 그라데이션·먼지 때문에 bbox 가 캔버스 전체가 되던 문제 → 침식한 마스크에서 픽셀이 충분한 행/열만으로 bbox
    me = np.asarray(Image.fromarray((m * 255).astype(np.uint8)).filter(ImageFilter.MinFilter(7))) > 0
    rows = np.where(me.sum(axis=1) >= 6)[0]; cols = np.where(me.sum(axis=0) >= 6)[0]
    if len(rows) == 0 or len(cols) == 0: return None
    y0, y1, x0, x1 = max(0, rows.min() - 6), min(m.shape[0] - 1, rows.max() + 6), max(0, cols.min() - 6), min(m.shape[1] - 1, cols.max() + 6)
    m[:y0, :] = False; m[y1 + 1:, :] = False; m[:, :x0] = False; m[:, x1 + 1:] = False
    fig = im.crop((x0, y0, x1 + 1, y1 + 1))
    mask = Image.fromarray((m[y0:y1 + 1, x0:x1 + 1] * 255).astype(np.uint8)).filter(ImageFilter.MinFilter(3)).filter(ImageFilter.GaussianBlur(0.8))
    fa = np.asarray(fig).astype(np.float32); rr, gg, bb = fa[..., 0], fa[..., 1], fa[..., 2]
    pink = ((rr - gg) > 70) & ((bb - gg) > 55)
    if pink.any():
        bl = np.asarray(fig.filter(ImageFilter.MedianFilter(7))).astype(np.float32); fa[pink] = bl[pink]; fig = Image.fromarray(np.clip(fa, 0, 255).astype(np.uint8))
    rgba = fig.convert('RGBA'); rgba.putalpha(mask)
    w, h = rgba.size; pad = int(max(w, h) * 0.02)
    canvas = Image.new('RGBA', (w + 2 * pad, h + pad), (0, 0, 0, 0)); canvas.paste(rgba, (pad, 0), rgba)
    sc = size / max(canvas.size); return canvas.resize((max(8, int(canvas.size[0] * sc)), max(8, int(canvas.size[1] * sc))), Image.LANCZOS)

for k, lst in urls.items():
    if k.startswith('CHIBI') or k.startswith('ANCHOR'): continue
    for i, u in enumerate(lst):
        p = os.path.join(OUT, '%s_%d.png' % (k, i + 1))
        if os.path.exists(p) and os.path.getsize(p) > 1000: continue
        ok = fetch(u, p); print('dl', k, i + 1, ok)
apply = '--apply' in sys.argv; done = []; tiles = []
for k, n in picks.items():
    src = os.path.join(OUT, '%s_%d.png' % (k, n))
    if not os.path.exists(src): print('MISSING', src); continue
    im = Image.open(src)
    if k.startswith('Sched_'):
        im = im.convert('RGB'); w, h = im.size; ch = int(w * 3 / 4)
        if h > ch: y0 = (h - ch) // 2; im = im.crop((0, y0, w, y0 + ch))
        else: cw = int(h * 4 / 3); x0 = (w - cw) // 2; im = im.crop((x0, 0, x0 + cw, h))
        res = im.resize((1024, 768), Image.LANCZOS)
    else:
        res = cutout(im, 256 if k.startswith('UI_Goods') else 1024)
        if res is None: print('no figure', k); continue
    fin = os.path.join(OUT, k + '_final.png'); res.save(fin, optimize=True)
    dst = os.path.join(RES, k + '.png')
    if apply:
        if os.path.exists(dst):
            bk = os.path.join(OLD, k + '.png')
            if not os.path.exists(bk): shutil.copy2(dst, bk)
        shutil.copy2(fin, dst); done.append(k)
    tiles.append((k, res))
print('applied', len(done), done)
if tiles:
    cols = 8; tw, th = 180, 320; rows = (len(tiles) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * tw, rows * (th + 16)), (60, 60, 60)); d = ImageDraw.Draw(sheet)
    for i, (k, t) in enumerate(tiles):
        t2 = t.convert('RGBA'); t2.thumbnail((tw, th)); x = (i % cols) * tw; y = (i // cols) * (th + 16)
        sheet.paste(t2, (x + (tw - t2.width) // 2, y + 16 + (th - t2.height) // 2), t2); d.text((x + 3, y + 2), k.replace('Raise_Girl_', ''), fill=(255, 255, 255))
    sp = os.path.join(ROOT, 'Claude outputs', 'r95_raising_new.jpg'); sheet.save(sp, quality=85); print('sheet', sp)
