using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public struct ATileDriverData
    {
        public ATile tile;
        public byte selfBitmask;
        public byte bitmask;
        public int x;
        public int y;
        public Color32 color;
        public byte variation;
        public UVTransform tileData;

        public int chunkX;
        public int chunkY;

        public ATileset tileset;
        public MeshData mesh;
    }
}