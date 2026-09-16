# -*- coding: utf-8 -*-
"""95차(v6): Docs/CUTSCENE_SCRIPTS_v6.md → Assets/_CoastRun/Scripts/Story/CinematicTable.cs (+ 리더용 txt).

    python Tools/Story/gen_cinematic_v4.py            # CinematicTable.cs 재생성 (MV 절은 기존 파일에서 보존)
    python Tools/Story/gen_cinematic_v4.py --txt      # Tools/Story/script/*.txt 도 v4 자막으로 다시 씀 (PRO·CHnn_Open·END_*)

md 규칙: `## <ID> 「제목」 · BGM · N컷 × Ds · 채도 S` 절 아래 `n. [**— 회상 —**] 자막` 줄 + 다음 줄 `` `Cut_T_ID` (앵커 … · **신규|유지** [`Cut_S_old`]) ``.
자막이 긴 컷(>108자)은 10~12초. 유지 컷은 v3 `Cut_S_*` 를 fallback 으로 둔다(keep84.json).
"""
import io, os, re, json, sys

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, '..', '..'))
MD = os.path.join(ROOT, 'Docs', 'CUTSCENE_SCRIPTS_v6.md')   # 95차: v6
CS = os.path.join(ROOT, 'Assets', '_CoastRun', 'Scripts', 'Story', 'CinematicTable.cs')
KEEP = os.path.join(ROOT, 'Tools', 'KlingGen', 'out', 'web84', 'keep84.json')
SCRIPT = os.path.join(HERE, 'script')

HDR = re.compile(r'^## (OPEN|CS\d|EV\d+|END_A|END_B|END_TRUE) 「([^」]+)」(.*)$')
VAR = {'OPEN': 'Opening', 'END_A': 'EndA', 'END_B': 'EndB', 'END_TRUE': 'EndTrue'}
KB = [0, 1, 2, 3, 1, 0, 3, 2]


def parse():
    lines = io.open(MD, encoding='utf-8').read().split('\n')
    secs, cur = [], None
    for ln in lines:
        m = HDR.match(ln)
        if m:
            cur = {'id': m.group(1), 'title': m.group(2), 'meta': m.group(3), 'cuts': []}; secs.append(cur); continue
        if ln.startswith('## ') and cur: cur = None
        if cur is None: continue
        m = re.match(r'^(\d+)\.\s+(.*?)\s*$', ln)
        if m:
            cap = m.group(2); tag = None
            t = re.match(r'^\*\*(— [^*]+? —)\*\*\s*(.*)$', cap)
            if t: tag, cap = t.group(1), t.group(2)
            if cap.startswith('_(자막 없음)_'): cap = ''   # 95차: 자막 없는 컷(그림·SFX 만)
            cur['cuts'].append({'n': int(m.group(1)), 'cap': cap, 'tag': tag, 'id': None}); continue
        m = re.search(r'`(Cut_T_[A-Z0-9_]+)`\s*\(앵커\s*([^·]*)·\s*\*\*(신규|유지)\*\*', ln)
        if m and cur['cuts'] and cur['cuts'][-1]['id'] is None:
            cur['cuts'][-1]['id'] = m.group(1)
    for s in secs:
        mm = re.search(r'(\d+)컷 × ([\d.]+)s', s['meta']); bg = re.search(r'BGM_M\d', s['meta'])
        sat = re.search(r'채도 ([\d.]+)(?: → ([\d.]+))?', s['meta']); ch = re.search(r'CH(\d+)', s['meta'])
        s['dur'] = float(mm.group(2)); s['bgm'] = bg.group(0) if bg else None
        s['sat'] = float(sat.group(1)); s['sat2'] = float(sat.group(2)) if sat.group(2) else None
        s['ch'] = int(ch.group(1)) if ch else None
        assert len(s['cuts']) == int(mm.group(1)), (s['id'], len(s['cuts']), mm.group(1))
        for c in s['cuts']: assert c['id'], (s['id'], c['n'])
    return secs


def esc(s): return s.replace('\\', '\\\\').replace('"', '\\"')


def dur_for(base, cap):
    n = len(cap)
    if n > 140: return 12.0
    if n > 125: return 11.0
    if n > 108: return 10.0
    return base


HEAD = '''using UnityEngine;

namespace CoastRun
{
    /// 68차: 시네마틱 대본 — 오프닝 + 컷씬 8편(+ 77차: 엔딩 3편·MV)을 **같은 형식**(컷 = 클립/스틸 + 자막 + 켄번즈 + 음악 한 곡 + 마무리 카드)으로.
    ///   95차: **대본 v6**(Docs/CUTSCENE_SCRIPTS_v6.md, 188컷: OPEN 4 꿈 조각 + CS1 이 오프닝) · 85차: 대본 v4 — OPEN 14컷(10.6s) · CS1~8(13~16컷, 8.5s) · **보조 컷씬 EV1~10(4컷 × 7s, 스토리 모드 CH2·4·6·8·9·11·13·14·15·19)** ·
    ///   엔딩 A 「엇갈린 정류장」 / B 「우유 두 병」 / TRUE 「맞닿은 주파수 91.9」(단서 여섯 개 clueMask 로 분기 — ClueSystem).
    ///   그림은 `Cut_T_<컷id>`(v4, 192장: Kling 웹 116 신규 + v3 `Cut_S_*` 복사 76) — 없으면 fallback(옛 v3 스틸). 자막이 긴 컷(>108자)은 10~12초.
    ///   **이 파일은 Tools/Story/gen_cinematic_v6.py 가 v6 md 에서 생성한다** — 자막을 고치려면 md 를 고치고 다시 생성할 것(MV 절만 손으로).
    public static class CinematicTable
    {
        public class Cut
        {
            public string clip, still, fallback, caption, tag;
            public float dur; public Vector2 from, to; public bool sepia;
            /// 85차: 컷별 채도 키프레임(-1 = Def.sat 그대로) — END_TRUE 2컷부터 1.0 복귀.
            public float sat = -1f;
            public Cut(string clip, string still, string caption, float dur = 9f, bool sepia = false, string tag = null, string fallback = null, int kb = 0)
            {
                this.clip = clip; this.still = still; this.caption = caption; this.dur = dur; this.sepia = sepia; this.tag = tag; this.fallback = fallback;
                // 켄번즈 4패턴: 0 = 천천히 밀고 들어감, 1 = 왼→오, 2 = 오→왼, 3 = 빠져나옴
                switch (kb % 4)
                {
                    case 0: from = new Vector2(1.06f, 0f); to = new Vector2(1.16f, 0f); break;
                    case 1: from = new Vector2(1.14f, 0.03f); to = new Vector2(1.10f, -0.03f); break;
                    case 2: from = new Vector2(1.14f, -0.03f); to = new Vector2(1.10f, 0.03f); break;
                    default: from = new Vector2(1.18f, 0f); to = new Vector2(1.06f, 0f); break;
                }
            }
        }

        public class Def
        {
            public string id, title, bgm, cardMain, cardSub; public float sat; public Cut[] cuts;
            /// 오프닝처럼 카드 뒤 음악 끝까지 붙들지(초). 0 = 2.2초.
            public float holdToSeconds;
            public bool gameTitleCard;
            /// 77차: 컷 길이 합(초) — 시네마 목록 표시용
            public float Length { get { float t = 0f; foreach (var c in cuts) t += c.dur; return t; } }
        }

        public static Def Get(string id)
        {
            switch (id)
            {
                case "OPEN": return Opening;
                case "CS1": return CS1; case "CS2": return CS2; case "CS3": return CS3; case "CS4": return CS4;
                case "CS5": return CS5; case "CS6": return CS6; case "CS7": return CS7; case "CS8": return CS8;
                case "EV1": return EV1; case "EV2": return EV2; case "EV3": return EV3; case "EV4": return EV4; case "EV5": return EV5;
                case "EV6": return EV6; case "EV7": return EV7; case "EV8": return EV8; case "EV9": return EV9; case "EV10": return EV10;
                case "END_A": return EndA; case "END_B": return EndB; case "END_TRUE": return EndTrue;
                case "MV": return MV;
                default: return null;
            }
        }
        public static Def Cutscene(int index) => Get("CS" + Mathf.Clamp(index, 1, 8));
        /// 85차: 보조 컷씬 EV1~10(스토리 모드 CH2·4·6·8·9·11·13·14·15·19)
        public static Def Event(int index) => Get("EV" + Mathf.Clamp(index, 1, EventCount));
        public const int EventCount = 10;
        /// 77차: 엔딩 시네마 id — ClueSystem.EndingId(clueMask) → "END_A" | "END_B" | "END_TRUE"
        public static readonly string[] EndingIds = { "END_A", "END_B", "END_TRUE" };
'''


def gen_cs(secs):
    keep = json.load(io.open(KEEP, encoding='utf-8')) if os.path.exists(KEEP) else {}
    old = io.open(CS, encoding='utf-8').read()
    mv_start = old.index('        // ── 75차(사용자): 뮤직비디오')
    mv_block = old[mv_start:old.rindex('    }\n}')].rstrip('\n')
    prev_bgm = 'BGM_M1'
    out = [HEAD]
    for s in secs:
        sid = s['id']; var = VAR.get(sid, sid)
        bgm = s['bgm'] or prev_bgm
        if s['bgm']: prev_bgm = s['bgm']
        label = {'OPEN': '오프닝', 'END_A': '엔딩 A', 'END_B': '엔딩 B', 'END_TRUE': '진엔딩'}.get(sid, ('보조 컷씬 ' + sid if sid.startswith('EV') else '컷씬 ' + sid[2:]))
        if sid == 'OPEN': extra = ', holdToSeconds = 12f, gameTitleCard = true, cardMain = "너와 나의 주파수", cardSub = "우리의 송전탑  ·  COAST RUN"'   # 95차: OPEN 4컷(꿈 조각) — 타이틀 카드 12초
        elif sid == 'END_A': extra = ', cardMain = "엇갈린 정류장", cardSub = "엔딩 A"'
        elif sid == 'END_B': extra = ', cardMain = "우유 두 병", cardSub = "엔딩 B"'
        elif sid == 'END_TRUE': extra = ', cardMain = "맞닿은 주파수 91.9", cardSub = "진엔딩"'
        elif sid.startswith('EV'): extra = ', cardMain = "%s", cardSub = "이야기 · 제 %d화"' % (esc(s['title']), s['ch'])
        else: extra = ', cardMain = "%s"' % esc(s['title'])
        chinfo = ' · CH%d' % s['ch'] if s['ch'] else ''
        out.append('        // ── %s 「%s」 · %s%s · %d컷 ──' % (label, s['title'], bgm, chinfo, len(s['cuts'])))
        out.append('        private static readonly Def %s = new Def' % var)
        out.append('        {')
        out.append('            id = "%s", title = "%s", bgm = "%s", sat = %.2ff%s,' % (sid, esc(s['title']), bgm, s['sat'], extra))
        out.append('            cuts = new[]')
        out.append('            {')
        for i, c in enumerate(s['cuts']):
            cid = c['id']; fb = keep.get(cid); fbs = '"%s"' % fb if fb else 'null'
            tag = '"%s"' % esc(c['tag']) if c['tag'] else 'null'
            sep = 'true' if c['tag'] else 'false'
            d = dur_for(s['dur'], c['cap'])
            init = ' { sat = %.2ff }' % s['sat2'] if (sid == 'END_TRUE' and s['sat2'] and i >= 1) else ''
            out.append('                new Cut(null, "%s", "%s", %.1ff, %s, %s, %s, %d)%s,' % (cid, esc(c['cap']), d, sep, tag, fbs, KB[i % 8], init))
        out.append('            }')
        out.append('        };')
        out.append('')
    out.append(mv_block)
    out.append('    }\n}\n')
    src = '\n'.join(out)
    io.open(CS, 'w', encoding='utf-8', newline='\n').write(src)
    print('CinematicTable.cs', len(src), 'bytes,', src.count('new Cut('), 'cuts')


# ── 리더용 txt(소설식 리더 StoryReaderUI / ChapterVN) — v4 자막을 NARR 줄로 ──
CS_CH = {1: 'CS1', 3: 'CS2', 5: 'CS3', 7: 'CS4', 10: 'CS5', 12: 'CS6', 17: 'CS7', 20: 'CS8'}
NO_CUT_TITLE = {16: ('지나간다', 'Passing by'), 18: ('조용해', 'Quiet')}   # 컷씬 없는 챕터의 제목(v3 유지)
EV_CH = {2: 'EV1', 4: 'EV2', 6: 'EV3', 8: 'EV4', 9: 'EV5', 11: 'EV6', 13: 'EV7', 14: 'EV8', 15: 'EV9', 19: 'EV10'}
TXT_HEAD = ('# 85차: 컷씬 대본 v4 — Docs/CUTSCENE_SCRIPTS_v4.md 의 자막을 지문(NARR)으로 옮긴 리더용. 시네마틱(CinematicTable)이 본편이고 이 파일은 「읽기」용.\n'
            '# 고친 뒤  python Tools/Story/cutscene_txt.py import Tools/Story/script  로 반영. 형식은 파일 아래 다른 파일과 같다(BG/CG/SAY/NARR/BGM).\n')


def gen_txt(secs):
    by = {s['id']: s for s in secs}
    files = sorted(os.listdir(SCRIPT))
    def find(sid):
        for f in files:
            if re.match(r'\d+_%s\.txt$' % re.escape(sid), f): return os.path.join(SCRIPT, f)
        return None
    def write(sid, sec, title=None):
        p = find(sid)
        if not p: print('?? no txt for', sid); return
        body = [TXT_HEAD]
        if title: body.append('제목: %s | EN: %s\n' % (title, title))
        body.append('')
        body.append('[0] BG | Blank | | | 현재 아래')
        if sec['bgm']: body.append('[1] BGM | %s | 0.55' % sec['bgm'].replace('BGM_', ''))
        n = 2
        for c in sec['cuts']:
            cap = c['cap']
            if c['tag']: cap = c['tag'] + ' ' + cap
            body.append('[%d] CG | %s | %s' % (n, c['id'][4:], c['id'])); n += 1
            body.append('[%d] NARR | %s' % (n, cap)); n += 1
        io.open(p, 'w', encoding='utf-8-sig', newline='\n').write('\n'.join(body) + '\n')
        return p
    write('PRO', by['OPEN'])
    for ch in range(1, 21):
        sid = CS_CH.get(ch) or EV_CH.get(ch)
        po = find('CH%02d_Open' % ch); pc = find('CH%02d_Close' % ch)
        if sid:
            write('CH%02d_Open' % ch, by[sid], by[sid]['title'])
        elif po:
            io.open(po, 'w', encoding='utf-8-sig', newline='\n').write(TXT_HEAD + '# 이 챕터는 컷씬 없음(육성·러닝만) — 비워 둔다(ChapterVN.Play 가 건너뜀).\n제목: %s | EN: %s\n' % (NO_CUT_TITLE.get(ch, ('%d화' % ch, 'Ch. %d' % ch))))
        if pc:
            io.open(pc, 'w', encoding='utf-8-sig', newline='\n').write(TXT_HEAD + '# v4: 챕터 마무리 대본 없음(시네마틱이 본편) — 비워 둔다.\n')
    for e in ('END_A', 'END_B', 'END_TRUE'):
        write(e, by[e])
    print('txt written')


if __name__ == '__main__':
    secs = parse()
    gen_cs(secs)
    if '--txt' in sys.argv: gen_txt(secs)
