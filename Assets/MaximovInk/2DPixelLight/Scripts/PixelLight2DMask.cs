using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaximovInk.PixelLight
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PixelLight2DMask : MonoBehaviour
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

        public void SetMat(Material mat)
        {
            meshRenderer.sharedMaterial = mat;
        }

        public void SetColor(Color color)
        {
            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
                meshRenderer.sharedMaterial.color = color;
        }

    }
}
