using UnityEngine;

namespace CoastRun
{
    public enum GamePhase { Title, Run }

    /// jette: 본편의 육성·스토리 회차 관리자(GameManager)를 K-POP 러닝에 필요한 만큼만 남긴 축약판.
    /// Save(회차 세이브)는 이 빌드에 없어 늘 null — RunTuning 은 기본값, 코인·레벨·업적은 MetaProfile(profile.json)에 쌓인다.
    [DefaultExecutionOrder(-900)]
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        /// 회차 세이브 — jette 엔 육성이 없으므로 항상 null (GameManager.Active == false).
        public SaveData Save { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.Title;
        public SaveManager SaveSys { get; private set; }
        public MetaProfile Profile => SaveSys.Profile;
        public void WriteProfileNow() => SaveSys.WriteProfile(Profile);

        public bool IsRetry => false;
        public static bool Active => I != null && I.Save != null;
        public bool FlowBusy => Flow != null && Flow.IsBusy;

        public static GameManager Ensure()
        {
            if (I != null) return I;
            var dir = GameDirector.EnsureExists();
            var gm = dir.GetComponent<GameManager>() ?? dir.gameObject.AddComponent<GameManager>();
            return gm;
        }

        private void Awake()
        {
            if (I != null && I != this) { Destroy(this); return; }
            I = this;
            SaveSys = GetComponent<SaveManager>() ?? gameObject.AddComponent<SaveManager>();
        }

        private void OnDestroy()
        {
            if (I == this) I = null;
        }

        private SceneFlowController Flow => GameDirector.Instance != null ? GameDirector.Instance.Flow : null;

        public bool HasSave => SaveSys.HasSave;

        /// K-POP 러닝모드 — 육성 스탯이 있으면 읽는다(jette 에선 세이브 파일이 없으니 null → 기본값).
        public SaveData PeekSave() => Save ?? SaveSys.Load();

        /// 회차 세이브 저장(Save 가 있을 때만).
        public void Persist()
        {
            if (Save != null) SaveSys.Write(Save);
        }
    }
}
