# -*- coding: utf-8 -*-
"""84차: Kling 웹 결과(urls.json) 다운로드 → 720x1280 JPG 로 Assets/Resources/CoastRun/Cut_T_*.jpg
   + v3 유지 컷 복사(keep84.json: Cut_T_* -> Cut_S_*).

사용:  python Tools/KlingGen/dl84.py            (전부, 원본 png 있으면 다운로드 skip)
       python Tools/KlingGen/dl84.py --force ID1 ID2   (지정 컷 다시 받기, 옛 파일은 _old_ 접두로 보관)
       python Tools/KlingGen/dl84.py --keep-only       (유지 컷 복사만)
"""
import json, os, sys, time, shutil, urllib.request

ROOT = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))   # C:\dev\game
OUT  = os.path.join(ROOT, 'Tools', 'KlingGen', 'out', 'web84')
RES  = os.path.join(ROOT, 'Assets', 'Resources', 'CoastRun')
W, H = 720, 1280

args = sys.argv[1:]
force = '--force' in args
keep_only = '--keep-only' in args
only = [a for a in args if not a.startswith('--')]

try:
    from PIL import Image
except ImportError:
    Image = None
    print('!! PIL 없음: png 다운로드만, jpg 변환 생략 (pip install pillow)')

def fetch(url, path, tries=4):
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0', 'Referer': 'https://kling.ai/'})
    for t in range(tries):
        try:
            with urllib.request.urlopen(req, timeout=60) as r, open(path + '.part', 'wb') as f:
                shutil.copyfileobj(r, f)
            os.replace(path + '.part', path)
            return True
        except Exception as e:
            print('   retry', t + 1, e)
            time.sleep(2 + 2 * t)
    return False

def to_jpg(png, jpg):
    if Image is None:
        return False
    im = Image.open(png).convert('RGB')
    w, h = im.size
    # 9:16 로 중앙 크롭 후 리사이즈
    tw = int(h * W / H)
    if tw <= w:
        x0 = (w - tw) // 2
        im = im.crop((x0, 0, x0 + tw, h))
    else:
        th = int(w * H / W)
        y0 = (h - th) // 2
        im = im.crop((0, y0, w, y0 + th))
    im = im.resize((W, H), Image.LANCZOS)
    im.save(jpg, 'JPEG', quality=90, optimize=True)
    old_png = jpg[:-4] + '.png'
    if os.path.exists(old_png):
        os.remove(old_png)
    return True

def main():
    os.makedirs(OUT, exist_ok=True)
    os.makedirs(RES, exist_ok=True)
    ok = fail = 0
    if not keep_only:
        urls = json.load(open(os.path.join(OUT, 'urls.json'), encoding='utf-8'))
        ids = only or list(urls.keys())
        for i, cid in enumerate(ids, 1):
            url = urls.get(cid)
            if not url:
                print('?? no url', cid); fail += 1; continue
            png = os.path.join(OUT, cid + '.png')
            jpg = os.path.join(RES, cid + '.jpg')
            if os.path.exists(png) and not force:
                pass
            else:
                if force and os.path.exists(png):
                    os.replace(png, os.path.join(OUT, '_old_' + cid + '_' + time.strftime('%H%M%S') + '.png'))
                print('[%3d/%d] %s' % (i, len(ids), cid))
                if not fetch(url, png):
                    print('   FAIL', cid); fail += 1; continue
            if os.path.exists(png) and os.path.getsize(png) > 10000:
                if to_jpg(png, jpg):
                    ok += 1
                else:
                    ok += 1
            else:
                print('   BAD FILE', cid); fail += 1
        print('download/convert ok=%d fail=%d' % (ok, fail))
    # 유지 컷 복사
    keep = json.load(open(os.path.join(OUT, 'keep84.json'), encoding='utf-8'))
    kc = kf = 0
    for tid, sid in keep.items():
        src = None
        for ext in ('.jpg', '.png'):
            p = os.path.join(RES, sid + ext)
            if os.path.exists(p):
                src = p; break
        if not src:
            print('?? keep source missing', tid, '<-', sid); kf += 1; continue
        dst = os.path.join(RES, tid + os.path.splitext(src)[1])
        shutil.copyfile(src, dst)
        kc += 1
    print('keep copied=%d missing=%d' % (kc, kf))
    n = len([f for f in os.listdir(RES) if f.startswith('Cut_T_') and f.endswith(('.jpg', '.png'))])
    print('Cut_T_* in Resources:', n, '(expected 192)')

if __name__ == '__main__':
    main()
