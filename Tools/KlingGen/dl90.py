# -*- coding: utf-8 -*-
"""90차: README_수정요청 HIGH 8컷 — Kling 웹 결과 다운로드 → 720x1280 → _old 백업 → 런타임 경로 같은 이름으로 교체.
   urls: Tools/KlingGen/out/web90/urls90.json (id -> [url1,url2]) / picks: 아래 PICK
"""
import json, os, sys, time, shutil, urllib.request
ROOT = r'C:\dev\game'
KG = os.path.join(ROOT, 'Tools', 'KlingGen')
OUT = os.path.join(KG, 'out', 'web90')
FINAL = os.path.join(KG, 'out', 'shape_mismatch', 'final')
OLD = os.path.join(FINAL, '_old')
CUT = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun', '컷씬이미지')
os.makedirs(OUT, exist_ok=True); os.makedirs(OLD, exist_ok=True)
from PIL import Image

PICK = {'Cut_T_OP_02': 1, 'Cut_T_OP_03': 1, 'Cut_T_OP_04': 1, 'Cut_T_OP_07': 1,
        'Cut_T_N2_07': 1, 'Cut_T_N5_05': 2, 'Cut_T_N6_05': 2, 'Cut_T_E6_03': 1}
SRC = {
 'Cut_T_OP_02': r'00_오프닝_너와나의주파수\02_오프닝_너와나의주파수_Cut_T_OP_02.jpg',
 'Cut_T_OP_03': r'00_오프닝_너와나의주파수\03_오프닝_너와나의주파수_Cut_T_OP_03.jpg',
 'Cut_T_OP_04': r'00_오프닝_너와나의주파수\04_오프닝_너와나의주파수_Cut_T_OP_04.jpg',
 'Cut_T_OP_07': r'00_오프닝_너와나의주파수\07_오프닝_너와나의주파수_Cut_T_OP_07.jpg',
 'Cut_T_N2_07': r'03_하트\07_하트_Cut_T_N2_07.jpg',
 'Cut_T_N5_05': r'10_그밤\05_그밤_Cut_T_N5_05.jpg',
 'Cut_T_N6_05': r'12_스무살\05_스무살_Cut_T_N6_05.jpg',
 'Cut_T_E6_03': r'11_물때\03_물때_Cut_T_E6_03.jpg',
}
FOCUS = {'Cut_T_OP_02':'DAD','Cut_T_OP_03':'DAD','Cut_T_N2_07':'DAD','Cut_T_OP_07':'MOM','Cut_T_OP_04':'MOM',
         'Cut_T_N5_05':'H12','Cut_T_N6_05':'D12','Cut_T_E6_03':'D20'}

def fetch(url, path, tries=4):
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'Referer': 'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path + '.part', 'wb') as f:
                shutil.copyfileobj(r, f)
            os.replace(path + '.part', path); return True
        except Exception as e:
            print('   retry', t + 1, e); time.sleep(2 + 2 * t)
    return False

urls = json.load(open(os.path.join(OUT, 'urls90.json'), encoding='utf-8'))
log = []
# 1) 후보 전부 다운로드
for k, lst in urls.items():
    for i, u in enumerate(lst):
        p = os.path.join(OUT, '%s_%d.png' % (k, i + 1))
        if os.path.exists(p) and os.path.getsize(p) > 1000: print('skip', p); continue
        ok = fetch(u, p); print('dl', k, i + 1, ok, os.path.getsize(p) if ok else 0)
        if ok:
            Image.open(p).convert('RGB').save(p[:-4] + '.jpg', quality=88)
# 2) 채택본 → 720x1280 → 백업 → 교체
apply = '--apply' in sys.argv
for k, n in PICK.items():
    src = os.path.join(OUT, '%s_%d.png' % (k, n))
    if not os.path.exists(src): print('MISSING', src); continue
    im = Image.open(src).convert('RGB')
    w, h = im.size
    # 9:16 센터 크롭 후 720x1280
    tw = int(h * 9 / 16)
    if w > tw: x0 = (w - tw) // 2; im = im.crop((x0, 0, x0 + tw, h))
    elif w < tw: th = int(w * 16 / 9); y0 = (h - th) // 2; im = im.crop((0, y0, w, y0 + th))
    im = im.resize((720, 1280), Image.LANCZOS)
    fin = os.path.join(OUT, k + '_final.jpg'); im.save(fin, quality=90)
    dst = os.path.join(CUT, SRC[k])
    if apply:
        if os.path.exists(dst):
            shutil.copy2(dst, os.path.join(OLD, os.path.basename(dst)))
        im.save(dst, quality=90)
        hf = os.path.join(FINAL, 'high', FOCUS[k], k + '.jpg')
        if os.path.exists(os.path.dirname(hf)): im.save(hf, quality=90)
        log.append('%s <- #%d -> %s' % (k, n, dst))
    else:
        log.append('%s <- #%d (dry) %s' % (k, n, dst))
print('\n'.join(log)); print('done apply=%s' % apply)
