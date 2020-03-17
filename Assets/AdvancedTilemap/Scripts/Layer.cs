using AdvancedTilemap.Liquid;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedTilemap
{
    [ExecuteInEditMode]
    public class Layer : MonoBehaviour
    {
        public ATilemap Tilemap;
        public Tileset Tileset;
        public LayerMask LayerMask { get { return layerMask; } set { layerMask = value; UpdateChunksFlags(); } }
        public PhysicsMaterial2D PhysicsMaterial2D { get { return physMaterial; } set { physMaterial = value; UpdateColliderProperties(); } }
        public bool IsTrigger { get { return isTrigger; } set { isTrigger = value; UpdateColliderProperties(); } }
        public bool ColliderEnabled { get => colliderGeneration; set { colliderGeneration = value; UpdateCollider(); } }
        public bool LiquidEnabled { get => liquidEnabled; set { liquidEnabled = value; UpdateLiquid(); } }
        public Material Material { get => material; set { material = value; UpdateRenderer(true, true, true); } }
        public Material LiquidMaterial { get => liquidMaterial; set { liquidMaterial = value; UpdateLiquid(); } }
        public Color TintColor { get => tintColor; set { tintColor = value; UpdateRenderer(color: true); } }
        public string Tag { get { return _tag; } set { _tag = value; UpdateChunksFlags(); } }
        public float ZOrder;

        public bool TrimInvoke;

        [HideInInspector, SerializeField]
        private LayerMask layerMask;
        [HideInInspector, SerializeField]
        private bool isTrigger;
        [HideInInspector, SerializeField]
        private PhysicsMaterial2D physMaterial;
        [HideInInspector, SerializeField]
        private string _tag = "Untagged";
        [HideInInspector, SerializeField]
        private Color tintColor = Color.white;
        [HideInInspector, SerializeField]
        private Material material;
        [HideInInspector, SerializeField]
        private Material liquidMaterial;
        [HideInInspector, SerializeField]
        private bool colliderGeneration = false;
        [HideInInspector, SerializeField]
        private bool liquidEnabled = false;

        [HideInInspector,SerializeField]
        public int Index;

        public int MinGridX { get; private set; }
        public int MinGridY { get; private set; }
        public int MaxGridX { get; private set; }
        public int MaxGridY { get; private set; }

        public Dictionary<uint, Chunk> chunksCache = new Dictionary<uint, Chunk>();

        [HideInInspector,SerializeField]
        private int childCount;

        private void Awake()
        {
            BuildChunkCache();

            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateMesh();
                chunk.Value.UpdateRenderer(true, true, true);
            }

            CalculateBounds();
        }

        private void OnValidate()
        {
            BuildChunkCache();
        }

        private void UpdateLiquid()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateLiquid();
            }
        }

        public void CalculateBounds()
        {

            MaxGridX = MinGridX = MaxGridY = MinGridX = 0;
            Bounds bounds = new Bounds();

            foreach (var chunk in chunksCache)
            {

                Bounds chunkBounds = chunk.Value.GetBounds();
                Vector2 min = transform.InverseTransformPoint(chunk.Value.transform.TransformPoint(chunkBounds.min));
                Vector2 max = transform.InverseTransformPoint(chunk.Value.transform.TransformPoint(chunkBounds.max));
                bounds.Encapsulate(min + Vector2.one * 0.5f);
                bounds.Encapsulate(max - Vector2.one * 0.5f);
            }

            MinGridX = Utilites.GetGridX(bounds.min);
            MinGridY = Utilites.GetGridY(bounds.min);
            MaxGridX = Utilites.GetGridX(bounds.min);
            MaxGridY = Utilites.GetGridY(bounds.min);
        }

        public void RefreshAll(bool immediate = false)
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.RefreshAll(immediate);
            }
        }

        public void UpdateMesh(bool immediate = false)
        {
            foreach (var chunk in chunksCache)
            {
                if (immediate)
                    chunk.Value.UpdateMeshImmediate();
                else
                    chunk.Value.UpdateMesh();
            }
        }

        public void UpdateRenderer(bool material = false, bool color = false, bool texture = false)
        {
            BuildChunkCache();
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateRenderer(material, color, texture);
            }
        }

        public void Clear()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }

        public void Trim()
        {
            foreach (var chunk in chunksCache)
            {
                if (chunk.Value.IsEmpty())
                {
                    DestroyImmediate(chunk.Value.gameObject);
                }
            }

            BuildChunkCache();
            CalculateBounds();
        }

        public void UpdateColliderProperties()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateColliderProperties();
            }
        }

        public void UpdateChunksFlags()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateFlags();
            }
        }

        public void UpdateCollider()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateColliderComponent();
            }
        }

        private void BuildChunkCache()
        {
            chunksCache.Clear();
            for (int i = 0; i < transform.childCount; ++i)
            {
                Chunk chunk = transform.GetChild(i).GetComponent<Chunk>();
                if (chunk)
                {
                    int chunkX = (chunk.GridPosX < 0 ? (chunk.GridPosX + 1 - ATilemap.CHUNK_SIZE) : chunk.GridPosX) / ATilemap.CHUNK_SIZE;
                    int chunkY = (chunk.GridPosY < 0 ? (chunk.GridPosY + 1 - ATilemap.CHUNK_SIZE) : chunk.GridPosY) / ATilemap.CHUNK_SIZE;
                    uint key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));
                    chunksCache[key] = chunk;
                }
            }
        }

        public Chunk GetOrCreateChunk(int gx, int gy,bool autoCreate = true)
        {
            if (chunksCache.Count == 0 && childCount > 0)
                BuildChunkCache();

            int chunkX = (gx < 0 ? (gx + 1 - ATilemap.CHUNK_SIZE) : gx) / ATilemap.CHUNK_SIZE;
            int chunkY = (gy < 0 ? (gy + 1 - ATilemap.CHUNK_SIZE) : gy) / ATilemap.CHUNK_SIZE;
            uint key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));

            Chunk chunk;

            chunksCache.TryGetValue(key, out chunk);

            if (chunk == null && autoCreate)
            {
                var go = new GameObject();
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(chunkX * ATilemap.CHUNK_SIZE, chunkY * ATilemap.CHUNK_SIZE);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                chunk = go.AddComponent<Chunk>();
                chunk.Init(chunkX * ATilemap.CHUNK_SIZE, chunkY * ATilemap.CHUNK_SIZE, this);
                chunk.UpdateRenderer(true, true, true);
                chunksCache[key] = chunk;
            }

            return chunk;
        }

        private void Update()
        {
            childCount = transform.childCount;

            if (!Application.isPlaying)
                return;

            foreach (var chunk in chunksCache)
            {
                if (chunk.Value.Loaded != chunk.Value.gameObject.activeSelf)
                {
                    chunk.Value.gameObject.SetActive(chunk.Value.Loaded);
                }
            }

            if (TrimInvoke)
            {
                Trim();
                TrimInvoke = false;
            }
        }

        #region liquid_physics
        
        private bool GetSettled(int x, int y)
        {
            var chunk = GetOrCreateChunk(x, y, false);
            if (chunk == null)
                return false;
            int chunkGridX = (x < 0 ? -x - 1 : x) % ATilemap.CHUNK_SIZE;
            int chunkGridY = (y < 0 ? -y - 1 : y) % ATilemap.CHUNK_SIZE;
            if (x < 0) chunkGridX = ATilemap.CHUNK_SIZE - 1 - chunkGridX;
            if (y < 0) chunkGridY = ATilemap.CHUNK_SIZE - 1 - chunkGridY;
            return chunk.GetSettled(chunkGridX, chunkGridY);
        }

        private void SetSettled(int x, int y, bool value)
        {
            var chunk = GetOrCreateChunk(x, y, false);
            if (chunk == null)
                return;
            int chunkGridX = (x < 0 ? -x - 1 : x) % ATilemap.CHUNK_SIZE;
            int chunkGridY = (y < 0 ? -y - 1 : y) % ATilemap.CHUNK_SIZE;
            if (x < 0) chunkGridX = ATilemap.CHUNK_SIZE - 1 - chunkGridX;
            if (y < 0) chunkGridY = ATilemap.CHUNK_SIZE - 1 - chunkGridY;
            chunk.SetSettled(chunkGridX, chunkGridY,value);
        }

        private float CalculateVerticalFlowValue(float remainingLiquid, float destination)
        {
            float sum = remainingLiquid + destination;
            float value = 0;

            if (sum <= LiquidChunk.MAX_VALUE)
            {
                value = LiquidChunk.MAX_VALUE;
            }
            else if (sum < 2 * LiquidChunk.MAX_VALUE + LiquidChunk.MAX_COMPRESSION)
            {
                value = (LiquidChunk.MAX_VALUE * LiquidChunk.MAX_VALUE + sum * LiquidChunk.MAX_COMPRESSION) / (LiquidChunk.MAX_VALUE + LiquidChunk.MAX_COMPRESSION);
            }
            else
            {
                value = (sum + LiquidChunk.MAX_COMPRESSION) / 2f;
            }

            return value;
        }

        public void SimulateLiquid(Vector2Int min, Vector2Int max)
        {
            Parallel.For(min.x, max.x, (int x) =>
            {
                for (int y = min.y; y < max.y; ++y)
                {
                    SimulateCell(x, y);
                }
            });
            Parallel.For(min.x, max.x, (int x) =>
            {
                for (int y = min.y; y < max.y; ++y)
                {
                    if (GetLiquid(x, y) < LiquidChunk.MIN_VALUE)
                    {
                        SetSettled(x, y, false);
                    }
                }
            });
        }

        private bool IsEmpty(int x, int y)
        {
            return Tilemap.GetTile(x, y, Index) == 0 && ATilemap.MIN_LIQUID_Y < y;
        }

        private float GetLiquid(int x, int y) =>
            Tilemap.GetLiquid(x, y, Index);

        private void SetLiquid(int x, int y, float value)=>
            Tilemap.SetLiquid(x, y, value, Index);

        private void AddLiquid(int x, int y, float value) =>
            Tilemap.AddLiquid(x, y, value, Index);

        private void SimulateCell(int x, int y)
        {
            if (!IsEmpty(x, y))
            {
                if (GetLiquid(x, y) != 0)
                    SetLiquid(x, y, 0);
                return;
            }

            var liquidValue = GetLiquid(x, y);

            if (liquidValue == 0)
                return;
            if (GetSettled(x, y))
                return;
            if (liquidValue < LiquidChunk.MIN_VALUE)
            {
                SetLiquid(x, y, 0);
                return;
            }

            var startValue = liquidValue;
            var remainingValue = startValue;
            var flow = 0f;

            //Bottom
            if (IsEmpty(x, y - 1))
            {
                var bLiquid = GetLiquid(x, y - 1);
                flow = CalculateVerticalFlowValue(startValue, bLiquid) - bLiquid;
                if (bLiquid > 0 && flow > LiquidChunk.MIN_FLOW)
                    flow *= LiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(LiquidChunk.MAX_FLOW, startValue))
                    flow = Mathf.Min(LiquidChunk.MAX_FLOW, startValue);

                if (flow != 0)
                {
                    remainingValue -= flow;

                    SetSettled(x, y - 1, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x, y - 1, flow);

                }
            }

            if (remainingValue < LiquidChunk.MIN_VALUE)
            {
                AddLiquid(x,y,-remainingValue);
                return;
            }
            //Left
            if (IsEmpty(x - 1, y))
            {
                flow = (remainingValue - GetLiquid(x - 1, y)) / 4f;
                if (flow > LiquidChunk.MIN_FLOW)
                    flow *= LiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(LiquidChunk.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(LiquidChunk.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;
                    SetSettled(x - 1, y, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x - 1, y, flow);

                }
            }

            if (remainingValue < LiquidChunk.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }
            //Right

            if (IsEmpty(x + 1, y))
            {
                flow = (remainingValue - GetLiquid(x + 1, y)) / 3f;
                if (flow > LiquidChunk.MIN_FLOW)
                    flow *= LiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(LiquidChunk.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(LiquidChunk.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;
                    SetSettled(x + 1, y, false);
                    AddLiquid(x, y,-flow);
                    AddLiquid(x + 1, y, flow);

                }
            }

            if (remainingValue < LiquidChunk.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }
            //Top

            if (IsEmpty(x, y + 1))
            {
                flow = remainingValue - CalculateVerticalFlowValue(remainingValue, GetLiquid(x, y + 1));
                if (flow > LiquidChunk.MIN_FLOW)
                    flow *= LiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(LiquidChunk.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(LiquidChunk.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;

                    SetSettled(x, y + 1, false);
                    AddLiquid(x,y,-flow);
                    AddLiquid(x, y+1, flow);

                }
            }

            if (remainingValue < LiquidChunk.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            if (startValue-remainingValue < LiquidChunk.STABLE_FLOW)
            {
                SetSettled(x, y, true);
            }
            else
            {
                SetSettled(x + 1, y, false);

                SetSettled(x - 1, y, false);

                SetSettled(x, y + 1, false);

                SetSettled(x, y - 1, false);
            }

            /* if (startValue == remainingValue)
             {
                 var newSettleCount = (byte)(GetSettleCount(x, y) + 1);
                 SetSettleCount(x, y, newSettleCount);
                 if (newSettleCount >= 10)
                 {
                     SetSettled(x, y, true);
                 }
             }
             else
             {
                 SetSettled(x + 1, y, false);

                 SetSettled(x - 1, y, false);

                 SetSettled(x, y + 1, false);

                 SetSettled(x, y - 1, false);
             }*/

        }


        #endregion
    }
}
