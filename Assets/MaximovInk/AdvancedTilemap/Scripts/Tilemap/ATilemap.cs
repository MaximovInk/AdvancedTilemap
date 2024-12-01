using System.Collections.Generic;
using UnityEngine;

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

        public bool ShowGrid
        {
            get => _showGrid;
            set => _showGrid=value;
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

        [SerializeField] private bool _showGrid;

        public AChunkLoaderSettings ChunkLoader
        {
            get => _chunkLoaderSettings;
            set => _chunkLoaderSettings = value;
        }
        [HideInInspector, SerializeField]
        private AChunkLoaderSettings _chunkLoaderSettings;

        public List<ALayer> layers = new();
        private readonly List<ALayerLoadChunksData> _loadedChunks = new();

        private Vector2Int _lastGridPos;

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;

            UpdateLoader();
        }

        public void Refresh(bool immediate = false)
        {
            foreach (var layer in layers)
            {
                if (layer == null) continue;

                layer.Refresh(immediate);
            }
        }

        private void Trim()
        {
            foreach (var layer in layers)
            {
                if (layer == null) continue;

                layer.Trim();
            }

        }

        private void UpdateUndoStack()
        {
            foreach (var layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateUndoStack();
            }
        }

        private void UpdateRenderer()
        {
            foreach(var layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateRenderer();
            }
        }

        private void UpdateChunksFlags()
        {
            foreach (var layer in layers)
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
            var layerT = layer.transform;
            layerT.SetParent(transform);
            layerT.localPosition = Vector3.zero;
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
            foreach (var layer in layers)
            {
                if (immediate)
                    layer.Trim();
                else
                    layer.TrimInvoke = true;
            }
        }

        public void Clear()
        {
            foreach (var layer in layers)
            {
                layer.Clear();
            }
        }

        private void UpdateLoader()
        {
            for (var i = 0; i < _loadedChunks.Count; i++)
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

            for (var ix = -loaderData.TargetOffset.x; ix <= loaderData.TargetOffset.x; ix++)
            {
                for (var iy = -loaderData.TargetOffset.y; iy <= loaderData.TargetOffset.y; iy++)
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