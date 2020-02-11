using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    public class RaycastLightMesh : LightMesh
    {

        public float maskCutawayDst = .1f;

        public float Radius = 1f;
        [Range(0f, 1f)]
        public float resolution = 1f;

        protected override void UpdateMesh()
        {
            mesh = Utilites.GenCircle(Radius, OverlayColor, resolution, Fade);
            //lightMask.GenMesh(Radius, Fade, resolution,Intensity);
            lightMask.SetMesh(Utilites.GenCircle(Radius, Color.Lerp(Color.clear, Color.white, Intensity), resolution, Fade));
            lightMask.SetMat(MaskMaterial);
            //meshFilter.sharedMesh = mesh;
            // meshRenderer.sharedMaterial = MeshMaterial;
            //Utilites.GenCircle(radius, Color.Lerp(Color.clear, Color.white, intensity), resolution, fade);

            ApplyMesh();
        }
    }
}
