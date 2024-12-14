using UnityEngine;

namespace MaximovInk.MKRendering
{
    
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class MKCamera : MonoBehaviour
    {
        [SerializeField] private MKCameraSettings _settings;

        [SerializeField, HideInInspector] private Camera _postProcessCamera;
        [SerializeField, HideInInspector] private Camera _lightCamera;
        [SerializeField, HideInInspector] private Renderer _postProcessOverlay;
        [SerializeField, HideInInspector] private RenderTexture _renderTexture;

        private Vector2 _cameraSize;
        private Material _renderMaterial;

        private MKCameraRenderData _renderData;

        private static readonly int DarknessProp = Shader.PropertyToID("_Darkness");
        private static readonly int Radius = Shader.PropertyToID("_Radius");
        private static readonly int VSamples = Shader.PropertyToID("_VSamples");
        private static readonly int HSamples = Shader.PropertyToID("_HSamples");

        private void OnValidate()
        {
            UpdateView();
        }

        private void ValidatePostProcessCamera()
        {
            if (_postProcessCamera == null)
            {
                _postProcessCamera = GetComponent<Camera>();
            }

            _postProcessCamera.nearClipPlane = 0f;
            _postProcessCamera.orthographic = true;

            float camH = _postProcessCamera.orthographicSize * 2f;
            var aspect = _postProcessCamera.aspect;
            _cameraSize = new Vector2(aspect * camH, camH);

            _renderData.ScaledResolution = new Vector2Int((int)(_renderData.ScaledResolution.y * aspect), (int)(_renderData.ScaledResolution.y));

            _postProcessCamera.cullingMask &= ~_renderData.CameraSettings.Lighting.LightingMask;
        }

        private void ValidateLightCamera()
        {
            if (_lightCamera == null)
            {
                _lightCamera = new GameObject().AddComponent<Camera>();
                _lightCamera.name = "RenderCamera Instance";
            }

            _lightCamera.clearFlags = CameraClearFlags.Color;
            _lightCamera.backgroundColor = Color.black;
            _lightCamera.nearClipPlane = 0f;
            _lightCamera.orthographicSize = _postProcessCamera.orthographicSize;
            _lightCamera.orthographic = true;
            _lightCamera.cullingMask = _renderData.CameraSettings.Lighting.LightingMask;

            var rcTransform = _lightCamera.transform;
            rcTransform.SetParent(transform);
            rcTransform.localPosition = new Vector3(0, 0, 0.02f);
        }

        private void ValidateQuad()
        {
            if (_postProcessOverlay == null)
            {
                _postProcessOverlay = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<Renderer>();
                _postProcessOverlay.name = "Overlay Instance";
            }

            var spriteSize = Vector2.one;
            var scale = (_cameraSize / spriteSize) + _renderData.CameraSettings.Offset;

            Transform quadTransform;

            (quadTransform = _postProcessOverlay.transform).SetParent(transform);
            quadTransform.localPosition = new Vector3(0, 0, 0.01f);
            quadTransform.localScale = scale;
        }

        public void ValidateRenderTexture()
        {
            if ((_renderTexture != null && _renderTexture.width == _renderData.ScaledResolution.x &&
                 _renderTexture.height == _renderData.ScaledResolution.y) ||
                (_renderData.ScaledResolution.x <= 0 || _renderData.ScaledResolution.y <= 0)) return;

            if (_renderTexture != null)
            {
                _lightCamera.targetTexture = null;
                DestroyImmediate(_renderTexture);
            }

            _renderTexture = new RenderTexture(_renderData.ScaledResolution.x, _renderData.ScaledResolution.y, 0)
            {
                name = "_RenderTexture Instance"
            };

            _lightCamera.targetTexture = _renderTexture;
        }

        private Color GetDarkness()
        {
            var lighting = _renderData.CameraSettings.Lighting;

            if (Application.isPlaying)
            {
                return lighting.Darkness;
            }

            return Color.clear;
        }

        public void ValidateRenderMaterial()
        {
            var lighting = _renderData.CameraSettings.Lighting;

            if (lighting.RenderLightMaterial == null) return;

            _renderMaterial = new Material(lighting.RenderLightMaterial)
            {
                mainTexture = _renderTexture
            };

            _renderMaterial.SetColor(DarknessProp, GetDarkness());
            _renderMaterial.SetFloat(HSamples, lighting.Blur.HSamples);
            _renderMaterial.SetFloat(VSamples, lighting.Blur.VSamples);
            _renderMaterial.SetFloat(Radius, lighting.Blur.Radius);

            if (_postProcessOverlay.sharedMaterial != _renderMaterial)
                _postProcessOverlay.sharedMaterial = _renderMaterial;

        }
       
        public void UpdateView()
        {

            var res = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            var scaledRes = new Vector2Int((int)(res.x * _settings.ResolutionScale), (int)(res.y * _settings.ResolutionScale));

            _renderData = new MKCameraRenderData()
            {
                CameraSettings = _settings,
                Resolution = res,
                ScaledResolution = scaledRes
            };

            ValidatePostProcessCamera();
            ValidateLightCamera();
            ValidateQuad();
            ValidateRenderTexture();
            ValidateRenderMaterial();
        }

        public void Reset()
        {
            foreach (Transform child in transform)
            {
                DestroyImmediate(child.gameObject);
            }

            _lightCamera = null;
            _postProcessOverlay = null;
            _postProcessCamera = null;
        }

        private void Update()
        {
            var res = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);

            var scaledRes = new Vector2Int((int)(res.x * _settings.ResolutionScale), (int)(res.y * _settings.ResolutionScale));

            if (scaledRes != _renderData.ScaledResolution)
            {
                UpdateView();
            }


        }

    }

}
