using UnityEngine;
using UnityEngine.Serialization;

namespace MaximovInk.MKRendering
{
    [System.Serializable]
    public struct MKBlurData
    {
        public float Radius;
        public float HSamples;
        public float VSamples;
    }

    [System.Serializable]
    public struct MKLightingData
    {
        public MKBlurData Blur;

        public Color Darkness;

        public Material RenderLightMaterial;
    }

    [System.Serializable]
    public struct MKCameraSettings
    {
        public Vector2 Offset;
        public float ResolutionScale;

        public MKLightingData Lighting;
    }

    [System.Serializable]
    public struct MKCameraRenderData
    {
        public MKCameraSettings CameraSettings;
        public MKRenderingSettings RenderSettings;
        public Vector2Int Resolution;
        public Vector2Int ScaledResolution;
    }

 


}
