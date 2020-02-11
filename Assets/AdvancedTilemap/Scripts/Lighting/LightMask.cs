using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class LightMask : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private void OnEnable()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetMesh(Mesh mesh)
        {
            meshFilter.sharedMesh = mesh;
        }
        /*
        public void GenMesh(float radius,float fade,float resolution,float intensity)
        {
            meshFilter.sharedMesh = Utilites.GenCircle(radius, Color.Lerp(Color.clear,Color.white, intensity), resolution, fade);
        }
        */
        public void SetMat(Material mat)
        {
            meshRenderer.sharedMaterial = mat;
        }

    }


}
