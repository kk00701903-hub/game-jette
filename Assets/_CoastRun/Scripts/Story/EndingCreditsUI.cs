using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 109차(사용자): 엔딩 시네마가 끝나면 **엔딩 크레딧처럼** 지금까지 모은 컬렉션 사진(포토카드 Card_nn)을 한 장씩 보여 준다.
    ///   검은 화면 · 카드가 아래에서 천천히 떠오르며 이름 + 뒷면 손글씨 · 한 장 3.4초 · 탭하면 다음 장, 길게 탭(스킵 버튼)하면 끝.
    ///   모은 사진이 없으면 「아직 모은 사진이 없다」 한 장만. 끝나면 onDone(→ 에필로그 카드 → 원래 흐름).
    public static class EndingCreditsUI
    {
        private static Canvas _canvas; private static Runner _runner;
        public static bool IsOpen => _canvas != null;
        private const float PerCard = 3.4f;

        public static void Show(MetaProfile profile, Action onDone)
        {
            Close();
            var owned = new List<CardDef>();
            if (profile != null) foreach (var c in PhotocardTable.Cards) if (Collection.HasCard(c.id)) owned.Add(c);
            _canvas = CoastUiCanvas.Create("EndingCreditsCanvas", 478);
            var root = CoastUiCanvas.Root(_canvas);
            float pad = CoastUiCanvas.HudPad;
            var black = CoastHudLayout.MakeImage(root, "Black", Vector2.zero, Vector2.one, new Vector2(-pad - 400f, -pad - 400f), new Vector2(pad + 400f, pad + 400f), Color.black);
            black.raycastTarget = true;
            var host = new GameObject("Runner").AddComponent<Runner>();
            host.transform.SetParent(root, false);
            _runner = host;
            host.Begin(root, black, owned, () => { Close(); onDone?.Invoke(); });
        }

        public static void Close()
        {
            if (_canvas != null) UnityEngine.Object.Destroy(_canvas.gameObject);
            _canvas = null; _runner = null;
        }

        private class Runner : MonoBehaviour
        {
            private RectTransform _root; private List<CardDef> _cards; private Action _done;
            private bool _skipAll, _next; private Text _counter;

            public void Begin(RectTransform root, Image tapArea, List<CardDef> cards, Action done)
            {
                _root = root; _cards = cards; _done = done;
                var tb = tapArea.gameObject.AddComponent<Button>(); tb.transition = Selectable.Transition.None;
                tb.onClick.AddListener(() => _next = true);
                // 머리 — 「모은 사진 N / 30」
                var head = CoastHudLayout.MakeText(root, "Head", Loc.T($"모은 사진 {cards.Count} / {PhotocardTable.Count}", $"Photos collected {cards.Count} / {PhotocardTable.Count}"), 20, TextAnchor.MiddleCenter,
                    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -96f), new Vector2(0f, -56f));
                head.color = new Color(1f, 0.92f, 0.70f, 0.9f); head.fontStyle = FontStyle.Bold;
                _counter = head;
                // 오른쪽 아래 「건너뛰기」
                var sk = CoastUiArt.GlossyPill(root, "Skip", new Color(0.35f, 0.35f, 0.42f, 0.85f), 16, 4); sk.raycastTarget = true;
                var srt = sk.rectTransform; srt.anchorMin = srt.anchorMax = new Vector2(1f, 0f); srt.pivot = new Vector2(1f, 0f);
                srt.anchoredPosition = new Vector2(-20f, 40f); srt.sizeDelta = new Vector2(150f, 48f);
                var st = CoastHudLayout.MakeText(srt, "T", Loc.T("건너뛰기 ▶", "Skip ▶"), 17, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(0f, 2f), Vector2.zero);
                st.color = Color.white; st.fontStyle = FontStyle.Bold;
                var sb = sk.gameObject.AddComponent<Button>(); sb.transition = Selectable.Transition.None; sb.onClick.AddListener(() => _skipAll = true);
                StartCoroutine(Run());
            }

            private IEnumerator Run()
            {
                yield return new WaitForSecondsRealtime(0.4f);
                if (_cards.Count == 0)
                {
                    var t = CoastHudLayout.MakeText(_root, "None", Loc.T("아직 모은 사진이 없다.\n러닝과 대회에서 사진을 모아 보자.", "No photos collected yet.\nCollect them in runs and contests."), 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(40f, 0f), new Vector2(-40f, 0f));
                    t.color = new Color(1f, 0.96f, 0.86f); t.horizontalOverflow = HorizontalWrapMode.Wrap;
                    float w = 0f; while (w < 2.6f && !_skipAll && !_next) { w += Time.unscaledDeltaTime; yield return null; }
                }
                for (int i = 0; i < _cards.Count && !_skipAll; i++)
                {
                    _next = false;
                    var c = _cards[i];
                    var frame = CoastUiArt.Panel(_root, "Card" + c.id, new Color(1f, 0.98f, 0.94f), 22); frame.raycastTarget = false;
                    var frt = frame.rectTransform; frt.anchorMin = frt.anchorMax = new Vector2(0.5f, 0.5f); frt.pivot = new Vector2(0.5f, 0.5f);
                    frt.sizeDelta = new Vector2(420f, 630f);
                    var tex = ArtAssets.LoadTexture(c.Image) ?? ArtAssets.LoadTexture(c.FallbackImage);
                    var im = CoastHudLayout.MakeImage(frt, "I", Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -12f), tex != null ? Color.white : new Color(0.55f, 0.62f, 0.85f));
                    im.raycastTarget = false; if (tex != null) { im.sprite = CoastUiArt.AsSprite(tex); im.preserveAspect = true; }
                    var nm = CoastHudLayout.MakeText(_root, "N" + c.id, (Collection.CardSigned(c.id) ? "✦ " : "") + c.Name, 26, TextAnchor.MiddleCenter, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(30f, -380f), new Vector2(-30f, -330f));
                    nm.color = new Color(1f, 0.92f, 0.70f); nm.fontStyle = FontStyle.Bold; CoastUiArt.OutlineText(nm, new Color(0f, 0f, 0f, 0.6f), 1.5f);
                    var bk = CoastHudLayout.MakeText(_root, "B" + c.id, c.Back, 18, TextAnchor.MiddleCenter, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(40f, -424f), new Vector2(-40f, -384f));
                    bk.color = new Color(0.85f, 0.85f, 0.92f); bk.fontStyle = FontStyle.Italic; bk.horizontalOverflow = HorizontalWrapMode.Wrap;
                    if (_counter != null) _counter.text = Loc.T($"모은 사진 {i + 1} / {_cards.Count}", $"Photo {i + 1} / {_cards.Count}");
                    CoastAudioManager.PlayAnywhere(CoastSfx.Coin, 0.25f);
                    float t = 0f;
                    var grp = new[] { (Graphic)frame, im, nm, bk };
                    while (t < PerCard && !_skipAll && !_next)
                    {
                        t += Time.unscaledDeltaTime;
                        float u = Mathf.Clamp01(t / PerCard);
                        float a = Mathf.Min(1f, Mathf.Clamp01(t / 0.6f), Mathf.Clamp01((PerCard - t) / 0.6f));
                        float y = Mathf.Lerp(-60f, 60f, u) + 40f;   // 아래에서 위로 천천히
                        frt.anchoredPosition = new Vector2(0f, y);
                        nm.rectTransform.anchoredPosition = new Vector2(0f, y - 40f); bk.rectTransform.anchoredPosition = new Vector2(0f, y - 40f);
                        frt.localScale = Vector3.one * (0.96f + 0.06f * u);
                        foreach (var g in grp) { var col = g.color; col.a = a * (g == im && tex == null ? 1f : 1f); g.color = col; }
                        yield return null;
                    }
                    Destroy(frame.gameObject); Destroy(nm.gameObject); Destroy(bk.gameObject);
                }
                // 끝 — 「fin」
                var fin = CoastHudLayout.MakeText(_root, "Fin", Loc.T("— 너와 나의 주파수 —", "— Our Frequency —"), 28, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                fin.color = new Color(1f, 0.92f, 0.70f, 0f); fin.fontStyle = FontStyle.Bold;
                float f = 0f; while (f < 1.6f && !_skipAll) { f += Time.unscaledDeltaTime; var col = fin.color; col.a = Mathf.Clamp01(f / 0.8f); fin.color = col; yield return null; }
                var cb = _done; _done = null; cb?.Invoke();
            }
        }
    }
}
