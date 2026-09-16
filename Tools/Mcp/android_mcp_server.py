#!/usr/bin/env python3
"""Coast Run — Android 에뮬레이터/실기기 MCP 서버 (Claude 데스크톱 로컬 MCP, 의존성 없음).

Cursor 에서는 에이전트가 터미널로 adb/emulator 를 직접 돌렸다(권한: 터미널 auto-run).
Claude 데스크톱에는 그런 터미널 권한이 없으므로 같은 일을 이 MCP 서버가 대신한다.

Claude 데스크톱 설정(앱 설정 → 개발자 → 구성 편집)에 이렇게 등록:
  "mcpServers": { "android": { "command": "C:\\\\Users\\\\ares2\\\\AppData\\\\Local\\\\Programs\\\\Python\\\\Python312\\\\python.exe",
                               "args": ["C:\\\\dev\\\\game\\\\Tools\\\\Mcp\\\\android_mcp_server.py"] } }

경로·기본값은 C:\\dev\\game\\.env 의 ANDROID_* 를 읽는다(없으면 %LOCALAPPDATA%\\Android\\Sdk 기본값).
  ANDROID_SDK_ROOT   SDK 루트
  ANDROID_ADB        adb.exe 전체 경로
  ANDROID_EMULATOR   emulator.exe 전체 경로
  ANDROID_AVD        기본 AVD 이름 (CoastRun_ARM64)
  ANDROID_AVD_ALT    보조 AVD 이름 (CoastRun_API30, x86_64)
  ANDROID_PKG        패키지명 (com.jette.coastrun)
  ANDROID_ACTIVITY   런처 액티비티 (com.unity3d.player.UnityPlayerActivity)

도구:
  android_ping        : adb/emulator 존재·버전, 연결 기기, AVD 목록 한 번에
  android_devices     : adb devices -l
  android_avds        : emulator -list-avds
  android_start_avd   : AVD 부팅(이미 떠 있으면 그대로) → sys.boot_completed=1 까지 대기
  android_kill_avd    : 에뮬레이터 종료(adb emu kill)
  android_install     : APK 설치(기본 Builds/CoastRun.apk, -r)
  android_launch      : 앱 실행(am start, -e boot <mode> 로 CoastRun 부트 모드 전달 가능)
  android_stop_app    : 앱 강제 종료(am force-stop)
  android_screencap   : 화면 캡처 → Tools/_shots/<name>.png
  android_logcat      : logcat 덤프(Unity/AndroidRuntime/DEBUG 필터 기본, 마지막 n줄)
  android_input       : tap x y | swipe x1 y1 x2 y2 [ms] | key <KEYCODE> | text <문자열>
  android_shell       : 임의 adb shell 명령
  android_props       : 기기 정보(모델·API·ABI·해상도·boot_completed)

주의: Unity 6 은 Android x86_64 빌드를 지원하지 않는다. ARM APK 는 x86_64 AVD 에서 SIGILL,
ARM64 AVD 는 x86_64 호스트의 QEMU2 에서 부팅 불가 → 이 PC 에선 실기기(adb) 가 사실상 유일한 경로.
(자세한 내용: Tools/Mcp/EMU_VISUAL.md)
"""
import json, os, subprocess, sys, time

PROJECT = r"C:\dev\game"
SHOTS = os.path.join(PROJECT, "Tools", "_shots")
NO_WINDOW = getattr(subprocess, "CREATE_NO_WINDOW", 0)


# ── .env ──────────────────────────────────────────────────────────
def load_env():
    env = {}
    p = os.path.join(PROJECT, ".env")
    if os.path.isfile(p):
        with open(p, encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if not line or line.startswith("#") or "=" not in line:
                    continue
                k, v = line.split("=", 1)
                env[k.strip()] = v.strip().strip('"').strip("'")
    return env


ENV = load_env()
SDK = ENV.get("ANDROID_SDK_ROOT") or os.path.join(os.environ.get("LOCALAPPDATA", ""), "Android", "Sdk")
ADB = ENV.get("ANDROID_ADB") or os.path.join(SDK, "platform-tools", "adb.exe")
EMU = ENV.get("ANDROID_EMULATOR") or os.path.join(SDK, "emulator", "emulator.exe")
AVD = ENV.get("ANDROID_AVD") or "CoastRun_ARM64"
AVD_ALT = ENV.get("ANDROID_AVD_ALT") or "CoastRun_API30"
PKG = ENV.get("ANDROID_PKG") or "com.jette.coastrun"
ACTIVITY = ENV.get("ANDROID_ACTIVITY") or "com.unity3d.player.UnityPlayerActivity"
DEFAULT_APK = os.path.join(PROJECT, "Builds", "CoastRun.apk")


# ── 실행 헬퍼 ──────────────────────────────────────────────────────
def run(cmd, timeout=60, binary=False):
    try:
        r = subprocess.run(cmd, capture_output=True, timeout=timeout, creationflags=NO_WINDOW)
    except FileNotFoundError:
        return 127, f"실행 파일 없음: {cmd[0]}"
    except subprocess.TimeoutExpired:
        return 124, f"시간 초과({timeout}s): {' '.join(map(str, cmd))}"
    if binary:
        return r.returncode, r.stdout
    out = (r.stdout or b"").decode("utf-8", "replace") + (r.stderr or b"").decode("utf-8", "replace")
    return r.returncode, out.strip()


def adb(*args, serial=None, timeout=60, binary=False):
    cmd = [ADB]
    if serial:
        cmd += ["-s", serial]
    cmd += list(args)
    return run(cmd, timeout, binary)


def devices():
    code, out = adb("devices", "-l", timeout=20)
    devs = []
    for ln in out.splitlines()[1:]:
        parts = ln.split()
        if len(parts) >= 2 and parts[1] == "device":
            devs.append({"serial": parts[0], "info": " ".join(parts[2:])})
    return devs, out


def pick_serial(args):
    s = args.get("serial")
    if s:
        return s
    devs, _ = devices()
    if not devs:
        return None
    return devs[0]["serial"]


def avds():
    if not os.path.exists(EMU):
        return []
    code, out = run([EMU, "-list-avds"], 30)
    return [l.strip() for l in out.splitlines() if l.strip() and not l.startswith("INFO")]


def j(obj):
    return json.dumps(obj, ensure_ascii=False, indent=1)


# ── 도구 ──────────────────────────────────────────────────────────
def tool_ping(args):
    info = {"sdk": SDK, "adb": ADB, "adb_exists": os.path.exists(ADB),
            "emulator": EMU, "emulator_exists": os.path.exists(EMU),
            "default_avd": AVD, "alt_avd": AVD_ALT, "pkg": PKG, "apk": DEFAULT_APK, "apk_exists": os.path.exists(DEFAULT_APK)}
    if info["adb_exists"]:
        _, v = adb("version", timeout=15)
        info["adb_version"] = v.splitlines()[0] if v else v
        devs, raw = devices()
        info["devices"] = devs
    if info["emulator_exists"]:
        info["avds"] = avds()
    return j(info)


def tool_devices(args):
    devs, raw = devices()
    return j({"devices": devs, "raw": raw})


def tool_avds(args):
    return j({"avds": avds(), "emulator": EMU})


def tool_start_avd(args):
    name = args.get("avd") or AVD
    devs, _ = devices()
    for d in devs:
        if d["serial"].startswith("emulator-"):
            return j({"ok": True, "already": True, "serial": d["serial"]})
    if not os.path.exists(EMU):
        return j({"ok": False, "error": "emulator.exe 없음: " + EMU})
    if name not in avds():
        return j({"ok": False, "error": f"AVD '{name}' 없음", "avds": avds()})
    gpu = args.get("gpu") or "auto"
    cmd = [EMU, "-avd", name, "-gpu", gpu, "-no-snapshot-save", "-no-boot-anim"]
    if args.get("extra"):
        cmd += str(args["extra"]).split()
    flags = getattr(subprocess, "DETACHED_PROCESS", 0) | getattr(subprocess, "CREATE_NEW_PROCESS_GROUP", 0)
    subprocess.Popen(cmd, creationflags=flags, close_fds=True, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    wait = float(args.get("wait_seconds", 240))
    t0 = time.time()
    serial = None
    while time.time() - t0 < wait:
        time.sleep(5)
        devs, _ = devices()
        for d in devs:
            if d["serial"].startswith("emulator-"):
                serial = d["serial"]
        if serial:
            _, boot = adb("shell", "getprop", "sys.boot_completed", serial=serial, timeout=15)
            if boot.strip() == "1":
                return j({"ok": True, "serial": serial, "avd": name, "boot_seconds": int(time.time() - t0)})
    return j({"ok": False, "serial": serial, "avd": name,
              "note": f"{int(wait)}초 안에 boot_completed=1 이 안 됨 — ARM64 AVD 는 x86_64 호스트에서 부팅이 안 될 수 있음(EMU_VISUAL.md)."})


def tool_kill_avd(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    code, out = adb("emu", "kill", serial=serial, timeout=20)
    return j({"ok": code == 0, "serial": serial, "out": out})


def tool_install(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음 — android_start_avd 또는 실기기 USB 연결"})
    apk = args.get("apk") or DEFAULT_APK
    if not os.path.exists(apk):
        return j({"ok": False, "error": "APK 없음: " + apk})
    code, out = adb("install", "-r", apk, serial=serial, timeout=float(args.get("timeout", 600)))
    return j({"ok": code == 0 and "Success" in out, "serial": serial, "apk": apk, "bytes": os.path.getsize(apk), "out": out[-800:]})


def tool_launch(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    pkg = args.get("pkg") or PKG
    act = args.get("activity") or ACTIVITY
    cmd = ["shell", "am", "start", "-n", f"{pkg}/{act}"]
    if args.get("boot"):
        cmd += ["-e", "boot", str(args["boot"])]
    if args.get("extras"):
        cmd += str(args["extras"]).split()
    code, out = adb(*cmd, serial=serial, timeout=60)
    return j({"ok": code == 0 and "Error" not in out, "serial": serial, "component": f"{pkg}/{act}", "out": out})


def tool_stop_app(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    pkg = args.get("pkg") or PKG
    code, out = adb("shell", "am", "force-stop", pkg, serial=serial, timeout=30)
    return j({"ok": code == 0, "serial": serial, "out": out})


def tool_screencap(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    name = args.get("name") or ("device_" + time.strftime("%H%M%S"))
    if not name.lower().endswith(".png"):
        name += ".png"
    os.makedirs(SHOTS, exist_ok=True)
    path = os.path.join(SHOTS, name)
    code, data = adb("exec-out", "screencap", "-p", serial=serial, timeout=60, binary=True)
    if code != 0 or not data or not data.startswith(b"\x89PNG"):
        return j({"ok": False, "serial": serial, "error": "screencap 실패", "code": code, "bytes": len(data) if data else 0})
    with open(path, "wb") as f:
        f.write(data)
    return j({"ok": True, "serial": serial, "path": path, "bytes": len(data)})


def tool_logcat(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    if args.get("clear"):
        adb("logcat", "-c", serial=serial, timeout=20)
        return j({"ok": True, "cleared": True})
    lines = int(args.get("lines", 120))
    filt = args.get("filter")
    cmd = ["logcat", "-d", "-v", "time"]
    if filt == "all":
        pass
    elif filt:
        cmd += str(filt).split()
    else:
        cmd += ["*:S", "Unity:V", "DEBUG:I", "AndroidRuntime:E", "ActivityManager:I"]
    code, out = adb(*cmd, serial=serial, timeout=60)
    tail = "\n".join(out.splitlines()[-lines:])
    return tail if tail else "(로그 없음)"


def tool_input(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    cmd = str(args.get("command", "")).strip().split()
    if not cmd:
        return "command 필요: tap x y | swipe x1 y1 x2 y2 [ms] | key KEYCODE_BACK | text 문자열"
    kind = cmd[0].lower()
    if kind == "tap" and len(cmd) == 3:
        a = ["input", "tap", cmd[1], cmd[2]]
    elif kind == "swipe" and len(cmd) >= 5:
        a = ["input", "swipe"] + cmd[1:6]
    elif kind == "key" and len(cmd) == 2:
        a = ["input", "keyevent", cmd[1]]
    elif kind == "text" and len(cmd) >= 2:
        a = ["input", "text", " ".join(cmd[1:]).replace(" ", "%s")]
    else:
        return "형식 오류: " + " ".join(cmd)
    code, out = adb("shell", *a, serial=serial, timeout=30)
    return j({"ok": code == 0, "serial": serial, "out": out})


def tool_shell(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    c = str(args.get("command", "")).strip()
    if not c:
        return "command 필요"
    code, out = adb("shell", c, serial=serial, timeout=float(args.get("timeout", 60)))
    return j({"ok": code == 0, "serial": serial, "out": out[-4000:]})


def tool_props(args):
    serial = pick_serial(args)
    if not serial:
        return j({"ok": False, "error": "연결된 기기 없음"})
    keys = ["ro.product.model", "ro.build.version.release", "ro.build.version.sdk", "ro.product.cpu.abi", "ro.product.cpu.abilist", "sys.boot_completed"]
    info = {"serial": serial}
    for k in keys:
        _, v = adb("shell", "getprop", k, serial=serial, timeout=15)
        info[k] = v.strip()
    _, wm = adb("shell", "wm", "size", serial=serial, timeout=15)
    info["wm.size"] = wm.strip()
    _, pk = adb("shell", "pm", "path", PKG, serial=serial, timeout=15)
    info["app_installed"] = "package:" in pk
    return j(info)


TOOLS = [
    {"name": "android_ping", "description": "adb/emulator 존재·버전, 연결된 기기, AVD 목록을 한 번에 확인(세션 시작 시 1회).",
     "inputSchema": {"type": "object", "properties": {}}},
    {"name": "android_devices", "description": "adb devices -l — 연결된 기기/에뮬레이터 목록.",
     "inputSchema": {"type": "object", "properties": {}}},
    {"name": "android_avds", "description": "emulator -list-avds — 만들어져 있는 AVD 이름 목록.",
     "inputSchema": {"type": "object", "properties": {}}},
    {"name": "android_start_avd", "description": "AVD 를 띄우고 부팅 완료까지 기다린다(기본 .env ANDROID_AVD=CoastRun_ARM64). 이미 에뮬레이터가 떠 있으면 그대로 사용.",
     "inputSchema": {"type": "object", "properties": {"avd": {"type": "string"}, "gpu": {"type": "string", "description": "auto|host|swiftshader_indirect"}, "wait_seconds": {"type": "number"}, "extra": {"type": "string", "description": "추가 emulator 인자"}}}},
    {"name": "android_kill_avd", "description": "에뮬레이터 종료(adb emu kill).",
     "inputSchema": {"type": "object", "properties": {"serial": {"type": "string"}}}},
    {"name": "android_install", "description": "APK 설치(-r). 기본 C:\\dev\\game\\Builds\\CoastRun.apk. 478MB 급이면 수 분 걸림.",
     "inputSchema": {"type": "object", "properties": {"apk": {"type": "string"}, "serial": {"type": "string"}, "timeout": {"type": "number"}}}},
    {"name": "android_launch", "description": "앱 실행(am start). boot 로 CoastRun 부트 모드(run|marbles 등) 전달.",
     "inputSchema": {"type": "object", "properties": {"boot": {"type": "string"}, "pkg": {"type": "string"}, "activity": {"type": "string"}, "extras": {"type": "string"}, "serial": {"type": "string"}}}},
    {"name": "android_stop_app", "description": "앱 강제 종료(am force-stop).",
     "inputSchema": {"type": "object", "properties": {"pkg": {"type": "string"}, "serial": {"type": "string"}}}},
    {"name": "android_screencap", "description": "기기 화면 캡처를 Tools/_shots/<name>.png 로 저장하고 경로를 돌려준다.",
     "inputSchema": {"type": "object", "properties": {"name": {"type": "string"}, "serial": {"type": "string"}}}},
    {"name": "android_logcat", "description": "logcat 덤프. 기본 필터 Unity/DEBUG/AndroidRuntime/ActivityManager, 마지막 lines 줄. filter='all' 로 전체, clear=true 로 비우기.",
     "inputSchema": {"type": "object", "properties": {"lines": {"type": "integer"}, "filter": {"type": "string"}, "clear": {"type": "boolean"}, "serial": {"type": "string"}}}},
    {"name": "android_input", "description": "입력 주입: tap x y | swipe x1 y1 x2 y2 [ms] | key KEYCODE_BACK | text 문자열",
     "inputSchema": {"type": "object", "properties": {"command": {"type": "string"}, "serial": {"type": "string"}}, "required": ["command"]}},
    {"name": "android_shell", "description": "임의 adb shell 명령 한 줄.",
     "inputSchema": {"type": "object", "properties": {"command": {"type": "string"}, "serial": {"type": "string"}, "timeout": {"type": "number"}}, "required": ["command"]}},
    {"name": "android_props", "description": "기기 정보: 모델·Android 버전·API·ABI·해상도·부팅 완료·앱 설치 여부.",
     "inputSchema": {"type": "object", "properties": {"serial": {"type": "string"}}}},
]
HANDLERS = {"android_ping": tool_ping, "android_devices": tool_devices, "android_avds": tool_avds,
            "android_start_avd": tool_start_avd, "android_kill_avd": tool_kill_avd, "android_install": tool_install,
            "android_launch": tool_launch, "android_stop_app": tool_stop_app, "android_screencap": tool_screencap,
            "android_logcat": tool_logcat, "android_input": tool_input, "android_shell": tool_shell, "android_props": tool_props}


# ── MCP stdio 루프 ─────────────────────────────────────────────────
def reply(msg_id, result=None, error=None):
    m = {"jsonrpc": "2.0", "id": msg_id}
    if error is not None:
        m["error"] = error
    else:
        m["result"] = result
    sys.stdout.write(json.dumps(m, ensure_ascii=False) + "\n")
    sys.stdout.flush()


def main():
    sys.stdin.reconfigure(encoding="utf-8")
    sys.stdout.reconfigure(encoding="utf-8")
    for line in sys.stdin:
        line = line.strip()
        if not line:
            continue
        try:
            msg = json.loads(line)
        except Exception:
            continue
        mid = msg.get("id")
        method = msg.get("method", "")
        params = msg.get("params") or {}
        if method == "initialize":
            reply(mid, {"protocolVersion": params.get("protocolVersion", "2024-11-05"),
                        "capabilities": {"tools": {}},
                        "serverInfo": {"name": "coastrun-android", "version": "1.0"}})
        elif method == "notifications/initialized" or mid is None:
            continue
        elif method == "ping":
            reply(mid, {})
        elif method == "tools/list":
            reply(mid, {"tools": TOOLS})
        elif method == "tools/call":
            name = params.get("name"); args = params.get("arguments") or {}
            h = HANDLERS.get(name)
            if not h:
                reply(mid, error={"code": -32601, "message": "unknown tool " + str(name)})
                continue
            try:
                text = h(args)
                reply(mid, {"content": [{"type": "text", "text": str(text)}], "isError": False})
            except Exception as e:
                reply(mid, {"content": [{"type": "text", "text": "오류: " + repr(e)}], "isError": True})
        else:
            reply(mid, error={"code": -32601, "message": "method not found: " + method})


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--selftest":
        # PC 에서 직접 확인용:  python Tools\Mcp\android_mcp_server.py --selftest
        print(tool_ping({}))
        devs, _ = devices()
        if devs:
            print(tool_props({}))
        sys.exit(0)
    main()
