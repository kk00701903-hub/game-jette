using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// Emulator / adb smoke:
    ///   -e boot run     → skip prologue, start run (clouds/coins)
    ///   -e boot marbles → open 구슬치기 mission fullscreen on Title
    public sealed class EmuSmokeBoot : MonoBehaviour
    {
        public const string PrefArmed = "CoastRun_EmuSmokeBoot";
        public const string PrefMode = "CoastRun_EmuSmokeMode";
        private float _armedAt;
        private bool _fired;
        private bool _checkedIntent;
        private string _mode = "run";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRunner()
        {
            if (Object.FindAnyObjectByType<EmuSmokeBoot>() != null) return;
            var go = new GameObject("EmuSmokeBoot");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<EmuSmokeBoot>();
        }

        private void Awake()
        {
            _armedAt = Time.unscaledTime;
        }

        private void Update()
        {
            if (!_checkedIntent)
            {
                _checkedIntent = true;
                string boot = ReadBootExtra();
                if (boot == "run" || boot == "marbles")
                {
                    _mode = boot;
                    PlayerPrefs.SetInt(MainMenuController.SkipPrologueKey, 1);
                    PlayerPrefs.SetInt(PrefArmed, 1);
                    PlayerPrefs.SetString(PrefMode, boot);
                    PlayerPrefs.Save();
                    Debug.Log("[EmuSmoke] Intent boot=" + boot + " armed");
                }
                else if (PlayerPrefs.GetInt(PrefArmed, 0) != 1)
                {
                    Destroy(gameObject);
                    return;
                }
                else
                    _mode = PlayerPrefs.GetString(PrefMode, "run");
            }

            if (_fired) return;
            if (PlayerPrefs.GetInt(PrefArmed, 0) != 1) return;
            if (Time.unscaledTime - _armedAt < 2.5f) return;
            var dir = GameDirector.Instance;
            if (dir == null || dir.Flow == null) return;
            if (dir.Flow.IsBusy) return;
            if (dir.Flow.State != FlowState.Title) return;

            _fired = true;
            PlayerPrefs.SetInt(PrefArmed, 0);
            PlayerPrefs.Save();
            if (_mode == "marbles")
            {
                Debug.Log("[EmuSmoke] Title ready → Mission Marbles");
                OpenMarblesOverlay();
            }
            else
            {
                Debug.Log("[EmuSmoke] Title ready → OnTitleStartPressed");
                dir.Flow.OnTitleStartPressed();
            }
            Destroy(gameObject);
        }

        private static void OpenMarblesOverlay()
        {
            var canvas = CoastUiCanvas.Create("EmuMarblesCanvas", 800);
            var root = CoastUiCanvas.Root(canvas);
            var host = new GameObject("Host", typeof(RectTransform)).GetComponent<RectTransform>();
            host.SetParent(root, false);
            host.anchorMin = Vector2.zero; host.anchorMax = Vector2.one;
            host.offsetMin = host.offsetMax = Vector2.zero;
            // Dim so RawImage alpha bugs are obvious
            var dim = host.gameObject.AddComponent<Image>();
            dim.color = new Color(0.12f, 0.10f, 0.14f, 1f);
            dim.raycastTarget = false;
            MissionMiniGames.Start(ChapterMission.Kind.Marbles, host, true, _ => { });
        }

        private static string ReadBootExtra()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    if (activity == null) return null;
                    using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
                    {
                        if (intent == null) return null;
                        return intent.Call<string>("getStringExtra", "boot");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[EmuSmoke] Intent read failed: " + e.Message);
                return null;
            }
#else
            return PlayerPrefs.GetString("CoastRun_BootExtra", "");
#endif
        }
    }
}
