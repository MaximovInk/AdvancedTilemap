using System.Collections.Generic;
using AdvancedTilemap.Extra;
using UnityEngine;

namespace MaximovInk.PixelLight
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer),typeof(MeshFilter))]
    public class PixelLight2D : MonoBehaviour
    {
        [SerializeField] protected float _intensity;
        [SerializeField] protected Material _customMaterial;

        [SerializeField]
        protected Color _overlayColor = Color.white;

        [SerializeField,HideInInspector]
        protected int[] triangles;
        [SerializeField, HideInInspector]
        protected Vector3[] vertices;
        [SerializeField, HideInInspector]
        protected Vector2[] uv;
        [SerializeField, HideInInspector]
        protected List<Vector2> points;

        protected Mesh _mesh;
        [SerializeField,HideInInspector]
        private MeshFilter _meshFilter;
        [SerializeField,HideInInspector]
        private MeshRenderer _meshRenderer;
        [SerializeField,HideInInspector]
        private TransformChangedEvent transformObserver;
        [SerializeField, HideInInspector]
        private PixelLight2DMask _mask;

        protected bool _isDirty;

        [SerializeField] private bool _updateAlways;
        [SerializeField] private float _updateMeshRate = 0.1f;
        private float _updateMeshTimer;

        private void Awake()
        {
            if (Application.isPlaying)
                _updateMeshRate = PixelLightingManager.Instance.PlayModeLightUpdateRate;
        }

        private void Update()
        {

            if (_updateAlways)
                _updateMeshTimer += Time.deltaTime;

            if (_updateMeshTimer > _updateMeshRate)
            {

                if (transform.lossyScale.x < 0)
                {
                    var localScale = transform.localScale;

                    localScale = new Vector3(localScale.x * -1, localScale.y,
                        localScale.y);

                    transform.localScale = localScale;
                }

                _updateMeshTimer = 0f;
                _isDirty = true;
            }

            if (_isDirty)
            {
                _isDirty = false;
                ValidateMesh();
                CalculatePoints();
                GenerateMesh();
                ApplyData();
            }
        }

        private void ValidateMesh()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
            }

            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            if (_meshRenderer == null)
            {
                _meshRenderer = GetComponent<MeshRenderer>();
            }

            if (transformObserver == null)
            {
                transformObserver = GetComponent<TransformChangedEvent>();
                if (transformObserver == null)
                {
                    transformObserver = gameObject.AddComponent<TransformChangedEvent>();
                }
                transformObserver.TransformChanged += ()=>_isDirty=true;
            }

            if (_mask == null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }

                var go = new GameObject();
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                go.layer= Mathf.RoundToInt(Mathf.Log(PixelLightingManager.Instance.LightMask.value, 2));
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.name = "_LightLayerMesh";
                _mask = go.AddComponent<PixelLight2DMask>();
            }

        }


        protected virtual void CalculatePoints()
        {

        }

        protected virtual void GenerateMesh()
        {

        }

        protected virtual void ApplyData()
        {
            _mesh.Clear();

            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.uv = uv;
            _mesh.RecalculateNormals();
            _mesh.RecalculateTangents();

            var material = _customMaterial;
            if (material == null)
            {
                material = PixelLightingManager.Instance.LightMeshMaterial;
            }

            var overlayMat = new Material(material)
            {
                color = _overlayColor
            };
            _meshRenderer.material = overlayMat;
            _meshFilter.sharedMesh = _mesh;

            _meshRenderer.enabled = false;

            var maskMat = new Material(material)
            {
                color = new Color(1f, 1f, 1f, _intensity)
            };
            _mask.SetMat(maskMat);
            _mask.SetMesh(_mesh);

        }
    }
}