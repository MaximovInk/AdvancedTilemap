using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public abstract class ATileDriver
    {
        public abstract string Name { get; }
        public int UVInTilesX { get; protected set; } = 1;
        public int UVInTilesY { get; protected set; } = 1;

        public abstract void SetTile(ATileDriverData data);

        public abstract List<ATile> GenerateTiles(ATileset tileset);

        public abstract bool DrawTileGUIPreview(ATileset tileset, ATile tile, byte variationID = 0);

        public virtual void SelectTile(ATileUV uv, ATile tile, int variationID = 0)
        {
            var uvSize = uv.Max - uv.Min;

            uv.Min -= new Vector2(0, (UVInTilesY-1) * uvSize.y);
            uv.Max += new Vector2((UVInTilesX-1) * uvSize.x, 0);
            tile.SetUV(uv, variationID);
        }

        public static bool IsEquals(ATileDriver a, ATileDriver b)
        {
            var aTileDriver = a is null ? string.Empty : a.Name;
            var bTileDriver = b is null ? string.Empty : b.Name;

            return aTileDriver == bTileDriver;
        }

        //TOOD:REMOVE

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
            var uvMax = uv.Max;
            var uvSize = uv.Max - uvMin;

            if (UVRects == null || UVRects.Length != UVInTilesX * UVInTilesY)
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
    }
}
