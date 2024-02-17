using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class LineBrushToolEditor : AToolEditor
    {
        private Vector2Int lastMousePos;
        private bool firstClicked = false;
            
        public override void Update(ref ALayerEditorData data)
        {
            Vector3 mousePosition = data.Event.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.origin;

            if (!firstClicked)
            {
                if (data.Event.type == EventType.MouseDown && data.Event.button == 0)
                {
                    lastMousePos = data.gridPos;
                    firstClicked = true;
                }
            }
            else
            {
                if (data.Event.type == EventType.MouseUp && data.Event.button == 0)
                {
                    var layer = data.Layer;
                    layer.BeginRecordingCommand();
                    if (data.Event.shift)
                    {
                        Utilites.DrawLine(layer, lastMousePos, data.gridPos, 0, Color.white, true);
                    }
                    else
                    {
                        var tileID = data.selectedTile;

                        
                        Utilites.DrawLine(layer, lastMousePos, data.gridPos, data.brushSize, tileID, data.color, data.UVTransform);
                    }
                    layer.EndRecordCommand();
                    layer.Refresh();
                    EditorUtility.SetDirty(data.Layer);

                    firstClicked = false;
                }
            }
        }
    }
}