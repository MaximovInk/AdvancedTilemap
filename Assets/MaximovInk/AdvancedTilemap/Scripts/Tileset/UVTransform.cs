using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MaximovInk.AdvancedTilemap
{
    [Serializable]
    public struct UVTransform
    {
        public readonly bool Equals(UVTransform other)
        {
            return _rot90 == other._rot90 && _flipVertical == other._flipVertical && _flipHorizontal == other._flipHorizontal;
        }

        public readonly override bool Equals(object obj)
        {
            return obj is UVTransform other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(_rot90, _flipVertical, _flipHorizontal);
        }

        [FormerlySerializedAs("rot90")] public bool _rot90;
        [FormerlySerializedAs("flipVertical")] public bool _flipVertical;
        [FormerlySerializedAs("flipHorizontal")] public bool _flipHorizontal;

        public static bool operator ==(UVTransform c1, UVTransform c2)
        {
            return c1.Equals(c2);
        }
        public static bool operator !=(UVTransform c1, UVTransform c2)
        {
            return !c1.Equals(c2);
        }

        public void Print()
        {
            Debug.Log(ToString());
        }

        public readonly override string ToString()
        {
            return $"r:{_rot90} v:{_flipVertical} h:{_flipHorizontal}";
        }
    }
}