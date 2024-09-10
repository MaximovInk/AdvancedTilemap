using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public struct ALightingSettings
    {
        public bool Enabled; 
        public Material LightMaterial;
        public ALayer ForegroundLayer;
        public ALayer BackgroundLayer;

        public LayerMask LightingMask;
    }

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

    [ExecuteAlways]
    public class ATilemap : MonoBehaviour
    {
        public const int LIQUID_DEAD_Y = -100;
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

        public bool ShowGrid
        {
            get => _showGrid;
            set => _showGrid=value;
        }

        public bool DisplayChunksInHierarchy
        {
            get { return displayChunksHierarchy; }
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

        public Material LightMaterial => _lighting.LightMaterial;

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

        [SerializeField] private bool _showGrid;

        public bool LightingEnabled
        {
            get => _lighting.Enabled;
            set
            {
                bool changed = _lighting.Enabled != value;
                _lighting.Enabled = value;
                if (changed)
                    UpdateLightingState();
            }
        }

        public ALightingSettings Lighting
        {
            get => _lighting;
            set => _lighting = value;
        }
        [HideInInspector,SerializeField]
        private ALightingSettings _lighting;

        public AChunkLoaderSettings ChunkLoader
        {
            get => _chunkLoaderSettings;
            set => _chunkLoaderSettings = value;
        }
        [HideInInspector, SerializeField]
        private AChunkLoaderSettings _chunkLoaderSettings;

        private bool _invokeUpdateLight;

        private float _lightTimer = 0.1f;

        public List<ALayer> layers = new();
        private List<ALayerLoadChunksData> _loadedChunks = new();

        private void Awake()
        {
            if (!Application.isPlaying) return;

            InitLight();
            SimulateLight();

        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;

            _lightTimer += Time.deltaTime;

            if (_invokeUpdateLight && _lightTimer > 0.1f)
            {
                _lightTimer = 0f;

                _invokeUpdateLight = false;

                SimulateLight();
            }

            UpdateLoader();
        }

        public void Refresh(bool immediate = false)
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.Refresh(immediate);
            }
        }

        private void Trim()
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.Trim();
            }

        }

        private void UpdateUndoStack()
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateUndoStack();
            }
        }

        private void UpdateRenderer()
        {
            foreach(ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateRenderer();
            }
        }

        private void UpdateChunksFlags()
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateChunksFlags();
            }
        }

        public ALayer MakeLayer()
        {
            var go = new GameObject();
            var layer = go.AddComponent<ALayer>();

            go.name = $"layer{layers.Count}";
            layer.transform.SetParent(transform);
            layer.transform.localPosition = Vector3.zero;
            layer.Tilemap = this;
            layers.Add(layer);

            return layer;
        }

        public void RemoveLayer(int index)
        {
            DestroyImmediate(layers[index].gameObject);
            layers.RemoveAt(index);
        }

        public void TrimAll(bool immediate = false)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (immediate)
                    layers[i].Trim();
                else
                    layers[i].TrimInvoke = true;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                    layers[i].Clear();
            }
        }

        private void UpdateLightingState()
        {
            var l = _lighting;

            l.ForegroundLayer.UpdateLightingState(l.Enabled && true);
            l.BackgroundLayer.UpdateLightingState(false);
        }

        private byte _lightStep = 25;

        private void InitLight()
        {
            if (!_lighting.Enabled) return;

            foreach (var layer in layers)
            {
                layer.OnTileChanged += ()=>_invokeUpdateLight=true;
                layer.OnChunkCreated += () => _invokeUpdateLight = true;
            }
        }

        private void EmitLight(ALayer layer, int x, int y)
        {
            var light = layer.GetLight(x, y);

            EmitLight(layer, x -1, y, light);
            EmitLight(layer, x +1, y, light);
            EmitLight(layer, x , y -1, light);
            EmitLight(layer, x, y +1, light);
        }

        private void EmitLight(ALayer layer, int x, int y, byte value)
        {
            //Profiler.BeginSample("emitLight");

            if (value < _lightStep)
                return;

            var newValue = (byte)(value - _lightStep);


            var oldValue = layer.GetLight(x, y);

            if (oldValue < newValue)
            {
                layer.SetLight(x, y, newValue);

                EmitLight(layer,x -1,y, newValue);
                EmitLight(layer,x +1,y, newValue);
                EmitLight(layer,x ,y-1, newValue);
                EmitLight(layer,x ,y+1, newValue);
            }

            // Profiler.EndSample();
        }

        private void SimulateLight()
        {
            if (!_lighting.Enabled) return;

            Profiler.BeginSample("light");


            var l = _lighting;
            l.ForegroundLayer.CalculateBounds();
            l.BackgroundLayer.CalculateBounds();

            var fBounds = l.ForegroundLayer.Bounds;

            var bounds = new Bounds(fBounds.center,fBounds.size);
            bounds.Encapsulate(l.BackgroundLayer.Bounds);

            var min = Utilites.GetGridPosition(l.ForegroundLayer, bounds.min);
            var max = Utilites.GetGridPosition(l.ForegroundLayer, bounds.max);


            for (int ix = min.x; ix < max.x; ix++)
            {
                for (int iy = min.y; iy < max.y; iy++)
                {
                    if (l.BackgroundLayer.GetTile(ix, iy) != ATile.EMPTY ||
                        l.ForegroundLayer.GetTile(ix, iy) != ATile.EMPTY)
                    {
                        l.ForegroundLayer.SetLight(ix, iy, 0);
                        continue;
                    }

                    l.ForegroundLayer.SetLight(ix,iy, 255);
                }
            }

            var layer = l.ForegroundLayer;

            for (int ix = min.x; ix < max.x; ix++)
            {
                for (int iy = min.y; iy < max.y; iy++)
                {
                    //var light = layer.GetLight(ix, iy);
                    /*RecursiveLight(layer, ix,iy, 0, -1);
                    RecursiveLight(layer, ix, iy, 0, 1);
                    RecursiveLight(layer, ix, iy, 1, 0);
                    RecursiveLight(layer, ix, iy, -1, 0);*/

                    //EmitLight(layer, ix, iy);


                }
            }

            Profiler.EndSample();
        }

        private Vector2Int _lastGridPos;

        private void UpdateLoader()
        {



            for (int i = 0; i < _loadedChunks.Count; i++)
            {
                foreach (var chunk in _loadedChunks[i].Chunks)
                {
                    chunk.IsLoaded = false;
                }

                _loadedChunks[i].Chunks.Clear();
            }

            _loadedChunks.Clear();

            if (!_chunkLoaderSettings.Enabled)
                return;


            var layer = layers[0];
            var loaderData = _chunkLoaderSettings;

            var gridPosCenter = Utilites.GetGridPosition(layer, loaderData.Target.position);


            if (Vector2Int.Distance(_lastGridPos, gridPosCenter) < AChunk.CHUNK_SIZE)
            {
                return;
            }

            foreach (var t in layers)
            {
                _loadedChunks.Add(new ALayerLoadChunksData()
                {
                    Layer = t,
                    Chunks = new List<AChunk>()
                });
            }

            _lastGridPos = gridPosCenter;

            var loadingChunk = layer.GetOrCreateChunk(gridPosCenter.x, gridPosCenter.y);
            loadingChunk.IsLoaded = true;
            _loadedChunks[0].Chunks.Add(loadingChunk);



           // Debug.Log($"{loadingChunk.GridX} {loadingChunk.GridY}");

            for (int ix = -loaderData.TargetOffset.x; ix <= loaderData.TargetOffset.x; ix++)
            {
                for (int iy = -loaderData.TargetOffset.y; iy <= loaderData.TargetOffset.y; iy++)
                {
                    if(ix == 0 && iy == 0)continue;

                    var temp = layer.GetOrCreateChunk(loadingChunk.GridX + AChunk.CHUNK_SIZE * ix,
                        loadingChunk.GridY + AChunk.CHUNK_SIZE * iy);

                    temp.IsLoaded = true;

                    _loadedChunks[0].Chunks.Add(temp);
                }
            }


        }

    }
}