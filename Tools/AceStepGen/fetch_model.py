# -*- coding: utf-8 -*-
"""ACE-Step 1.5 모델(ACE-Step/Ace-Step1.5, 약 5GB)을 checkpoints/ 로 먼저 내려받는다.

서버가 첫 요청 안에서 모델을 받으면 렌더 타임아웃(ACESTEP_GENERATION_TIMEOUT)에 걸려
곡이 버려진다 — 받을 건 미리 받아 두고, 생성은 모델이 준비된 뒤에 시킨다.
끊겨도 다시 돌리면 이어받는다(huggingface_hub 가 .incomplete 에서 재개).

사용: C:\\dev\\ACE-Step-1.5\\venv_cpu\\Scripts\\python.exe Tools/AceStepGen/fetch_model.py
"""
import os
import sys

REPO = "ACE-Step/Ace-Step1.5"
DEST = r"C:\dev\ACE-Step-1.5\checkpoints"


def main():
    os.environ.setdefault("HF_HUB_ENABLE_HF_TRANSFER", "0")
    from huggingface_hub import snapshot_download

    print("downloading %s -> %s" % (REPO, DEST), flush=True)
    path = snapshot_download(repo_id=REPO, local_dir=DEST, resume_download=True,
                             max_workers=4)
    total = 0
    for root, _dirs, files in os.walk(path):
        for f in files:
            total += os.path.getsize(os.path.join(root, f))
    print("done: %s (%.2f GB)" % (path, total / (1024.0 ** 3)), flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
