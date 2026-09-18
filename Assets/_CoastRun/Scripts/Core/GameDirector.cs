using System;
using UnityEngine;

namespace CoastRun
{
    public enum FlowState
    {
        Boot,
        Title,
        Run,
        StageClear
    }

    public enum TransitionType
    {
        None,
        Fade,
        WhiteFlash,
        SlowMotion
    }

    /// Persistent root — Story / Stage / Environment / Progression / Flow / UI.
    [DefaultExecutionOrder(-1000)]
    public class GameDirector : MonoBehaviour
    {
        public static GameDirector Instance { get; private set; }

        [SerializeField] private StageManager stages;
        [SerializeField] private DynamicEnvironmentManager environment;
        [SerializeField] private ProgressionManager progression;
        [SerializeField] private SceneFlowController flow;
        [SerializeField] private UIRoot uiRoot;
        [SerializeField] private GameManager gameManager;

        public StageManager Stages => stages;
        public DynamicEnvironmentManager Environment => environment;
        public ProgressionManager Progression => progression;
        public SceneFlowController Flow => flow;
        public UIRoot UI => uiRoot;
        public GameManager Game => gameManager;

        public bool CampaignCleared { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBeforeSceneLoad()
        {
            // Boot scene creates director; this is a safety net if Boot is skipped in editor.
        }

        public static GameDirector EnsureExists()
        {
            if (Instance != null)
                return Instance;

            var go = new GameObject("GameDirector");
            DontDestroyOnLoad(go);
            var dir = go.AddComponent<GameDirector>();
            dir.BuildChildren();
            return dir;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (flow == null)
                BuildChildren();
            CoastSystemBars.ApplyImmersive();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// Mobile: OS background suspends audio; on resume title DDOL bed can come back
        /// under the run/K-POP stem. Re-assert menu BGM off while in a run.
        /// Also re-hide Android status bar (OEM re-shows it over the run HUD).
        private void OnApplicationPause(bool pause)
        {
            if (pause) return;
            CoastSystemBars.ApplyImmersive();
            EnsureRunOwnsBgm();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus) return;
            CoastSystemBars.ApplyImmersive();
            EnsureRunOwnsBgm();
        }

        private static void EnsureRunOwnsBgm()
        {
            bool inRun = ArcadeRun.Active
                || (Instance != null && Instance.flow != null && Instance.flow.State == FlowState.Run);
            if (inRun) TitleAudio.StopMenuGlobal();
        }

        private void BuildChildren()
        {
            stages = GetOrAdd<StageManager>();
            environment = GetOrAdd<DynamicEnvironmentManager>();
            progression = GetOrAdd<ProgressionManager>();
            flow = GetOrAdd<SceneFlowController>();
            uiRoot = GetOrAdd<UIRoot>();
            gameManager = GetOrAdd<GameManager>();

            progression.Load();
            uiRoot.EnsureBuilt();
            flow.Bind(this);
        }

        private T GetOrAdd<T>() where T : Component
        {
            var c = GetComponent<T>() ?? gameObject.AddComponent<T>();
            return c;
        }
    }
}
