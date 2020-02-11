using System;
using System.Collections.Generic;
using System.Linq;
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
        public Material Material { get => material; set { material = value; UpdateRenderer(true, true, true); } }
        public Color TintColor { get => tintColor; set { tintColor = value; UpdateRenderer(color: true); } }
        public string Tag { get { return _tag; } set { _tag = value; UpdateChunksFlags(); } }
        public float ZOrder;

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
        private bool colliderGeneration = false;

        [HideInInspector,SerializeField]
        public int Index;

        public int MinGridX { get; private set; }
        public int MinGridY { get; private set; }
        public int MaxGridX { get; private set; }
        public int MaxGridY { get; private set; }

        public Dictionary<uint, Chunk> chunksCache = new Dictionary<uint, Chunk>();

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

            MinGridX = Utils.GetGridX(bounds.min);
            MinGridY = Utils.GetGridY(bounds.min);
            MaxGridX = Utils.GetGridX(bounds.min);
            MaxGridY = Utils.GetGridY(bounds.min);
        }

        public void RefreshAll(bool immediate = false)
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.RefreshAll(immediate);
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
            if (chunksCache.Count == 0 && transform.childCount > 0)
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

       
    }
}
