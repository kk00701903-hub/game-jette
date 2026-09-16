#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Rebuild CoastRun APK via CoastRemote. Usage: rebuild_apk.py [release|dev]"""
import socket
import sys
import time
from pathlib import Path

PORT = 47001
ROOT = Path(r"C:\dev\game")
LB = ROOT / "Builds" / "last_build.txt"

MENUS = {
    "release": "Coast Run/Build/Android APK (IL2CPP, ARM64+ARMv7)",
    "dev": "Coast Run/Build/Android APK — Development (IL2CPP, ARM64+ARMv7)",
    "emu": "Coast Run/Build/Android APK — Emulator (IL2CPP, x86_64)",
}
APKS = {
    "release": ROOT / "Builds" / "CoastRun.apk",
    "dev": ROOT / "Builds" / "CoastRun_dev.apk",
    "emu": ROOT / "Builds" / "CoastRun_emu.apk",
}


def send(cmd, timeout=30.0):
    s = socket.create_connection(("127.0.0.1", PORT), timeout=timeout)
    try:
        s.sendall((cmd + "\n").encode("utf-8"))
        buf = b""
        s.settimeout(timeout)
        try:
            while not buf.endswith(b"\n"):
                chunk = s.recv(65536)
                if not chunk:
                    break
                buf += chunk
        except socket.timeout:
            pass
        return buf.decode("utf-8", "replace").strip()
    finally:
        s.close()


def main():
    mode = (sys.argv[1] if len(sys.argv) > 1 else "release").lower()
    if mode not in MENUS:
        print("usage: rebuild_apk.py [release|dev|emu]", flush=True)
        return 2
    apk = APKS[mode]
    print("stop", send("stop"), flush=True)
    time.sleep(1.5)
    prev = LB.stat().st_mtime if LB.exists() else 0
    print("start build", mode, flush=True)
    print(send("menu " + MENUS[mode]), flush=True)

    deadline = time.time() + 1800
    while time.time() < deadline:
        time.sleep(25)
        if not LB.exists():
            print("no last_build yet", flush=True)
            continue
        mt = LB.stat().st_mtime
        if mt <= prev:
            print("waiting build...", time.strftime("%H:%M:%S"), flush=True)
            continue
        txt = LB.read_text(encoding="utf-8", errors="replace")
        print("last_build updated", flush=True)
        print(txt[:500], flush=True)
        if "Succeeded" in txt:
            if apk.exists():
                print(f"APK {apk.stat().st_size/1024/1024:.1f} MB", flush=True)
            return 0
        if "Failed" in txt:
            return 1
    print("TIMEOUT", flush=True)
    return 2


if __name__ == "__main__":
    sys.exit(main())
