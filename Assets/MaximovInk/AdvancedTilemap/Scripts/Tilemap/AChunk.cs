using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class AChunkData
    {
        public int ArraySize => data.Length;
        public ushort[] data;
        public byte[] bitmaskData;
        public Color32[] colors;
        public bool[] collision;
        public byte[] variations;
        public UVTransform[] transforms;

        public bool IsEmpty
        {
            get
            {
                for (var i = 0; i < ArraySize; i++)
                {
                    if (data[i] > 0) return false;
                }

                return true;
            }
        }

        public AChunkData(int width, int height)
        {
            data = new ushort[width * height];
            bitmaskData = new byte[width * height];
            colors = new Color32[width * height];
            collision = new bool[width * height];
            variations = new byte[width * height];
            transforms = new UVTransform[width * height];
        }

        public void FillCollision(bool value)
        {
            for (var i = 0; i < ArraySize; i++)
            {
                IsDirty |= collision[i] != value;

                collision[i] = value;
            }
        }

        public void FillBitmask(byte value = 0)
        {
            for (var i = 0; i < ArraySize; i++)
            {
                IsDirty |= bitmaskData[i] != value;

                bitmaskData[i] = value;
            }
        }

        public void Fill(ushort value = 0)
        {
            for (var i = 0; i < ArraySize; i++)
            {
                IsDirty |= data[i] != value;

                data[i] = value;
            }
        }

        public void FillColor(Color32 value)
        {
            for (int i = 0; i < ArraySize; i++)
            {
                IsDirty |= !colors[i].Equals(value);

                colors[i] = value;
            }
        }

        public bool IsDirty { get; set; }
    }

    [ExecuteAlways]
    public class AChunk : MonoBehaviour
    {
        public const int CHUNK_SIZE = 32;
        public ALayer layer;
        [FormerlySerializedAs("meshData")] [HideInInspector,SerializeField]
        private MeshData _meshData;

        public MeshData GetMeshData() => _meshData;

        [SerializeField, HideInInspector] private MeshFilter meshFilter;
        [SerializeField, HideInInspector] private MeshRenderer meshRenderer;
        [SerializeField, HideInInspector] private PolygonCollider2D _collider2D;

        [SerializeField,HideInInspector] private AChunkData _data;
        [SerializeField, HideInInspector] private AChunkPersistenceData _persistenceData;
        [SerializeField] private ALiquidChunk liquidChunk;

        private ChunkProcessor _chunkProcessor;

        public bool colliderIsDirty = false;

        public int GridX;
        public int GridY;

        private MaterialPropertyBlock materialProperty;

        private void OnDrawGizmos()
        {
            if (layer == null) layer = GetComponentInParent<ALayer>();

            if (layer.Tileset == null) return;

            if (!layer.ShowChunkBounds) return;

            Gizmos.color = Color.blue;
            var min = transform.position;
            var max = min + (Vector3)layer.Tileset.GetTileUnit() * CHUNK_SIZE;

            Gizmos.DrawLine(min, new Vector3(min.x,max.y));
            Gizmos.DrawLine(new Vector3(min.x, max.y), max);
            Gizmos.DrawLine(max, new Vector3(max.x, min.y));
            Gizmos.DrawLine(new Vector3(max.x, min.y), min);
        }

        private void Awake()
        {
            CheckRenderer();
            CheckValidate();
            UpdateRenderer();
        }

        public void Init()
        {
            CheckRenderer();
            CheckValidate();
            UpdateRenderer();
            ColliderEnabledChange(layer.ColliderEnabled);
            UpdateLiquidState();
        }

        private void CheckValidate()
        {
            if (layer?.Tileset != null)
            {
                var tileUnit = layer.Tileset.GetTileUnit();
                transform.localPosition = new Vector3(GridX * tileUnit.x, GridY * tileUnit.y);
            }

            if (_meshData == null)
            {
                _meshData = new MeshData();
                meshFilter.sharedMesh = _meshData.GetMesh();
            }

            if (layer == null)
            {
                layer = GetComponentInParent<ALayer>();
            }

            if (_data == null)
            {
                _data = new AChunkData(CHUNK_SIZE, CHUNK_SIZE);
                _data.Fill();
                _data.FillBitmask();
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
            if (_data.IsDirty)
            {
                ValidateVariations();
                _data.IsDirty = false;
                Generate();
            }

            if (colliderIsDirty)
            {
                colliderIsDirty = false;
                GenerateCollider(true);
            }

#if UNITY_EDITOR
            if(!Application.isPlaying)
                UpdateRenderer();
#endif
        }

        public void Refresh(bool immediate = false)
        {
            if (immediate)
            {
                _data.IsDirty = true;
                if (layer.UpdateVariationsOnRefresh)
                    GenerateVariations();
            }
            Update();
        }

        #region Mesh

        private void CheckRenderer()
        {
            if (meshFilter == null)
                meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        public void CheckDataValidate()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.data[i] == 0)
                    continue;

                if (_data.data[i] > layer.Tileset.TilesCount)
                    EraseTile(i % CHUNK_SIZE, i / CHUNK_SIZE);
            }
        }

        public void UpdateRenderer()
        {
            CheckRenderer();

            if (layer == null) return;

            meshRenderer.sharedMaterial = layer.Material;

            if (materialProperty == null)
                materialProperty = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(materialProperty);

            if (layer.Tileset?.Texture == null)
                return;


            materialProperty.SetTexture("_MainTex", layer.Tileset.Texture);
            materialProperty.SetColor("_Color", layer.TintColor);

            meshRenderer.SetPropertyBlock(materialProperty);

            //meshRenderer.sharedMaterial.SetTexture("_MainTex", layer.Tileset.Texture);

        }

        public void ValidateVariations()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.data[i] == 0) continue;

                var tile = layer.Tileset.GetTile(_data.data[i]);

                _data.variations[i] = tile.ValidateVariationID(_data.variations[i]);
            }
        }

        public void GenerateVariations()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.data[i] == 0) continue;

                _data.variations[i] = layer.Tileset.GetTile(_data.data[i]).GenVariation();
            }
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
        }


        public void Generate()
        {
            _meshData.Clear();

            CheckDataValidate();

            //GENERATE
            ValidateChunkProcessor();

            _persistenceData = new AChunkPersistenceData()
            {
                Layer = layer,
                Material =  layer.Material,
                Position = new Vector2Int(GridX,GridY)

            };

            _chunkProcessor.ProcessData(new AChunkProcessorData()
            {
                ChunkData = _data,
                ChunkPersistenceData = _persistenceData,
                MeshData = _meshData
            });

        }

        #endregion

        #region data

        public void FillData(ushort tileID)
        {
            _data.Fill(tileID);
        }

        public void FillColor(Color32 color)
        {
            _data.FillColor(color);
        }

        public ushort GetTile(int x, int y)
        {
            return _data.data[x + y * CHUNK_SIZE];
        }

        public bool SetTile(int x, int y, ushort tileID,UVTransform transform = default)
        {
            var variation = layer.Tileset.GetTile(tileID).GenVariation();
            int idx = x + y * CHUNK_SIZE;

            if (_data.data[idx] == tileID && _data.transforms[idx] == transform)
            {
                if (_data.variations[idx] != variation)
                {
                    _data.variations[idx] = variation;
                    _data.IsDirty = true;
                }

                return false;
            }

            _data.data[idx] = tileID;

            _data.collision[idx] 
                = (tileID > 0) && !layer.Tileset.GetTile(tileID).ColliderDisabled;

            _data.transforms[idx] = transform;

            _data.variations[idx] = layer.Tileset.GetTile(tileID).GenVariation();

            colliderIsDirty = true;
            _data.IsDirty = true;

            return true;
        }

        public bool EraseTile(int x, int y)
        {
            if (_data.data[x + y * CHUNK_SIZE] == 0) return false;

            _data.data[x + y * CHUNK_SIZE] = 0;
            _data.collision[x + y * CHUNK_SIZE] = false;

            _data.IsDirty = true; 
            colliderIsDirty = true;

            return true;
        }

        public byte GetBitmask(int x, int y)
        {
            return _data.bitmaskData[x + y * CHUNK_SIZE];
        }

        public void SetBitmask(int x, int y, byte bitmask)
        {
            _data.bitmaskData[x + y * CHUNK_SIZE] = bitmask;

            _data.IsDirty = true; 
        }

        public void SetColor(int x,int y, Color32 color)
        {
            bool changed = !_data.colors[x + y * CHUNK_SIZE].Equals(color);
            _data.colors[x + y * CHUNK_SIZE] = color;
            _data.IsDirty |= changed;
        }

        public Color32 GetColor(int x,int y)
        {
            return _data.colors[x + y * CHUNK_SIZE];
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < _data.data.Length; i++)
            {
                if (_data.data[i] != 0)
                    return false;
            }

            return true;
        }

        public void SetVariation(int x,int y, byte variationID)
        {

        }

        public byte GetVariation(int x,int y)
        {
            return _data.variations[x + y * CHUNK_SIZE];
        }

        public void GenVariation(int x,int y)
        {
            int idx = x + y * CHUNK_SIZE;

            var tileID = _data.data[idx];

            if (tileID == 0) {         
                _data.variations[idx] = 0; return;
            }

            _data.variations[idx] = layer.Tileset.GetTile(tileID).GenVariation();
        }

        #endregion

        #region Collider

        public void ColliderEnabledChange(bool active)
        {

            _collider2D = GetComponent<PolygonCollider2D>();

            if(active && _collider2D == null)
            {
                _collider2D = gameObject.AddComponent<PolygonCollider2D>();
                colliderIsDirty = true;
            }

            if(!active && _collider2D != null)
            {
                DestroyImmediate(_collider2D);
            }
        }

        public void GenerateCollider(bool immediate)
        {
            if (_collider2D == null) return;

            if (!immediate)
            {
                colliderIsDirty = true;
                return;
            }

            _collider2D.pathCount = 0;

            List<ColliderSegment> segments = GetSegments();
            List<List<Vector2>> paths = FindPath(segments);
            paths = ScaleToTiles(paths);

            _collider2D.pathCount = paths.Count;
            for (int i = 0; i < paths.Count; i++)
            {
                _collider2D.SetPath(i, paths[i].ToArray());
            }
        }

        private List<List<Vector2>> ScaleToTiles(List<List<Vector2>> input)
        {
            var scale = (Vector2)layer.Tileset.TileSize / (Vector2)layer.Tileset.PixelPerUnit;

            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Count; j++)
                {
                    var p = input[i][j];


                    input[i][j] = new Vector2(p.x * scale.x, p.y * scale.y);
                }
            }

            return input;

        }

        private List<List<Vector2>> FindPath(List<ColliderSegment> segments)
        {
            List<List<Vector2>> paths = new List<List<Vector2>>();

            while(segments.Count > 0)
            {
                Vector2 currentPoint = segments[0].point2;
                List<Vector2> currentPath = new List<Vector2> { segments[0].point1, segments[0].point2 };
                segments.Remove(segments[0]);

                bool pathComplete = false;
                while (!pathComplete)
                {
                    pathComplete = true;
                    for (int s = 0; s < segments.Count; s++)
                    {
                        if(segments[s].point1 == currentPoint)
                        {
                            pathComplete = false;
                            currentPath.Add(segments[s].point2);
                            currentPoint = segments[s].point2;
                            segments.Remove(segments[s]);
                        } else if(segments[s].point2 == currentPoint)
                        {
                            pathComplete = false;
                            currentPath.Add(segments[s].point1);
                            currentPoint = segments[s].point1;
                            segments.Remove(segments[s]);
                        }
                    }
                }
                paths.Add(currentPath);
            }
            return paths;
        }

        private List<ColliderSegment> GetSegments()
        {
            List<ColliderSegment> segments = new List<ColliderSegment>();

            for (int i = 0; i < _data.data.Length; i++)
            {
                if (!_data.collision[i])
                    continue;

                int x = i % CHUNK_SIZE;
                int y = i / CHUNK_SIZE;
                //top
                if(y + 1 >= CHUNK_SIZE || !_data.collision[x + (y+1) * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x,y+1), new Vector2(x+1,y+1)));
                }
                //bottom
                if (y - 1 < 0 || !_data.collision[x + (y - 1) * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x + 1, y)));
                }
                //right
                if (x + 1 >= CHUNK_SIZE || !_data.collision[x + 1 + y * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x+1, y), new Vector2(x + 1, y + 1)));
                }
                //left
                if (x - 1 < 0 || !_data.collision[x - 1 + y * CHUNK_SIZE])
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x, y + 1)));
                }
            }

            return segments;
        }

        private struct ColliderSegment
        {
            public Vector2 point1;
            public Vector2 point2;

            public ColliderSegment(Vector2 point1, Vector2 point2)
            {
                this.point1 = point1;
                this.point2 = point2;
            }
        }

        #endregion

        public Bounds GetBounds()
        {
            Bounds bounds = meshFilter.sharedMesh ? meshFilter.sharedMesh.bounds : default;
            if (bounds == default)
            {
                Vector3 vMinMax = Vector2.Scale(new Vector2(GridX < 0 ? CHUNK_SIZE : 0f, GridY < 0 ? CHUNK_SIZE : 0f), Vector3.one);
                bounds.SetMinMax(vMinMax, vMinMax);
            }
            for (int i = 0; i < _data.data.Length; ++i)
            {
                if (_data.data[i] == 0)
                    continue;

                int gx = i % CHUNK_SIZE;
                if (GridX >= 0) gx++;

                int gy = i / CHUNK_SIZE;
                if (GridY >= 0) gy++;

                Vector2 gridPos = Vector2.Scale(new Vector2(gx, gy), Vector3.one);

                bounds.Encapsulate(gridPos);
            }
            return bounds;

        }

        #region Liquid

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
            if (layer.LiquidEnabled)
            {
                liquidChunk = GetComponentInChildren<ALiquidChunk>();
                if(liquidChunk == null)
                {
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    go.transform.localPosition = new Vector3(0,0,0.05f);
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;

                    liquidChunk = go.AddComponent<ALiquidChunk>();
                    liquidChunk.Init(CHUNK_SIZE, CHUNK_SIZE, this);
                }
                liquidChunk.SetMaterial(layer.LiquidMaterial);
            }
            else if (liquidChunk != null)
            {
               DestroyImmediate(liquidChunk.gameObject);
            }
        }

        #endregion
    }
}
