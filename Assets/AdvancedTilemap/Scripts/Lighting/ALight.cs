using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class ALight : MonoBehaviour
    {
        public Material MeshMaterial;
        public Material MaskMaterial;

        public Color OverlayColor = Color.white;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        protected LightMask lightMask { 
            get 
            { 
                if (_lightMask == null)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        DestroyImmediate(transform.GetChild(i).gameObject);
                    }

                    var go = new GameObject();
                    go.transform.SetParent(transform);
                   // go.hideFlags = HideFlags.HideInHierarchy;
                    go.transform.localPosition = Vector3.zero;
                    go.layer = LayerMask.NameToLayer("Lighting");
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = Vector3.one;
                    go.name = "_LightLayerMesh";
                    _lightMask = go.AddComponent<LightMask>();
                } 
                return _lightMask;
            }
        }
        private LightMask _lightMask;

        public float UpdateMeshRate = 0.1f;
        private float updateMeshTimer;

        [SerializeField,HideInInspector]
        protected Mesh mesh;
        [SerializeField, HideInInspector]
        protected Mesh maskMesh;


        #region Temp_data

        protected int[] triangles;
        protected Vector3[] vertices;
        protected Vector2[] uv;

        protected List<Vector2> points;

        #endregion


        private void LateUpdate()
        {
            updateMeshTimer += Time.deltaTime;

            if (updateMeshTimer > UpdateMeshRate)
            {
                updateMeshTimer = 0f;
                CalculatePoints();
            }
        }

        private void OnEnable()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            mesh = new Mesh();
            maskMesh = new Mesh();
            CalculatePoints();
        }

        private void Update()
        {
            CalculatePoints();
            GenerateMesh();
            meshRenderer?.sharedMaterial?.SetColor("_Color", OverlayColor);
        }

        protected virtual void GenerateMesh()
        {

        }

        protected virtual void SmoothPoints(ref List<Vector2> points)
        {
           
        }

        protected virtual void CalculatePoints()
        {
           
        }

        protected virtual void ApplyData()
        {
            mesh.Clear();
            maskMesh.Clear();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            maskMesh.vertices = vertices;
            maskMesh.triangles = triangles;
            maskMesh.uv = uv;
            maskMesh.RecalculateNormals();
            mesh.RecalculateTangents();

            ApplyToMesh();
            ApplyToMask();
        }

        private void ApplyToMask()
        {
            lightMask.SetMesh(maskMesh);
            lightMask.SetMat(MaskMaterial);
            lightMask.SetColor(OverlayColor);
        }

        private void ApplyToMesh()
        {
            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = MeshMaterial;
        }
    }
}
