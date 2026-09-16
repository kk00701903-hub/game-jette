#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Visual emulator smoke on ARM64 AVD (Unity 6 dropped Android x86_64).
Captures run + marbles screenshots for agent visual review."""
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
EMU = SDK / "emulator" / "emulator.exe"
AVD = "CoastRun_ARM64"  # ARM image + ARM APK (x86 AVD SIGILLs under native_bridge)
ROOT = Path(r"C:\dev\game")
OUT = ROOT / "Tools" / "_shots" / "visual_emu"
OUT.mkdir(parents=True, exist_ok=True)
APKS = [
    ROOT / "Builds" / "CoastRun.apk",
    ROOT / "Builds" / "CoastRun_emu.apk",
]


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


def ensure_emu():
    code, out = adb("devices")
    for ln in out.splitlines():
        if "emulator-" in ln and ln.endswith("\tdevice"):
            return ln.split()[0]
    if EMU.exists():
        print("starting", AVD, flush=True)
        subprocess.Popen(
            [str(EMU), "-avd", AVD, "-gpu", "host", "-no-snapshot-save"],
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
    deadline = time.time() + 240
    while time.time() < deadline:
        time.sleep(4)
        code, out = adb("devices")
        for ln in out.splitlines():
            if "emulator-" in ln and ln.endswith("\tdevice"):
                serial = ln.split()[0]
                _, boot = adb("-s", serial, "shell", "getprop", "sys.boot_completed")
                if boot.strip() == "1":
                    print("device", serial, flush=True)
                    return serial
        print("waiting emu...", flush=True)
    print("EMU_TIMEOUT", flush=True)
    sys.exit(2)


def pick_apk():
    for p in APKS:
        if p.exists() and p.stat().st_size > 1_000_000:
            return p
    print("NO_APK", flush=True)
    sys.exit(2)


def png_stats(path):
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
    black = cyan = bright = total = 0
    rs = gs = bs = 0
    for y in range(0, h, 6):
        row = raw[y * stride + 1 : (y + 1) * stride]
        for x in range(0, w, 6):
            r, g, b = row[x * 4], row[x * 4 + 1], row[x * 4 + 2]
            total += 1
            rs += r
            gs += g
            bs += b
            if r < 20 and g < 20 and b < 20:
                black += 1
            if b > 180 and g > 150 and r < 160:
                cyan += 1
            if r > 200 and g > 200 and b > 200:
                bright += 1
    return {
        "w": w,
        "h": h,
        "black": black / max(total, 1),
        "cyan": cyan / max(total, 1),
        "bright": bright / max(total, 1),
        "avg": (rs // total, gs // total, bs // total),
    }


def shot(serial, name):
    remote = f"/sdcard/{name}.png"
    local = OUT / f"{name}.png"
    adb("-s", serial, "shell", "screencap", "-p", remote)
    adb("-s", serial, "pull", remote, str(local))
    if not local.exists() or local.stat().st_size < 3000:
        print("SHOT_FAIL", name, flush=True)
        return None
    st = png_stats(local)
    print(
        f"{name}: {st['w']}x{st['h']} avg={st['avg']} black={st['black']:.3f} cyan={st['cyan']:.3f} bright={st['bright']:.3f}",
        flush=True,
    )
    return st


def launch(serial, boot):
    adb("-s", serial, "shell", "am", "force-stop", PKG)
    time.sleep(0.6)
    adb("-s", serial, "logcat", "-c")
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
    print("launched boot=" + boot, flush=True)


def log_has(serial, *needles):
    _, log = adb("-s", serial, "logcat", "-d")
    (OUT / "last_log.txt").write_text(log, encoding="utf-8")
    low = log.lower()
    return {n: (n.lower() in low) for n in needles}


def main():
    serial = ensure_emu()
    apk = pick_apk()
    print("APK", apk.name, f"{apk.stat().st_size/1024/1024:.1f} MB", flush=True)
    adb("-s", serial, "uninstall", PKG)
    code, out = adb("-s", serial, "install", "-r", str(apk), timeout=400)
    print(out[-400:], flush=True)
    if code != 0 and "Success" not in out:
        print("INSTALL_FAIL", flush=True)
        return 1

    # --- Run (clouds/coins) ---
    launch(serial, "run")
    time.sleep(8)
    shot(serial, "run_08s")
    time.sleep(10)
    shot(serial, "run_18s")
    time.sleep(12)
    shot(serial, "run_30s")
    flags = log_has(serial, "SIGILL", "EmuSmoke", "Fatal signal")
    print("run_flags", flags, flush=True)

    # --- Marbles ---
    launch(serial, "marbles")
    time.sleep(6)
    shot(serial, "marbles_06s")
    time.sleep(6)
    shot(serial, "marbles_12s")
    flags2 = log_has(serial, "SIGILL", "EmuSmoke", "Mission")
    print("marbles_flags", flags2, flush=True)

    # Heuristic: if nearly all black + SIGILL → arch fail
    r30 = OUT / "run_30s.png"
    if r30.exists():
        st = png_stats(r30)
        if st["black"] > 0.9 and flags.get("SIGILL"):
            print("VISUAL_EMU_FAIL: native crash (SIGILL) — wrong ABI for this AVD", flush=True)
            return 2
        if st["black"] > 0.9:
            print("VISUAL_EMU_FAIL: black screen — app did not render", flush=True)
            return 2
        if st["black"] < 0.25 and st["avg"][1] > 40:
            print("VISUAL_EMU_OK: gameplay pixels present — review PNGs in", OUT, flush=True)
            return 0
    print("VISUAL_EMU_REVIEW shots in", OUT, flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
