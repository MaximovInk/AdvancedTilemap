
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [ExecuteAlways]
    public class ALiquidChunk : MonoBehaviour
    {
        public bool meshIsDirty = false;

        [HideInInspector, SerializeField]
        private float[] data;
        [HideInInspector, SerializeField]
        private bool[] settledData;
        [HideInInspector, SerializeField]
        private MeshFilter meshFilter;
        [HideInInspector, SerializeField]
        private MeshRenderer meshRenderer;
        [HideInInspector, SerializeField]
        private MeshData meshData;

        public AChunk Chunk;

        public const float MAX_VALUE = 1F;
        public const float MIN_VALUE = 0.005F;

        public const float MAX_COMPRESSION = 0.25F;

        public const float MIN_FLOW = 0.005F;
        public const float MAX_FLOW = 4F;

        public const float FLOW_SPEED = 1F;

        public const float STABLE_FLOW = 0.0001F;

        public const int MIN_LIQUID_Y = -100;

        private void OnEnable()
        {
            meshData ??= new MeshData();

            if (meshFilter == null)
                meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (Chunk == null)
                Chunk = GetComponentInParent<AChunk>();

        }

        public void Init(int w, int h, AChunk chunk)
        {
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            this.Chunk = chunk;
            settledData = new bool[w * h];
            data = new float[w * h];

            if (meshFilter.sharedMesh == null)
                meshFilter.sharedMesh = meshData.GetMesh();
        }


        public void SetMaterial(Material material)
        {
            meshRenderer.sharedMaterial = material == null ?
                null : new Material(material);
        }

        public bool GetSettled(int x, int y)
        {
            return settledData[x + y * AChunk.CHUNK_SIZE];
        }

        public void SetSettled(int x, int y, bool value)
        {
            settledData[x + y * AChunk.CHUNK_SIZE] = value;
            meshIsDirty = true;
        }

        public void AddLiquid(int x, int y, float value)
        {
            data[x + y * AChunk.CHUNK_SIZE] += value;
            settledData[x + y * AChunk.CHUNK_SIZE] = false;

            meshIsDirty = true;
        }

        public float GetLiquid(int x, int y)
        {
            return data[x + y * AChunk.CHUNK_SIZE];
        }

        public void SetLiquid(int x, int y, float value)
        {
            meshIsDirty = true;
            data[x + y * AChunk.CHUNK_SIZE] = value;
        }

        public void ApplyData()
        {
            meshData.ApplyData();

            meshFilter.mesh = meshData.GetMesh();
        }

        public void GenerateMesh()
        {
            meshData.Clear();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > 0)
                {
                    int ix = i % AChunk.CHUNK_SIZE;
                    int iy = i / AChunk.CHUNK_SIZE;
                    float value = Mathf.Clamp01(data[i]);

                    var topLiquid = Chunk.Layer.GetLiquid(ix + Chunk.GridX, iy + 1 + Chunk.GridY);

                    var liquidLerp = Mathf.Lerp(data[i], topLiquid, 0.5f);

                    meshData.AddSquare(new MeshDataParameters
                    {
                        color = Color.Lerp(Chunk.Layer.MinLiquidColor, Chunk.Layer.MaxLiquidColor, liquidLerp/4f),
                        uv = ATileUV.Identity,
                        vX0 = ix,
                        vX1 = ix+1,
                        vY0 = iy,
                        vY1 = iy+(topLiquid == 0 ? value : 1),
                        z = Chunk.Layer.Tileset.TilesCount * ATilemap.Z_TILE_OFFSET + 0.01f,
                        unit = Chunk.Layer.Tileset.GetTileUnit()
                    });
                }
            }
        }

        private void LateUpdate()
        {
            if (meshIsDirty)
            {
                meshIsDirty = false;

                GenerateMesh();
                ApplyData();
            }
        }
    }
}
