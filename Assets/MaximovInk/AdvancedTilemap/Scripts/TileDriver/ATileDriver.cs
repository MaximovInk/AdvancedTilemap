using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [Serializable]
    public abstract class ATileDriver
    {
        public abstract string Name { get; }

        public virtual int UVInTilesX
        {
            get;
            protected set;
        } = 1;

        public virtual int UVInTilesY
        {
            get;
            protected set;
        } = 1;

        [SerializeField, HideInInspector] protected int _uvInTilesX = 1;
        [SerializeField, HideInInspector] protected int _uvInTilesY = 1;

        public abstract void SetTile(ATileDriverData data);

        public virtual ATile GenerateTile(ATileset tileset)
        {
            var tile = new ATile(Name);
            var uv = new ATileUV();
           
            uv.TextureSize = new Vector2Int(tileset.Texture.width, tileset.Texture.height);
            uv.Min = tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);
            uv.Max = uv.Min + tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);
           
            tile.SetUV(uv);

            return tile;
        }

        public virtual List<ATile> GenerateTiles(ATileset tileset)
        {
            var tiles = new List<ATile>();

            var texture = tileset.Texture;

            if (texture == null) return tiles;

            var width = texture.width / (tileset.TileSize.x * UVInTilesX);
            var height = texture.height / (tileset.TileSize.y * UVInTilesY);

            var texSize = new Vector2Int(width, height);

            for (var ix = 0; ix <= width; ix++)
            {
                for (var iy = 0; iy < height; iy++)
                {
                    var tile = new ATile(Name);
                    var uv = new ATileUV();
                    uv.Min = new Vector2Int(ix, iy) * tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);
                    uv.Max = uv.Min + tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);
                    uv.TextureSize = texSize;

                    tile.SetUV(uv);
                    tiles.Add(tile);
                }
            }

            return tiles;
        }

        public static bool IsEquals(ATileDriver a, ATileDriver b)
        {
            var aTileDriver = a is null ? string.Empty : a.Name;
            var bTileDriver = b is null ? string.Empty : b.Name;

            return aTileDriver == bTileDriver;
        }

#if UNITY_EDITOR
        public virtual void SelectTile(ATileset tileset, ATileUV uv, ATile tile, int variationID = 0)
        {
            var uvSize = uv.Max - uv.Min;

            uv.Min -= new Vector2(0, (UVInTilesY - 1) * uvSize.y);
            uv.Max += new Vector2((UVInTilesX - 1) * uvSize.x, 0);
            tile.SetUV(uv, variationID);

        }

        protected Rect[,] UVRects;
        protected Rect[,] GUIViewRects;

        protected Rect[,] GenUVRects(Vector2 uvMin, Vector2 uvSize)
        {
            var uvUnit = new Vector2(uvSize.x / UVInTilesX, uvSize.y / UVInTilesY);

            var rects = new Rect[UVInTilesX, UVInTilesY];

            for (int ix = 0; ix < UVInTilesX; ix++)
            {
                for (int iy = 0; iy < UVInTilesY; iy++)
                {
                    ATileUV aTileUV = ATileUV.Generate(uvMin + uvUnit * new Vector2(ix, UVInTilesY - 1 - iy), uvUnit);

                    rects[ix, iy] = new Rect(aTileUV.Min, aTileUV.Max);
                }
            }

            return rects;
        }

        protected Rect[,] GenGUITileUnit(Rect rect, int width, Vector2 space)
        {
            var GUITileUnit = width / Mathf.Max(UVInTilesX, UVInTilesY);

            var rects = new Rect[UVInTilesX, UVInTilesY];

            for (int ix = 0; ix < UVInTilesX; ix++)
            {
                for (int iy = 0; iy < UVInTilesY; iy++)
                {
                    rects[ix, iy] = new Rect(
                        rect.x + GUITileUnit * ix + ix * space.x,
                        rect.y + GUITileUnit * iy + iy * space.y,
                        GUITileUnit,
                        GUITileUnit);
                }
            }

            return rects;
        }

        protected bool GenUVsGUI(ATile tile, Rect rect, int size, byte variationID)
        {
            var uv = tile.GetUV(variationID);
            var uvMin = uv.Min;
            var uvSize = uv.Max - uvMin;

            UVRects = GenUVRects(uvMin, uvSize);

            if (GUIViewRects == null || GUIViewRects.Length != UVInTilesX * UVInTilesY)
            {
                if (rect.x != 0 && rect.y != 0)
                    GUIViewRects = GenGUITileUnit(rect, size, new Vector2(10, 10));
                else
                {

                    return true;
                }
            }
            return false;
        }

        public abstract bool DrawTileGUIPreview(ATileset tileset, ATile tile, byte variationID = 0);

        public virtual bool HasDrawTileProperties => false;

        public virtual void DrawTileProperties(ATileset tileset, ATile tile)
        {}
#endif
    }
}
