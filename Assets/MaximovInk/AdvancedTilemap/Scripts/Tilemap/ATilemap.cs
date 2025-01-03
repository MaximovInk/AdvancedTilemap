
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public struct AChunkLoaderSettings
    {
        public bool Enabled;
        public Transform Target;
        public Vector2Int TargetOffset;
    }

    [System.Serializable]
    public struct ALayerLoadChunksData
    {
        public ALayer Layer;
        public List<AChunk> Chunks;
    }

    [System.Serializable]
    public struct ALightingSettings
    {
        public bool Enabled;
        public Material LightMaterial;
        public LayerMask LightingMask;
    }

    [ExecuteAlways]
    public class ATilemap : MonoBehaviour
    {
        public const float Z_TILE_OFFSET = 0.1F;

        public int SortingOrder { get => sortingOrder;
            set { var changed = sortingOrder != value; sortingOrder = value; if(changed) UpdateRenderer(); } }
        public bool UndoEnabled { get => undoEnabled;
            set { var changed = undoEnabled != value; undoEnabled = value; if(changed) UpdateUndoStack(); } }
        public bool AutoTrim
        {
            get => _autoTrim; set
            {
               var changed = AutoTrim != value;
                _autoTrim = value;
                if (changed) Trim();
            }
        }

        public bool DisplayChunksInHierarchy
        {
            get => displayChunksHierarchy;
            set
            {
                var changed = displayChunksHierarchy != value;
                displayChunksHierarchy = value;
                if (changed)
                    UpdateChunksFlags();
            }
        }

        public float LiquidStepsDuration
        {
            get => _liquidStepsDuration;
            set => _liquidStepsDuration = value;
        }

        [SerializeField]
        private float _liquidStepsDuration = 0.1f;

        [SerializeField]
        private bool displayChunksHierarchy = true;
        [SerializeField]
        private int sortingOrder;
        [SerializeField]
        private bool undoEnabled;
        [SerializeField]
        private bool _autoTrim;

        public AChunkLoaderSettings ChunkLoader
        {
            get => _chunkLoaderSettings;
            set => _chunkLoaderSettings = value;
        }
        [HideInInspector, SerializeField]
        private AChunkLoaderSettings _chunkLoaderSettings;

        public List<ALayer> Layers = new();
        private readonly List<ALayerLoadChunksData> _loadedChunks = new();

        private Vector2Int _lastGridPos;

        public ALightingSettings Lighting
        {
            get => _lighting;
            set => _lighting = value;
        }
        [HideInInspector, SerializeField]
        private ALightingSettings _lighting;

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;

            Profiler.BeginSample("Update loader");
            UpdateLoader();
            Profiler.EndSample();


            Profiler.BeginSample("Update light");
            if (_invokeUpdateLight)
                UpdateLighting(true);
            Profiler.EndSample();
        }

        public void Refresh(bool immediate = false)
        {
            foreach (var layer in Layers)
            {
                if (layer == null) continue;

                layer.Refresh(immediate);
            }
        }

        private void Trim()
        {
            foreach (var layer in Layers)
            {
                if (layer == null) continue;

                layer.Trim();
            }

        }

        private void UpdateUndoStack()
        {
            foreach (var layer in Layers)
            {
                if (layer == null) continue;

                layer.UpdateUndoStack();
            }
        }

        private void UpdateRenderer()
        {
            foreach(var layer in Layers)
            {
                if (layer == null) continue;

                layer.UpdateRenderer();
            }
        }

        private void UpdateChunksFlags()
        {
            foreach (var layer in Layers)
            {
                if (layer == null) continue;

                layer.UpdateChunksFlags();
            }
        }

        public ALayer MakeLayer()
        {
            var go = new GameObject();
            var layer = go.AddComponent<ALayer>();

            go.name = $"layer{Layers.Count}";
            var layerT = layer.transform;
            layerT.SetParent(transform);
            layerT.localPosition = Vector3.zero;
            layer.Tilemap = this;
            Layers.Add(layer);

            return layer;
        }

        public void RemoveLayer(int index)
        {
            DestroyImmediate(Layers[index].gameObject);
            Layers.RemoveAt(index);
        }

        public void TrimAll(bool immediate = false)
        {
            foreach (var layer in Layers)
            {
                if (immediate)
                    layer.Trim();
                else
                    layer.TrimInvoke = true;
            }
        }

        public void Clear()
        {
            foreach (var layer in Layers)
            {
                layer.Clear();
            }
        }

        private void UpdateLoader(bool ignorePosDistance = false)
        {
            var loaderData = _chunkLoaderSettings;

            if (!loaderData.Enabled) return;
            if (Layers.Count == 0) return;
            if (loaderData.Target == null) return;

            var layer = Layers[0];

            var gridPosCenter = Utilites.ConvertGlobalCoordsToGrid(layer, loaderData.Target.position);

            if (Vector2Int.Distance(_lastGridPos, gridPosCenter) < AChunk.CHUNK_SIZE/2f && !ignorePosDistance)
            {
                return;
            }

            for (var i = 0; i < _loadedChunks.Count; i++)
            {
                foreach (var chunk in _loadedChunks[i].Chunks)
                {
                    chunk.IsLoaderActive = false;
                }

                _loadedChunks[i].Chunks.Clear();
                _loadedChunks[i].Layer.TrimIfNeeded();
            }
            _loadedChunks.Clear();

            foreach (var t in Layers)
            {
                _loadedChunks.Add(new ALayerLoadChunksData()
                {
                    Layer = t,
                    Chunks = new List<AChunk>()
                });
            }

            for (int i = 0; i < _loadedChunks.Count; i++)
            {
                layer = _loadedChunks[i].Layer;

                var loadingChunk = layer.GetOrCreateChunk(gridPosCenter.x, gridPosCenter.y);
                loadingChunk.IsLoaderActive = true;
                _loadedChunks[i].Chunks.Add(loadingChunk);

                for (var ix = -loaderData.TargetOffset.x; ix <= loaderData.TargetOffset.x; ix++)
                {
                    for (var iy = -loaderData.TargetOffset.y; iy <= loaderData.TargetOffset.y; iy++)
                    {
                        if (ix == 0 && iy == 0) continue;

                        var temp = layer.GetOrCreateChunk(loadingChunk.GridX + AChunk.CHUNK_SIZE * ix,
                            loadingChunk.GridY + AChunk.CHUNK_SIZE * iy);

                        temp.IsLoaderActive = true;

                        _loadedChunks[i].Chunks.Add(temp);

                    }
                }
            }

            _lastGridPos = gridPosCenter;

            UpdateLighting();

        }

        private void Awake()
        {
            UpdateLighting();

            UpdateLoader(true);
        }

        public List<ALayer> GetLayersWithTileName(string nameID)
        {
            var found = new List<ALayer>();

            foreach (var layer in Layers)
            {
                if(layer.Tileset == null)continue;

                if(layer.Tileset.HasTile(nameID))
                    found.Add(layer);
            }

            return found;
        }

        #region LIGHTING

        [SerializeField]
        private bool _invokeUpdateLight;

        private readonly List<ALayer> _foreground = new();
        private readonly List<ALayer> _background = new();

        private int GetTile(List<ALayer> layers, int x, int y)
        {
            foreach (var t in layers)
            {
                var tile = t.GetTile(x, y);
                if (t.GetTile(x, y) != 0) return tile;
            }

            return 0;
        }

        private void SetTile(List<ALayer> layers, int x, int y, ushort tile)
        {
            foreach (var t in layers)
            {
                t.SetTile(x, y, tile);
            }
        }

        private void SetLight(List<ALayer> layers, int x, int y, byte value)
        {
            foreach (var t in layers)
            {
                t.SetLight(x, y, value);
            }
        }

        private void EmitLight(List<ALayer> layers, int x, int y)
        {
            foreach (var t in layers)
            {
                t.EmitLight(x, y);
            }
        }

        public void UpdateLighting(bool immediate = false)
        {
            _invokeUpdateLight = true;

            if (!immediate) return;

            Profiler.BeginSample("prepare");

            _invokeUpdateLight = false;

            var l = _lighting;

            _foreground.Clear();
            _background.Clear();

            var bounds = new Bounds();

            foreach (var layer in Layers)
            {
                if (layer.LightType == LightLayerType.NoLight)
                {
                    layer.UpdateLightingState(false);
                    continue;
                }

                switch (layer.LightType)
                {
                    case LightLayerType.Foreground:
                        _foreground.Add(layer);
                        break;
                    case LightLayerType.Background:
                        _background.Add(layer);
                        break;
                }

                bounds.Encapsulate(layer.Bounds);

                layer.UpdateLightingState(l.Enabled);
            }

            Profiler.EndSample();

            if (!l.Enabled) return;

            if (_foreground.Count == 0) return;

            var mainLayer = _foreground[0];

            var min = Utilites.GetGridPosition(mainLayer, bounds.min);
            var max = Utilites.GetGridPosition(mainLayer, bounds.max);

            if (!Application.isPlaying) return;

            Profiler.BeginSample("clear");

            List<Vector2Int> emits = new();

            for (int ix = min.x; ix < max.x; ix++)
            {
                for (int iy = max.y; iy >= min.y; iy--)
                {
                    var frontTile = GetTile(_foreground, ix, iy);

                    var hasTile = frontTile > 0 ;

                    SetLight(_foreground, ix, iy, (byte)(hasTile ? 0 : 255));

                    if (!hasTile)
                    {
                        emits.Add(new Vector2Int(ix, iy));
                    }
                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("emit");

            foreach (var em in emits)
            {
                EmitLight(_foreground, em.x, em.y);
            }
            Profiler.EndSample();
        }

        #endregion

    }
}