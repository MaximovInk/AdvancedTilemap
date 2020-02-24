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

            return new Rect(new Vector2(pos.x,pos.y),new Vector2(size.x,size.y));
        }

        public Rect GetTexPreview()
        {
            var pos = new Vector2((Position.x) * tileset.TileTexUnitX, Position.y * tileset.TileTexUnitY);
            var unitX = tileset.TileTexUnitX;
            var unitY = tileset.TileTexUnitY;

            if (Type == BlockType.Overlap)
            {
                return new Rect(new Vector2(pos.x,pos.y),new  Vector2(unitX*2f,unitY*2f));
            }
            return new Rect(pos, new Vector2(unitX,unitY));
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

        public Texture2D Texture;

        public List<Tile> tiles = new List<Tile>();

        public Vector2Int TileSize;

        public float TileTexUnitX => (float)TileSize.x / Texture.width;
        public float TileTexUnitY => (float)TileSize.y / Texture.height;

        public Vector2 TileTexUnit => new Vector2(
            (float)TileSize.x / Texture.width, 
            (float)TileSize.y / Texture.height);


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
