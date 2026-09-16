using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 66차-1: 육성 대회 러닝 — 꼬마+라이벌 2명이 같이 달린다.
    /// 사용자: 마을 대회 동반자는 **자전거**를 타고, 바퀴가 속도에 맞춰 돈다.
    public class ContestRivals : MonoBehaviour
    {
        public static ContestRivals Instance { get; private set; }

        private class Rival
        {
            public string name; public Color col; public Transform root; public Transform body; public float baseH;
            public float dist, lateral, laneTarget, hop, hopVel, speedMul, phase, nextLane, nextHop, mood, moodT;
            public int lane;
            public Transform[] wheels;
            // 86차: 장애물 회피·충돌
            public float stun, wobble, hitCool, lookCool; public ObstacleHazard lastHit; public Renderer[] rends; public Color[] rendCols;
        }

        private PlayerController _player;
        private readonly Rival[] _rivals = new Rival[3];
        private System.Random _rng;
        private float _laneOffset = 2.2f;
        private static Texture2D _wheelTex;

        public static void Create(PlayerController player, int seed)
        {
            if (player == null) return;
            if (Instance != null) Destroy(Instance.gameObject);
            var go = new GameObject("ContestRivals");
            var r = go.AddComponent<ContestRivals>();
            r._player = player;
            r._rng = new System.Random(seed);
            r._laneOffset = player.Config != null ? player.Config.laneOffset : 2.2f;
            r.Build();
        }

        public static void Clear() { if (Instance != null) Destroy(Instance.gameObject); Instance = null; }

        private void Awake() { Instance = this; }
        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Build()
        {
            string[] keys = { "Rival_Kid", "Rival_Boy", "Rival_Girl" };
            string[] fallback = { "MG_Char_ButlerBack", null, null };
            string[] ko = { "꼬마", "보미", "태오" };
            string[] en = { "Kid", "Bomi", "Taeo" };
            Color[] cols = { new Color(0.35f, 0.65f, 1f), new Color(1f, 0.55f, 0.25f), new Color(0.65f, 0.45f, 0.95f) };
            Color[] shirts = { new Color(0.35f, 0.70f, 1f), new Color(1f, 0.50f, 0.35f), new Color(0.55f, 0.40f, 0.90f) };
            float[] heights = { 1.55f, 1.65f, 1.60f };
            int[] lanes = { -1, 1, 0 };
            float[] starts = { 5f, -4f, 9f };
            _wheelTex = ArtAssets.LoadTexture("BikeWheel");
            for (int i = 0; i < 3; i++)
            {
                var tex = ArtAssets.LoadTexture(keys[i]) ?? (fallback[i] != null ? ArtAssets.LoadTexture(fallback[i]) : null);
                var r = new Rival { name = Loc.T(ko[i], en[i]), col = cols[i], baseH = heights[i], lane = lanes[i] };
                r.root = new GameObject("Rival_" + ko[i]).transform;
                r.root.SetParent(transform, false);
                r.body = MakeBikeBody(r.root, tex, heights[i], shirts[i], out r.wheels);
                r.dist = _player.PathDistance + starts[i];
                r.lateral = r.laneTarget = lanes[i] * _laneOffset;
                r.speedMul = 1f;
                r.phase = (float)_rng.NextDouble() * 6.28f;
                r.nextLane = 2f + (float)_rng.NextDouble() * 4f;
                r.nextHop = 4f + (float)_rng.NextDouble() * 6f;
                r.mood = 0f; r.moodT = 0f;
                BlobShadow.Attach(r.root, 0.7f);
                MakeTag(r);
                r.rends = r.body.GetComponentsInChildren<Renderer>(true);
                r.rendCols = new Color[r.rends.Length];
                for (int k = 0; k < r.rends.Length; k++) { var m = r.rends[k].sharedMaterial; r.rendCols[k] = m != null && m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : Color.white; }
                _rivals[i] = r;
            }
        }

        /// 자전거 프레임 + 돌아가는 바퀴 2개 + 탄 사람(스프라이트 또는 프리미티브).
        private static Transform MakeBikeBody(Transform root, Texture2D riderTex, float height, Color shirt, out Transform[] wheels)
        {
            var body = new GameObject("Bike").transform;
            body.SetParent(root, false);
            body.localPosition = Vector3.zero;

            var frame = CoastMaterials.CreateLit(new Color(0.25f, 0.28f, 0.35f), 0.35f);
            var accent = CoastMaterials.CreateLit(shirt, 0.4f);
            var dark = CoastMaterials.CreateLit(new Color(0.12f, 0.12f, 0.14f), 0.2f);
            var skin = CoastMaterials.CreateLit(new Color(0.98f, 0.84f, 0.70f), 0.1f);
            var pants = CoastMaterials.CreateLit(new Color(0.28f, 0.36f, 0.55f), 0.1f);

            // 프레임(옆모습에 가깝게 — 진행 방향 +Z)
            Box(body, "TubeTop", new Vector3(0f, 0.62f, 0.02f), new Vector3(0.06f, 0.06f, 0.72f), frame);
            Box(body, "TubeDown", new Vector3(0f, 0.38f, -0.05f), new Vector3(0.06f, 0.06f, 0.55f), frame);
            Box(body, "SeatPost", new Vector3(0f, 0.72f, -0.22f), new Vector3(0.05f, 0.35f, 0.05f), frame);
            Box(body, "Seat", new Vector3(0f, 0.92f, -0.22f), new Vector3(0.14f, 0.06f, 0.28f), dark);
            Box(body, "Fork", new Vector3(0f, 0.48f, 0.42f), new Vector3(0.05f, 0.45f, 0.05f), frame);
            Box(body, "Handle", new Vector3(0f, 0.95f, 0.40f), new Vector3(0.55f, 0.05f, 0.05f), dark);
            Box(body, "PedalArm", new Vector3(0f, 0.32f, 0f), new Vector3(0.08f, 0.04f, 0.22f), accent);

            wheels = new Transform[2];
            wheels[0] = MakeWheel(body, "WheelF", new Vector3(0f, 0.28f, 0.48f));
            wheels[1] = MakeWheel(body, "WheelB", new Vector3(0f, 0.28f, -0.42f));

            // 라이더
            if (riderTex != null)
            {
                var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad.name = "Rider"; quad.transform.SetParent(body, false);
                CoastEditUtil.DestroyCollider(quad);
                float h = height * 0.72f;
                float w = h * riderTex.width / (float)Mathf.Max(1, riderTex.height) * 0.85f;
                quad.transform.localScale = new Vector3(w, h, 1f);
                quad.transform.localPosition = new Vector3(0f, 0.55f + h * 0.35f, -0.05f);
                var shader = CoastMaterials.Require("CoastRun/ChromaUnlit", "CoastRun/UnlitCurved", "Universal Render Pipeline/Unlit", "Sprites/Default");
                var mat = CoastMaterials.NewMat(shader);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", riderTex); else mat.mainTexture = riderTex;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_KeyColor")) mat.SetColor("_KeyColor", new Color(1f, 0f, 1f, 1f));
                if (mat.HasProperty("_OutlineOn")) { mat.SetFloat("_OutlineOn", 1f); mat.SetColor("_OutlineColor", new Color(0.06f, 0.05f, 0.10f, 1f)); mat.SetFloat("_OutlineWidth", 5f); }
                var mr = quad.GetComponent<Renderer>();
                mr.sharedMaterial = mat; mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; mr.receiveShadows = false;
                // 빌보드는 쓰지 않음 — 자전거와 같이 길을 따라 바라봄
            }
            else
            {
                Box(body, "LegL", new Vector3(-0.08f, 0.55f, -0.08f), new Vector3(0.10f, 0.42f, 0.12f), pants);
                Box(body, "LegR", new Vector3(0.08f, 0.55f, 0.05f), new Vector3(0.10f, 0.38f, 0.12f), pants);
                var torso = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                torso.name = "Torso"; torso.transform.SetParent(body, false);
                torso.transform.localPosition = new Vector3(0f, 1.05f, -0.05f);
                torso.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);
                torso.transform.localScale = new Vector3(0.32f, 0.28f, 0.24f);
                CoastEditUtil.DestroyCollider(torso);
                torso.GetComponent<Renderer>().sharedMaterial = CoastMaterials.CreateLit(shirt, 0.15f);
                Box(body, "ArmL", new Vector3(-0.18f, 1.05f, 0.18f), new Vector3(0.08f, 0.08f, 0.36f), skin);
                Box(body, "ArmR", new Vector3(0.18f, 1.05f, 0.18f), new Vector3(0.08f, 0.08f, 0.36f), skin);
                var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                head.name = "Head"; head.transform.SetParent(body, false);
                head.transform.localPosition = new Vector3(0f, 1.42f, 0f);
                head.transform.localScale = Vector3.one * 0.28f;
                CoastEditUtil.DestroyCollider(head);
                head.GetComponent<Renderer>().sharedMaterial = skin;
            }
            return body;
        }

        private static Transform MakeWheel(Transform parent, string name, Vector3 pos)
        {
            var hub = new GameObject(name).transform;
            hub.SetParent(parent, false);
            hub.localPosition = pos;
            // 옆에서 보이는 원판(바퀴 면) — X축으로 세워 진행방향에 맞춤
            var disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Disc"; disc.transform.SetParent(hub, false);
            CoastEditUtil.DestroyCollider(disc);
            disc.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            disc.transform.localScale = new Vector3(0.56f, 0.04f, 0.56f);
            var dark = CoastMaterials.CreateLit(new Color(0.1f, 0.1f, 0.12f), 0.15f);
            if (_wheelTex != null)
            {
                var shader = CoastMaterials.Require("CoastRun/ChromaUnlit", "Universal Render Pipeline/Unlit", "Sprites/Default", "Unlit/Texture");
                var mat = CoastMaterials.NewMat(shader);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", _wheelTex); else mat.mainTexture = _wheelTex;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                if (mat.HasProperty("_KeyColor")) mat.SetColor("_KeyColor", new Color(1f, 0f, 1f, 1f));
                if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", 0.1f);
                disc.GetComponent<Renderer>().sharedMaterial = mat;
            }
            else disc.GetComponent<Renderer>().sharedMaterial = dark;
            // 타이어 링(테두리 느낌)
            var tire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tire.name = "Tire"; tire.transform.SetParent(hub, false);
            CoastEditUtil.DestroyCollider(tire);
            tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            tire.transform.localScale = new Vector3(0.62f, 0.055f, 0.62f);
            var tireMat = CoastMaterials.CreateLit(new Color(0.08f, 0.08f, 0.09f), 0.1f);
            tire.GetComponent<Renderer>().sharedMaterial = tireMat;
            // 디스크가 타이어 안쪽에 보이게
            disc.transform.localScale = new Vector3(0.50f, 0.035f, 0.50f);
            return hub;
        }

        private static void Box(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent, false);
            CoastEditUtil.DestroyCollider(go);
            go.transform.localPosition = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void MakeTag(Rival r)
        {
            var go = new GameObject("Tag", typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(r.root, false);
            var cv = go.GetComponent<Canvas>(); cv.renderMode = RenderMode.WorldSpace; cv.sortingOrder = 20;
            var rt = go.GetComponent<RectTransform>(); rt.sizeDelta = new Vector2(160f, 44f); rt.localScale = Vector3.one * 0.006f;
            rt.localPosition = new Vector3(0f, r.baseH + 0.28f, 0f);
            var pill = CoastUiArt.GlossyPill(rt, "Pill", r.col, 22, 6); pill.raycastTarget = false;
            pill.rectTransform.anchorMin = Vector2.zero; pill.rectTransform.anchorMax = Vector2.one; pill.rectTransform.offsetMin = Vector2.zero; pill.rectTransform.offsetMax = Vector2.zero;
            var t = CoastHudLayout.MakeText(pill.rectTransform, "T", r.name, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 3f), Vector2.zero);
            t.color = Color.white; t.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(t, new Color(0f, 0f, 0f, 0.45f), 1.5f);
            go.AddComponent<YawBillboard>();
        }

        private void Update()
        {
            if (_player == null) { Destroy(gameObject); return; }
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            bool finished = _player.State == SkateState.Finish;
            float pd = _player.PathDistance;
            float ps = Mathf.Max(2f, _player.Speed * _player.SpeedBoost);
            for (int i = 0; i < _rivals.Length; i++)
            {
                var r = _rivals[i]; if (r == null) continue;
                r.moodT -= dt;
                if (r.moodT <= 0f) { r.mood = (float)_rng.NextDouble() * 2f - 1f; r.moodT = 3f + (float)_rng.NextDouble() * 3f; }
                float rel = r.dist - pd;
                float want = 1f + r.mood * 0.10f;
                if (rel > 15f) want = 0.78f; else if (rel > 9f) want = Mathf.Min(want, 0.94f);
                if (rel < -10f) want = 1.28f; else if (rel < -5f) want = Mathf.Max(want, 1.08f);
                if (finished) want = 0.6f;
                // 86차(사용자): 장애물을 **스스로 피한다** — 앞 6~14 m 같은 레인에 장애물이 있으면 빈 옆 레인으로, 낮은 것은 가끔 점프.
                //   피하지 못하고 겹치면 **주인공처럼 꽈당**(속도 뚝·비틀·붉게 번쩍·파편·「꽈당!」·효과음).
                r.lookCool -= dt; r.hitCool -= dt;
                if (r.stun > 0f) { r.stun -= dt; }
                if (r.lookCool <= 0f && r.stun <= 0f)
                {
                    r.lookCool = 0.18f;
                    var ahead = ObstacleAhead(r, r.lane, 2.5f, 13f);
                    if (ahead != null && Mathf.Abs(r.laneTarget - r.lateral) < 0.2f)
                    {
                        float top = HazardTop(ahead);
                        int side = _rng.Next(2) == 0 ? -1 : 1;
                        int best = 0; float bestFree = -1f;
                        for (int t = 0; t < 2; t++)
                        {
                            int nl = r.lane + (t == 0 ? side : -side);
                            if (nl < -1 || nl > 1) continue;
                            if (Mathf.Abs(rel) < 4f && nl == _player.Lane) continue;
                            bool taken = false;
                            for (int j = 0; j < _rivals.Length; j++) if (j != i && _rivals[j] != null && _rivals[j].lane == nl && Mathf.Abs(_rivals[j].dist - r.dist) < 3f) taken = true;
                            if (taken) continue;
                            float free = FreeAhead(r, nl, 1f, 14f);
                            if (free > bestFree) { bestFree = free; best = nl; }
                        }
                        double roll = _rng.NextDouble();
                        if (bestFree >= 6f && roll < 0.82)
                        { r.lane = best; r.laneTarget = best * _laneOffset; r.nextLane = 2.5f + (float)_rng.NextDouble() * 4.5f; }
                        else if (top < ObstacleHazard.BounceHeight && r.hop <= 0.001f && roll < 0.95)
                        { r.hopVel = 5.2f; r.nextHop = 5f + (float)_rng.NextDouble() * 7f; }
                        // else: 못 피함 → 아래 충돌 판정으로
                    }
                }
                if (r.hitCool <= 0f && r.hop < 0.5f)
                {
                    var hit = ObstacleAhead(r, r.lane, -0.6f, 0.9f, true);
                    if (hit != null && hit != r.lastHit) HitReact(r, hit, Mathf.Abs(rel));
                }
                if (r.stun > 0f) want = Mathf.Min(want, 0.42f);
                r.speedMul = Mathf.MoveTowards(r.speedMul, want, dt * (r.stun > 0f ? 4f : 0.6f));
                r.dist += ps * r.speedMul * dt;
                r.nextLane -= dt;
                if (r.nextLane <= 0f && r.stun <= 0f)
                {
                    r.nextLane = 2.5f + (float)_rng.NextDouble() * 4.5f;
                    int nl = Mathf.Clamp(r.lane + (_rng.Next(2) == 0 ? -1 : 1), -1, 1);
                    bool near = Mathf.Abs(rel) < 4f;
                    if (!(near && nl == _player.Lane) && nl != r.lane)
                    {
                        bool taken = false;
                        for (int j = 0; j < _rivals.Length; j++) if (j != i && _rivals[j] != null && _rivals[j].lane == nl && Mathf.Abs(_rivals[j].dist - r.dist) < 3f) taken = true;
                        if (!taken && FreeAhead(r, nl, 1f, 10f) >= 8f) { r.lane = nl; r.laneTarget = nl * _laneOffset; }   // 86차: 장애물 있는 레인으로는 안 옮긴다
                    }
                }
                r.lateral = Mathf.MoveTowards(r.lateral, r.laneTarget, dt * 7f);
                // 자전거는 점프 대신 가벼운 언덕 바운스만
                r.nextHop -= dt;
                if (r.nextHop <= 0f && r.hop <= 0.001f) { r.nextHop = 5f + (float)_rng.NextDouble() * 7f; r.hopVel = 2.8f; }
                if (r.hop > 0f || r.hopVel > 0f)
                {
                    r.hopVel -= 18f * dt; r.hop += r.hopVel * dt;
                    if (r.hop <= 0f) { r.hop = 0f; r.hopVel = 0f; }
                }
                r.phase += dt * (8f + 4f * r.speedMul);
                float bob = r.hop > 0f ? 0f : Mathf.Abs(Mathf.Sin(r.phase * 0.5f)) * 0.025f;
                r.root.position = DownhillPath.Point(r.dist, r.lateral, r.hop + bob);
                r.root.rotation = DownhillPath.Rotation;
                if (r.body != null)
                {
                    float lean = Mathf.Clamp((r.laneTarget - r.lateral) * 6f, -12f, 12f);
                    float wob = 0f;
                    if (r.wobble > 0f) { r.wobble -= dt; wob = Mathf.Sin(r.wobble * 28f) * 14f * Mathf.Clamp01(r.wobble / 0.7f); }
                    r.body.localRotation = Quaternion.Euler(r.stun > 0f ? -10f * Mathf.Clamp01(r.stun) : 0f, 0f, lean + wob);
                    // 붉게 번쩍(피격 0.35 s)
                    if (r.rends != null && r.hitCool > 0.9f)
                    {
                        float k = Mathf.Clamp01((r.hitCool - 1.05f) / 0.35f);   // 1.4→1.05 동안 붉게, 그 뒤 원색
                        for (int q = 0; q < r.rends.Length; q++) { var m = r.rends[q].material; if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.Lerp(r.rendCols[q], new Color(1f, 0.35f, 0.3f), k)); }
                    }
                }
                // 바퀴 회전 — 이동 속도에 비례
                if (r.wheels != null)
                {
                    float spin = ps * r.speedMul * 120f * dt;
                    for (int w = 0; w < r.wheels.Length; w++)
                        if (r.wheels[w] != null) r.wheels[w].Rotate(spin, 0f, 0f, Space.Self);
                }
            }
        }

        // ── 86차: 장애물 인식(길은 직선 — z = 거리, x = 레인 옆거리) ──
        private ObstacleHazard ObstacleAhead(Rival r, int lane, float minAhead, float maxAhead, bool overlapOnly = false)
        {
            float lx = lane * _laneOffset; ObstacleHazard best = null; float bestDz = float.MaxValue;
            var list = ObstacleHazard.Active;
            for (int k = 0; k < list.Count; k++)
            {
                var h = list[k]; if (h == null || !h.isActiveAndEnabled) continue;
                var col = h.GetComponent<Collider>(); if (col == null) continue;
                var b = col.bounds;
                float dz = b.center.z - r.dist;
                if (overlapOnly) { if (r.dist < b.min.z - 0.3f || r.dist > b.max.z + 0.3f) continue; }
                else if (dz < minAhead || dz > maxAhead) continue;
                float halfW = Mathf.Max(0.35f, b.extents.x);
                if (Mathf.Abs(b.center.x - lx) > halfW + 0.45f) continue;
                if (dz < bestDz) { bestDz = dz; best = h; }
            }
            return best;
        }
        private float FreeAhead(Rival r, int lane, float minAhead, float maxAhead)
        {
            var h = ObstacleAhead(r, lane, minAhead, maxAhead);
            if (h == null) return maxAhead;
            var col = h.GetComponent<Collider>();
            return col != null ? col.bounds.min.z - r.dist : 0f;
        }
        private static float HazardTop(ObstacleHazard h)
        {
            float top = 0f;
            foreach (var c in h.GetComponentsInChildren<Collider>(true)) { if (c.GetComponent<NearMissZone>() != null) continue; top = Mathf.Max(top, c.bounds.size.y); }
            return top;
        }
        private void HitReact(Rival r, ObstacleHazard h, float relAbs)
        {
            r.lastHit = h; r.hitCool = 1.4f; r.stun = 0.9f; r.wobble = 0.7f;
            r.hopVel = Mathf.Max(r.hopVel, 1.6f);
            var pos = r.root.position + Vector3.up * 0.6f;
            JuiceDirector.Instance?.PlayRivalHit(pos, Mathf.Clamp01(1f - relAbs / 14f));
        }

        public int PlayerRank()
        {
            if (_player == null) return 1;
            int rank = 1; float pd = _player.PathDistance;
            for (int i = 0; i < _rivals.Length; i++) if (_rivals[i] != null && _rivals[i].dist > pd) rank++;
            return rank;
        }

        public static string RankText()
        {
            if (Instance == null) return "";
            int r = Instance.PlayerRank();
            return Loc.T($"{r}위", r == 1 ? "1st" : r == 2 ? "2nd" : r == 3 ? "3rd" : "4th");
        }
    }
}
