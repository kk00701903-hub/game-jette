"""K-POP 러닝모드 옆 챕터 선택 칩 — 바와 같은 라벤더→핑크 유리 + 시안 네온 악센트.

출력: Assets/Resources/CoastRun/UI_ChapterChip.png (320×320, 알파)
중앙은 CH 번호/제목을 Unity Text 로 올리므로 비워 둔다(은은한 음표만).
"""
import math
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont, ImageChops

W = H = 320
PAD = 28
BX0, BY0, BX1, BY1 = PAD, PAD, W - PAD, H - PAD
R = 42
FONT = r"C:\Windows\Fonts\malgunbd.ttf"
OUT = os.path.join(os.path.dirname(__file__), "..", "..", "Assets", "Resources", "CoastRun", "UI_ChapterChip.png")


def lerp(a, b, t):
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(len(a)))


def multi(stops, t):
    for i in range(len(stops) - 1):
        t0, c0 = stops[i]
        t1, c1 = stops[i + 1]
        if t <= t1:
            return lerp(c0, c1, (t - t0) / max(1e-6, t1 - t0))
    return stops[-1][1]


def glow_layer(draw_fn, blur, color):
    l = Image.new("L", (W, H), 0)
    draw_fn(ImageDraw.Draw(l))
    l = l.filter(ImageFilter.GaussianBlur(blur))
    g = Image.new("RGBA", (W, H), color + (255,))
    g.putalpha(l)
    return g


def rr_mask(size, box, r):
    m = Image.new("L", size, 0)
    ImageDraw.Draw(m).rounded_rectangle(box, r, fill=255)
    return m


canvas = Image.new("RGBA", (W, H), (0, 0, 0, 0))

# 1. 바깥 글로우 (바와 동일 톤)
canvas.alpha_composite(
    glow_layer(lambda d: d.rounded_rectangle((BX0 - 2, BY0 - 2, BX1 + 2, BY1 + 2), R + 2, fill=255), 14, (150, 90, 255))
)
canvas.alpha_composite(
    glow_layer(lambda d: d.rounded_rectangle((BX0, BY0, BX1, BY1), R, fill=255), 5, (255, 120, 220))
)

# 2. 본체 그라데이션 (대각: 좌상 라벤더 → 우하 핑크)
bw, bh = BX1 - BX0, BY1 - BY0
body = Image.new("RGBA", (bw, bh))
px = body.load()
stops = [(0, (178, 170, 255)), (0.45, (210, 155, 245)), (0.78, (245, 150, 220)), (1, (255, 175, 230))]
for y in range(bh):
    for x in range(bw):
        t = 0.55 * (x / max(1, bw - 1)) + 0.45 * (y / max(1, bh - 1))
        c = multi(stops, t)
        # 유리: 위쪽 하이라이트
        hi = int(70 * max(0, 1 - y / (bh * 0.55)))
        r = min(255, c[0] + hi // 2)
        g = min(255, c[1] + hi // 2)
        b = min(255, c[2] + hi // 3)
        # 가운데 살짝 진한 심(텍스트가 뜨게)
        cx, cy = x / bw - 0.5, y / bh - 0.55
        core = max(0, 1 - (cx / 0.42) ** 2) * max(0, 1 - (cy / 0.5) ** 2)
        r = int(r * (1 - 0.18 * core) + 110 * 0.18 * core)
        g = int(g * (1 - 0.18 * core) + 70 * 0.18 * core)
        b = int(b * (1 - 0.18 * core) + 190 * 0.18 * core)
        px[x, y] = (r, g, b, 200)
bm = rr_mask(body.size, (0, 0, bw - 1, bh - 1), R)
body.putalpha(ImageChops.multiply(body.getchannel("A"), bm))
canvas.alpha_composite(body, (BX0, BY0))

# 3. 테두리
rim = Image.new("RGBA", (W, H), (0, 0, 0, 0))
rd = ImageDraw.Draw(rim)
rd.rounded_rectangle((BX0, BY0, BX1, BY1), R, outline=(255, 230, 250, 255), width=5)
rd.rounded_rectangle((BX0 + 7, BY0 + 7, BX1 - 7, BY1 - 7), R - 7, outline=(255, 255, 255, 80), width=2)
canvas.alpha_composite(rim.filter(ImageFilter.GaussianBlur(0.7)))

# 4. 상단 시안 네온 아이콘 — 작은 책갈피/리스트(챕터) + 음표
icon = Image.new("RGBA", (W, H), (0, 0, 0, 0))
idraw = ImageDraw.Draw(icon)
cx, cy = W // 2, BY0 + 52
# 미니 이퀄라이저 다이아몬드 (바 오른쪽 로고와 호응)
bars = [10, 18, 26, 18, 10]
bw_bar = 7
gap = 5
total_w = len(bars) * bw_bar + (len(bars) - 1) * gap
x0 = cx - total_w // 2
for i, h in enumerate(bars):
    x = x0 + i * (bw_bar + gap)
    idraw.rounded_rectangle((x, cy - h // 2, x + bw_bar, cy + h // 2), 3, fill=(160, 230, 255, 230))
# 글로우
glow = icon.filter(ImageFilter.GaussianBlur(4))
canvas.alpha_composite(Image.blend(Image.new("RGBA", (W, H), (0, 0, 0, 0)), glow, 0.85))
canvas.alpha_composite(icon)

# 5. 반짝이·작은 음표 (바와 동일 분위기)
spark = Image.new("RGBA", (W, H), (0, 0, 0, 0))
sd = ImageDraw.Draw(spark)
for ox, oy, s in ((58, 70, 7), (250, 78, 6), (70, 230, 5), (248, 235, 6), (160, 250, 4)):
    sd.polygon([(ox, oy - s), (ox + s * 0.35, oy), (ox, oy + s), (ox - s * 0.35, oy)], fill=(255, 255, 255, 160))
    sd.polygon([(ox - s, oy), (ox, oy + s * 0.35), (ox + s, oy), (ox, oy - s * 0.35)], fill=(255, 255, 255, 120))
# 작은 음표(유니코드)
try:
    f_note = ImageFont.truetype(FONT, 22)
except Exception:
    f_note = ImageFont.load_default()
sd.text((60, 175), "♪", font=f_note, fill=(180, 230, 255, 140))
sd.text((235, 165), "♫", font=f_note, fill=(255, 200, 240, 130))
canvas.alpha_composite(spark)

# 6. 상단 작은 "챕터" 워터마크 (한글) — Unity 라벨과 겹치지 않게 아주 옅게, 또는 생략하고
#    Unity 쪽이 "챕터" 텍스트를 올리므로 여기선 아이콘만 두고 끝.

os.makedirs(os.path.dirname(OUT), exist_ok=True)
canvas.save(OUT)
print("wrote", os.path.abspath(OUT), canvas.size)
