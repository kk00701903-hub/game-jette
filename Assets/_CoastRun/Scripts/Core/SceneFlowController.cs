using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoastRun
{
    /// jette: 앱 흐름 — Boot → Title → Run(K-POP 한 곡) → Title. 컷씬·엔딩·육성 씬은 이 빌드에 없다.
    /// (본편의 SceneFlowController 에서 스토리 경로를 걷어낸 축약판. 러닝 씬 프리로드·장막 전환은 그대로.)
    public class SceneFlowController : MonoBehaviour
    {
        public const string BootScene = "00_Boot";
        public const string TitleScene = "01_Title";
        public const string RunScene = "02_Run";

        private GameDirector _director;
        private FlowState _state = FlowState.Boot;
        private AsyncOperation _runPreload;
        private bool _runSceneReady;
        private int _pendingChapter = 1;
        private int _pendingStage = 1;
        private bool _busy;
        private float _busySince;
        public bool IsBusy => _busy;
        /// 워치독이 막힌 플로우를 강제로 푼다.
        public void ForceIdle() { StopAllCoroutines(); _busy = false; _director?.UI?.SetLoader(false); }
        /// 전환 코루틴이 예외로 끝나지 못하면 _busy 가 영원히 true → 12초 넘게 바쁘면 풀어준다.
        private bool BusyGuard()
        {
            if (_busy && Time.unscaledTime - _busySince > 12f) { Debug.LogWarning("[SceneFlow] busy for too long — releasing"); _busy = false; }
            return _busy;
        }

        public FlowState State => _state;
        public int PendingChapter => _pendingChapter;
        public int PendingStage => _pendingStage;
        /// 본편의 프롤로그 핸드오프 — jette 엔 없다(항상 false). GameSession 이 읽는다.
        public bool AwaitingPrologueHandoff => false;

        public event Action<FlowState, FlowState> OnStateChanged;

        public void Bind(GameDirector director)
        {
            _director = director;
        }

        public Task GoTo(FlowState next, TransitionType t) =>
            RunTask(GoToRoutine(next, t));

        private Task RunTask(IEnumerator routine)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(Wrap(routine, tcs));
            return tcs.Task;
        }

        private static IEnumerator Wrap(IEnumerator routine, TaskCompletionSource<bool> tcs)
        {
            yield return routine;
            tcs.TrySetResult(true);
        }

        private IEnumerator GoToRoutine(FlowState next, TransitionType transition)
        {
            if (BusyGuard() && next != FlowState.StageClear)
            {
                Debug.LogWarning($"[SceneFlow] GoTo({next}) ignored — busy ({_state})");
                yield break;
            }

            _busy = true; _busySince = Time.unscaledTime;
            var prev = _state;

            yield return PlayTransitionOut(transition, next);

            switch (next)
            {
                case FlowState.Title:
                    Time.timeScale = 1f;
                    yield return LoadSingle(TitleScene);
                    BeginPreloadRun();
                    break;

                case FlowState.Run:
                    yield return ActivateOrLoadRun();
                    break;

                case FlowState.StageClear:
                    // Stays in Run scene — UI only.
                    Time.timeScale = 1f;
                    break;

                case FlowState.Boot:
                    break;
            }

            SetState(next);
            yield return PlayTransitionIn(transition, prev, next);
            _busy = false;
        }

        private void SetState(FlowState next)
        {
            var prev = _state;
            _state = next;
            OnStateChanged?.Invoke(prev, next);
        }

        // ── Public flow entry points ───────────────────────────────────────

        public void BootToTitle()
        {
            StartCoroutine(BootRoutine());
        }

        private IEnumerator BootRoutine()
        {
            SetState(FlowState.Boot);
            _director?.UI?.Snap(1f, Color.black);
            yield return null;
            yield return null;
            yield return GoToRoutine(FlowState.Title, TransitionType.None);
        }

        /// 타이틀 START — K-POP 한 곡 달리기(ArcadeRun.StartKpop 이 부른다). 챕터 N = 스테이지 N.
        public void StartStoryRun(int stageIndex, bool withPrologue)
        {
            _pendingStage = Mathf.Clamp(stageIndex, 1, 20);
            _pendingChapter = Timeline.ArcOf(_pendingStage);
            TitleAudio.StopMenuGlobal();
            StartCoroutine(GoToRoutine(FlowState.Run, TransitionType.Fade));
        }

        /// 스테이지(곡) 완주 — K-POP 은 StageManager.KpopFinishCo → GameSession.EndKpopRun 이 결과창을 띄우므로
        /// 여기선 진행만 기록하고 아무 화면도 열지 않는다.
        public void NotifyStageCleared(StageDef stage, bool chapterComplete)
        {
            if (stage == null)
                return;
            _pendingChapter = stage.chapterIndex;
            _pendingStage = stage.stageIndex;
            _director?.Progression?.SaveCheckpoint(stage.chapterIndex, stage.stageIndex);
        }

        private IEnumerator PlayTransitionOut(TransitionType t, FlowState next)
        {
            var ui = _director != null ? _director.UI : null;
            if (ui == null)
                yield break;

            switch (t)
            {
                case TransitionType.None:
                    yield break;
                case TransitionType.Fade:
                {
                    float dur = 0.8f;
                    if (_state == FlowState.StageClear && next == FlowState.Run)
                        dur = 0.4f;
                    yield return ui.Fade(0f, 1f, dur, Color.black);
                    break;
                }
                case TransitionType.WhiteFlash:
                    yield return ui.WhiteFlash(0.15f, 0.6f);
                    ui.Snap(1f, Color.black);
                    break;
                case TransitionType.SlowMotion:
                    float elapsed = 0f;
                    float start = Time.timeScale > 0.01f ? Time.timeScale : 1f;
                    while (elapsed < 0.3f)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        Time.timeScale = Mathf.Lerp(start, 0.35f, elapsed / 0.3f);
                        yield return null;
                    }
                    Time.timeScale = 0.35f;
                    break;
            }
        }

        private IEnumerator PlayTransitionIn(TransitionType t, FlowState prev, FlowState next)
        {
            var ui = _director != null ? _director.UI : null;
            if (ui == null)
                yield break;

            float fadeIn = 0.4f;
            if (t == TransitionType.None)
            {
                ui.Snap(0f);
                yield break;
            }
            if (t == TransitionType.SlowMotion)
            {
                Time.timeScale = 1f;
                yield break;
            }
            if (t == TransitionType.WhiteFlash)
            {
                yield return ui.Fade(1f, 0f, 0.4f, Color.black);
                yield break;
            }
            if (t == TransitionType.Fade)
                yield return ui.Fade(1f, 0f, fadeIn, Color.black);
        }

        private void BeginPreloadRun()
        {
            if (_runPreload != null || _runSceneReady)
                return;
            if (!Application.CanStreamedLevelBeLoaded(RunScene))
                return;
            _runPreload = SceneManager.LoadSceneAsync(RunScene, LoadSceneMode.Additive);
            if (_runPreload != null)
                _runPreload.allowSceneActivation = false;
        }

        private IEnumerator ActivateOrLoadRun()
        {
            _director?.UI?.SetLoader(true);

            if (_runPreload != null)
            {
                _runPreload.allowSceneActivation = true;
                while (!_runPreload.isDone)
                    yield return null;
                _runPreload = null;
                _runSceneReady = true;
                yield return UnloadIfLoaded(TitleScene);
            }
            else if (!_runSceneReady || !IsSceneLoaded(RunScene))
            {
                yield return LoadSingle(RunScene);
                _runSceneReady = true;
            }

            _director?.UI?.SetLoader(false);
        }

        private IEnumerator LoadSingle(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning("[SceneFlow] Scene missing from build: " + sceneName);
                yield break;
            }

            _runSceneReady = sceneName == RunScene;
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (op != null && !op.isDone)
                yield return null;
        }

        private static IEnumerator UnloadIfLoaded(string sceneName)
        {
            if (!IsSceneLoaded(sceneName))
                yield break;
            var op = SceneManager.UnloadSceneAsync(sceneName);
            while (op != null && !op.isDone)
                yield return null;
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).name == sceneName)
                    return true;
            return false;
        }

        public static string ResolveRunScene() => RunScene;
        public static string ResolveTitleScene() => TitleScene;
    }
}
