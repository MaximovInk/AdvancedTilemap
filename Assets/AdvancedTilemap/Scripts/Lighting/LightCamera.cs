using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class LightCamera : MonoBehaviour
    {
        private Camera cam;
        public Camera RenderCamera;
        public Color Darkness = Color.black;
        public Transform Quad;
        public Vector2 Offset;
        private Vector2 resolution;
        private RenderTexture renderTexture;

        private void OnEnable()
        {
            resolution = new Vector2(Screen.width, Screen.height);

            if (cam == null)
                cam = GetComponent<Camera>();
            if (Quad == null || RenderCamera == null)
                return;

            UpdateLightView();
        }

        private void UpdateLightView()
        {


            float cameraHeight = cam.orthographicSize * 2;
            Vector2 cameraSize = new Vector3(cam.aspect * cameraHeight, cameraHeight, 1);
            Vector2 spriteSize = Vector2.one;

            Vector3 scale = cameraSize / spriteSize + Offset;

            Quad.transform.localPosition = Vector2.zero;
            Quad.transform.localScale = scale;

            RenderCamera.orthographicSize = cam.orthographicSize;
            RenderCamera.orthographic = true;

            if (renderTexture == null || renderTexture.width != Screen.width || renderTexture.height != Screen.height)
            {
                renderTexture = new RenderTexture(Screen.width,Screen.height,0);
                renderTexture.name = "_RenderTexture Instance";

                RenderCamera.targetTexture = renderTexture;
                Quad.GetComponent<Renderer>().sharedMaterial.mainTexture = renderTexture;
            }
        }

        private void Update()
        {
            if (resolution.x != Screen.width || resolution.y != Screen.height)
            {
                // do stuff
                UpdateLightView();
                resolution.x = Screen.width;
                resolution.y = Screen.height;
            }        
        }
    }
}
