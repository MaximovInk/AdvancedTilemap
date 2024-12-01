using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MaximovInk.AdvancedTilemap
{
    public class EditorUtils
    {
        public static void DrawRect(Rect rect, Color color)
        {
            DrawRectWithOutline(rect, color, color);
        }

        public static void DrawRectWithOutline(Rect rect, Color color, Color colorOutline)
        {
            /*Vector3[] rectVerts = {
            new(rect.x, rect.y, 0),
            new(rect.x + rect.width, rect.y, 0),
            new(rect.x + rect.width, rect.y + rect.height, 0),
            new(rect.x, rect.y + rect.height, 0) };
            Handles.DrawSolidRectangleWithOutline(rectVerts, color, colorOutline);*/

           // EditorGUI.DrawRect(rect, colorOutline);

            DrawBorderRect(rect, colorOutline, 3f);
        }

        private const float MAX_SCALE_H = 500f;
        private const float MIN_SCALE_H = 300f;

        public const float PREVIEW_SCALE_DEFAULT = 1f;


        public static void DrawBorderRect(Rect area, Color color, float borderWidth)
        {
            //------------------------------------------------
            float x1 = area.x;
            float y1 = area.y;
            float x2 = area.width;
            float y2 = borderWidth;

            Rect lineRect = new Rect(x1, y1, x2, y2);

            EditorGUI.DrawRect(lineRect, color);

            //------------------------------------------------
            x1 = area.x + area.width;
            y1 = area.y;
            x2 = borderWidth;
            y2 = area.height;

            lineRect = new Rect(x1, y1, x2, y2);

            EditorGUI.DrawRect(lineRect, color);

            //------------------------------------------------
            x1 = area.x;
            y1 = area.y;
            x2 = borderWidth;
            y2 = area.height;

            lineRect = new Rect(x1, y1, x2, y2);

            EditorGUI.DrawRect(lineRect, color);

            //------------------------------------------------
            x1 = area.x;
            y1 = area.y + area.height;
            x2 = area.width;
            y2 = borderWidth;

            lineRect = new Rect(x1, y1, x2, y2);

            EditorGUI.DrawRect(lineRect, color);
        }

        public static void DrawPreviewTileset(ATileset tileset, ref ushort selectedTile, ref float previewScaler, ref Vector2 scrollViewValue, ref bool showAsList, float height = 300f, float lOffset = 20f, float rOffset = 20f)
        {
            if (tileset == null || tileset.Texture == null) return;

            if (showAsList)
            {
                scrollViewValue = GUILayout.BeginScrollView(scrollViewValue);
                GUILayout.BeginVertical();

                for (int i = 1; i <= tileset.TilesCount; i++)
                {
                    if (GUILayout.Button($"{tileset.GetTile(i).ID}) Tile [{tileset.GetTile(i).TileDriverID}]"))
                    {
                        selectedTile = (ushort)i;
                    }
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            else
            {
                previewScaler = EditorGUILayout.Slider("Scale:", previewScaler, 0.2f, 10f);

                var newHeight = Mathf.Clamp(height * previewScaler, MIN_SCALE_H, MAX_SCALE_H);

                if (GUILayout.Button("Reset scale"))
                {
                    previewScaler = PREVIEW_SCALE_DEFAULT;
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(lOffset);

                    using (new GUILayout.VerticalScope())
                    {
                        scrollViewValue = GUILayout.BeginScrollView(scrollViewValue, false, false, GUILayout.Height(newHeight));

                        var aspect = (float)tileset.Texture.height / tileset.Texture.width;

                        int scaledValue = (int)(height * previewScaler);

                        GUILayout.Label("");
                        var rectView = GUILayoutUtility.GetLastRect();

                        var rW = scaledValue / aspect - 25f;
                        var rH = scaledValue - 25f;

                        GUILayout.Label("", GUILayout.Width(rW), GUILayout.Height(rH));

                        var rect = GUILayoutUtility.GetLastRect();

                        rect.x += rectView.width / 2f - rect.width / 2f;

                        DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1f));

                        GUI.DrawTexture(rect, tileset.Texture);

                        for (int i = 1; i <= tileset.TilesCount; i++)
                        {
                            var tile = tileset.GetTile(i);

                            if (tile.ID == selectedTile)
                                DrawPreviewTile(rect, tile, new Color(1, 1, 1, 0.5f), new Color(0, 0, 0, 0.2f), ref selectedTile);

                            else
                                DrawPreviewTile(rect, tile, new Color(1, 1, 1, 0.5f), Color.clear, ref selectedTile);
                        }

                        if (selectedTile > 0 && selectedTile <= tileset.TilesCount)
                        {
                            var tile = tileset.GetTile(selectedTile);

                            DrawPreviewTile(rect, tile, Color.red, Color.clear, ref selectedTile);

                            for (int i = 1; i < tile.Variations.Count; i++)
                            {
                                DrawPreviewTile(rect, tile.Variations[i], new Color(0, 0, 1, 0.5f), Color.clear);
                            }
                        }

                        GUILayout.EndScrollView();
                    }

                    GUILayout.Space(rOffset);
                }

                


 
            }
           
        }

        public static void DrawPreviewTile(Rect rect, ATileUV uv, Color color, Color fillColor)
        {
            var uvSize = uv.Max - uv.Min;

            var tileRect = new Rect(
                (int)(rect.x + uv.Min.x * rect.width),
                (int)(rect.y + rect.height - uv.Max.y * rect.height),
                (int)(uvSize.x * rect.width),
                (int)(uvSize.y * rect.height)
                );

            DrawRectWithOutline(tileRect, fillColor, color);
        }

        public static void DrawPreviewTile(Rect rect, ATile tile, Color color, Color fillColor, ref ushort selectedTile)
        {
            var uv = tile.GetUV();
            var uvSize = uv.Max - uv.Min;

            var tileRect = new Rect(
                (int)(rect.x + uv.Min.x * rect.width),
                (int)(rect.y + rect.height - uv.Max.y * rect.height),
                (int)(uvSize.x * rect.width),
                (int)(uvSize.y * rect.height)
                );

            if (GUI.Button(tileRect, "", GUIStyle.none))
            {
                selectedTile = selectedTile == tile.ID ? (ushort)0 : tile.ID;
            }

            DrawRectWithOutline(tileRect, fillColor, color);
        }

        public static void SaveAsset(Object obj)
        {
            if (obj == null) return;

            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
