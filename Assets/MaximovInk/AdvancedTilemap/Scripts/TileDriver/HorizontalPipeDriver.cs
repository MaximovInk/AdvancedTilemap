using UnityEngine;
using static MaximovInk.Bitmask;

namespace MaximovInk.AdvancedTilemap
{

    public class HorizontalPipeDriver : ATileDriver
    {
        public HorizontalPipeDriver()
        {
            UVInTilesX = 3;
            UVInTilesY = 1;
        }

        public override string ID => "Horizontal Pipe";

        public override void SetTile(ATileDriverData data)
        {
            var tile = data.tile;


            byte bitmask = data.selfBitmask;
            /*
            1 | 2 | 4
            8 | t | 16
            32| 64| 128

            */
            var uv = tile.GetUV(data.variation);
            var uvMin = uv.Min;
            var uvMax = uv.Max;
            var uvSize = uvMax - uvMin;

            var x0 = uvMin.x;
            var x1 = uvMin.x + uvSize.x * 1f / 6f;
            var x2 = uvMin.x + uvSize.x * 1f / 3f;
            var x3 = uvMin.x + uvSize.x * 2f / 3f;
            var x4 = uvMin.x + uvSize.x * 5f / 6f;
            var x5 = uvMax.x;

            var meshDataParam = new MeshDataParameters();
            meshDataParam.color = data.color;
            meshDataParam.unit = data.tileset.GetTileUnit();


            meshDataParam.vX0 = data.x;
            meshDataParam.vY0 = data.y;
            meshDataParam.vX1 = data.x + 1;
            meshDataParam.vY1 = data.y + 1;

            if (bitmask.HasBit(8))
            {
                if (bitmask.HasBit(16))
                {
                    meshDataParam.uv = ATileUV.Generate(
                      new Vector2(x2, uvMin.y),
                      new Vector2(x3, uvMax.y));

                    data.mesh.AddSquare(meshDataParam);
                }
                else
                {
                    meshDataParam.uv = ATileUV.Generate(
                      new Vector2(x3, uvMin.y),
                      new Vector2(x5, uvMax.y));

                    data.mesh.AddSquare(meshDataParam);
                }
            }

            if (bitmask.HasBit(16) && !bitmask.HasBit(8))
            {
                meshDataParam.uv = ATileUV.Generate(
                       new Vector2(x0, uvMin.y),
                       new Vector2(x2, uvMax.y));

                data.mesh.AddSquare(meshDataParam);
            }

            if (!bitmask.HasBit(16) && !bitmask.HasBit(8))
            {
                meshDataParam.vX0 = data.x;
                meshDataParam.vY0 = data.y;
                meshDataParam.vX1 = data.x + 0.5f;
                meshDataParam.vY1 = data.y + 1;
                meshDataParam.uv = ATileUV.Generate(
                      new Vector2(x0, uvMin.y),
                      new Vector2(x1, uvMax.y));

                data.mesh.AddSquare(meshDataParam);

                meshDataParam.vX0 = data.x + 0.5f;
                meshDataParam.vY0 = data.y;
                meshDataParam.vX1 = data.x + 1;
                meshDataParam.vY1 = data.y + 1;
                meshDataParam.uv = ATileUV.Generate(
                        new Vector2(x4, uvMin.y),
                         new Vector2(x5, uvMax.y));

                data.mesh.AddSquare(meshDataParam);
            }
        }

#if UNITY_EDITOR
        public override bool DrawTileGUIPreview(ATileset tileset, ATile tile, byte variationID = 0)
        {
            var uv = tile.GetUV(variationID);
            var uvMin = uv.Min;
            var uvMax = uv.Max;
            var uvSize = uv.Max - uvMin;

            var uvUnit = new Vector2(uvSize.x / 3, uvSize.y / 1);

            var width = 150;
            var viewTileUnit = width / 3;

            GUILayout.Label("", GUILayout.Width(width + 30), GUILayout.Height((width)));

            var rect = GUILayoutUtility.GetLastRect();

            var tile1Rect = new Rect(rect.x, rect.y, viewTileUnit, viewTileUnit);
            GUI.DrawTextureWithTexCoords(tile1Rect, tileset.Texture, new Rect(uvMin, uvUnit));

            if (GUI.Button(tile1Rect, "", GUIStyle.none))
                return true;

            var tile2Rect = new Rect(rect.x + viewTileUnit + 10, rect.y, viewTileUnit, viewTileUnit);
            GUI.DrawTextureWithTexCoords(tile2Rect, tileset.Texture, new Rect(uvMin + new Vector2(uvUnit.x, 0), uvUnit));

            if (GUI.Button(tile2Rect, "", GUIStyle.none))
                return true;

            var tile3Rect = new Rect(rect.x + viewTileUnit * 2 + 20, rect.y, viewTileUnit, viewTileUnit);
            GUI.DrawTextureWithTexCoords(tile3Rect, tileset.Texture, new Rect(uvMin + new Vector2(uvUnit.x * 2, 0), uvUnit));

            if (GUI.Button(tile3Rect, "", GUIStyle.none))
                return true;

            GUILayout.Space(20);

            return false;
        }
#endif
    }

}
