using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AdvancedTilemap
{
    public delegate void ChunkLoaded(int x, int y,int layer);
    public delegate void ChunkUnloaded(int x, int y,int layer);
    public delegate void TileDataChanged(int x, int y, int idx, int layer);

    [ExecuteAlways]
    public class ATilemap : MonoBehaviour
    {
        public const int CHUNK_SIZE = 32;

        public bool AutoTrim = true;

        public bool DisplayChunksInHierarchy { get { return displayChunksHierarchy; } set { displayChunksHierarchy = value; UpdateChunksFlags(); } }
        public int SortingOrder { get { return sortingOrder; } set { sortingOrder = value; UpdateRenderer(); } }

        public List<Layer> Layers = new List<Layer>();

        public int maxLightDistance = 10;

        [SerializeField]
        private bool displayChunksHierarchy = true;
        [SerializeField]
        private int sortingOrder;

        public float ZBlockOffset = 0.1f;

        public Vector2Int RenderMin;
        public Vector2Int RenderMax;

        public int chunkLoadingOffset = 2;

        public float loadRate = 0.5f;

        private float loadTimer = 0;

        private SpriteRenderer previewTextureBrush;

        #region events

        public event TileDataChanged OnTileDataChanged;
        public event ChunkLoaded OnChunkLoaded;
        public event ChunkUnloaded OnChunkUnloaded;

        #endregion

        #region public methods

        public void GenPreviewTextureBrush(int sizeX =1,int sizeY = 1)
        {
            if (previewTextureBrush == null)
            {
                var go = new GameObject();
                go.name = "_PreviewTextureBrushInstance";
                previewTextureBrush = go.AddComponent<SpriteRenderer>();
            }
            previewTextureBrush.gameObject.hideFlags = HideFlags.HideAndDontSave;

            /*var tex = Layers[layer].Tileset.Texture;
             var rect = Layers[layer].Tileset.GetTile(id).GetTexPreview();
             rect = new Rect(rect.x*tex.width,rect.y* tex.height,rect.width* tex.width,rect.height* tex.height);

             var sprite = Sprite.Create(tex, rect, new Vector2(0,0), Layers[layer].Tileset.PPU);*/
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(1,1,1,1), new Vector2(0, 0), 1);

            previewTextureBrush.sharedMaterial = Layers[0].Material;
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

        public void TrimAll()
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Trim();
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
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            chunk.SetLiquid(chunkGridX, chunkGridY,value);
        }

        public void AddLiquid(int gx, int gy, float value, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            chunk.AddLiquid(chunkGridX, chunkGridY,value);
        }

        public float GetLiquid(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
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

            var genVariation = GetTile(gx, gy,layer) != idx;

            chunk.SetTile(chunkGridX, chunkGridY, idx);
           
            if (Layers[layer].Tileset.GetTile(idx).Type == BlockType.Overlap)
            {
                UpdateBitmask(gx, gy,layer);
                UpdateAllNeighborsBitmask(gx, gy,layer);
            }

            if (genVariation)
                GenVariation(gx, gy,layer);

            if (AutoTrim)
                Layers[layer].Trim();
        }

        public byte GetTile(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            return chunk.GetTile(chunkGridX, chunkGridY);
        }

        public void Erase(int gx, int gy, int layer)
        {
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
            int chunkGridX = (gx < 0 ? -gx - 1 : gx) % CHUNK_SIZE;
            int chunkGridY = (gy < 0 ? -gy - 1 : gy) % CHUNK_SIZE;
            if (gx < 0) chunkGridX = CHUNK_SIZE - 1 - chunkGridX;
            if (gy < 0) chunkGridY = CHUNK_SIZE - 1 - chunkGridY;

            if (chunk.GetTile(chunkGridX, chunkGridY) == 0)
                return;

            chunk.Erase(chunkGridX, chunkGridY);

            UpdateBitmask(gx, gy,layer);
            UpdateAllNeighborsBitmask(gx, gy,layer);
            SetVariation(gx, gy, 0,layer);

            if (AutoTrim)
                Layers[layer].Trim();
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
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
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
            Chunk chunk = Layers[layer].GetOrCreateChunk(gx, gy);
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

                    chunk.Disable();

                      
                }
            }
        }

        public void LoadAllChunks()
        {
            List<Tuple<int, int>> loaded = new List<Tuple<int, int>>();

            for (int i = 0; i < Layers.Count; i++)
            {
                for (int j = 0; j < Layers[i].chunksCache.Count; j++)
                {
                    var chunk = Layers[i].chunksCache.ElementAt(j).Value;

                    chunk.Enable();

                }
            }
        }

        public void LoadChunks(Vector2Int min, Vector2Int max)
        {

            for (int i = 0; i < Layers.Count; i++)
            {
                for (int x = min.x; x < max.x; x++)
                {
                    for (int y = min.y; y < max.y; y++)
                    {
                        var chunk = Layers[i].GetOrCreateChunk(x, y, false);

                        if (chunk != null)
                        {
                            chunk.Enable();

                        }


                    }
                }
            }
        }

        private void Update()
        {
            loadTimer += Time.deltaTime;


            if (Application.isPlaying)
            {
                if (loadTimer > loadRate)
                {
                    UnloadAllChunks();

                    var min = Utilites.GetGridPosition(Utilites.BoundsMin(Camera.main) - new Vector2(CHUNK_SIZE * chunkLoadingOffset, CHUNK_SIZE*chunkLoadingOffset));
                    var max = Utilites.GetGridPosition(Utilites.BoundsMax(Camera.main) + new Vector2(CHUNK_SIZE * chunkLoadingOffset, CHUNK_SIZE*chunkLoadingOffset));
                    LoadChunks(min, max);
                    loadTimer = 0;
                }
            }


        }
        #endregion
    }
}
