using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [System.Serializable]
    public class AChunkData
    {
        public int ArraySize => data.Length;
        public ushort[] data;
        public byte[] bitmaskData;
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
                    if (data[i] > 0) return false;
                }

                return true;
            }
        }

        public AChunkData(int width, int height)
        {
            data = new ushort[width * height];
            bitmaskData = new byte[width * height];
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
                IsDirty |= bitmaskData[i] != value;

                bitmaskData[i] = value;
            }
        }

        public void Fill(ushort value = 0)
        {
            for (var i = 0; i < ArraySize; i++)
            {
                IsDirty |= data[i] != value;

                data[i] = value;
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
