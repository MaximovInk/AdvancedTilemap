
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class ATilePrefab : MonoBehaviour
    {
        public AChunk AttachedChunk => _chunk;
        public Vector2Int Position => _position;
        public ushort TileID => _tileID;

        [HideInInspector,SerializeField]
        private AChunk _chunk;
        [HideInInspector, SerializeField]
        private Vector2Int _position;
        [HideInInspector, SerializeField]
        private ushort _tileID;

        public void Init(AChunk chunk, int x, int y, ushort tileID)
        {
            _chunk = chunk;
            _position = new Vector2Int(x, y);
            _tileID = tileID;

            OnInitialized();
        }

        protected virtual void OnInitialized()
        {

        }
    }
}
