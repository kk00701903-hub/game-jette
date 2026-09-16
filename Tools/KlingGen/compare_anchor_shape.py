"""Compare Cut_T stills vs ref87 anchors; flag large silhouette/costume mismatches."""
from __future__ import annotations

import json
import re
import shutil
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"c:/dev/game")
REF = ROOT / "Tools/KlingGen/ref87"
PLAN = ROOT / "Tools/KlingGen/plan84.json"
OUT = ROOT / "Tools/KlingGen/out/shape_mismatch"
SHEET = OUT / "_sheets"


def find_cut_dir() -> Path:
    coast = ROOT / "Assets/Resources/CoastRun"
    best, nbest = None, 0
    for p in coast.iterdir():
        if not p.is_dir():
            continue
        n = sum(1 for f in p.rglob("*") if f.suffix.lower() in {".png", ".jpg", ".jpeg", ".webp"})
        if n > nbest:
            best, nbest = p, n
    if best is None:
        raise SystemExit("cutscene dir not found")
    return best


def collect_cut_t(cut_dir: Path) -> dict[str, Path]:
    files: dict[str, Path] = {}
    for f in cut_dir.rglob("*"):
        if not f.is_file() or f.suffix.lower() not in {".png", ".jpg", ".jpeg", ".webp"}:
            continue
        m = re.search(r"(Cut_T_[A-Z0-9]+_\d+)", f.name, re.I)
        if not m:
            continue
        lid = m.group(1)
        prev = files.get(lid)
        if prev is None or f.stat().st_size > prev.stat().st_size:
            files[lid] = f
    return files


def load_rgb(path: Path, size: tuple[int, int] | None = None) -> np.ndarray:
    im = Image.open(path).convert("RGB")
    if size:
        im = im.resize(size, Image.Resampling.LANCZOS)
    return np.asarray(im, dtype=np.float32)


def fg_mask(rgb: np.ndarray) -> np.ndarray:
    """Drop near-white / pale-cream backgrounds common on anchors."""
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    bright = (r > 230) & (g > 220) & (b > 200) & (np.abs(r - g) < 25) & (np.abs(g - b) < 35)
    # also drop very pale flat areas
    pale = (r > 245) & (g > 240) & (b > 230)
    return ~(bright | pale)


def hsv_hist(rgb: np.ndarray, mask: np.ndarray | None = None, bins: int = 24) -> np.ndarray:
    r = rgb[..., 0] / 255.0
    g = rgb[..., 1] / 255.0
    b = rgb[..., 2] / 255.0
    mx = np.maximum(np.maximum(r, g), b)
    mn = np.minimum(np.minimum(r, g), b)
    df = mx - mn + 1e-6
    h = np.zeros_like(mx)
    mask_r = (mx == r) & (df > 1e-5)
    mask_g = (mx == g) & (df > 1e-5)
    mask_b = (mx == b) & (df > 1e-5)
    h[mask_r] = ((g - b) / df)[mask_r] % 6
    h[mask_g] = ((b - r) / df)[mask_g] + 2
    h[mask_b] = ((r - g) / df)[mask_b] + 4
    h = h / 6.0
    s = df / (mx + 1e-6)
    v = mx
    if mask is None:
        mask = np.ones(h.shape, dtype=bool)
    # prefer colorful / mid-value pixels (clothes, hair)
    keep = mask & (s > 0.08) & (v > 0.08) & (v < 0.98)
    if keep.sum() < 80:
        keep = mask & (v < 0.97)
    hh = h[keep]
    ss = s[keep]
    vv = v[keep]
    if hh.size == 0:
        return np.zeros(bins * 3, dtype=np.float64)
    h_hist, _ = np.histogram(hh, bins=bins, range=(0, 1), density=True)
    s_hist, _ = np.histogram(ss, bins=bins, range=(0, 1), density=True)
    v_hist, _ = np.histogram(vv, bins=bins, range=(0, 1), density=True)
    vec = np.concatenate([h_hist, s_hist, v_hist]).astype(np.float64)
    n = np.linalg.norm(vec) + 1e-9
    return vec / n


def region_hists(rgb: np.ndarray) -> dict[str, np.ndarray]:
    h, w = rgb.shape[:2]
    # focus on subject center column
    x0, x1 = int(w * 0.18), int(w * 0.82)
    y_hair0, y_hair1 = int(h * 0.02), int(h * 0.28)
    y_top0, y_top1 = int(h * 0.22), int(h * 0.55)
    y_bot0, y_bot1 = int(h * 0.50), int(h * 0.92)
    regions = {
        "hair": rgb[y_hair0:y_hair1, x0:x1],
        "torso": rgb[y_top0:y_top1, x0:x1],
        "legs": rgb[y_bot0:y_bot1, x0:x1],
        "full": rgb[int(h * 0.05) : int(h * 0.95), x0:x1],
    }
    out = {}
    for name, crop in regions.items():
        m = fg_mask(crop) if name != "full" else fg_mask(crop)
        # for stills, fg_mask may keep scenery — still ok for hue mixture
        out[name] = hsv_hist(crop, m)
    return out


def cos(a: np.ndarray, b: np.ndarray) -> float:
    return float(np.dot(a, b) / (np.linalg.norm(a) * np.linalg.norm(b) + 1e-9))


def edge_profile(rgb: np.ndarray) -> np.ndarray:
    """Vertical ink density profile — rough silhouette height shape."""
    gray = rgb.mean(axis=2)
    gy = np.abs(np.diff(gray, axis=0, prepend=gray[:1]))
    gx = np.abs(np.diff(gray, axis=1, prepend=gray[:, :1]))
    mag = gx + gy
    # center column
    h, w = mag.shape
    col = mag[:, int(w * 0.25) : int(w * 0.75)].mean(axis=1)
    # downsample to 32 bins
    bins = 32
    idx = np.linspace(0, h, bins + 1).astype(int)
    prof = np.array([col[idx[i] : idx[i + 1]].mean() if idx[i + 1] > idx[i] else 0 for i in range(bins)])
    prof = prof / (prof.sum() + 1e-9)
    return prof


def score_pair(anchor_rgb: np.ndarray, still_rgb: np.ndarray) -> dict:
    ah = region_hists(anchor_rgb)
    sh = region_hists(still_rgb)
    # weights: torso clothes matter most for identity, then hair
    sim_torso = cos(ah["torso"], sh["torso"])
    sim_hair = cos(ah["hair"], sh["hair"])
    sim_full = cos(ah["full"], sh["full"])
    sim_legs = cos(ah["legs"], sh["legs"])
    pe = 1.0 - float(np.abs(edge_profile(anchor_rgb) - edge_profile(still_rgb)).sum()) / 2.0
    # mismatch score high = bad
    # low clothing/hair similarity => high mismatch
    mismatch = (
        (1.0 - sim_torso) * 0.40
        + (1.0 - sim_hair) * 0.30
        + (1.0 - sim_full) * 0.15
        + (1.0 - sim_legs) * 0.05
        + (1.0 - max(0.0, pe)) * 0.10
    )
    return {
        "mismatch": mismatch,
        "sim_torso": sim_torso,
        "sim_hair": sim_hair,
        "sim_full": sim_full,
        "sim_legs": sim_legs,
        "sil": pe,
    }


# Expected costume cues (hue buckets) — soft rules for extra flagging
def costume_flags(key: str, still_rgb: np.ndarray) -> list[str]:
    flags = []
    h, w = still_rgb.shape[:2]
    torso = still_rgb[int(h * 0.28) : int(h * 0.55), int(w * 0.22) : int(w * 0.78)]
    hair = still_rgb[int(h * 0.04) : int(h * 0.26), int(w * 0.25) : int(w * 0.75)]
    r, g, b = torso[..., 0], torso[..., 1], torso[..., 2]
    hr, hg, hb = hair[..., 0], hair[..., 1], hair[..., 2]

    def frac(cond) -> float:
        return float(cond.mean()) if cond.size else 0.0

    # orange raincoat (KID/DAD)
    orange = (r > 140) & (g > 60) & (g < 160) & (b < 100) & (r > g) & (r > b)
    # sky-blue hoodie (H19)
    sky = (b > 130) & (g > 140) & (r < 160) & (b >= r - 10) & (g > r)
    # pale blue tee (H12)
    pale_blue = (b > 140) & (g > 150) & (r > 120) & (b > r) & ((b - r) < 80)
    # navy vest (D12)
    navy = (b > 60) & (r < 90) & (g < 110) & (b > r) & (b > g - 20) & ((r + g + b) / 3 < 140)
    # grey coat (D20)
    grey = (np.abs(r - g) < 25) & (np.abs(g - b) < 25) & (r > 70) & (r < 180)
    # dark hair amount in hair band
    dark_hair = (hr < 80) & (hg < 70) & (hb < 70)

    fo, fs, fp, fn, fg = frac(orange), frac(sky), frac(pale_blue), frac(navy), frac(grey)
    fdh = frac(dark_hair)

    if key in ("KID", "DAD") and fo < 0.02:
        flags.append("missing_orange_raincoat")
    if key == "H19" and fs < 0.015 and fp < 0.02:
        flags.append("missing_sky_hoodie")
    if key == "H12" and fp < 0.01 and fs < 0.01:
        flags.append("missing_pale_blue_tee")
    if key == "D12" and fn < 0.008:
        flags.append("missing_navy_vest_cue")
    if key == "D20" and fg < 0.04 and fo < 0.01:
        flags.append("missing_grey_coat_cue")
    if key in ("H12", "H19", "D12", "D20", "KID", "DAD", "MOM") and fdh < 0.02:
        flags.append("weak_dark_hair")
    # H12 bob vs H19 long: hair band darkness lower half of hair crop
    hair_lower = hair[hair.shape[0] // 2 :, :]
    longish = frac((hair_lower[..., 0] < 90) & (hair_lower[..., 1] < 80) & (hair_lower[..., 2] < 80))
    if key == "H12" and longish > 0.35:
        flags.append("hair_looks_long_for_H12")
    if key == "H19" and longish < 0.05 and fdh > 0.05:
        flags.append("hair_looks_short_for_H19")
    return flags


def thumb(path: Path, size=(180, 320)) -> Image.Image:
    im = Image.open(path).convert("RGB")
    im.thumbnail(size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGB", size, (245, 243, 238))
    canvas.paste(im, ((size[0] - im.width) // 2, (size[1] - im.height) // 2))
    return canvas


def make_sheet(rows: list[dict], anchors: dict[str, Path], title: str, dest: Path):
    cols = 1
    cell_w, cell_h = 180, 320
    pad = 8
    label_h = 44
    # each row: anchor(s) + still
    max_keys = max((len(r["keys"]) for r in rows), default=1)
    row_w = (max_keys + 1) * (cell_w + pad) + pad
    row_h = cell_h + label_h + pad
    W = row_w
    H = 40 + len(rows) * row_h + pad
    sheet = Image.new("RGB", (W, H), (250, 248, 244))
    draw = ImageDraw.Draw(sheet)
    try:
        font = ImageFont.truetype("arial.ttf", 14)
        font_sm = ImageFont.truetype("arial.ttf", 11)
    except Exception:
        font = ImageFont.load_default()
        font_sm = font
    draw.text((12, 10), title, fill=(30, 30, 30), font=font)
    y = 40
    for r in rows:
        x = pad
        for k in r["keys"]:
            if k in anchors:
                sheet.paste(thumb(anchors[k]), (x, y))
                draw.text((x, y + cell_h + 2), k, fill=(60, 60, 60), font=font_sm)
            x += cell_w + pad
        sheet.paste(thumb(Path(r["path"])), (x, y))
        label = f"{r['id']}  miss={r['mismatch']:.2f}  {','.join(r['flags'][:2])}"
        draw.text((x, y + cell_h + 2), label[:48], fill=(120, 40, 40), font=font_sm)
        y += row_h
    dest.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(dest, quality=90)
    print("sheet", dest)


def main():
    cut_dir = find_cut_dir()
    print("cut_dir", cut_dir)
    files = collect_cut_t(cut_dir)
    plan = json.loads(PLAN.read_text(encoding="utf-8"))
    plan_map = {e["id"]: list(e.get("keys") or []) for e in plan}

    anchors = {p.stem: p for p in REF.glob("*.png")}
    print("anchors", sorted(anchors))
    print("cut_t", len(files), "plan84", len(plan_map))

    # preload anchor rgb at fixed size
    ar = {k: load_rgb(p, (192, 340)) for k, p in anchors.items()}

    results = []
    for lid, path in sorted(files.items()):
        keys = plan_map.get(lid) or []
        if not keys:
            # try infer single known cast from filename episode only — skip auto score
            continue
        # skip pure prop cuts
        char_keys = [k for k in keys if k in ar]
        if not char_keys:
            continue
        still = load_rgb(path, (192, 340))
        best = None
        best_key = None
        for k in char_keys:
            sc = score_pair(ar[k], still)
            if best is None or sc["mismatch"] < best["mismatch"]:
                # best = lowest mismatch among assigned keys (fairest)
                best = sc
                best_key = k
        # also compute worst key mismatch for multi-char (report both)
        worst = None
        for k in char_keys:
            sc = score_pair(ar[k], still)
            if worst is None or sc["mismatch"] > worst["mismatch"]:
                worst = sc
        flags = []
        for k in char_keys:
            flags.extend(f"{k}:{fl}" for fl in costume_flags(k, still))
        results.append(
            {
                "id": lid,
                "path": str(path),
                "keys": char_keys,
                "best_key": best_key,
                "mismatch": best["mismatch"],
                "worst_mismatch": worst["mismatch"],
                "sim_torso": best["sim_torso"],
                "sim_hair": best["sim_hair"],
                "sim_full": best["sim_full"],
                "sil": best["sil"],
                "flags": flags,
            }
        )

    results.sort(key=lambda r: (-(1 if r["flags"] else 0), -r["mismatch"]))

    OUT.mkdir(parents=True, exist_ok=True)
    SHEET.mkdir(parents=True, exist_ok=True)
    report_path = OUT / "report.json"
    report_path.write_text(json.dumps(results, ensure_ascii=False, indent=2), encoding="utf-8")

    # Select severe mismatches
    severe = []
    for r in results:
        severe_flag = any(
            x in ",".join(r["flags"])
            for x in (
                "missing_orange",
                "missing_sky",
                "missing_pale",
                "missing_navy",
                "missing_grey",
                "hair_looks_long",
                "hair_looks_short",
            )
        )
        if r["mismatch"] >= 0.42 or severe_flag:
            severe.append(r)

    # de-dup keep unique ids
    seen = set()
    severe_u = []
    for r in severe:
        if r["id"] in seen:
            continue
        seen.add(r["id"])
        severe_u.append(r)

    print(f"scored={len(results)} severe={len(severe_u)}")

    # copy + summarize by character
    by_key = defaultdict(list)
    manifest = []
    for i, r in enumerate(severe_u, 1):
        src = Path(r["path"])
        # folder by primary (best) key
        primary = r["best_key"] or (r["keys"][0] if r["keys"] else "UNK")
        dest_dir = OUT / primary
        dest_dir.mkdir(parents=True, exist_ok=True)
        dest = dest_dir / f"{i:02d}_{r['id']}_{src.name}"
        shutil.copy2(src, dest)
        by_key[primary].append(r["id"])
        manifest.append(
            {
                **{k: r[k] for k in ("id", "keys", "best_key", "mismatch", "sim_torso", "sim_hair", "flags")},
                "copied_to": str(dest.relative_to(OUT)),
                "src": str(src),
            }
        )

    (OUT / "severe.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")

    lines = [
        "# Anchor vs Cutscene shape mismatch",
        "",
        f"- Anchors: `{REF}`",
        f"- Scored Cut_T with plan84 keys: {len(results)}",
        f"- Flagged severe: {len(severe_u)}",
        "",
        "## Counts by best-matching assigned key",
        "",
    ]
    for k, ids in sorted(by_key.items(), key=lambda kv: -len(kv[1])):
        lines.append(f"- **{k}**: {len(ids)} — {', '.join(ids)}")
    lines += ["", "## Flagged list (mismatch desc)", ""]
    lines.append("| id | keys | mismatch | torso | hair | flags |")
    lines.append("|---|---|---:|---:|---:|---|")
    for r in severe_u:
        fl = ", ".join(r["flags"]) if r["flags"] else "—"
        lines.append(
            f"| {r['id']} | {','.join(r['keys'])} | {r['mismatch']:.2f} | {r['sim_torso']:.2f} | {r['sim_hair']:.2f} | {fl} |"
        )
    (OUT / "README.md").write_text("\n".join(lines) + "\n", encoding="utf-8")

    # contact sheets per key (top 12)
    for k, ids in by_key.items():
        rows = [r for r in severe_u if (r["best_key"] or r["keys"][0]) == k][:12]
        if rows:
            make_sheet(rows, anchors, f"Mismatch · {k} (anchor | still)", SHEET / f"{k}.jpg")

    # top overall sheet
    make_sheet(severe_u[:20], anchors, "Top shape mismatches (anchor | still)", SHEET / "_top20.jpg")

    print("OUT", OUT)
    print("by_key", {k: len(v) for k, v in by_key.items()})


if __name__ == "__main__":
    main()
