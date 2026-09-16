# -*- coding: utf-8 -*-
"""103차: 읽기용 v7 md + v7_map.txt + plan103.json → Docs/CUTSCENE_SCRIPTS_v7.md (생성용 표)"""
import re, json, io, sys
READ, MAP, PLAN, OUT = sys.argv[1:5]
t = io.open(READ, encoding='utf-8').read()
plan = {i['id']: i for i in json.load(io.open(PLAN, encoding='utf-8'))['I']}
secs = {}
order = []
for blk in re.split(r'\n(?=## )', t):
    m = re.match(r'## (OPEN|CS\d|EV\d+|END_A|END_B|END_TRUE) 「([^」]+)」', blk)
    if not m: continue
    cuts = []
    for ln in blk.split('\n'):
        mm = re.match(r'^(\d+)\.\s+(.*?)\s*$', ln)
        if mm: cuts.append(mm.group(2))
    secs[m.group(1)] = (m.group(2), cuts); order.append(m.group(1))
out = ['# 너와 나의 주파수 — 컷씬 대본 v7 (생성용 표 · v7.1 자막 + 그림 ID)', '',
       '> `python Tools/Story/gen_cinematic_v7.py --txt` 입력. 읽기용은 `CUTSCENE_SCRIPTS_v7_읽기용.md`, 문체는 `CUTSCENE_STYLE_GUIDE.md`.',
       '> 그림 ID 는 리소스 이름일 뿐 컷 번호와 무관 — 같은 그림을 여러 컷이 공유한다. 신규 그림은 `Cut_T_V7_*`(폴더 `컷씬이미지/97_v7신규`), 아직 없으면 뒤의 폴백 ID 로 표시.',
       '> 엄마 앵커(102차): 40세 얼굴 `Tools/KlingGen/out/mom_r102/ANCHOR_MOM40.png`(CS7-2 회상 얼굴 공개에만), 48세 `ANCHOR_MOM48.png`(현재), 회상 뒷모습 `ANCHOR_MOM_MEMORY.png`. 회상 컷에서 엄마 얼굴은 CS7-2 전까지 절대 보이지 않는다.', '']
for ln in io.open(MAP, encoding='utf-8'):
    if ln.startswith('#') or not ln.strip(): continue
    sid, bgm, sat, dur, body = ln.strip().split('|')
    title, cuts = list(secs[sid][0]), list(secs[sid][1]); title = secs[sid][0]
    toks = dict(tok.split(':') for tok in body.split())
    if sid == 'END_TRUE' and len(toks) == len(cuts) + 1:
        cuts.append('_(자막 없음)_ 세상이 하얗게 번진다.')   # 103차(사용자): 진엔딩 끝에 하얘지는 그림 한 컷(자막 없음)
    assert len(toks) == len(cuts), (sid, len(toks), len(cuts))
    meta = ('BGM_%s' % bgm[4:] if bgm.startswith('BGM') else bgm.replace('+', ' · '))   # 'CH4+BGM_M1' → 'CH4 · BGM_M1'
    out.append('## %s 「%s」 · %s · %d컷 × %ss · 채도 %s' % (sid, title, meta, len(cuts), dur, sat.replace('→', ' → ')))
    out.append('')
    for i, cap in enumerate(cuts, 1):
        ids = toks[str(i)].split('/')
        cid = 'Cut_T_' + ids[0]
        if ids[0].startswith('V7_'):
            p = plan[ids[0]]; anc = '+'.join(p['keys']) if p['keys'] else '-(배경)'
            fb = ' `Cut_T_%s`' % ids[1] if len(ids) > 1 else ''
            note = '(앵커 %s · **신규**%s)' % (anc, fb)
        else:
            note = '(앵커 기존 · **유지**)'
        out.append('%d. %s  ' % (i, cap))
        out.append('   `%s` %s  ' % (cid, note))
        out.append('')
io.open(OUT, 'w', encoding='utf-8', newline='\n').write('\n'.join(out) + '\n')
print('ok', OUT)
