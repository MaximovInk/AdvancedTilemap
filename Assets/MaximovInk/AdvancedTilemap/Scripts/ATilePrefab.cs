
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class ATilePrefab : MonoBehaviour
    {
        protected AChunk AttachedChunk;
        protected Vector2Int Position;
        protected ushort TileID;

        public void Init(AChunk chunk, int x, int y, ushort tileID)
        {
            AttachedChunk = chunk;
            Position = new Vector2Int(x, y);
            TileID = tileID;

            OnInitialized();
        }

        protected virtual void OnInitialized()
        {

        }
    }
}
