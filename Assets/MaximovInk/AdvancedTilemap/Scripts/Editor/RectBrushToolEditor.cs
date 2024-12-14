using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class RectBrushToolEditor : AToolEditor
    {
        private Vector2Int _lastMousePos;
        private bool _firstClicked;
        private bool _isDrag;

        public override void Update(ref ALayerEditorData data)
        {
            Vector3 mousePosition = data.Event.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.origin;

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

                    var tileID = data.selectedTile;

                    if (data.gridPos.x < _lastMousePos.x)
                    {
                        (data.gridPos.x, _lastMousePos.x) = (_lastMousePos.x, data.gridPos.x);
                    }

                    if (data.gridPos.y < _lastMousePos.y)
                    {
                        (data.gridPos.y, _lastMousePos.y) = (_lastMousePos.y, data.gridPos.y);
                    }

                    for (int ix = _lastMousePos.x; ix <= data.gridPos.x; ix++)
                    {
                        for (int iy = _lastMousePos.y; iy <= data.gridPos.y; iy++)
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

                    _firstClicked = false;
                }
            }

            base.Update(ref data);
        }

        private void GenerateSingleBlock(ALayerEditorData data)
        {
            var newData = data;
            newData.brushSize = 1;

            base.GenPreviewTextureBrush(ref newData);
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

            var current = data.gridPos;
            var first = _lastMousePos;

            var min = Vector2Int.Min(current, first);

            var position = data.Layer.transform.TransformPoint(min * data.Layer.Tileset.GetTileUnit());
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

                var current = data.gridPos;
                var first = _lastMousePos;

                var sizeX = Mathf.Abs(current.x - first.x);
                 var sizeY = Mathf.Abs(current.y - first.y);

                for (int ix = 0; ix <= sizeX; ix++)
                {
                    for (int iy = 0; iy <= sizeY; iy++)
                    {
                        data.PreviewTextureBrush.SetTile(ix, iy, 0);
                    }
                }
            }
            else
            {
                GenerateSingleBlock(data);
                return;

            }

            data.PreviewTextureBrush.Apply();
            data.PreviewTextureBrush.SetMaterial(data.Layer.Material, data.Layer.Tileset.Texture);

        }
    }
}