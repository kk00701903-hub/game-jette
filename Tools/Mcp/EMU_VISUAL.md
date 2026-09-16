# 에뮬레이터 시각 스모크 (Unity 6 / 이 PC)

## 왜 APK 시각 확인이 막히는가

| 경로 | 결과 |
|------|------|
| x86_64 AVD + ARM APK | `SIGILL` (native_bridge) |
| ARM64 AVD + ARM APK | Emulator 37: *「arm64 is not supported by QEMU2 on x86_64 host」* |
| x86_64 APK | Unity 6000.5: *「X86_64 is no longer supported」* — 빌드에서 제거됨 |

→ **이 환경에서는 Android Studio 에뮬로 Unity 6 APK 게임플레이 화면을 볼 수 없다.**  
시각 확인은 **실기기 adb** 또는 **Unity 에디터 Play + CoastRemote shot**.

## 실기기

```bat
adb install -r Builds\CoastRun.apk
adb shell am start -n com.jette.coastrun/com.unity3d.player.UnityPlayerActivity -e boot run
adb exec-out screencap -p > Tools\_shots\device_run.png

adb shell am start -n com.jette.coastrun/com.unity3d.player.UnityPlayerActivity -e boot marbles
adb exec-out screencap -p > Tools\_shots\device_marbles.png
```

## 에디터 (CoastRemote :47001)

```bat
# Unity 에디터에서 프로젝트 연 뒤
python Tools\Mcp\editor_visual_smoke.py
```

메뉴: `Coast Run/Dev/Mission - Marbles`, `Coast Run/Screenshot x3`.

## 스크립트

- `Tools/Mcp/visual_emu_smoke.py` — ARM AVD 시도(현재 호스트에선 부팅 실패)
- `Tools/Mcp/editor_visual_smoke.py` — 에디터 Play 캡처
