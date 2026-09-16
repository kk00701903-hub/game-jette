# Blender MCP 연결 가이드 (Coast Run)

Cursor ↔ Blender 연결용. 서버 설정은 `.cursor/mcp.json`에 이미 들어 있음.

## 확인된 환경

- Blender **5.2.1 LTS** — `C:\Program Files\Blender Foundation\Blender 5.2\`
- `uv` / `uvx` — `C:\Users\ares2\.local\bin\`
- 애드온 설치됨 —  
  `%APPDATA%\Blender Foundation\Blender\5.2\scripts\addons\blender_mcp.py`

## 1회 활성화 (Blender에서)

1. **Blender 실행** (5.2)
2. **Edit → Preferences → Add-ons**
3. 검색: `MCP` 또는 `Blender MCP`
4. **Interface: MCP for Blender** 체크(활성화)
5. Preferences 닫기  
   (목록에 없으면 재시작 후 다시 확인)

## 매 세션 (연결)

1. Blender 3D 뷰포트에서 **N** 키 → 사이드 패널
2. **BlenderMCP** / **MCP** 탭
3. **Connect to MCP Server** / **Start MCP Server** 클릭
4. **Cursor 재시작** → **Settings → MCP**에서 `blender` Connected 확인

## 사용 예

- “빈 씬에 스케이트보드 만들기 (오렌지 휠)”
- “소녀 실루엣 로우폴리 캐릭터 만들고 FBX로 Assets/_CoastRun/Art/Character 내보내기”

## 주의

- Blender를 **켠 상태**에서만 MCP가 동작함
- Photoshop MCP와 동시에 켜도 됨
- `uv`는 `C:\Users\ares2\.local\bin`에 설치됨 (PATH에 없으면 Cursor 재시작)

## 2026-09-14 추가 — 애드온이 "설치는 됐는데 비활성" 일 때

Blender 5.2 를 새로 깔면 `blender_mcp.py` 가 addons 폴더에 있어도 **활성화가 풀려** 있어 MCP 가 "Could not connect" 를 낸다.
Preferences 창을 거치지 않고 **Scripting 워크스페이스 → Python 콘솔**에서 한 줄:

```python
bpy.ops.preferences.addon_enable(module="blender_mcp"); bpy.ops.wm.save_userpref()
```

- 애드온 `register()` 가 서버를 **자동 시작**하므로 별도 Start 버튼이 필요 없다("BlenderMCP server started on localhost:9876").
- `save_userpref()` 까지 하면 다음 실행부터 자동. 수동 시작은 `bpy.ops.blendermcp.start_server()`.
- Claude 컴퓨터 사용으로 할 때: 앱 이름 "Blender 5.2"(launcher) + `blender.exe` 전체 경로 두 개를 함께 승인 요청. Claude 앱 창이 오른쪽 사이드바를 가리므로 N 패널 대신 콘솔 경로가 확실하다.
