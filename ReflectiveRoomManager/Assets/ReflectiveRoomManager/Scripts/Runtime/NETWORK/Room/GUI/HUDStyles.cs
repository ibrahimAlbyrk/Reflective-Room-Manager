using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room.GUI
{
    /// <summary>
    /// Shared GUI styles for Room Manager HUD components.
    /// </summary>
    public static class HUDStyles
    {
        public static GUIStyle BoxStyle { get; private set; }
        public static GUIStyle HeaderStyle { get; private set; }
        public static GUIStyle TabStyle { get; private set; }
        public static GUIStyle TabActiveStyle { get; private set; }

        private static bool _initialized;

        public static void Init()
        {
            if (_initialized) return;

            BoxStyle = new GUIStyle(UnityEngine.GUI.skin.box)
            {
                normal = { background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.85f)) },
                padding = new RectOffset(8, 8, 8, 8)
            };

            HeaderStyle = new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            TabStyle = new GUIStyle(UnityEngine.GUI.skin.button)
            {
                fixedHeight = 25,
                margin = new RectOffset(2, 2, 2, 2)
            };

            TabActiveStyle = new GUIStyle(UnityEngine.GUI.skin.button)
            {
                fixedHeight = 25,
                margin = new RectOffset(2, 2, 2, 2),
                fontStyle = FontStyle.Bold
            };
            TabActiveStyle.normal.textColor = new Color(0.4f, 1f, 0.4f);
            TabActiveStyle.hover.textColor = new Color(0.5f, 1f, 0.5f);

            _initialized = true;
        }

        public static string FormatTime(float sec)
        {
            var m = Mathf.FloorToInt(sec / 60f);
            var s = Mathf.FloorToInt(sec % 60f);
            return $"{m:D2}:{s:D2}";
        }

        /// <summary>
        /// Creates a 1x1 solid color texture for GUI drawing.
        /// </summary>
        public static Texture2D MakeColorTex(Color c) => MakeTex(2, 2, c);

        private static Texture2D MakeTex(int w, int h, Color c)
        {
            var pix = new Color[w * h];
            for (var i = 0; i < pix.Length; i++) pix[i] = c;
            var tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}
