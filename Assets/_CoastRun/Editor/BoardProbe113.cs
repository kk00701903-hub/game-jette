#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace CoastRun
{
    /// 113차 검증용: 그림 보드가 도로에 닿는지(바퀴 접지) · 주인공 발이 데크에 붙는지 숫자로 찍는다.
    /// 게임 뷰 캡쳐가 막힌 환경에서도 원격 `log` 로 확인할 수 있게 만든 프로브.
    public static class BoardProbe113
    {
        [MenuItem("Coast Run/Dev/113 - Board probe")]
        public static void Probe()
        {
            var visual = Object.FindFirstObjectByType<CoastPlayerVisual>();
            if (visual == null) { Debug.LogWarning("[113] CoastPlayerVisual 없음 — 러닝 중에 실행할 것"); return; }

            Transform board = null, quad = null;
            foreach (var t in visual.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Skateboard") board = t;
                if (t.name == "PaintedBoard") quad = t;
            }
            if (quad == null)
            {
                var names = new System.Text.StringBuilder();
                foreach (var t in visual.GetComponentsInChildren<Transform>(true))
                    names.Append(t.name).Append(' ');
                Debug.LogWarning(string.Format("[113] PaintedBoard 없음 — rigAvailable={0} mode={1} tex={2} 자식=[{3}]",
                    SkaterRig.Available, RunTuning.Mode,
                    ArtAssets.LoadTexture("Prop_Skateboard") != null, names.ToString()));
                return;
            }

            var mr = quad.GetComponent<Renderer>();
            var b = mr.bounds;
            float roadY = visual.transform.position.y - GetHalfHeight(visual);

            float footY = float.NaN;
            var anim = visual.GetComponentInChildren<Animator>();
            if (anim != null && anim.avatar != null && anim.avatar.isHuman)
            {
                var l = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                var r = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                if (l != null && r != null) footY = (l.position.y + r.position.y) * 0.5f;
            }

            float deckTopY = b.min.y + b.size.y * (0.693f - 0.013f) / (1f - 0.013f);
            Debug.LogWarning(string.Format(
                "[113] road={0:F3} boardBottom={1:F3} (접지오차 {2:+0.000;-0.000}) boardTop={3:F3} size={4:F3}x{5:F3} " +
                "deckTop={6:F3} foot={7:F3} (발-데크 {8:+0.000;-0.000}) boardLocal={9} quadLocal={10}",
                roadY, b.min.y, b.min.y - roadY, b.max.y, b.size.x, b.size.y,
                deckTopY, footY, footY - deckTopY,
                board != null ? board.localPosition.ToString("F3") : "-",
                quad.localPosition.ToString("F3")));
        }


        [MenuItem("Coast Run/Dev/113 - Force board visual")]
        public static void ForceBoard()
        {
            RunTuning.Mode = RunMode.Skateboard;
            var visual = Object.FindFirstObjectByType<CoastPlayerVisual>();
            if (visual == null) { Debug.LogWarning("[113] CoastPlayerVisual 없음 — 러닝 중에 실행할 것"); return; }
            visual.Build();
            Debug.LogWarning("[113] 보드 비주얼로 재빌드 (mode=Skateboard)");
            Probe();
        }

        static float GetHalfHeight(CoastPlayerVisual v)
        {
            var pc = v.GetComponent<PlayerController>();
            return pc != null ? pc.BodyHalfHeight : 0.8f;
        }
    }
}
#endif
