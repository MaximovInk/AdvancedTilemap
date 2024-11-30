using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class TileBrushToolEditor : AToolEditor
    {
        private Vector2Int lastMousePos;

        private bool isDrawing;

        public override void Update(ref ALayerEditorData data)
        {
            Vector3 mousePosition = data.Event.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.origin;

            var gridPos = data.gridPos;

            if (data.Event.type == EventType.MouseDown && data.Event.button == 0)
            {
                lastMousePos = gridPos;
                data.Layer.BeginRecordingCommand();
                isDrawing = true;
            }

            if ((data.Event.type == EventType.MouseDrag || data.Event.type == EventType.MouseDown) && data.Event.button == 1)
            {
                data.Event.Use();
                var layer = data.Layer;

                if (data.Event.control)
                {
                    data.selectedTile = layer.GetTile(gridPos.x, gridPos.y);
                    data.color = layer.GetColor(gridPos.x, gridPos.y);
                   // ALayerGUI.GenPreviewTextureBrush(ref data);
                   data.Tool.GenPreviewTextureBrush(ref data);
                }
                else if (data.Event.shift)
                {
                    layer.SetLiquid(gridPos.x, gridPos.y, 0);
                }
                else
                {
                    layer.AddLiquid(gridPos.x, gridPos.y, 0.5f);
                }

                layer.Refresh();
                EditorUtility.SetDirty(data.Layer);
            }

            if ((data.Event.type == EventType.MouseDrag || data.Event.type == EventType.MouseDown) && data.Event.button == 0)
            {
                data.Event.Use();

                var layer = data.Layer;
                if (data.Event.control)
                {
                    data.selectedTile = layer.GetTile(gridPos.x, gridPos.y);
                    data.color = layer.GetColor(gridPos.x, gridPos.y);
                    //ALayerGUI.GenPreviewTextureBrush(ref data);
                    data.Tool.GenPreviewTextureBrush(ref data);
                }
                else if (data.Event.shift)
                {
                    Utilites.DrawLine(layer, lastMousePos, gridPos, data.brushSize, 0, Color.white,data.UVTransform);
                }
                else
                {
                    var tileID = data.selectedTile; 
                    Utilites.DrawLine(layer, lastMousePos, gridPos, data.brushSize, tileID, data.color, data.UVTransform);
                }

                lastMousePos = gridPos;
                EditorUtility.SetDirty(data.Layer);
            }
            if (data.Event.type == EventType.MouseUp && data.Event.button == 0)
            {
                if (isDrawing)
                {
                    isDrawing = false;
                    data.Layer.EndRecordCommand();
                }
            }
        }
    }
}