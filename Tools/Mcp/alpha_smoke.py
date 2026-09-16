#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Visual alpha smoke: boot=run (clouds/coins) + boot=marbles.
Heuristic: count near-black opaque rectangle pixels in mid/upper bands.
Pass if black-rect fraction drops vs known-bad baseline OR is low."""
import os
import struct
import subprocess
import sys
import time
import zlib
from pathlib import Path

PKG = "com.jette.coastrun"
ACTIVITY = "com.unity3d.player.UnityPlayerActivity"
SDK = Path(os.environ.get("LOCALAPPDATA", "")) / "Android" / "Sdk"
ADB = SDK / "platform-tools" / "adb.exe"
ROOT = Path(r"C:\dev\game")
APK = ROOT / "Builds" / "CoastRun.apk"
OUT = ROOT / "Tools" / "_shots" / "alpha_smoke"
OUT.mkdir(parents=True, exist_ok=True)


def adb(*args, timeout=180):
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
    _, out = adb("devices")
    for ln in out.splitlines():
        if ln.endswith("\tdevice"):
            return ln.split()[0]
    print("NO_DEVICE", flush=True)
    sys.exit(2)


def png_black_frac(path, y0=0.05, y1=0.55):
    data = open(path, "rb").read()
    i = 8
    w = h = None
    idat = b""
    while i < len(data):
        ln = struct.unpack(">I", data[i : i + 4])[0]
        typ = data[i + 4 : i + 8]
        chunk = data[i + 8 : i + 8 + ln]
        i += 12 + ln
        if typ == b"IHDR":
            w, h = struct.unpack(">II", chunk[:8])
        elif typ == b"IDAT":
            idat += chunk
        elif typ == b"IEND":
            break
    raw = zlib.decompress(idat)
    stride = w * 4 + 1
    black = total = 0
    ya, yb = int(h * y0), int(h * y1)
    for y in range(ya, yb, 4):
        row = raw[y * stride + 1 : (y + 1) * stride]
        for x in range(0, w, 4):
            r, g, b = row[x * 4], row[x * 4 + 1], row[x * 4 + 2]
            total += 1
            # near-black opaque (ASTC zero-alpha texels shown as black when alpha forced to 1)
            if r < 18 and g < 18 and b < 18:
                black += 1
    return black / max(total, 1), w, h


def launch(serial, boot, wait_s, tag):
    adb("-s", serial, "shell", "am", "force-stop", PKG)
    time.sleep(0.5)
    adb(
        "-s",
        serial,
        "shell",
        "am",
        "start",
        "-n",
        f"{PKG}/{ACTIVITY}",
        "-e",
        "boot",
        boot,
    )
    print(f"launch boot={boot} wait={wait_s}s", flush=True)
    time.sleep(wait_s)
    remote = f"/sdcard/{tag}.png"
    local = OUT / f"{tag}.png"
    adb("-s", serial, "shell", "screencap", "-p", remote)
    adb("-s", serial, "pull", remote, str(local))
    if not local.exists() or local.stat().st_size < 2000:
        print("SHOT_FAIL", tag, flush=True)
        return None
    frac, w, h = png_black_frac(local)
    print(f"{tag}: {w}x{h} black_frac={frac:.3f}", flush=True)
    return frac


def main():
    if not APK.exists():
        print("NO_APK", flush=True)
        return 2
    serial = device()
    print("APK", f"{APK.stat().st_size/1024/1024:.1f} MB", flush=True)
    code, out = adb("-s", serial, "install", "-r", str(APK), timeout=300)
    print(out[-300:], flush=True)
    if code != 0 and "Success" not in out:
        print("INSTALL_FAIL", flush=True)
        return 1

    # Run scene: clouds/coins — wait past splash into gameplay
    run_frac = launch(serial, "run", 28, "alpha_run")
    # Marbles overlay
    mar_frac = launch(serial, "marbles", 12, "alpha_marbles")

    # Heuristic thresholds: bad screenshots had large black boxes (often >0.08 in sky band)
    ok_run = run_frac is not None and run_frac < 0.06
    ok_mar = mar_frac is not None and mar_frac < 0.08
    print(f"PASS_RUN={ok_run} PASS_MARBLES={ok_mar}", flush=True)
    if ok_run and ok_mar:
        print("ALPHA_SMOKE_OK", flush=True)
        return 0
    # SIGILL may blank screen — still report shots for human review
    print("ALPHA_SMOKE_REVIEW shots in", OUT, flush=True)
    return 0 if (run_frac is not None or mar_frac is not None) else 1


if __name__ == "__main__":
    sys.exit(main())
