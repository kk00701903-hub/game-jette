# 95차: 「도움」 버튼 아이콘 — Icon_Bulb.png 의 연파랑 원판을 지워 전구만 남긴다.
# 같은 줄의 ★·장바구니 아이콘이 배경 없는 그림이라, 원판이 있으면 분홍 알약 위에서 파란 덩어리로 보였다.
from PIL import Image
from pathlib import Path

SRC = Path(r"C:\dev\game\Assets\Resources\CoastRun\Icon_Bulb.png")
DST = Path(r"C:\dev\game\Assets\Resources\CoastRun\Icon_Help.png")

im = Image.open(SRC).convert("RGBA")
px = im.load()
w, h = im.size
print("src", im.size, "corner", px[0, 0], "center", px[w // 2, h // 2])

# 원판 색은 네 귀퉁이 안쪽에서 고르게 나타나는 연파랑. 화면 중앙 근처의 전구(노랑·흰·회보라)와 충분히 멀다.
disc = next(px[w // 2, y] for y in range(h) if px[w // 2, y][3] > 200)
print("disc color", disc)
tol = 26
cleared = 0
for y in range(h):
    for x in range(w):
        r, g, b, a = px[x, y]
        if a == 0:
            continue
        if abs(r - disc[0]) <= tol and abs(g - disc[1]) <= tol and abs(b - disc[2]) <= tol:
            px[x, y] = (r, g, b, 0)
            cleared += 1
print("cleared", cleared, "of", w * h)

a = im.split()[3]
bbox = a.getbbox()
print("bbox", bbox)
im.crop(bbox).save(DST)
print("wrote", DST, im.crop(bbox).size)
