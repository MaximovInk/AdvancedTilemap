using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public struct ALightingSettings
    {
        public bool Enabled;
        public Material LightMaterial;
        public ALayer ForegroundLayer;
        public ALayer BackgroundLayer;

        public LayerMask LightingMask;

        public Color ClearColor;
        public Color PixelColor;
        public bool IsInverse;
    }
}
