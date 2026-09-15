using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoastRun
{
    /// 집 화면 — 전체 화면 오버레이.
    ///   [방]     주인공·선택 펫 2D 표시, 하단에서 펫만 선택. (가구 트레이 배치 제거)
    ///   [텃밭]   화분 4개: 씨앗 심기(G) → 물 주기 → 수확/판매.
    ///   [놀이]   스토리 미션과 같은 놀이(구슬치기·윷·투호·딱지·무궁화).
    public class HomeUI : MonoBehaviour
    {
        public enum Tab { Room, Balcony, Play }

        private static readonly Color Navy = new Color(0.23f, 0.16f, 0.29f);
        private static readonly Color Ink = new Color(0.29f, 0.21f, 0.31f);
        private static readonly Color Coral = new Color(1f, 0.44f, 0.57f);
        private static readonly Color Mint = new Color(0.30f, 0.71f, 0.67f);
        private static readonly Color Sky = new Color(0.39f, 0.71f, 0.96f);
        private static readonly Color Sun = new Color(1f, 0.72f, 0.30f);
        private static readonly Color Cream = new Color(1f, 0.97f, 0.90f);
        private static readonly Color Grey = new Color(0.62f, 0.63f, 0.70f);

        private GameManager _gm;
        private Sprite _charSprite;
        private Action _onClose;
        private Canvas _canvas;
        private RectTransform _root, _body, _tray, _tabBar;
        private ScrollRect _trayScroll;
        private RectTransform _trayGhost;
        private Text _money, _hint;
        private readonly Image[] _tabImgs = new Image[3];
        private Tab _tab = Tab.Room;

        // 방
        private RectTransform _roomHost, _decoLayer, _charRt;
        private Image _charImg;
        private Vector2 _charPos = new Vector2(0.5f, 0.12f), _charTarget = new Vector2(0.5f, 0.12f);
        private float _wanderT = 2f, _walkPhase;
        private Action _onArrive;
        private readonly Dictionary<string, RectTransform> _placed = new Dictionary<string, RectTransform>();
        private string _dragging;
        private const bool ShowRoomGirl = true;    // 53차(사용자): 마이룸에 주인공 꼬마 + 펫이 같이 있다(방꾸미기 때 잠시 숨겼던 것 복구)
        private RectTransform _petRt; private Image _petImg; private Vector2 _petPos;

        // 베란다
        private int _selectedPot = -1;
        private readonly List<RectTransform> _potRoots = new List<RectTransform>();

        private SaveData Save => _gm != null ? _gm.Save : null;
        private MetaProfile Profile => _gm != null ? _gm.Profile : null;

        public static HomeUI Open(GameManager gm, Sprite charSprite, Action onClose)
        {
            var go = new GameObject("HomeUI");
            var ui = go.AddComponent<HomeUI>();
            ui._gm = gm; ui._charSprite = charSprite; ui._onClose = onClose;
            ui.Build();
            return ui;
        }

        // ── 뼈대 ─────────────────────────────────────────────────────────

        private void Build()
        {
            HomeData.Ensure(Profile);
            HomeData.EnsurePots(Save);
            _canvas = CoastUiCanvas.Create("HomeCanvas", 130, transform);
            _root = CoastUiCanvas.Root(_canvas);

            var dim = CoastHudLayout.MakeImage(_root, "Dim", Vector2.zero, Vector2.one, new Vector2(-CoastUiCanvas.HudPad, -CoastUiCanvas.HudPad), new Vector2(CoastUiCanvas.HudPad, CoastUiCanvas.HudPad), new Color(0.16f, 0.10f, 0.16f, 0.985f));
            dim.raycastTarget = true;

            // 헤더
            var head = CoastUiArt.CutePill(_root, "Head", new Color(0.29f, 0.18f, 0.33f), 22, 4);
            Rect(head.rectTransform, new Vector2(0f, 0.925f), new Vector2(1f, 1f), new Vector2(4f, 4f), new Vector2(-4f, -2f));
            var title = Text(head.transform, "Title", Loc.T("우리 집", "Home"), 24, Cream, TextAnchor.MiddleLeft);
            Rect(title.rectTransform, Vector2.zero, Vector2.one, new Vector2(24f, 0f), new Vector2(-260f, 0f));
            CoastUiArt.OutlineText(title, new Color(0f, 0f, 0f, 0.4f), 1.5f);
            _money = Text(head.transform, "Money", "", 18, new Color(1f, 0.85f, 0.45f), TextAnchor.MiddleRight);
            Rect(_money.rectTransform, Vector2.zero, Vector2.one, new Vector2(200f, 0f), new Vector2(-150f, 0f));
            Button(head.transform, "Close", Loc.T("나가기", "Leave"), Grey, new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(124f, 46f), Close);

            // 탭 — 헤더 바로 아래, 클릭 보장(Fill에도 raycast)
            _tabBar = new GameObject("Tabs", typeof(RectTransform)).GetComponent<RectTransform>();
            _tabBar.SetParent(_root, false);
            Rect(_tabBar, new Vector2(0f, 0.855f), new Vector2(1f, 0.918f), new Vector2(4f, 0f), new Vector2(-4f, 0f));
            string[] names = { Loc.T("방", "Room"), Loc.T("텃밭", "Garden"), Loc.T("놀이", "Play") };
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var pill = CoastUiArt.CutePill(_tabBar, "Tab" + i, Grey, 16, 3);
                Rect(pill.rectTransform, new Vector2(i / 3f, 0f), new Vector2((i + 1) / 3f, 1f), new Vector2(i == 0 ? 0f : 4f, 0f), new Vector2(i == 2 ? 0f : -4f, 0f));
                pill.raycastTarget = true;
                foreach (var im in pill.GetComponentsInChildren<Image>(true)) im.raycastTarget = true;
                var b = pill.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None;
                b.targetGraphic = pill;
                b.onClick.AddListener(() => { CoastPrefs.Vibrate(); SetTab((Tab)idx); });
                var t = Text(pill.transform, "T", names[i], 20, Color.white, TextAnchor.MiddleCenter);
                t.raycastTarget = false;
                CoastUiArt.OutlineText(t, new Color(0f, 0f, 0f, 0.35f), 1.5f);
                _tabImgs[i] = pill;
            }
            _tabBar.SetAsLastSibling();

            // 본문(장면) + 트레이 — 탭과 겹치지 않게 본문 상단을 살짝 내림
            var bodyFrame = CoastUiArt.CutePill(_root, "BodyFrame", new Color(0.95f, 0.85f, 0.70f), 22, 4);
            Rect(bodyFrame.rectTransform, new Vector2(0f, 0.248f), new Vector2(1f, 0.848f), new Vector2(4f, 0f), new Vector2(-4f, 0f));
            _body = new GameObject("Body", typeof(RectTransform), typeof(RectMask2D)).GetComponent<RectTransform>();
            _body.SetParent(bodyFrame.transform, false);
            Rect(_body, Vector2.zero, Vector2.one, new Vector2(7f, 10f), new Vector2(-7f, -7f));

            var trayFrame = CoastUiArt.CutePill(_root, "TrayFrame", new Color(1f, 0.97f, 0.90f), 22, 4);
            Rect(trayFrame.rectTransform, new Vector2(0f, 0.012f), new Vector2(1f, 0.240f), new Vector2(4f, 0f), new Vector2(-4f, 0f));
            _tray = new GameObject("Tray", typeof(RectTransform)).GetComponent<RectTransform>();
            _tray.SetParent(trayFrame.transform, false);
            Rect(_tray, Vector2.zero, Vector2.one, new Vector2(6f, 6f), new Vector2(-6f, -6f));
            _trayScroll = null;

            SetTab(Tab.Room);
            _tabBar.SetAsLastSibling();
        }

        private void SetTab(Tab t)
        {
            _tab = t;
            for (int i = 0; i < 3; i++)
            {
                var fill = _tabImgs[i].transform.Find("Fill")?.GetComponent<Image>();
                var lip = _tabImgs[i].transform.Find("Lip")?.GetComponent<Image>();
                Color c = i == (int)t ? (i == 0 ? Coral : i == 1 ? Mint : Sun) : Grey;
                if (fill != null) fill.color = c;
                if (lip != null) lip.color = Color.Lerp(c, Color.black, 0.45f);
            }
            Clear(_body); Clear(_tray);
            _placed.Clear(); _potRoots.Clear(); _selectedPot = -1; _progress = null; _progressFill = null;
            _petRt = null; _petImg = null; _charRt = null; _charImg = null;
            switch (t)
            {
                case Tab.Room: BuildRoom(); BuildRoomTray(); break;
                case Tab.Balcony: BuildBalcony(); BuildSeedTray(); break;
                case Tab.Play: BuildPlay(); BuildPlayTray(); break;
            }
            RefreshMoney();
            if (_tabBar != null) _tabBar.SetAsLastSibling();
        }

        private void RefreshMoney()
        {
            if (_money != null && Save != null) _money.text = $"{LevelSystem.FormatK(Save.stats.money)} G";   // 53차: k 표기
        }

        private void Close()
        {
            _gm?.Persist(); _gm?.WriteProfileNow();
            var cb = _onClose; _onClose = null;
            Destroy(gameObject);
            cb?.Invoke();
        }

        // ── 방 ───────────────────────────────────────────────────────────

        private void BuildRoom()
        {
            _roomHost = _body;
            // 배경(방 그림) — 여백 없이 본문 전체를 cover로 채운다
            var bgMask = new GameObject("BgMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            bgMask.transform.SetParent(_roomHost, false);
            Rect(bgMask.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bgMask.GetComponent<Image>().color = new Color(0.63f, 0.53f, 0.50f);
            bgMask.GetComponent<Mask>().showMaskGraphic = true;
            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter)).GetComponent<Image>();
            bg.transform.SetParent(bgMask.transform, false);
            var brt = bg.rectTransform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = Vector2.zero; brt.offsetMin = brt.offsetMax = Vector2.zero;
            var fit = bg.GetComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;   // 여백 없이 가득
            fit.aspectRatio = 1f;
            // 38차: 쿼터뷰(아이소메트릭) 방 그림(UI_Room_Iso)
            var iso = ArtAssets.LoadTexture("UI_Room_Iso");
            var art = iso ?? ArtAssets.LoadTexture("UI_Raising_Room_" + Timeline.SeasonOf(Save != null ? Save.week : 1)) ?? ArtAssets.LoadTexture("UI_Raising_Room");
            if (iso != null)
            {
                fit.aspectRatio = (float)iso.width / Mathf.Max(1, iso.height);
                bgMask.GetComponent<Image>().color = new Color(0.22f, 0.16f, 0.14f);
            }
            if (art != null) bg.sprite = CoastUiArt.AsSprite(art); bg.raycastTarget = false;
            // 바닥 탭(캐릭터 숨김 시에도 장식 드래그와 겹치지 않게 투명 판 유지)
            var floor = CoastHudLayout.MakeImage(_roomHost, "FloorTap", new Vector2(0f, 0f), new Vector2(1f, 0.46f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0f));
            floor.raycastTarget = ShowRoomGirl;
            if (ShowRoomGirl)
            {
                var tap = floor.gameObject.AddComponent<PointerTap>();
                tap.OnTap = pos => { var n = ToNorm(pos); _charTarget = new Vector2(Mathf.Clamp(n.x, 0.06f, 0.94f), Mathf.Clamp(n.y, 0.02f, 0.40f)); _wanderT = 6f; _onArrive = null; };
            }
            _decoLayer = new GameObject("Deco", typeof(RectTransform)).GetComponent<RectTransform>();
            _decoLayer.SetParent(_roomHost, false);
            Rect(_decoLayer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            // 주인공 — 방꾸미기는 방만 크게 보여 캐릭터는 숨김
            if (ShowRoomGirl)
            {
                _charRt = new GameObject("Girl", typeof(RectTransform)).GetComponent<RectTransform>();
                _charRt.SetParent(_roomHost, false);
                _charRt.anchorMin = _charRt.anchorMax = new Vector2(0.5f, 0.12f); _charRt.pivot = new Vector2(0.5f, 0f);
                _charRt.sizeDelta = iso != null ? new Vector2(150f, 214f) : new Vector2(210f, 300f);
                var sh = CoastHudLayout.MakeImage(_charRt, "Shadow", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-64f, -4f), new Vector2(64f, 12f), new Color(0f, 0f, 0f, 0.22f));
                sh.sprite = CoastUiArt.RoundedRect(30); sh.type = Image.Type.Sliced;
                _charImg = new GameObject("Img", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                _charImg.transform.SetParent(_charRt, false);
                Rect(_charImg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                _charImg.preserveAspect = true; _charImg.raycastTarget = false;
                // 2D 스프라이트만 (육성 포즈 / Raise_Girl)
                if (_charSprite != null) _charImg.sprite = _charSprite;
                else
                {
                    var girlTex = ArtAssets.LoadTexture("Raise_Girl_Normal")
                                  ?? ArtAssets.LoadTexture("Raise_Girl_Happy")
                                  ?? ArtAssets.LoadTexture("UI_Face_Girl");
                    if (girlTex != null) _charImg.sprite = CoastUiArt.AsSprite(RaisingUI.ChromaKeyed(girlTex));
                    else _charImg.color = new Color(1f, 0.8f, 0.6f);
                }
                _charPos = _charTarget = new Vector2(0.5f, 0.12f);
                // 장착 펫 — 2D UI_Pet / Obs_Pet 만
                var petKind = Save != null ? Save.equippedPet : PetKind.None;
                if (petKind != PetKind.None)
                {
                    _petRt = new GameObject("Pet", typeof(RectTransform)).GetComponent<RectTransform>();
                    _petRt.SetParent(_roomHost, false);
                    _petRt.anchorMin = _petRt.anchorMax = new Vector2(0.62f, 0.10f); _petRt.pivot = new Vector2(0.5f, 0f);
                    _petRt.sizeDelta = new Vector2(96f, 96f);
                    var psh = CoastHudLayout.MakeImage(_petRt, "Shadow", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-34f, -3f), new Vector2(34f, 8f), new Color(0f, 0f, 0f, 0.2f));
                    psh.sprite = CoastUiArt.RoundedRect(20); psh.type = Image.Type.Sliced;
                    _petImg = new GameObject("Img", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                    _petImg.transform.SetParent(_petRt, false);
                    Rect(_petImg.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                    _petImg.preserveAspect = true; _petImg.raycastTarget = false;
                    var petTex = ArtAssets.LoadTexture("UI_Pet_" + petKind) ?? ArtAssets.LoadTexture("Obs_Pet_" + petKind);
                    if (petTex != null) _petImg.sprite = CoastUiArt.AsSprite(RaisingUI.ChromaKeyed(petTex));
                    _petPos = new Vector2(0.62f, 0.10f);
                }
            }
            RefreshPlaced();
        }

        private Vector2 ToNorm(Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_roomHost, screen, null, out var local);
            var r = _roomHost.rect;
            return new Vector2((local.x - r.xMin) / r.width, (local.y - r.yMin) / r.height);
        }

        private void RefreshPlaced()
        {
            if (_decoLayer == null) return;
            if (_charRt != null) _charRt.SetParent(_roomHost, false);
            Clear(_decoLayer); _placed.Clear();
            var p = Profile; HomeData.Ensure(p);
            // 벽걸이 먼저(뒤), 바닥은 y가 높을수록(멀수록) 먼저 그려서 앞뒤가 맞게
            var items = new List<HomeItem>(p.homeItems);
            items.Sort((a, b) =>
            {
                var da = HomeData.Find(a.id); var db = HomeData.Find(b.id);
                bool wa = da != null && HomeData.IsWall(da), wb = db != null && HomeData.IsWall(db);
                if (wa != wb) return wa ? -1 : 1;
                return b.y.CompareTo(a.y);
            });
            foreach (var h in items)
            {
                var d = HomeData.Find(h.id); if (d == null || !HomeData.IsActive(d)) continue;
                var size = HomeData.Size(d);
                var go = DecoVisual(_decoLayer, d, size, !HomeData.IsWall(d));
                var rt = go.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(h.x, h.y); rt.pivot = new Vector2(0.5f, 0f);
                rt.anchoredPosition = Vector2.zero; rt.sizeDelta = size;
                var hit = go.AddComponent<Image>(); hit.color = new Color(0f, 0f, 0f, 0f); hit.raycastTarget = true;
                var drag = go.AddComponent<DecoDrag>();
                string id = h.id;
                drag.OnDrag = pos =>
                {
                    var n = HomeData.ClampPos(d, ToNorm(pos) - new Vector2(0f, size.y * 0.5f / _roomHost.rect.height));
                    rt.anchorMin = rt.anchorMax = n; _dragging = id;
                };
                drag.OnEnd = pos =>
                {
                    var n = HomeData.ClampPos(d, ToNorm(pos) - new Vector2(0f, size.y * 0.5f / _roomHost.rect.height));
                    bool snapped = HomeData.NearSpot(d.id, n);
                    if (snapped) n = HomeData.Spot(d.id);   // 31차: 제자리 근처면 스냅
                    HomeData.Place(Profile, d, n.x, n.y); _gm.WriteProfileNow(); _dragging = null; RefreshPlaced();
                    if (snapped && _placed.TryGetValue(id, out var prt)) { StartCoroutine(PopIn(prt)); CoastToast.Show(Loc.T("딱 맞는 자리!", "Perfect spot!")); }
                };
                drag.OnTap = () =>
                {
                    if (id == "treadmill")
                    {
                        if (!HomeData.TreadmillReady(Save, Profile)) { CoastToast.Show(Loc.T("이번 페이즈엔 이미 운동했어.", "Already trained this phase.")); return; }
                        if (!ShowRoomGirl || _charRt == null)
                        {
                            if (HomeData.UseTreadmill(Save, Profile)) { _gm.Persist(); RefreshMoney(); CoastToast.Show(Loc.T("러닝머신 30분! 체력 +2, 스트레스 +1", "30 min on the treadmill! Stamina +2, stress +1")); }
                            return;
                        }
                        _charTarget = new Vector2(Mathf.Clamp(h.x + 0.14f, 0.06f, 0.94f), Mathf.Clamp(h.y, 0.02f, 0.40f)); _wanderT = 8f;
                        _onArrive = () => { if (HomeData.UseTreadmill(Save, Profile)) { _gm.Persist(); RefreshMoney(); CoastToast.Show(Loc.T("러닝머신 30분! 체력 +2, 스트레스 +1", "30 min on the treadmill! Stamina +2, stress +1")); } };
                    }
                    else CoastToast.Show(Loc.T($"{d.Name} — 끌어서 옮길 수 있어. 트레이에서 [치우기].", $"{d.Name} — drag to move. Remove from the tray."));
                };
                _placed[h.id] = rt;
            }
            if (_charRt != null) _charRt.SetParent(_decoLayer, false);
            if (_petRt != null) _petRt.SetParent(_decoLayer, false);
            ReorderDepth();
            RefreshProgress();
        }

        private Text _progress; private Image _progressFill;
        /// 방 완성도(놓은 장식 / 전체) — Dreamy Room 식 진행 바. 전부 놓으면 1회 300G.
        private void RefreshProgress()
        {
            if (_roomHost == null) return;
            var p = Profile; int n = HomeData.PlacedCount(p), all = HomeData.ActiveCount;
            if (_progress == null)
            {
                var bar = CoastUiArt.Panel(_roomHost, "Progress", new Color(0.1f, 0.05f, 0.12f, 0.55f), 14);
                Rect(bar.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
                bar.rectTransform.pivot = new Vector2(0.5f, 1f); bar.rectTransform.anchoredPosition = new Vector2(0f, -8f); bar.rectTransform.sizeDelta = new Vector2(300f, 30f);
                _progressFill = CoastUiArt.Panel(bar.transform, "Fill", new Color(1f, 0.75f, 0.35f, 0.9f), 12);
                Rect(_progressFill.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(3f, 3f), new Vector2(0f, -3f));
                _progressFill.rectTransform.pivot = new Vector2(0f, 0.5f);
                _progress = Text(bar.transform, "T", "", 12, Color.white, TextAnchor.MiddleCenter);
                CoastUiArt.OutlineText(_progress, new Color(0f, 0f, 0f, 0.5f), 1.2f);
            }
            _progress.transform.parent.SetAsLastSibling();
            _progress.text = Loc.T($"방 완성도 {n} / {all}", $"Room {n} / {all}");
            _progressFill.rectTransform.sizeDelta = new Vector2(294f * Mathf.Clamp01(n / (float)all), 0f);
            if (HomeData.IsComplete(p) && !p.homeCompleteRewarded)
            {
                p.homeCompleteRewarded = true; Save.stats.money += HomeData.CompleteReward; _gm.Persist(); _gm.WriteProfileNow(); RefreshMoney();
                CoastToast.Show(Loc.T($"방 완성! 보너스 +{HomeData.CompleteReward}G", $"Room complete! Bonus +{HomeData.CompleteReward}G"));
            }
        }

        /// 놓을 때 팡 — 0.35초 스케일 바운스 + 반짝이 네 알.
        private System.Collections.IEnumerator PopIn(RectTransform rt)
        {
            if (rt == null) yield break;
            float t = 0f;
            var sparks = new List<RectTransform>();
            for (int i = 0; i < 6; i++)
            {
                var s = CoastUiArt.Panel(rt, "Spark", new Color(1f, 0.95f, 0.6f, 0.95f), 4);
                s.rectTransform.anchorMin = s.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); s.rectTransform.sizeDelta = new Vector2(8f, 8f);
                sparks.Add(s.rectTransform);
            }
            while (t < 0.45f && rt != null)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / 0.35f);
                float sc = u < 0.6f ? Mathf.Lerp(0.3f, 1.18f, u / 0.6f) : Mathf.Lerp(1.18f, 1f, (u - 0.6f) / 0.4f);
                rt.localScale = new Vector3(sc, sc, 1f);
                for (int i = 0; i < sparks.Count; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad; float r = 30f + 70f * u;
                    sparks[i].anchoredPosition = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r + 20f);
                    sparks[i].GetComponent<Image>().color = new Color(1f, 0.95f, 0.6f, 1f - u);
                }
                yield return null;
            }
            if (rt != null) rt.localScale = Vector3.one;
            foreach (var s in sparks) if (s != null) Destroy(s.gameObject);
        }

        /// 방 탭 하단 — 펫 선택 + 조리 버튼.
        private void BuildRoomTray()
        {
            _hint = Text(_tray, "Hint", Loc.T("같이 다닐 펫 · 오른쪽에서 재료 조리", "Pick a pet · cook ingredients on the right"), 13, Ink, TextAnchor.MiddleCenter);
            Rect(_hint.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(8f, -28f), new Vector2(-8f, -4f));
            _hint.fontStyle = FontStyle.Bold;

            // 조리 버튼(우측)
            var cook = CoastUiArt.CutePill(_tray, "Cook", new Color(1f, 0.72f, 0.42f), 14, 3);
            cook.raycastTarget = true;
            foreach (var im in cook.GetComponentsInChildren<Image>(true)) im.raycastTarget = true;
            Rect(cook.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-118f, 6f), new Vector2(-8f, -34f));
            var ct = Text(cook.transform, "T", Loc.T("🍳 조리", "🍳 Cook"), 14, Navy, TextAnchor.MiddleCenter);
            ct.fontStyle = FontStyle.Bold; ct.raycastTarget = false;
            var cb = cook.gameObject.AddComponent<Button>(); cb.transition = Selectable.Transition.None; cb.targetGraphic = cook;
            cb.onClick.AddListener(() => { CoastPrefs.Vibrate(); InventoryUI.Open(_gm, () => { if (_tab == Tab.Room) { Clear(_tray); BuildRoomTray(); RefreshMoney(); } }, cookMode: true); });

            _trayScroll = MakeHScroll(_tray, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(4f, 4f), new Vector2(-124f, -32f), out var content);
            const float cw = 108f, gap = 10f;
            float x = 4f;
            int n = 0;
            foreach (var k in PetShop.ForSale)
            {
                if (Save == null || !PetShop.Owns(Save, k)) continue;
                var tex = ArtAssets.LoadTexture("UI_Pet_" + k) ?? ArtAssets.LoadTexture("Obs_Pet_" + k);
                AddPetCard(content, x, k, PetCompanion.Names[(int)k], tex, cw);
                x += cw + gap;
                n++;
            }
            if (n == 0)
            {
                var empty = Text(_tray, "Empty", Loc.T("상점에서 펫을 데려오세요", "Get a pet from the shop"), 14, Grey, TextAnchor.MiddleCenter);
                Rect(empty.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-130f, -36f));
            }
            content.sizeDelta = new Vector2(Mathf.Max(x, cw), 0f);
        }

        private void AddPetCard(RectTransform content, float x, PetKind k, string name, Texture2D tex, float cw)
        {
            bool on = (Save != null ? Save.equippedPet : PetKind.None) == k;
            var chip = CoastUiArt.CutePill(content, "Pet_" + k, on ? new Color(1f, 0.86f, 0.45f) : new Color(0.94f, 0.92f, 0.96f), 14, 3);
            chip.raycastTarget = true;
            foreach (var im in chip.GetComponentsInChildren<Image>(true)) im.raycastTarget = true;
            var crt = chip.rectTransform;
            crt.anchorMin = new Vector2(0f, 0f); crt.anchorMax = new Vector2(0f, 1f); crt.pivot = new Vector2(0f, 0.5f);
            crt.anchoredPosition = new Vector2(x, 0f); crt.sizeDelta = new Vector2(cw, 0f);
            if (crt.GetComponent<RectMask2D>() == null) crt.gameObject.AddComponent<RectMask2D>();
            if (tex != null)
            {
                var pi = new GameObject("I", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                pi.transform.SetParent(crt, false);
                pi.sprite = CoastUiArt.AsSprite(RaisingUI.ChromaKeyed(tex));
                // AspectRatioFitter(EnvelopeParent)는 RectMask2D/스트레치 부모와 레이아웃 루프를 일으켜 에디터·플레이가 멈출 수 있음
                pi.preserveAspect = false; pi.raycastTarget = false;
                Rect(pi.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 22f), new Vector2(-4f, -4f));
            }
            var t = Text(crt, "T", on ? "★ " + name : name, 12, Navy, TextAnchor.MiddleCenter);
            t.fontStyle = FontStyle.Bold; t.raycastTarget = false;
            CoastUiArt.OutlineText(t, new Color(1f, 1f, 1f, 0.85f), 1.5f);
            Rect(t.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(2f, 3f), new Vector2(-2f, 22f));
            var pk = k;
            var b = chip.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None; b.targetGraphic = chip;
            b.onClick.AddListener(() => SelectPet(pk));
        }

        private void SelectPet(PetKind k)
        {
            if (Save == null) return;
            if (k == PetKind.None || !PetShop.Owns(Save, k)) { CoastToast.Show(Loc.T("아직 없는 펫이야.", "You don't own that pet.")); return; }
            PetShop.Equip(Save, k);
            PetCompanion.Selected = k;
            RunTuning.Pet = k;
            _gm.Persist();
            CoastToast.Show(Loc.T($"{PetCompanion.Names[(int)k]} 선택! 러닝에 같이 나와.", $"{PetCompanion.Names[(int)k]} selected — joins runs."));
            if (_tab == Tab.Room) SetTab(Tab.Room);
        }

        private void BeginTrayGhost(DecoDef d)
        {
            ClearTrayGhost();
            _trayGhost = new GameObject("TrayGhost", typeof(RectTransform)).GetComponent<RectTransform>();
            _trayGhost.SetParent(_root, false);
            _trayGhost.pivot = new Vector2(0.5f, 0f);
            _trayGhost.sizeDelta = HomeData.Size(d) * 0.85f;
            var vis = DecoVisual(_trayGhost, d, _trayGhost.sizeDelta, true);
            Rect(vis.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            foreach (var g in _trayGhost.GetComponentsInChildren<Graphic>()) g.raycastTarget = false;
            CanvasGroup cg = _trayGhost.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false; cg.alpha = 0.92f;
        }

        private void MoveTrayGhost(Vector2 screenPos)
        {
            if (_trayGhost == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screenPos, null, out var local);
            _trayGhost.anchorMin = _trayGhost.anchorMax = new Vector2(0.5f, 0.5f);
            _trayGhost.anchoredPosition = local;
        }

        private void EndTrayGhost(DecoDef d, Vector2 screenPos)
        {
            ClearTrayGhost();
            if (_roomHost == null || d == null) return;
            // 방(_body) 위에 떨어뜨렸을 때만 배치
            if (!RectTransformUtility.RectangleContainsScreenPoint(_roomHost, screenPos, null))
            {
                CoastToast.Show(Loc.T("방 안으로 끌어다 놓아.", "Drop it inside the room."));
                return;
            }
            var p = Profile;
            if (!HomeData.Owns(p, d))
            {
                if (d.FromRun || !HomeData.TryBuy(Save, p, d)) { CoastToast.Show(Loc.T("G가 모자라.", "Not enough G.")); return; }
                _gm.Persist();
                CoastToast.Show(Loc.T($"{d.Name} 구매!", $"Bought {d.Name}!"));
            }
            if (HomeData.IsPlaced(p, d)) HomeData.Remove(p, d.id);
            var n = HomeData.ClampPos(d, ToNorm(screenPos));
            if (HomeData.NearSpot(d.id, n)) n = HomeData.Spot(d.id);
            HomeData.Place(p, d, n.x, n.y);
            _gm.WriteProfileNow();
            RefreshPlaced(); Clear(_tray); BuildRoomTray(); RefreshMoney();
            if (_placed.TryGetValue(d.id, out var prt)) StartCoroutine(PopIn(prt));
        }

        private void ClearTrayGhost()
        {
            if (_trayGhost != null) { Destroy(_trayGhost.gameObject); _trayGhost = null; }
        }

        private void RoomAct(DecoDef d)
        {
            var p = Profile; string pop = null;
            if (HomeData.IsPlaced(p, d)) { HomeData.Remove(p, d.id); CoastToast.Show(Loc.T($"{d.Name}를 치웠어.", $"Removed {d.Name}.")); }
            else if (HomeData.Owns(p, d))
            {
                var pos = HomeData.Spot(d.id);   // 31차: 제자리로 팡
                HomeData.Place(p, d, pos.x, pos.y);
                pop = d.id;
            }
            else if (!d.FromRun)
            {
                if (!HomeData.TryBuy(Save, p, d)) { CoastToast.Show(Loc.T("G가 모자라.", "Not enough G.")); return; }
                var pos = HomeData.Spot(d.id);
                HomeData.Place(p, d, pos.x, pos.y);
                _gm.Persist();
                CoastToast.Show(Loc.T($"{d.Name} 구매!", $"Bought {d.Name}!"));
                pop = d.id;
            }
            _gm.WriteProfileNow();
            RefreshPlaced(); Clear(_tray); BuildRoomTray(); RefreshMoney();
            if (pop != null && _placed.TryGetValue(pop, out var prt)) StartCoroutine(PopIn(prt));
        }

        // ── 베란다 ───────────────────────────────────────────────────────

        // ── 텃밭: 집앞 마당(UI_Tama_Yard) 위 화분 4개 — 작물 4종, 2~3주 자라고 수확 때 성공 확률 ──
        private static readonly Vector2[] PotSpots = { new Vector2(0.18f, 0.14f), new Vector2(0.40f, 0.10f), new Vector2(0.62f, 0.10f), new Vector2(0.84f, 0.14f) };
        private void BuildBalcony()
        {
            // 집앞 텃밭/마당 배경
            var bgMask = new GameObject("BgMask", typeof(RectTransform), typeof(Image), typeof(Mask));
            bgMask.transform.SetParent(_body, false);
            Rect(bgMask.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bgMask.GetComponent<Image>().color = new Color(0.35f, 0.55f, 0.40f);
            bgMask.GetComponent<Mask>().showMaskGraphic = true;
            var bg = new GameObject("Bg", typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter)).GetComponent<Image>();
            bg.transform.SetParent(bgMask.transform, false);
            var brt = bg.rectTransform; brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = brt.offsetMax = Vector2.zero;
            var fit = bg.GetComponent<AspectRatioFitter>(); fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            var yard = ArtAssets.LoadTexture("UI_Tama_Yard")
                       ?? ArtAssets.LoadTexture("BG_OrangeFarm")
                       ?? ArtAssets.LoadTexture("UI_Raising_Room");
            if (yard != null) { fit.aspectRatio = (float)yard.width / Mathf.Max(1, yard.height); bg.sprite = CoastUiArt.AsSprite(yard); }
            bg.raycastTarget = false;
            var head = Text(_body, "Head", Loc.T("🌱 텃밭 · 심으면 주마다 자라고, 다 자라면 수확(성공 확률!)", "🌱 Garden · grows weekly, harvest when ripe (chance!)"), 13, Color.white, TextAnchor.MiddleCenter);
            Rect(head.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(10f, -34f), new Vector2(-10f, -6f));
            CoastUiArt.OutlineText(head, new Color(0f, 0f, 0f, 0.6f), 1.5f);
            _potRoots.Clear();
            for (int i = 0; i < HomeData.PotCount; i++)
            {
                var root = new GameObject("Pot" + i, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                root.SetParent(_body, false);
                root.anchorMin = root.anchorMax = PotSpots[i]; root.pivot = new Vector2(0.5f, 0f);
                root.anchoredPosition = Vector2.zero; root.sizeDelta = new Vector2(120f, 180f);
                root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f); root.GetComponent<Image>().raycastTarget = true;
                int pi = i;
                var b = root.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None; b.onClick.AddListener(() => PotTapped(pi));
                _potRoots.Add(root);
            }
            RefreshPots();
        }

        private void RefreshPots()
        {
            var s = Save; HomeData.EnsurePots(s);
            for (int i = 0; i < _potRoots.Count; i++)
            {
                var root = _potRoots[i]; Clear(root);
                int stage = HomeData.Stage(s, i);
                var pot = s.pots[i]; var seed = HomeData.Seed(pot.seed);
                bool sel = _selectedPot == i;
                // 화분 그림자 + 통 + 흙
                var sh = CoastUiArt.Panel(root, "Shadow", new Color(0f, 0f, 0f, 0.22f), 20);
                Rect(sh.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                sh.rectTransform.pivot = new Vector2(0.5f, 0.5f); sh.rectTransform.anchoredPosition = new Vector2(0f, 4f); sh.rectTransform.sizeDelta = new Vector2(104f, 26f);
                var body = CoastUiArt.Panel(root, "Pot", sel ? new Color(0.98f, 0.62f, 0.45f) : new Color(0.85f, 0.47f, 0.34f), 14);
                Rect(body.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                body.rectTransform.pivot = new Vector2(0.5f, 0f); body.rectTransform.anchoredPosition = new Vector2(0f, 6f); body.rectTransform.sizeDelta = new Vector2(80f, 54f);
                var rim = CoastUiArt.Panel(root, "Rim", new Color(0.95f, 0.62f, 0.47f), 10);
                Rect(rim.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                rim.rectTransform.pivot = new Vector2(0.5f, 0f); rim.rectTransform.anchoredPosition = new Vector2(0f, 54f); rim.rectTransform.sizeDelta = new Vector2(92f, 14f);
                var soil = CoastUiArt.Panel(root, "Soil", new Color(0.35f, 0.22f, 0.14f), 8);
                Rect(soil.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                soil.rectTransform.pivot = new Vector2(0.5f, 0f); soil.rectTransform.anchoredPosition = new Vector2(0f, 58f); soil.rectTransform.sizeDelta = new Vector2(78f, 8f);
                if (seed != null) DrawPlant(root, seed, stage);
                // 이름표
                string cap = seed == null ? Loc.T("빈 화분", "Empty")
                    : stage == 5 ? Loc.T($"{seed.Name} 다 자랐다!", $"{seed.Name} ready!")
                    : Loc.T($"{seed.Name} · {seed.weeks - pot.growth}주 남음", $"{seed.Name} · {seed.weeks - pot.growth}w left");
                var t = Text(root, "Cap", cap, 12, Color.white, TextAnchor.MiddleCenter);
                Rect(t.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -58f), new Vector2(24f, -36f));
                CoastUiArt.OutlineText(t, new Color(0f, 0f, 0f, 0.7f), 1.5f);
                string label; Color col;
                if (seed == null) { label = sel ? Loc.T("아래서 고르기 ↓", "Pick below ↓") : Loc.T("심기", "Plant"); col = Mint; }
                else if (stage == 5) { label = Loc.T($"수확 · 성공 {Mathf.RoundToInt(seed.chance * 100)}%", $"Harvest · {Mathf.RoundToInt(seed.chance * 100)}%"); col = Coral; }
                else { label = Loc.T("자라는 중", "Growing"); col = Grey; }
                var b = Button(root, "Act", label, col, new Vector2(0.5f, 1f), new Vector2(0f, -2f), new Vector2(132f, 34f), () => PotTapped(i));
                b.GetComponentInChildren<Text>().fontSize = CoastHudLayout.Scaled(12);
                b.interactable = !(seed != null && stage < 5);
            }
        }

        /// 작물 그림(단계 1 씨앗 → 2 새싹 → 3 줄기 → 4 봉오리 → 5 열매/꽃) — 작물마다 색·모양 다르게.
        private static void DrawPlant(RectTransform root, SeedDef seed, int stage)
        {
            if (stage <= 1)
            {
                var sprout = CoastUiArt.Panel(root, "Seed", new Color(0.75f, 0.65f, 0.45f), 4);
                Rect(sprout.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                sprout.rectTransform.pivot = new Vector2(0.5f, 0f); sprout.rectTransform.anchoredPosition = new Vector2(0f, 62f); sprout.rectTransform.sizeDelta = new Vector2(12f, 8f);
                return;
            }
            float h = stage == 2 ? 24f : stage == 3 ? 60f : stage == 4 ? 88f : 104f;
            bool rice = seed.id == "rice";
            int stems = rice ? 5 : 1;
            for (int sIdx = 0; sIdx < stems; sIdx++)
            {
                float dx = rice ? (sIdx - 2) * 10f : 0f;
                var stem = CoastUiArt.Panel(root, "Stem" + sIdx, rice && stage == 5 ? new Color(0.80f, 0.72f, 0.35f) : new Color(0.35f, 0.65f, 0.35f), 4);
                Rect(stem.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                stem.rectTransform.pivot = new Vector2(0.5f, 0f); stem.rectTransform.anchoredPosition = new Vector2(dx, 64f); stem.rectTransform.sizeDelta = new Vector2(rice ? 4f : 7f, h * (rice ? 0.9f + 0.05f * (sIdx % 3) : 1f));
                stem.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rice ? (sIdx - 2) * 6f : 0f);
            }
            if (!rice)
                for (int k = 0; k < (stage >= 3 ? 2 : 1); k++)
                {
                    var leaf = CoastUiArt.Panel(root, "Leaf" + k, new Color(0.40f, 0.72f, 0.40f), 12);
                    Rect(leaf.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                    leaf.rectTransform.pivot = new Vector2(k == 0 ? 1f : 0f, 0.5f); leaf.rectTransform.anchoredPosition = new Vector2(k == 0 ? -2f : 2f, 64f + h * (k == 0 ? 0.35f : 0.6f));
                    leaf.rectTransform.sizeDelta = new Vector2(30f, 16f); leaf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, k == 0 ? 25f : -25f);
                }
            if (stage < 4) return;
            float r = stage == 4 ? 10f : 20f;
            if (seed.id == "rose")
            {
                for (int k = 0; k < 6; k++)
                {
                    float a = k * 60f * Mathf.Deg2Rad;
                    var petal = CoastUiArt.Panel(root, "Petal" + k, seed.petal, 14);
                    Rect(petal.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                    petal.rectTransform.pivot = new Vector2(0.5f, 0.5f); petal.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(a) * r * 0.85f, 64f + h + Mathf.Sin(a) * r * 0.85f); petal.rectTransform.sizeDelta = new Vector2(r * 1.2f, r * 1.2f);
                }
                var mid = CoastUiArt.Panel(root, "Mid", Color.Lerp(seed.petal, Color.white, 0.4f), 14);
                Rect(mid.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                mid.rectTransform.pivot = new Vector2(0.5f, 0.5f); mid.rectTransform.anchoredPosition = new Vector2(0f, 64f + h); mid.rectTransform.sizeDelta = new Vector2(r, r);
            }
            else if (rice)
            {
                for (int k = 0; k < 5; k++)
                {
                    var ear = CoastUiArt.Panel(root, "Ear" + k, seed.petal, 6);
                    Rect(ear.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                    ear.rectTransform.pivot = new Vector2(0.5f, 0f); ear.rectTransform.anchoredPosition = new Vector2((k - 2) * 10f + (k - 2) * 2f, 64f + h * 0.8f); ear.rectTransform.sizeDelta = new Vector2(8f, r * 1.3f);
                    ear.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (k - 2) * 14f);
                }
            }
            else
            {
                // 토마토·감자: 열매 3알
                int n = stage == 5 ? 3 : 1;
                for (int k = 0; k < n; k++)
                {
                    var fruit = CoastUiArt.Panel(root, "Fruit" + k, seed.petal, 12);
                    Rect(fruit.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
                    fruit.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    fruit.rectTransform.anchoredPosition = new Vector2((k - 1) * 18f, 64f + h * (0.55f + 0.2f * (k % 2)) - (seed.id == "potato" ? h * 0.5f : 0f));
                    fruit.rectTransform.sizeDelta = new Vector2(r, r * 0.9f);
                }
            }
        }

        private void PotTapped(int i)
        {
            var s = Save; HomeData.EnsurePots(s);
            var pot = s.pots[i]; var seed = HomeData.Seed(pot.seed);
            if (seed == null) { _selectedPot = _selectedPot == i ? -1 : i; RefreshPots(); Clear(_tray); BuildSeedTray(); return; }
            if (HomeData.IsBloomed(s, i))
            {
                bool ok = HomeData.Harvest(s, i, out var sd); _gm.Persist();
                if (ok) { CoastToast.Show(Loc.T($"수확 성공! {sd.Name} → {sd.RewardText}", $"Harvest! {sd.Name} → {sd.RewardText}")); CoastAudioManager.PlayAnywhere(CoastSfx.RankS, 0.6f); }
                else { CoastToast.Show(Loc.T($"{sd.Name}이(가) 시들었다… (성공 {Mathf.RoundToInt(sd.chance * 100)}%)", $"{sd.Name} withered… ({Mathf.RoundToInt(sd.chance * 100)}%)")); CoastAudioManager.PlayAnywhere(CoastSfx.NearMiss, 0.5f); }
            }
            else CoastToast.Show(Loc.T($"아직 자라는 중 — {seed.weeks - pot.growth}주 남았어(주가 지나면 자라).", $"Still growing — {seed.weeks - pot.growth}w left."));
            RefreshPots(); RefreshMoney();
        }

        private void BuildSeedTray()
        {
            string head = _selectedPot >= 0 ? Loc.T($"화분 {_selectedPot + 1}에 심을 작물을 골라", $"Pick a crop for pot {_selectedPot + 1}") : Loc.T("🌱 식물 키우기 · 고르기  (빈 화분을 먼저 탭)", "🌱 Grow a plant · pick (tap an empty pot first)");
            _hint = Text(_tray, "Hint", head, 13, Ink, TextAnchor.MiddleCenter);
            Rect(_hint.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -26f), new Vector2(0f, 0f));
            var scroll = MakeHScroll(_tray, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -28f), out var content);
            const float cw = 154f, gap = 8f;
            Color[] cardCols = { new Color(1f, 0.88f, 0.84f), new Color(1f, 0.95f, 0.80f), new Color(0.90f, 0.97f, 0.82f), new Color(1f, 0.88f, 0.94f) };
            for (int i = 0; i < HomeData.Seeds.Length; i++)
            {
                var sd = HomeData.Seeds[i];
                var card = CoastUiArt.CutePill(content, "S_" + sd.id, cardCols[i % cardCols.Length], 16, 3);
                card.rectTransform.anchorMin = new Vector2(0f, 0f); card.rectTransform.anchorMax = new Vector2(0f, 1f); card.rectTransform.pivot = new Vector2(0f, 0.5f);
                card.rectTransform.anchoredPosition = new Vector2(i * (cw + gap), 0f); card.rectTransform.sizeDelta = new Vector2(cw, 0f);
                // 아이콘: 작물 색 원 + 잎
                var ic = new GameObject("Icon", typeof(RectTransform)).GetComponent<RectTransform>();
                ic.SetParent(card.transform, false); ic.anchorMin = ic.anchorMax = new Vector2(0.5f, 1f); ic.pivot = new Vector2(0.5f, 1f); ic.anchoredPosition = new Vector2(0f, -8f); ic.sizeDelta = new Vector2(64f, 64f);
                var mid = CoastUiArt.Panel(ic, "M", sd.petal, 22);
                mid.rectTransform.anchorMin = mid.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); mid.rectTransform.sizeDelta = new Vector2(sd.id == "rice" ? 26f : 44f, sd.id == "rice" ? 56f : 40f);
                var lf = CoastUiArt.Panel(ic, "L", sd.center, 10);
                lf.rectTransform.anchorMin = lf.rectTransform.anchorMax = new Vector2(0.5f, 0.5f); lf.rectTransform.anchoredPosition = new Vector2(14f, 22f); lf.rectTransform.sizeDelta = new Vector2(22f, 12f); lf.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 30f);
                var nm = Text(card.transform, "Name", sd.Name, 16, Navy, TextAnchor.MiddleCenter); nm.fontStyle = FontStyle.Bold;
                Rect(nm.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(4f, 84f), new Vector2(-4f, 108f));
                var pr = Text(card.transform, "Price", $"{sd.price}G", 15, new Color(0.35f, 0.55f, 0.20f), TextAnchor.MiddleCenter); pr.fontStyle = FontStyle.Bold;
                Rect(pr.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(4f, 62f), new Vector2(-4f, 84f));
                var info = Text(card.transform, "Info", Loc.T($"{sd.weeks}주 · {sd.RewardText}", $"{sd.weeks}w · {sd.RewardText}"), 11, Ink, TextAnchor.MiddleCenter);
                Rect(info.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(2f, 44f), new Vector2(-2f, 62f));
                bool can = _selectedPot >= 0 && Save.stats.money >= sd.price;
                var b = Button(card.transform, "Buy", Loc.T($"성공 {Mathf.RoundToInt(sd.chance * 100)}%", $"{Mathf.RoundToInt(sd.chance * 100)}%"), can ? Mint : Grey, new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(130f, 34f), () =>
                {
                    if (_selectedPot < 0) { CoastToast.Show(Loc.T("먼저 빈 화분을 탭해.", "Tap an empty pot first.")); return; }
                    if (!HomeData.Plant(Save, _selectedPot, sd)) { CoastToast.Show(Loc.T("G가 모자라거나 빈 화분이 아니야.", "Not enough G or pot not empty.")); return; }
                    _gm.Persist(); CoastToast.Show(Loc.T($"{sd.Name}을(를) 심었어. {sd.weeks}주 뒤에 수확!", $"Planted {sd.Name}. Harvest in {sd.weeks}w!"));
                    _selectedPot = -1; RefreshPots(); RefreshMoney(); Clear(_tray); BuildSeedTray();
                });
                b.GetComponentInChildren<Text>().fontSize = CoastHudLayout.Scaled(13);
                b.interactable = can;
            }
            content.sizeDelta = new Vector2(HomeData.Seeds.Length * (cw + gap), 0f);
        }

        // ── 놀이 (시안: 파스텔 카드 + MG 아이콘 + 하기) ───────────────────

        private static readonly Color[] PlayCardCols =
        {
            new Color(0.78f, 0.90f, 0.98f), // 구슬 — 하늘
            new Color(0.98f, 0.84f, 0.88f), // 윷 — 분홍
            new Color(0.84f, 0.94f, 0.78f), // 투호 — 연두
            new Color(0.99f, 0.92f, 0.72f), // 딱지 — 노랑
            new Color(0.90f, 0.82f, 0.96f), // 무궁화 — 보라
        };
        private static readonly Color[] PlayTitleCols =
        {
            new Color(0.18f, 0.32f, 0.62f),
            new Color(0.62f, 0.22f, 0.32f),
            new Color(0.22f, 0.48f, 0.28f),
            new Color(0.55f, 0.32f, 0.12f),
            new Color(0.42f, 0.22f, 0.58f),
        };
        private static readonly string[] PlayIcons = { "UI_MG_Marbles", "UI_MG_Yut", "UI_MG_Tuho", "UI_MG_Ddakji", "UI_MG_Mugunghwa" };
        private static readonly string[] PlayBlurbsKo =
        {
            "(방향 선택) → [발사]로 원 구슬을 쳐요. 3번 안에 상대편 구슬 2개 이하로 남기면 승리!",
            "윷을 던져 3칸! 먼저 도착하면 승리",
            "(방향 선택) → 한 게이지로 화살 던져 5번 중 3번 항아리에 넣으면 승리",
            "한 게이지로 위로 던져 노란 구역에서 뒤집기! 3번 안에 한 번 뒤집으면 승리",
            "[달리기]를 누르면 앞으로! 술래가 돌아보면 멈춰, 3번 안에 도달하면 승리",
        };
        private static readonly string[] PlayBlurbsEn =
        {
            "Aim → [Shoot] the cue marble. Leave 2 or fewer opponent marbles in 3 turns!",
            "Throw yut — first to finish wins!",
            "Aim → throw with the gauge. Land 3 of 5 in the jar to win!",
            "Hit the yellow zone on the gauge to flip! Once in 3 tries wins.",
            "Hold [Run]! Freeze when the tagger turns — reach them in 3 tries.",
        };
        private static readonly Color[] PlayFlowerCols =
        {
            new Color(0.45f, 0.72f, 0.95f),
            new Color(0.95f, 0.55f, 0.70f),
            new Color(0.45f, 0.78f, 0.48f),
            new Color(0.98f, 0.78f, 0.28f),
            new Color(0.72f, 0.52f, 0.92f),
        };

        private void BuildPlay()
        {
            // 크림 바탕 + 컨페티·코너 꽃 (시안)
            CoastHudLayout.MakeImage(_body, "Bg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.99f, 0.96f, 0.90f));
            SprinklePlayDecor(_body);

            var defs = ChapterMission.All;
            float gap = 0.012f;
            float rowH = (0.96f - gap * (defs.Length - 1)) / Mathf.Max(1, defs.Length);
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                float top = 0.98f - i * (rowH + gap);
                float bot = top - rowH;
                var card = CoastUiArt.CutePill(_body, "G" + i, PlayCardCols[i % PlayCardCols.Length], 18, 3);
                Rect(card.rectTransform, new Vector2(0.025f, bot), new Vector2(0.975f, top), Vector2.zero, Vector2.zero);

                // 왼쪽: 꽃 테두리 원형 아이콘
                BuildPlayIcon(card.transform, i);

                var nm = Text(card.transform, "Name", Loc.T(d.nameKo, d.nameEn), 18, PlayTitleCols[i % PlayTitleCols.Length], TextAnchor.MiddleLeft);
                Rect(nm.rectTransform, new Vector2(0f, 0.52f), new Vector2(1f, 1f), new Vector2(108f, 2f), new Vector2(-128f, -2f));
                nm.fontStyle = FontStyle.Bold;
                nm.resizeTextForBestFit = true; nm.resizeTextMinSize = 12; nm.resizeTextMaxSize = CoastHudLayout.Scaled(20);

                string blurb = Loc.T(PlayBlurbsKo[i % PlayBlurbsKo.Length], PlayBlurbsEn[i % PlayBlurbsEn.Length]);
                var desc = Text(card.transform, "Desc", blurb, 11, Ink, TextAnchor.UpperLeft);
                desc.horizontalOverflow = HorizontalWrapMode.Wrap;
                desc.verticalOverflow = VerticalWrapMode.Truncate;
                desc.resizeTextForBestFit = true; desc.resizeTextMinSize = 9; desc.resizeTextMaxSize = CoastHudLayout.Scaled(12);
                Rect(desc.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.58f), new Vector2(108f, 6f), new Vector2(-128f, 0f));

                var kind = d.kind;
                Button(card.transform, "Play", Loc.T("하기", "Play"), Coral, new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(112f, 46f), () => StartMini(kind));
            }
        }

        private static void BuildPlayIcon(Transform card, int i)
        {
            Color flower = PlayFlowerCols[i % PlayFlowerCols.Length];
            // 꽃잎 4장 (아이콘 원 둘레)
            const float cx = 50f;
            for (int p = 0; p < 4; p++)
            {
                float ang = p * 90f + 45f;
                float rad = ang * Mathf.Deg2Rad;
                float px = cx + Mathf.Cos(rad) * 30f;
                float py = Mathf.Sin(rad) * 30f;
                var petal = CoastHudLayout.MakeImage(card, "Petal" + p, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(px - 11f, py - 11f), new Vector2(px + 11f, py + 11f), flower);
                petal.raycastTarget = false;
                petal.sprite = CoastUiArt.RoundedRect(32);
                petal.color = new Color(flower.r, flower.g, flower.b, 0.92f);
            }
            var ring = CoastHudLayout.MakeImage(card, "IcoRing", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(12f, -38f), new Vector2(88f, 38f), Color.white);
            ring.raycastTarget = false;
            ring.sprite = CoastUiArt.RoundedRect(40);
            ring.color = new Color(1f, 1f, 1f, 0.98f);
            if (ring.gameObject.GetComponent<Mask>() == null)
            {
                var m = ring.gameObject.AddComponent<Mask>();
                m.showMaskGraphic = true;
            }
            string iconName = PlayIcons[i % PlayIcons.Length];
            var tex = ArtAssets.LoadTexture(iconName);
            if (tex != null)
            {
                var im = new GameObject("I", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                im.transform.SetParent(ring.transform, false);
                im.sprite = CoastUiArt.AsSprite(RaisingUI.ChromaKeyed(tex));
                im.preserveAspect = true; im.raycastTarget = false;
                Rect(im.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 6f), new Vector2(-6f, -6f));
            }
        }

        private static void SprinklePlayDecor(RectTransform host)
        {
            Color[] dots =
            {
                new Color(1f, 0.55f, 0.70f, 0.55f), new Color(0.55f, 0.75f, 1f, 0.50f),
                new Color(0.95f, 0.80f, 0.30f, 0.50f), new Color(0.55f, 0.88f, 0.55f, 0.45f),
                new Color(0.78f, 0.55f, 0.95f, 0.50f),
            };
            Vector2[] spots =
            {
                new Vector2(0.06f, 0.92f), new Vector2(0.94f, 0.90f), new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.10f),
                new Vector2(0.04f, 0.55f), new Vector2(0.96f, 0.48f), new Vector2(0.50f, 0.97f), new Vector2(0.72f, 0.05f),
                new Vector2(0.28f, 0.04f), new Vector2(0.15f, 0.75f), new Vector2(0.85f, 0.70f), new Vector2(0.60f, 0.95f),
            };
            for (int i = 0; i < spots.Length; i++)
            {
                float s = (i % 3 == 0) ? 14f : 8f;
                var c = dots[i % dots.Length];
                var d = CoastHudLayout.MakeImage(host, "Dot" + i, spots[i], spots[i],
                    new Vector2(-s, -s), new Vector2(s, s), c);
                d.raycastTarget = false;
                d.sprite = CoastUiArt.RoundedRect(32);
            }
            // 코너 꽃 뭉치
            PlaceCornerFlower(host, new Vector2(0.02f, 0.02f), new Color(0.92f, 0.45f, 0.70f));
            PlaceCornerFlower(host, new Vector2(0.98f, 0.02f), new Color(0.70f, 0.50f, 0.95f));
            PlaceCornerFlower(host, new Vector2(0.02f, 0.98f), new Color(0.45f, 0.78f, 0.95f));
            PlaceCornerFlower(host, new Vector2(0.98f, 0.98f), new Color(0.98f, 0.72f, 0.35f));
        }

        private static void PlaceCornerFlower(RectTransform host, Vector2 anchor, Color col)
        {
            for (int i = 0; i < 5; i++)
            {
                float ang = i * 72f * Mathf.Deg2Rad;
                var p = CoastHudLayout.MakeImage(host, "F", anchor, anchor,
                    new Vector2(Mathf.Cos(ang) * 10f - 7f, Mathf.Sin(ang) * 10f - 7f),
                    new Vector2(Mathf.Cos(ang) * 10f + 7f, Mathf.Sin(ang) * 10f + 7f),
                    new Color(col.r, col.g, col.b, 0.85f));
                p.raycastTarget = false;
                p.sprite = CoastUiArt.RoundedRect(28);
            }
            var center = CoastHudLayout.MakeImage(host, "FC", anchor, anchor, new Vector2(-5f, -5f), new Vector2(5f, 5f), new Color(1f, 0.92f, 0.45f, 0.95f));
            center.raycastTarget = false;
            center.sprite = CoastUiArt.RoundedRect(24);
        }

        private void BuildPlayTray()
        {
            int left = HomeData.RewardPlaysLeft(Save);
            int max = HomeData.MiniGameRewardPerWeek;
            string ko = left > 0
                ? $"스토리와 같은 놀이예요 · 이번 주 보상 {left}/{max}회\n다 쓰면 연습(보상 없음). 주차가 바뀌면 다시 채워져."
                : $"스토리와 같은 놀이예요 · 이번 주 보상 {max}/{max}회 다 씀 · 연습(보상 없음).\n주차가 바뀌면 다시 채워져.";
            string en = left > 0
                ? $"Same games as story · rewards {left}/{max} this week\nThen practice (no reward). Refills each week."
                : $"Same games as story · {max}/{max} rewards used · practice only.\nRefills each week.";
            var t = Text(_tray, "Info", Loc.T(ko, en), 14, Ink, TextAnchor.MiddleCenter);
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            Rect(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 10f), new Vector2(-10f, -10f));
        }

        private void StartMini(ChapterMission.Kind kind)
        {
            if (ChapterMissionUI.IsOpen) return;
            bool rewardable = HomeData.RewardPlaysLeft(Save) > 0;
            // replay=true: 스토리 게이트와 무관하게 언제든 플레이(미션 클리어 비트는 안 건드림)
            ChapterMissionUI.Play(kind, true, won =>
            {
                if (won && rewardable)
                {
                    int pay = ChapterMission.Reward(_gm);
                    int given = HomeData.GiveReward(Save, pay);
                    if (given > 0) { _gm.Persist(); CoastToast.Show(Loc.T($"+{given}G!", $"+{given}G!")); }
                }
                else if (won && !rewardable)
                    CoastToast.Show(Loc.T("연습 게임 — 보상은 없어.", "Practice — no reward."));
                RefreshMoney();
                if (_tab == Tab.Play) { Clear(_tray); BuildPlayTray(); }
            });
        }

        // ── 주인공 걷기 ──────────────────────────────────────────────────

        private void Update()
        {
            if (_tab != Tab.Room || _charRt == null || _roomHost == null) return;
            float dt = Time.unscaledDeltaTime;
            _wanderT -= dt;
            if (_wanderT <= 0f)
            {
                _wanderT = UnityEngine.Random.Range(3f, 7f);
                _charTarget = new Vector2(UnityEngine.Random.Range(0.15f, 0.85f), UnityEngine.Random.Range(0.04f, 0.36f));
                _onArrive = null;
            }
            var delta = _charTarget - _charPos;
            float speed = 0.28f;
            bool walking = delta.magnitude > 0.005f;
            if (walking)
            {
                var step = delta.normalized * speed * dt;
                if (step.magnitude >= delta.magnitude) { _charPos = _charTarget; walking = false; var cb = _onArrive; _onArrive = null; cb?.Invoke(); }
                else _charPos += step;
                if (Mathf.Abs(delta.x) > 0.01f) _charImg.rectTransform.localScale = new Vector3(delta.x < 0f ? -1f : 1f, 1f, 1f);
                _walkPhase += dt * 11f;
            }
            else _walkPhase = Mathf.MoveTowards(_walkPhase, 0f, dt * 6f);
            float bob = walking ? Mathf.Abs(Mathf.Sin(_walkPhase)) * 7f : 0f;
            float depth = Mathf.Lerp(1.0f, 0.78f, _charPos.y / 0.42f);   // 멀수록 작게
            _charRt.anchorMin = _charRt.anchorMax = new Vector2(_charPos.x, _charPos.y);
            _charRt.anchoredPosition = new Vector2(0f, bob);
            _charRt.localScale = new Vector3(depth, depth * (walking ? 1f + 0.03f * Mathf.Sin(_walkPhase * 2f) : 1f), 1f);
            // 53차: 펫은 주인공 옆(뒤쪽 살짝)을 느긋하게 따라온다
            if (_petRt != null)
            {
                var want = _charPos + new Vector2(_charImg != null && _charImg.rectTransform.localScale.x < 0f ? 0.11f : -0.11f, -0.01f);
                want.x = Mathf.Clamp(want.x, 0.05f, 0.95f); want.y = Mathf.Clamp(want.y, 0.02f, 0.40f);
                _petPos = Vector2.MoveTowards(_petPos, want, 0.22f * dt);
                bool pw = (want - _petPos).magnitude > 0.004f;
                float pb = pw ? Mathf.Abs(Mathf.Sin(Time.unscaledTime * 9f)) * 6f : Mathf.Sin(Time.unscaledTime * 2f) * 2f;
                float pd = Mathf.Lerp(1.0f, 0.78f, _petPos.y / 0.42f);
                _petRt.anchorMin = _petRt.anchorMax = _petPos; _petRt.anchoredPosition = new Vector2(0f, pb); _petRt.localScale = new Vector3(pd, pd, 1f);
                if (_petImg != null && pw) _petImg.rectTransform.localScale = new Vector3(want.x < _petPos.x ? -1f : 1f, 1f, 1f);
            }
            if (_dragging == null) ReorderDepth();
        }

        /// 앞뒤 정렬: 벽걸이 → (바닥 가구·주인공을 y 내림차순: 멀수록 먼저) 순으로 형제 순서를 맞춘다.
        private void ReorderDepth()
        {
            if (_decoLayer == null) return;
            var floor = new List<(RectTransform rt, float y)>();
            int wallCount = 0;
            foreach (var kv in _placed)
            {
                var d = HomeData.Find(kv.Key);
                if (d != null && HomeData.IsWall(d)) { kv.Value.SetSiblingIndex(wallCount++); }
                else floor.Add((kv.Value, kv.Value.anchorMin.y));
            }
            if (_charRt != null && _charRt.parent == _decoLayer) floor.Add((_charRt, _charPos.y + 0.015f));
            if (_petRt != null && _petRt.parent == _decoLayer) floor.Add((_petRt, _petPos.y + 0.012f));
            floor.Sort((a, b) => b.y.CompareTo(a.y));
            for (int i = 0; i < floor.Count; i++) floor[i].rt.SetSiblingIndex(wallCount + i);
        }

        // ── 장식 그림(공용) ────────────────────────────────────────────────

        /// Resources/CoastRun/UI_Deco_<id>(마젠타 키 가능)가 있으면 그 그림, 없으면 색 알약(한자 없음).
        public static GameObject DecoVisual(Transform parent, DecoDef d, Vector2 size, bool shadow)
        {
            var root = new GameObject("Deco_" + d.id, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            if (shadow)
            {
                var sh = CoastHudLayout.MakeImage(root.transform, "Shadow", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                    new Vector2(-size.x * 0.38f, -4f), new Vector2(size.x * 0.38f, 10f), new Color(0f, 0f, 0f, 0.18f));
                sh.sprite = CoastUiArt.RoundedRect(20); sh.type = Image.Type.Sliced; sh.raycastTarget = false;
            }
            var tex = ArtAssets.LoadTexture("UI_Deco_" + d.id);
            if (tex != null)
            {
                var img = new GameObject("Img", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                img.transform.SetParent(root.transform, false);
                img.sprite = CoastUiArt.AsSprite(RaisingUI.ChromaKeyed(tex)); img.preserveAspect = true; img.raycastTarget = false;
                Rect(img.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            else
            {
                var pill = CoastUiArt.CutePill(root.transform, "Ph", d.color, 16, 3);
                pill.raycastTarget = false;
                Rect(pill.rectTransform, Vector2.zero, Vector2.one, new Vector2(size.x * 0.08f, size.y * 0.12f), new Vector2(-size.x * 0.08f, -size.y * 0.06f));
                // 한자 tag 대신 짧은 이름만(공간 있을 때)
                if (size.y >= 72f)
                {
                    var nm = Text(pill.transform, "Name", d.Name, Mathf.Clamp(Mathf.RoundToInt(size.y * 0.14f), 10, 16), new Color(0.18f, 0.12f, 0.10f), TextAnchor.MiddleCenter);
                    nm.fontStyle = FontStyle.Bold;
                    nm.resizeTextForBestFit = true; nm.resizeTextMinSize = 8; nm.resizeTextMaxSize = 16;
                    nm.horizontalOverflow = HorizontalWrapMode.Wrap;
                    Rect(nm.rectTransform, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.88f), Vector2.zero, Vector2.zero);
                }
            }
            return root;
        }

        // ── 헬퍼 ─────────────────────────────────────────────────────────

        private static void Rect(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = oMin; rt.offsetMax = oMax;
        }

        private static Text Text(Transform parent, string name, string s, int size, Color color, TextAnchor align)
        {
            var t = CoastHudLayout.MakeText(parent, name, s, size, align, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            t.color = color; return t;
        }

        private static Button Button(Transform parent, string name, string label, Color color, Vector2 anchor, Vector2 pos, Vector2 size, Action onClick)
        {
            var pill = CoastUiArt.CutePill(parent, name, color, 16, 3);
            pill.rectTransform.anchorMin = pill.rectTransform.anchorMax = anchor; pill.rectTransform.pivot = anchor;
            pill.rectTransform.anchoredPosition = pos; pill.rectTransform.sizeDelta = size;
            pill.raycastTarget = true;
            var b = pill.gameObject.AddComponent<Button>(); b.transition = Selectable.Transition.None;
            b.onClick.AddListener(() => { CoastPrefs.Vibrate(); onClick?.Invoke(); });
            var t = Text(pill.transform, "T", label, 16, Color.white, TextAnchor.MiddleCenter);
            t.resizeTextForBestFit = true; t.resizeTextMinSize = 12; t.resizeTextMaxSize = CoastHudLayout.Scaled(16);
            CoastUiArt.OutlineText(t, new Color(0f, 0f, 0f, 0.35f), 1.5f);
            return b;
        }

        private static ScrollRect MakeHScroll(Transform parent, Vector2 aMin, Vector2 aMax, Vector2 oMin, Vector2 oMax, out RectTransform content)
        {
            var go = new GameObject("HScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(RectMask2D));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); Rect(rt, aMin, aMax, oMin, oMax);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            var sr = go.GetComponent<ScrollRect>(); sr.horizontal = true; sr.vertical = false; sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 30f;
            content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(rt, false);
            content.anchorMin = new Vector2(0f, 0f); content.anchorMax = new Vector2(0f, 1f); content.pivot = new Vector2(0f, 0.5f);
            content.anchoredPosition = Vector2.zero; content.sizeDelta = new Vector2(100f, 0f);
            sr.content = content; sr.viewport = rt;
            return sr;
        }

        private static void Clear(Transform t)
        {
            if (t == null) return;
            for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject);
        }

        /// 탭(클릭) 위치를 넘겨주는 간단한 핸들러.
        private class PointerTap : MonoBehaviour, IPointerClickHandler
        {
            public Action<Vector2> OnTap;
            public void OnPointerClick(PointerEventData e) { if (e.dragging) return; OnTap?.Invoke(e.position); }
        }

        /// 가구 끌기 + 탭 구분.
        private class DecoDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
        {
            public Action<Vector2> OnDrag, OnEnd; public Action OnTap;
            private bool _dragged;
            public void OnBeginDrag(PointerEventData e) { _dragged = true; transform.SetAsLastSibling(); }
            void IDragHandler.OnDrag(PointerEventData e) => OnDrag?.Invoke(e.position);
            public void OnEndDrag(PointerEventData e) { OnEnd?.Invoke(e.position); }
            public void OnPointerClick(PointerEventData e) { if (_dragged) { _dragged = false; return; } OnTap?.Invoke(); }
        }

        /// 트레이 → 방 드래그. 위로 끌면 방에 놓고, 가로는 트레이 스크롤.
        private class TrayDecoDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            public ScrollRect Scroll;
            public Action OnBegin;
            public Action<Vector2> OnMove, OnEnd;
            private bool _armed;
            private Vector2 _start;
            private float _contentStart;
            public void OnBeginDrag(PointerEventData e)
            {
                _armed = false;
                _start = e.position;
                if (Scroll != null && Scroll.content != null)
                    _contentStart = Scroll.content.anchoredPosition.x;
            }
            public void OnDrag(PointerEventData e)
            {
                var d = e.position - _start;
                if (!_armed)
                {
                    if (d.y > 18f && Mathf.Abs(d.y) > Mathf.Abs(d.x) * 0.85f)
                    {
                        _armed = true;
                        if (Scroll != null) Scroll.enabled = false;
                        OnBegin?.Invoke();
                        OnMove?.Invoke(e.position);
                        return;
                    }
                    // 가로 스크롤 수동
                    if (Scroll != null && Scroll.content != null && Mathf.Abs(d.x) > 4f)
                    {
                        var pos = Scroll.content.anchoredPosition;
                        pos.x = _contentStart + d.x;
                        float minX = Mathf.Min(0f, Scroll.viewport.rect.width - Scroll.content.rect.width);
                        pos.x = Mathf.Clamp(pos.x, minX, 0f);
                        Scroll.content.anchoredPosition = pos;
                    }
                    return;
                }
                OnMove?.Invoke(e.position);
            }
            public void OnEndDrag(PointerEventData e)
            {
                if (Scroll != null) Scroll.enabled = true;
                if (_armed) OnEnd?.Invoke(e.position);
                _armed = false;
            }
        }
    }
}
