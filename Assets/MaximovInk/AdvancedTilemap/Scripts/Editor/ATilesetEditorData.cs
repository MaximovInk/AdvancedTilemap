using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public struct ATilesetEditorData
    {
        public ushort SelectedTile;
        public float PreviewScale;
        public Vector2 ScrollViewValue;
        public int TileDriverID;

        public Vector2 SelectedTileScroll;
        public int TilesWidth;
        public ParameterType SelectedParameterType;
    }
}
