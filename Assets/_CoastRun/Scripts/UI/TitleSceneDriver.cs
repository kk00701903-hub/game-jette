using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoastRun
{
    /// Ensures Title scene wires MainMenu into SceneFlow.
    public class TitleSceneDriver : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Auto()
        {
            var scene = SceneManager.GetActiveScene();
            string n = scene.name;
            bool isTitle = n == SceneFlowController.TitleScene || scene.path.Contains("01_Title");
            if (!isTitle)
                return;
            if (Object.FindAnyObjectByType<TitleSceneDriver>() != null)
                return;

            var go = new GameObject("TitleSceneDriver");
            go.AddComponent<TitleSceneDriver>();
        }

        private void Start()
        {
            GameDirector.EnsureExists();
            // jette: 타이틀은 JetteTitleController(시안 한 장 + START) — 스토리/더보기 없음.
            if (Object.FindAnyObjectByType<JetteTitleController>() == null)
                gameObject.AddComponent<JetteTitleController>();
        }
    }
}
