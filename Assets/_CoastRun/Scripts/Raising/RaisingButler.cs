using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 23차-7: 집사 꼬마 — 프린세스 메이커의 집사처럼, 기억을 잃은 주인공(과 플레이어)에게 지금 뭘 하면 좋은지 알려준다.
    /// 방 왼쪽 아래에 서 있고, 말풍선에 상황별 조언. 탭하면 다음 조언. 상태를 읽어 우선순위대로 말한다.
    public class RaisingButler
    {
        private readonly RectTransform _root;
        private readonly Image _body;
        private readonly Text _bubble;
        private readonly RectTransform _bubbleRt;
        private readonly List<string> _lines = new List<string>();
        private int _idx;
        private float _bob;
        public bool Visible => _root != null && _root.gameObject.activeSelf;

        public RaisingButler(RectTransform host, System.Action onTapped)
        {
            _root = new GameObject("Butler", typeof(RectTransform)).GetComponent<RectTransform>();
            _root.SetParent(host, false);
            _root.anchorMin = _root.anchorMax = new Vector2(0f, 0f); _root.pivot = new Vector2(0f, 0f);
            _root.anchoredPosition = new Vector2(8f, 14f); _root.sizeDelta = new Vector2(190f, 300f);

            var shadow = CoastHudLayout.MakeImage(_root, "Shadow", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-60f, -4f), new Vector2(60f, 14f), new Color(0f, 0f, 0f, 0.2f));
            shadow.sprite = CoastUiArt.RoundedRect(30); shadow.type = Image.Type.Sliced; shadow.raycastTarget = false;

            var go = new GameObject("Body", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(_root, false);
            _body = go.GetComponent<Image>();
            var tex = ArtAssets.LoadTexture("UI_Butler_Boy");
            if (tex != null) _body.sprite = CoastUiArt.AsSprite(tex); else _body.color = new Color(0.2f, 0.2f, 0.3f);
            _body.preserveAspect = true;
            var brt = _body.rectTransform; brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero; brt.pivot = new Vector2(0.5f, 0f);
            var btn = go.GetComponent<Button>(); btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => { Next(); onTapped?.Invoke(); });

            // 말풍선(오른쪽 위로), 꼬리
            var pill = CoastUiArt.CutePill(_root, "Bubble", new Color(1f, 0.99f, 0.95f), 18, 3);
            _bubbleRt = pill.rectTransform;
            _bubbleRt.anchorMin = _bubbleRt.anchorMax = new Vector2(1f, 1f); _bubbleRt.pivot = new Vector2(0f, 0f);
            _bubbleRt.anchoredPosition = new Vector2(-40f, -80f); _bubbleRt.sizeDelta = new Vector2(400f, 96f);
            pill.raycastTarget = false;
            var tail = CoastUiArt.Panel(_bubbleRt, "Tail", new Color(1f, 0.99f, 0.95f), 6);
            var trt = tail.rectTransform; trt.anchorMin = trt.anchorMax = new Vector2(0f, 0f); trt.pivot = new Vector2(0.5f, 0.5f);
            trt.anchoredPosition = new Vector2(18f, -2f); trt.sizeDelta = new Vector2(26f, 26f); trt.localRotation = Quaternion.Euler(0f, 0f, 45f); tail.raycastTarget = false;
            var tag = CoastHudLayout.MakeText(_bubbleRt, "Tag", Loc.T("집사 도담", "Dodam"), 14, TextAnchor.UpperLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -24f), new Vector2(-12f, -6f));
            tag.color = new Color(0.85f, 0.4f, 0.3f); tag.fontStyle = FontStyle.Bold; tag.raycastTarget = false;
            _bubble = CoastHudLayout.MakeText(_bubbleRt, "T", "", 19, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(16f, 8f), new Vector2(-12f, -24f));
            _bubble.color = new Color(0.25f, 0.15f, 0.12f); _bubble.horizontalOverflow = HorizontalWrapMode.Wrap; _bubble.raycastTarget = false;
            _bubble.resizeTextForBestFit = true; _bubble.resizeTextMinSize = 14; _bubble.resizeTextMaxSize = 19;
        }

        public void SetVisible(bool on) { _root.gameObject.SetActive(on); }

        /// 상태를 읽어 조언 목록을 다시 만든다(우선순위 순). 처음 문장은 바로 보여준다.
        public void Refresh(SaveData save, bool firstVisit)
        {
            _lines.Clear();
            if (save == null) { _lines.Add(Loc.T("아가씨, 오늘도 제가 옆에 있겠습니다.", "Miss, I'm right here today too.")); Show(0); return; }
            var st = save.stats;
            int week = save.week;
            if (firstVisit)
                _lines.Add(Loc.T("아가씨, 기억은 없어도 몸은 기억합니다. 아래 밥·놀기·알바를 눌러 카드를 골라 보세요.", "Miss, your body remembers. Tap Feed, Play or Work and pick a card."));
            // 74차: 스트레스 구간(0~100) 기준 — 번아웃은 「놀기 한 주」를 권한다(밥 한 칸으로는 못 푼다).
            if (st.Burnout)
                _lines.Add(Loc.T($"번아웃입니다(스트레스 {st.stress}/{st.StressLimit}). 이번 주는 놀기로 푸시는 게 좋겠습니다 — 오름 산책·바다 수영.",
                                  $"Burnout (stress {st.stress}/{st.StressLimit}). Spend this week playing — a walk or a swim."));
            else if (st.Stage >= StressStage.Worn)
                _lines.Add(Loc.T($"스트레스가 {st.stress}까지 왔습니다. 놀기(산책·수영)나 쓰다듬기로 기운을 돌려 주세요.", $"Stress is at {st.stress}. Play or pet Haneul to recover."));
            int need = StoryGate.Required(save);
            int have = StoryGate.Stamina(save);
            bool gateSoon = week >= Timeline.WeekEnd(save.chapter);
            if (have < need)
            {
                if (gateSoon)
                    _lines.Add(Loc.T($"이번 주가 게이트입니다. 체력 {have}/{need} — 밥·연습으로 체력을 올리셔야 달립니다.", $"Gate week. Stamina {have}/{need} — Feed/Play to raise it or you can't run."));
                else
                    _lines.Add(Loc.T($"스토리 게이트까지 체력 {need - have}이 모자랍니다. 목표 리본을 보며 밥·놀기를 고르세요.", $"Need {need - have} more stamina. Use the goal ribbon when picking Feed/Play."));
            }
            if (st.money < 60)
                _lines.Add(Loc.T("지갑이 가볍습니다. 알바 카드를 골라 두면 장보기·교육비를 댑니다.", "Purse is light. Pick a Work card for shopping and lessons."));
            var warn = Survival.Warning(save);
            if (warn != null)
                _lines.Add(Loc.T("생활이 위험합니다. 장보기부터 하세요 — " + warn, "Survival risk — open Shop. " + warn));
            int lowest = Mathf.Min(st.stamina, Mathf.Min(st.agility, st.charm));
            string low = lowest == st.stamina ? Loc.T("체력", "stamina") : lowest == st.agility ? Loc.T("순발력", "agility") : Loc.T("매력", "charm");
            _lines.Add(Loc.T($"지금 가장 낮은 건 {low}입니다. 놀기에서 교육 카드가 뜨면 골라 보세요.", $"{low} is lowest. If a Lesson card appears under Play, take it."));
            var rec = save.CurrentChapter;
            if (rec != null)
            {
                int target = rec.heartsTarget > 0 ? rec.heartsTarget : ChapterGrading.HeartTarget(save.chapter);
                int sCut = Mathf.CeilToInt(target * ChapterGrading.S_Ratio);
                int left = sCut - save.chapterHearts;
                if (left > 0) _lines.Add(Loc.T($"S컷까지 하트 {left}개. 대회와 대성공으로 모으세요.", $"{left} hearts to S-cut. Contests and great successes help."));
            }
            _lines.Add(Loc.T("자동을 켜면 제가 밥·알바를 규칙대로 돌립니다. 카드 고르기는 직접 하실 때 더 재밌습니다.", "Auto follows simple rules. Picking cards yourself is more fun."));
            _lines.Add(Loc.T("송전탑은 여전히 창밖에 있습니다. 약속한 생일까지, 제가 세어 두겠습니다.", "The tower is still out the window. I'll count the days to the promised birthday."));
            _idx = 0;
            Show(0);
        }

        private void Next()
        {
            if (_lines.Count == 0) return;
            _idx = (_idx + 1) % _lines.Count;
            Show(_idx);
            _bob = 1f;
        }

        private void Show(int i)
        {
            if (_lines.Count == 0) { _bubble.text = ""; return; }
            _bubble.text = _lines[Mathf.Clamp(i, 0, _lines.Count - 1)];
        }

        /// 매 프레임: 살짝 숨 쉬고, 탭하면 콩 뛴다.
        public void Tick(float dt)
        {
            if (!Visible) return;
            float t = Time.unscaledTime;
            float breathe = 1f + Mathf.Sin(t * 2.1f) * 0.012f;
            _bob = Mathf.MoveTowards(_bob, 0f, dt * 3f);
            float hop = Mathf.Sin(Mathf.Clamp01(_bob) * Mathf.PI) * 18f;
            _body.rectTransform.localScale = new Vector3(1f / breathe, breathe, 1f);
            _body.rectTransform.anchoredPosition = new Vector2(0f, hop);
            _bubbleRt.localScale = Vector3.one * (1f + Mathf.Sin(t * 2.1f + 1f) * 0.006f);
        }
    }
}
