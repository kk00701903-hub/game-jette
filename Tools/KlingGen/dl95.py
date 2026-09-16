# -*- coding: utf-8 -*-
"""95차: Kling 웹 결과(urls95.json id->[url...]) 다운로드 → picks95.json(id->1-based index) 채택본을 720x1280 으로 → 옛 파일 _old95 백업 → 런타임 경로(cut_paths95.json) 교체.
   python Tools/KlingGen/dl95.py            # 다운로드 + 시트만
   python Tools/KlingGen/dl95.py --apply    # 교체까지
"""
import json, os, sys, time, shutil, urllib.request
ROOT = r'C:\dev\game'; KG = os.path.join(ROOT, 'Tools', 'KlingGen')
OUT = os.path.join(KG, 'out', 'web95'); OLD = os.path.join(KG, 'out', 'shape_mismatch', 'final', '_old95')
CUT = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun', '컷씬이미지')
os.makedirs(OUT, exist_ok=True); os.makedirs(OLD, exist_ok=True)
from PIL import Image
urls = json.load(open(os.path.join(OUT, 'urls95.json'), encoding='utf-8'))
picks = json.load(open(os.path.join(OUT, 'picks95.json'), encoding='utf-8'))
paths = json.load(open(os.path.join(KG, 'cut_paths95.json'), encoding='utf-8'))

def fetch(url, path, tries=4):
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'Referer': 'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path + '.part', 'wb') as f: shutil.copyfileobj(r, f)
            os.replace(path + '.part', path); return True
        except Exception as e: print('   retry', t + 1, e); time.sleep(2 + 2 * t)
    return False

for k, lst in urls.items():
    if k.startswith('ANCHOR'): continue
    for i, u in enumerate(lst):
        p = os.path.join(OUT, '%s_%d.png' % (k, i + 1))
        if os.path.exists(p) and os.path.getsize(p) > 1000: continue
        ok = fetch(u, p); print('dl', k, i + 1, ok)
apply = '--apply' in sys.argv
done = []
for k, n in picks.items():
    if k not in paths: print('NO PATH', k); continue
    src = os.path.join(OUT, '%s_%d.png' % (k, n))
    if not os.path.exists(src): print('MISSING', src); continue
    im = Image.open(src).convert('RGB'); w, h = im.size
    tw = int(h * 9 / 16)
    if w > tw: x0 = (w - tw) // 2; im = im.crop((x0, 0, x0 + tw, h))
    elif w < tw: th = int(w * 16 / 9); y0 = (h - th) // 2; im = im.crop((0, y0, w, y0 + th))
    im = im.resize((720, 1280), Image.LANCZOS)
    fin = os.path.join(OUT, k + '_final.jpg'); im.save(fin, quality=90)
    dst = os.path.join(CUT, paths[k].replace('/', os.sep))
    if apply:
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        if os.path.exists(dst):
            bk = os.path.join(OLD, os.path.basename(dst))
            if not os.path.exists(bk): shutil.copy2(dst, bk)
        shutil.copy2(fin, dst); done.append(k)
print('applied', len(done), done)
# 검수 시트(채택본만, 6열)
ks = [k for k in picks if os.path.exists(os.path.join(OUT, k + '_final.jpg'))]
if ks:
    cols = 6; tw, th = 270, 480; rows = (len(ks) + cols - 1) // cols
    from PIL import ImageDraw
    sheet = Image.new('RGB', (cols * tw, rows * (th + 18)), (20, 20, 20)); d = ImageDraw.Draw(sheet)
    for i, k in enumerate(ks):
        im = Image.open(os.path.join(OUT, k + '_final.jpg')).resize((tw, th)); x = (i % cols) * tw; y = (i // cols) * (th + 18)
        sheet.paste(im, (x, y + 18)); d.text((x + 4, y + 2), k.replace('Cut_T_', ''), fill=(255, 255, 255))
    sp = os.path.join(ROOT, 'Claude outputs', 'r95_cuts_new.jpg'); sheet.save(sp, quality=85); print('sheet', sp)
