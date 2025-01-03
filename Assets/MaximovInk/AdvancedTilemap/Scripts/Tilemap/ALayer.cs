﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static MaximovInk.Bitmask;

namespace MaximovInk.AdvancedTilemap
{
    public enum LightLayerType
    {
        NoLight,
        Background,
        Foreground,
    }

    public delegate void TileChanged(Vector2Int pos, int oldID, int newID);

    public delegate void ChunkCreated(AChunk chunk);

    [ExecuteAlways]
    public class ALayer : MonoBehaviour, ITilemap
    {
        public event TileChanged OnTileChanged;
        public event ChunkCreated OnChunkCreated;

        public bool IsActive { get => gameObject.activeSelf; set
            {
                if (value == gameObject.activeSelf) return;

                gameObject.SetActive(value);
            }
        }

        public bool ColliderEnabled { get => _colliderEnabled; set { _colliderEnabled = value; UpdateCollider(); } }
        public Material LiquidMaterial { get => _liquidMaterial; set {
                var changed = _liquidMaterial != value;
                _liquidMaterial = value;
                if (changed) UpdateLiquidState();
            } }
        public Material Material { get => _material; set { _material = value; UpdateRenderer(); } }
        public Color TintColor { get => _tintColor; set { _tintColor = value; UpdateRenderer(); } }
        public bool IsUndoEnabled => Tilemap.UndoEnabled;
        public PhysicsMaterial2D PhysicsMaterial2D { get => _physMaterial;
            set { _physMaterial = value; UpdateColliderProperties(); } }
        public bool IsTrigger { get => _isTrigger;
            set { _isTrigger = value; UpdateColliderProperties(); } }
        public string Tag { get => _tag;
            set { _tag = value; UpdateChunksFlags(); } }
        public LayerMask LayerMask { get => _layerMask;
            set { _layerMask = value; UpdateChunksFlags(); } }
        public bool LiquidEnabled { get => _liquidEnabled; set
            {
                var changed = _liquidEnabled != value;
                _liquidEnabled = value;
                if (changed)
                    UpdateLiquidState();
            } }
        public bool AutoTrim => Tilemap.AutoTrim;

        public int MinGridX { get; private set; }
        public int MinGridY { get; private set; }
        public int MaxGridX { get; private set; }
        public int MaxGridY { get; private set; }

        public Bounds Bounds => _bounds;

        [SerializeField,HideInInspector]
        private Bounds _bounds;

        public ATilemap Tilemap;
        public ATileset Tileset;

        public LightLayerType LightType;
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

        private readonly Dictionary<uint, AChunk> chunksCache = new();

        private float _liquidTimer = 0;

        public void CalculateBounds(bool immediate = false)
        {
            if (!immediate)
            {
                BoundsIsDirty = true;
                return;
            }

            if (Tileset == null) return;


            MinGridX = 0; MinGridY = 0; MaxGridX = 0; MaxGridY = 0;
            _bounds = new Bounds();

            var unit = Tileset.GetTileUnit();

            foreach (var chunk in chunksCache)
            {
                var chunkBounds = chunk.Value.GetBounds();
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

        public TilemapCommandContainer TilemapCommands;

        private ATilemapCommand _currentRecording;

        public void UpdateUndoStack()
        {
            if (IsUndoEnabled && TilemapCommands == null)
                TilemapCommands = new TilemapCommandContainer();
            if (!IsUndoEnabled && TilemapCommands != null)
            {
                ClearUndoStack();
                TilemapCommands = null;
            }
        }

        private void ClearUndoStack()
        {
            TilemapCommands.undoCommands.Clear();
            TilemapCommands.redoCommands.Clear();
            _currentRecording = null;
        }

        public void BeginRecordingCommand()
        {
            if (!IsUndoEnabled)
                return;

            _currentRecording = new ATilemapCommand();
        }

        public void EndRecordCommand()
        {
            if (!IsUndoEnabled) return;
            
            if(_currentRecording == null)
            {
                Debug.LogError("Need start command to finish recording");
                return;
            }

            if (_currentRecording.isEmpty()) 
                return;

            if(TilemapCommands == null) TilemapCommands = new TilemapCommandContainer();

            TilemapCommands.undoCommands.Push(_currentRecording);
            TilemapCommands.redoCommands.Clear();

            _currentRecording = null;
        }

        public void Undo()
        {
            if (!IsUndoEnabled) return;

            if (_currentRecording != null) return;

            if (TilemapCommands.undoCommands.Count == 0) return;

            var command = TilemapCommands.undoCommands.Pop();

            if (command == null)
                return;

            for (var i = 0; i < command.tileChanges.Count; i++)
            {
                var tileChange = command.tileChanges[i];
                if (tileChange.OldTileData == 0)
                    SetTile(tileChange.Position.x, tileChange.Position.y,0);
                else
                    SetTile(tileChange.Position.x, tileChange.Position.y, tileChange.OldTileData);
            }

            TilemapCommands.redoCommands.Push(command);
        }

        public void Redo()
        {
            if (!IsUndoEnabled) return;

            if (_currentRecording != null) return;

            if (TilemapCommands.redoCommands.Count == 0) return;

            var command = TilemapCommands.redoCommands.Pop();

            if (command == null)
                return;

            for (var i = 0; i < command.tileChanges.Count; i++)
            {
                var tileChange = command.tileChanges[i];

                if(tileChange.NewTileData == 0) 
                    SetTile(tileChange.Position.x, tileChange.Position.y,0);
                else
                    SetTile(tileChange.Position.x, tileChange.Position.y, tileChange.NewTileData);
            }

            TilemapCommands.undoCommands.Push(command);
        }

        #endregion

        #region BaseData

        public void SetTile(int x, int y, string tileName, UVTransform data = default)
        {
            var tileID = Tileset.GetTile(tileName);

            SetTile(x,y,tileID.ID, data);
        }

        public void SetTile(int x, int y, ushort tileID, UVTransform data = default)
        {
            var chunk = GetOrCreateChunk(x, y, tileID != ATile.EMPTY);

            if (chunk == null) return;

            var coords = ConvertGlobalGridToChunk(x, y);

            if (IsUndoEnabled && _currentRecording != null)
            {
                var oldID = chunk.GetTile(coords.x, coords.y);
                var newID = tileID;

                if(oldID != newID)
                {
                    var match = _currentRecording.tileChanges.Find(n => n != null && n.Position.x == x && n.Position.y == y);

                    if (match == null)
                    {
                        _currentRecording.tileChanges.Add(new ATilemapCommand.TileData() {
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

            var setTileReturn = chunk.SetTile(coords.x, coords.y, tileID, data);

            if (setTileReturn.IsChanged) {
                UpdateAllBitmask(x, y);
                UpdateNeighborsBitmask(x, y);

                if (AutoTrim && tileID == ATile.EMPTY)
                    TrimInvoke = true;

                OnTileChanged?.Invoke(new Vector2Int(x,y) , setTileReturn.OldID, tileID);
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

        public void SetSelfBitmask(int x, int y, byte bitmask)
        {
            var chunk = GetOrCreateChunk(x, y, false);

            if (chunk == null)
                return;

            var coords = ConvertGlobalGridToChunk(x, y);

            if (chunk.GetSelfBitmask(coords.x, coords.y) != bitmask)
                chunk.SetSelfBitmask(coords.x, coords.y, bitmask);

        }

        public byte GetSelfBitmask(int x, int y)
        {
            var chunk = GetOrCreateChunk(x, y, false);

            if (chunk == null) return 0;

            var coords = ConvertGlobalGridToChunk(x, y);

            return chunk.GetSelfBitmask(coords.x, coords.y);
        }

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

        public void UpdateAllBitmask(int x, int y)
        {
            SetBitmask(x, y, CalculateBitmask(x, y));
            SetSelfBitmask(x,y, CalculateSelfBitmask(x,y));
        }

        public void UpdateNeighborsBitmask(int x, int y)
        {
            for (var ix = x - 1; ix < x + 2; ix++)
            {
                for (var iy = y - 1; iy < y + 2; iy++)
                {
                    if (ix == x && iy == y)
                        continue;

                    UpdateAllBitmask(ix, iy);
                }
            }
        }

        public byte CalculateBitmask(int x, int y)
        {
            var tileID = GetTile(x, y);

            if (tileID == 0)
                return 0;

            byte bitmask = 0;

            if (GetTile(x - 1, y + 1) != 0)
                bitmask |= LEFT_TOP;

            if (GetTile(x, y + 1) != tileID)
                bitmask |= TOP;

            if (GetTile(x + 1, y + 1) != 0)
                bitmask |= RIGHT_TOP;

            if (GetTile(x - 1, y) != 0)
                bitmask |= LEFT;

            if (GetTile(x + 1, y) != 0)
                bitmask |= RIGHT;

            if (GetTile(x - 1, y - 1) != 0)
                bitmask |= LEFT_BOTTOM;

            if (GetTile(x, y - 1) != 0)
                bitmask |= BOTTOM;

            if (GetTile(x + 1, y - 1) != 0)
                bitmask |= RIGHT_BOTTOM;


            return bitmask;
        }

        public byte CalculateSelfBitmask(int x, int y)
        {
            var tileID = GetTile(x, y);

            if (tileID == 0)
                return 0;

            byte bitmask = 0;

            if (GetTile(x - 1, y + 1) == tileID)
                bitmask |= Bitmask.LEFT_TOP;

            if (GetTile(x, y + 1) == tileID)
                bitmask |= TOP;

            if (GetTile(x + 1, y + 1) == tileID)
                bitmask |= RIGHT_TOP;

            if (GetTile(x - 1, y) == tileID)
                bitmask |= LEFT;

            if (GetTile(x + 1, y) == tileID)
                bitmask |= RIGHT;

            if (GetTile(x - 1, y - 1) == tileID)
                bitmask |= LEFT_BOTTOM;

            if (GetTile(x, y - 1) == tileID)
                bitmask |= BOTTOM;

            if (GetTile(x + 1, y - 1) == tileID)
                bitmask |= RIGHT_BOTTOM;


            return bitmask;
        }

        #endregion

        #region Main

        private readonly Stack<AChunk> _freeChunks = new();

        public void FreeChunk(AChunk chunk)
        {
            chunk.gameObject.SetActive(false);
            chunk.IsLoaderActive = false;

            _freeChunks.Push(chunk);

        }

        public AChunk UseChunk()
        {
            return _freeChunks.Pop();
        }

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
            var chunkX = (x < 0 ? (x + 1 - AChunk.CHUNK_SIZE) : x) / AChunk.CHUNK_SIZE;
            var chunkY = (y < 0 ? (y + 1 - AChunk.CHUNK_SIZE) : y) / AChunk.CHUNK_SIZE;
            var key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));

            chunksCache.TryGetValue(key, out var chunk);

            if (chunk != null || !autoCreate) return chunk;

            GameObject go;

            if (_freeChunks.Count > 0)
            {
               chunk = UseChunk();
               go = chunk.gameObject;
               go.SetActive(true);
            }
            else
            { 
                go = new GameObject();
                chunk = go.AddComponent<AChunk>();
            }

            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(chunkX * AChunk.CHUNK_SIZE, chunkY * AChunk.CHUNK_SIZE);

            chunk.Layer = this;
            chunk.GridX = chunkX * AChunk.CHUNK_SIZE;
            chunk.GridY = chunkY * AChunk.CHUNK_SIZE;
            go.name = $"Chunk [{chunk.GridX},{chunk.GridY}]";
            chunk.Init();

            chunksCache[key] = chunk;

            OnChunkCreated?.Invoke(chunk);

            return chunk;
        }

        public void Trim()
        {
            foreach (var chunk in chunksCache)
            {
                if(chunk.Value == null) continue;

                if (chunk.Value.CanTrim)
                {
                    if (Application.isPlaying)
                    {
                        FreeChunk(chunk.Value);
                    }
                    else
                    {
                        DestroyImmediate(chunk.Value.gameObject);
                    }
                   
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
            for (var i = 0; i < transform.childCount; ++i)
            {
                var chunk = transform.GetChild(i).GetComponent<AChunk>();
                if (chunk)
                {
                    if(!Application.isPlaying)
                        chunk.IsLoaderActive = false;

                    if (chunk.gameObject.activeSelf)
                    {
                        var chunkX = (chunk.GridX < 0 ? (chunk.GridX + 1 - AChunk.CHUNK_SIZE) : chunk.GridX) / AChunk.CHUNK_SIZE;
                        var chunkY = (chunk.GridY < 0 ? (chunk.GridY + 1 - AChunk.CHUNK_SIZE) : chunk.GridY) / AChunk.CHUNK_SIZE;
                        var key = (uint)((chunkY << 16) | (chunkX & 0x0000FFFF));
                        chunksCache[key] = chunk;
                        chunk.UpdateRenderer();
                    }
                   
                }
            }

            CalculateBounds();
        }

        public void Clear()
        {
            if (IsUndoEnabled)
            {
                CalculateBounds();
                for (var ix = MinGridX; ix < MaxGridX; ix++)
                {
                    for (var iy = MinGridY; iy < MaxGridY; iy++)
                    {
                        SetTile(ix, iy,0);
                    }
                }

                return;
            }

            foreach (var chunk in chunksCache)
            {
                if (Application.isPlaying)
                {
                    FreeChunk(chunk.Value);
                }
                else
                {
                    DestroyImmediate(chunk.Value.gameObject);
                }
            }
            chunksCache.Clear();

            CalculateBounds();
        }

        public Vector2Int ConvertGlobalGridToChunk(int x, int y)
        {
            var cx = (x < 0 ? -x - 1 : x) % AChunk.CHUNK_SIZE;
            var cy = (y < 0 ? -y - 1 : y) % AChunk.CHUNK_SIZE;
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

            for (var ix = min.x; ix < max.x; ix++)
            {
                for (var iy = min.y; iy < max.y; iy++)
                {
                    SimulateCell(ix,iy);
                }
            }

            for (var ix = min.x; ix < max.x; ix++)
            {
                for (var iy = min.y; iy < max.y; iy++)
                {
                    if (GetLiquid(ix, iy) < ALiquidRenderer.MIN_VALUE)
                        SetSettled(ix, iy, false);
                }
            }
        }

        private bool IsEmptyLiq(int x, int y) =>
            GetTile(x, y) == 0 && ALiquidRenderer.MIN_LIQUID_Y < y;

        private float CalculateVerticalFlowValue(float remainingLiquid, float destination)
        {
            var sum = remainingLiquid + destination;
            float value;

            if (sum <= ALiquidRenderer.MAX_VALUE)
            {
                value = ALiquidRenderer.MAX_VALUE;
            }
            else if (sum < 2 * ALiquidRenderer.MAX_VALUE + ALiquidRenderer.MAX_COMPRESSION)
            {
                value = (ALiquidRenderer.MAX_VALUE * ALiquidRenderer.MAX_VALUE + sum * ALiquidRenderer.MAX_COMPRESSION) / (ALiquidRenderer.MAX_VALUE + ALiquidRenderer.MAX_COMPRESSION);
            }
            else
            {
                value = (sum + ALiquidRenderer.MAX_COMPRESSION) / 2f;
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

            if(liquidValue < ALiquidRenderer.MIN_VALUE)
            { SetLiquid(x, y, 0); return; }

            var startValue = liquidValue;
            var remainingValue = startValue;
            var flow = 0f;

            //bottom
            if (IsEmptyLiq(x, y - 1))
            {
                var bLiquid = GetLiquid(x, y - 1);
                flow = CalculateVerticalFlowValue(startValue, bLiquid) - bLiquid;
                if (bLiquid > 0 && flow > ALiquidRenderer.MIN_FLOW)
                    flow *= ALiquidRenderer.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidRenderer.MAX_FLOW, startValue))
                    flow = Mathf.Min(ALiquidRenderer.MAX_FLOW, startValue);

                if (flow != 0)
                {
                    remainingValue -= flow;

                    SetSettled(x, y - 1, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x, y - 1, flow);

                }
            }
            if (remainingValue < ALiquidRenderer.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            //left
            if (IsEmptyLiq(x - 1, y))
            {
                flow = (remainingValue - GetLiquid(x - 1, y)) / 4f;
                if (flow > ALiquidRenderer.MIN_FLOW)
                    flow *= ALiquidRenderer.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidRenderer.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(ALiquidRenderer.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;
                    SetSettled(x - 1, y, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x - 1, y, flow);

                }
            }
            if (remainingValue < ALiquidRenderer.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            //right
            if (IsEmptyLiq(x + 1, y))
            {
                flow = (remainingValue - GetLiquid(x + 1, y)) / 3f;
                if (flow > ALiquidRenderer.MIN_FLOW)
                    flow *= ALiquidRenderer.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidRenderer.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(ALiquidRenderer.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;
                    SetSettled(x + 1, y, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x + 1, y, flow);

                }
            }
            if (remainingValue < ALiquidRenderer.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            //top
            if (IsEmptyLiq(x, y + 1))
            {
                flow = remainingValue - CalculateVerticalFlowValue(remainingValue, GetLiquid(x, y + 1));
                if (flow > ALiquidRenderer.MIN_FLOW)
                    flow *= ALiquidRenderer.FLOW_SPEED;

                flow = Mathf.Max(flow, 0);
                if (flow > Mathf.Min(ALiquidRenderer.MAX_FLOW, remainingValue))
                    flow = Mathf.Min(ALiquidRenderer.MAX_FLOW, remainingValue);

                if (flow != 0)
                {
                    remainingValue -= flow;

                    SetSettled(x, y + 1, false);
                    AddLiquid(x, y, -flow);
                    AddLiquid(x, y + 1, flow);

                }
            }
            if (remainingValue < ALiquidRenderer.MIN_VALUE)
            {
                AddLiquid(x, y, -remainingValue);
                return;
            }

            if (startValue - remainingValue < ALiquidRenderer.STABLE_FLOW)
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

        public bool TryGetLight(int x, int y, out byte value)
        {
            var chunk = GetOrCreateChunk(x, y, false);

            if (chunk == null)
            {
                value = 0;
                return false;
            }

            var coords = ConvertGlobalGridToChunk(x, y);

            value = chunk.GetLight(coords.x, coords.y);
            return true;
        }

        private void TryEmitLightTo(int x, int y, byte set)
        {
            if (!TryGetLight(x, y, out var value)) return;

            if (value >= set) return;

            SetLight(x, y, set);

            EmitLight(x, y);
        }

        private const int LIGHT_STEP = byte.MaxValue/LIGHT_STEPS_COUNT;
        private const int LIGHT_STEPS_COUNT = 3;

        public void EmitLight(int x, int y)
        {
            if (!TryGetLight(x, y, out var current)) return;

            var newCurrent = (byte)Mathf.Clamp(current - LIGHT_STEP, 0,255);

            TryEmitLightTo(x-1,y, newCurrent);
            TryEmitLightTo(x+1,y, newCurrent);
            TryEmitLightTo(x,y+1, newCurrent);
            TryEmitLightTo(x,y-1, newCurrent);
        }

        #endregion

    }
}
