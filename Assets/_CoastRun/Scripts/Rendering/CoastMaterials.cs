using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoastRun
{
    /// Runtime URP materials — tracked so CoastPalette edits refresh live.
    public static class CoastMaterials
    {
        private static Shader _lit;
        private static Shader _unlit;
        private static Shader _toon;

        // 24차-6(점검 2-1): 스폰마다 만든 머티리얼을 강참조 리스트가 영원히 붙잡아 씬 재로드 후에도 해제되지 않았다
        // (타일당 ~50개 + 젤리 스테이지당 1000개 이상). 라이브 팔레트 갱신은 에디터 OnValidate에서만 쓰므로
        // 에디터에서만, 그것도 약참조로 추적한다. 빌드에선 추적 자체를 하지 않는다.
        private class Tracked
        {
            public WeakReference<Material> Ref;
            public Func<Color> Getter;
            public bool Unlit;
            public bool CustomShadow;   // 25차-1: SetShadow 로 지정한 그림자색은 팔레트 갱신 때 덮어쓰지 않는다
        }

#if UNITY_EDITOR
        private static readonly List<Tracked> TrackedMats = new List<Tracked>(128);
#endif

        /// 82차(모바일 APK): Shader.Find 가 스트리핑으로 null 이면 `new Material(null)` → ArgumentNullException(parameter: shader).
        /// Always Included(GraphicsSettings) + Resources/CoastRun/Shaders 로 포함을 보장하고,
        /// 그래도 없으면 Sprites/Default 까지 내려가 Material 생성에 null 을 넘기지 않는다.
        ///
        /// Player Settings 점검 메모 (재현 테스트용):
        /// - Managed Stripping Level 을 Minimal 로 낮춰도 동일하면 → 관리코드 스트리핑이 아니라
        ///   셰이더 미포함(Always Included / Resources) 문제.
        /// - Minimal 에서만 사라지고 Low/Medium 에서 나면 → stripping 영향 가능(link.xml / stripEngineCode).
        /// BuildMenu 는 Android 를 Low + stripEngineCode=false 로 고정한다.
        public static Shader Require(params string[] names)
        {
            if (names != null)
                for (int i = 0; i < names.Length; i++)
                {
                    if (string.IsNullOrEmpty(names[i])) continue;
                    var s = Shader.Find(names[i]);
                    if (s != null) return s;
                }
            var last = Shader.Find("Sprites/Default") ?? Shader.Find("UI/Default") ?? Shader.Find("Hidden/InternalErrorShader");
            if (last == null)
                Debug.LogError("[CoastMaterials] Shader not found (incl. Sprites/Default). Check Always Included Shaders.");
            return last;
        }

        public static Material NewMat(Shader shader)
        {
            if (shader == null)
            {
                shader = Require();
                if (shader == null)
                {
                    Debug.LogError("[CoastMaterials] Shader not found — cannot create Material");
                    return null;
                }
            }
            return new Material(shader);
        }

        public static Shader LitShader
        {
            get
            {
                if (_lit == null)
                {
                    _lit = Require(
                        "Universal Render Pipeline/Lit",
                        "Universal Render Pipeline/Simple Lit",
                        "Standard",
                        "Sprites/Default");
                }

                return _lit;
            }
        }

        public static Shader UnlitShader
        {
            get
            {
                if (_unlit == null)
                {
                    // The curved variant first, so sea, coins, wires and outlines bend with
                    // the road. Sky and clouds opt out via SetFlat.
                    _unlit = Require(
                        "CoastRun/UnlitCurved",
                        "Universal Render Pipeline/Unlit",
                        "Unlit/Color",
                        "Sprites/Default");
                }

                return _unlit;
            }
        }

        public static Shader ToonShader
        {
            get
            {
                if (_toon == null)
                    _toon = Shader.Find("CoastRun/ToonLit");   // optional — CreateToon falls back to Lit
                return _toon;
            }
        }

        public static Material CreateToon(Color color, Texture2D tex = null, float smoothness = 0.05f)
        {
            return CreateToon(color, null, tex, smoothness);
        }

        public static Material CreateToon(Color color, Func<Color> liveColor, Texture2D tex = null,
            float smoothness = 0.05f)
        {
            Material mat;
            if (ToonShader != null)
            {
                mat = NewMat(ToonShader);
                if (mat == null) return null;
                ApplyColor(mat, color, false);
                if (mat.HasProperty("_ShadowColor"))
                    mat.SetColor("_ShadowColor", CoastPalette.ShadowCool);
                if (mat.HasProperty("_Smoothness"))
                    mat.SetFloat("_Smoothness", smoothness);
                if (tex != null)
                {
                    if (mat.HasProperty("_BaseMap"))
                        mat.SetTexture("_BaseMap", tex);
                    else if (mat.HasProperty("_MainTex"))
                        mat.SetTexture("_MainTex", tex);
                }
            }
            else
            {
                mat = NewMat(LitShader);
                if (mat == null) return null;
                ApplyColor(mat, color, false);
                if (mat.HasProperty("_Smoothness"))
                    mat.SetFloat("_Smoothness", smoothness);
                if (tex != null)
                {
                    if (mat.HasProperty("_BaseMap"))
                        mat.SetTexture("_BaseMap", tex);
                    else if (mat.HasProperty("_MainTex"))
                        mat.SetTexture("_MainTex", tex);
                }
            }

            Track(mat, liveColor ?? (() => color), false);
            return mat;
        }

        public static Material CreateLit(Color color, float smoothness = 0.08f) =>
            CreateToon(color, null, null, smoothness);

        public static Material CreateLit(Func<Color> liveColor, float smoothness = 0.08f) =>
            CreateToon(liveColor(), liveColor, null, smoothness);

        public static Material CreateUnlit(Color color) => CreateUnlit(color, null);

        public static Material CreateUnlit(Color color, Func<Color> liveColor)
        {
            var mat = NewMat(UnlitShader);
            if (mat == null) return null;
            ApplyColor(mat, color, true);
            Track(mat, liveColor ?? (() => color), true);
            return mat;
        }

        public static Material CreateUnlit(Func<Color> liveColor) =>
            CreateUnlit(liveColor(), liveColor);

        public static Material CreateTransparent(Color color) =>
            CreateTransparent(color, null);

        public static Material CreateTransparent(Color color, Func<Color> liveColor)
        {
            // Prefer UnlitCurved so mobile builds keep real alpha (no URP strip of transparent variants).
            var mat = MakeUnlitCurvedTransparent(null, color, curveWeight: 0f, fogWeight: 0f);
            if (mat == null)
            {
                mat = CreateUnlit(color, liveColor);
                if (mat == null) return null;
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3000;
            }
            Track(mat, liveColor ?? (() => color), true);
            return mat;
        }

        /// Shared alpha-blended UnlitCurved setup. Avoids stock URP Unlit/Lit Transparent
        /// variants — on Android with Strip Unused Variants those force OutputAlpha→1
        /// (black boxes around clouds/coins, square SoftDisc blobs).
        static Material MakeUnlitCurvedTransparent(Texture2D tex, Color tint, float curveWeight, float fogWeight, bool additive = false)
        {
            var shader = Require("CoastRun/UnlitCurved", "Sprites/Default", "UI/Default");
            var mat = NewMat(shader);
            if (mat == null) return null;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            else mat.color = tint;
            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                else if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            }
            float src = (float)UnityEngine.Rendering.BlendMode.SrcAlpha;
            float dst = additive
                ? (float)UnityEngine.Rendering.BlendMode.One
                : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", src);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", dst);
            // Keep RT / framebuffer alpha opaque by default (MiniStage RawImage).
            if (mat.HasProperty("_SrcBlendAlpha")) mat.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            if (mat.HasProperty("_DstBlendAlpha")) mat.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", 0f);
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            if (mat.HasProperty("_CurveWeight")) mat.SetFloat("_CurveWeight", curveWeight);
            if (mat.HasProperty("_FogWeight")) mat.SetFloat("_FogWeight", fogWeight);
            mat.renderQueue = 3000;
            return mat;
        }

        /// Alpha-blended, fog-free painted backdrop (Hallasan far layer, mission yards).
        public static Material CreateTexturedTransparentNoFog(Texture2D tex, Color tint) =>
            MakeUnlitCurvedTransparent(tex, tint, curveWeight: 0f, fogWeight: 0f);

        /// 12차: 도로와 같이 휘는 투명 텍스처 언릿 — 블롭 그림자·아이템 광원·장애물 경고 링.
        public static Material CreateTexturedTransparentCurved(Texture2D tex, Color tint, bool additive = false) =>
            MakeUnlitCurvedTransparent(tex, tint, curveWeight: 1f, fogWeight: additive ? 0f : 1f, additive: additive);

        /// Alpha-blended textured unlit for painted billboards (clouds, far town, coin faces).
        /// Flat + no fog — UnlitCurved property Blend keeps real alpha on mobile builds.
        public static Material CreateTexturedTransparent(Texture2D tex, Color tint) =>
            MakeUnlitCurvedTransparent(tex, tint, curveWeight: 0f, fogWeight: 0f);

        /// Particle soft discs via UnlitCurved (CurveWeight=0). Stock URP Particles/Unlit
        /// loses transparent variants on Android → opaque squares.
        public static Material CreateParticle(Color color)
        {
            var mat = MakeUnlitCurvedTransparent(BlobShadow.SoftDisc(), color, curveWeight: 0f, fogWeight: 0f);
            if (mat == null) return null;
            ApplyColor(mat, color, true);
            return mat;
        }

        /// Pins a material in place while the rest of the world bends (sky, clouds).
        public static Material SetFlat(Material mat)
        {
            if (mat != null && mat.HasProperty("_CurveWeight"))
                mat.SetFloat("_CurveWeight", 0f);
            return mat;
        }

        /// Painted backdrops keep their own painted haze: no distance fog on top.
        public static Material SetNoFog(Material mat, float weight = 0f)
        {
            if (mat != null && mat.HasProperty("_FogWeight"))
                mat.SetFloat("_FogWeight", weight);
            return mat;
        }

        public static void RefreshTracked()
        {
#if UNITY_EDITOR
            for (int i = TrackedMats.Count - 1; i >= 0; i--)
            {
                var t = TrackedMats[i];
                if (t.Ref == null || !t.Ref.TryGetTarget(out var mat) || mat == null)
                {
                    TrackedMats.RemoveAt(i);
                    continue;
                }

                Color c = t.Getter != null ? t.Getter() : Color.magenta;
                ApplyColor(mat, c, t.Unlit);
                if (!t.Unlit && !t.CustomShadow && mat.HasProperty("_ShadowColor"))
                    mat.SetColor("_ShadowColor", CoastPalette.ShadowCool);
            }
#endif
        }

        /// 에디터 진단용: 현재 추적 중인 머티리얼 수.
        public static int TrackedCount
        {
            get
            {
#if UNITY_EDITOR
                return TrackedMats.Count;
#else
                return 0;
#endif
            }
        }

        public static void ApplyToonToHierarchy(Transform root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r.sharedMaterial == null)
                    continue;
                Color c = r.sharedMaterial.HasProperty("_BaseColor")
                    ? r.sharedMaterial.GetColor("_BaseColor")
                    : r.sharedMaterial.color;
                Texture t = r.sharedMaterial.HasProperty("_BaseMap")
                    ? r.sharedMaterial.GetTexture("_BaseMap")
                    : r.sharedMaterial.mainTexture;
                r.sharedMaterial = CreateToon(c, t as Texture2D);
            }
        }

        /// 25차-1: 주인공 피부처럼 따뜻한 그림자가 필요한 머티리얼. 에디터 팔레트 갱신(RefreshTracked)이
        /// 이 값을 ShadowCool 로 되돌려 왼팔이 파랗게 보이던 원인 → 지정한 색을 기억해 둔다.
        public static Material SetShadow(Material mat, Color shadow, float? threshold = null)
        {
            if (mat == null) return null;
            if (mat.HasProperty("_ShadowColor")) mat.SetColor("_ShadowColor", shadow);
            if (threshold.HasValue && mat.HasProperty("_ShadowThreshold")) mat.SetFloat("_ShadowThreshold", threshold.Value);
#if UNITY_EDITOR
            for (int i = TrackedMats.Count - 1; i >= 0; i--)
                if (TrackedMats[i].Ref != null && TrackedMats[i].Ref.TryGetTarget(out var m) && m == mat) { TrackedMats[i].CustomShadow = true; break; }
#endif
            return mat;
        }

        private static void Track(Material mat, Func<Color> getter, bool unlit)
        {
#if UNITY_EDITOR
            if (mat == null || getter == null)
                return;
            // 죽은 항목이 쌓이지 않게 512개마다 한 번 정리
            if ((TrackedMats.Count & 511) == 511)
                for (int i = TrackedMats.Count - 1; i >= 0; i--)
                    if (TrackedMats[i].Ref == null || !TrackedMats[i].Ref.TryGetTarget(out var m) || m == null)
                        TrackedMats.RemoveAt(i);
            TrackedMats.Add(new Tracked { Ref = new WeakReference<Material>(mat), Getter = getter, Unlit = unlit });
#endif
        }

        private static void ApplyColor(Material mat, Color color, bool unlit)
        {
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);
            else
                mat.color = color;
        }
    }
}
