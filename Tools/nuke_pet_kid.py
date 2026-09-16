"""Remove solid/cream backgrounds from My Room pet + kid sprites."""
from PIL import Image
import collections
from pathlib import Path


def nuke(path: Path, tol=38, soft=18):
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    px = im.load()
    samples = []
    for x in range(w):
        samples.append(px[x, 0][:3])
        samples.append(px[x, h - 1][:3])
    for y in range(h):
        samples.append(px[0, y][:3])
        samples.append(px[w - 1, y][:3])
    samples = [c for c in samples if sum(c) / 3 > 160] or [px[0, 0][:3]]
    br = sum(c[0] for c in samples) // len(samples)
    bg = sum(c[1] for c in samples) // len(samples)
    bb = sum(c[2] for c in samples) // len(samples)
    print(path.name, "bg~", (br, bg, bb), "size", (w, h))

    def dist(c):
        return abs(c[0] - br) + abs(c[1] - bg) + abs(c[2] - bb)

    visited = bytearray(w * h)
    q = collections.deque()

    def try_add(x, y):
        i = y * w + x
        if visited[i]:
            return
        if dist(px[x, y]) <= tol + soft:
            visited[i] = 1
            q.append((x, y))

    for x in range(w):
        try_add(x, 0)
        try_add(x, h - 1)
    for y in range(h):
        try_add(0, y)
        try_add(w - 1, y)

    while q:
        x, y = q.popleft()
        for nx, ny in ((x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)):
            if nx < 0 or ny < 0 or nx >= w or ny >= h:
                continue
            i = ny * w + nx
            if visited[i]:
                continue
            if dist(px[nx, ny]) <= tol:
                visited[i] = 1
                q.append((nx, ny))

    out_im = im.copy()
    opx = out_im.load()
    removed = 0
    for y in range(h):
        for x in range(w):
            i = y * w + x
            if not visited[i]:
                continue
            c = opx[x, y]
            d = dist(c)
            if d <= tol:
                opx[x, y] = (c[0], c[1], c[2], 0)
            else:
                a = int(255 * min(1.0, (d - tol) / max(1, soft)))
                opx[x, y] = (c[0], c[1], c[2], min(c[3], a))
            removed += 1
    out_im.save(path)
    print("  nuked", removed, "pixels ->", path)
    return out_im


def main():
    base = Path(r"c:/dev/game/Assets/Resources/CoastRun")
    for name in ["UI_Pet_WildGoose.png", "Raise_Kid.png"]:
        src = base / name
        bak = base / name.replace(".png", "_bak.png")
        if not bak.exists():
            Image.open(src).save(bak)
            print("backup", bak.name)
        # restore from bak if re-running
        Image.open(bak).save(src)

    nuke(base / "UI_Pet_WildGoose.png", tol=48, soft=24)
    nuke(base / "Raise_Kid.png", tol=44, soft=22)

    for name in ["UI_Pet_WildGoose.png", "Raise_Kid.png"]:
        im = Image.open(base / name).convert("RGBA")
        total = im.size[0] * im.size[1]
        clear = sum(1 for p in im.getdata() if p[3] < 10)
        print(name, "transparent%", round(100 * clear / total, 1), "corner", im.getpixel((2, 2)))


if __name__ == "__main__":
    main()
