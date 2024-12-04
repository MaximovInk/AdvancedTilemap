using System;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {
        public event Action OnTileBeginChanged;
        public event Action<ushort,ushort> OnTileChanged;

        [SerializeField, HideInInspector] private AChunkData _data;
        [SerializeField, HideInInspector] private AChunkPersistenceData _persistenceData;

        public void MakeDirty()
        {
            _data.IsDirty = true;
        }

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
            return _data.tiles[x + y * CHUNK_SIZE];
        }

        public bool SetTile(int x, int y, ushort tileID, UVTransform transform = default)
        {
            var idx = x + y * CHUNK_SIZE;

            var variation = Layer.Tileset.GetTile(tileID).GenVariation();

            OnTileBeginChanged?.Invoke();

            if (_data.tiles[idx] == tileID && _data.transforms[idx] == transform)
            {
                if (_data.variations[idx] != variation)
                {
                    _data.variations[idx] = variation;
                    OnTileChanged?.Invoke(tileID,tileID);
                    _data.IsDirty = true;
                }

                return false;
            }


            if (tileID == 0)
            {
                TryRemovePrefabAt(x, y);
            }
            else
            {
                SpawnPrefabAt(Layer.Tileset.GetTile(tileID), x, y);
            }

            var oldTileID = _data.tiles[idx];

            _data.tiles[idx] = tileID;
            _data.collision[idx]
                = IsCollision(tileID);
            _data.transforms[idx] = transform;
            _data.variations[idx] = Layer.Tileset.GetTile(tileID).GenVariation();

            _data.IsDirty = true;

            OnTileChanged?.Invoke(oldTileID, tileID);

            Layer.Tilemap.UpdateLighting();


            return true;
        }

        private bool IsCollision(ushort tileID)
        {
            return (tileID > 0) && !Layer.Tileset.GetTile(tileID).ColliderDisabled;
        }

        public byte GetBitmask(int x, int y)
        {
            return _data.bitmask[x + y * CHUNK_SIZE];
        }

        public void SetBitmask(int x, int y, byte bitmask)
        {
            var changed = _data.bitmask[x + y * CHUNK_SIZE] != bitmask;

            _data.bitmask[x + y * CHUNK_SIZE] = bitmask;

            _data.IsDirty = changed;
        }

        public byte GetSelfBitmask(int x, int y)
        {
            return _data.selfBitmask[x + y * CHUNK_SIZE];
        }

        public void SetSelfBitmask(int x, int y, byte bitmask)
        {
            var changed = _data.selfBitmask[x + y * CHUNK_SIZE] != bitmask;

            _data.selfBitmask[x + y * CHUNK_SIZE] = bitmask;

            _data.IsDirty = changed;
        }

        public void SetColor(int x, int y, Color32 color)
        {
            bool changed = !_data.colors[x + y * CHUNK_SIZE].Equals(color);
            _data.colors[x + y * CHUNK_SIZE] = color;
            _data.IsDirty |= changed ;
        }

        public Color32 GetColor(int x, int y)
        {
            return _data.colors[x + y * CHUNK_SIZE];
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < _data.tiles.Length; i++)
            {
                if (_data.tiles[i] != 0)
                    return false;
            }

            return true;
        }

        public void SetVariation(int x, int y, byte variationID)
        {

        }

        public byte GetVariation(int x, int y)
        {
            return _data.variations[x + y * CHUNK_SIZE];
        }

        public void GenVariation(int x, int y)
        {
            int idx = x + y * CHUNK_SIZE;

            var tileID = _data.tiles[idx];

            if (tileID == 0)
            {
                _data.variations[idx] = 0; return;
            }

            _data.variations[idx] = Layer.Tileset.GetTile(tileID).GenVariation();
        }

    }
}
