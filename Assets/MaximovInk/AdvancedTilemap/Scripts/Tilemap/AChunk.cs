using System;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [ExecuteAlways]
    public partial class AChunk : MonoBehaviour
    {
        public event Action GenerationJobsCompleted;

        public const int CHUNK_SIZE = 16;
        public ALayer Layer;
        public int GridX;
        public int GridY;

        public bool IsLoaderActive
        {
            get => _isLoaderActive;
            set
            {
                _isLoaderActive = value;
                UpdateLoadedState();
            }
        }
        public bool CanTrim => !IsLoaderActive && IsEmpty();

        private ChunkProcessor _chunkProcessor;

        public PolygonCollider2D Collider => _collider2D;
        [SerializeField, HideInInspector] private PolygonCollider2D _collider2D;

        [SerializeField] private bool _isLoaderActive;

        private void Awake()
        {
            CheckRenderer();
            CheckValidate();
            UpdateRenderer();
        }

        private void Start()
        {
            _data.IsDirty = true;
        }

        public void Init()
        {
            CheckRenderer();
            CheckValidate();
            UpdateRenderer();
            
            UpdateLiquidState();
            UpdateFlags();

            UpdateLightingState(Layer.Tilemap.Lighting.Enabled);
            ColliderEnabledChange(Layer.ColliderEnabled);

            UpdateLoadedState();
        }

        public void UpdateFlags()
        {
            if (Layer.Tilemap.DisplayChunksInHierarchy)
            {
                gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
            }
            else
            {
                gameObject.hideFlags |= HideFlags.HideInHierarchy;
            }

            tag = Layer.Tag;
            gameObject.layer = Layer.LayerMask;
        }

        private void CheckValidate()
        {
            if (Layer != null ? Layer.Tileset : false)
            {
                var tileUnit = Layer.Tileset.GetTileUnit();
                transform.localPosition = new Vector3(GridX * tileUnit.x, GridY * tileUnit.y);
            }

            if (_meshData == null)
            {
                _meshData = new MeshData();
                _meshFilter.sharedMesh = _meshData.GetMesh();
            }

            if (Layer == null)
            {
                Layer = GetComponentInParent<ALayer>();
            }

            if (_data == null)
            {
                _data = new AChunkData(CHUNK_SIZE, CHUNK_SIZE);
                _data.Fill();
                _data.FillBitmask();
                _data.FillSelfBitmask();
                _data.FillColor(Color.white);
                _data.FillCollision(false);
            }
        }

        private void OnValidate()
        {
            CheckRenderer();
            CheckValidate();
            UpdateRenderer();
        }

        private void Update()
        {
            RealtimeUpdate();
            EditorUpdate();
        }

        private bool _trimInvoke;

        private void RealtimeUpdate()
        {
            if (_trimInvoke)
            {
                _trimInvoke = false;

                if (!Layer.TrimIfNeeded())
                    Layer.CalculateBounds();
            }

            if (_data.IsDirty)
            {
                _data.IsDirty = false;

                ValidateVariations();
                Generate();
            }
        }

        private void EditorUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UpdateRenderer();
#endif
        }

        private void ValidateChunkProcessor()
        {
            if (_chunkProcessor != null) return;

            MakeChunkProcessor();
        }

        private void MakeChunkProcessor()
        {
            _chunkProcessor = new ChunkProcessor(this);
            _chunkProcessor.AddJob(new DummyMeshJob());
            _chunkProcessor.AddJob(new DummyCollisionJob());

        }

        public void Generate()
        {
            _meshData.Clear();

            CheckDataValidate();

            //GENERATE
            ValidateChunkProcessor();

            _persistenceData = new AChunkPersistenceData()
            {
                Layer = Layer,
                Material = Layer.Material,
                Position = new Vector2Int(GridX, GridY)
            };

            _chunkProcessor.ProcessData(new AChunkProcessorData()
            {
                ChunkData = _data,
                ChunkPersistenceData = _persistenceData,
                MeshData = _meshData,
                Chunk = this
            });


            _data.IsDirty = false;

            GenerationJobsCompleted?.Invoke();

            _trimInvoke = true;
        }

        #region Collider

        public void UpdateColliderProperties()
        {
            if (_collider2D == null)
                return;

            _collider2D.sharedMaterial = Layer.PhysicsMaterial2D;
            _collider2D.isTrigger = Layer.IsTrigger;
        }

        public void ColliderEnabledChange(bool active)
        {
            _collider2D = GetComponent<PolygonCollider2D>();

            if(active && _collider2D == null)
            {
                _collider2D = gameObject.AddComponent<PolygonCollider2D>();
                _data.IsDirty = true;
                UpdateCollisionState();
            }

            if(!active && _collider2D != null)
            {
                DestroyImmediate(_collider2D);
            }

        }

        public void UpdateCollisionState()
        {
            _data.FillCollision(false);

            for (int i = 0; i < _data.ArraySize; i++)
            {
                _data.collision[i] = IsCollision(_data.tiles[i]);
            }
        }

        #endregion

        public Bounds GetBounds()
        {
            Bounds bounds = _meshFilter.sharedMesh ? _meshFilter.sharedMesh.bounds : default;

            var tileUnit = Layer.Tileset.GetTileUnit();
            if (bounds == default)
            {
                Vector3 vMinMax = Vector2.Scale(new Vector2(GridX < 0 ? CHUNK_SIZE : 0f, GridY < 0 ? CHUNK_SIZE : 0f), tileUnit);
                bounds.SetMinMax(vMinMax, vMinMax);
            }
            for (int i = 0; i < _data.tiles.Length; ++i)
            {
                if (_data.tiles[i] == 0)
                    continue;

                int gx = i % CHUNK_SIZE;
                if (GridX >= 0) gx++;

                int gy = i / CHUNK_SIZE;
                if (GridY >= 0) gy++;

                Vector2 gridPos = Vector2.Scale(new Vector2(gx, gy), tileUnit);

                bounds.Encapsulate(gridPos);
            }
            return bounds;

        }

        private void UpdateLoadedState()
        {
            var state = IsLoaderActive || !Layer.Tilemap.ChunkLoader.Enabled || !Application.isPlaying;

            if (_meshRenderer.enabled != state)
            {
                _meshRenderer.enabled = state;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (child.gameObject.activeSelf != state)
                {
                    child.gameObject.SetActive(state);
                }
            }


        }
    }
}
