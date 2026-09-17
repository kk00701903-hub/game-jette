using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 105차(재미요소 P1-4, 스타듀 달력): 52주 한 장 — 계절 4줄 × 13칸. 칸마다 대회(▶)·컷씬(★)·이벤트(·)·축제(♪) 표시, 지금 주는 분홍 테두리.
    ///   육성 화면 주차 알약을 누르면 열린다. 아래에 「다음 일정」 3줄.
    public static class CalendarUI
    {
        private static Canvas _canvas;
        public static bool IsOpen => _canvas != null;

        public static void Open(SaveData s, Action onClose = null)
        {
            Close();
            if (s == null) { onClose?.Invoke(); return; }
            var crt = EventCardKit.Card("CalendarCanvas", 468, new Vector2(660f, 1000f), out _canvas, 0f);
            EventCardKit.JellyTitle(crt, Loc.T("1년 달력", "The Year"), new Color(0.45f, 0.35f, 0.95f), new Color(0.20f, 0.12f, 0.45f), 30f, 80f, 44);
            var sub = CoastHudLayout.MakeText(crt, "Sub", Loc.T($"지금 {s.week}주차 · {Timeline.SeasonName(Timeline.SeasonOf(s.week))} · {s.chapter}장", $"Week {s.week} · {Timeline.SeasonName(Timeline.SeasonOf(s.week))} · Ch.{s.chapter}"), 18, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -140f), new Vector2(-20f, -112f));
            sub.color = new Color(0.90f, 0.32f, 0.45f); sub.fontStyle = FontStyle.Bold;

            // 범례
            var legend = CoastHudLayout.MakeText(crt, "Legend", Loc.T("▶ 대회   ★ 컷씬   · 이야기   ♪ 축제   ▣ 지금", "▶ contest  ★ cutscene  · story  ♪ festival  ▣ now"), 15, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -172f), new Vector2(-20f, -146f));
            legend.color = new Color(0.40f, 0.35f, 0.45f);

            Color[] seasonCol = { new Color(0.95f, 0.80f, 0.90f), new Color(0.75f, 0.88f, 0.98f), new Color(0.98f, 0.85f, 0.65f), new Color(0.85f, 0.87f, 0.95f) };
            float top = 190f, cell = 42f, gap = 4f, left = 74f;
            for (int season = 0; season < 4; season++)
            {
                float rowY = top + season * (cell * 2f + 22f);
                var lab = CoastHudLayout.MakeText(crt, "S" + season, Timeline.SeasonName((SeasonKind)season), 18, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(22f, -rowY - cell * 2f), new Vector2(left - 4f, -rowY));
                lab.color = new Color(0.35f, 0.25f, 0.45f); lab.fontStyle = FontStyle.Bold;
                for (int i = 0; i < 13; i++)
                {
                    int week = season * 13 + i + 1;
                    int chapter = Timeline.ChapterOf(week);
                    bool last = week == Timeline.WeekEnd(chapter);
                    bool now = week == s.week;
                    string mark = "";
                    if (last && StoryProgress.IsRunChapter(chapter)) mark = "▶";
                    else if (last && StoryProgress.CutsceneIndex(chapter) > 0) mark = "★";
                    else if (last && StoryProgress.EventIndex(chapter) > 0) mark = "·";
                    var fest = Festival.AtWeek(week);
                    if (fest != null) mark = "♪";
                    float x = left + i * (cell + gap);
                    Color bg = week < s.week ? new Color(0.88f, 0.86f, 0.84f) : seasonCol[season];
                    var box = CoastUiArt.Panel(crt, "W" + week, bg, 8); box.raycastTarget = false;
                    var br = box.rectTransform; br.anchorMin = br.anchorMax = new Vector2(0f, 1f); br.pivot = new Vector2(0f, 1f);
                    br.anchoredPosition = new Vector2(x, -rowY); br.sizeDelta = new Vector2(cell, cell * 2f);
                    if (now)
                    {
                        var edge = CoastUiArt.Panel(crt, "Now", new Color(0.93f, 0.22f, 0.52f), 10); edge.raycastTarget = false;
                        var er = edge.rectTransform; er.anchorMin = er.anchorMax = new Vector2(0f, 1f); er.pivot = new Vector2(0f, 1f);
                        er.anchoredPosition = new Vector2(x - 3f, -rowY + 3f); er.sizeDelta = new Vector2(cell + 6f, cell * 2f + 6f);
                        edge.transform.SetSiblingIndex(box.transform.GetSiblingIndex());
                    }
                    var wt = CoastHudLayout.MakeText(br, "N", week.ToString(), 13, TextAnchor.UpperCenter, Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(0f, -4f));
                    wt.color = new Color(0.30f, 0.28f, 0.36f);
                    if (mark.Length > 0)
                    {
                        var mt = CoastHudLayout.MakeText(br, "M", mark, mark == "·" ? 30 : 20, TextAnchor.LowerCenter, Vector2.zero, Vector2.one, new Vector2(0f, 4f), new Vector2(0f, 0f));
                        mt.color = mark == "▶" ? new Color(0.93f, 0.22f, 0.52f) : mark == "★" ? new Color(0.95f, 0.60f, 0.10f) : mark == "♪" ? new Color(0.30f, 0.60f, 0.30f) : new Color(0.45f, 0.40f, 0.55f);
                        mt.fontStyle = FontStyle.Bold;
                    }
                }
            }

            // 다음 일정 3줄
            float ly = top + 4 * (cell * 2f + 22f) + 6f;
            var box2 = EventCardKit.InfoBox(crt, ly, 200f);
            int n = 0;
            var nc = RaisingFun.NextContest(s);
            if (nc != null) { EventCardKit.IconRow(box2, "Icon_Speed", new Color(1f, 0.75f, 0.80f), Loc.T($"{RaisingFun.WeeksUntil(s, nc)}주 뒤 대회 「{nc.Name}」 · {nc.GoalText} · 추천 {RaisingFun.StatName(RaisingFun.RecommendedStat(nc))} {RaisingFun.RecommendedValue(nc)}", $"Contest in {RaisingFun.WeeksUntil(s, nc)}w: {nc.Name} · {nc.GoalText}"), 12f + n * 60f, 52f, 15, null, null, 16f, 12f); n++; }
            var nf = Festival.Next(s.week);
            if (nf != null) { EventCardKit.IconRow(box2, "Icon_Star", new Color(0.75f, 0.95f, 0.75f), Loc.T($"{Mathf.Max(0, nf.week - s.week)}주 뒤 축제 「{nf.Name}」 · {RaisingFun.StatName(nf.stat)} {s.stats.Get(nf.stat)}/{nf.target}", $"Festival in {Mathf.Max(0, nf.week - s.week)}w: {nf.Name} · {RaisingFun.StatName(nf.stat)} {s.stats.Get(nf.stat)}/{nf.target}"), 12f + n * 60f, 52f, 15, null, null, 16f, 12f); n++; }
            int nextCut = 0; foreach (var cc in StoryProgress.CutsceneChapters) if (cc >= s.chapter && !(cc == s.chapter && StoryProgress.ChapterRead(cc))) { nextCut = cc; break; }
            if (nextCut > 0) { EventCardKit.IconRow(box2, "Icon_Card", new Color(1f, 0.90f, 0.60f), Loc.T($"컷씬 {StoryProgress.CutsceneIndex(nextCut)} 은 {nextCut}장 끝({Timeline.WeekEnd(nextCut)}주차)에", $"Cutscene {StoryProgress.CutsceneIndex(nextCut)} at end of ch.{nextCut} (week {Timeline.WeekEnd(nextCut)})"), 12f + n * 60f, 52f, 15, null, null, 16f, 12f); n++; }
            string pend = ClueSystem.PendingSummary(s);
            if (pend != null && n < 3) { EventCardKit.IconRow(box2, "Icon_Bulb", new Color(0.80f, 0.88f, 1f), pend, 12f + n * 60f, 52f, 14, null, null, 16f, 12f); n++; }
            for (int i = 0; i < 4; i++) { int p = Festival.Place(s, i); if (p > 0 && n < 3) { EventCardKit.IconRow(box2, "Icon_Coin", new Color(1f, 0.85f, 0.45f), Loc.T($"{Festival.All[i].Name} {Festival.PlaceName(p)}", $"{Festival.All[i].Name} {Festival.PlaceName(p)}"), 12f + n * 60f, 52f, 15, null, null, 16f, 12f); n++; } }

            EventCardKit.IconButton(crt, "Close", "Icon_Arrow", Loc.T("닫기", "Close"), new Color(0.60f, 0.62f, 0.70f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(300f, 70f), () => { Close(); onClose?.Invoke(); }, 26);
        }

        public static void Close() { if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject); _canvas = null; }
    }
}
