using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public struct ATileDriverData
    {
        public ATile tile;
        public byte bitmask;
        public int x;
        public int y;
        public Color32 color;
        public bool blend;
        public byte variation;
        public UVTransform tileData;

        public ATileset tileset;
        public MeshData mesh;
    }
}