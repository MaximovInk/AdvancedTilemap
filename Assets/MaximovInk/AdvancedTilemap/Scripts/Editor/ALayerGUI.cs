﻿using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public static class ALayerGUI
    {
        private static bool _isDirty;
        private static bool _isShift;

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
            layer.Tag = EditorGUILayout.TagField("Tag:", layer.Tag);

           

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

            if (layer == null) return;
            if (layer.Tileset == null) return;

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
            }

        }

        public static void Enable(ref ALayerEditorData data)
        {
            data.color = Color.white;
            data.selectedTile = 0;
            data.Tool?.DestroyTexturePreview(ref data);

            if (data.selectedTile == 0 && data.Layer?.Tileset != null && data.Layer?.Tileset.TilesCount > 0)
                data.selectedTile = 1;
        }

        public static void Disable(ref ALayerEditorData data)
        {
            data.Tool?.DestroyTexturePreview(ref data);
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

        private static void DrawGUIPanel(ref ALayerEditorData data)
        {
            Handles.BeginGUI();

            GUILayout.BeginVertical();

            GUILayout.Space(10);

            data.Tool ??= new TileBrushToolEditor();

            GUILayout.BeginHorizontal();

            GUILayout.Space(5);

            if (MKEditorStyles.Button(MKEditorStyles.GetPaintIcon(), data.Tool is TileBrushToolEditor))
            {
                data.Tool = new TileBrushToolEditor();
                data.ToolGenerateInvoke = true;
            }

            GUILayout.Space(5);

            if (MKEditorStyles.Button(MKEditorStyles.GetRectIcon(), data.Tool is RectBrushToolEditor))
            {
                data.Tool = new RectBrushToolEditor();
                data.ToolGenerateInvoke = true;
            }

            GUILayout.Space(5);

            if (MKEditorStyles.Button(MKEditorStyles.GetLineIcon(), data.Tool is LineBrushToolEditor))
            {
                data.Tool = new LineBrushToolEditor();
                data.ToolGenerateInvoke = true;
            }

            GUILayout.Space(5);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginVertical(GUILayout.MaxWidth(90));

            var style = MKEditorStyles.Styles[1];

            GUILayout.Label($"<b>Tile pos: [{data.gridPos.x}:{data.gridPos.y}] </b>", style);
            GUILayout.Label($"<b>Bounds Min: [{data.Layer.MinGridX};{data.Layer.MinGridY}]</b>", style);
            GUILayout.Label($"<b>Bounds Max: [{data.Layer.MaxGridX};{data.Layer.MaxGridY}]</b>", style);

            GUILayout.Space(10);

            GUILayout.Label("<b>Controls</b>", style);
            GUILayout.Label("<b>LMB - place block</b>", style);
            GUILayout.Label("<b>LMB+LSHIFT - remove block</b>", style);
            GUILayout.Label("<b>LMB+LCTRL - copy block</b>", style);
            GUILayout.Label("<b>RMB - place liquid</b>", style);
            GUILayout.Label("<b>RMB+LSHIFT - remove liquid</b>", style);

            GUILayout.EndVertical();

            GUILayout.EndVertical();

            Handles.EndGUI();
        }

        private static void HandleInput(ref bool repaint, ref ALayerEditorData data)
        {
            var e = data.Event;

            switch (e.type)
            {
                case EventType.KeyDown:
                {
                    if (e.keyCode == KeyCode.R)
                    {
                        data.UVTransform._rot90 = !data.UVTransform._rot90;
                       // e.Use();
                        repaint = true;
                    }
                    if (e.keyCode == KeyCode.H)
                    {
                        data.UVTransform._flipHorizontal = !data.UVTransform._flipHorizontal;
                        //e.Use();
                        repaint = true;
                    }
                    if (e.keyCode == KeyCode.V)
                    {
                        data.UVTransform._flipVertical = !data.UVTransform._flipVertical;
                        //e.Use();
                        repaint = true;
                    }
                }
                    break;
            }
        }

        private static void HandleEvents(ref bool repaint, ref ALayerEditorData data)
        {
            Event e = Event.current;
            data.Event = e;

            if (data.Tool != null)
            {
                if (e.shift && !_isShift)
                {
                    _isShift = true;
                    data.Tool.GenPreviewTextureBrush(ref data);
                }
                else if (!e.shift && _isShift)
                {
                    _isShift = false;
                    data.Tool.GenPreviewTextureBrush(ref data);
                }

                Vector3 mousePosition = e.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                mousePosition = ray.origin;

                data.gridPos = Utilites.ConvertGlobalCoordsToGrid(data.Layer, mousePosition);

                if(!data.Tool.UpdatePreviewBrushPos(ref data))
                    data.Tool.GenPreviewTextureBrush(ref data);

                data.Tool.Update(ref data);

                repaint = true;
            }
        }



        private static bool OnSceneGUIPaint(ALayer layer, ref ALayerEditorData data)
        {
            MKEditorStyles.ValidateStyles();

            DrawGUIPanel(ref data);

            var repaint = false;

            if (EditorWindow.mouseOverWindow != SceneView.currentDrawingSceneView)
            {
       
                return true;
            }

            if (DragAndDrop.objectReferences.Length > 0)
            {

                return true;
            }

            if ((Tools.current != Tool.Rect && Tools.current != Tool.None))
            {

                return true;
            }

            if (EditorApplication.isCompiling)
            {
   
                return false;
            }

            var handleInput = true;

            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            data.Event = Event.current;


            if (data.Event.type == EventType.ScrollWheel)
                handleInput = false;

            if (layer.Tileset == null) return false;
            if (!(data.selectedTile > 0 && data.selectedTile < layer.Tileset.TilesCount + 1))
            {

                return false;
            }

            if (data.SelectedToolbar == 0 || data.SelectedToolbar == 1)
            {
  

                HandleEvents(ref repaint, ref data);

                if (handleInput)
                {
                    HandleInput(ref repaint, ref data);
                }
            }

            return repaint;
        }

        private static void DrawLiquidBox(ref ALayerEditorData data)
        {
            var layer = data.Layer;

            layer.LiquidEnabled = EditorGUILayout.Toggle("Liquid enabled", layer.LiquidEnabled);

            if (!layer.LiquidEnabled) return;

            layer.LiquidMaterial = (Material)EditorGUILayout.ObjectField("Material", layer.LiquidMaterial, typeof(Material), false);
            layer.MinLiquidColor = EditorGUILayout.ColorField("Min Liquid color:", layer.MinLiquidColor);
            layer.MaxLiquidColor = EditorGUILayout.ColorField("Max Liquid color:", layer.MaxLiquidColor);
        }

        private static void DrawPaintBox(ref ALayerEditorData data)
        {
            var layer = data.Layer;
            GUILayout.Label("Paint:");
            Tools.current = Tool.None;
            EditorGUI.BeginChangeCheck();

            data.ShowTilesAsList = GUILayout.Toggle(data.ShowTilesAsList, "ShowAsList");

            if (layer.Tileset != null)
                EditorUtils.DrawPreviewTileset(layer.Tileset, ref data.selectedTile, ref data.PreviewScale, ref data.TilesetScrollView, ref data.ShowTilesAsList, 300f);

            data.color = EditorGUILayout.ColorField("Paint color:", data.color);
            data.brushSize = EditorGUILayout.IntSlider("Brush size:", data.brushSize, 1, 32);
            data.brushSize = Mathf.Clamp(data.brushSize, 1, 32);
            data.UVTransform._rot90 = EditorGUILayout.Toggle("rot90", data.UVTransform._rot90);
            data.UVTransform._flipVertical = EditorGUILayout.Toggle("flipVertical", data.UVTransform._flipVertical);
            data.UVTransform._flipHorizontal = EditorGUILayout.Toggle("flipHorizontal", data.UVTransform._flipHorizontal);

            if (EditorGUI.EndChangeCheck() && data.Tool != null)
            {
                data.Tool.GenPreviewTextureBrush(ref data);
            }
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
            layer.LightType = (LightLayerType)EditorGUILayout.EnumPopup("Light type", layer.LightType);
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
