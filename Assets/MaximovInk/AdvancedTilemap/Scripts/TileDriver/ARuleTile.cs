using UnityEngine;
using static MaximovInk.Bitmask;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MaximovInk.AdvancedTilemap
{
    public sealed class ARuleTile : ATileDriver
    {
        private const bool blend = true;

        public override string ID => "ARuleTile";
        public override string Description => "Works as starbound and can be filled with unlimited pattern";

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

            var xArray = new float[fillX * 2 + 2 + 1];
            var yArray = new float[fillY * 2 + 3 + 2];

            var stepX = 1f / (xArray.Length - 1);
            var stepY = 1f / (yArray.Length - 1);

            for (var i = 0; i < xArray.Length; i++)
            {
                xArray[i] = uvMin.x + i * stepX * uvSize.x;
            }

            for (var i = 0; i < yArray.Length; i++)
            {
                yArray[i] = uvMin.y + i * stepY * uvSize.y;
            }

            var fX = fillX;
            var fY = fillY;

            var gX = data.x + data.chunkX;
            var gY = data.y + data.chunkY;

             while (gX < 0)
             {
                 gX += fX;
             }

             while (gY < 0)
             {
                 gY += fY;
             }

            var indexX = gX % (fX);
            var indexY = gY % (fY);


            var tileMinUV = new Vector2(xArray[indexX * 2 + 1], yArray[indexY * 2 + 1]);
            var tileMaxUV = new Vector2(xArray[indexX * 2 + 3], yArray[indexY * 2 + 3]);

            var uvStepX = stepX * uvSize.x;
            var uvStepY = stepY * uvSize.y;

            meshDataParam.vX0 = cellMinX;
            meshDataParam.vX1 = cellMaxX;
            meshDataParam.vY0 = cellMinY;
            meshDataParam.vY1 = cellMaxY;

            meshDataParam.uv.Min = tileMinUV;
            meshDataParam.uv.Max = tileMaxUV;

            meshDataParam.color = data.color;
            meshDataParam.unit = data.tileset.GetTileUnit();

            data.mesh.AddSquare(meshDataParam);

            var bitmask = data.selfBitmask;

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
            if (!bitmask.HasBit(LEFT_TOP | TOP | LEFT) ) //!bitmask.HasBit(LEFT_TOP | TOP | LEFT) //&& !bitmask.HasBit(TOP) && !bitmask.HasBit(LEFT)
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
            if (!bitmask.HasBit(RIGHT_TOP | TOP | RIGHT))
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
            if (!bitmask.HasBit(LEFT_BOTTOM | BOTTOM | LEFT))
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
            if (!bitmask.HasBit(RIGHT_BOTTOM | BOTTOM | RIGHT))
            {
                meshDataParam.vX0 = vX3;
                meshDataParam.vX1 = vX4;
                meshDataParam.vY0 = vY0;
                meshDataParam.vY1 = vY1;
                meshDataParam.uv.Min = new Vector2(x3, y0);
                meshDataParam.uv.Max = new Vector2(x4, y1);


                data.mesh.AddSquare(meshDataParam);
            }

            if (!blend)
            {

                //left
                if (!bitmask.HasBit(LEFT))
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
                if (!bitmask.HasBit(BOTTOM))
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
                if (!bitmask.HasBit(RIGHT))
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
                if (!bitmask.HasBit(TOP))
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
            if (!bitmask.HasBit(LEFT))
            {
                //top
                if (bitmask.HasBit(LEFT_TOP))
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
                if (bitmask.HasBit(LEFT_BOTTOM))
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
            if (!bitmask.HasBit(BOTTOM))
            {
                //left
                if (bitmask.HasBit(LEFT_BOTTOM))
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
                if (!bitmask.HasBit(RIGHT_BOTTOM))
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

            //right
            if (!bitmask.HasBit(RIGHT))
            {
                //bottom
                if (bitmask.HasBit(RIGHT_BOTTOM))
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
                if (!bitmask.HasBit(RIGHT_TOP))
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
            if (!bitmask.HasBit(TOP))
            {
                //right

                if (!bitmask.HasBit(RIGHT_TOP))
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
                if (!bitmask.HasBit(LEFT_TOP))
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

        }

        public int FillX(ATile tile)
        {
            var xParam = GetOrAddParameter(tile, "FillX", true);

            return xParam.intValue;
        }

        public int FillY(ATile tile)
        {
            var yParam = GetOrAddParameter(tile, "FillY");

            return yParam.intValue;
        }

#if UNITY_EDITOR

        public override void SelectTile(ATileset tileset, ATileUV uv, ATile tile, int variationID = 0)
        {
            base.SelectTile(tileset, uv, tile, variationID);

            var fillX = FillX(tile);
            var fillY = FillY(tile);

            UpdateUVInTiles(tile, tileset, fillX, fillY);
        }

        private void UpdateUVInTiles(ATile tile, ATileset tileset, int fillX, int fillY)
        {
            UVInTilesX = fillX + 1;
            UVInTilesY = fillY + 2;

            var uv = tile.GetUV();

            var size = tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);

            var newMin = new Vector2(uv.Min.x, uv.Max.y - size.y);
            var newMax = new Vector2(uv.Min.x + size.x, uv.Max.y);

            uv.Min = newMin;
            uv.Max = newMax;

            tile.SetUV(uv);

            var lastMaxX = uv.Max.x;



            for (byte i = 1; i < tile.Variations.Count; i++)
            {
                var vUV = tile.GetUV(i);

                //vUV.Min = new Vector2(lastMaxX, vUV.Min.y);
                //vUV.Max = vUV.Min + tileset.TileTexUnit * new Vector2(UVInTilesX, UVInTilesY);

                newMin = new Vector2(lastMaxX, uv.Min.y);
                newMax = new Vector2(lastMaxX + size.x, uv.Max.y);

                vUV.Min = newMin;
                vUV.Max = newMax;

                lastMaxX = vUV.Max.x;

                tile.SetUV(vUV, i);
            }
        }

        public override bool HasDrawTileProperties => true;

        public override void DrawTileProperties(ATileset tileset, ATile tile)
        {
            base.DrawTileProperties(tileset, tile);

            var xParam = GetOrAddParameter(tile, "FillX");
            var yParam = GetOrAddParameter(tile, "FillY");

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

            var width = 150;
            var viewTileUnit = width / 3;

            GUILayout.Label("", GUILayout.Width(width), GUILayout.Height(width));

            var rect = GUILayoutUtility.GetLastRect();

            var tile1Rect = new Rect(rect.x, rect.y, viewTileUnit, viewTileUnit);
            GUI.DrawTextureWithTexCoords(tile1Rect, tileset.Texture,
                new Rect(uvMin + uvUnit * new Vector2(0, UVInTilesY - 1), uvUnit));

            if (GUI.Button(tile1Rect, "", EditorStyles.selectionRect))
                return true;

            var aspect = UVInTilesX / (float)UVInTilesY;

            var tile2Rect = new Rect(rect.x, rect.y + viewTileUnit + 10, viewTileUnit * 2 * aspect, viewTileUnit * 2);



            GUI.DrawTextureWithTexCoords(tile2Rect, tileset.Texture, new Rect(uvMin, new Vector2(uvSize.x, uvSize.y)));

            GUILayout.Space(20);

            return false;
        }
#endif
    }
}
