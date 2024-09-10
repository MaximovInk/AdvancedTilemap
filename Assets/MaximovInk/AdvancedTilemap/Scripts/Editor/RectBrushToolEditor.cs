using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class RectBrushToolEditor : AToolEditor
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

                    var tileID = data.selectedTile;

                    if (data.gridPos.x < lastMousePos.x)
                    {
                        (data.gridPos.x, lastMousePos.x) = (lastMousePos.x, data.gridPos.x);
                    }

                    if (data.gridPos.y < lastMousePos.y)
                    {
                        (data.gridPos.y, lastMousePos.y) = (lastMousePos.y, data.gridPos.y);
                    }

                    for (int ix = lastMousePos.x; ix <= data.gridPos.x; ix++)
                    {
                        for (int iy = lastMousePos.y; iy <= data.gridPos.y; iy++)
                        {
                            if (data.Event.shift)
                            {
                                layer.SetTile(ix, iy,0);
                            }
                            else
                            {
                                layer.SetTile(ix, iy, tileID);
                            }
                        }
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