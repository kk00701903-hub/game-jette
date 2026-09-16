using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoastRun
{
    /// DDOL transition overlay — solid fade / white flash only.
    /// First-boot must never show UI_Loading_Mock (로딩중) — that art is retired;
    /// the title scene plays Title_Bus.mp4 instead.
    /// Veil is parented to the Canvas (full bleed), not HudInset, so letterbox margins
    /// never show the live 3D world during a fade.
    public class UIRoot : MonoBehaviour
    {
        private Canvas _canvas;
        private Image _veil;
        private Image _loaderDot;
        private CanvasGroup _veilCg;

        public void EnsureBuilt()
        {
            if (_canvas != null)
                return;

            _canvas = CoastUiCanvas.Create("FlowUIRoot", 500);
            DontDestroyOnLoad(_canvas.gameObject);

            // Full-screen under the canvas root — NOT under PortraitSafeArea/HudInset.
            var veilGo = new GameObject("Veil", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            veilGo.transform.SetParent(_canvas.transform, false);
            veilGo.transform.SetAsLastSibling();
            var rt = veilGo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _veil = veilGo.GetComponent<Image>();
            _veil.sprite = null;
            _veil.color = Color.black;
            _veil.raycastTarget = true;
            _veil.preserveAspect = false;
            _veilCg = veilGo.GetComponent<CanvasGroup>();
            _veilCg.alpha = 0f;
            _veilCg.blocksRaycasts = false;

            var tipParent = CoastUiCanvas.Root(_canvas);
            var dotGo = new GameObject("LoaderDot", typeof(RectTransform), typeof(Image));
            dotGo.transform.SetParent(tipParent != null ? tipParent : veilGo.transform, false);
            var drt = dotGo.GetComponent<RectTransform>();
            drt.anchorMin = drt.anchorMax = new Vector2(0.5f, 0.12f);
            drt.sizeDelta = new Vector2(10f, 10f);
            _loaderDot = dotGo.GetComponent<Image>();
            _loaderDot.color = new Color(1f, 1f, 1f, 0.35f);
            _loaderDot.raycastTarget = false;
            SetLoader(false);
        }

        private void ApplySolid(Color c)
        {
            if (_veil == null) return;
            _veil.sprite = null;
            _veil.color = c;
        }

        /// 씬 전환 페이드 베일 알파(1=완전 가림). 챕터 시작 연출은 이게 내려간 뒤에 띄운다.
        public float VeilAlpha
        {
            get
            {
                EnsureBuilt();
                return _veilCg != null ? _veilCg.alpha : 0f;
            }
        }

        /// 18차-5: 페이드 코루틴이 중간에 끊겨 '투명한데 입력만 막는' 베일이 남지 않게,
        /// 매 프레임 알파와 레이캐스트 차단을 맞춘다(알파 1% 이하 = 통과).
        private void LateUpdate()
        {
            if (_veilCg == null) return;
            bool block = _veilCg.alpha > 0.01f;
            if (_veilCg.blocksRaycasts != block) _veilCg.blocksRaycasts = block;
            if (block && _veil != null && _veil.transform.GetSiblingIndex() != _veil.transform.parent.childCount - 1)
                _veil.transform.SetAsLastSibling();
        }

        public void SetLoader(bool on)
        {
            if (_loaderDot != null)
                _loaderDot.enabled = on;
        }

        /// null → solid black. Pass a non-black color for flash/tint covers.
        public IEnumerator Fade(float from, float to, float duration, Color? color = null)
        {
            EnsureBuilt();
            ApplySolid(color ?? Color.black);

            _veilCg.blocksRaycasts = true;
            float t = 0f;
            duration = Mathf.Max(0.01f, duration);
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / duration);
                _veilCg.alpha = Mathf.Lerp(from, to, u);
                yield return null;
            }

            _veilCg.alpha = to;
            _veilCg.blocksRaycasts = to > 0.01f;
            if (to <= 0.01f)
                ApplySolid(Color.black);
        }

        public IEnumerator WhiteFlash(float flashSeconds, float fadeSeconds)
        {
            EnsureBuilt();
            ApplySolid(Color.white);
            _veilCg.alpha = 1f;
            _veilCg.blocksRaycasts = true;
            float t = 0f;
            while (t < flashSeconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return Fade(1f, 0f, fadeSeconds, Color.white);
            ApplySolid(Color.black);
        }

        public void Snap(float alpha, Color? color = null)
        {
            EnsureBuilt();
            ApplySolid(color ?? Color.black);
            _veilCg.alpha = alpha;
            _veilCg.blocksRaycasts = alpha > 0.01f;
        }
    }
}
