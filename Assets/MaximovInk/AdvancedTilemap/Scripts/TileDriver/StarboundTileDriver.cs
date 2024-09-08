using UnityEngine;
using static MaximovInk.Bitmask;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MaximovInk.AdvancedTilemap
{
    public sealed class StarboundTileDriver : ATileDriver
    {
        public StarboundTileDriver()
        {
            UVInTilesX = 2;
            UVInTilesY = 3;
        }

        public override string Name => "Starbound alike";

        public override void SetTile(ATileDriverData data)
        {
            var tile = data.tile;

            var meshDataParam = new MeshDataParameters();

            meshDataParam.z = ATilemap.Z_TILE_OFFSET * tile.ID;

            var cellMinX = data.x;
            var cellMinY = data.y;

            var cellMaxX = data.x + 1f;
            var cellMaxY = data.y + 1f;

            var uv = tile.GetUV(data.variation);
            var uvMin = uv.Min;
            var uvMax = uv.Max;

            var uvSize = uvMax - uvMin;

            var x0 = uvMin.x;
            var x1 = uvMin.x + uvSize.x * 0.25f;
            var x2 = uvMin.x + uvSize.x * 0.5f;
            var x3 = uvMin.x + uvSize.x * 0.75f;
            var x4 = uvMax.x;

            var y0 = uvMin.y;
            var y1 = uvMin.y + uvSize.y * 1f / 6f;
            var y2 = uvMin.y + uvSize.y * 2f / 6f;
            var y3 = uvMin.y + uvSize.y * 3f / 6f;
            var y4 = uvMin.y + uvSize.y * 4f / 6f;
            var y5 = uvMin.y + uvSize.y * 5f / 6f;
            var y6 = uvMax.y;

            var vX0 = cellMinX - 0.5f;
            var vX1 = cellMinX;
            var vX2 = cellMinX + 0.5f;
            var vX3 = cellMaxX;
            var vX4 = cellMaxX + 0.5f;

            var vY0 = cellMinY - 0.5f;
            var vY1 = cellMinY;
            var vY2 = cellMinY + 0.5f;
            var vY3 = cellMaxY;
            var vY4 = cellMaxY + 0.5f;

            meshDataParam.vX0 = cellMinX;
            meshDataParam.vX1 = cellMaxX;
            meshDataParam.vY0 = cellMinY;
            meshDataParam.vY1 = cellMaxY;
            meshDataParam.uv.Min = new Vector2(x1, y1);
            meshDataParam.uv.Max = new Vector2(x3, y3);
            meshDataParam.color = data.color;
            meshDataParam.unit = data.tileset.GetTileUnit();

            data.mesh.AddSquare(meshDataParam);

            byte bitmask = data.bitmask;
            /*
         1 | 2 | 4
         8 | t | 16
         32| 64| 128
         
         */



            //left top
            if (!HasBit(bitmask, 1) && !HasBit(bitmask, 8) && !HasBit(bitmask, 2))
            {
                meshDataParam.vX0 = vX0;
                meshDataParam.vX1 = vX1;
                meshDataParam.vY0 = vY3;
                meshDataParam.vY1 = vY4;
                meshDataParam.uv.Min = new Vector2(x0, y3);
                meshDataParam.uv.Max = new Vector2(x1, y4);

                data.mesh.AddSquare(meshDataParam);
            }
            //right top
            if (!HasBit(bitmask, 4) && !HasBit(bitmask, 16) && !HasBit(bitmask, 2))
            {
                meshDataParam.vX0 = vX3;
                meshDataParam.vX1 = vX4;
                meshDataParam.vY0 = vY3;
                meshDataParam.vY1 = vY4;
                meshDataParam.uv.Min = new Vector2(x3, y3);
                meshDataParam.uv.Max = new Vector2(x4, y4);

                data.mesh.AddSquare(meshDataParam);
            }
            //left bottom
            if (!HasBit(bitmask, 32) && !HasBit(bitmask, 8) && !HasBit(bitmask, 64))
            {


                meshDataParam.vX0 = vX0;
                meshDataParam.vX1 = vX1;
                meshDataParam.vY0 = vY0;
                meshDataParam.vY1 = vY1;
                meshDataParam.uv.Min = new Vector2(x0, y0);
                meshDataParam.uv.Max = new Vector2(x1, y1);


                data.mesh.AddSquare(meshDataParam);
            }
            //right bottom
            if (!HasBit(bitmask, 128) && !HasBit(bitmask, 16) && !HasBit(bitmask, 64))
            {
                meshDataParam.vX0 = vX3;
                meshDataParam.vX1 = vX4;
                meshDataParam.vY0 = vY0;
                meshDataParam.vY1 = vY1;
                meshDataParam.uv.Min = new Vector2(x3, y0);
                meshDataParam.uv.Max = new Vector2(x4, y1);


                data.mesh.AddSquare(meshDataParam);
            }

            if (data.blend)
            {
                //left
                if (!HasBit(bitmask, 8))
                {
                    //top
                    if (HasBit(bitmask, 1))
                    {
                        meshDataParam.vX0 = vX0;
                        meshDataParam.vX1 = vX1;
                        meshDataParam.vY0 = vY2;
                        meshDataParam.vY1 = vY3;
                        meshDataParam.uv.Min = new Vector2(x1, y5);
                        meshDataParam.uv.Max = new Vector2(x2, y6);

                        data.mesh.AddSquare(meshDataParam);
                    }
                    else
                    {
                        meshDataParam.vX0 = vX0;
                        meshDataParam.vX1 = vX1;
                        meshDataParam.vY0 = vY2;
                        meshDataParam.vY1 = vY3;
                        meshDataParam.uv.Min = new Vector2(x0, y2);
                        meshDataParam.uv.Max = new Vector2(x1, y3);


                        data.mesh.AddSquare(meshDataParam);
                    }
                    //bottom
                    if (HasBit(bitmask, 32))
                    {
                        meshDataParam.vX0 = vX0;
                        meshDataParam.vX1 = vX1;
                        meshDataParam.vY0 = vY1;
                        meshDataParam.vY1 = vY2;
                        meshDataParam.uv.Min = new Vector2(x1, y4);
                        meshDataParam.uv.Max = new Vector2(x2, y5);

                        data.mesh.AddSquare(meshDataParam);
                    }
                    else
                    {
                        meshDataParam.vX0 = vX0;
                        meshDataParam.vX1 = vX1;
                        meshDataParam.vY0 = vY1;
                        meshDataParam.vY1 = vY2;
                        meshDataParam.uv.Min = new Vector2(x0, y1);
                        meshDataParam.uv.Max = new Vector2(x1, y2);

                        data.mesh.AddSquare(meshDataParam);
                    }
                }

                //bottom
                if (!HasBit(bitmask, 64))
                {
                    //left
                    if (HasBit(bitmask, 32))
                    {
                        meshDataParam.vX0 = vX1;
                        meshDataParam.vX1 = vX2;
                        meshDataParam.vY0 = vY0;
                        meshDataParam.vY1 = vY1;
                        meshDataParam.uv.Min = new Vector2(x0, y5);
                        meshDataParam.uv.Max = new Vector2(x1, y6);

                        data.mesh.AddSquare(meshDataParam);
                    }
                    else
                    {
                        meshDataParam.vX0 = vX1;
                        meshDataParam.vX1 = vX2;
                        meshDataParam.vY0 = vY0;
                        meshDataParam.vY1 = vY1;
                        meshDataParam.uv.Min = new Vector2(x1, y0);
                        meshDataParam.uv.Max = new Vector2(x2, y1);


                        data.mesh.AddSquare(meshDataParam);
                    }

                    //right
                    if (!HasBit(bitmask, 128))
                    {
                        if (!HasBit(bitmask, 128))
                        {
                            meshDataParam.vX0 = vX2;
                            meshDataParam.vX1 = vX3;
                            meshDataParam.vY0 = vY0;
                            meshDataParam.vY1 = vY1;
                            meshDataParam.uv.Min = new Vector2(x2, y0);
                            meshDataParam.uv.Max = new Vector2(x3, y1);

                            data.mesh.AddSquare(meshDataParam);
                        }
                    }
                }

                //right
                if (!HasBit(bitmask, 16))
                {
                    //bottom
                    if (HasBit(bitmask, 128))
                    {
                        meshDataParam.vX0 = vX3;
                        meshDataParam.vX1 = vX4;
                        meshDataParam.vY0 = vY1;
                        meshDataParam.vY1 = vY2;
                        meshDataParam.uv.Min = new Vector2(x0, y4);
                        meshDataParam.uv.Max = new Vector2(x1, y5);

                        data.mesh.AddSquare(meshDataParam);
                    }
                    else
                    {
                        meshDataParam.vX0 = vX3;
                        meshDataParam.vX1 = vX4;
                        meshDataParam.vY0 = vY1;
                        meshDataParam.vY1 = vY2;
                        meshDataParam.uv.Min = new Vector2(x3, y1);
                        meshDataParam.uv.Max = new Vector2(x4, y2);

                        data.mesh.AddSquare(meshDataParam);
                    }

                    //top
                    if (!HasBit(bitmask, 4))
                    {
                        meshDataParam.vX0 = vX3;
                        meshDataParam.vX1 = vX4;
                        meshDataParam.vY0 = vY2;
                        meshDataParam.vY1 = vY3;
                        meshDataParam.uv.Min = new Vector2(x3, y2);
                        meshDataParam.uv.Max = new Vector2(x4, y3);


                        data.mesh.AddSquare(meshDataParam);
                    }
                }

                //top
                if (!HasBit(bitmask, 2))
                {
                    //right

                    if (!HasBit(bitmask, 4))
                    {
                        meshDataParam.vX0 = vX2;
                        meshDataParam.vX1 = vX3;
                        meshDataParam.vY0 = vY3;
                        meshDataParam.vY1 = vY4;
                        meshDataParam.uv.Min = new Vector2(x2, y3);
                        meshDataParam.uv.Max = new Vector2(x3, y4);

                        data.mesh.AddSquare(meshDataParam);
                    }
                    //left
                    if (!HasBit(bitmask, 1))
                    {
                        meshDataParam.vX0 = vX1;
                        meshDataParam.vX1 = vX2;
                        meshDataParam.vY0 = vY3;
                        meshDataParam.vY1 = vY4;
                        meshDataParam.uv.Min = new Vector2(x1, y3);
                        meshDataParam.uv.Max = new Vector2(x2, y4);

                        data.mesh.AddSquare(meshDataParam);
                    }
                }

                return;
            }

            //left
            if (!HasBit(bitmask, 8))
            {
                meshDataParam.vX0 = vX0;
                meshDataParam.vX1 = vX1;
                meshDataParam.vY0 = vY1;
                meshDataParam.vY1 = vY3;
                meshDataParam.uv.Min = new Vector2(x0, y1);
                meshDataParam.uv.Max = new Vector2(x1, y3);


                data.mesh.AddSquare(meshDataParam);
            }
            //bottop
            if (!HasBit(bitmask, 64))
            {
                meshDataParam.vX0 = vX1;
                meshDataParam.vX1 = vX3;
                meshDataParam.vY0 = vY0;
                meshDataParam.vY1 = vY1;
                meshDataParam.uv.Min = new Vector2(x1, y0);
                meshDataParam.uv.Max = new Vector2(x3, y1);


                data.mesh.AddSquare(meshDataParam);
            }
            //right
            if (!HasBit(bitmask, 16))
            {

                meshDataParam.vX0 = vX3;
                meshDataParam.vX1 = vX4;
                meshDataParam.vY0 = vY1;
                meshDataParam.vY1 = vY3;
                meshDataParam.uv.Min = new Vector2(x3, y1);
                meshDataParam.uv.Max = new Vector2(x4, y3);



                data.mesh.AddSquare(meshDataParam);
            }
            //top
            if (!HasBit(bitmask, 2))
            {
                meshDataParam.vX0 = vX1;
                meshDataParam.vX1 = vX3;
                meshDataParam.vY0 = vY3;
                meshDataParam.vY1 = vY4;
                meshDataParam.uv.Min = new Vector2(x1, y3);
                meshDataParam.uv.Max = new Vector2(x3, y4);


                data.mesh.AddSquare(meshDataParam);
            }
        }

#if UNITY_EDITOR
        public override bool DrawTileGUIPreview(ATileset tileset, ATile tile, byte variationID = 0)
        {
            var uv = tile.GetUV(variationID);
            var uvMin = uv.Min;
            var uvSize = uv.Max - uvMin;

            var uvUnit = new Vector2(uvSize.x / 2, uvSize.y / 3);

            var width = 150;
            var viewTileUnit = width / 3;

            GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(width));

            var rect = GUILayoutUtility.GetLastRect();

            var tile1Rect = new Rect(rect.x, rect.y, viewTileUnit, viewTileUnit);
            GUI.DrawTextureWithTexCoords(tile1Rect, tileset.Texture, new Rect(uvMin + uvUnit * new Vector2(0, 2), uvUnit));

            if (GUI.Button(tile1Rect, "", EditorStyles.selectionRect))
                return true;

            var tile2Rect = new Rect(rect.x, rect.y + viewTileUnit + 10, viewTileUnit * 2, viewTileUnit * 2);
            GUI.DrawTextureWithTexCoords(tile2Rect, tileset.Texture, new Rect(uvMin, uvUnit * 2));

            GUILayout.Space(20);

            return false;
        }
#endif
    }

}
