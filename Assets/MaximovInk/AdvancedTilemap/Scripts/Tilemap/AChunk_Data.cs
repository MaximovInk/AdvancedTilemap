using System;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {
        public event Action OnTileChanged;

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
            return _data.data[x + y * CHUNK_SIZE];
        }

        public bool SetTile(int x, int y, ushort tileID, UVTransform transform = default)
        {
            int idx = x + y * CHUNK_SIZE;


            var variation = Layer.Tileset.GetTile(tileID).GenVariation();

            if (_data.data[idx] == tileID && _data.transforms[idx] == transform)
            {
                if (_data.variations[idx] != variation)
                {
                    _data.variations[idx] = variation;
                    OnTileChanged?.Invoke();
                    _data.IsDirty = true;
                }

                return false;
            }

            _data.data[idx] = tileID;
            _data.collision[idx]
                = IsCollision(tileID);
            _data.transforms[idx] = transform;
            _data.variations[idx] = Layer.Tileset.GetTile(tileID).GenVariation();

            _data.IsDirty = true;

            OnTileChanged?.Invoke();

            return true;
        }

        /*public bool EraseTile(int x, int y)
        {
            if (_data.data[x + y * CHUNK_SIZE] == 0) return false;

            _data.data[x + y * CHUNK_SIZE] = 0;
            _data.collision[x + y * CHUNK_SIZE] = false;

            _data.IsDirty = true;
            OnTileChanged?.Invoke();
            return true;
        }*/

        private bool IsCollision(ushort tileID)
        {
            return (tileID > 0) && !Layer.Tileset.GetTile(tileID).ColliderDisabled;
        }

        public byte GetBitmask(int x, int y)
        {
            return _data.bitmaskData[x + y * CHUNK_SIZE];
        }

        public void SetBitmask(int x, int y, byte bitmask)
        {
            var changed = _data.bitmaskData[x + y * CHUNK_SIZE] != bitmask;

            _data.bitmaskData[x + y * CHUNK_SIZE] = bitmask;

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
            for (int i = 0; i < _data.data.Length; i++)
            {
                if (_data.data[i] != 0)
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

            var tileID = _data.data[idx];

            if (tileID == 0)
            {
                _data.variations[idx] = 0; return;
            }

            _data.variations[idx] = Layer.Tileset.GetTile(tileID).GenVariation();
        }

    }
}
