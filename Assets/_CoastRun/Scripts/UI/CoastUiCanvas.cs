using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoastRun
{
    /// Overlay HUD clipped to the portrait camera, with inner padding so labels aren't cropped.
    public static class CoastUiCanvas
    {
        public const string SafeAreaName = "PortraitSafeArea";
        public const string InsetName = "HudInset";
        public const float HudPad = 28f;
        /// 배치 코드의 단위(720×1280) → 캔버스 기준(1080×1920) 배율
        public const float DesignScale = 1.5f;

        /// Every scene in the flow is an empty shell — the world, the canvases and the
        /// buttons are all built at runtime. Nothing was building the one object Unity UI
        /// needs to deliver a click: an EventSystem. Each canvas got a GraphicRaycaster,
        /// which finds the button under the finger, but with no EventSystem there was
        /// nobody to ask. START sat on screen and ignored every tap.
        ///
        /// One persistent EventSystem is enough for the whole app; it survives scene loads.
        public static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;
            if (Object.FindAnyObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(go);
            go.AddComponent<CoastRaycastWatchdog>();
#if UNITY_EDITOR
            go.AddComponent<CoastDebugClicker>();
#endif
        }

        public static Canvas Create(string name, int sortingOrder, Transform parent = null)
        {
            EnsureEventSystem();

            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            if (parent != null)
                go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            // 18차-3: 기준 해상도 1080×1920(FHD 9:16), Match 0.5 — 16:9~22:9 대응 표준 설정.
            // 화면 배치 코드는 720×1280 단위로 쓰여 있으므로 HudInset을 1.5배로 두어 그대로 맞춘다(DesignScale).
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeGo = new GameObject(SafeAreaName, typeof(RectTransform));
            safeGo.transform.SetParent(canvas.transform, false);
            var safe = safeGo.GetComponent<RectTransform>();
            safe.anchorMin = Vector2.zero;
            safe.anchorMax = Vector2.one;
            safe.offsetMin = Vector2.zero;
            safe.offsetMax = Vector2.zero;

            var insetGo = new GameObject(InsetName, typeof(RectTransform));
            insetGo.transform.SetParent(safe, false);
            var inset = insetGo.GetComponent<RectTransform>();
            inset.anchorMin = inset.anchorMax = new Vector2(0.5f, 0.5f);
            inset.pivot = new Vector2(0.5f, 0.5f);
            inset.anchoredPosition = Vector2.zero;
            inset.localScale = new Vector3(DesignScale, DesignScale, 1f);
            inset.sizeDelta = new Vector2(720f - 2f * HudPad, 1280f - 2f * HudPad);   // 실제 크기는 CoastPortraitSafeArea가 매 프레임 갱신

            if (go.GetComponent<CoastPortraitSafeArea>() == null)
                go.AddComponent<CoastPortraitSafeArea>();

            return canvas;
        }

        public static RectTransform Root(Canvas canvas)
        {
            if (canvas == null)
                return null;
            var safe = canvas.transform.Find(SafeAreaName);
            if (safe != null)
            {
                var inset = safe.Find(InsetName) as RectTransform;
                if (inset != null)
                    return inset;
                return safe as RectTransform;
            }

            return canvas.GetComponent<RectTransform>();
        }

        /// 러닝 HUD 배치 기준 폭(인셋 664 = 720−2×HudPad). 갤럭시 S25U 등 좁은 세로폰에선
        /// 인셋이 ~596으로 줄어 상단 알약이 겹친다 → HudFit 으로 664 기준 배치를 통째로 축소.
        public const float HudDesignWidth = 664f;
        /// 같은 기준의 높이(1280−2×HudPad). 9:16보다 **짧은** 비율(16:9 게임뷰·태블릿·폴더블 펼침)에서
        /// 절대 좌표 배치가 위아래로 잘리던 것을 막기 위해 세로도 이 값을 기준으로 축소한다.
        public const float HudDesignHeight = 1224f;
        /// 축소 하한 — 여기까지는 잘리지 않고 줄어들고, 더 극단적인 비율에서는 잘린다.
        public const float MinFitScale = 0.45f;

        /// 절대 좌표(664×1224) 배치를 화면에 맞춰 통째로 축소하는 상자. 배경처럼 꽉 차야 하는 것은
        /// 이 상자 **바깥**(인셋)에 먼저 붙이고, 좌표로 놓는 UI만 이 상자의 자식으로 둔다.
        public static RectTransform MakeFitBox(RectTransform parent, string name = "Fit")
        {
            if (parent == null) return null;
            var existing = parent.Find(name) as RectTransform;
            if (existing != null) return existing;
            var go = new GameObject(name, typeof(RectTransform), typeof(CoastUiDesignFit));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<CoastUiDesignFit>().ApplyNow();
            return rt;
        }

        /// 부모 인셋 크기로 664×1224 기준 배율을 구한다(가로·세로 중 더 빡빡한 쪽, 1 초과 없음).
        public static float FitScale(float insetW, float insetH)
        {
            if (insetW < 8f || insetH < 8f) return 1f;
            float byW = insetW / HudDesignWidth;
            float byH = insetH / HudDesignHeight;
            return Mathf.Clamp(Mathf.Min(byW, byH), MinFitScale, 1f);
        }

        /// 해상도(+안전 영역) → HudInset 의 디자인 단위 크기와 축소 배율. 런타임과 에디터 점검 메뉴가
        /// 같은 식을 쓰게 한 곳에 모아 둔다. fit==1 이면 기준(664×1224)이 그대로 들어간다는 뜻이고,
        /// fit 이 MinFitScale 에 걸린 경우(=반환 크기가 664×1224보다 작다)에만 잘림이 생긴다.
        public static void DesignMetrics(float screenW, float screenH, float safeW, float safeH,
                                         out Vector2 insetSize, out float fit)
        {
            float lw = Mathf.Log(Mathf.Max(1f, screenW) / 1080f, 2f);
            float lh = Mathf.Log(Mathf.Max(1f, screenH) / 1920f, 2f);
            float scale = Mathf.Pow(2f, Mathf.Lerp(lw, lh, 0.5f));
            var sz = new Vector2(safeW, safeH) / scale / DesignScale;
            float iw = Mathf.Max(100f, sz.x - 2f * HudPad);
            float ih = Mathf.Max(100f, sz.y - 2f * HudPad);
            fit = FitScale(iw, ih);
            insetSize = new Vector2(iw / fit, ih / fit);
        }

        /// 러닝 크롬·여정 바가 같은 Fit 자식을 쓰도록. 폭이 충분하면 scale=1(에디터 720×1280 동일).
        public static RectTransform HudFitRoot(Canvas canvas)
        {
            var inset = Root(canvas);
            if (inset == null) return null;
            var existing = inset.Find("HudFit") as RectTransform;
            if (existing != null) return existing;
            var go = new GameObject("HudFit", typeof(RectTransform), typeof(CoastHudNarrowFit));
            go.transform.SetParent(inset, false);
            var frt = go.GetComponent<RectTransform>();
            // 위쪽 고정 — 축소해도 노치 아래 상단 바가 아래로 밀리지 않음
            frt.anchorMin = frt.anchorMax = new Vector2(0.5f, 1f);
            frt.pivot = new Vector2(0.5f, 1f);
            frt.anchoredPosition = Vector2.zero;
            go.GetComponent<CoastHudNarrowFit>().ApplyNow();
            return frt;
        }
    }

    /// 인셋이 HudDesignWidth×HudDesignHeight 보다 좁거나 **짧을** 때 Fit 자식을 비율 유지로 축소
    /// (매 프레임 안전영역 갱신 반영). 74차(사용자: 갤럭시 16:9 게임뷰에서 상하 잘림): 전엔 가로만
    /// 봤기 때문에 9:16보다 짧은 비율에서는 배율이 1로 남아 위아래가 그대로 잘렸다.
    public class CoastHudNarrowFit : MonoBehaviour
    {
        private RectTransform _self, _parent;
        private float _lastW = -1f, _lastH = -1f;

        private void LateUpdate() => ApplyNow();

        public void ApplyNow()
        {
            if (_self == null) _self = transform as RectTransform;
            if (_parent == null) _parent = _self != null ? _self.parent as RectTransform : null;
            if (_self == null || _parent == null) return;
            float rw = _parent.rect.width, rh = _parent.rect.height;
            if (rw < 8f || rh < 8f) return;
            if (Mathf.Abs(rw - _lastW) < 0.25f && Mathf.Abs(rh - _lastH) < 0.25f) return;
            _lastW = rw; _lastH = rh;
            float fit = CoastUiCanvas.FitScale(rw, rh);
            _self.localScale = new Vector3(fit, fit, 1f);
            // 스케일 후 부모와 같은 높이를 덮도록 내부 높이 = rh/fit
            _self.sizeDelta = new Vector2(CoastUiCanvas.HudDesignWidth, rh / fit);
        }
    }

    /// 664×1224 절대 좌표 배치를 화면 비율에 맞춰 통째로 축소하는 상자(가로·세로 동시 고려).
    /// 상자 크기는 부모(안전 영역 인셋)를 덮도록 rw/fit × rh/fit 로 잡아, 화면 가장자리에 붙인
    /// UI가 축소 후에도 실제 가장자리에 붙는다. 세로가 기준(1224)보다 남으면 그만큼 더 넓게 퍼진다.
    public class CoastUiDesignFit : MonoBehaviour
    {
        private RectTransform _self, _parent;
        private float _lastW = -1f, _lastH = -1f;

        private void LateUpdate() => ApplyNow();

        public void ApplyNow()
        {
            if (_self == null) _self = transform as RectTransform;
            if (_parent == null) _parent = _self != null ? _self.parent as RectTransform : null;
            if (_self == null || _parent == null) return;
            float rw = _parent.rect.width, rh = _parent.rect.height;
            if (rw < 8f || rh < 8f) return;
            if (Mathf.Abs(rw - _lastW) < 0.25f && Mathf.Abs(rh - _lastH) < 0.25f) return;
            _lastW = rw; _lastH = rh;
            float fit = CoastUiCanvas.FitScale(rw, rh);
            _self.localScale = new Vector3(fit, fit, 1f);
            // 가로는 배치 기준(664)을 넘기지 않는다 — 가운데 정렬 좌표(x0 = (664−…)/2)가 어긋나지 않게.
            _self.sizeDelta = new Vector2(Mathf.Min(CoastUiCanvas.HudDesignWidth, rw / fit), rh / fit);
        }
    }

    public class CoastPortraitSafeArea : MonoBehaviour
    {
        private RectTransform _safe, _inset;

        private void Awake()
        {
            var t = transform.Find(CoastUiCanvas.SafeAreaName);
            _safe = t as RectTransform;
            Apply();   // 첫 프레임 배치 코드가 실제 크기를 보게 즉시 한 번
        }

        private void LateUpdate() => Apply();

        public void Apply()
        {
            if (_safe == null)
                return;

            var cam = Camera.main;
            Rect r = cam != null ? cam.pixelRect : new Rect(0f, 0f, Screen.width, Screen.height);
            // 12차: 기기 안전 영역(펀치홀·노치·제스처 바)과 교집합. 전엔 카메라 사각형만 써서
            // 좌상단 하트·우상단 알약이 실기기에서 반쯤 잘렸다.
            Rect sa = Screen.safeArea;
            float x0 = Mathf.Max(r.xMin, sa.xMin), y0 = Mathf.Max(r.yMin, sa.yMin);
            float x1 = Mathf.Min(r.xMax, sa.xMax), y1 = Mathf.Min(r.yMax, sa.yMax);
            if (x1 - x0 > 8f && y1 - y0 > 8f) r = Rect.MinMaxRect(x0, y0, x1, y1);
            float w = Mathf.Max(1f, Screen.width);
            float h = Mathf.Max(1f, Screen.height);

            // Android: translucent status bar can overlay HUD while Screen.safeArea
            // still reports full height. Keep a minimum top clear when the bar is up,
            // or when top inset is suspiciously zero on a tall phone.
            float topClear = h - (r.y + r.height);
            float minTop = 0f;
#if UNITY_ANDROID && !UNITY_EDITOR
            float dp = Screen.dpi > 40f ? Screen.dpi : 160f;
            if (CoastSystemBars.StatusBarVisible || topClear < 2f)
                minTop = Mathf.Clamp(dp * 0.28f, 40f, h * 0.055f);   // ~28dp status / cutout
#endif
            if (topClear < minTop)
                r = Rect.MinMaxRect(r.xMin, r.yMin, r.xMax, h - minTop);

            _safe.anchorMin = new Vector2(r.x / w, r.y / h);
            _safe.anchorMax = new Vector2((r.x + r.width) / w, (r.y + r.height) / h);
            _safe.offsetMin = Vector2.zero;
            _safe.offsetMax = Vector2.zero;
            // HudInset: 안전 영역 크기를 디자인 단위(÷1.5)로 환산하고 안쪽 여백(HudPad)을 뺀다
            if (_inset == null) _inset = _safe.Find(CoastUiCanvas.InsetName) as RectTransform;
            if (_inset != null)
            {
                // 74차(사용자: 갤럭시 16:9 게임뷰에서 상하 잘림) — 배치 좌표는 664×1224(9:16) 기준인데
                // 9:16보다 **짧은** 비율(16:9·3:4·태블릿·폴더블 펼침)에선 인셋 높이가 1224보다 작아져
                // 위(주차 알약)와 아래(다음 턴 줄)가 화면 밖으로 나갔다. 인셋을 「최소 664×1224 보장 +
                // 비율 유지 축소」로 바꾼다: 자식 좌표계는 늘 기준 크기 이상이고, 축소분을 스케일로 돌려
                // 화면에서 차지하는 영역(sizeDelta×scale)은 예전과 동일하다 → 꽉 찬 배경도 그대로 full-bleed.
                CoastUiCanvas.DesignMetrics(w, h, r.width, r.height, out var insetSize, out float fit);
                float s = CoastUiCanvas.DesignScale * fit;
                _inset.localScale = new Vector3(s, s, 1f);
                _inset.sizeDelta = insetSize;
            }
        }
    }

    /// 18차-5: 입력 감시견 — "버튼이 눌리다가 어느 순간부터 안 눌림"의 전형적 원인은
    /// 보이지 않는데 레이캐스트만 막는 UI(알파 0인 CanvasGroup/Image가 화면을 덮음)다.
    /// 1초마다 훑어서 그런 것을 풀고 한 번만 로그를 남긴다. 버튼(Selectable)이 달린 투명 이미지는 의도된 것이라 건드리지 않는다.
    public class CoastRaycastWatchdog : MonoBehaviour
    {
        private float _next;
        private readonly System.Collections.Generic.HashSet<GameObject> _logged = new();

        private void Update()
        {
            if (Time.unscaledTime < _next) return;
            _next = Time.unscaledTime + 1f;
            foreach (var cg in FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None))
            {
                if (!cg.blocksRaycasts || cg.alpha > 0.02f || !cg.gameObject.activeInHierarchy) continue;
                // TitleUI is intentionally faded (splash / K-POP chapter select). Clearing
                // blocksRaycasts here used to leave the title permanently dead after restore.
                if (cg.gameObject.name == "TitleUI") continue;
                if (cg.GetComponentInParent<Selectable>() != null) continue;
                if (cg.GetComponentInParent<ScrollRect>() != null) continue;   // 24차-10(점검 2-5): 스크롤 수신 영역은 투명이 정상
                if (!CoversScreen(cg.transform as RectTransform)) continue;
                cg.blocksRaycasts = false;
                Log(cg.gameObject, "CanvasGroup alpha≈0");
            }
            foreach (var img in FindObjectsByType<Image>(FindObjectsSortMode.None))
            {
                if (!img.raycastTarget || img.color.a > 0.02f || !img.gameObject.activeInHierarchy) continue;
                if (img.GetComponentInParent<Selectable>() != null) continue;
                // 24차-10(점검 2-5): ScrollRect 의 투명 수신 이미지(알파 0.01)를 1초 뒤 꺼 버려 도감 팬아트·트로피 탭 스크롤이 죽었다.
                if (img.GetComponentInParent<ScrollRect>() != null) continue;
                if (!CoversScreen(img.rectTransform)) continue;
                img.raycastTarget = false;
                Log(img.gameObject, "Image alpha≈0");
            }
        }

        private static readonly Vector3[] _c = new Vector3[4];
        private static bool CoversScreen(RectTransform rt)
        {
            if (rt == null) return false;
            rt.GetWorldCorners(_c);
            float w = _c[2].x - _c[0].x, h = _c[2].y - _c[0].y;
            return w >= Screen.width * 0.6f && h >= Screen.height * 0.6f;
        }

        private void Log(GameObject go, string why)
        {
            if (_logged.Contains(go)) return;
            _logged.Add(go);
            Debug.LogWarning($"[RaycastWatchdog] 입력을 막던 투명 UI를 풀었다: {go.name} ({why}) 부모={go.transform.parent?.name}");
        }
    }

#if UNITY_EDITOR
    /// 18차 에디터 검증용: K = 마우스 아래 UI 요소에 클릭 이벤트를 직접 보낸다(원격 제어에서 왼쪽 클릭이 안 들어올 때).
    public class CoastDebugClicker : MonoBehaviour
    {
        private void Update()
        {
            if (CoastRemoteKeys.Down(KeyCode.J)) Dump();
            if (!CoastRemoteKeys.Down(KeyCode.K)) return;
            var es = EventSystem.current; if (es == null) return;
            var pd = new PointerEventData(es) { position = Input.mousePosition, button = PointerEventData.InputButton.Left };
            var hits = new System.Collections.Generic.List<RaycastResult>();
            es.RaycastAll(pd, hits);
            foreach (var h in hits)
            {
                var target = ExecuteEvents.GetEventHandler<IPointerClickHandler>(h.gameObject);
                if (target == null) continue;
                pd.pointerPress = target; pd.pointerPressRaycast = h; pd.pointerCurrentRaycast = h;
                ExecuteEvents.Execute(target, pd, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.Execute(target, pd, ExecuteEvents.pointerUpHandler);
                ExecuteEvents.Execute(target, pd, ExecuteEvents.pointerClickHandler);
                Debug.Log("[DebugClick] " + target.name);
                return;
            }
            Debug.Log("[DebugClick] no handler under " + Input.mousePosition);
        }

        /// J = 레이캐스트 진단 덤프(Tools/ui_debug.txt): 캔버스별 Raycast 결과·설정, 마우스/터치 상태
        private void Dump()
        {
            var sb = new System.Text.StringBuilder();
            var es = EventSystem.current;
            sb.Append($"t={Time.unscaledTime:0.0} ts={Time.timeScale} mouse={Input.mousePosition} screen={Screen.width}x{Screen.height} es={(es ? es.name : "null")} module={(es && es.currentInputModule ? es.currentInputModule.GetType().Name : "null")} enabled={(es ? es.enabled.ToString() : "-")} sel={(es && es.currentSelectedGameObject ? es.currentSelectedGameObject.name : "-")}\n");
            sb.Append($"mouseBtn0={Input.GetMouseButton(0)} touches={Input.touchCount} simulateMouse={Input.simulateMouseWithTouches} cursorLock={Cursor.lockState} visible={Cursor.visible}\n");
            var pd = new PointerEventData(es) { position = Input.mousePosition };
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                var gr = c.GetComponent<GraphicRaycaster>();
                var hits = new System.Collections.Generic.List<RaycastResult>();
                if (gr != null && gr.isActiveAndEnabled) gr.Raycast(pd, hits);
                sb.Append($"  canvas {c.name} order={c.sortingOrder} root={c.isRootCanvas} enabled={c.enabled} active={c.gameObject.activeInHierarchy} mode={c.renderMode} cam={(c.worldCamera ? c.worldCamera.name : "-")} scale={c.scaleFactor:0.00} rect={c.GetComponent<RectTransform>().rect.size} gr={(gr ? gr.isActiveAndEnabled.ToString() : "none")} hits={hits.Count}");
                foreach (var h in hits) sb.Append(" [" + h.gameObject.name + "]");
                var cgs = c.GetComponentsInChildren<CanvasGroup>(true);
                foreach (var g in cgs) if (g.blocksRaycasts && g.alpha < 0.05f && g.gameObject.activeInHierarchy) sb.Append($" BLOCKER?{g.name}(a={g.alpha:0.00})");
                sb.Append("\n");
            }
            foreach (var e in FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None)) sb.Append($"  eventsystem {e.name} enabled={e.enabled} active={e.gameObject.activeInHierarchy}\n");
            Debug.Log("[UIDump]\n" + sb);
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(Application.dataPath, "../Tools/ui_debug.txt"), sb + "\n"); } catch { }
        }
    }
#endif
}
