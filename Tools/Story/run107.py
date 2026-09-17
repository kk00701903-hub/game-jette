# -*- coding: utf-8 -*-
"""107차: 읽기용 v7 → 생성용 md → CinematicTable.cs + script/*.txt → ChapterScript.Data/En.cs"""
import subprocess, sys, os
ROOT = r'C:\dev\game'
os.chdir(ROOT)
steps = [
    [sys.executable, r'Tools\Story\build_v7_md.py', 'Docs\\CUTSCENE_SCRIPTS_v7_읽기용.md', r'Tools\Story\v7_map.txt', r'Tools\KlingGen\plan103.json', r'Docs\CUTSCENE_SCRIPTS_v7.md'],
    [sys.executable, r'Tools\Story\gen_cinematic_v7.py', '--txt'],
    [sys.executable, r'Tools\Story\cutscene_txt.py', 'import', r'Tools\Story\script'],
]
for st in steps:
    print('>>', ' '.join(st[1:]), flush=True)
    r = subprocess.run(st)
    if r.returncode != 0: print('FAILED', r.returncode); sys.exit(r.returncode)
print('ALL OK')
