using UnityEngine;
using static MaximovInk.Bitmask;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MaximovInk.AdvancedTilemap
{

    public class VerticalPipeDriver : ATileDriver
    {
        public VerticalPipeDriver()
        {
            UVInTilesX = 1;
            UVInTilesY = 3;
        }

        public override string ID => "Vertical Pipe";

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

            var y0 = uvMin.y;
            var y1 = uvMin.y + uvSize.y * 1f / 6f;
            var y2 = uvMin.y + uvSize.y * 1f / 3f;
            var y3 = uvMin.y + uvSize.y * 2f / 3f;
            var y4 = uvMin.y + uvSize.y * 5f / 6f;
            var y5 = uvMax.y;

            var meshDataParam = new MeshDataParameters();
            meshDataParam.color = data.color;
            meshDataParam.unit = data.tileset.GetTileUnit();


            meshDataParam.vX0 = data.x;
            meshDataParam.vY0 = data.y;
            meshDataParam.vX1 = data.x + 1;
            meshDataParam.vY1 = data.y + 1;

            if (bitmask.HasBit(64))
            {
                if (bitmask.HasBit(2)) 
                {
                    meshDataParam.uv = ATileUV.Generate(
                      new Vector2(uvMin.x, y2),
                      new Vector2(uvMax.x, y3));

                    data.mesh.AddSquare(meshDataParam);
                }
                else
                {
                    meshDataParam.uv = ATileUV.Generate(
                      new Vector2(uvMin.x, y3),
                      new Vector2(uvMax.x, y5));

                    data.mesh.AddSquare(meshDataParam);
                }
            }

            if (bitmask.HasBit(2) && !bitmask.HasBit(64))
            {
                meshDataParam.uv = ATileUV.Generate(
                       new Vector2(uvMin.x, y0),
                       new Vector2(uvMax.x, y2));

                data.mesh.AddSquare(meshDataParam);
            }

            if (!bitmask.HasBit(2) && !bitmask.HasBit(64))
            {
                meshDataParam.vX0 = data.x;
                meshDataParam.vY0 = data.y;
                meshDataParam.vX1 = data.x + 1f;
                meshDataParam.vY1 = data.y + 0.5f;
                meshDataParam.uv = ATileUV.Generate(
                      new Vector2( uvMin.x,y0),
                      new Vector2( uvMax.x, y1));

                data.mesh.AddSquare(meshDataParam);

                meshDataParam.vX0 = data.x ;
                meshDataParam.vY0 = data.y + 0.5f;
                meshDataParam.vX1 = data.x + 1;
                meshDataParam.vY1 = data.y + 1;

                meshDataParam.uv = ATileUV.Generate(
                         new Vector2(uvMin.x,y4 ),
                         new Vector2(uvMax.x,y5));

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

            var uvUnit = new Vector2(uvSize.x / 1, uvSize.y / 3);

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
