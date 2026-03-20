using UnityEngine;

namespace ClimbGames.Core
{
    public static class GUIStyles
    {
        private static GUIStyle _sceneLabelStyle;
        private static GUIStyle _backgroundBoxStyle;

        public static GUIStyle SceneLabelStyle
        {
            get
            {
                if (_sceneLabelStyle == null)
                {
                    _sceneLabelStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 14,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(10, 10, 5, 5)
                    };
                    _sceneLabelStyle.normal.textColor = Color.white;
                }
                return _sceneLabelStyle;
            }
        }

        public static GUIStyle BackgroundBoxStyle
        {
            get
            {
                if (_backgroundBoxStyle == null)
                {
                    _backgroundBoxStyle = new GUIStyle(GUI.skin.box);
                    Texture2D tex = new Texture2D(1, 1);
                    tex.SetPixel(0, 0, new Color(0, 0, 0, 0.6f));
                    tex.Apply();
                    _backgroundBoxStyle.normal.background = tex;
                }
                return _backgroundBoxStyle;
            }
        }
    }
}