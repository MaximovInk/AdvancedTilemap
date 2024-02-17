using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [ExecuteAlways]
    public class ALiquidChunk : MonoBehaviour
    {
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

        public AChunk chunk;

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
            if (meshData == null)
                meshData = new MeshData();

            if (meshFilter == null)
                meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (chunk == null)
                chunk = GetComponentInParent<AChunk>();

        }

        public void Init(int w, int h, AChunk chunk)
        {
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            this.chunk = chunk;
            settledData = new bool[w * h];
            data = new float[w * h];

            if (meshFilter.sharedMesh == null)
                meshFilter.sharedMesh = meshData.GetMesh();
        }

        public bool meshIsDirty = false;

        public void SetMaterial(Material material)
        {
            if (material == null)
                meshRenderer.sharedMaterial = null;
            else
                meshRenderer.sharedMaterial = new Material(material);
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

                    var topLiquid = chunk.Layer.GetLiquid(ix + chunk.GridX, iy + 1 + chunk.GridY);

                    var liquidLerp = Mathf.Lerp(data[i], topLiquid, 0.5f);

                    meshData.AddSquare(new MeshDataParameters
                    {
                        color = Color.Lerp(chunk.Layer.MinLiquidColor, chunk.Layer.MaxLiquidColor, liquidLerp/4f),
                        uv = ATileUV.Identity,
                        vX0 = ix,
                        vX1 = ix+1,
                        vY0 = iy,
                        vY1 = iy+(topLiquid == 0 ? value : 1),
                        z = 0,
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
