using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 85차(대본 v4 「분기 조건」): 플레이 중 모이는 **단서 여섯 개**(SaveData.clueMask) → 엔딩 A/B/TRUE.
    ///   이름(CS7 카드) · 편지 스무 통(EV9) · 하트 흔적(CS2 부표 페인트) · 보석 머리띠(CS8 방수 통) · 돌 세 개(CS4 뒤 다시 쌓기) · 91.9 라디오(CS3 복원).
    ///   컷씬은 항상 재생되므로 단서는 **컷씬 직후 육성 화면의 「단서」 카드**에서 확정한다. 편지·돌은 카드에서 한 번 더 손을 대야(두 번째 버튼) 얻는다 —
    ///   그냥 넘기면 빠진다(놓친 단서는 시네마 다시보기로는 안 채워진다: 스토리 진행 중 카드에서만).
    ///   END_A 「엇갈린 정류장」: 이름 없음 또는 라디오 없음 · END_B 「우유 두 병」: 이름+라디오는 있으나 나머지 중 하나라도 없음 · END_TRUE: 여섯 개 전부.
    public static class ClueSystem
    {
        [Flags]
        public enum Clue { None = 0, Name = 1, Letters = 2, Heart = 4, Headband = 8, Stones = 16, Radio = 32, All = 63 }

        public static bool Has(SaveData s, Clue c) => s != null && (s.clueMask & (int)c) != 0;
        public static int Count(SaveData s) { if (s == null) return 0; int n = 0; for (int i = 0; i < 6; i++) if ((s.clueMask & (1 << i)) != 0) n++; return n; }

        /// 씬 id("CS2", "EV9" …) 가 끝났을 때 주는 단서. 없으면 None.
        public static Clue ClueFor(string sceneId)
        {
            switch (sceneId)
            {
                case "CS2": return Clue.Heart;
                case "CS3": return Clue.Radio;
                case "CS4": return Clue.Stones;
                case "CS7": return Clue.Name;
                case "CS8": return Clue.Headband;
                case "EV9": return Clue.Letters;
                default: return Clue.None;
            }
        }

        /// 엔딩 시네마 id — v4 분기 조건.
        public static string EndingId(int mask)
        {
            bool name = (mask & (int)Clue.Name) != 0, radio = (mask & (int)Clue.Radio) != 0;
            if (!name || !radio) return "END_A";
            return (mask & (int)Clue.All) == (int)Clue.All ? "END_TRUE" : "END_B";
        }

        public static string Name(Clue c)
        {
            switch (c)
            {
                case Clue.Name: return Loc.T("이름", "The name");
                case Clue.Letters: return Loc.T("편지 스무 통", "Twenty letters");
                case Clue.Heart: return Loc.T("하트 흔적", "The heart mark");
                case Clue.Headband: return Loc.T("보석 머리띠", "The gem headband");
                case Clue.Stones: return Loc.T("돌 세 개", "Three stones");
                case Clue.Radio: return Loc.T("91.9 라디오", "Radio 91.9");
                default: return "";
            }
        }
        /// 카드 아이콘(Resources 그림 이름 — 없으면 색 동그라미만).
        public static string Icon(Clue c)
        {
            switch (c)
            {
                case Clue.Name: return "Icon_Camera"; case Clue.Letters: return "Icon_Book"; case Clue.Heart: return "Icon_Star";
                case Clue.Headband: return "Icon_Coin"; case Clue.Stones: return "Icon_Tower"; case Clue.Radio: return "Icon_Bulb"; default: return "Icon_Star";
            }
        }
        /// 카드 본문 — 왜 이 단서인지 한 줄.
        private static string Body(Clue c)
        {
            switch (c)
            {
                case Clue.Heart: return Loc.T("부표의 삐뚤어진 하트 — 아빠 손으로 그린 것과 같은 페인트 자국을 기억해 두었다.", "The crooked heart on the buoy — the same paint mark Dad used to make.");
                case Clue.Radio: return Loc.T("고장 난 라디오를 고쳤다. 다이얼은 91.9에 멈춰 있었다.", "The broken radio works again. The dial rests at 91.9.");
                case Clue.Stones: return Loc.T("탑 아래 돌 세 개가 무너져 있다. 다시 쌓을까, 그냥 둘까.", "The three stones under the tower have fallen. Stack them again — or leave them.");
                case Clue.Name: return Loc.T("코팅된 수색 카드 — 「고하늘 실종 1년」. 내 이름이 거기 있었다.", "A laminated search card — 'Go Haneul, missing one year'. My name was there.");
                case Clue.Headband: return Loc.T("방수 통 속 푸른 보석 머리띠. 아빠가 끝까지 지킨 것.", "The blue gem headband inside the waterproof case. What Dad kept to the end.");
                case Clue.Letters: return Loc.T("편지 스무 통. 전부 읽으면 밤이 새겠지만, 한 통도 남기고 싶지 않다.", "Twenty letters. Reading them all will take the night, but I don't want to leave one unread.");
                default: return "";
            }
        }

        /// 단서 카드 — sceneId 컷씬이 끝난 뒤 육성 화면에서. 단서가 없거나 이미 가진 것이면 바로 onDone.
        ///   편지·돌은 두 번째 버튼(다시 쌓기 / 전부 읽기)을 눌러야 얻는다.
        /// 단서 카드 — sceneId 컷씬이 끝난 뒤 육성 화면에서. 단서가 없거나 이미 가진 것이면 바로 onDone.
        ///   편지·돌은 행동을 골라야 얻는다. 편지는 발췌 2~3장을 넘긴 뒤 「전부 읽기 / 덮기」.
        public static void ShowAfterScene(SaveData save, string sceneId, Action onDone)
        {
            var c = ClueFor(sceneId);
            if (save == null || c == Clue.None || Has(save, c)) { onDone?.Invoke(); return; }
            if (c == Clue.Letters) { ShowLetterReading(save, onDone); return; }
            bool choice = c == Clue.Stones;
            Show(save, c, choice, onDone);
        }

        /// EV9: 편지 발췌를 넘긴 뒤 「전부 읽기 / 덮기」 선택.
        private static void ShowLetterReading(SaveData save, Action onDone)
        {
            string[] pages =
            {
                Loc.T("「…스무 살 생일, 송전탑 아래. 네가 오든 안 오든 나는 갈게.」\n글씨 끝이 물에 번져 있다.",
                    "\"…20th birthday, under the tower. Whether you come or not, I'll go.\"\nThe ink runs at the edge."),
                Loc.T("「우유 두 병. 하나는 오늘, 하나는 내일.」\n같은 문장이 해마다 조금씩 달라진다.",
                    "\"Two bottles of milk. One for today, one for tomorrow.\"\nThe same line, shifted a little each year."),
                Loc.T("받는 사람 칸 — 「하늘」. 스무 통 모두 같은 이름.\n마지막 통만 봉하지 않은 채다.",
                    "The name on every envelope — \"Haneul\".\nOnly the last one is still unsealed."),
            };
            ShowLetterPage(save, pages, 0, onDone);
        }

        private static void ShowLetterPage(SaveData save, string[] pages, int index, Action onDone)
        {
            Close();
            bool last = index >= pages.Length - 1;
            var card = EventCardKit.Card("LetterRead", 340, new Vector2(560f, 520f), out _canvas, 20f);
            EventCardKit.JellyTitle(card, Loc.T($"편지 · {index + 1}/{pages.Length}", $"Letter · {index + 1}/{pages.Length}"),
                new Color(1f, 0.88f, 0.45f), new Color(0.40f, 0.22f, 0.05f), 26f, 70f, 40);
            EventCardKit.Divider(card, 104f);
            EventCardKit.IconRow(card, "Icon_Book", new Color(0.98f, 0.80f, 0.35f), Loc.T("스무 통 상자", "Box of twenty"), 128f, 48f, 22, $"{index + 1}/{pages.Length}", new Color(0.30f, 0.55f, 0.95f));
            var box = EventCardKit.InfoBox(card, 190f, 160f);
            var body = CoastHudLayout.MakeText(box, "Body", pages[index], 18, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(16f, 10f), new Vector2(-16f, -10f));
            body.color = EventCardKit.Ink; body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.resizeTextForBestFit = true; body.resizeTextMinSize = 12; body.resizeTextMaxSize = CoastHudLayout.Scaled(18);
            if (!last)
            {
                EventCardKit.IconButton(card, "Next", "Icon_Arrow", Loc.T("다음 통을 연다", "Open the next one"), new Color(0.95f, 0.55f, 0.35f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(340f, 70f), () => ShowLetterPage(save, pages, index + 1, onDone));
            }
            else
            {
                Action finish = () => { Close(); onDone?.Invoke(); };
                EventCardKit.IconButton(card, "Take", "Icon_Star", Loc.T("스무 통 전부 읽는다", "Read all twenty"), new Color(0.95f, 0.55f, 0.35f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(340f, 70f), () =>
                    {
                        save.clueMask |= (int)Clue.Letters; GameManager.I?.Persist();
                        CoastToast.Show(Loc.T($"단서 획득 — {Name(Clue.Letters)} ({Count(save)}/6)", $"Clue — {Name(Clue.Letters)} ({Count(save)}/6)"));
                        finish();
                    });
                EventCardKit.IconButton(card, "Skip", "Icon_Refresh", Loc.T("몇 통만 읽고 덮는다", "Read a few, close the box"), new Color(0.55f, 0.58f, 0.68f),
                    new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(340f, 64f), () =>
                    {
                        CoastToast.Show(Loc.T("단서를 지나쳤다 — 편지는 덜 읽힌 채다", "You passed the clue — letters left unread"));
                        finish();
                    }, 18);
            }
        }

        private static Canvas _canvas;

        private static void Show(SaveData save, Clue c, bool choice, Action onDone)
        {
            Close();
            var card = EventCardKit.Card("ClueCard", 340, new Vector2(560f, choice ? 520f : 440f), out _canvas, 20f);
            EventCardKit.JellyTitle(card, Loc.T("단서", "CLUE"), new Color(1f, 0.85f, 0.30f), new Color(0.35f, 0.16f, 0.02f), 26f, 70f, 46);
            EventCardKit.Divider(card, 104f);
            EventCardKit.IconRow(card, Icon(c), new Color(0.98f, 0.80f, 0.35f), Name(c), 132f, 56f, 26, $"{Count(save) + (choice ? 0 : 1)}/6", new Color(0.30f, 0.55f, 0.95f));
            var box = EventCardKit.InfoBox(card, 200f, choice ? 150f : 110f);
            var body = CoastHudLayout.MakeText(box, "Body", Body(c), 19, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(18f, 10f), new Vector2(-18f, -10f));
            body.color = EventCardKit.Ink; body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.resizeTextForBestFit = true; body.resizeTextMinSize = 12; body.resizeTextMaxSize = CoastHudLayout.Scaled(19);
            Action finish = () => { Close(); onDone?.Invoke(); };
            if (!choice)
            {
                save.clueMask |= (int)c; GameManager.I?.Persist();
                CoastToast.Show(Loc.T($"단서 획득 — {Name(c)} ({Count(save)}/6)", $"Clue — {Name(c)} ({Count(save)}/6)"));
                EventCardKit.IconButton(card, "Ok", "Icon_Star", Loc.T("기억해 둔다", "I'll remember"), new Color(0.30f, 0.62f, 0.95f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(300f, 70f), finish);
            }
            else
            {
                string take = c == Clue.Stones ? Loc.T("돌을 다시 쌓는다", "Stack the stones again") : Loc.T("스무 통 전부 읽는다", "Read all twenty");
                string skip = c == Clue.Stones ? Loc.T("그냥 둔다", "Leave them") : Loc.T("몇 통만 읽고 덮는다", "Read a few, close the box");
                EventCardKit.IconButton(card, "Take", "Icon_Star", take, new Color(0.95f, 0.55f, 0.35f), new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(340f, 70f), () =>
                {
                    save.clueMask |= (int)c; GameManager.I?.Persist();
                    CoastToast.Show(Loc.T($"단서 획득 — {Name(c)} ({Count(save)}/6)", $"Clue — {Name(c)} ({Count(save)}/6)"));
                    finish();
                });
                EventCardKit.IconButton(card, "Skip", "Icon_Refresh", skip, new Color(0.55f, 0.58f, 0.68f), new Vector2(0.5f, 0f), new Vector2(0f, 30f), new Vector2(340f, 64f), () =>
                {
                    CoastToast.Show(Loc.T("단서를 지나쳤다", "You passed the clue by"));
                    finish();
                }, 20);
            }
        }

        public static void Close()
        {
            if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null;
        }
        public static bool IsOpen => _canvas != null;

        /// 상태창·시네마 목록용 한 줄: 「단서 3/6 · 이름 · 라디오 · 하트」
        public static string Summary(SaveData s)
        {
            if (s == null) return "";
            var sb = new System.Text.StringBuilder(); sb.Append(Loc.T($"단서 {Count(s)}/6", $"Clues {Count(s)}/6"));
            foreach (Clue c in new[] { Clue.Name, Clue.Radio, Clue.Heart, Clue.Headband, Clue.Stones, Clue.Letters }) if (Has(s, c)) sb.Append(" · ").Append(Name(c));
            return sb.ToString();
        }
    }
}
