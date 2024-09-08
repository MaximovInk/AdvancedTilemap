using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {
        [SerializeField] private ALiquidChunk liquidChunk;

        public bool GetSettled(int x, int y)
        {
            return liquidChunk.GetSettled(x, y);
        }
        public void SetSettled(int x, int y, bool value)
        {
            liquidChunk.SetSettled(x, y, value);
        }

        public float GetLiquid(int gx, int gy)
        {
            return liquidChunk.GetLiquid(gx, gy);
        }
        public void SetLiquid(int gx, int gy, float value)
        {
            liquidChunk.SetLiquid(gx, gy, value);
        }
        public void AddLiquid(int gx, int gy, float value)
        {
            liquidChunk.AddLiquid(gx, gy, value);
        }

        public void UpdateLiquidState()
        {
            if (Layer.LiquidEnabled)
            {
                liquidChunk = GetComponentInChildren<ALiquidChunk>();
                if (liquidChunk == null)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(0, 0, 0.05f);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;

                    liquidChunk = go.AddComponent<ALiquidChunk>();
                    liquidChunk.Init(CHUNK_SIZE, CHUNK_SIZE, this);
                }
                liquidChunk.SetMaterial(Layer.LiquidMaterial);
            }
            else if (liquidChunk != null)
            {
                DestroyImmediate(liquidChunk.gameObject);
            }
        }

    }
}
