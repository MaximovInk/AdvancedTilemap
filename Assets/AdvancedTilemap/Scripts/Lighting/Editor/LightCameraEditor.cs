using UnityEditor;
using UnityEngine;
namespace AdvancedTilemap.Lighting
{
    [CustomEditor(typeof(LightCamera))]
    public class LightCameraEditor : Editor
    {

        private Material LastMaterial;
        

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var t = target as LightCamera;
            if (LastMaterial != t.RenderTextureMaterial)
            {
                t.UpdateLightView();
                LastMaterial = t.RenderTextureMaterial;

            }
        }

    }
}
