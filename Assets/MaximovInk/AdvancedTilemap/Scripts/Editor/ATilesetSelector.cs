using System;
using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class ATilesetSelector : EditorWindow
    {
        public ATileset tileset;
        public Action<ATileUV> SelectCallback;

        public static ATilesetSelector Init(ATileset tileset, Action<ATileUV> selectCallback)
        {
            ATilesetSelector window = (ATilesetSelector)GetWindow(typeof(ATilesetSelector));
            window.tileset = tileset;
            window.SelectCallback = selectCallback;

            float scaledValue = 200 * 1 * window.position.height / (220f);
            var wndPos = window.position;
            wndPos.width = scaledValue + 20f;
            window.position = wndPos;

            window.Show();
            return window;
        }

        private float previewScaler = 1f;
        private Vector2 scrollViewValue;

        private void OnGUI()
        {
            previewScaler = EditorGUILayout.Slider("Scale:", previewScaler, 0.2f, 10f);

            scrollViewValue = GUILayout.BeginScrollView(scrollViewValue, "helpBox", GUILayout.Height(position.height));

            var aspect = (float)tileset.Texture.height / tileset.Texture.width;

            float scaledValue = 200 * previewScaler * position.height/(220f);

            GUILayout.Label("", GUILayout.Width(scaledValue), GUILayout.Height(aspect * scaledValue));
            var rect = GUILayoutUtility.GetLastRect();

            EditorUtils.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1f));

            GUI.DrawTexture(rect, tileset.Texture);

            var width = tileset.Texture.width / tileset.TileSize.x;
            var height = tileset.Texture.height / tileset.TileSize.y;

            var tileUnitX = scaledValue / width;
            var tileUnitY = scaledValue / height * aspect;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var tileRect = new Rect(
                        rect.x+tileUnitX*i,
                        rect.y+tileUnitY*j,
                        tileUnitX,
                        tileUnitY
                        );

                    if (GUI.Button(tileRect, "", GUIStyle.none))
                    {
                        var uvMin = new Vector2(
                            i*tileset.TileTexUnit.x,
                            1f-(j+1)*tileset.TileTexUnit.y);
                        var uvMax = new Vector2(
                            uvMin.x+tileset.TileTexUnit.x,
                            uvMin.y+tileset.TileTexUnit.y
                            );

                        SelectCallback?.Invoke(new ATileUV { Min = uvMin, Max = uvMax });
                        Close();
                    }

                    EditorUtils.DrawRectWithOutline(tileRect, Color.clear, new Color(1, 1, 1, 0.2f));
                }
            }

            GUILayout.EndScrollView();
        }

    }
}
