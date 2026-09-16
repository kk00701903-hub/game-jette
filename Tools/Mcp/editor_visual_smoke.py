#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Editor visual smoke via CoastRemote (127.0.0.1:47001).
Requires Unity Editor open on this project with CoastRemote running.
Captures: title/run if possible + marbles mission screenshots."""
import json
import socket
import sys
import time
from pathlib import Path

HOST, PORT = "127.0.0.1", 47001
OUT = Path(r"C:\dev\game\Tools\_shots\editor_visual")
OUT.mkdir(parents=True, exist_ok=True)


def send(cmd, timeout=60.0):
    s = socket.create_connection((HOST, PORT), timeout=5.0)
    try:
        s.settimeout(timeout)
        s.sendall((cmd.strip() + "\n").encode("utf-8"))
        buf = b""
        while not buf.endswith(b"\n") and len(buf) < 2_000_000:
            chunk = s.recv(65536)
            if not chunk:
                break
            buf += chunk
        return buf.decode("utf-8", "replace").strip()
    finally:
        s.close()


def main():
    try:
        pong = send("ping", 5)
    except OSError as e:
        print("CoastRemote DOWN — open Unity on C:\\dev\\game first:", e, flush=True)
        return 2
    print("ping", pong, flush=True)
    print("status", send("status"), flush=True)

    # Enter play if not playing
    st = send("status")
    if "playing" not in st.lower() and '"play":true' not in st.lower() and "isPlaying" not in st:
        # CoastRemote status format varies — always try play
        print("play", send("play", 120), flush=True)
        time.sleep(4)

    # Marbles overlay
    print("menu marbles", send("menu Coast Run/Dev/Mission - Marbles", 30), flush=True)
    time.sleep(2.5)
    shot1 = OUT / "editor_marbles.png"
    print("shot", send(f"shot {shot1.as_posix()}", 30), flush=True)
    time.sleep(0.5)

    # Also x3 screenshot to default path then copy name
    print("menu shotx3", send("menu Coast Run/Screenshot x3 (play mode)", 15), flush=True)
    time.sleep(1)

    print("EDITOR_VISUAL_OK shots under", OUT, flush=True)
    print("Inspect editor_marbles.png — SoftDisc must be round (no black/brown squares).", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
