# -*- coding: utf-8 -*-
"""104차(사용자: 「육성모드 화면의 배경음을 프린세스 메이커 같은 음악으로」)
tracks.json 에 육성 허브 곡 BGM_M15 발주서를 넣는다. 한 번만 돌리면 되고, 다시 돌려도 중복되지 않는다."""
import collections
import io
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
PATH = os.path.join(HERE, "tracks.json")

ENTRY = collections.OrderedDict([
    ("name", "BGM_M15"),
    ("group", "menu"),
    ("priority", "P1"),
    ("prompt",
     "cozy raising-sim daily life theme in the spirit of Princess Maker: "
     "gentle 3/4 waltz, music box celesta lead, harpsichord and pizzicato strings, "
     "soft woodwind counter melody, warm chamber orchestra, storybook and homely, "
     "calm afternoon in a small seaside house, no drum kit, only light tambourine, "
     "unhurried, seamlessly loopable, recurring three-note motif D E F# on celesta"),
    ("bpm", 92),
    ("key", "F Major"),
    ("duration", 110),
    ("seed", 1513),
    ("loop", True),
])


def main():
    with io.open(PATH, encoding="utf-8") as f:
        doc = json.load(f, object_pairs_hook=collections.OrderedDict)
    before = len(doc["tracks"])
    doc["tracks"] = [t for t in doc["tracks"] if t.get("name") != ENTRY["name"]]
    doc["tracks"].append(ENTRY)
    with io.open(PATH, "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(doc, ensure_ascii=False, indent=1) + "\n")
    print("tracks %d -> %d (%s)" % (before, len(doc["tracks"]), ENTRY["name"]))


if __name__ == "__main__":
    main()
