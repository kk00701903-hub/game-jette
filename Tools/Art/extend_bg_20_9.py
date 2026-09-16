# -*- coding: utf-8 -*-
"""16:9(1080×1920) 배경을 제작 기준 20:9(1080×2400)으로 늘린다.

새 전략: 배경은 1080×2400 으로 길게 그려 두고 화면을 덮는다(S25 는 위아래 30px 만 잘림).
기존 16:9 그림은 중앙에 그대로 두고, 위아래 240px 을 가장자리 띠를 세로로 늘려 채운다
(하늘은 그라데이션이라 자연스럽고, 아래는 Play 판 밑 흙길만 뽑아 흐리게 — 오버스캔 영역).
"""
import os
import shutil
import sys
from PIL import Image, ImageFilter

ROOT = r"C:\dev\game"
RES = os.path.join(ROOT, "Assets", "Resources", "CoastRun")
BACKUP = os.path.join(ROOT, "Tools", "_bg20_backup")
TARGET_W, TARGET_H = 1080, 2400
# 가장자리 몇 줄만 뽑아 늘린다 — 구름·꽃 같은 무늬가 섞이면 이음선이 보이므로 얇게.
TOP_SRC, BOT_SRC = 10, 10
TOP_BLUR, BOT_BLUR = 3.0, 4.0      # 늘린 띠 흐리기(세로로 늘어난 결 감추기)


def blend_out(band, outward, darken=1.0):
    """이음선 쪽은 원본 그대로, 화면 밖으로 갈수록 가로로 완전히 흐린 색(=줄무늬 없음)으로 섞는다.

    가장자리 몇 줄을 세로로 늘리면 구름·꽃 경계가 세로 줄무늬로 남는다. 바깥쪽에서
    「줄마다 한 색」으로 수렴시키면 줄무늬가 사라지고, 이음선에서는 원본과 정확히 일치한다.
    """
    w, hh = band.size
    flat = band.resize((1, hh), Image.Resampling.BOX).resize((w, hh), Image.Resampling.BILINEAR)
    src, dst = band.load(), flat.load()
    out = Image.new("RGB", (w, hh))
    px = out.load()
    for y in range(hh):
        # t = 0 (이음선) → 1 (화면 밖 끝)
        t = (1.0 - y / max(1, hh - 1)) if outward == "up" else (y / max(1, hh - 1))
        k = t * t                                  # 이음선 근처는 거의 원본
        d = 1.0 - (1.0 - darken) * t               # 바깥으로 갈수록 살짝 어둡게(자연스러운 비네팅)
        for x in range(w):
            a, b = src[x, y], dst[x, y]
            px[x, y] = (int((a[0] * (1 - k) + b[0] * k) * d),
                        int((a[1] * (1 - k) + b[1] * k) * d),
                        int((a[2] * (1 - k) + b[2] * k) * d))
    return out


def extend(name):
    src = os.path.join(RES, name + ".png")
    im = Image.open(src).convert("RGB")
    w, h = im.size
    if (w, h) == (TARGET_W, TARGET_H):
        print("  already 20:9 -", name)
        return False
    if w != TARGET_W:
        im = im.resize((TARGET_W, round(h * TARGET_W / w)), Image.Resampling.LANCZOS)
        w, h = im.size
    pad = (TARGET_H - h) // 2
    if pad <= 0:
        print("  already tall -", name, im.size)
        return False

    os.makedirs(BACKUP, exist_ok=True)
    keep = os.path.join(BACKUP, "%s_%dx%d.png" % (name, w, h))
    if not os.path.exists(keep):
        shutil.copy2(src, keep)

    out = Image.new("RGB", (TARGET_W, TARGET_H))
    out.paste(im, (0, pad))
    top = im.crop((0, 0, w, TOP_SRC)).resize((TARGET_W, pad), Image.Resampling.LANCZOS)
    bot = im.crop((0, h - BOT_SRC, w, h)).resize((TARGET_W, pad), Image.Resampling.LANCZOS)
    top = blend_out(top.filter(ImageFilter.GaussianBlur(TOP_BLUR)), outward="up")
    bot = blend_out(bot.filter(ImageFilter.GaussianBlur(BOT_BLUR)), outward="down", darken=0.78)
    out.paste(top, (0, 0))
    out.paste(bot, (0, TARGET_H - pad))
    out.save(src, optimize=True)
    print("  %s: %dx%d -> %dx%d (pad %dpx)" % (name, w, h, TARGET_W, TARGET_H, pad))
    return True


names = sys.argv[1:] or ["UI_Title_Mock"]
print("extend to %dx%d" % (TARGET_W, TARGET_H))
for n in names:
    extend(n)
print("backup:", BACKUP)
