# -*- coding: utf-8 -*-
"""87차: 앵커 후보 다운로드 (urls.json: ID -> [url1,url2]) -> out/web87/<ID>_<n>.png
   + --pick 로 최종 앵커 복사: python dl87.py --pick H12=2 D12=2 ...  -> Tools/KlingGen/ref87/<ID>.png (768x1360 로 통일)
"""
import json, os, sys, time, shutil, urllib.request
ROOT=os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT=os.path.join(ROOT,'Tools','KlingGen','out','web87'); REF=os.path.join(ROOT,'Tools','KlingGen','ref87')
os.makedirs(OUT,exist_ok=True); os.makedirs(REF,exist_ok=True)
try:
    from PIL import Image
except ImportError:
    Image=None
def fetch(url,path,tries=4):
    req=urllib.request.Request(url,headers={'User-Agent':'Mozilla/5.0','Referer':'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req,timeout=60) as r, open(path+'.part','wb') as f: shutil.copyfileobj(r,f)
            os.replace(path+'.part',path); return True
        except Exception as e:
            print('   retry',t+1,e); time.sleep(2+2*t)
    return False
args=sys.argv[1:]
if '--pick' in args:
    for a in args[args.index('--pick')+1:]:
        k,n=a.split('='); src=os.path.join(OUT,'%s_%s.png'%(k,n)); dst=os.path.join(REF,(k[:-1] if k in ('DAD2','MOM2') else k)+'.png')
        if Image:
            im=Image.open(src).convert('RGB'); im=im.resize((768,1360),Image.LANCZOS); im.save(dst,optimize=True)
        else: shutil.copy(src,dst)
        print('pick',k,n,'->',dst)
    print('\n'.join(sorted(os.listdir(REF)))); sys.exit(0)
urls=json.load(open(os.path.join(OUT,'urls.json')))
for k,lst in urls.items():
    for i,u in enumerate(lst):
        p=os.path.join(OUT,'%s_%d.png'%(k,i+1))
        if os.path.exists(p) and os.path.getsize(p)>1000: print('skip',p); continue
        ok=fetch(u,p); print('dl',k,i+1,ok,os.path.getsize(p) if ok else 0)
        if ok and Image:
            im=Image.open(p).convert('RGB'); im.save(p[:-4]+'.jpg',quality=88)
print('done')
