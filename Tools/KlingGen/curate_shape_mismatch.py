"""Curate hard shape/identity mismatches vs ref87; rebuild organized folder."""
from __future__ import annotations

import json
import shutil
from collections import defaultdict
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"c:/dev/game")
REF = ROOT / "Tools/KlingGen/ref87"
OUT = ROOT / "Tools/KlingGen/out/shape_mismatch"
CURATED = OUT / "curated"
SHEET = CURATED / "_sheets"
SEVERE = OUT / "severe.json"

# Hard silhouette / identity markers only (ignore weak_dark_hair noise)
HARD = (
    "hair_looks_long_for_H12",
    "hair_looks_short_for_H19",
    "missing_orange_raincoat",
    "missing_sky_hoodie",
    "missing_navy_vest_cue",
    "missing_grey_coat_cue",
    "missing_pale_blue_tee",
)

# From Docs/HANDOVER*: generated against older anchors / known regen watchlist
HANDOVER_WATCH = {
    "Cut_T_OP_02": "DAD/H12 — 옛 앵커 시절 생성(HANDOVER: 아빠 컷 재생성 후보)",
    "Cut_T_OP_03": "DAD — 옛 앵커 시절 생성(HANDOVER: 아빠 컷 재생성 후보)",
    "Cut_T_OP_04": "DAD/H12/MOM — 옛 앵커 시절 생성(HANDOVER: 아빠·엄마 컷)",
    "Cut_T_N2_07": "DAD/H12 — 옛 앵커 시절 생성(HANDOVER: 아빠 컷 재생성 후보)",
    "Cut_T_OP_07": "MOM — 엄마 바이블 교체 후 재검수 대상",
    "Cut_T_N2_05": "MOM/H12 — 엄마·하늘12 동시 등장, 바이블 교체 후 재검수",
}

REASON_KO = {
    "hair_looks_long_for_H12": "H12인데 머리가 단발(보브)이 아니라 길게 읽힘",
    "hair_looks_short_for_H19": "H19인데 긴 머리 실루엣이 약함(단발처럼 보임)",
    "missing_orange_raincoat": "주황 우비 실루엣이 거의 안 보임(KID/DAD 시그니처)",
    "missing_sky_hoodie": "하늘색 후드 실루엣/색이 약함(H19 시그니처)",
    "missing_navy_vest_cue": "네이비 조끼 큐가 약함(D12 시그니처)",
    "missing_grey_coat_cue": "회색 롱코트 큐가 약함(D20 시그니처)",
    "missing_pale_blue_tee": "연한 하늘색 티 큐가 약함(H12 시그니처)",
}


def thumb(path: Path, size=(160, 284)) -> Image.Image:
    im = Image.open(path).convert("RGB")
    im.thumbnail(size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGB", size, (245, 243, 238))
    canvas.paste(im, ((size[0] - im.width) // 2, (size[1] - im.height) // 2))
    return canvas


def make_sheet(rows, anchors, title, dest: Path):
    cell_w, cell_h = 160, 284
    pad = 8
    label_h = 52
    max_keys = max((len(r["keys"]) for r in rows), default=1)
    row_w = (max_keys + 1) * (cell_w + pad) + pad + 8
    row_h = cell_h + label_h + pad
    W = max(row_w, 520)
    H = 48 + len(rows) * row_h + pad
    sheet = Image.new("RGB", (W, H), (250, 248, 244))
    draw = ImageDraw.Draw(sheet)
    try:
        font = ImageFont.truetype("arial.ttf", 15)
        font_sm = ImageFont.truetype("arial.ttf", 11)
    except Exception:
        font = ImageFont.load_default()
        font_sm = font
    draw.text((12, 12), title, fill=(30, 30, 30), font=font)
    y = 48
    for r in rows:
        x = pad
        for k in r["keys"]:
            if k in anchors:
                sheet.paste(thumb(anchors[k]), (x, y))
                draw.text((x, y + cell_h + 2), k, fill=(50, 50, 50), font=font_sm)
            x += cell_w + pad
        sheet.paste(thumb(Path(r["src"])), (x, y))
        reason = r.get("reason_short") or ",".join(r.get("hard") or [])
        draw.text((x, y + cell_h + 2), r["id"], fill=(140, 40, 40), font=font_sm)
        draw.text((pad, y + cell_h + 20), reason[:90], fill=(70, 70, 70), font=font_sm)
        y += row_h
    dest.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(dest, quality=92)
    print("sheet", dest)


def main():
    severe = json.loads(SEVERE.read_text(encoding="utf-8"))
    by_id = {r["id"]: r for r in severe}

    curated = []
    for r in severe:
        flags = ",".join(r.get("flags") or [])
        hits = [h for h in HARD if h in flags]
        hair = any("hair_looks" in h for h in hits)
        # Costume miss only when mismatch is high OR character is solo-key
        solo = len(r.get("keys") or []) == 1
        costume = [h for h in hits if h.startswith("missing_")]
        keep = False
        hard = []
        if hair:
            keep = True
            hard.extend([h for h in hits if "hair_looks" in h])
        # strong costume misses
        for h in costume:
            if r["mismatch"] >= 0.58 or solo:
                # skip sky_hoodie misses that are likely night/silhouette (keep only extreme)
                if h == "missing_sky_hoodie" and r["mismatch"] < 0.62 and not solo:
                    continue
                if h == "missing_pale_blue_tee" and r["mismatch"] < 0.60 and not solo:
                    continue
                keep = True
                hard.append(h)
        note = HANDOVER_WATCH.get(r["id"])
        if note and r["id"] not in {c["id"] for c in curated}:
            keep = True
            if not hard:
                hard = ["handover_watch"]
        if not keep:
            continue
        hard = list(dict.fromkeys(hard))
        reasons = []
        for h in hard:
            if h == "handover_watch":
                reasons.append(note)
            else:
                reasons.append(REASON_KO.get(h, h))
        curated.append(
            {
                **r,
                "hard": hard,
                "reason": " · ".join(reasons),
                "reason_short": " / ".join(hard),
                "handover": note,
            }
        )

    # ensure handover watchlist entries present even if not in severe
    for lid, note in HANDOVER_WATCH.items():
        if any(c["id"] == lid for c in curated):
            continue
        if lid in by_id:
            r = by_id[lid]
            curated.append(
                {
                    **r,
                    "hard": ["handover_watch"],
                    "reason": note,
                    "reason_short": "handover_watch",
                    "handover": note,
                }
            )

    # priority sort: hair shape first, then handover, then mismatch
    def rank(c):
        hair_n = sum(1 for h in c["hard"] if "hair_looks" in h)
        how = 1 if c.get("handover") else 0
        return (-hair_n, -how, -c.get("mismatch", 0))

    curated.sort(key=rank)

    # rebuild curated folder
    if CURATED.exists():
        shutil.rmtree(CURATED)
    CURATED.mkdir(parents=True)
    SHEET.mkdir(parents=True)

    anchors = {p.stem: p for p in REF.glob("*.png")}
    by_key = defaultdict(list)
    rows_out = []
    for i, r in enumerate(curated, 1):
        primary = r.get("best_key") or (r["keys"][0] if r.get("keys") else "UNK")
        # prefer hair-related key for folder
        for h in r["hard"]:
            if "H12" in h:
                primary = "H12"
            elif "H19" in h:
                primary = "H19"
            elif "orange" in h and "KID" in r.get("keys", []):
                primary = "KID"
            elif "orange" in h and "DAD" in r.get("keys", []):
                primary = "DAD"
            elif "navy" in h:
                primary = "D12"
            elif "grey" in h:
                primary = "D20"
            elif "sky" in h:
                primary = "H19"
            elif "pale_blue" in h:
                primary = "H12"
        dest_dir = CURATED / primary
        dest_dir.mkdir(exist_ok=True)
        src = Path(r["src"])
        dest = dest_dir / f"{i:02d}_{r['id']}{src.suffix}"
        shutil.copy2(src, dest)
        # also write sidecar reason
        (dest_dir / f"{i:02d}_{r['id']}.txt").write_text(
            f"id: {r['id']}\nkeys: {', '.join(r.get('keys') or [])}\n"
            f"mismatch: {r.get('mismatch', 0):.3f}\nhard: {', '.join(r['hard'])}\n"
            f"reason: {r['reason']}\nsrc: {src}\n",
            encoding="utf-8",
        )
        by_key[primary].append(r["id"])
        rows_out.append(
            {
                "id": r["id"],
                "keys": r.get("keys") or [],
                "folder": primary,
                "mismatch": r.get("mismatch"),
                "hard": r["hard"],
                "reason": r["reason"],
                "copied_to": str(dest.relative_to(CURATED)),
                "src": str(src),
            }
        )

    (CURATED / "curated.json").write_text(json.dumps(rows_out, ensure_ascii=False, indent=2), encoding="utf-8")

    lines = [
        "# 앵커 대비 형상 차이 큰 컷씬 (선별)",
        "",
        "자동 색 히스토그램만으로는 배경 때문에 과다 검출되어, **머리 실루엣·시그니처 의상·HANDOVER 재생성 후보**만 남겼습니다.",
        "",
        f"- 앵커: `Tools/KlingGen/ref87/`",
        f"- 선별: **{len(rows_out)}**컷 → `Tools/KlingGen/out/shape_mismatch/curated/`",
        "",
        "## 캐릭터별",
        "",
    ]
    for k, ids in sorted(by_key.items(), key=lambda kv: -len(kv[1])):
        lines.append(f"- **{k}** ({len(ids)}): {', '.join(ids)}")
    lines += ["", "## 상세", "", "| # | id | keys | folder | reason |", "|--:|---|---|---|---|"]
    for i, r in enumerate(rows_out, 1):
        lines.append(
            f"| {i} | {r['id']} | {','.join(r['keys'])} | {r['folder']} | {r['reason']} |"
        )
    lines += [
        "",
        "## 콘택트 시트",
        "",
        "- `_sheets/_all.jpg` — 전체",
        "- `_sheets/<KEY>.jpg` — 캐릭터별",
        "",
        "## 참고",
        "",
        "- 근접샷·야간·실루엣 컷은 의상 색이 약해도 정상인 경우가 있음 → 시트에서 한 번 더 확인.",
        "- `severe/` 상위 폴더의 93컷은 자동 과다검출본(참고용).",
        "",
    ]
    (CURATED / "README.md").write_text("\n".join(lines), encoding="utf-8")

    make_sheet(rows_out, anchors, f"Curated shape mismatches ({len(rows_out)})", SHEET / "_all.jpg")
    for k, ids in by_key.items():
        rows = [r for r in rows_out if r["folder"] == k]
        make_sheet(rows, anchors, f"Curated · {k}", SHEET / f"{k}.jpg")

    print("curated", len(rows_out), dict((k, len(v)) for k, v in by_key.items()))


if __name__ == "__main__":
    main()
