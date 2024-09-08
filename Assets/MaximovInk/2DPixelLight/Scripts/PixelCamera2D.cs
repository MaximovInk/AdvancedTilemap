using UnityEngine;

namespace MaximovInk.PixelLight
{
    [RequireComponent(typeof(Camera))]
    public class PixelCamera2D : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Camera _postProcessCamera;
        [SerializeField,HideInInspector]
        private Camera _renderCamera;
        [SerializeField, HideInInspector]
        private Renderer _postProcessOverlay;
        [SerializeField,HideInInspector]
        private RenderTexture _renderTexture;
        [SerializeField, HideInInspector]
        private RenderTexture _resultTexture;

        private Vector2 _cameraSize;
        private Material _renderMaterial;

        private static readonly int DarknessProp = Shader.PropertyToID("_Darkness");

        public void ValidatePostProcessCamera()
        {
            if (_postProcessCamera == null)
            {
                _postProcessCamera = GetComponent<Camera>();
            }

            _postProcessCamera.nearClipPlane = 0f;
            if(!_postProcessCamera.CompareTag("MainCamera"))
                _postProcessCamera.tag = "MainCamera";

            _postProcessCamera.orthographic = true;

            float camH = _renderCamera.orthographicSize * 2f;
            var aspect = _renderCamera.aspect;
            _cameraSize = new Vector2(aspect * camH, camH);


            _postProcessCamera.cullingMask &= ~_renderData.LightMask;
        }

        public void ValidateQuad()
        {
            if (_postProcessOverlay == null)
            {
                _postProcessOverlay = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<Renderer>();
                _postProcessOverlay.name = "Overlay Instance";
            }

            Vector2 spriteSize = Vector2.one;
            Vector3 scale = (_cameraSize / spriteSize) + _renderData.Offset;

            Transform quadTransform;

            (quadTransform = _postProcessOverlay.transform).SetParent(transform);
            quadTransform.localPosition = new Vector3(0,0,0.01f);
            quadTransform.localScale = scale;

        }

        public void ValidateRenderCamera()
        {
            if (_renderCamera == null)
            {
                _renderCamera = new GameObject().AddComponent<Camera>();
                _renderCamera.name = "RenderCamera Instance";
            }

            _renderCamera.clearFlags = CameraClearFlags.Color;
            _renderCamera.backgroundColor = Color.black;
            _renderCamera.nearClipPlane = 0f;
            _renderCamera.transform.SetParent(transform);
            _renderCamera.transform.localPosition = new Vector3(0, 0, 0.02f);


            _renderCamera.orthographicSize = _postProcessCamera.orthographicSize * _renderData.RenderCameraScale;
            _renderCamera.orthographic = true;

            _renderCamera.cullingMask = _renderData.LightMask;
        }

        public void ValidateRenderTexture()
        {
            if ((_renderTexture == null || _renderTexture.width != _renderData.Resolution.x || _renderTexture.height != _renderData.Resolution.y) && (_renderData.Resolution.x > 0 && _renderData.Resolution.y > 0))
            {
                if (_renderTexture != null)
                {
                    _renderCamera.targetTexture = null;
                    DestroyImmediate(_renderTexture);
                }

                _renderTexture = new RenderTexture(_renderData.Resolution.x, _renderData.Resolution.y, 0)
                {
                    name = "_RenderTexture Instance"
                };

                _renderCamera.targetTexture = _renderTexture;


            }
        }

        public void ValidateRenderMaterial()
        {
            if (_renderData.RenderLightMaterial == null) return;

            _renderMaterial = new Material(_renderData.RenderLightMaterial)
            {
                mainTexture = _renderTexture
            };

            _renderMaterial.SetColor(DarknessProp, _renderData.Darkness);
            _renderMaterial.SetFloat(HSamples, _renderData.RenderMaterialData.HSamples);
            _renderMaterial.SetFloat(VSamples, _renderData.RenderMaterialData.VSamples);
            _renderMaterial.SetFloat(Radius, _renderData.RenderMaterialData.Radius);

            if (_postProcessOverlay.sharedMaterial != _renderMaterial)
                _postProcessOverlay.sharedMaterial = _renderMaterial;

        }

        public void Reload()
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }
            _renderCamera = null;
            _postProcessOverlay = null;
            _postProcessCamera = null;
        }

        private PixelCameraRenderData _renderData;
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int VSamples = Shader.PropertyToID("_VSamples");
        private static readonly int HSamples = Shader.PropertyToID("_HSamples");

        public void UpdateView(PixelCameraRenderData data)
        {
            _renderData = data;

            ValidateRenderCamera();
            ValidatePostProcessCamera();

            ValidateQuad();
            ValidateRenderTexture();
            ValidateRenderMaterial();
        }
    }
}
