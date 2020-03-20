using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class LightCamera : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;
        public Camera RenderCamera;
        public Color Darkness = Color.black;
        public Transform Quad;
        public Vector2 Offset;
        [SerializeField, HideInInspector]
        private Vector2 resolution;
        [SerializeField]
        private RenderTexture RenderTexture;
        public Material RenderTextureMaterial;

        public LayerMask LightingMask;

        private void OnValidate()
        {

            UpdateLightView();
        }

        private void OnEnable()
        {
            if (RenderCamera == null)
            {
                RenderCamera = new GameObject().AddComponent<Camera>();
                RenderCamera.clearFlags = CameraClearFlags.Color;
                RenderCamera.backgroundColor = Color.black;
                RenderCamera.nearClipPlane = 0f;
                RenderCamera.transform.SetParent(transform);
            }
            if (Quad == null)
            {
                Quad = GameObject.CreatePrimitive(PrimitiveType.Quad).GetComponent<Transform>();
                Quad.transform.SetParent(transform);
            }
            if (cam == null)
            {
                cam = GetComponent<Camera>();
                cam.nearClipPlane = 0f;
                cam.tag = "MainCamera";
            }

            ValidateView();

            resolution = new Vector2(Screen.width, Screen.height);

            UpdateLightView();
        }

        private void Start()
        {
            UpdateLightView();
        }

        private void ValidateView()
        {
            cam.cullingMask &= ~LightingMask;
            RenderCamera.orthographic = true;
            RenderCamera.cullingMask = LightingMask;
            cam.orthographic = true;
            Quad.transform.localPosition = Vector3.zero;
            RenderCamera.transform.localPosition = Vector3.zero;
        }

        public void UpdateLightView()
        {
            if (RenderCamera == null || Quad== null || RenderTextureMaterial == null)
                return;

            ValidateView();

            resolution.x = Screen.width;
            resolution.y = Screen.height;

            float cameraHeight = cam.orthographicSize * 2;
            Vector2 cameraSize = new Vector3(cam.aspect * cameraHeight, cameraHeight, 1);
            Vector2 spriteSize = Vector2.one;

            Vector3 scale = cameraSize / spriteSize + Offset;

            Quad.transform.localPosition = Vector2.zero;
            Quad.transform.localScale = scale;

            RenderCamera.orthographicSize = cam.orthographicSize;
            RenderCamera.orthographic = true;

            if ((RenderTexture == null || RenderTexture.width != Screen.width || RenderTexture.height != Screen.height) && (Screen.width > 0 && Screen.height > 0))
            {
                RenderTexture = new RenderTexture(Screen.width, Screen.height, 0);
                RenderTexture.name = "_RenderTexture Instance";

                RenderCamera.targetTexture = RenderTexture;

                Quad.GetComponent<Renderer>().sharedMaterial = new Material( RenderTextureMaterial);
                Quad.GetComponent<Renderer>().sharedMaterial.mainTexture = RenderTexture;
            }

            Quad.GetComponent<Renderer>().sharedMaterial.SetColor("_Darkness", Darkness);

        }

        private void Update()
        {
            if (((resolution.x != Screen.width || resolution.y != Screen.height) && Screen.width != 0 && Screen.height != 0))
            {
                UpdateLightView();
            }

        }
    }
}
