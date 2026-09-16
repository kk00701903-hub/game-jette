# -*- coding: utf-8 -*-
"""102: mother anchor images (Kling web) -> Tools/KlingGen/out/mom_r102/
   candidates <key>_<n>.png + picked anchors ANCHOR_MOM40.png / ANCHOR_MOM48.png / ANCHOR_MOM_MEMORY.png / ANCHOR_MOM40_FULL.png
   python Tools/KlingGen/dl102.py"""
import json, os, shutil, time, urllib.request
KG = os.path.join(r'C:\dev\game', 'Tools', 'KlingGen'); OUT = os.path.join(KG, 'out', 'mom_r102')
os.makedirs(OUT, exist_ok=True)
urls = json.load(open(os.path.join(OUT, 'urls102.json'), encoding='utf-8'))
PICKS = {'ANCHOR_MOM40': ('MOM40P', 2), 'ANCHOR_MOM48': ('MOM48P', 1), 'ANCHOR_MOM_MEMORY': ('MOMFB', 1), 'ANCHOR_MOM40_FULL': ('MOM40N_b', 2)}

def fetch(url, path, tries=4):
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'Referer': 'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path + '.part', 'wb') as f: shutil.copyfileobj(r, f)
            os.replace(path + '.part', path); return True
        except Exception as e: print('   retry', t + 1, e); time.sleep(2 + 2 * t)
    return False

for k, lst in urls.items():
    for i, u in enumerate(lst):
        p = os.path.join(OUT, '%s_%d.png' % (k, i + 1))
        if os.path.exists(p) and os.path.getsize(p) > 1000: continue
        print('dl', k, i + 1, fetch(u, p))
for name, (k, n) in PICKS.items():
    src = os.path.join(OUT, '%s_%d.png' % (k, n))
    if os.path.exists(src): shutil.copyfile(src, os.path.join(OUT, name + '.png')); print('pick', name, '<-', k, n)
    else: print('MISSING', src)
print('DONE')
