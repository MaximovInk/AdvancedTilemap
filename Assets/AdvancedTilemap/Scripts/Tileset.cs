using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AdvancedTilemap
{
    [Serializable]
    public class Tile
    {
        public string Name = "Tile";

        public BlockType Type;
        public int VariationsCount;
        public Vector2Int Position;
        public Vector2Int Size;

        public bool BlendOverlap = true;
        public bool OverlapDepthIsIndex = true;
        public int OverlapDepth;

        public Tileset tileset;

        public Vector2 GetTexPos(int variation = 0)
        {
            if (tileset.Texture == null)
                return Vector2.zero;
            return new Vector2(Position.x + Size.x * variation, Position.y);
        }

        public Vector2 GetPxSize()
        {
            if (tileset.Texture == null)
                return Vector2.zero;

            return new Vector2( tileset.TileTexUnitX * Size.x,  tileset.TileTexUnitY * Size.y);
        }

        public float TexRatio()
        {
            var size = new Vector2(Size.x * VariationsCount, Size.y);

            return size.x/size.y;
        }

        public Rect TextureRect()
        {
            var pos = new Vector2((Position.x) * tileset.TileTexUnitX, Position.y * tileset.TileTexUnitY);
            var size = new Vector2(Size.x * VariationsCount * tileset.TileTexUnitX, Size.y*tileset.TileTexUnitY);

            return new Rect(pos, size);
        }

        public Rect GetTexPreview()
        {
            var pos 
                = new Vector2((Position.x) * tileset.TileTexUnitX, Position.y * tileset.TileTexUnitY);

            var rect = Rect.zero;

            if (Type == BlockType.Overlap)
            {
                rect = new Rect(new Vector2(pos.x,pos.y),new  Vector2(tileset.TileTexUnitX * 2f, tileset.TileTexUnitY * 2f));
            }
            rect = new Rect(pos, new Vector2(tileset.TileTexUnitX, tileset.TileTexUnitY));

           
            return rect;
        }
    }

    public enum BlockType
    { 
        Single,
        Overlap,
        Multi,
        Slope
    }

    [CreateAssetMenu(menuName = "TileTerrain2D")]
    public class Tileset : ScriptableObject
    {
        public int PPU = 8;
        public int ZLenght => tiles.Max(n=>n.OverlapDepth);

        public Texture2D Texture { get => texture; set { texture = value; UpdateValues(); } }
        [SerializeField,HideInInspector]
        private Texture2D texture;

        public List<Tile> tiles = new List<Tile>();

        public Vector2Int TileSize;

        public float TileTexUnitX = 0;
        public float TileTexUnitY = 0;

        public Vector2 TileTexUnit = Vector2.zero;

        public void UpdateValues()
        {
            if (texture == null)
            {
                TileTexUnitX = 0;
                TileTexUnitY = 0;
            }
            else
            {
                TileTexUnitX = (float)TileSize.x / Texture.width;
                TileTexUnitY = (float)TileSize.y / Texture.height;
            }
            TileTexUnit = new Vector2(TileTexUnitY,TileTexUnitY);
        }


        public Tile GetTile(int index)
        {
            if ((index - 1) >= 0 && (index - 1) < tiles.Count)
                return tiles[index - 1];
            return null;
        }

        public Tile GetTile(string name)
        {
            return tiles.Find(n => n.Name == name);
        }

    }
}
