
using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{

    public static class ALayerGUI
    {
        /*public static GUIStyle RichHelpBoxStyle = new GUIStyle("helpBox")
        {
            richText = true
        };

        public static GUIStyle ToolbarBoxStyle = new GUIStyle()
        {
            normal = { textColor = Color.white },
            richText = true,
        };*/

        public static GUIStyle[] Styles;

        public static void ValidateStyles()
        {
            if(Styles != null && Styles.Length == 2)return;

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

        private static bool _isDirty;

        public static bool DrawGUI(ALayer layer, ref ALayerEditorData data)
        {
            _isDirty = false;

            BeginChangeCheck();

            data.Layer = layer;
            data.Event = Event.current;


            GUILayout.BeginVertical("helpBox");

            GUILayout.Label($"{layer.name}");

            layer.IsActive = EditorGUILayout.Toggle("Active:", layer.IsActive);

            layer.Tileset = (ATileset)EditorGUILayout.ObjectField("Tileset", layer.Tileset, typeof(ATileset), false);
            layer.Material = (Material)EditorGUILayout.ObjectField("Material", layer.Material, typeof(Material), false);
            layer.LayerMask = EditorGUILayout.LayerField("Layer:", layer.LayerMask);
            layer.Tag = EditorGUILayout.TagField("Tag:", layer.gameObject.tag);

            if (data.selectedTile == 0 && layer.Tileset != null && layer.Tileset.TilesCount > 0)
                data.selectedTile = 1;


            if (GUILayout.Button("Refresh"))
            {
                layer.Refresh(true);
            }

            GUILayout.Space(20);


            data.SelectedToolbar =
                GUILayout.Toolbar(data.SelectedToolbar, new string[] { "All", "Paint", "Map", "Renderer", "Liquid", "Collision" });

            if (data.LastSelectedToolbar != data.SelectedToolbar && data.SelectedToolbar == 0)
            {
                Tools.current = Tool.None;
                data.LastSelectedToolbar = data.SelectedToolbar;
                data.Tool = new TileBrushToolEditor();
                data.selectedTile = 0;
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
            if (data.selectedTile == 0) return ;

            OnSceneGUIUndo(layer, ref data);
            data.RepaintInvoke |= OnSceneGUIPaint(layer, ref data);

            if (layer != null || layer.Tilemap != null)
            {
                var lTransform = layer.transform;



                var cellSize = layer.Tileset.GetTileUnit();

                Handles.color = GlobalSettings.TilemapGridColor;

                for (int i = layer.MinGridX; i <= layer.MaxGridX + 1; i++)
                {
                 Handles.DrawLine(
                     lTransform.TransformPoint(new Vector3(i * cellSize.x, layer.MinGridY * cellSize.y)),
                     lTransform.TransformPoint(new Vector3(i * cellSize.x, (layer.MaxGridY+1) * cellSize.y)));   
                }

                for (int i = layer.MinGridY; i <= layer.MaxGridY + 1; i++)
                {
                    Handles.DrawLine(
                        lTransform.TransformPoint(new Vector3(layer.MinGridX * cellSize.x, i * cellSize.y)),
                        lTransform.TransformPoint(new Vector3((layer.MaxGridX + 1) * cellSize.x, i * cellSize.y)));
                }

                Handles.color = Color.white;

                /* for (float i = 1; i < GridWidth; i++)
                {
                    Gizmos.DrawLine(
                        this.transform.TransformPoint(new Vector3(MapBounds.min.x + i * CellSize.x, MapBounds.min.y)),
                        this.transform.TransformPoint(new Vector3(MapBounds.min.x + i * CellSize.x, MapBounds.max.y))
                    );
                }

                // Vertical lines
                for (float i = 1; i < GridHeight; i++)
                {
                    Gizmos.DrawLine(
                        this.transform.TransformPoint(new Vector3(MapBounds.min.x, MapBounds.min.y + i * CellSize.y, 0)),
                        this.transform.TransformPoint(new Vector3(MapBounds.max.x, MapBounds.min.y + i * CellSize.y, 0))
                    );
                }*/
            }

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

        private static bool isShift;
 
        private static bool OnSceneGUIPaint(ALayer layer, ref ALayerEditorData data)
        {
            ValidateStyles();

            var repaint = false;

            if (Tools.current != Tool.Rect && Tools.current != Tool.None)
                return true;

            if (EditorWindow.mouseOverWindow != SceneView.currentDrawingSceneView)
            {
                return true;
            }

            if (DragAndDrop.objectReferences.Length > 0)
            {
                return true;
            }

            data.Event = Event.current;



            if (layer.Tileset == null) return false;
            if (!(data.selectedTile > 0 && data.selectedTile < layer.Tileset.TilesCount+1))
                return false;

            if (data.SelectedToolbar == 0 || data.SelectedToolbar == 1)
            {
                Event e = Event.current;
                data.Event = e;

                //var isShift = e.shift;

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

                var buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    richText = true
                };
                var selectedButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    normal = { textColor = new Color(0.7f,0.5f,1f,1f) },
                    richText = true
                };
                //new Color(0.2f,0.1f,0.4f,0f)

                data.Tool ??= new TileBrushToolEditor();

                    GUILayout.BeginVertical(GUILayout.MaxWidth(90));

                    if (GUILayout.Button(
                            "<b>Brush</b>",
                            (data.Tool is TileBrushToolEditor) ? selectedButtonStyle : buttonStyle))
                        data.Tool = new TileBrushToolEditor();

                    if (GUILayout.Button(
                            "<b>Line</b>",
                            (data.Tool is LineBrushToolEditor) ? selectedButtonStyle : buttonStyle))
                        data.Tool = new LineBrushToolEditor();

                    if (GUILayout.Button(
                            "<b>FillRect</b>",
                            (data.Tool is RectBrushToolEditor) ? selectedButtonStyle : buttonStyle))
                        data.Tool = new RectBrushToolEditor();


                    GUILayout.Label($"<b>Tile pos: [{data.gridPos.x}:{data.gridPos.y}] </b>", Styles[1]);
                    GUILayout.Label($"<b>Bounds Min: [{data.Layer.MinGridX};{data.Layer.MinGridY}]</b>", Styles[1]);
                    GUILayout.Label($"<b>Bounds Max: [{data.Layer.MaxGridX};{data.Layer.MaxGridY}]</b>", Styles[1]);

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

                    repaint=true;
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

            //data.PreviewTextureBrush.transform.position = position;
            data.PreviewTextureBrush.SetPosition(position);
            if (data.brushSize >= 1)
            {
                
               // data.PreviewTextureBrush.transform.position = 
                data.PreviewTextureBrush.SetPosition((Vector3)position - data.PreviewTextureBrush.transform.localScale / 2f + new Vector3(0.5f, 0.5f, 0));
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
                color = (isShift ? Color.red : (Color)data.color * data.Layer.TintColor),
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
                EditorUtils.DrawPreviewTileset(layer.Tileset, ref data.selectedTile, ref data.PreviewScale, ref data.TilesetScrollView, 300f);

            data.color = EditorGUILayout.ColorField("Paint color:", data.color);
            data.brushSize = EditorGUILayout.IntSlider("Brush size:", data.brushSize, 1, 32);
            data.brushSize = Mathf.Clamp(data.brushSize, 1, 32);
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

            GUILayout.Label("Global editor settings");
            GlobalSettings.TilemapGridColor = EditorGUILayout.ColorField("Grid Color", GlobalSettings.TilemapGridColor);


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
