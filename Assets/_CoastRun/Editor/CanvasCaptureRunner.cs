// 79차(사용자: 「캔버스 전략 수정하고 맞는지 유니티 캡쳐해서 확인」): 브릿지(47001)가 좀비 리스너에
// 잡혀 응답을 못 할 때를 위한 자급식 점검기. Library/CoastRun_CanvasCapture.txt 가 있으면
//   ① 지정 씬을 열고 ② 플레이로 들어가 ③ 해상도를 차례로 바꿔 캡쳐하고
//   ④ 필수 버튼이 세이프존(중앙 16:9) 안에 있는지 재서 Tools/_shots/canvas_report.txt 에 적고
//   ⑤ 플레이를 끈다. 파일 형식: 1줄 = 씬 이름, 그다음 줄마다 "가로x세로".
// 깃발은 스크립트 리로드 때 읽으므로, 그림만 바꿨을 때는 이 파일을 한 번 건드려 리로드를 만든다.
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoastRun.Editor
{
    [InitializeOnLoad]
    public static class CanvasCaptureRunner
    {
        public const string FlagPath = "Library/CoastRun_CanvasCapture.txt";
        public const string OutDir = "Tools/_shots";

        static CanvasCaptureRunner()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += Kick;
        }
        private static void Kick()
        {
            if (!File.Exists(FlagPath)) return;
            var job = Read();
            if (job.sizes.Count == 0) { File.Delete(FlagPath); return; }

            if (EditorApplication.isPlaying)
            {
                // 씬이 다르면(플레이 중에 깃발을 세운 경우) 일단 플레이를 끄고, 다음 리로드에서 씬부터 연다.
                if (!string.IsNullOrEmpty(job.scene) && SceneManager.GetActiveScene().name != job.scene)
                {
                    Debug.Log($"[CanvasCapture] 플레이 중지 후 {job.scene} 로 전환 예정(현재 {SceneManager.GetActiveScene().name})");
                    EditorApplication.isPlaying = false;
                    return;
                }
                if (UnityEngine.Object.FindAnyObjectByType<CanvasCaptureBehaviour>() != null) return;
                var go = new GameObject("CanvasCapture");
                var cap = go.AddComponent<CanvasCaptureBehaviour>();
                cap.sizes = job.sizes;
                return;
            }

            if (!string.IsNullOrEmpty(job.scene) && SceneManager.GetActiveScene().name != job.scene)
            {
                var guids = AssetDatabase.FindAssets("t:Scene " + job.scene);
                if (guids.Length > 0)
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            Debug.Log($"[CanvasCapture] {job.scene} 에서 {job.sizes.Count}개 해상도 캡쳐 시작");
            EditorApplication.EnterPlaymode();
        }

        private static (string scene, List<Vector2Int> sizes) Read()
        {
            string scene = null;
            var sizes = new List<Vector2Int>();
            foreach (var raw in File.ReadAllLines(FlagPath))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var parts = line.ToLowerInvariant().Split('x');
                if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                    sizes.Add(new Vector2Int(w, h));
                else if (scene == null)
                    scene = line;
            }
            return (scene, sizes);
        }

        public static void Finish(string report)
        {
            // 99차-2(사용자: 「화면 사이즈 멋대로 바꾸지 마라 · 20:9 로 고정」):
            //   점검하느라 바꾼 해상도를 반드시 제작 기준 20:9(1080×2400)로 되돌린다.
            try { SetGameViewSize(1080, 2400); } catch { /* 게임 뷰가 없으면 넘어간다 */ }
            Directory.CreateDirectory(OutDir);
            File.WriteAllText(Path.Combine(OutDir, "canvas_report.txt"), report, new UTF8Encoding(false));
            if (File.Exists(FlagPath)) File.Delete(FlagPath);
            Debug.Log("[CanvasCapture] 완료 — " + Path.GetFullPath(Path.Combine(OutDir, "canvas_report.txt")));
            EditorApplication.isPlaying = false;
        }

        /// 게임 뷰 크기를 고정 해상도로 (CoastRemote.SetGameViewSize 와 같은 방식).
        public static string SetGameViewSize(int w, int h)
        {
            var asm = typeof(EditorWindow).Assembly;
            var sizesType = asm.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instance = singleType.GetProperty("instance").GetValue(null, null);
            var groupTypeEnum = sizesType.GetProperty("currentGroupType").GetValue(instance, null);
            var group = sizesType.GetMethod("GetGroup").Invoke(instance, new object[] { (int)groupTypeEnum });
            var gt = group.GetType();
            int total = (int)gt.GetMethod("GetTotalCount").Invoke(group, null);
            int idx = -1;
            for (int i = 0; i < total; i++)
            {
                var gs = gt.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                var gsT = gs.GetType();
                int gw = (int)gsT.GetProperty("width").GetValue(gs, null), gh = (int)gsT.GetProperty("height").GetValue(gs, null);
                if (gw == w && gh == h && gsT.GetProperty("sizeType").GetValue(gs, null).ToString() == "FixedResolution") { idx = i; break; }
            }
            if (idx < 0)
            {
                var gvsType = asm.GetType("UnityEditor.GameViewSize");
                var gvsTypeEnum = asm.GetType("UnityEditor.GameViewSizeType");
                var ctor = gvsType.GetConstructor(new[] { gvsTypeEnum, typeof(int), typeof(int), typeof(string) });
                var ns = ctor.Invoke(new object[] { Enum.Parse(gvsTypeEnum, "FixedResolution"), w, h, "Capture " + w + "x" + h });
                gt.GetMethod("AddCustomSize").Invoke(group, new[] { ns });
                idx = total;
            }
            var t = Type.GetType("UnityEditor.GameView,UnityEditor");
            var gv = t != null ? EditorWindow.GetWindow(t, false, null, false) : null;
            if (gv == null) return "게임 뷰 없음";
            gv.GetType().GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
              ?.Invoke(gv, new object[] { idx, null });
            GameViewAspectMenu.FitZoomMenu();   // 확대(>1x)로 상하가 잘려 보이는 것 방지
            gv.Repaint();
            return w + "x" + h;
        }
    }

    /// 플레이 중에 해상도를 바꿔 가며 캡쳐하고, 필수 버튼이 세이프존 안인지 잰다.
    public class CanvasCaptureBehaviour : MonoBehaviour
    {
        public List<Vector2Int> sizes;

        /// 세이프존을 반드시 지켜야 하는 요소 — 스토리 모드·더보기·Play(+챕터 칩).
        private static readonly string[] Must = { "StoryBtn", "MoreBtn", "KpopBtn", "ChapterChip" };

        private IEnumerator Start()
        {
            var sb = new StringBuilder();
            sb.Append("[캔버스 전략 점검] 제작 1080×2400(20:9) · 세이프존 1080×1920(16:9) · S25 상하 30px 허용\n");
            sb.Append("유니티 " + Application.unityVersion + " · 씬 " + SceneManager.GetActiveScene().name + " · " + DateTime.Now + "\n\n");
            Directory.CreateDirectory(CanvasCaptureRunner.OutDir);
            yield return new WaitForSecondsRealtime(4f);   // 스플래시·페이드인 끝나게
            foreach (var sz in sizes)
            {
                string set = CanvasCaptureRunner.SetGameViewSize(sz.x, sz.y);
                yield return new WaitForSecondsRealtime(2.5f);
                for (int i = 0; i < 3; i++) yield return null;

                sb.Append($"── {sz.x}×{sz.y} (비 {(float)sz.x / sz.y:0.000}) — 게임뷰 {set}, 실제 {Screen.width}×{Screen.height}\n");
                CoastUiCanvas.DesignMetrics(Screen.width, Screen.height, Screen.safeArea.width, Screen.safeArea.height,
                                            out var inset, out float fit);
                CoastUiCanvas.SafeZoneMetrics(Screen.width, Screen.height, out float bgScale, out float crop, out bool safeOk);
                sb.Append($"   인셋 {inset.x:0}×{inset.y:0} 배율 {fit:0.000} · 배경배율 {bgScale:0.000} 상하잘림 {crop:0}px · 세이프존 {(safeOk ? "OK" : "잘림!")}\n");

                // 세이프존(중앙 16:9) 화면 좌표
                float szW = Mathf.Min(Screen.width, Screen.height * 1080f / 1920f);
                float szH = Mathf.Min(Screen.height, Screen.width * 1920f / 1080f);
                var zone = new Rect((Screen.width - szW) * 0.5f, (Screen.height - szH) * 0.5f, szW, szH);
                sb.Append($"   세이프존 화면영역 x {zone.xMin:0}~{zone.xMax:0}, y {zone.yMin:0}~{zone.yMax:0}\n");

                foreach (var name in Must)
                {
                    var rt = Find(name);
                    if (rt == null) { sb.Append($"   [{name}] 없음(이 화면에 없을 수 있음)\n"); continue; }
                    var r = ScreenRect(rt);
                    bool onScreen = r.xMin >= -1f && r.yMin >= -1f && r.xMax <= Screen.width + 1f && r.yMax <= Screen.height + 1f;
                    bool inZone = r.xMin >= zone.xMin - 1f && r.yMin >= zone.yMin - 1f && r.xMax <= zone.xMax + 1f && r.yMax <= zone.yMax + 1f;
                    sb.Append($"   [{name}] x {r.xMin:0}~{r.xMax:0}, y {r.yMin:0}~{r.yMax:0}"
                            + $"  화면안 {(onScreen ? "OK" : "밖!")}  세이프존안 {(inZone ? "OK" : "밖!")}\n");
                }

                string shot = Path.GetFullPath(Path.Combine(CanvasCaptureRunner.OutDir, $"canvas_{sz.x}x{sz.y}.png"));
                if (File.Exists(shot)) File.Delete(shot);
                ScreenCapture.CaptureScreenshot(shot, 1);
                yield return new WaitForSecondsRealtime(2f);
                sb.Append($"   캡쳐 {(File.Exists(shot) ? "저장" : "실패")} {shot}\n\n");
            }

            CanvasCaptureRunner.Finish(sb.ToString());
        }

        private static RectTransform Find(string name)
        {
            foreach (var rt in FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude))
                if (rt.name == name) return rt;
            return null;
        }

        private static readonly Vector3[] _c = new Vector3[4];
        private static Rect ScreenRect(RectTransform rt)
        {
            rt.GetWorldCorners(_c);
            var canvas = rt.GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector2 p = cam != null ? RectTransformUtility.WorldToScreenPoint(cam, _c[i]) : (Vector2)_c[i];
                x0 = Mathf.Min(x0, p.x); y0 = Mathf.Min(y0, p.y);
                x1 = Mathf.Max(x1, p.x); y1 = Mathf.Max(y1, p.y);
            }
            return Rect.MinMaxRect(x0, y0, x1, y1);
        }
    }
}
#endif
