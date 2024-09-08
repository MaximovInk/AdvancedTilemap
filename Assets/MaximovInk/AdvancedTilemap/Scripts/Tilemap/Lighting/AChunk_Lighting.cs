using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {
        [SerializeField] private ALightingChunk lightingChunk;

        public byte GetLight(int x, int y)
        {
            return lightingChunk.GetLight(x, y);
        }

        public void SetLight(int gx, int gy, byte value)
        {
            if(lightingChunk == null)
                UpdateLightingState(true);

            lightingChunk.SetLight(gx,gy, value);
        }

        public void UpdateLightingState(bool active)
        {
            if (active)
            {
                lightingChunk = GetComponentInChildren<ALightingChunk>();

                if (lightingChunk == null)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(0, 0, 0.05f);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;

                    lightingChunk = go.AddComponent<ALightingChunk>();
                    lightingChunk.Init(CHUNK_SIZE, CHUNK_SIZE, this);

                }

                lightingChunk.SetMaterial(Layer.Tilemap.LightMaterial);
            }
            else if (lightingChunk != null)
            {
                DestroyImmediate(lightingChunk.gameObject);
            }
        }

    }
}
