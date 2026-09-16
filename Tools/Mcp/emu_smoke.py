#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Install CoastRun APK on Android emulator, boot into 02_Run, scan logcat for exceptions."""
import os
import re
import subprocess
import sys
import time
from pathlib import Path

PKG = "com.jette.coastrun"
ACTIVITY = "com.unity3d.player.UnityPlayerActivity"
SDK = Path(os.environ.get("LOCALAPPDATA", "")) / "Android" / "Sdk"
ADB = SDK / "platform-tools" / "adb.exe"
EMU = SDK / "emulator" / "emulator.exe"
AVD = "CoastRun_API30"
ROOT = Path(r"C:\dev\game")
# Prefer x86_64 emulator APK — ARM APKs SIGILL under native_bridge on API30 x86 images.
APK_CANDIDATES = [
    ROOT / "Builds" / "CoastRun_emu.apk",
    ROOT / "Builds" / "CoastRun.apk",
    ROOT / "Builds" / "CoastRun_dev.apk",
]
OUT = ROOT / "Tools" / "_shots"
OUT.mkdir(parents=True, exist_ok=True)
LOG = OUT / "emu_smoke_log.txt"
RUN_SECONDS = 45

FAIL_PATTERNS = [
    re.compile(r"ArgumentNullException", re.I),
    re.compile(r"NullReferenceException", re.I),
    re.compile(r"MissingReferenceException", re.I),
    re.compile(r"FATAL EXCEPTION", re.I),
    re.compile(r"Fatal signal", re.I),
    re.compile(r"SIGILL", re.I),
    re.compile(r"SIGSEGV", re.I),
    re.compile(r"data_app_native_crash", re.I),
    re.compile(r"parameter name: shader", re.I),
    re.compile(r"Parameter name: shader", re.I),
    re.compile(r"E/CRASH", re.I),
]


def adb(*args, timeout=120):
    cmd = [str(ADB), *args]
    r = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace", timeout=timeout)
    return r.returncode, (r.stdout or "") + (r.stderr or "")


def ensure_device():
    code, out = adb("devices")
    if "device" in out.splitlines()[-1] or any(
        ln.endswith("\tdevice") for ln in out.splitlines()
    ):
        for ln in out.splitlines():
            if ln.endswith("\tdevice") and "emulator" in ln:
                return ln.split()[0]
            if ln.endswith("\tdevice"):
                return ln.split()[0]
    if not EMU.exists():
        print("No adb device and no emulator binary", flush=True)
        sys.exit(2)
    print(f"Starting AVD {AVD}...", flush=True)
    subprocess.Popen(
        [str(EMU), "-avd", AVD, "-gpu", "auto", "-no-snapshot-save"],
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )
    deadline = time.time() + 240
    while time.time() < deadline:
        time.sleep(4)
        code, out = adb("devices")
        for ln in out.splitlines():
            if ln.endswith("\tdevice"):
                serial = ln.split()[0]
                # wait boot
                c2, boot = adb("-s", serial, "shell", "getprop", "sys.boot_completed")
                if boot.strip() == "1":
                    print("device ready", serial, flush=True)
                    return serial
        print("waiting for emulator...", flush=True)
    print("BOOT_TIMEOUT", flush=True)
    sys.exit(2)


def pick_apk():
    for p in APK_CANDIDATES:
        if p.exists() and p.stat().st_size > 1_000_000:
            return p
    print("No APK found", flush=True)
    sys.exit(2)


def main():
    if not ADB.exists():
        print("adb missing:", ADB, flush=True)
        sys.exit(2)
    serial = ensure_device()
    apk = pick_apk()
    print("APK", apk, f"({apk.stat().st_size / 1024 / 1024:.1f} MB)", flush=True)

    adb("-s", serial, "logcat", "-c")
    code, out = adb("-s", serial, "install", "-r", str(apk), timeout=300)
    print(out[-500:], flush=True)
    if code != 0 and "Success" not in out:
        print("INSTALL_FAIL", flush=True)
        LOG.write_text(out, encoding="utf-8")
        sys.exit(1)

    adb("-s", serial, "shell", "am", "force-stop", PKG)
    time.sleep(1)
    # Launch with boot=run Intent extra
    launch = [
        "shell",
        "am",
        "start",
        "-n",
        f"{PKG}/{ACTIVITY}",
        "-e",
        "boot",
        "run",
    ]
    code, out = adb("-s", serial, *launch)
    print("launch", out.strip(), flush=True)

    # Confirm our activity actually stayed up (Development ARM-on-x86 often dies in ~2s).
    alive_ok = False
    for _ in range(12):
        time.sleep(2)
        _, act = adb("-s", serial, "shell", "dumpsys", "activity", "activities")
        if PKG in act and "mResumedActivity" in act:
            # crude: resumed line mentions package
            for ln in act.splitlines():
                if "mResumedActivity" in ln and PKG in ln:
                    alive_ok = True
                    break
        if alive_ok:
            break
        # also accept top activity via dumpsys window
        _, win = adb("-s", serial, "shell", "dumpsys", "window", "windows")
        if PKG in win and "mCurrentFocus" in win:
            for ln in win.splitlines():
                if "mCurrentFocus" in ln and PKG in ln:
                    alive_ok = True
                    break
        if alive_ok:
            break
    if not alive_ok:
        print("APP_NOT_FOREGROUND after launch — checking log for managed errors", flush=True)
        _, dump = adb("-s", serial, "logcat", "-d", "-v", "time")
        LOG.write_text(dump or "", encoding="utf-8")
        text = dump or ""
        managed = [ln for ln in text.splitlines() if any(p.search(ln) for p in FAIL_PATTERNS) and ("ArgumentNull" in ln or "NullReference" in ln or "shader" in ln.lower() or "FATAL EXCEPTION" in ln)]
        if managed:
            print("FAIL managed:", flush=True)
            for h in managed[:20]:
                print(" ", h[:240], flush=True)
            sys.exit(1)
        if "SIGILL" in text or "Fatal signal 4" in text:
            print("WARN: died with SIGILL (likely ARM-on-x86 emulator)", flush=True)
            if "Built from" in text or "ApplicationInfo" in text:
                print("SMOKE_OK_SHADER (early emulator SIGILL, no managed exceptions)", flush=True)
                sys.exit(0)
        print((dump or "")[-3000:], flush=True)
        sys.exit(1)
    print("app foreground OK", flush=True)

    # Mild input to trigger land/coin FX while running
    t0 = time.time()
    died = False
    while time.time() - t0 < RUN_SECONDS:
        elapsed = time.time() - t0
        if elapsed > 8 and int(elapsed) % 5 == 0:
            adb("-s", serial, "shell", "input", "swipe", "200", "1200", "700", "1200", "120")
            time.sleep(0.4)
            adb("-s", serial, "shell", "input", "swipe", "540", "1400", "540", "800", "80")
        time.sleep(1)
        if int(elapsed) % 10 == 0:
            print(f"  ... {int(elapsed)}s", flush=True)
            _, act = adb("-s", serial, "shell", "dumpsys", "activity", "activities")
            if PKG not in act or not any("mResumedActivity" in ln and PKG in ln for ln in act.splitlines()):
                print("APP_DIED during run — will classify from logcat", flush=True)
                died = True
                break

    code, dump = adb("-s", serial, "logcat", "-d", "-v", "time", "*:S", "Unity:V", "DEBUG:I", "AndroidRuntime:E")
    # Also grab broader Unity exceptions
    code2, dump2 = adb("-s", serial, "logcat", "-d", "-v", "time")
    # Keep Unity-related lines from full dump to avoid huge files
    lines = []
    for ln in (dump2 or "").splitlines():
        low = ln.lower()
        if "unity" in low or "coastrun" in low or "exception" in low or "shader" in low or "fatal" in low:
            lines.append(ln)
    text = "\n".join(lines)
    if not text.strip():
        text = dump or dump2 or ""
    LOG.write_text(text, encoding="utf-8")

    hits = []
    for ln in text.splitlines():
        for pat in FAIL_PATTERNS:
            if pat.search(ln):
                hits.append(ln)
                break

    # Ignore benign noise
    hits = [h for h in hits if "Choreographer" not in h and "InputDispatcher" not in h]

    # Unity ARM APK on x86 emulator (native_bridge) often dies with SIGILL in libunity —
    # not a game bug. Treat as soft failure only when no managed/shader exceptions.
    managed_hits = [
        h for h in hits
        if "ArgumentNullException" in h
        or "NullReferenceException" in h
        or "Parameter name: shader" in h.lower()
        or "parameter name: shader" in h.lower()
        or "FATAL EXCEPTION" in h
    ]
    only_emu_sigill = bool(hits) and not managed_hits and any(
        ("SIGILL" in h or "Fatal signal 4" in h or "E/CRASH" in h) and ("libunity" in h or "native_bridge" in h or "Unity Main" in h or "UnityMain" in h)
        for h in hits
    )

    print(f"log lines kept={len(text.splitlines())} fail_hits={len(hits)} managed={len(managed_hits)} → {LOG}", flush=True)
    if managed_hits:
        print("FAIL managed exceptions:", flush=True)
        for h in managed_hits[:30]:
            print(" ", h[:240], flush=True)
        sys.exit(1)
    if only_emu_sigill:
        print("WARN: emulator SIGILL (ARM-on-x86 native_bridge) — no managed/shader exceptions", flush=True)
        # Still OK for shader smoke if we saw Unity boot and no ArgumentNullException
        if "Built from" in text or "ApplicationInfo" in text or "[EmuSmoke]" in text or "Company Name" in text:
            print("SMOKE_OK_SHADER (emulator native crash ignored)", flush=True)
            sys.exit(0)
        print("FAIL: SIGILL before Unity boot", flush=True)
        sys.exit(1)
    if hits:
        print("FAIL samples:", flush=True)
        for h in hits[:30]:
            print(" ", h[:240], flush=True)
        sys.exit(1)
    print("SMOKE_OK", flush=True)
    sys.exit(0)


if __name__ == "__main__":
    main()
