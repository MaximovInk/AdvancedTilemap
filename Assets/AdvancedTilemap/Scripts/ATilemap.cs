using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedTilemap
{
    [Serializable]
    public class Command
    {
        [Serializable]
        public class TileData
        {
            public byte LastTile;
            public byte NewTile;
            public Vector2Int Position = Vector2Int.zero;
            public int Layer;
        }
        public List<TileData> TileChanges = new List<TileData>();

        public bool IsEmpty()
        {
            if (TileChanges == null)
                return true;

            for (int i = 0; i < TileChanges.Count; i++)
            {
                if (TileChanges[i].LastTile != TileChanges[i].NewTile)
                    return false;
            }

            return true;
        }
    }

    [ExecuteAlways]
    public class ATilemap : MonoBehaviour
    {
        public const int CHUNK_SIZE = 32;
        public const int MIN_LIQUID_Y = -100;

        public bool DisplayChunksInHierarchy { get { return displayChunksHierarchy; } set { displayChunksHierarchy = value; UpdateChunksFlags(); } }
        public int SortingOrder { get { return sortingOrder; } set { sortingOrder = value; UpdateRenderer(); } }
        public bool UndoEnabled { get { return undoEnabled; } set { if (value != undoEnabled) ClearUndoStack();  undoEnabled = value; } }
        public float LiquidStepsDuration = 0.1f;
        public float ChunkLoadingDuration = 0.5f;
        public bool AutoTrim = true;
        public float ZBlockOffset = 0.1f;
        public int ChunkLoadingOffset = 2;
        public List<Layer> Layers = new List<Layer>();

        [SerializeField]
        private bool displayChunksHierarchy = true;
        [SerializeField]
        private int sortingOrder;
        [SerializeField]
        private float loadTimer = 0;
        [SerializeField]
        private float liquidTimer = 0;
        [SerializeField]
        private bool undoEnabled;

        public Color LiquidMinColor;
        public Color LiquidMaxColor;

        private SpriteRenderer previewTextureBrush;

        #region events

        #endregion

        #region Commands
        public bool IsRecordingCommand { get; private set; }

        [SerializeField]
        public Stack<Command> undoStack = new Stack<Command>();
        [SerializeField]
        public Stack<Command> redoStack = new Stack<Command>();
        [SerializeField]
        private Command currentRecordingCommand;

        private bool CheckUndoEnabled()
        {
            if (!UndoEnabled) 
            {
                Debug.LogError("Undo disabled");
            }
            return UndoEnabled;
        }

        #endregion

        #region public methods

        public void ClearUndoStack()
        {
            undoStack.Clear();
            redoStack.Clear();
            currentRecordingCommand = null;
            IsRecordingCommand = false;
        }
        public void BeginRecordCommand()
        {
            if (!UndoEnabled) return;

            if (IsRecordingCommand)
            {
                Debug.LogError("End recording command before begin new");
                return;
            }

            currentRecordingCommand = new Command();

            IsRecordingCommand = true;
        }
        public void EndRecordCommand()
        {
            if (!CheckUndoEnabled()) return;

            if (!IsRecordingCommand)
            {
                Debug.LogError("Start recording command before ending");
                return;
            }

            IsRecordingCommand = false;
            if (currentRecordingCommand == null || currentRecordingCommand.IsEmpty())
                return;

            undoStack.Push(currentRecordingCommand);
            redoStack.Clear();
        }
        public void Undo()
        {
            if (!CheckUndoEnabled() || undoStack.Count==0) return;

            var command = undoStack.Pop();

            if (command == null)
                return;

            for (int i = 0; i < command.TileChanges.Count; i++)
            {
                var tileChange = command.TileChanges[i];
                if (tileChange.LastTile == 0)
                    Erase(tileChange.Position.x, tileChange.Position.y, tileChange.Layer);
                else
                    SetTile(tileChange.Position.x, tileChange.Position.y, tileChange.LastTile, tileChange.Layer);
            }

            redoStack.Push(command);
        }
        public void Redo()
        {
            if (!CheckUndoEnabled() || redoStack.Count == 0) return;

            var command = redoStack.Pop();

            if (command == null)
                return;

            for (int i = 0; i < command.TileChanges.Count; i++)
            {
                var tileChange = command.TileChanges[i];
                SetTile(tileChange.Position.x, tileChange.Position.y, tileChange.NewTile, tileChange.Layer);
            }

            undoStack.Push(command);
        }

        public void GenPreviewTextureBrush(int sizeX =1,int sizeY = 1)
        {
            if (previewTextureBrush == null)
            {
                var go = new GameObject();
                go.name = "_PreviewTextureBrushInstance";
                previewTextureBrush = go.AddComponent<SpriteRenderer>();
            }
            previewTextureBrush.gameObject.hideFlags = HideFlags.HideAndDontSave;
            //MAYBE ADD PREVIEW TEXTURE
            /*var tex = Layers[layer].Tileset.Texture;
             var rect = Layers[layer].Tileset.GetTile(id).GetTexPreview();
             rect = new Rect(rect.x*tex.width,rect.y* tex.height,rect.width* tex.width,rect.height* tex.height);

             var sprite = Sprite.Create(tex, rect, new Vector2(0,0), Layers[layer].Tileset.PPU);*/
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(1,1,1,1), new Vector2(0, 0), 1);
            previewTextureBrush.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            previewTextureBrush.sortingOrder = 9999;
            previewTextureBrush.color = new Color(1,1,1,0.5f);
            previewTextureBrush.sprite = sprite;
            previewTextureBrush.transform.localScale = new Vector3(sizeX,sizeY,1);

        }
        public void SetActivePreviewBrush(bool value)
        {
            if (previewTextureBrush == null)
                return;
            previewTextureBrush.gameObject.SetActive(value);
        }
        public bool UpdatePreviewBrushPos(Vector2 position)
        {
            if (previewTextureBrush == null)
                return false;
            previewTextureBrush.transform.position = (Vector2)Utilites.GetGridPosition(new Vector2(position.x+0.5f - previewTextureBrush.transform.localScale.x / 2f, position.y+0.5f-previewTextureBrush.transform.localScale.y/2f));
            return true;
        }

        public void TrimAll(bool immediate = false)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                if (immediate)
                {
                    Layers[i].Trim();
                    continue;
                }
                Layers[i].TrimInvoke = true;
            }
        }
        public void ClearAll()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Clear();
            }
        }

        public void OnValidate()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                if (Layers[i] == null)
                {
                    Layers.RemoveAt(i);
                    i--;
                }
            }
        }

        public Layer CreateLayer()
        {
            var go = new GameObject();
            go.transform.SetParent(transform);

            var layer = go.AddComponent<Layer>();

            layer.ZOrder = Layers.Count;
            layer.name = "Layer " + layer.ZOrder;
            layer.transform.localPosition = Vector3.zero;
            layer.transform.localScale = Vector3.one;
            layer.transform.localRotation = Quaternion.identity;
            layer.Tilemap = this;

            return layer;
        }

        public void SetLiquid(int gx,int gy, float value,int layer)
        {
            if (!Layers[layer].LiquidEnabled)
            {
                Debug.LogError("Liquid for layer["+layer+"] disabled");
                return;
            }

            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy, false);

            if (chunk == null)
                return;

            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            chunk.SetLiquid(chunkGridX, chunkGridY,value);
        }
        public void AddLiquid(int gx, int gy, float value, int layer)
        {
            if (!Layers[layer].LiquidEnabled)
            {
                Debug.LogError("Liquid for layer[" + layer + "] disabled");
                return;
            }

            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy,false);

            if (chunk == null)
                return;

            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            chunk.AddLiquid(chunkGridX, chunkGridY,value);
        }
        public float GetLiquid(int gx, int gy, int layer)
        {
            if (!Layers[layer].LiquidEnabled)
            {
                Debug.LogError("Liquid for layer[" + layer + "] disabled");
                return 0;
            }

            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy, false);

            if (chunk == null)
                return 0;

            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            return chunk.GetLiquid(chunkGridX, chunkGridY);
        }

        public void SetTile(int gx, int gy, byte idx, int layer)
        {
            if (Layers[layer].Tileset.GetTile(idx) == null)
                return;

            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            if (chunk.GetTile(chunkGridX, chunkGridY) == idx)
                return;

            var genVariation = chunk.GetTile(chunkGridX, chunkGridY) != idx;

            if (UndoEnabled && IsRecordingCommand)
            {
                if (currentRecordingCommand == null)
                    currentRecordingCommand = new Command();

                var oldID = chunk.GetTile(chunkGridX, chunkGridY);

                var newID = idx;

                var match = currentRecordingCommand.TileChanges.Find(n => n != null && n.Layer == layer && n.Position.x == gx && n.Position.y == gy);

                if (match == null)
                {
                    match = new Command.TileData()
                    {
                        Position = new Vector2Int(gx, gy),
                        LastTile = oldID,
                        NewTile = idx,
                        Layer = layer
                    };
                    currentRecordingCommand.TileChanges.Add(match);
                }
                else
                {
                    match.NewTile = idx;
                }
            }

            chunk.SetTile(chunkGridX, chunkGridY, idx);
           
            if (Layers[layer].Tileset.GetTile(idx).Type == BlockType.Overlap)
            {
                UpdateBitmask(gx, gy,layer);
                UpdateAllNeighborsBitmask(gx, gy,layer);
            }

            if (genVariation)
                GenVariation(gx, gy,layer);

             Layers[layer].TrimInvoke = true;
        }
        public byte GetTile(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy,false);

            if (chunk == null)
                return 0;
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            return chunk.GetTile(chunkGridX, chunkGridY);
        }
        public void Erase(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy,false);
            if (chunk == null)
                return;
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            if (chunk.GetTile(chunkGridX, chunkGridY) == 0)
                return;

            if (UndoEnabled && IsRecordingCommand)
            {
                if (currentRecordingCommand == null)
                    currentRecordingCommand = new Command();

                var oldID = chunk.GetTile(chunkGridX, chunkGridY);

                var match = currentRecordingCommand.TileChanges.Find(n => n != null && n.Layer == layer && n.Position.x == gx && n.Position.y == gy);

                if (match == null)
                {
                    match = new Command.TileData()
                    {
                        Position = new Vector2Int(gx, gy),
                        LastTile = oldID,
                        NewTile = 0,
                        Layer = layer
                    };
                    currentRecordingCommand.TileChanges.Add(match);
                }
                else
                {
                    match.NewTile = 0;
                }
            }

            chunk.Erase(chunkGridX, chunkGridY);

            UpdateBitmask(gx, gy,layer);
            UpdateAllNeighborsBitmask(gx, gy,layer);
            SetVariation(gx, gy, 0,layer);

            if (AutoTrim)
                Layers[layer].TrimInvoke = true;
        }

        public void SetVariation(int gx, int gy, byte variation, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            chunk.SetVariation(chunkGridX, chunkGridY, variation);
        }
        public byte GetVariation(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            return chunk.GetVariation(chunkGridX, chunkGridY);
        }
        public void GenVariation(int gx, int gy, int layer)
        {
            var tile = Layers[layer].Tileset.GetTile(GetTile(gx, gy,layer));

            if (tile == null)
                return;

            SetVariation(gx, gy, (byte)UnityEngine.Random.Range(0, tile.VariationsCount), layer);
        }
    
        public void SetBitmask(int gx, int gy, byte bitmask, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;
            chunk.SetBitmask(chunkGridX, chunkGridY, bitmask);
        }
        public byte GetBitmask(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy,false);
            if (chunk == null)
                return 0;
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;
            return chunk.GetBitmask(chunkGridX, chunkGridY);
        }
        public byte CalculateBitmask(int gx, int gy, int layer)
        {
            var tileIdx = GetTile(gx, gy,layer);

            if (tileIdx == 0)
                return 0;

            byte bitmask = 0;

            if (GetTile(gx - 1, gy + 1, layer) == tileIdx)
            {
                bitmask |= 1;
            }
            if (GetTile(gx, gy + 1, layer) == tileIdx)
            {
                bitmask |= 2;
            }
            if (GetTile(gx + 1, gy + 1, layer) == tileIdx)
            {
                bitmask |= 4;
            }
            if (GetTile(gx - 1, gy, layer) == tileIdx)
            {
                bitmask |= 8;
            }
            if (GetTile(gx + 1, gy, layer) == tileIdx)
            {
                bitmask |= 16;
            }
            if (GetTile(gx - 1, gy - 1, layer) == tileIdx)
            {
                bitmask |= 32;
            }
            if (GetTile(gx, gy - 1, layer) == tileIdx)
            {
                bitmask |= 64;
            }
            if (GetTile(gx + 1, gy - 1, layer) == tileIdx)
            {
                bitmask |= 128;
            }

            return bitmask;
        }
        public void UpdateBitmask(int gx, int gy, int layer)
        {
            var oldBitmask = GetBitmask(gx, gy,layer);

            var newBitmask = CalculateBitmask(gx, gy, layer);

            if (oldBitmask != newBitmask)
            {
                SetBitmask(gx, gy, newBitmask, layer);
            }
        }

        public void SetColor(int gx, int gy, Color32 color, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;
            chunk.SetColor(chunkGridX, chunkGridY, color);
        }
        public Color32 GetColor(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy,false);
            if (chunk == null)
                return Color.clear;
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;
            return chunk.GetColor(chunkGridX, chunkGridY);
        }

        public void CalculateLayersOrder()
        {
            var lastZOrder = 0;
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].ZOrder = lastZOrder;
                if(Layers[i].Tileset != null)
                    lastZOrder += Layers[i].Tileset.ZLenght;
            }
        }

        #endregion

        #region private methods

        private void Start()
        {
            ClearUndoStack();
        }

        private void UpdateChunksFlags()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].UpdateChunksFlags();
            }
        }
        public void RefreshAll(bool immediate = false)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].RefreshAll(immediate);
            }
        }
        private void UpdateRenderer(bool material = false, bool color = false, bool texture = false)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].UpdateRenderer(material,color,texture);
            }
        }

        private void UpdateAllNeighborsBitmask(int gx, int gy, int layer)
        {
            for (int ix = gx - 1; ix < gx + 2; ix++)
            {
                for (int iy = gy - 1; iy < gy + 2; iy++)
                {
                    if (ix == gx && iy == gy)
                        continue;

                    UpdateBitmask(ix, iy,layer);
                }
            }
        }

        public void UnloadAllChunks()
        {
          
            for (int i = 0; i < Layers.Count; i++)
            {
                for (int j = 0; j < Layers[i].chunksCache.Count; j++)
                {
                    var chunk = Layers[i].chunksCache.ElementAt(j).Value;

                    chunk.Unload();

                      
                }
            }
        }
        public void LoadAllChunks()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                for (int j = 0; j < Layers[i].chunksCache.Count; j++)
                {
                    var chunk = Layers[i].chunksCache.ElementAt(j).Value;

                    chunk.Load();

                }
            }
        }
        public void LoadChunks(Vector2Int min, Vector2Int max)
        {
            Parallel.For(0, Layers.Count, (int i) => {
                for (int j = 0; j < Layers[i].chunksCache.Count; j++)
                {
                    var chunk = Layers[i].chunksCache.ElementAt(j).Value;
                    var x = chunk.GridPosX;
                    var y = chunk.GridPosY;
                    if (x >= min.x && x <= max.x && y >= min.y && y <= max.y)
                    {
                        chunk.Load();
                    }
                    else
                    {
                        chunk.Unload();
                    }
                }
            });
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                loadTimer += Time.deltaTime;

                if (loadTimer > ChunkLoadingDuration)
                {
                    var min = Utilites.GetGridPosition(Utilites.BoundsMin(Camera.main) - new Vector2(CHUNK_SIZE * ChunkLoadingOffset, CHUNK_SIZE * ChunkLoadingOffset));
                    var max = Utilites.GetGridPosition(Utilites.BoundsMax(Camera.main) + new Vector2(CHUNK_SIZE * ChunkLoadingOffset, CHUNK_SIZE * ChunkLoadingOffset));
                    LoadChunks(min, max);
                    loadTimer -= ChunkLoadingDuration;
                }

                liquidTimer += Time.deltaTime;

                if (liquidTimer > LiquidStepsDuration)
                {
                    liquidTimer -= LiquidStepsDuration;

                    var min = Utilites.GetGridPosition(Utilites.BoundsMin(Camera.main) - new Vector2(CHUNK_SIZE * ChunkLoadingOffset, CHUNK_SIZE * ChunkLoadingOffset));
                    var max = Utilites.GetGridPosition(Utilites.BoundsMax(Camera.main) + new Vector2(CHUNK_SIZE * ChunkLoadingOffset, CHUNK_SIZE * ChunkLoadingOffset));

                    for (int i = 0; i < Layers.Count; i++)
                    {
                        if (!Layers[i].LiquidEnabled)
                            continue;

                        Layers[i].SimulateLiquid(min, max);
                    }
                }
            }
        }

        #endregion

        
    }
}
