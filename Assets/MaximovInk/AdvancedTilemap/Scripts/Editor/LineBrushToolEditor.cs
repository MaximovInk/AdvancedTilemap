using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class LineBrushToolEditor : AToolEditor
    {
        private Vector2Int _lastMousePos;
        private bool _firstClicked;
        private bool _isDrag;

        public override void Update(ref ALayerEditorData data)
        {
            Vector3 mousePosition = data.Event.mousePosition;
            HandleUtility.GUIPointToWorldRay(mousePosition);

            if (!_firstClicked)
            {
                if (data.Event.type == EventType.MouseDown && data.Event.button == 0)
                {
                    _lastMousePos = data.gridPos;
                    _firstClicked = true;

                    _isDrag = true;
                }
            }
            else
            {
                if (data.Event.type == EventType.MouseUp && data.Event.button == 0)
                {
                    _isDrag = false;

                    var layer = data.Layer;
                    layer.BeginRecordingCommand();
                    if (data.Event.shift)
                    {
                        Utilites.DrawLine(layer, _lastMousePos, data.gridPos, 0, Color.white, true);
                    }
                    else
                    {
                        var tileID = data.selectedTile;

                        
                        Utilites.DrawLine(layer, _lastMousePos, data.gridPos, data.brushSize, tileID, data.color, data.UVTransform);
                    }
                    layer.EndRecordCommand();
                    layer.Refresh();
                    EditorUtility.SetDirty(data.Layer);

                    _firstClicked = false;
                }
            }
        }

        public override bool UpdatePreviewBrushPos(ref ALayerEditorData data)
        {
            if (!_isDrag)
            {
                base.UpdatePreviewBrushPos(ref data);
                return false;
            }

            if (data.PreviewTextureBrush == null)
                return false;

            var position = data.Layer.transform.TransformPoint(_lastMousePos * data.Layer.Tileset.GetTileUnit());
            position.z = data.Layer.transform.position.z - 1;

            data.PreviewTextureBrush.SetPosition(position);
            if (data.brushSize >= 1)
            {
                data.PreviewTextureBrush.SetPosition(
                    position 
                    - data.PreviewTextureBrush.transform.localScale / 2f
                    + new Vector3(0.5f, 0.5f, 0));
            }

            return false;
        }

        public override void GenPreviewTextureBrush(ref ALayerEditorData data)
        {
            if (data.Layer == null || data.Layer.Tileset == null)
            {
                DestroyTexturePreview(ref data);
                return;
            }

            if (data.PreviewTextureBrush == null)
            {
                var go = new GameObject
                {
                    name = "_PreviewPaintBrush"
                };
                data.PreviewTextureBrush = go.AddComponent<PaintPreview>();

                data.PreviewTextureBrush.Validate();
            }

            var isShift = data.Event.shift;
            data.PreviewTextureBrush.Clear();
            if (_isDrag)
            {
                data.PreviewTextureBrush.SetDriverData(new ATileDriverData()
                {
                    tileset = data.Layer.Tileset,
                    tile = data.Layer.Tileset.GetTile(data.selectedTile),
                    tileData = data.UVTransform,
                    color = (isShift ? Color.red : (Color)data.color * data.Layer.TintColor),
                    variation = 0,
                });
                Utilites.DrawLine(data.PreviewTextureBrush, Vector2Int.zero, data.gridPos - _lastMousePos, 0, Color.white, true);
            }
            else
            {
                base.GenPreviewTextureBrush(ref data);
                return;
            }
            data.PreviewTextureBrush.Apply();

            data.PreviewTextureBrush.SetMaterial(data.Layer.Material, data.Layer.Tileset.Texture);
        }
    }
}