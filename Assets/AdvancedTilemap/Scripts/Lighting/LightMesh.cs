using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public class LightMesh : MonoBehaviour
    {
        public Material MeshMaterial;
        public Material MaskMaterial;

        public Color OverlayColor = Color.white;
        [Range(0f,1f)]
        public float Fade = 0.95f;
        [Range(0f,1f)]
        public float Intensity = 1f;

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
                    go.hideFlags = HideFlags.HideInHierarchy;
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

        private void LateUpdate()
        {
            updateMeshTimer += Time.deltaTime;

            if (updateMeshTimer > UpdateMeshRate)
            {
                updateMeshTimer = 0f;
                UpdateMesh();
            }
        }

        private void OnEnable()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            mesh = new Mesh();
            UpdateMesh();
        }

        protected virtual void UpdateMesh()
        { 
            
        }

        protected void ApplyMesh()
        {
            meshFilter.sharedMesh = mesh;
            meshRenderer.material = MeshMaterial;
        }
    }
}
