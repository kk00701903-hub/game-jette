# -*- coding: utf-8 -*-
"""103차: v7 신규 컷 그림(Kling 웹) — urls103.json(id->[url,url]) 다운로드 → picks103.json(id->1-based) 채택본을
   720x1280 jpg 로 Assets/Resources/CoastRun/컷씬이미지/97_v7신규/<id>.jpg 에 넣고 _paths.txt 에 폴더 등록. 시트 Claude outputs/r103_cuts_new.jpg
   python Tools/KlingGen/dl103.py [--apply]"""
import json, os, sys, time, shutil, urllib.request
ROOT = r'C:\dev\game'; KG = os.path.join(ROOT, 'Tools', 'KlingGen'); OUT = os.path.join(KG, 'out', 'web103')
CUT = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun', '컷씬이미지'); NEW = os.path.join(CUT, '97_v7신규')
os.makedirs(OUT, exist_ok=True)
from PIL import Image, ImageDraw
urls = json.load(open(os.path.join(OUT, 'urls103.json'), encoding='utf-8'))
picks = json.load(open(os.path.join(OUT, 'picks103.json'), encoding='utf-8'))

def fetch(url, path, tries=4):
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'Referer': 'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path + '.part', 'wb') as f: shutil.copyfileobj(r, f)
            os.replace(path + '.part', path); return True
        except Exception as e: print('   retry', t + 1, e); time.sleep(2 + 2 * t)
    return False

FORCE = set(json.load(open(os.path.join(OUT, 'force103.json'), encoding='utf-8'))) if os.path.exists(os.path.join(OUT, 'force103.json')) else set()
for k, lst in urls.items():
    for i, u in enumerate(lst):
        p = os.path.join(OUT, '%s_%d.png' % (k, i + 1))
        if k not in FORCE and os.path.exists(p) and os.path.getsize(p) > 1000: continue
        print('dl', k, i + 1, fetch(u, p))
apply = '--apply' in sys.argv; done = []
for k, n in picks.items():
    src = os.path.join(OUT, '%s_%d.png' % (k, n))
    if not os.path.exists(src): print('MISSING', src); continue
    im = Image.open(src).convert('RGB'); w, h = im.size
    tw = int(h * 9 / 16)
    if w > tw: x0 = (w - tw) // 2; im = im.crop((x0, 0, x0 + tw, h))
    elif w < tw: th = int(w * 16 / 9); y0 = (h - th) // 2; im = im.crop((0, y0, w, y0 + th))
    im = im.resize((720, 1280), Image.LANCZOS)
    fin = os.path.join(OUT, k + '_final.jpg'); im.save(fin, quality=90)
    if apply:
        os.makedirs(NEW, exist_ok=True)
        shutil.copy2(fin, os.path.join(NEW, 'Cut_T_' + k + '.jpg')); done.append(k)
if apply:
    pp = os.path.join(CUT, '_paths.txt'); txt = open(pp, encoding='utf-8-sig').read()
    line = 'CoastRun/컷씬이미지/97_v7신규'
    if line not in txt:
        open(pp, 'w', encoding='utf-8-sig', newline='\n').write(txt.rstrip('\n') + '\n' + line + '\n'); print('_paths.txt +', line)
print('applied', len(done))
ks = [k for k in picks if os.path.exists(os.path.join(OUT, k + '_final.jpg'))]
if ks:
    cols = 8; tw, th = 225, 400; rows = (len(ks) + cols - 1) // cols
    sheet = Image.new('RGB', (cols * tw, rows * (th + 18)), (20, 20, 20)); d = ImageDraw.Draw(sheet)
    for i, k in enumerate(ks):
        im = Image.open(os.path.join(OUT, k + '_final.jpg')).resize((tw, th)); x = (i % cols) * tw; y = (i // cols) * (th + 18)
        sheet.paste(im, (x, y + 18)); d.text((x + 4, y + 2), k, fill=(255, 255, 255))
    sp = os.path.join(ROOT, 'Claude outputs', 'r103_cuts_new.jpg'); sheet.save(sp, quality=85); print('sheet', sp)
