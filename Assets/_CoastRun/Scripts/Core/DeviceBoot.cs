using UnityEngine;

namespace CoastRun
{
    /// 67차: 기기 공통 부트 설정.
    ///   6) 게임 중 화면 절전(자동 꺼짐) 방지 — Android 는 별도 권한 없이 Screen.sleepTimeout 으로 충분(WAKE_LOCK 은 유니티가 자동 추가).
    ///   1) 빌드에서 MeshCollider 등이 엔진 코드 스트리핑으로 빠져 `CreatePrimitive` 가 실패하던 문제 — 코드에서 형식을 직접 참조해 남긴다
    ///      (BuildMenu 는 stripEngineCode 도 끄고, Assets/link.xml 도 둔다 — 삼중 안전).
    /// 82차: 러닝 FX 가 Shader.Find null → new Material(null) 로 터지던 문제 — 부트에서 핵심 셰이더를 워밍업·로그.
    /// 재현 테스트: Player Settings > Managed Stripping Level 을 Minimal 로 낮춰 APK 빌드.
    ///   - Minimal 에서도 shader null → Always Included / Resources 미포함이 원인.
    ///   - Minimal 에서만 사라지고 Low+ 에서 재현 → stripping 영향 (BuildMenu 는 Low + stripEngineCode=false).
    public static class DeviceBoot
    {
        private static readonly System.Type[] KeepTypes =
        {
            typeof(MeshCollider), typeof(BoxCollider), typeof(SphereCollider), typeof(CapsuleCollider),
            typeof(MeshFilter), typeof(MeshRenderer), typeof(Rigidbody), typeof(ParticleSystem), typeof(TrailRenderer), typeof(LineRenderer),
        };

        private static readonly string[] WarmShaders =
        {
            "CoastRun/ChromaUnlit", "CoastRun/UnlitCurved", "CoastRun/ToonLit", "CoastRun/InkOutline", "CoastRun/UIDesaturate",
            "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit", "Universal Render Pipeline/Particles/Unlit",
            "Sprites/Default",
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (KeepTypes.Length == 0) Debug.Log("[DeviceBoot] keep");
            int miss = 0;
            for (int i = 0; i < WarmShaders.Length; i++)
                if (Shader.Find(WarmShaders[i]) == null) { miss++; Debug.LogWarning("[DeviceBoot] shader missing: " + WarmShaders[i]); }
            // Touch CoastMaterials so Unlit/Particle caches resolve before first FX spawn (~02_Run 20~40s).
            var _ = CoastMaterials.UnlitShader;
            var warmup = CoastMaterials.CreateParticle(Color.white);
            if (warmup != null) Object.Destroy(warmup);
            if (miss > 0) Debug.LogWarning("[DeviceBoot] missing shaders=" + miss + " (fallbacks active)");
        }
    }
}
