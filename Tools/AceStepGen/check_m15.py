# -*- coding: utf-8 -*-
"""104차: BGM_M15 를 기존 곡들과 비교(길이·라우드니스·루프 이음새)하고 파형/스펙트로그램 그림을 남긴다."""
import io
import os

import numpy as np
import soundfile as sf

BGM = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)),
                                    "..", "..", "Assets", "Resources", "CoastRun", "BGM"))
SHOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "m15_check.png"))


def load(name):
    for ext in (".ogg", ".wav"):
        p = os.path.join(BGM, name + ext)
        if os.path.exists(p):
            x, sr = sf.read(p, always_2d=True)
            return x.mean(axis=1), sr, os.path.basename(p)
    return None, None, None


def main():
    rows = []
    for name in ("BGM_M15", "BGM_M13", "BGM_M14", "BGM_M5"):
        mono, sr, base = load(name)
        if mono is None:
            rows.append((name, "없음", "", "", ""))
            continue
        rows.append((base, "%.1f s" % (len(mono) / float(sr)),
                     "%.3f" % float(np.sqrt((mono ** 2).mean())),
                     "%.3f" % float(np.abs(mono).max()),
                     "%.4f" % float(abs(mono[0] - mono[-1]))))
    print("%-16s %8s %7s %7s %10s" % ("file", "length", "rms", "peak", "loop seam"))
    for r in rows:
        print("%-16s %8s %7s %7s %10s" % r)

    mono, sr, _ = load("BGM_M15")
    if mono is None:
        return
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    fig, ax = plt.subplots(2, 1, figsize=(11, 5.2), constrained_layout=True)
    t = np.arange(len(mono)) / float(sr)
    ax[0].plot(t, mono, lw=0.3, color="#3b6fb0")
    ax[0].set_title("BGM_M15 (raising hub) waveform - 92 s loop")
    ax[0].set_xlim(0, t[-1]); ax[0].set_ylabel("amp")
    ax[1].specgram(mono, NFFT=2048, Fs=sr, noverlap=1024, cmap="magma")
    ax[1].set_ylim(0, 8000); ax[1].set_ylabel("Hz"); ax[1].set_xlabel("seconds")
    fig.savefig(SHOT, dpi=110)
    print("plot: " + SHOT)


if __name__ == "__main__":
    main()
