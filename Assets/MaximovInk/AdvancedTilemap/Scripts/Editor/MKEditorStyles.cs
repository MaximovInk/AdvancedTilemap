using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public static class MKEditorStyles
    {
        private static int[] PaintIconArray = new[]
        {
            0,0,0,0,0,0,0,0,
            0,1,1,0,0,0,0,0,
            0,1,0,1,0,0,0,0,
            0,0,1,0,1,0,0,0,
            0,0,0,1,0,1,0,0,
            0,0,0,0,1,1,1,0,
            0,0,0,0,0,1,1,0,
            0,0,0,0,0,0,0,0,
        };

        private static int[] RectArray = new[]
        {
            0,0,0,0,0,0,0,0,
            0,1,1,1,1,1,1,0,
            0,1,0,0,0,0,1,0,
            0,1,0,0,0,0,1,0,
            0,1,0,0,0,0,1,0,
            0,1,0,0,0,0,1,0,
            0,1,1,1,1,1,1,0,
            0,0,0,0,0,0,0,0,
        };


        private static int[] LineArray = new[]
        {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,1,0,
            0,0,0,0,0,1,0,0,
            0,0,0,0,1,0,0,0,
            0,0,0,1,0,0,0,0,
            0,0,1,0,0,0,0,0,
            0,1,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
        };

        private static int[] EmptyArray = new[]
        {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
        };

        //private static Texture2D _paintIcon;


        //private static Texture2D _eraseIcon;

        private static EditorTexButton _paint;
        private static EditorTexButton _rect;
        private static EditorTexButton _line;


        private static Color SceneButtonActive = new(1,1,1);
        private static Color SceneButtonNormal = new Color(0.5f,0.5f,0.5f);
        private static Color SceneButtonHover = new Color(0.8f,0.8f,0.8f);
        private static Color SceneButtonBackground = new Color(0.2f,0.2f,0.2f);

        private static int SceneButtonSize = 40;

        public static GUIStyle[] Styles;

        private static void ValidateButton(ref EditorTexButton button, int[] array)
        {
            if(button is not { IsValid: true })
                button = new EditorTexButton(
                    array, 8,
                    SceneButtonActive,
                    SceneButtonHover,
                    SceneButtonNormal,
                    SceneButtonBackground);

        }

        public static EditorTexButton GetPaintIcon()
        {
            ValidateButton(ref _paint, PaintIconArray);

            return _paint;
        }

        public static EditorTexButton GetRectIcon()
        {
            ValidateButton(ref _rect, RectArray);

            return _rect;
        }

        public static EditorTexButton GetLineIcon()
        {
            ValidateButton(ref _line, LineArray);

            return _line;
        }


        public static bool Button(EditorTexButton button, bool active = false)
        {
            var style = new GUIStyle(button.GetStyle());

              if (active)
              {
                  style.hover = style.active;
                  style.normal = style.active;
              }

            return GUILayout.Button(
                string.Empty,
                style,
                GUILayout.Width(SceneButtonSize),
                GUILayout.Height(SceneButtonSize));
        }

        public static Texture2D GetTexture(int[] array, int size, Color f, Color b)
        {
            var tex = new Texture2D(size, size);

            var colors = new Color32[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                colors[i] = array[array.Length - 1 - i] > 0 ? f : b;
            }

            tex.SetPixels32(colors);

            tex.filterMode = FilterMode.Point;

            tex.Apply();

            return tex;
        }

        public static void ValidateStyles()
        {
            if (Styles != null && Styles.Length == 2) return;

            Styles = new GUIStyle[]
            {
                new("helpBox")  { richText = true },
                new()
                {
                    normal = { textColor = Color.white },
                    richText = true,
                }

            };
        }
    }
}
