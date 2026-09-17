# -*- coding: utf-8 -*-
"""105차(재미요소): 축제 배너 4 · 에필로그 7 · 일상 장면 12 = 23장 다운로드 → Resources/CoastRun/<ID>.jpg (16:9, 1280x720 로 통일)
   urls105.json: ID -> url(.origin). 원본은 Tools/KlingGen/out/web105/<ID>.png 에 남긴다."""
import json, os, sys, time, shutil, urllib.request
ROOT=os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT=os.path.join(ROOT,'Tools','KlingGen','out','web105'); RES=os.path.join(ROOT,'Assets','Resources','CoastRun')
os.makedirs(OUT,exist_ok=True)
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
urls=json.load(open(os.path.join(os.path.dirname(os.path.abspath(__file__)),(sys.argv[1] if len(sys.argv)>1 else 'urls105.json')),encoding='utf-8'))
ok_n=0
for k,u in urls.items():
    p=os.path.join(OUT,k+'.png')
    if not (os.path.exists(p) and os.path.getsize(p)>1000):
        ok=fetch(u,p); print('dl',k,ok,os.path.getsize(p) if ok else 0)
        if not ok: continue
    if Image:
        im=Image.open(p).convert('RGB')
        if im.width!=1280: im=im.resize((1280,int(im.height*1280/im.width)),Image.LANCZOS)
        im.save(os.path.join(RES,k+'.jpg'),quality=88)
    else: shutil.copy(p,os.path.join(RES,k+'.png'))
    ok_n+=1
print('done',ok_n,'/',len(urls))
