
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public interface ITilemap
    {
        public void SetTile(int x, int y, ushort tileID, UVTransform data = default);
        public void SetColor(int x, int y, Color32 color);
    }
}
