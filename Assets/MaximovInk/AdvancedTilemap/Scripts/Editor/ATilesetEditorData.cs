using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public struct ATilesetEditorData
    {
        public ushort SelectedTile;
        public float PreviewScale;
        public Vector2 ScrollViewValue;

        public Vector2 SelectedTileScroll;
        public int TilesWidth;
        public ParameterType SelectedParameterType;

        public int TileDriverIndex;
        public string TileDriverID;

        public bool InvokeClearAll;

        public bool ShowHiddenParameters;
        public bool ShowTilesAsList;
    }
}
