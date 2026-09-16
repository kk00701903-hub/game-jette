using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace CoastRun
{
    /// Keeps Android system status bar off the run HUD.
    /// Player Settings already request insets hidden, but OEMs re-show a translucent
    /// status bar after Home/resume — it then overlays Pause / weather / score.
    public static class CoastSystemBars
    {
        public static void ApplyImmersive()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var insets = AndroidApplication.currentWindowInsets;
                if (insets == null) return;
                insets.SetSystemBarsBehavior(AndroidWindowInsets.SystemBarsBehavior.ShowTransientBarsBySwipe);
                insets.Hide(AndroidWindowInsets.Type.StatusBars);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[CoastSystemBars] " + e.Message);
            }
#endif
        }

        public static bool StatusBarVisible
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                try
                {
                    var insets = AndroidApplication.currentWindowInsets;
                    return insets != null && insets.IsVisible(AndroidWindowInsets.Type.StatusBars);
                }
                catch { return false; }
#else
                return false;
#endif
            }
        }
    }
}
