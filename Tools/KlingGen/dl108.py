# -*- coding: utf-8 -*-
"""108차: 돌발 이벤트 그림 15(UI_Ev_*, 16:9 → 1280 폭 jpg) + 스케줄 카드 14(Sched_*, 4:3 → 1024x768 jpg) 다운로드 → Resources/CoastRun"""
import json, os, sys, time, shutil, urllib.request
ROOT=os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
OUT=os.path.join(ROOT,'Tools','KlingGen','out','web108'); RES=os.path.join(ROOT,'Assets','Resources','CoastRun')
os.makedirs(OUT,exist_ok=True)
from PIL import Image
def fetch(url,path,tries=4):
    req=urllib.request.Request(url,headers={'User-Agent':'Mozilla/5.0','Referer':'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req,timeout=60) as r, open(path+'.part','wb') as f: shutil.copyfileobj(r,f)
            os.replace(path+'.part',path); return True
        except Exception as e:
            print('   retry',t+1,e); time.sleep(2+2*t)
    return False
urls=json.load(open(os.path.join(os.path.dirname(os.path.abspath(__file__)),sys.argv[1] if len(sys.argv)>1 else 'urls108.json'),encoding='utf-8'))
n=0
for k,u in urls.items():
    p=os.path.join(OUT,k+'.png')
    if not (os.path.exists(p) and os.path.getsize(p)>1000):
        ok=fetch(u,p); print('dl',k,ok)
        if not ok: continue
    im=Image.open(p).convert('RGB')
    if k.startswith('Sched_'):
        w,h=im.size; tw,th=1024,768
        s=max(tw/w,th/h); im=im.resize((round(w*s),round(h*s)),Image.LANCZOS); x=(im.width-tw)//2; y=(im.height-th)//2; im=im.crop((x,y,x+tw,y+th))
    elif im.width!=1280: im=im.resize((1280,int(im.height*1280/im.width)),Image.LANCZOS)
    im.save(os.path.join(RES,k+'.jpg'),quality=88); n+=1
print('done',n,'/',len(urls))
