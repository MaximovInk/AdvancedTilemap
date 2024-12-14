using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class TileBrushToolEditor : AToolEditor
    {
        private Vector2Int _lastMousePos;

        private bool _isDrawing;

        public override void Update(ref ALayerEditorData data)
        {
            var gridPos = data.gridPos;

            if (data.Event.type == EventType.MouseDown && data.Event.button == 0)
            {
                _lastMousePos = gridPos;
                data.Layer.BeginRecordingCommand();
                _isDrawing = true;
            }

            if ((data.Event.type == EventType.MouseDrag || data.Event.type == EventType.MouseDown) && data.Event.button == 1)
            {
                data.Event.Use();
                var layer = data.Layer;

                if (data.Event.control)
                {
                    data.selectedTile = layer.GetTile(gridPos.x, gridPos.y);
                    data.color = layer.GetColor(gridPos.x, gridPos.y);
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
                    Utilites.DrawLine(layer, _lastMousePos, gridPos, data.brushSize, 0, Color.white,data.UVTransform);
                }
                else
                {
                    var tileID = data.selectedTile; 
                    Utilites.DrawLine(layer, _lastMousePos, gridPos, data.brushSize, tileID, data.color, data.UVTransform);
                }

                _lastMousePos = gridPos;
                EditorUtility.SetDirty(data.Layer);
            }
            if (data.Event.type == EventType.MouseUp && data.Event.button == 0)
            {
                if (_isDrawing)
                {
                    _isDrawing = false;
                    data.Layer.EndRecordCommand();
                }
            }

            base.Update(ref data);
        }
    }
}