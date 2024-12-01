using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class AChunkData
    {
        public int ArraySize => tiles.Length;
        public ushort[] tiles;

        public byte[] bitmask;
        public byte[] selfBitmask;

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
                    if (tiles[i] > 0) return false;
                }

                return true;
            }
        }

        public AChunkData(int width, int height)
        {
            tiles = new ushort[width * height];
            bitmask = new byte[width * height];
            selfBitmask = new byte[width * height];
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
                IsDirty |= bitmask[i] != value;

                bitmask[i] = value;
            }
        }

        public void FillSelfBitmask(byte value = 0)
        {
            for (var i = 0; i < ArraySize; i++)
            {
                IsDirty |= selfBitmask[i] != value;

                selfBitmask[i] = value;
            }
        }

        public void Fill(ushort value = 0)
        {
            for (var i = 0; i < ArraySize; i++)
            {
                IsDirty |= tiles[i] != value;

                tiles[i] = value;
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

        public bool IsDirty { get=> _isDirty; set
            {
                //if(value)
                //Debug.Log("WFTF");
                _isDirty = value;
            }
        }

        private bool _isDirty;
    }
}
