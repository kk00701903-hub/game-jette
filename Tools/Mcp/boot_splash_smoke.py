#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Cold-start title boot smoke: install APK, clear data, launch (no boot=run),
screenshot at 1/2/3/5/8s. Expect bus splash — not UI_Loading_Mock (로딩중)."""
import os
import subprocess
import sys
import time
from pathlib import Path

PKG = "com.jette.coastrun"
ACTIVITY = "com.unity3d.player.UnityPlayerActivity"
SDK = Path(os.environ.get("LOCALAPPDATA", "")) / "Android" / "Sdk"
ADB = SDK / "platform-tools" / "adb.exe"
ROOT = Path(r"C:\dev\game")
APK = ROOT / "Builds" / "CoastRun.apk"
OUT = ROOT / "Tools" / "_shots" / "boot_splash"
OUT.mkdir(parents=True, exist_ok=True)


def adb(*args, timeout=120):
    r = subprocess.run(
        [str(ADB), *args],
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
        timeout=timeout,
    )
    return r.returncode, (r.stdout or "") + (r.stderr or "")


def device():
    code, out = adb("devices")
    for ln in out.splitlines():
        if ln.endswith("\tdevice"):
            return ln.split()[0]
    print("NO_DEVICE", flush=True)
    sys.exit(2)


def main():
    if not APK.exists():
        print("NO_APK", APK, flush=True)
        sys.exit(2)
    serial = device()
    print("device", serial, "apk", f"{APK.stat().st_size / 1024 / 1024:.1f} MB", flush=True)

    adb("-s", serial, "logcat", "-c")
    code, out = adb("-s", serial, "install", "-r", str(APK), timeout=300)
    print(out[-400:], flush=True)
    if code != 0 and "Success" not in out:
        print("INSTALL_FAIL", flush=True)
        sys.exit(1)

    adb("-s", serial, "shell", "am", "force-stop", PKG)
    adb("-s", serial, "shell", "pm", "clear", PKG)
    time.sleep(0.8)
    adb("-s", serial, "logcat", "-c")
    adb("-s", serial, "shell", "am", "start", "-n", f"{PKG}/{ACTIVITY}")
    print("launched cold start", flush=True)

    t0 = time.time()
    shots = []
    for sec in (1, 2, 3, 5, 8):
        while time.time() - t0 < sec:
            time.sleep(0.05)
        remote = f"/sdcard/boot_{sec}s.png"
        local = OUT / f"boot_{sec}s.png"
        adb("-s", serial, "shell", "screencap", "-p", remote)
        adb("-s", serial, "pull", remote, str(local))
        ok = local.exists() and local.stat().st_size > 1000
        print(f"shot {local.name} {'OK' if ok else 'MISSING'}", flush=True)
        shots.append(local)

    code, log = adb("-s", serial, "logcat", "-d")
    (OUT / "boot_log.txt").write_text(log, encoding="utf-8")
    has_bus = "boot splash start video=" in log
    print("marker boot_splash_log=", has_bus, flush=True)

    if not any(p.exists() and p.stat().st_size > 1000 for p in shots):
        print("BOOT_SMOKE_FAIL no screenshots", flush=True)
        sys.exit(1)
    print("BOOT_SMOKE_OK", flush=True)
    print("Inspect", OUT, flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main() or 0)
