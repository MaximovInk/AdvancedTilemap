using UnityEngine;

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

        public bool RepaintInvoke;
    }
}
