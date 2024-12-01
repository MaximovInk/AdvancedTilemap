using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public enum BitmaskMode
    {
        Self,
        All,
        Other
    }

    [System.Serializable]
    public class ATile
    {
        public const ushort EMPTY = 0;

        public ushort ID;

        public List<ATileUV> Variations = new();
        public List<float> Probabilities = new();

        public bool RandomVariations;

        public ParameterContainer ParameterContainer;
        public bool ColliderDisabled;

        public BitmaskMode BitmaskMode = BitmaskMode.Self;

        public ATilePrefab Prefab; 

        public ATileDriver TileDriver
        {
            get { return _cachedTileDriver ??= Utilites.GetTileDriverInstance(_tileDriverID); }
        }
        private ATileDriver _cachedTileDriver;
        public string TileDriverID => _tileDriverID;

        [SerializeField]
        private string _tileDriverID;

        public void SetUV(ATileUV uv,int id = 0)
        {
            Variations[id] = uv;
        }
        public ATileUV GetUV(byte id = 0)
        {
            id = ValidateVariationID(id);

            return Variations[id];
        }

        public byte ValidateVariationID(byte id)
        {
            if (id < 0 || id >= Variations.Count)
                id = GenVariation();

            return id;
        }

        public byte GenVariation()
        {
            float totalProbability = 0;

            for (int i = 0; i < Probabilities.Count; i++)
            {
                totalProbability += Probabilities[i];
            }

            float hitProbability = Random.value * totalProbability;

            for (int i = 0; i < Probabilities.Count; i++)
            {
                if (hitProbability < Probabilities[i])
                {
                    return (byte)i;
                }
                else
                    hitProbability -= Probabilities[i];
            }

            return 0;
        }

        public void AddVariation()
        {
            var variationsCount = Variations.Count;

            var lastTileUV = Variations[variationsCount - 1];

            var uvSize = lastTileUV.Max - lastTileUV.Min;

            var newMin = lastTileUV.Min + new Vector2(uvSize.x, 0);
            var newTileUV = ATileUV.Generate(newMin, newMin + uvSize);

            newTileUV.TextureSize = lastTileUV.TextureSize;

            if (newTileUV.Max.x > 1.05f)
            {
                newMin = new Vector2(0, lastTileUV.Min.y - uvSize.y);
                newTileUV = ATileUV.Generate(newMin, newMin + new Vector2(uvSize.x, uvSize.y));
            }

            Variations.Add(newTileUV);
            Probabilities.Add(1);
        }

        public ATile(string tileDriverID)
        {
            Variations.Add(ATileUV.Identity);
            ParameterContainer = new ParameterContainer();

            _tileDriverID = tileDriverID;
        }
    }
}
