using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [ExecuteAlways]
    public class ALayer : MonoBehaviour
    {
        public event Action OnTileChanged;
        public event Action OnChunkCreated;

        public bool IsActive { get => gameObject.activeSelf; set
            {
                if (value == gameObject.activeSelf) return;

                gameObject.SetActive(value);
            }
        }

        public bool ColliderEnabled { get => _colliderEnabled; set { _colliderEnabled = value; UpdateCollider(); } }
        public Material LiquidMaterial { get => _liquidMaterial; set {
                bool changed = _liquidMaterial != value;
                _liquidMaterial = value;
                if (changed) UpdateLiquidState();
            } }
        public Material Material { get => _material; set { _material = value; UpdateRenderer(); } }
        public Color TintColor { get => _tintColor; set { _tintColor = value; UpdateRenderer(); } }
        public bool IsUndoEnabled { get => Tilemap.UndoEnabled; }
        public PhysicsMaterial2D PhysicsMaterial2D { get { return _physMaterial; } set { _physMaterial = value; UpdateColliderProperties(); } }
        public bool IsTrigger { get { return _isTrigger; } set { _isTrigger = value; UpdateColliderProperties(); } }
        public string Tag { get { return _tag; } set { _tag = value; UpdateChunksFlags(); } }
        public LayerMask LayerMask { get { return _layerMask; } set { _layerMask = value; UpdateChunksFlags(); } }
        public bool LiquidEnabled { get => _liquidEnabled; set
            {
                bool changed = _liquidEnabled != value;
                _liquidEnabled = value;
                if (changed)
                    UpdateLiquidState();
            } }
        public bool AutoTrim { get => Tilemap.AutoTrim; }

     
        public int MinGridX { get; private set; }
        public int MinGridY { get; private set; }
        public int MaxGridX { get; private set; }
        public int MaxGridY { get; private set; }

        public Bounds Bounds => _bounds;

        [SerializeField,HideInInspector]
        private Bounds _bounds;

        public ATilemap Tilemap;
        public ATileset Tileset;

        public Color MinLiquidColor = Color.white;
        public Color MaxLiquidColor = Color.white;
        public bool ShowChunkBounds = false;
        public bool UpdateVariationsOnRefresh = true;
        public bool TrimInvoke;
        public bool ChunkCacheDirty;
        public bool BoundsIsDirty;

        [SerializeField, HideInInspector] private Material _material;
        [SerializeField, HideInInspector] private bool _colliderEnabled;
        [HideInInspector, SerializeField] private Color _tintColor = Color.white;
        [HideInInspector, SerializeField] private bool _liquidEnabled;
        [HideInInspector, SerializeField] private Material _liquidMaterial;
        [HideInInspector, SerializeField] private LayerMask _layerMask;
        [HideInInspector, SerializeField] private string _tag = "Untagged";
        [HideInInspector, SerializeField] private bool _isTrigger;
        [HideInInspector, SerializeField] private PhysicsMaterial2D _physMaterial;

        private Dictionary<uint, AChunk> chunksCache = new Dictionary<uint, AChunk>();

        private float _liquidTimer = 0;

        public void CalculateBounds(bool immediate = false)
        {
            if (!immediate)
            {
                BoundsIsDirty = true;
                return;
            }


            MinGridX = 0; MinGridY = 0; MaxGridX = 0; MaxGridY = 0;
            _bounds = new Bounds();

            var unit = Tileset.GetTileUnit();

            foreach (var chunk in chunksCache)
            {
                Bounds chunkBounds = chunk.Value.GetBounds();
                Vector2 min = transform.InverseTransformPoint(chunk.Value.transform.TransformPoint(chunkBounds.min));
                Vector2 max = transform.InverseTransformPoint(chunk.Value.transform.TransformPoint(chunkBounds.max));
                _bounds.Encapsulate(min + Vector2.one * unit);
                _bounds.Encapsulate(max - Vector2.one * unit);
            }

            MinGridX = Utilites.GetGridX(this, _bounds.min);
            MinGridY = Utilites.GetGridY(this, _bounds.min);
            MaxGridX = Utilites.GetGridX(this, _bounds.max);
            MaxGridY = Utilites.GetGridY(this, _bounds.max);
        }

        #region Undo

        public TilemapCommandContainer tilemapCommands;

        private ATilemapCommand currentRecording;

        public void UpdateUndoStack()
        {
            if (IsUndoEnabled && tilemapCommands == null)
                tilemapCommands = new TilemapCommandContainer();
            if (!IsUndoEnabled && tilemapCommands != null)
            {
                ClearUndoStack();
                tilemapCommands = null;
            }
        }

        private void ClearUndoStack()
        {
            tilemapCommands.undoCommands.Clear();
            tilemapCommands.redoCommands.Clear();
            currentRecording = null;
        }

        public void BeginRecordingCommand()
        {
            if (!IsUndoEnabled)
                return;

            currentRecording = new ATilemapCommand();
        }

        public void EndRecordCommand()
        {
            if (!IsUndoEnabled) return;
            
            if(currentRecording == null)
            {
                Debug.LogError("Need start command to finish recording");
                return;
            }

            if (currentRecording.isEmpty()) 
                return;

            if(tilemapCommands == null) tilemapCommands = new TilemapCommandContainer();

            tilemapCommands.undoCommands.Push(currentRecording);
            tilemapCommands.redoCommands.Clear();

            currentRecording = null;
        }

        public void Undo()
        {
            if (!IsUndoEnabled) return;

            if (currentRecording != null) return;

            if (tilemapCommands.undoCommands.Count == 0) return;

            var command = tilemapCommands.undoCommands.Pop();

            if (command == null)
                return;

            for (int i = 0; i < command.tileChanges.Count; i++)
            {
                var tileChange = command.tileChanges[i];
                if (tileChange.OldTileData == 0)
                    SetTile(tileChange.Position.x, tileChange.Position.y,0);
                else
                    SetTile(tileChange.Position.x, tileChange.Position.y, tileChange.OldTileData);
            }

            tilemapCommands.redoCommands.Push(command);
        }

        public void Redo()
        {
            if (!IsUndoEnabled) return;

            if (currentRecording != null) return;

            if (tilemapCommands.redoCommands.Count == 0) return;

            var command = tilemapCommands.redoCommands.Pop();

            if (command == null)
                return;

            for (int i = 0; i < command.tileChanges.Count; i++)
            {
                var tileChange = command.tileChanges[i];

                if(tileChange.NewTileData == 0) 
                    SetTile(tileChange.Position.x, tileChange.Position.y,0);
                else
                    SetTile(tileChange.Position.x, tileChange.Position.y, tileChange.NewTileData);
            }

            tilemapCommands.undoCommands.Push(command);
        }

        #endregion

        #region BaseData

        public void SetTile(int x, int y, ushort tileID, UVTransform data = default)
        {
            var chunk = GetOrCreateChunk(x, y, tileID != ATile.EMPTY);

            if (chunk == null) return;

            var coords = ConvertGlobalGridToChunk(x, y);

            if (IsUndoEnabled && currentRecording != null)
            {
                var oldID = chunk.GetTile(coords.x, coords.y);
                var newID = tileID;

                if(oldID != newID)
                {
                    var match = currentRecording.tileChanges.Find(n => n != null && n.Position.x == x && n.Position.y == y);

                    if (match == null)
                    {
                        currentRecording.tileChanges.Add(new ATilemapCommand.TileData() {
                            Position = new Vector2Int(x,y),
                            OldTileData = oldID,
                            NewTileData = newID
                        });
                    }else
                    {
                        match.NewTileData = newID;
                    }

                    if (newID == 0)
                        TrimInvoke = true;
                }
            }

            if (chunk.SetTile(coords.x, coords.y, tileID, data)) {
                UpdateBitmask(x, y);
                UpdateNeighborsBitmask(x, y);

                if (AutoTrim && tileID == ATile.EMPTY)
                    TrimInvoke = true;
            }
        }

        public ushort GetTile(int x, int y)
        {
            var chunk = GetOrCreateChunk(x, y,false);

            if (chunk == null)
                return 0;

            var coords = ConvertGlobalGridToChunk(x, y);

            return chunk.GetTile(coords.x, coords.y);
        }

        public void SetColor(int x,int y ,Color32 color)
        {
            var chunk = GetOrCreateChunk(x, y, false);

            if (chunk == null) return;

            var coords = ConvertGlobalGridToChunk(x, y);

            chunk.SetColor(coords.x, coords.y, color);
        }

        public Color32 GetColor(int x, int y)
        {
            var chunk = GetOrCreateChunk(x, y,false);

            if (chunk == null)
                return Color.white;

            var coords = ConvertGlobalGridToChunk(x, y);

            return chunk.GetColor(coords.x, coords.y);
        }

        #endregion

        #region Bitmask

        public void SetBitmask(int x, int y, byte bitmask)
        {
            var chunk = GetOrCreateChunk(x, y,false);

            if (chunk == null)
                return;

            var coords = ConvertGlobalGridToChunk(x, y);

            if(chunk.GetBitmask(coords.x, coords.y) != bitmask)
                chunk.SetBitmask(coords.x, coords.y, bitmask);
        }

        public byte GetBitmask(int x, int y)
        {
            var chunk = GetOrCreateChunk(x, y,false);

            if (chunk == null) return 0;

            var coords = ConvertGlobalGridToChunk(x, y);

            return chunk.GetBitmask(coords.x, coords.y);
        }

        public void UpdateBitmask(int x, int y)
        {
            SetBitmask(x, y, CalculateBitmask(x, y));

            OnTileChanged?.Invoke();
        }

        public void UpdateNeighborsBitmask(int x, int y)
        {
            for (int ix = x - 1; ix < x + 2; ix++)
            {
                for (int iy = y - 1; iy < y + 2; iy++)
                {
                    if (ix == x && iy == y)
                        continue;

                    UpdateBitmask(ix, iy);
                }
            }
        }

        public byte CalculateBitmask(int x, int y)
        {
            var tileID = GetTile(x, y);

            if (tileID == 0)
                return 0;

            byte bitmask = 0;

            if (GetTile(x - 1, y + 1) == tileID)
                bitmask |= 1;

            if (GetTile(x, y + 1) == tileID)
                bitmask |= 2;

            if (GetTile(x + 1, y + 1) == tileID)
                bitmask |= 4;

            if (GetTile(x - 1, y) == tileID)
                bitmask |= 8;

            if (GetTile(x + 1, y) == tileID)
                bitmask |= 16;

            if (GetTile(x - 1, y - 1) == tileID)
                bitmask |= 32;

            if (GetTile(x, y - 1) == tileID)
                bitmask |= 64;

            if (GetTile(x + 1, y - 1) == tileID)
                bitmask |= 128;


            return bitmask;
        }

        #endregion

        #region Main

        public void Refresh(bool immediate = false)
        {
            Update();
            foreach (var chunk in chunksCache)
            {
                chunk.Value.Refresh(immediate);
            }

            CalculateBounds();
        }

        public AChunk GetOrCreateChunk(int x, int y, bool autoCreate = true)
        {
            int chunkX = (x < 0 ? (x + 1 - AChunk.CHUNK_SIZE) : x) / AChunk.CHUNK_SIZE;
            int chunkY = (y < 0 ? (y + 1 - AChunk.CHUNK_SIZE) : y) / AChunk.CHUNK_SIZE;
            uint key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));

            AChunk chunk;

            chunksCache.TryGetValue(key, out chunk);

            if (chunk == null && autoCreate)
            {
                var go = new GameObject();
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(chunkX * AChunk.CHUNK_SIZE, chunkY * AChunk.CHUNK_SIZE);

                chunk = go.AddComponent<AChunk>();
                chunk.Layer = this;
                chunk.GridX = chunkX * AChunk.CHUNK_SIZE;
                chunk.GridY = chunkY * AChunk.CHUNK_SIZE;
                chunk.Init();

                chunksCache[key] = chunk;

                OnChunkCreated?.Invoke();
            }

            return chunk;
        }

        public void Trim()
        {
            foreach (var chunk in chunksCache)
            {
                if (chunk.Value.CanTrim)
                {
                    DestroyImmediate(chunk.Value.gameObject);
                }
            }
            BuildChunkCache();
          
        }

        public bool TrimIfNeeded()
        {
            if (!AutoTrim) return false;

            Trim();

            return true;
        }

        private void BuildChunkCache()
        {
            chunksCache.Clear();
            for (int i = 0; i < transform.childCount; ++i)
            {
                AChunk chunk = transform.GetChild(i).GetComponent<AChunk>();
                if (chunk)
                {
                    int chunkX = (chunk.GridX < 0 ? (chunk.GridX + 1 - AChunk.CHUNK_SIZE) : chunk.GridX) / AChunk.CHUNK_SIZE;
                    int chunkY = (chunk.GridY < 0 ? (chunk.GridY + 1 - AChunk.CHUNK_SIZE) : chunk.GridY) / AChunk.CHUNK_SIZE;
                    uint key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));
                    chunksCache[key] = chunk;
                    chunk.UpdateRenderer();
                }
            }

            CalculateBounds();
        }

        public void Clear()
        {
            if (IsUndoEnabled)
            {
                CalculateBounds();
                for (int ix = MinGridX; ix < MaxGridX; ix++)
                {
                    for (int iy = MinGridY; iy < MaxGridY; iy++)
                    {
                        SetTile(ix, iy,0);
                    }
                }

                return;
            }

            foreach (var chunk in chunksCache)
            {
                DestroyImmediate(chunk.Value.gameObject);
            }
            chunksCache.Clear();

            CalculateBounds();
        }

        public Vector2Int ConvertGlobalGridToChunk(int x, int y)
        {
            int cx = (x < 0 ? -x - 1 : x) % AChunk.CHUNK_SIZE;
            int cy = (y < 0 ? -y - 1 : y) % AChunk.CHUNK_SIZE;
            if (x < 0) cx = AChunk.CHUNK_SIZE - 1 - cx;
            if (y < 0) cy = AChunk.CHUNK_SIZE - 1 - cy;

            return new Vector2Int(cx, cy);
        }

        public void UpdateCollider(bool immediate = false)
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.ColliderEnabledChange(_colliderEnabled);

                if (_colliderEnabled)
                    chunk.Value.MakeDirty();
            }
        }

        public void UpdateRenderer()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateRenderer();
            }
        }

        private void OnValidate()
        {
            BuildChunkCache();
        }

        private void Awake()
        {
            BuildChunkCache();
        }

        private void Update()
        {
            if (TrimInvoke)
            {
                TrimInvoke = false;
                Trim();
            }

            if (ChunkCacheDirty)
            {
                ChunkCacheDirty = false;
                BuildChunkCache();
            }

            if (BoundsIsDirty)
            {
                BoundsIsDirty = false;
                CalculateBounds(true);

            }

            if (Application.isPlaying)
            {
                if (_liquidEnabled)
                {
                    _liquidTimer += Time.deltaTime;
                    if (_liquidTimer > Tilemap.LiquidStepsDuration)
                    {
                        _liquidTimer = 0;
                        SimulateLiquid();
                    }
                }
            }

        }

        public void UpdateChunksFlags()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateFlags();
            }
        }
       
        public void UpdateColliderProperties()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateColliderProperties();
            }
        }

        public void UpdateCollider()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.ColliderEnabledChange(_colliderEnabled);
            }
        }

        #endregion

        #region Liquid

        private void SimulateLiquid()
        {
            CalculateBounds();

            var min = Utilites.GetGridPosition(this,Camera.main.BoundsMin() - new Vector2(AChunk.CHUNK_SIZE * 3, AChunk.CHUNK_SIZE * 3));
            var max = Utilites.GetGridPosition(this,Camera.main.BoundsMax() + new Vector2(AChunk.CHUNK_SIZE * 3, AChunk.CHUNK_SIZE * 3));

            for (int ix = min.x; ix < max.x; ix++)
            {
                for (int iy = min.y; iy < max.y; iy++)
                {
                    SimulateCell(ix,iy);
                }
            }

            for (int ix = min.x; ix < max.x; ix++)
            {
                for (int iy = min.y; iy < max.y; iy++)
                {
                    if (GetLiquid(ix, iy) < ALiquidChunk.MIN_VALUE)
                        SetSettled(ix, iy, false);
                }
            }
        }

        private bool IsEmptyLiq(int x, int y) =>
            GetTile(x, y) == 0 && ALiquidChunk.MIN_LIQUID_Y < y;

        private float CalculateVerticalFlowValue(float remainingLiquid, float destination)
        {
            float sum = remainingLiquid + destination;
            float value;

            if (sum <= ALiquidChunk.MAX_VALUE)
            {
                value = ALiquidChunk.MAX_VALUE;
            }
            else if (sum < 2 * ALiquidChunk.MAX_VALUE + ALiquidChunk.MAX_COMPRESSION)
            {
                value = (ALiquidChunk.MAX_VALUE * ALiquidChunk.MAX_VALUE + sum * ALiquidChunk.MAX_COMPRESSION) / (ALiquidChunk.MAX_VALUE + ALiquidChunk.MAX_COMPRESSION);
            }
            else
            {
                value = (sum + ALiquidChunk.MAX_COMPRESSION) / 2f;
            }

            return value;
        }

        private void SimulateCell(int x, int y)
        {
            if (!IsEmptyLiq(x, y))
            {
                if (GetLiquid(x, y) != 0)
                    SetLiquid(x, y, 0);
                return;
            }

            var liquidValue = GetLiquid(x, y);
            if (liquidValue == 0) return;
            if (GetSettled(x, y)) return;

            if(liquidValue < ALiquidChunk.MIN_VALUE)
            { SetLiquid(x, y, 0); return; }

            var startValue = liquidValue;
            var remainingValue = startValue;
            var flow = 0f;

            //bottom
            if (IsEmptyLiq(x, y - 1))
            {
                var bLiquid = GetLiquid(x, y - 1);
                flow = CalculateVerticalFlowValue(startValue, bLiquid) - bLiquid;
                if (bLiquid > 0 && flow > ALiquidChunk.MIN_FLOW)
                    flow *= ALiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidChunk.MAX_FLOW, startValue))
                    flow = Mathf.Min(ALiquidChunk.MAX_FLOW, startValue);

                if (flow != 0)
                {
                    remainingValue -= flow;

                    SetSettled(x, y - 1, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x, y - 1, flow);

                }
            }
            if (remainingValue < ALiquidChunk.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            //left
            if (IsEmptyLiq(x - 1, y))
            {
                flow = (remainingValue - GetLiquid(x - 1, y)) / 4f;
                if (flow > ALiquidChunk.MIN_FLOW)
                    flow *= ALiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidChunk.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(ALiquidChunk.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;
                    SetSettled(x - 1, y, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x - 1, y, flow);

                }
            }
            if (remainingValue < ALiquidChunk.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            //right
            if (IsEmptyLiq(x + 1, y))
            {
                flow = (remainingValue - GetLiquid(x + 1, y)) / 3f;
                if (flow > ALiquidChunk.MIN_FLOW)
                    flow *= ALiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidChunk.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(ALiquidChunk.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;
                    SetSettled(x + 1, y, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x + 1, y, flow);

                }
            }
            if (remainingValue < ALiquidChunk.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            //top
            if (IsEmptyLiq(x, y + 1))
            {
                flow = remainingValue - CalculateVerticalFlowValue(remainingValue, GetLiquid(x, y + 1));
                if (flow > ALiquidChunk.MIN_FLOW)
                    flow *= ALiquidChunk.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidChunk.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(ALiquidChunk.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;

                    SetSettled(x, y + 1, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x, y + 1, flow);

                }
            }
            if (remainingValue < ALiquidChunk.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            if (startValue - remainingValue < ALiquidChunk.STABLE_FLOW)
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
        }

        private void UpdateLiquidState()
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateLiquidState();
            }
        }

        private bool GetSettled(int x, int y)
        {
            var chunk = GetOrCreateChunk(x, y, false);
            if (chunk == null)
                return false ;
            var coords = ConvertGlobalGridToChunk(x, y);
            return chunk.GetSettled(coords.x, coords.y);
        }

        private void SetSettled(int x, int y, bool value)
        {
            var chunk = GetOrCreateChunk(x, y, false);
            if (chunk == null)
                return;
            var coords = ConvertGlobalGridToChunk(x, y);

            
            chunk.SetSettled(coords.x, coords.y, value);
        }

        public void SetLiquid(int x, int y, float value)
        {
            if (!LiquidEnabled)
            {
                Debug.LogError("Liquid for layer[" + name + "] disabled");
                return;
            }

            var chunk = GetOrCreateChunk(x, y, false);

            var coords = ConvertGlobalGridToChunk(x, y);

            if (chunk == null)
                return;

            chunk.SetLiquid(coords.x, coords.y, value);
        }

        public void AddLiquid(int x, int y, float value)
        {
            if (!LiquidEnabled)
            {
                Debug.LogError("Liquid for layer[" + name + "] disabled");
                return;
            }

            var chunk = GetOrCreateChunk(x, y, false);

            if (chunk == null)
                return;

            var coords = ConvertGlobalGridToChunk(x, y);

            chunk.AddLiquid(coords.x, coords.y, value);
        }
        public float GetLiquid(int x, int y)
        {
            if (!LiquidEnabled)
            {
                Debug.LogError("Liquid for layer[" + name + "] disabled");
                return 0;
            }

            var chunk = GetOrCreateChunk(x, y, false);

            if (chunk == null)
                return 0;

            var coords = ConvertGlobalGridToChunk(x, y);

            return chunk.GetLiquid(coords.x, coords.y);
        }

        #endregion

        #region Lighting

        public void UpdateLightingState(bool active)
        {
            foreach (var chunk in chunksCache)
            {
                chunk.Value.UpdateLightingState(active);
            }
        }

        public void SetLight(int x, int y, byte value)
        {
            var chunk = GetOrCreateChunk(x, y, false);

            var coords = ConvertGlobalGridToChunk(x, y);

            if (chunk == null)
                return;

            chunk.SetLight(coords.x, coords.y, value);
        }

        public byte GetLight(int x, int y)
        {
            var chunk = GetOrCreateChunk(x, y, false);

            if (chunk == null)
                return 0;

            var coords = ConvertGlobalGridToChunk(x, y);

            return chunk.GetLight(coords.x, coords.y);
        }

        #endregion

        #region Gizmos

        /*
        private void OnDrawGizmos()
        {
            DrawGizmos();
        }

        public void DrawGizmos()
        {
           // Debug.Log($"{Tilemap != null}&& {Tilemap.ShowGrid}");

            if (Tilemap != null && Tilemap.ShowGrid)
            {
                var camTransform = Camera.current.transform;
                var layerTransform = transform;

                var tilemapPlane = new Plane(layerTransform.forward, layerTransform.position);

                var rayCamToPlane = new Ray(camTransform.position, camTransform.forward);
                tilemapPlane.Raycast(rayCamToPlane, out var distCamToTilemap);

                Debug.Log(HandleUtility.GetHandleSize(rayCamToPlane.GetPoint(distCamToTilemap)) <= 3f);

                if (HandleUtility.GetHandleSize(rayCamToPlane.GetPoint(distCamToTilemap)) <= 3f)
                {
                    Gizmos.color = GlobalSettings.TilemapGridColor;

                    for (float i = 1; i < AChunk.CHUNK_SIZE; i++)
                    {
                        Gizmos.DrawLine(
                            this.transform.TransformPoint(new Vector3(Bounds.min.x + i * 1f, Bounds.min.y)),
                            this.transform.TransformPoint(new Vector3(Bounds.min.x + i * 1f, Bounds.max.y))
                        );
                    }

                    // Vertical lines
                    for (float i = 1; i < AChunk.CHUNK_SIZE; i++)
                    {
                        Gizmos.DrawLine(
                            this.transform.TransformPoint(new Vector3(Bounds.min.x, Bounds.min.y + i * 1f, 0)),
                            this.transform.TransformPoint(new Vector3(Bounds.max.x, Bounds.min.y + i * 1f, 0))
                        );
                    }

                    Gizmos.color = Color.white;
                }


            }
        }
*/

        #endregion
    }
}
