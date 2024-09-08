using UnityEngine;

namespace MaximovInk.PixelLight
{
    [System.Serializable]
    public struct RenderMaterialData
    {
        public float Radius;
        public float HSamples;
        public float VSamples;
    }

    [System.Serializable]
    public struct PixelCameraRenderData
    {
        public LayerMask LightMask;
        public Color Darkness;
        public Vector2 Offset;
        public Vector2Int Resolution;
        public Material RenderLightMaterial;
        public RenderMaterialData RenderMaterialData;

        public float RenderCameraScale;
    }

    [ExecuteAlways]
    public class PixelLightingManager : MonoBehaviourSingletonAuto<PixelLightingManager>
    {
        public LayerMask LightMask => _lightingMask;

        public float PlayModeLightUpdateRate => _playModeLightUpdateRate;

        public Material LightMeshMaterial => _lightMeshMaterial;
        private static Vector2Int Resolution => new Vector2Int(Screen.width, Screen.height);

        public float RenderCameraScale = 1f;

        public PixelCamera2D Camera2D
        {
            get
            {
                if (_camera2D == null)
                {
                    _camera2D = FindFirstObjectByType<PixelCamera2D>();

                    if (_camera2D == null)
                    {
                        _camera2D = Camera.main.gameObject.AddComponent<PixelCamera2D>();
                    }
                }

                return _camera2D;
            }
        }
        private PixelCamera2D _camera2D;

        [SerializeField]
        private Material _lightMeshMaterial;

          [SerializeField]
         private Material _renderLightingMaterial;

         [SerializeField]
         private LayerMask _lightingMask;
         [SerializeField]
         private Vector2 _offset;
         [SerializeField]
         private Color _darkness = Color.black;

         [SerializeField] private float _playModeLightUpdateRate = 0f;
         
        [SerializeField]
        private float multiplierResolution = 1f;

        [SerializeField] private float _blurRadius;
        [SerializeField] private float _blurHorizontalSamples;
        [SerializeField] private float _blurVerticalSamples;

        [SerializeField,HideInInspector]
        private PixelCameraRenderData _renderData;

        private Vector2Int _scaledResolution;

        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy) return;

            FillData();
            Camera2D.UpdateView(_renderData);
        }

        private void FillData()
        {
            _renderData = new PixelCameraRenderData()
            {
                Darkness = _darkness,
                LightMask = _lightingMask,
                Offset = _offset,
                RenderLightMaterial = _renderLightingMaterial,
                Resolution = _scaledResolution,
                RenderMaterialData = new RenderMaterialData()
                {
                    HSamples = _blurHorizontalSamples,
                    VSamples = _blurVerticalSamples,
                    Radius = _blurRadius
                },
                RenderCameraScale = RenderCameraScale
            };
        }

        private void Update()
        {
            FillData();

            multiplierResolution = Mathf.Clamp(multiplierResolution, 0.02f, 2f);

            var newRes = new Vector2Int((int)(Resolution.x * multiplierResolution), (int)(Resolution.y * multiplierResolution));

            if ((_scaledResolution.x == newRes.x && _scaledResolution.y == newRes.y) || newRes.x <= 0 ||
                newRes.y <= 0) return;

            _scaledResolution = newRes;

            Camera2D.UpdateView(_renderData);
        }
    }
}
