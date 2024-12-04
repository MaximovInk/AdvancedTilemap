using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {
        [HideInInspector,SerializeField]
        private ALightRenderer _lightRenderer;

        public byte GetLight(int x, int y)
        {
            return _lightRenderer.GetLight(x, y);
        }

        public void SetLight(int gx, int gy, byte value)
        {
             if (_lightRenderer == null)
                 UpdateLightingState(true);

             _lightRenderer.SetLight(gx, gy, value);
        }

        public void UpdateLightingState(bool active)
        {
            if (active)
            {
                if(_lightRenderer == null)
                    _lightRenderer = GetComponentInChildren<ALightRenderer>();

                if (_lightRenderer == null)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(0, 0, 0.05f);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;

                    _lightRenderer = go.AddComponent<ALightRenderer>();
                    _lightRenderer.Init(CHUNK_SIZE, CHUNK_SIZE, this);

                }

                _lightRenderer.SetMaterial(Layer.Tilemap.Lighting.LightMaterial);
                _lightRenderer.gameObject.layer = Layer.Tilemap.Lighting.LightingMask;

            }
            else if (_lightRenderer != null)
            {
                DestroyImmediate(_lightRenderer.gameObject);
            }

        }

    }
}
