# -*- coding: utf-8 -*-
"""CoastRemote(127.0.0.1:47001) 한 줄 명령 클라이언트.

사용:
  python Tools/Mcp/coast_remote.py wait           # 에디터가 살아나고 컴파일이 끝날 때까지
  python Tools/Mcp/coast_remote.py "res 1080 2340"
  python Tools/Mcp/coast_remote.py play / "shot s25" / "log 40"
"""
import json
import socket
import sys
import time

HOST, PORT = "127.0.0.1", 47001


def send(cmd, timeout=30.0):
    with socket.create_connection((HOST, PORT), timeout) as s:
        s.settimeout(timeout)
        s.sendall((cmd + "\n").encode("utf-8"))
        line = s.makefile(encoding="utf-8").readline().strip()
    try:
        return json.loads(line) if line else {}
    except Exception:
        return {"raw": line}


def wait_ready(limit=900.0, quiet=3.0):
    """ping 이 되고, compiling/updating 이 quiet 초 동안 계속 false 일 때까지."""
    t0 = time.time()
    calm = 0.0
    last = ""
    while time.time() - t0 < limit:
        try:
            st = send("status", 20.0)
        except Exception as e:
            msg = "bridge: %s" % type(e).__name__
            if msg != last:
                print("[%4.0fs] %s" % (time.time() - t0, msg))
                last = msg
            time.sleep(2.0)
            calm = 0.0
            continue
        busy = st.get("compiling") or st.get("updating")
        errs = st.get("compileErrors") or ""
        msg = "compiling=%s updating=%s playing=%s scene=%s" % (
            st.get("compiling"), st.get("updating"), st.get("playing"), st.get("scene"))
        if msg != last:
            print("[%4.0fs] %s" % (time.time() - t0, msg))
            last = msg
        if errs:
            print("=== 컴파일 에러 ===")
            print(errs)
            return False
        if busy:
            calm = 0.0
        else:
            calm += 1.5
            if calm >= quiet:
                print("[%4.0fs] ready" % (time.time() - t0))
                return True
        time.sleep(1.5)
    print("시간 초과")
    return False


if __name__ == "__main__":
    arg = " ".join(sys.argv[1:]) or "status"
    if arg == "wait":
        sys.exit(0 if wait_ready() else 1)
    try:
        print(json.dumps(send(arg), ensure_ascii=False))
    except Exception as e:
        print("연결 실패: %s: %s" % (type(e).__name__, e))
        sys.exit(2)
