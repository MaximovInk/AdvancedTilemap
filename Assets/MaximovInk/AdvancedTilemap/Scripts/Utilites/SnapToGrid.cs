using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [ExecuteAlways]
    public class SnapToGrid : MonoBehaviour
    {
        public float PPU = 16;

        private void LateUpdate()
        {
            PPU = Mathf.Clamp(PPU, 2, 64);
            
            var position = transform.position;

            position.x = (Mathf.Round(position.x * PPU) / PPU);
            position.y = (Mathf.Round(position.y * PPU) / PPU);

            transform.localPosition = position;
        }
    }
}