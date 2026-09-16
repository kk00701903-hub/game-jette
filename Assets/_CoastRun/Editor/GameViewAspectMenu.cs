#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CoastRun.Editor
{
    /// Game 뷰를 세로(720×1280)로 고정하는 메뉴. 마우스로 드롭다운을 못 고를 때를 위한 키보드 경로.
    public static class GameViewAspectMenu
    {
        /// 74차(사용자: 「9:16으로 정상화」): 기본값 — 플레이어 설정(720×1280)과 같은 9:16 세로.
        ///   단축키 Ctrl+Alt+Shift+P.
        [MenuItem("Coast Run/Debug/Game view 9:16 세로 720x1280 (정상화) %#&p")]
        public static void SetPortrait() => Select("Portrait 720x1280", 720, 1280);

        /// 같은 9:16 이지만 캔버스 기준 해상도(1080×1920)와 1:1 — 글자 크기 확인용.
        [MenuItem("Coast Run/Debug/Game view 9:16 세로 1080x1920 (FHD)")]
        public static void SetPortraitFhd() => Select("Portrait 1080x1920", 1080, 1920);

        /// 95차(사용자: 「갤럭시 S25 기준으로 유니티 화면 사이즈 조정」): 기본 개발 크기.
        ///   S25 · S25 Edge = 1080×2340(19.5:9), S25+ · S25 Ultra = 1440×3120(19.5:9) — 비율이 같아 배치는 하나로 본다.
        ///   단축키 Ctrl+Alt+Shift+G.
        [MenuItem("Coast Run/Debug/Game view 갤럭시 S25 1080x2340 (기본) %#&g")]
        public static void SetGalaxyS25() => Select(GalaxyS25Label, 1080, 2340);

        [MenuItem("Coast Run/Debug/Game view 갤럭시 S25 Ultra 1440x3120")]
        public static void SetGalaxyS25Ultra() => Select("Galaxy S25 Ultra 1440x3120", 1440, 3120);

        // 74차(사용자: 갤럭시 16:9에서 상하 잘림) — 확인용 프리셋. 갤럭시 S7·J·A 세대가 16:9(=세로 9:16, 위 FHD와 같다),
        // S10 이후는 19:9~19.5:9 다. 잘림 점검은 9:16보다 **짧은** 비율(태블릿·폴더블)로 한다.
        [MenuItem("Coast Run/Debug/Game view 갤럭시 19.3:9 1440x3088")]
        public static void SetGalaxy193() => Select("Galaxy 19.3:9 1440x3088", 1440, 3088);

        [MenuItem("Coast Run/Debug/Game view 태블릿 16:10 1200x1920")]
        public static void SetTablet1610() => Select("Tablet 16:10 1200x1920", 1200, 1920);

        // ── 9:16 자동 정상화(1회) ────────────────────────────────────────
        //  74차(사용자: 「유니티 사이즈 9:16 으로 정상화해줘」). 게임뷰의 선택 크기는 에디터 창 상태라
        //  밖에서 고칠 수 없다 → 이 깃발 파일이 있으면 **다음 스크립트 리로드 때 한 번** 9:16 으로 돌려놓고
        //  깃발을 지운다. 이후 태블릿·폴더블 프리셋을 골라도 다시 건드리지 않는다.
        private const string NormalizeFlag = "Library/CoastRun_NormalizeGameView.flag";
        private const float Portrait916 = 9f / 16f;
        private const string GalaxyS25Label = "Galaxy S25 1080x2340";

        /// 지금 당장 못 고칠 때(에디터가 꺼져 있을 때) 예약해 두는 용도 — 다음 컴파일에 적용된다.
        /// 95차: 깃발 파일에 목표를 적어 둔다("S25" 또는 "9:16").
        [MenuItem("Coast Run/Debug/Game view 갤럭시 S25 예약(다음 컴파일)")]
        public static void ArmNormalizeS25()
        {
            System.IO.File.WriteAllText(NormalizeFlag, "S25");
            Debug.Log("[Coast Run] 다음 스크립트 컴파일에 게임뷰를 갤럭시 S25(1080×2340)로 맞춥니다.");
        }

        [MenuItem("Coast Run/Debug/Game view 9:16 정상화 예약(다음 컴파일)")]
        public static void ArmNormalize()
        {
            System.IO.File.WriteAllText(NormalizeFlag, "9:16");
            Debug.Log("[Coast Run] 다음 스크립트 컴파일에 게임뷰를 9:16 으로 되돌립니다.");
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void NormalizeOnReload()
        {
            if (!System.IO.File.Exists(NormalizeFlag)) return;
            string want = "9:16";
            try { want = System.IO.File.ReadAllText(NormalizeFlag).Trim(); } catch { /* 못 읽으면 9:16 */ }
            try { System.IO.File.Delete(NormalizeFlag); } catch { /* 지우기 실패해도 아래는 한 번은 돈다 */ }
            bool s25 = want.IndexOf("S25", StringComparison.OrdinalIgnoreCase) >= 0;
            EditorApplication.delayCall += () =>
            {
                var gv = FindGameView();
                if (gv == null) { Debug.Log("[Coast Run] Game 뷰가 열려 있지 않아 게임뷰 크기 조정을 건너뜁니다."); return; }
                float now = gv.position.height > 1f ? gv.position.width / gv.position.height : 0f;
                if (s25)
                {
                    SetGalaxyS25();
                    Debug.Log($"[Coast Run] 게임뷰를 갤럭시 S25(1080×2340, 비율 {1080f / 2340f:0.000})로 맞췄습니다. (직전 창 비율 {now:0.000})");
                }
                else
                {
                    SetPortraitFhd();
                    Debug.Log($"[Coast Run] 게임뷰를 9:16(1080×1920, 비율 {Portrait916:0.000})으로 정상화했습니다. (직전 창 비율 {now:0.000})");
                }
            };
        }

        /// 95차(사용자: 「화면 위아래로 짤리지 않게」): 에디터에서 위아래가 잘려 보이는 가장 흔한 원인은
        ///   레이아웃이 아니라 **Game 뷰 확대 배율**이다(Scale 슬라이더가 1× 보다 크면 창 밖으로 넘친 만큼 잘린다).
        ///   크기를 고를 때마다 창에 꼭 맞는 기본 배율로 되돌린다. 내부 API 라 없으면 조용히 넘어간다.
        [MenuItem("Coast Run/Debug/Game view 배율 창에 맞추기")]
        public static void FitZoomMenu()
        {
            var gv = FindGameView();
            if (gv == null) { Debug.Log("[Coast Run] Game 뷰가 열려 있지 않습니다."); return; }
            FitZoom(gv);
            gv.Repaint();
        }

        private static void FitZoom(EditorWindow gv)
        {
            if (gv == null) return;
            try
            {
                var t = gv.GetType();
                var def = t.GetProperty("defaultScale", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var snap = t.GetMethod("SnapZoom", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (def == null || snap == null) return;
                float scale = (float)def.GetValue(gv);
                snap.Invoke(gv, new object[] { scale });
                Debug.Log($"[Coast Run] Game 뷰 배율을 창에 맞춤(×{scale:0.00}) — 위아래 잘림은 배율이 1× 보다 클 때 생깁니다.");
            }
            catch (Exception e) { Debug.Log("[Coast Run] Game 뷰 배율 조정 건너뜀: " + e.Message); }
        }

        /// 열려 있는 Game 뷰(없으면 null) — 정상화 때문에 창을 새로 띄우지 않는다.
        private static EditorWindow FindGameView()
        {
            var t = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            if (t == null) return null;
            var all = Resources.FindObjectsOfTypeAll(t);
            return all != null && all.Length > 0 ? all[0] as EditorWindow : null;
        }

        /// 해상도별로 HudInset(배치 좌표계)이 기준 664×1224 를 확보하는지 한 번에 찍어 본다.
        /// OK = 잘리지 않음(기준 크기 이상 확보), CROP = 축소 하한(MinFitScale)에 걸려 잘림.
        [MenuItem("Coast Run/Debug/화면 비율 점검(로그)")]
        public static void CheckAspects()
        {
            (string name, int w, int h)[] cases =
            {
                ("에디터 기본 720x1280 (9:16)", 720, 1280),
                ("갤럭시 16:9 1080x1920", 1080, 1920),
                ("갤럭시 16:9 1440x2560", 1440, 2560),
                ("갤럭시 16:9 750x1334", 750, 1334),
                ("갤럭시 18.5:9 1080x2220", 1080, 2220),
                ("갤럭시 S25 19.5:9 1080x2340", 1080, 2340),
                ("갤럭시 S25 Ultra 1440x3120", 1440, 3120),
                ("S25 + 상단바 1080x2220", 1080, 2220),
                ("S24U 19.3:9 1440x3088", 1440, 3088),
                ("태블릿 16:10 1200x1920", 1200, 1920),
                ("탭 4:3 1536x2048", 1536, 2048),
                ("폴더블 펼침 1812x2176", 1812, 2176),
                ("정사각 1080x1080", 1080, 1080),
                ("가로 16:9 1920x1080", 1920, 1080),
            };

            var sb = new System.Text.StringBuilder();
            sb.Append($"[화면 비율 점검] 배치 기준 {CoastUiCanvas.HudDesignWidth}×{CoastUiCanvas.HudDesignHeight}, 축소 하한 {CoastUiCanvas.MinFitScale:0.00}\n");
            foreach (var c in cases)
            {
                CoastUiCanvas.DesignMetrics(c.w, c.h, c.w, c.h, out var inset, out float fit);
                bool ok = inset.x >= CoastUiCanvas.HudDesignWidth - 0.5f && inset.y >= CoastUiCanvas.HudDesignHeight - 0.5f;
                sb.Append($"  {(ok ? "OK  " : "CROP")} {c.name,-28} 화면비 {(float)c.w / c.h:0.000}  인셋 {inset.x:0}×{inset.y:0}  배율 {fit:0.000}\n");
            }
            Debug.Log(sb.ToString());
        }

        private static void Select(string label, int w, int h)
        {
            try
            {
                var asm = typeof(UnityEditor.Editor).Assembly;
                var sizesType = asm.GetType("UnityEditor.GameViewSizes");
                var singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                var instance = singletonType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                // 74차(사용자: 「9:16으로 정상화」): 전엔 항상 Standalone 그룹에 프리셋을 넣고 그 인덱스로 골랐다.
                //   게임뷰가 실제로 보여 주는 목록은 **활성 빌드 타깃**의 그룹(이 프로젝트는 Android)이라
                //   인덱스가 엇갈려 엉뚱한 크기가 선택됐다 → 현재 그룹을 읽어서 쓴다.
                var curGroupType = (GameViewSizeGroupType)sizesType.GetProperty("currentGroupType").GetValue(instance);
                var group = sizesType.GetMethod("GetGroup").Invoke(instance, new object[] { (int)curGroupType });
                var gt = group.GetType();

                int count = (int)gt.GetMethod("GetTotalCount").Invoke(group, null);
                int found = -1;
                for (int i = 0; i < count; i++)
                {
                    var size = gt.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
                    var st = size.GetType();
                    string text = st.GetProperty("baseText").GetValue(size) as string;
                    // 이름이 같거나(전에 만든 프리셋) 해상도가 같으면(유니티 기본 목록의 같은 크기) 그걸 쓴다 — 중복 생성 방지.
                    bool sameSize = (int)st.GetProperty("width").GetValue(size) == w
                                 && (int)st.GetProperty("height").GetValue(size) == h;
                    if (text == label || sameSize) { found = i; break; }
                }
                if (found < 0)
                {
                    var sizeType = asm.GetType("UnityEditor.GameViewSize");
                    var enumType = asm.GetType("UnityEditor.GameViewSizeType");
                    var ctor = sizeType.GetConstructor(new[] { enumType, typeof(int), typeof(int), typeof(string) });
                    var newSize = ctor.Invoke(new object[] { Enum.Parse(enumType, "FixedResolution"), w, h, label });
                    gt.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
                    found = (int)gt.GetMethod("GetTotalCount").Invoke(group, null) - 1;
                }

                var gameViewType = asm.GetType("UnityEditor.GameView");
                var gv = EditorWindow.GetWindow(gameViewType);
                var m = gameViewType.GetMethod("SizeSelectionCallback", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                m.Invoke(gv, new object[] { found, null });
                FitZoom(gv);
                gv.Repaint();
                Debug.Log($"[Coast Run] Game view → {label} ({curGroupType} 그룹 #{found}) · 화면비 {(float)w / h:0.000} (9:16 = 0.563, S25 = 0.462)");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Coast Run] Game view aspect reflection failed: " + e.Message
                    + "\n  수동: Game 뷰 왼쪽 위 해상도 드롭다운 → 720x1280 (또는 + 로 9:16 추가)");
            }
        }
    }
}
#endif
