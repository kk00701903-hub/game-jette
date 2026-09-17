# -*- coding: utf-8 -*-
"""104차: BGM_M15(육성 허브) 후처리 — 렌더 당시 numpy/soundfile 이 없어 건너뛴 단계를 따로 돌린다.

무음 트림 → 꼬리 페이드 정리 → 루프 크로스페이드 → 라우드니스 정규화 → ogg 인코딩.
결과는 Assets/Resources/CoastRun/BGM/BGM_M15.ogg 를 덮어쓴다.
사용: python Tools/AceStepGen/post_m15.py [take1|take2]
"""
import os
import shutil
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, HERE)
import generate as g  # noqa: E402  (같은 폴더의 파이프라인 함수 재사용)

TAKES = os.path.join(HERE, "takes")
OUT = os.path.normpath(os.path.join(HERE, "..", "..", "Assets", "Resources", "CoastRun", "BGM"))


def stats(path, label):
    import numpy as np
    import soundfile as sf
    x, sr = sf.read(path, always_2d=True)
    mono = x.mean(axis=1)
    rms = float(np.sqrt((mono ** 2).mean()))
    peak = float(np.abs(mono).max())
    seam = float(abs(mono[0] - mono[-1]))
    print("  %-10s %6.1f s  rms %.3f  peak %.3f  loop seam %.4f"
          % (label, len(mono) / float(sr), rms, peak, seam))


def main():
    take = sys.argv[1] if len(sys.argv) > 1 else "take1"
    # 파이프라인 기본값 0.18 은 이 저장소의 다른 BGM 보다 많이 크다(M14 0.119 · M5 0.118 · M13 0.076).
    # 게임은 BedVol 고정 볼륨으로 틀기 때문에, 타이틀에서 육성으로 넘어갈 때 소리가 튀지 않게 이웃에 맞춘다.
    target = float(sys.argv[2]) if len(sys.argv) > 2 else 0.115
    src = os.path.join(TAKES, "BGM_M15_%s.wav" % take)
    if not os.path.exists(src):
        print("no take: " + src)
        return 1
    work = os.path.join(TAKES, "_m15_work.wav")
    shutil.copyfile(src, work)
    stats(work, "raw")
    g.trim_silence(work)
    g.trim_tail_fade(work)
    g.loop_crossfade(work, seconds=2.0)
    g.normalize_loudness(work, target_rms=target)
    stats(work, "processed")
    ogg = g.to_ogg(work)
    if not ogg.endswith(".ogg"):
        print("ogg encode failed")
        return 1
    dst = os.path.join(OUT, "BGM_M15.ogg")
    shutil.copyfile(ogg, dst)
    os.remove(ogg)
    print("wrote %s (%d KB)" % (dst, os.path.getsize(dst) // 1024))
    return 0


if __name__ == "__main__":
    sys.exit(main())
