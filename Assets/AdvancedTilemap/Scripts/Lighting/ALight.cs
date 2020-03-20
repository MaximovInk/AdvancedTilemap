using System.Collections.Generic;
using UnityEngine;
using AdvancedTilemap.Extra;

namespace AdvancedTilemap.Lighting
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer),typeof(TransformChangedEvent))]
    public class ALight : MonoBehaviour
    {
        public Material MeshMaterial;

        public bool Static;

        public Color OverlayColor = Color.white;
        public float Intensity = 1f;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private TransformChangedEvent transformObserver;

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

        public bool UpdateMesh = false;
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

        private void OnEnable()
        {
            mesh = new Mesh();
            maskMesh = new Mesh();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            transformObserver = GetComponent<TransformChangedEvent>();
            if (transformObserver == null)
            {
                transformObserver = gameObject.AddComponent<TransformChangedEvent>();
            }
            transformObserver.TransformChanged += InvokeCreateLight;
            mesh = new Mesh();
            maskMesh = new Mesh();
            CreateLight();
        }

        private void OnDisable()
        {
            transformObserver.TransformChanged -= InvokeCreateLight;
        }

        public void CreateLight()
        {
            if (Static)
                return;

            CalculatePoints();
            GenerateMesh();
            ApplyData();
            meshRenderer?.sharedMaterial?.SetColor("_Color", OverlayColor);
        }

        private bool needUpdateLight;
        private void InvokeCreateLight()
        {
            needUpdateLight = true;
        }

        private void Update()
        {
            if (needUpdateLight)
            {
                CreateLight();
                needUpdateLight = false;
            }

            if (!UpdateMesh)
                return;

            updateMeshTimer += Time.deltaTime;

            if (updateMeshTimer > UpdateMeshRate)
            {
                updateMeshTimer = 0f;
                CreateLight();
            }
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
            if (MeshMaterial == null)
                return;
            lightMask.SetMat(new Material(MeshMaterial));
            lightMask.SetColor(new Color(1,1,1, Intensity));
        }

        private void ApplyToMesh()
        {
            meshFilter.sharedMesh = mesh;
            if (MeshMaterial == null)
                return;
            meshRenderer.sharedMaterial = new Material(MeshMaterial);
            meshRenderer.sharedMaterial.color = OverlayColor;
        }
    }
}
