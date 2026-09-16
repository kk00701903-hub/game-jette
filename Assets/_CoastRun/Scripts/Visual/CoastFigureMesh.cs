using UnityEngine;

namespace CoastRun
{
    /// 마이룸·러닝용 간단한 절차형 3D 피겨(Blender 미연결 시 폴백).
    /// 주인공·펫을 구/캡슐로 조립해 toon 머티리얼을 입힌다.
    public static class CoastFigureMesh
    {
        public static Transform BuildHaneul(Transform parent, float height = 1.6f)
        {
            var root = new GameObject("Fig_Haneul").transform;
            root.SetParent(parent, false);
            float s = height / 1.6f;
            var skin = new Color(1f, 0.82f, 0.70f);
            var hair = new Color(0.35f, 0.22f, 0.14f);
            var shirt = new Color(0.55f, 0.82f, 0.95f);
            var shorts = new Color(0.35f, 0.45f, 0.55f);
            Part(root, "Body", PrimitiveType.Capsule, new Vector3(0f, 0.72f * s, 0f), new Vector3(0.38f, 0.42f, 0.28f) * s, shirt);
            Part(root, "Head", PrimitiveType.Sphere, new Vector3(0f, 1.28f * s, 0f), Vector3.one * 0.42f * s, skin);
            Part(root, "Hair", PrimitiveType.Sphere, new Vector3(0f, 1.40f * s, -0.02f * s), new Vector3(0.46f, 0.28f, 0.48f) * s, hair);
            Part(root, "Bag", PrimitiveType.Capsule, new Vector3(0f, 0.28f * s, 0f), new Vector3(0.36f, 0.22f, 0.26f) * s, shorts);
            Part(root, "Pack", PrimitiveType.Cube, new Vector3(0f, 0.85f * s, -0.18f * s), new Vector3(0.28f, 0.32f, 0.12f) * s, new Color(0.20f, 0.55f, 0.55f));
            return root;
        }

        public static Transform BuildPet(Transform parent, PetKind kind, float height = 0.55f)
        {
            var root = new GameObject("Fig_Pet_" + kind).transform;
            root.SetParent(parent, false);
            switch (kind)
            {
                case PetKind.Sparrow:
                    Part(root, "Body", PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), new Vector3(0.38f, 0.32f, 0.42f) * (height / 0.55f), new Color(0.85f, 0.55f, 0.25f));
                    Part(root, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.42f, 0.08f), Vector3.one * 0.28f * (height / 0.55f), new Color(0.95f, 0.75f, 0.40f));
                    break;
                case PetKind.BlackPig:
                    Part(root, "Body", PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), new Vector3(0.55f, 0.40f, 0.48f) * (height / 0.55f), new Color(0.25f, 0.18f, 0.16f));
                    Part(root, "Snout", PrimitiveType.Sphere, new Vector3(0f, 0.26f, 0.28f), new Vector3(0.22f, 0.16f, 0.18f) * (height / 0.55f), new Color(0.55f, 0.35f, 0.35f));
                    break;
                case PetKind.WildGoose:
                    Part(root, "Body", PrimitiveType.Sphere, new Vector3(0f, 0.20f, 0f), new Vector3(0.42f, 0.30f, 0.55f) * (height / 0.55f), new Color(0.55f, 0.62f, 0.70f));
                    Part(root, "Neck", PrimitiveType.Capsule, new Vector3(0f, 0.42f, 0.18f), new Vector3(0.10f, 0.18f, 0.10f) * (height / 0.55f), new Color(0.70f, 0.75f, 0.80f));
                    Part(root, "Head", PrimitiveType.Sphere, new Vector3(0f, 0.58f, 0.26f), Vector3.one * 0.18f * (height / 0.55f), new Color(0.25f, 0.28f, 0.32f));
                    break;
                case PetKind.BikerThug:
                    Part(root, "Bike", PrimitiveType.Cube, new Vector3(0f, 0.18f, 0f), new Vector3(0.55f, 0.18f, 0.22f) * (height / 0.55f), new Color(0.35f, 0.38f, 0.45f));
                    Part(root, "WheelF", PrimitiveType.Cylinder, new Vector3(0.22f, 0.12f, 0f), new Vector3(0.18f, 0.04f, 0.18f) * (height / 0.55f), new Color(0.15f, 0.15f, 0.18f));
                    Part(root, "WheelB", PrimitiveType.Cylinder, new Vector3(-0.22f, 0.12f, 0f), new Vector3(0.18f, 0.04f, 0.18f) * (height / 0.55f), new Color(0.15f, 0.15f, 0.18f));
                    Part(root, "Rider", PrimitiveType.Capsule, new Vector3(0f, 0.38f, 0f), new Vector3(0.18f, 0.16f, 0.16f) * (height / 0.55f), new Color(0.95f, 0.55f, 0.70f));
                    break;
                default:
                    Part(root, "Ball", PrimitiveType.Sphere, new Vector3(0f, 0.22f, 0f), Vector3.one * 0.4f * (height / 0.55f), new Color(0.95f, 0.8f, 0.3f));
                    break;
            }
            return root;
        }

        /// UI용: 피겨를 잠깐 렌더해 스프라이트로 만든다(마이룸 RawImage).
        public static Sprite Capture(Transform figure, int px = 256)
        {
            if (figure == null) return null;
            var camGo = new GameObject("FigCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            cam.orthographic = true;
            cam.orthographicSize = 1.05f;
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 20f;
            cam.cullingMask = 1 << 31;
            SetLayerRecursive(figure.gameObject, 31);
            camGo.transform.position = new Vector3(0f, 0.85f, -4f);
            camGo.transform.LookAt(new Vector3(0f, 0.7f, 0f));
            figure.position = Vector3.zero;
            var rt = new RenderTexture(px, px, 16, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(px, px, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, px, px), 0, 0); tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.Destroy(rt); Object.Destroy(camGo);
            return Sprite.Create(tex, new Rect(0, 0, px, px), new Vector2(0.5f, 0f), 100f);
        }

        private static void Part(Transform parent, string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (type == PrimitiveType.Cylinder) go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            go.GetComponent<Renderer>().sharedMaterial = CoastMaterials.CreateToon(color, null, null, 0.35f);
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform c in go.transform) SetLayerRecursive(c.gameObject, layer);
        }
    }
}
