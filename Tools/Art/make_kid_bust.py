# 95차: 튜토리얼 화자(꼬마) 흉상 — Raise_Kid.png 의 투명 여백을 잘라 UI_Kid_Bust.png 으로.
# 원본은 위쪽 1/4 가량이 빈 하늘이라 말풍선 옆 작은 상자에 넣으면 아이가 떠 보였다.
from PIL import Image
from pathlib import Path

SRC = Path(r"C:\dev\game\Assets\Resources\CoastRun\Raise_Kid.png")
DST = Path(r"C:\dev\game\Assets\Resources\CoastRun\UI_Kid_Bust.png")

im = Image.open(SRC).convert("RGBA")
a = im.split()[3]
print("src", im.size, "alpha range", a.getextrema())
# 누끼 뒤 (0,0) 에 불투명 픽셀 한 알이 남아 있어 getbbox() 는 원본 크기를 그대로 준다.
# → 한 줄에 4알 이상 불투명한 줄만 '아이'로 세어 실제 몸통 사각형을 찾는다.
px = a.load()
w, h = im.size
rows = [y for y in range(h) if sum(1 for x in range(0, w, 2) if px[x, y] >= 128) >= 4]
cols = [x for x in range(w) if sum(1 for y in range(0, h, 2) if px[x, y] >= 128) >= 4]
if not rows or not cols:
    raise SystemExit("알파가 비어 있다 — 배경 제거가 안 된 그림")
bbox = (cols[0], rows[0], cols[-1] + 1, rows[-1] + 1)
print("body bbox", bbox)

pad = 6
x0 = max(0, bbox[0] - pad); y0 = max(0, bbox[1] - pad)
x1 = min(im.width, bbox[2] + pad); y1 = min(im.height, bbox[3] + pad)
out = im.crop((x0, y0, x1, y1))
out.save(DST)
print("wrote", DST, out.size)
