using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public struct AChunkPersistenceData
    {
        public ALayer Layer;
        public Material Material;
        public Vector2Int Position;
    }
}