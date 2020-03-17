using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class LightCamera : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Camera cam;
        public Camera RenderCamera;
        public Color Darkness = Color.black;
        public Transform Quad;
        public Vector2 Offset;
        [SerializeField, HideInInspector]
        private Vector2 resolution;
        [SerializeField, HideInInspector]
        private RenderTexture RenderTexture;
        public Material RenderTextureMaterial;

        private void OnValidate()
        {

            UpdateLightView();
        }

        private void OnEnable()
        {
            resolution = new Vector2(Screen.width, Screen.height);

            if (cam == null)
                cam = GetComponent<Camera>();
            if (Quad == null || RenderCamera == null)
                return;

            UpdateLightView();
        }

        private void Start()
        {
            UpdateLightView();
        }

        public void UpdateLightView()
        {
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
