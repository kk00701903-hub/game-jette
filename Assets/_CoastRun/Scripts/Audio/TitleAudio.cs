using System.Collections;
using UnityEngine;

namespace CoastRun
{
    /// Title-screen BGM + UI SFX (procedural until real clips land).
    /// 48차-6(사용자): 타이틀 BGM(M5)은 앱을 켜자마자(00_Boot) 시작하고, 씬이 바뀌어도 살아남는 전역 소스(DontDestroyOnLoad)에서
    /// 계속 나온다. PlayMenu 는 같은 곡이 이미 나오고 있으면 끊지 않는다. StopMenu 는 러닝/육성으로 나갈 때만.
    public class TitleAudio : MonoBehaviour
    {
        private const float BedVol = 0.85f;
        private static AudioSource s_bgm;
        private static MonoBehaviour s_fadeHost;
        private static Coroutine s_fadeCo;
        private AudioSource _bgm => EnsureBgm();
        private AudioSource _sfx;
        private AudioClip _click;
        private AudioClip _start;
        private bool _cleared;

        private static AudioSource EnsureBgm()
        {
            if (s_bgm != null) return s_bgm;
            var go = new GameObject("TitleBgm(Global)");
            Object.DontDestroyOnLoad(go);
            s_bgm = go.AddComponent<AudioSource>();
            s_bgm.playOnAwake = false;
            s_bgm.spatialBlend = 0f;
            s_fadeHost = go.AddComponent<TitleBgmFader>();
            return s_bgm;
        }

        private static MonoBehaviour FadeHost()
        {
            EnsureBgm();
            if (s_fadeHost == null) s_fadeHost = s_bgm.GetComponent<TitleBgmFader>() ?? s_bgm.gameObject.AddComponent<TitleBgmFader>();
            return s_fadeHost;
        }

        /// 00_Boot 에서 호출 — 리스너·타이틀 UI 가 뜨기 전에 곡부터 튼다.
        public static void PlayMenuEarly()
        {
            var src = EnsureBgm();
            var real = CoastBgmLibrary.Load(CoastBgmLibrary.Menu(false));
            if (real == null || (src.isPlaying && src.clip == real)) return;
            StopFade();
            src.clip = real; src.volume = BedVol; src.loop = true; src.Play();
        }

        public void PlayMenu(bool cleared)
        {
            _cleared = cleared;
            Ensure();
            PlayMenuClip(cleared, fadeIn: false);
        }

        /// 메인 메뉴 BGM(M5)을 켠다. fadeIn 이면 볼륨 0→BedVol.
        public static void PlayMenuClip(bool cleared, bool fadeIn)
        {
            var src = EnsureBgm();
            var real = CoastBgmLibrary.Load(CoastBgmLibrary.Menu(cleared));
            if (real == null) { StopFade(); if (src.isPlaying) src.Stop(); src.clip = null; return; }
            if (src.isPlaying && src.clip == real)
            {
                if (fadeIn) FadeTo(BedVol, 0.55f);
                else { StopFade(); src.volume = BedVol; }
                return;
            }
            StopFade();
            src.clip = real;
            src.loop = true;
            if (fadeIn) { src.volume = 0f; src.Play(); FadeTo(BedVol, 0.55f); }
            else { src.volume = BedVol; if (!src.isPlaying) src.Play(); else { src.Stop(); src.Play(); } }
        }

        /// 스토리 모드(육성) 화면 BGM — M13 「하늘의 약속」.
        public static void PlayRaising()
        {
            var src = EnsureBgm();
            var real = CoastBgmLibrary.Load(CoastBgmLibrary.RaisingHub());
            if (real == null) { StopFade(); if (src.isPlaying) src.Stop(); src.clip = null; return; }
            if (src.isPlaying && src.clip == real) { StopFade(); src.volume = BedVol; return; }
            StopFade();
            src.clip = real;
            src.volume = BedVol;
            src.loop = true;
            src.Play();
        }

        /// 더보기 열 때 — 메인 BGM 페이드아웃(클립은 유지, 볼륨만 0).
        public static void FadeMenuOut(float dur = 0.55f)
        {
            if (s_bgm == null || !s_bgm.isPlaying) return;
            FadeTo(0f, dur);
        }

        /// 더보기 닫을 때 — 메인 BGM 다시 들리게(필요하면 클립 복구 후 페이드인).
        public static void FadeMenuIn(bool cleared, float dur = 0.55f)
        {
            var src = EnsureBgm();
            var real = CoastBgmLibrary.Load(CoastBgmLibrary.Menu(cleared));
            if (real == null) return;
            if (src.clip != real || !src.isPlaying)
            {
                src.clip = real; src.loop = true; src.volume = 0f; src.Play();
            }
            FadeTo(BedVol, dur);
        }

        /// 레코드 미리듣기 등 — 허브 BGM만 잠시 죽임(클립 유지).
        public static void SetBedVolume(float vol)
        {
            StopFade();
            if (s_bgm != null) s_bgm.volume = Mathf.Clamp01(vol);
        }

        public void StopMenu() => StopMenuGlobal();

        /// Always force-stop the DDOL title bed. On mobile, Output Suspension can leave
        /// isPlaying==false while the source is still a live voice — gating on isPlaying
        /// then no-ops, and resume mixes title M5 with run/K-POP BGM.
        public static void StopMenuGlobal()
        {
            StopFade();
            if (s_bgm == null) return;
            s_bgm.Stop();
            s_bgm.clip = null;
            s_bgm.volume = 0f;
        }

        public void PlayClick()
        {
            Ensure();
            _sfx.PlayOneShot(_click, 0.4f);
        }

        public void PlayStart()
        {
            Ensure();
            _sfx.PlayOneShot(_start, 0.55f);
        }

        private static void FadeTo(float target, float dur)
        {
            var host = FadeHost();
            if (s_fadeCo != null) host.StopCoroutine(s_fadeCo);
            s_fadeCo = host.StartCoroutine(FadeCo(target, dur));
        }

        private static void StopFade()
        {
            if (s_fadeCo == null || s_fadeHost == null) { s_fadeCo = null; return; }
            s_fadeHost.StopCoroutine(s_fadeCo);
            s_fadeCo = null;
        }

        private static IEnumerator FadeCo(float target, float dur)
        {
            var src = s_bgm;
            if (src == null) { s_fadeCo = null; yield break; }
            float from = src.volume;
            if (dur <= 0.01f) { src.volume = target; s_fadeCo = null; yield break; }
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                if (src == null) break;
                src.volume = Mathf.Lerp(from, target, Mathf.Clamp01(t / dur));
                yield return null;
            }
            if (src != null) src.volume = target;
            s_fadeCo = null;
        }

        private void Ensure()
        {
            EnsureBgm();

            if (_sfx == null)
            {
                var go = new GameObject("TitleSfx");
                go.transform.SetParent(transform, false);
                _sfx = go.AddComponent<AudioSource>();
                _sfx.playOnAwake = false;
                _sfx.spatialBlend = 0f;
            }

            if (_click == null)
                _click = ProceduralAudio.CreateBlip(660f, 0.04f);
            if (_start == null)
                _start = ProceduralAudio.CreateBlip(440f, 0.12f);
        }

        /// DDOL BGM 페이드용(로직 없는 호스트).
        private class TitleBgmFader : MonoBehaviour { }
    }
}
