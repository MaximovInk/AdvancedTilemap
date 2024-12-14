using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public abstract class AToolEditor
    {
        public virtual void Update(ref ALayerEditorData data)
        {
            if (data.ToolGenerateInvoke)
            {
                GenPreviewTextureBrush(ref data);
                data.ToolGenerateInvoke = false;
            }
        }

        public virtual bool UpdatePreviewBrushPos(ref ALayerEditorData data)
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

        public virtual void DestroyTexturePreview(ref ALayerEditorData data)
        {
            if (data.PreviewTextureBrush != null)
                Object.DestroyImmediate(data.PreviewTextureBrush.gameObject);
        }

        public virtual void GenPreviewTextureBrush(ref ALayerEditorData data)
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
                variation = 0,
            });

            data.PreviewTextureBrush.SetMaterial(data.Layer.Material, data.Layer.Tileset.Texture);
        }
    }
}