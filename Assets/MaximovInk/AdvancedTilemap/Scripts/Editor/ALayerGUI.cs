using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public struct ALayerEditorData
    {
        public int SelectedToolbar;
        public int LastSelectedToolbar;
        public AToolEditor Tool;
        public ALayer Layer;
        public ushort selectedTile;
        public float PreviewScale;
        public Vector2 TilesetScrollView;
        public Event Event;
        public PaintPreview PreviewTextureBrush;
        public int brushSize;
        public Color color;
        public UVTransform UVTransform;
        public Vector2Int gridPos;
    }

    public static class ALayerGUI
    {
        private static bool _isDirty;

        public static bool DrawGUI(ALayer layer, ref ALayerEditorData data)
        {
            _isDirty = false;

            BeginChangeCheck();

            data.Layer = layer;
            data.Event = Event.current;


            GUILayout.BeginVertical("helpBox");

            GUILayout.Label($"{layer.name}");

            layer.Tileset = (ATileset)EditorGUILayout.ObjectField("Tileset", layer.Tileset, typeof(ATileset), false);
            layer.Material = (Material)EditorGUILayout.ObjectField("Material", layer.Material, typeof(Material), false);
            layer.LayerMask = EditorGUILayout.LayerField("Layer:", layer.LayerMask);
            layer.Tag = EditorGUILayout.TagField("Layer:", layer.gameObject.tag);

            if (data.selectedTile == 0 && layer.Tileset != null && layer.Tileset.TilesCount > 0)
                data.selectedTile = 1;

            data.SelectedToolbar =
                GUILayout.Toolbar(data.SelectedToolbar, new string[] { "All", "Paint", "Map", "Renderer", "Liquid", "Collision" });

            if (data.LastSelectedToolbar != data.SelectedToolbar && data.SelectedToolbar == 0)
            {
                Tools.current = Tool.None;
                data.LastSelectedToolbar = data.SelectedToolbar;
                data.Tool = new TileBrushToolEditor();
                data.selectedTile = 0;
            }

            if (GUILayout.Button("Refresh"))
            {
                layer.Refresh(true);
            }

            GUILayout.Space(20);

            GUILayout.BeginVertical("helpBox");

            switch (data.SelectedToolbar)
            {
                case 0:
                    DrawPaintBox(ref data);
                    DrawMapBox(ref data);
                    DrawRendererBox(ref data);
                    DrawLiquidBox(ref data);
                    DrawColliderBox(ref data);
                    break;
                case 1:
                    DrawPaintBox(ref data);
                    break;
                case 2:
                    DrawMapBox(ref data);
                    break;
                case 3:
                    DrawRendererBox(ref data);
                    break;
                case 4:
                    DrawLiquidBox(ref data);
                    break;
                case 5:
                    DrawColliderBox(ref data);
                    break;
            }

            GUILayout.EndVertical();


            GUILayout.EndVertical();

            EndChangeCheck();

            return _isDirty;
        }

        public static void SceneGUI(ALayer layer, ref ALayerEditorData data)
        {
            if (data.selectedTile == 0) return;

            OnSceneGUIUndo(layer, ref data);
            OnSceneGUIPaint(layer, ref data);
        }

        public static void Enable(ref ALayerEditorData data)
        {
            data.color = Color.white;
            data.selectedTile = 0;

            DestroyTexturePreview(ref data);
        }

        public static void Disable(ref ALayerEditorData data)
        {
            DestroyTexturePreview(ref data);

        }

        private static void OnSceneGUIUndo(ALayer layer, ref ALayerEditorData data)
        {
            if (layer.IsUndoEnabled)
            {
                Handles.BeginGUI();

                float offset = 120;

                GUILayout.BeginArea(new Rect(0, Screen.height - offset, 90, offset));

                if (GUILayout.Button("Undo"))
                {
                    layer.Undo();
                    layer.Refresh();
                    EditorUtility.SetDirty(layer);
                }
                if (GUILayout.Button("Redo"))
                {
                    layer.Redo();
                    layer.Refresh();
                    EditorUtility.SetDirty(layer);
                }

                GUILayout.EndArea();

                Handles.EndGUI();
            }


        }

        private static bool OnSceneGUIPaint(ALayer layer, ref ALayerEditorData data)
        {
            bool repaint = false;

            if (Tools.current != Tool.None)
                return repaint;



            if (layer.Tileset == null) return repaint;

            if (!(data.selectedTile > 0 && data.selectedTile < layer.Tileset.TilesCount+1))
                return repaint;

            if (data.SelectedToolbar == 0 || data.SelectedToolbar == 1)
            {
                Event e = Event.current;
                data.Event = e;

                var isShift = e.shift;

                switch (e.type)
                {
                    case EventType.KeyDown:
                        {
                            if (e.keyCode == KeyCode.R)
                            {
                                data.UVTransform._rot90 = !data.UVTransform._rot90;
                                e.Use();
                                repaint = true;
                            }
                            if (e.keyCode == KeyCode.H)
                            {
                                data.UVTransform._flipHorizontal = !data.UVTransform._flipHorizontal;
                                e.Use();
                                repaint = true;
                            }
                            if (e.keyCode == KeyCode.V)
                            {
                                data.UVTransform._flipVertical = !data.UVTransform._flipVertical;
                                e.Use();
                                repaint = true;
                            }
                        }
                        break;
                }

                Handles.BeginGUI();

                var buttonStyle = new GUIStyle(GUI.skin.button);
                var selectedButtonStyle = new GUIStyle(GUI.skin.button);
                selectedButtonStyle.normal.textColor = Color.blue;

                if (data.Tool == null)
                    data.Tool = new TileBrushToolEditor();

                GUILayout.BeginVertical(GUILayout.MaxWidth(90));

                if (GUILayout.Button(
                    "Brush",
                    (data.Tool is TileBrushToolEditor) ? selectedButtonStyle : buttonStyle))
                    data.Tool = new TileBrushToolEditor();

                if (GUILayout.Button(
                    "Line",
                    (data.Tool is LineBrushToolEditor) ? selectedButtonStyle : buttonStyle))
                    data.Tool = new LineBrushToolEditor();

                if (GUILayout.Button(
                   "FillRect",
                   (data.Tool is RectBrushToolEditor) ? selectedButtonStyle : buttonStyle))
                    data.Tool = new RectBrushToolEditor();


                GUILayout.EndVertical();

                Handles.EndGUI();

                if (e.type == EventType.Layout)
                    HandleUtility.AddDefaultControl(0);

                if (data.Tool != null)
                {
                    if (e.shift && !isShift)
                    {
                        isShift = true;
                        GenPreviewTextureBrush(ref data);
                    }
                    else if (!e.shift && isShift)
                    {
                        isShift = false;
                        GenPreviewTextureBrush(ref data);
                    }

                    Vector3 mousePosition = e.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                    mousePosition = ray.origin;

                    data.gridPos = Utilites.ConvertGlobalCoordsToGrid(layer, mousePosition);

                    if (!UpdatePreviewBrushPos(ref data))
                        GenPreviewTextureBrush(ref data);

                    data.Tool.Update(ref data);
                }
            }

            return repaint;
        }

        private static bool UpdatePreviewBrushPos(ref ALayerEditorData data)
        {
            if (data.PreviewTextureBrush == null)
                return false;

            var position = data.Layer.transform.TransformPoint(data.gridPos * data.Layer.Tileset.GetTileUnit());

            position.z = data.Layer.transform.position.z - 1;

            data.PreviewTextureBrush.transform.position = position;
            if (data.brushSize > 1)
            {
                data.PreviewTextureBrush.transform.position = (Vector3)position - data.PreviewTextureBrush.transform.localScale / 2f + new Vector3(0.5f, 0.5f, 0);
            }

            return true;
        }

        private static void DestroyTexturePreview(ref ALayerEditorData data)
        {
            if (data.PreviewTextureBrush != null)
                Object.DestroyImmediate(data.PreviewTextureBrush.gameObject);
        }

        public static void GenPreviewTextureBrush(ref ALayerEditorData data)
        {
            if (data.Layer.Tileset == null)
            {
                DestroyTexturePreview(ref data);

                return;
            }

            if (data.PreviewTextureBrush == null)
            {
                var go = new GameObject();
                go.name = "_PreviewPaintBrush";
                data.PreviewTextureBrush = go.AddComponent<PaintPreview>();

                data.PreviewTextureBrush.Validate();
            }

            var isShift = data.Event.shift;

            data.PreviewTextureBrush.GenerateBlock(data.brushSize, new ATileDriverData()
            {
                tileset = data.Layer.Tileset,
                tile = data.Layer.Tileset.GetTile(data.selectedTile),
                tileData = data.UVTransform,
                color = (isShift ? Color.red : (Color)data.color),
                blend = true,
                variation = 0,
            });

            data.PreviewTextureBrush.SetMaterial(data.Layer.Material, data.Layer.Tileset.Texture);
        }

        private static void DrawLiquidBox(ref ALayerEditorData data)
        {
            var layer = data.Layer;

            layer.LiquidEnabled = EditorGUILayout.Toggle("Liquid enabled", layer.LiquidEnabled);

            if (layer.LiquidEnabled)
            {
                layer.LiquidMaterial = (Material)EditorGUILayout.ObjectField("Material", layer.LiquidMaterial, typeof(Material), false);
                layer.MinLiquidColor = EditorGUILayout.ColorField("Min Liquid color:", layer.MinLiquidColor);
                layer.MaxLiquidColor = EditorGUILayout.ColorField("Max Liquid color:", layer.MaxLiquidColor);
            }
        }

        private static void DrawPaintBox(ref ALayerEditorData data)
        {
            var layer = data.Layer;
            GUILayout.Label("Paint:");
            Tools.current = Tool.None;
            EditorGUI.BeginChangeCheck();

            if (layer.Tileset != null)
                EditorUtils.DrawPreviewTileset(layer.Tileset, ref data.selectedTile, ref data.PreviewScale, ref data.TilesetScrollView);

            data.color = EditorGUILayout.ColorField("Paint color:", data.color);
            data.brushSize = EditorGUILayout.IntSlider("Brush size:", data.brushSize, 1, 32);
            data.UVTransform._rot90 = EditorGUILayout.Toggle("rot90", data.UVTransform._rot90);
            data.UVTransform._flipVertical = EditorGUILayout.Toggle("flipVertical", data.UVTransform._flipVertical);
            data.UVTransform._flipHorizontal = EditorGUILayout.Toggle("flipHorizontal", data.UVTransform._flipHorizontal);
            if (EditorGUI.EndChangeCheck())
                GenPreviewTextureBrush(ref data);
            
        }

        private static void DrawMapBox(ref ALayerEditorData data)
        {
            var layer = data.Layer;
            GUILayout.Label("Map:");
            if (GUILayout.Button("Clear"))
            {
                layer.BeginRecordingCommand();
                layer.Clear();
                layer.EndRecordCommand();
            }
            layer.UpdateVariationsOnRefresh = EditorGUILayout.Toggle("Update variations on refresh", layer.UpdateVariationsOnRefresh);
            layer.ShowChunkBounds = EditorGUILayout.Toggle("Show chunk bounds", layer.ShowChunkBounds);
        }

        private static void DrawRendererBox(ref ALayerEditorData data)
        {
            var layer = data.Layer;
            GUILayout.Label("Renderer:");
            if (GUILayout.Button("Update renderer"))
            {
                layer.UpdateRenderer();
            }

            layer.TintColor = EditorGUILayout.ColorField("Tint Color:", layer.TintColor);
        }

        private static void DrawColliderBox(ref ALayerEditorData data)
        {
            var layer = data.Layer;
            GUILayout.Label("Collision:");
            layer.ColliderEnabled = EditorGUILayout.Toggle("Collider enabled", layer.ColliderEnabled);
            layer.PhysicsMaterial2D = EditorGUILayout.ObjectField("Name:", layer.PhysicsMaterial2D, typeof(PhysicsMaterial2D), false) as PhysicsMaterial2D;
            layer.IsTrigger = EditorGUILayout.Toggle("Is trigger:", layer.IsTrigger);
        }

        private static void BeginChangeCheck() => EditorGUI.BeginChangeCheck();

        private static void EndChangeCheck()
        {
            _isDirty |= EditorGUI.EndChangeCheck();
        }
    }
}
