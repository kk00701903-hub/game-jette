"""Final human-reviewed shape-mismatch set (vs auto false positives)."""
from __future__ import annotations

import json
import shutil
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(r"c:/dev/game")
REF = ROOT / "Tools/KlingGen/ref87"
OUT = ROOT / "Tools/KlingGen/out/shape_mismatch"
FINAL = OUT / "final"
SHEET = FINAL / "_sheets"
CURATED_JSON = OUT / "curated" / "curated.json"

# Severity: high = silhouette/identity clearly wrong vs ref87; mid = notable but context may excuse
FINAL_LIST = [
    {
        "id": "Cut_T_OP_03",
        "severity": "high",
        "focus": "DAD",
        "issue": "우비 실루엣: 앵커는 무릎까지 긴 주황 레인코트인데 컷은 짧은 재킷형으로 읽힘",
    },
    {
        "id": "Cut_T_OP_02",
        "severity": "high",
        "focus": "DAD",
        "issue": "아빠 얼굴·체형이 앵커(40대 초반·마른 어부)보다 사실적·나이 들어 보임(옛 앵커 계열)",
    },
    {
        "id": "Cut_T_N2_07",
        "severity": "high",
        "focus": "DAD",
        "issue": "배 위 아빠: 얼굴·체형 디테일이 앵커와 크게 어긋남(HANDOVER 재생성 후보)",
    },
    {
        "id": "Cut_T_OP_07",
        "severity": "high",
        "focus": "MOM",
        "issue": "엄마 실루엣: 앵커는 해녀복+태왁인데 컷은 한복·실내 복장으로 완전 다른 형상",
    },
    {
        "id": "Cut_T_OP_04",
        "severity": "high",
        "focus": "MOM",
        "issue": "오프닝 가족샷 — 엄마/아빠가 새 ref87 바이블과 어긋날 가능성 큼(HANDOVER 재생성 후보)",
    },
    {
        "id": "Cut_T_N2_05",
        "severity": "mid",
        "focus": "MOM",
        "issue": "엄마 옆얼굴이 앵커보다 날카롭고 성숙하게 읽힘(바이블 교체 후 재검수)",
    },
    {
        "id": "Cut_T_N3_06",
        "severity": "mid",
        "focus": "D12",
        "issue": "도윤12: 긴소매·조끼 흰줄 등 의상 디테일/비율이 앵커(반팔+네이비조끼)와 다름",
    },
    {
        "id": "Cut_T_OP_12",
        "severity": "mid",
        "focus": "D12",
        "issue": "야간 카트 장면 — D12 얼굴이 앵커의 둥근 아이 비율보다 길어·성숙하게 보임",
    },
    {
        "id": "Cut_T_N8_10",
        "severity": "mid",
        "focus": "KID",
        "issue": "꼬마 비율이 앵커보다 치비·둥글게 과장됨 / H19 머리핀 형태도 다름",
    },
    {
        "id": "Cut_T_N8_14",
        "severity": "mid",
        "focus": "H19",
        "issue": "꽃밭 장면 — H19 긴머리 실루엣이 약하거나 머리 길이가 짧게 읽힐 여지",
    },
    {
        "id": "Cut_T_N4_11",
        "severity": "mid",
        "focus": "H19",
        "issue": "보드 액션 — H19 팔다리 비율이 앵커보다 가늘고 길게 과장",
    },
    {
        "id": "Cut_T_E10_04",
        "severity": "mid",
        "focus": "H19",
        "issue": "창가 클로즈업 — 얼굴형이 앵커보다 둥글고 통통함(2차 재생성 이력 있음)",
    },
    {
        "id": "Cut_T_N5_05",
        "severity": "high",
        "focus": "H12",
        "issue": "트럭/갓길 다인물컷 — H12·D12·MOM 동시 등장, 비율·의상 큐가 앵커와 크게 어긋나 2차 재생성까지 간 컷",
    },
    {
        "id": "Cut_T_E6_03",
        "severity": "high",
        "focus": "D20",
        "issue": "도윤20 단독 — 회색 코트/얼굴 큐 약함, 2차 재생성 이력 있는 문제 컷",
    },
    {
        "id": "Cut_T_N6_05",
        "severity": "high",
        "focus": "D12",
        "issue": "도윤12 관련 — 2차 재생성 이력, 아이 비율·조끼 실루엣 재검수 필요",
    },
]


def thumb(path: Path, size=(168, 298)) -> Image.Image:
    im = Image.open(path).convert("RGB")
    im.thumbnail(size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGB", size, (245, 243, 238))
    canvas.paste(im, ((size[0] - im.width) // 2, (size[1] - im.height) // 2))
    return canvas


def main():
    curated = {r["id"]: r for r in json.loads(CURATED_JSON.read_text(encoding="utf-8"))}
    # also allow lookup from severe for ids not in curated
    severe = {r["id"]: r for r in json.loads((OUT / "severe.json").read_text(encoding="utf-8"))}
    report = {r["id"]: r for r in json.loads((OUT / "report.json").read_text(encoding="utf-8"))}

    if FINAL.exists():
        shutil.rmtree(FINAL)
    FINAL.mkdir(parents=True)
    SHEET.mkdir(parents=True)
    anchors = {p.stem: p for p in REF.glob("*.png")}

    rows = []
    missing = []
    for item in FINAL_LIST:
        lid = item["id"]
        src_info = curated.get(lid) or severe.get(lid) or report.get(lid)
        if not src_info:
            missing.append(lid)
            continue
        src = Path(src_info["src"] if "src" in src_info else src_info["path"])
        if not src.exists():
            missing.append(lid)
            continue
        focus = item["focus"]
        dest_dir = FINAL / item["severity"] / focus
        dest_dir.mkdir(parents=True, exist_ok=True)
        dest = dest_dir / f"{lid}{src.suffix}"
        shutil.copy2(src, dest)
        (dest_dir / f"{lid}.txt").write_text(
            f"{lid}\nfocus: {focus}\nseverity: {item['severity']}\n"
            f"issue: {item['issue']}\nsrc: {src}\n",
            encoding="utf-8",
        )
        keys = src_info.get("keys") or [focus]
        rows.append({**item, "keys": keys, "src": str(src), "copied_to": str(dest.relative_to(FINAL))})

    (FINAL / "final.json").write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding="utf-8")

    high = [r for r in rows if r["severity"] == "high"]
    mid = [r for r in rows if r["severity"] == "mid"]
    lines = [
        "# 앵커 대비 형상 차이 — 최종 선별",
        "",
        "plan84 Cut_T 112컷을 앵커(`ref87`)와 비교한 뒤, **자동 점수 과다검출을 걷어내고 시트 육안 검수**로 확정했습니다.",
        "",
        f"- **high (재생성 우선)**: {len(high)}컷",
        f"- **mid (재검수/선택적)**: {len(mid)}컷",
        f"- 폴더: `Tools/KlingGen/out/shape_mismatch/final/`",
        "",
        "## HIGH — 재생성 우선",
        "",
        "| id | focus | issue |",
        "|---|---|---|",
    ]
    for r in high:
        lines.append(f"| {r['id']} | {r['focus']} | {r['issue']} |")
    lines += ["", "## MID — 재검수", "", "| id | focus | issue |", "|---|---|---|"]
    for r in mid:
        lines.append(f"| {r['id']} | {r['focus']} | {r['issue']} |")
    if missing:
        lines += ["", "## 파일 못 찾음", "", *[f"- {m}" for m in missing]]
    lines += [
        "",
        "## 시트",
        "",
        "- `_sheets/high.jpg`",
        "- `_sheets/mid.jpg`",
        "- `_sheets/all.jpg`",
        "",
        "## 제외한 것",
        "",
        "- 야간/실루엣/수면으로 의상 색만 약한 컷",
        "- 스토리상 한복·실내복이 **의도**일 수 있는지는 OP_07에 메모 — 바이블상 엄마=해녀복이면 불일치",
        "- 자동 `severe` 93컷·`curated` 31컷은 참고용으로 상위 폴더에 유지",
        "",
    ]
    (FINAL / "README.md").write_text("\n".join(lines), encoding="utf-8")

    def make_sheet(subset, title, dest):
        cell_w, cell_h = 160, 284
        pad, label_h = 8, 56
        max_keys = max((len(r["keys"]) for r in subset), default=1)
        row_w = (min(max_keys, 3) + 1) * (cell_w + pad) + pad
        row_h = cell_h + label_h + pad
        W = max(row_w, 640)
        H = 48 + len(subset) * row_h + pad
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
        for r in subset:
            x = pad
            focus = r["focus"]
            show_keys = [focus] + [k for k in r["keys"] if k != focus][:2]
            for k in show_keys:
                if k in anchors:
                    sheet.paste(thumb(anchors[k]), (x, y))
                    draw.text((x, y + cell_h + 2), k, fill=(50, 50, 50), font=font_sm)
                x += cell_w + pad
            sheet.paste(thumb(Path(r["src"])), (x, y))
            draw.text((x, y + cell_h + 2), f"{r['severity'].upper()} {r['id']}", fill=(140, 40, 40), font=font_sm)
            draw.text((pad, y + cell_h + 22), r["issue"][:95], fill=(60, 60, 60), font=font_sm)
            y += row_h
        sheet.save(dest, quality=92)
        print("sheet", dest)

    make_sheet(high, f"HIGH shape mismatch ({len(high)})", SHEET / "high.jpg")
    make_sheet(mid, f"MID shape mismatch ({len(mid)})", SHEET / "mid.jpg")
    make_sheet(rows, f"ALL final ({len(rows)})", SHEET / "all.jpg")
    print("final", len(rows), "missing", missing)


if __name__ == "__main__":
    main()
