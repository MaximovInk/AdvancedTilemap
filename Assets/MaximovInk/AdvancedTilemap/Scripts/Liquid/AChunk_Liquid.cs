using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {
        [SerializeField] private ALiquidRenderer _liquidRenderer;

        public bool GetSettled(int x, int y)
        {
            return _liquidRenderer.GetSettled(x, y);
        }
        public void SetSettled(int x, int y, bool value)
        {
            _liquidRenderer.SetSettled(x, y, value);
        }

        public float GetLiquid(int gx, int gy)
        {
            return _liquidRenderer.GetLiquid(gx, gy);
        }
        public void SetLiquid(int gx, int gy, float value)
        {
            _liquidRenderer.SetLiquid(gx, gy, value);
        }
        public void AddLiquid(int gx, int gy, float value)
        {
            _liquidRenderer.AddLiquid(gx, gy, value);
        }

        public void UpdateLiquidState()
        {
            if (Layer.LiquidEnabled)
            {
                _liquidRenderer = GetComponentInChildren<ALiquidRenderer>();
                if (_liquidRenderer == null)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(0, 0, 0.05f);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;

                    _liquidRenderer = go.AddComponent<ALiquidRenderer>();
                    _liquidRenderer.Init(CHUNK_SIZE, CHUNK_SIZE, this);
                }
                _liquidRenderer.SetMaterial(Layer.LiquidMaterial);
            }
            else if (_liquidRenderer != null)
            {
                DestroyImmediate(_liquidRenderer.gameObject);
            }
        }

    }
}
