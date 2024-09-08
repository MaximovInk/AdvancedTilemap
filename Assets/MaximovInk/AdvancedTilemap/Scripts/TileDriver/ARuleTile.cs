using UnityEngine;
using static MaximovInk.Bitmask;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MaximovInk.AdvancedTilemap
{
    public sealed class ARuleTile : ATileDriver
    {
        public override string Name => "ARuleTile (advanced)";

        public ARuleTile()
        {
            UVInTilesX = 2;
            UVInTilesY = 3;
        }


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

            var fillX = FillX(tile);
            var fillY = FillY(tile);

            var xArray = new float[fillX * 2 + 3];
            var yArray = new float[fillY * 2 + 3 + 2];

            var stepX = 1f / (xArray.Length - 1);
            var stepY = 1f / (yArray.Length - 1);

            for (int i = 0; i < xArray.Length; i++)
            {
                xArray[i] = uvMin.x + i * stepX * uvSize.x;
            }

            for (int i = 0; i < yArray.Length; i++)
            {
                yArray[i] = uvMin.y+ i * stepY * uvSize.y;
            }

            var indexX = data.x < 0 ? ((1 - data.x) % fillX) : data.x % fillX;
            var indexY = data.y < 0 ? ((1 - data.y) % fillY) : data.y % fillY;


            //  Debug.Log($"{fillX} {fillY} {indexX} {indexY} {yArray[^1]} {xArray[^1]}");
            var tileMinUV = new Vector2(xArray[indexX * 2 + 1], yArray[indexY * 2 + 1]);
            var tileMaxUV = new Vector2(xArray[indexX * 2 + 3], yArray[indexY * 2 + 3]);

            var uvStepX = stepX * uvSize.x;
            var uvStepY = stepY * uvSize.y; 

            meshDataParam.vX0 = cellMinX;
            meshDataParam.vX1 = cellMaxX;
            meshDataParam.vY0 = cellMinY;
            meshDataParam.vY1 = cellMaxY;

            meshDataParam.uv.Min  = tileMinUV;
            meshDataParam.uv.Max = tileMaxUV;

            //meshDataParam.uv.Min = uvMin;
            //meshDataParam.uv.Max = uvMax;

            meshDataParam.color = data.color;
            meshDataParam.unit = data.tileset.GetTileUnit();

            data.mesh.AddSquare(meshDataParam);

            byte bitmask = data.bitmask;

            /*
    1 | 2 | 4
    8 | t | 16
    32| 64| 128

    */

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

            var x0 = xArray[0];
            var x1 = xArray[1];
            var x2 = xArray[2];
            var x3 = xArray[fillX * 2 + 1];
            var x4 = xArray[fillX * 2 + 2];

            var y0 = yArray[0];
            var y1 = yArray[1];
            var y2 = yArray[2];
            var y3 = yArray[fillY * 2 + 1];
            var y4 = yArray[fillY * 2 + 2];
            var y5 = yArray[fillY * 2 + 3];
            var y6 = yArray[fillY * 2 + 4];


            var tileY0 = tileMinUV.y;
            var tileY1 = tileMinUV.y + uvStepY;
            var tileY2 = tileMaxUV.y;

            var tileX0 = tileMinUV.x;
            var tileX1 = tileMinUV.x + uvStepX;
            var tileX2 = tileMaxUV.x;

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

            if (!data.blend)
            {

                //left
                if (!HasBit(bitmask, 8))
                {
                    meshDataParam.vX0 = vX0;
                    meshDataParam.vX1 = vX1;
                    meshDataParam.vY0 = vY1;
                    meshDataParam.vY1 = vY3;
                    meshDataParam.uv.Min = new Vector2(x0, tileMinUV.y);
                    meshDataParam.uv.Max = new Vector2(x1, tileMaxUV.y);


                    data.mesh.AddSquare(meshDataParam);
                }
                //bottom
                if (!HasBit(bitmask, 64))
                {
                    meshDataParam.vX0 = vX1;
                    meshDataParam.vX1 = vX3;
                    meshDataParam.vY0 = vY0;
                    meshDataParam.vY1 = vY1;
                    meshDataParam.uv.Min = new Vector2(tileMinUV.x, y0);
                    meshDataParam.uv.Max = new Vector2(tileMaxUV.x, y1);


                    data.mesh.AddSquare(meshDataParam);
                }
                //right
                if (!HasBit(bitmask, 16))
                {

                    meshDataParam.vX0 = vX3;
                    meshDataParam.vX1 = vX4;
                    meshDataParam.vY0 = vY1;
                    meshDataParam.vY1 = vY3;
                    meshDataParam.uv.Min = new Vector2(x3, tileMinUV.y);
                    meshDataParam.uv.Max = new Vector2(x4, tileMaxUV.y);



                    data.mesh.AddSquare(meshDataParam);
                }
                //top
                if (!HasBit(bitmask, 2))
                {
                    meshDataParam.vX0 = vX1;
                    meshDataParam.vX1 = vX3;
                    meshDataParam.vY0 = vY3;
                    meshDataParam.vY1 = vY4;
                    meshDataParam.uv.Min = new Vector2(tileMinUV.x, y3);
                    meshDataParam.uv.Max = new Vector2(tileMaxUV.x, y4);


                    data.mesh.AddSquare(meshDataParam);
                }

                return;
            }

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
                    meshDataParam.uv.Min = new Vector2(x0, tileY1);
                    meshDataParam.uv.Max = new Vector2(x1, tileY2);


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
                    meshDataParam.uv.Min = new Vector2(x0, tileY0);
                    meshDataParam.uv.Max = new Vector2(x1, tileY1);

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
                    meshDataParam.uv.Min = new Vector2(tileX0, y0);
                    meshDataParam.uv.Max = new Vector2(tileX1, y1);


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
                        meshDataParam.uv.Min = new Vector2(tileX1, y0);
                        meshDataParam.uv.Max = new Vector2(tileX2, y1);

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
                    meshDataParam.uv.Min = new Vector2(x3, tileY0);
                    meshDataParam.uv.Max = new Vector2(x4, tileY1);

                    data.mesh.AddSquare(meshDataParam);
                }

                //top
                if (!HasBit(bitmask, 4))
                {
                    meshDataParam.vX0 = vX3;
                    meshDataParam.vX1 = vX4;
                    meshDataParam.vY0 = vY2;
                    meshDataParam.vY1 = vY3;
                    meshDataParam.uv.Min = new Vector2(x3, tileY1);
                    meshDataParam.uv.Max = new Vector2(x4, tileY2);


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
                    meshDataParam.uv.Min = new Vector2(tileX1, y3);
                    meshDataParam.uv.Max = new Vector2(tileX2, y4);

                    data.mesh.AddSquare(meshDataParam);
                }
                //left
                if (!HasBit(bitmask, 1))
                {
                    meshDataParam.vX0 = vX1;
                    meshDataParam.vX1 = vX2;
                    meshDataParam.vY0 = vY3;
                    meshDataParam.vY1 = vY4;
                    meshDataParam.uv.Min = new Vector2(tileX0, y3);
                    meshDataParam.uv.Max = new Vector2(tileX1, y4);

                    data.mesh.AddSquare(meshDataParam);
                }
            }


            /*
           
            

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

*/


        }

        public int FillX(ATile tile)
        {
            var xParam = tile.ParameterContainer.GetParam("FillX");
            xParam ??= tile.ParameterContainer.AddNewParam(new Parameter() { name = "FillX", type = ParameterType.Int, isHidden = true });

            return xParam.intValue;
        }
        public int FillY(ATile tile)
        {
            var yParam = tile.ParameterContainer.GetParam("FillY");
            yParam ??= tile.ParameterContainer.AddNewParam(new Parameter() { name = "FillY", type = ParameterType.Int, isHidden = true });

            return yParam.intValue;
        }

#if UNITY_EDITOR
        public override void SelectTile(ATileset tileset, ATileUV uv, ATile tile, int variationID = 0)
        {
            var xParam = tile.ParameterContainer.GetParam("FillX");
            var yParam = tile.ParameterContainer.GetParam("FillY");

            xParam ??= tile.ParameterContainer.AddNewParam(new Parameter() { name = "FillX", type = ParameterType.Int, isHidden = true });
            yParam ??= tile.ParameterContainer.AddNewParam(new Parameter() { name = "FillY", type = ParameterType.Int, isHidden = true });

            UpdateUVInTiles(tile, tileset, xParam.intValue, yParam.intValue);

            base.SelectTile(tileset, uv, tile, variationID);
        }

        private void UpdateUVInTiles(ATile tile, ATileset tileset, int x,int y)
        {
            UVInTilesX = x + 1;
            UVInTilesY = y + 2;

            var uv = tile.GetUV();
            uv.Max = uv.Min + tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);

            tile.SetUV(uv);

            var lastMaxX = uv.Max.x;

            for (byte i = 1; i < tile.Variations.Count; i++)
            {
                var vUV = tile.GetUV(i);

                vUV.Min = new Vector2(lastMaxX, vUV.Min.y);
                vUV.Max = vUV.Min + tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);

                tile.SetUV(vUV, i);
            }
        }

        public override bool HasDrawTileProperties => true;
        public override void DrawTileProperties(ATileset tileset, ATile tile)
        {
            base.DrawTileProperties(tileset, tile);
            //MINX (1) = 1 +1
            //MINY (1) = 1 +2

            //X(2) = 2 +1
            //Y(2) = 2 +2

            var xParam = tile.ParameterContainer.GetParam("FillX");
            var yParam = tile.ParameterContainer.GetParam("FillY");

            xParam ??= tile.ParameterContainer.AddNewParam(new Parameter() { name = "FillX", type = ParameterType.Int, isHidden = true });
            yParam ??= tile.ParameterContainer.AddNewParam(new Parameter() { name = "FillY", type = ParameterType.Int, isHidden = true });

            EditorGUI.BeginChangeCheck();

            xParam.intValue = Mathf.Max(1, EditorGUILayout.IntField("Fill X", xParam.intValue));
            yParam.intValue = Mathf.Max(1, EditorGUILayout.IntField("Fill Y", yParam.intValue));

            if (EditorGUI.EndChangeCheck())
            {
                UpdateUVInTiles(tile, tileset, xParam.intValue, yParam.intValue);
            }

        }

        public override bool DrawTileGUIPreview(ATileset tileset, ATile tile, byte variationID = 0)
        {
            var uv = tile.GetUV(variationID);
            var uvMin = uv.Min;
            var uvSize = uv.Max - uvMin;

            var uvUnit = new Vector2(uvSize.x / UVInTilesX, uvSize.y / UVInTilesY);

            /* var maxUv = Mathf.Max(UVInTilesX, UVInTilesY);
             var aspectX = 1f;

             if (UVInTilesX > UVInTilesY)
             {
                 aspectX = UVInTilesY / UVInTilesX;
             }*/

            var width = 150;
            var viewTileUnit = width / 3;

            GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(width));

            var rect = GUILayoutUtility.GetLastRect();

            var tile1Rect = new Rect(rect.x, rect.y, viewTileUnit, viewTileUnit);
            GUI.DrawTextureWithTexCoords(tile1Rect, tileset.Texture, new Rect(uvMin + uvUnit * new Vector2(0, UVInTilesY-4), uvUnit));

            if (GUI.Button(tile1Rect, "", EditorStyles.selectionRect))
                return true;

            var aspect = 1f;

            var tile2Rect = new Rect(rect.x, rect.y + viewTileUnit + 10, viewTileUnit * 2, viewTileUnit * 2);

            if (UVInTilesX > UVInTilesY)
            {
                aspect = (float)UVInTilesX / UVInTilesY;

                GUI.DrawTextureWithTexCoords(tile2Rect, tileset.Texture, new Rect(uvMin, new Vector2(uvSize.x, uvSize.y * aspect)));
            }
            else if (UVInTilesY > UVInTilesX)
            {
                aspect = (float)UVInTilesY / UVInTilesX;

                GUI.DrawTextureWithTexCoords(tile2Rect, tileset.Texture, new Rect(uvMin, new Vector2(uvSize.x * aspect, uvSize.y)));
            }
            else
            {
                GUI.DrawTextureWithTexCoords(tile2Rect, tileset.Texture, new Rect(uvMin, new Vector2(uvSize.x * aspect, uvSize.y)));
            }

            GUILayout.Space(20);

            return false;
        }
#endif

        /* var uv = tile.GetUV(variationID);
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

            return false;*/
    }
}
