using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// 「빨리가자 제때 런」 타이틀 — 시안(UI_Title_JetteGate, 1008×2240 = 9:20) 한 장 + START 버튼 하나.
    /// START = K-POP 러닝(ArcadeRun.StartKpop, 챕터는 마지막 클리어 다음이 자동). 스토리·더보기 없음.
    public class JetteTitleController : MonoBehaviour
    {
        // 시안 픽셀(1008×2240, 좌상단 원점) → 배경 제작 좌표(720×1600, 중앙 원점). 배율 720/1008.
        private const float MockW = 1008f, MockH = 2240f;
        private const float BtnL = 122f, BtnT = 1905f, BtnW = 763f, BtnH = 266f;   // START 버튼 사각형(시안 픽셀)

        private Canvas _canvas;
        private CanvasGroup _uiCg;
        private TitleAudio _audio;
        private GameManager _gm;
        private RectTransform _btnRt;
        private bool _ready;
        private float _pulse;

        private void Start()
        {
            Application.targetFrameRate = 60;
            GameDirector.EnsureExists();
            _gm = GameManager.Ensure();
            _audio = gameObject.GetComponent<TitleAudio>() ?? gameObject.AddComponent<TitleAudio>();

            // 카메라는 UI 뒤 단색만(그림이 화면을 다 덮는다). 리스너·세로 뷰포트는 있어야 한다.
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("TitleCamera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
                cam.transform.position = new Vector3(0f, 0f, -10f);
            }
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.36f, 0.62f, 0.86f);
            cam.cullingMask = 0;
            if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();
            if (cam.GetComponent<CoastPortraitViewport>() == null) cam.gameObject.AddComponent<CoastPortraitViewport>();

            BuildUi();
            _audio.PlayMenu(false);
            StartCoroutine(FadeIn());
        }

        private void BuildUi()
        {
            _canvas = CoastUiCanvas.Create("JetteTitleCanvas", 100);
            var root = CoastUiCanvas.Root(_canvas);

            var gate = Resources.Load<Texture2D>(ArtAssets.ResourceRoot + "UI_Title_JetteGate");
            var btnTex = Resources.Load<Texture2D>(ArtAssets.ResourceRoot + "UI_Jette_StartBtn");

            // 배경: 9:20 그림을 화면 전체에(FullBleedBackground — 20:9 에서 딱, 16:9 에선 위아래만 잘림).
            Image bg = null;
            if (gate != null)
            {
                // 제작 좌표 720×1600 이 되도록 pixelsPerUnit 을 잡는다(1008 px → 720 유닛).
                var sprite = Sprite.Create(gate, new Rect(0f, 0f, gate.width, gate.height), new Vector2(0.5f, 0.5f), 100f);
                bg = CoastUiCanvas.FullBleedBackground(_canvas, "JetteGate", sprite);
                bg.preserveAspect = false;
                bg.raycastTarget = false;
            }

            var ui = new GameObject("TitleUI", typeof(RectTransform), typeof(CanvasGroup));
            ui.transform.SetParent(bg != null ? bg.rectTransform : root, false);
            var urt = ui.GetComponent<RectTransform>();
            urt.anchorMin = Vector2.zero; urt.anchorMax = Vector2.one; urt.offsetMin = Vector2.zero; urt.offsetMax = Vector2.zero;
            _uiCg = ui.GetComponent<CanvasGroup>();
            _uiCg.alpha = 0f;

            // START — 시안의 버튼 픽셀을 그대로 오려 낸 그림(UI_Jette_StartBtn)을 같은 자리에 겹친다.
            float k = CoastUiCanvas.BgDesignWidth / MockW;   // 0.714
            float cx = (BtnL + BtnW * 0.5f) * k - CoastUiCanvas.BgDesignWidth * 0.5f;
            float cy = CoastUiCanvas.BgDesignHeight * 0.5f - (BtnT + BtnH * 0.5f) * k;
            var btnGo = new GameObject("StartBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(ui.transform, false);
            _btnRt = btnGo.GetComponent<RectTransform>();
            _btnRt.anchorMin = _btnRt.anchorMax = new Vector2(0.5f, 0.5f);
            _btnRt.pivot = new Vector2(0.5f, 0.5f);
            _btnRt.anchoredPosition = bg != null ? new Vector2(cx, cy) : new Vector2(0f, -420f);
            _btnRt.sizeDelta = new Vector2(BtnW * k, BtnH * k);
            var im = btnGo.GetComponent<Image>();
            if (btnTex != null) { im.sprite = CoastUiArt.AsSprite(btnTex); im.preserveAspect = true; }
            else im.color = new Color(0.16f, 0.52f, 0.92f);
            im.raycastTarget = true;
            var b = btnGo.GetComponent<Button>();
            b.transition = Selectable.Transition.None;
            b.onClick.AddListener(OnStart);
            if (btnTex == null)
            {
                var lbl = CoastHudLayout.MakeText(_btnRt, "Label", "START", 64, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                lbl.color = new Color(1f, 0.85f, 0.2f); lbl.fontStyle = FontStyle.Bold;
            }
        }

        private IEnumerator FadeIn()
        {
            var dir = GameDirector.Instance;
            yield return null;
            dir?.UI?.Snap(0f, Color.black);   // 부트 장막 걷기
            float t = 0f;
            while (t < 0.45f)
            {
                t += Time.unscaledDeltaTime;
                if (_uiCg != null) _uiCg.alpha = Mathf.Clamp01(t / 0.45f);
                yield return null;
            }
            if (_uiCg != null) _uiCg.alpha = 1f;
            _ready = true;
        }

        private void Update()
        {
            if (_btnRt == null) return;
            // 살짝 숨 쉬는 버튼(시안의 반짝임 대신)
            _pulse += Time.unscaledDeltaTime;
            float s = _ready ? 1f + 0.025f * Mathf.Sin(_pulse * 2.6f) : 1f;
            _btnRt.localScale = new Vector3(s, s, 1f);
            if (_ready && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
                OnStart();
        }

        private void OnStart()
        {
            if (!_ready) return;
            _ready = false;
            _audio?.PlayStart();
            StartCoroutine(PressThenGo());
        }

        private IEnumerator PressThenGo()
        {
            float t = 0f;
            while (t < 0.14f)
            {
                t += Time.unscaledDeltaTime;
                float s = Mathf.Lerp(1f, 0.93f, Mathf.Sin(Mathf.Clamp01(t / 0.14f) * Mathf.PI));
                if (_btnRt != null) _btnRt.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
            ArcadeRun.StartKpop(_gm);
        }
    }
}
