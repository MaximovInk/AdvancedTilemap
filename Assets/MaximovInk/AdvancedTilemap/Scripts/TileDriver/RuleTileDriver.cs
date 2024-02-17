using UnityEngine;
using static MaximovInk.Bitmask;

namespace MaximovInk.AdvancedTilemap
{
  
    public class RuleTileDriver : ATileDriver
    { 

        public RuleTileDriver()
        {
            UVInTilesX = 3;
            UVInTilesY = 5;
        }

#if UNITY_EDITOR
        public override bool DrawTileGUIPreview(ATileset tileset, ATile tile, byte variationID = 0)
        {
            var width = 150;

            GUILayout.Label("", GUILayout.Width(width), GUILayout.Height((width)));

            var rect = GUILayoutUtility.GetLastRect();

            if(GenUVsGUI(tile, rect, width, variationID))
            {
                GUILayout.Space(100);
                return false;
            }

            var selRect = rect;

            for (int ix = 0; ix < UVInTilesX; ix++)
            {
                for (int iy = 0; iy < UVInTilesY; iy++)
                {
                    var guiRect = GUIViewRects[ix, iy];
                    var uvRect = UVRects[ix, iy];

                    selRect.max = new Vector2(Mathf.Max(selRect.max.x, guiRect.max.x), Mathf.Max(selRect.max.y, guiRect.max.y));

                    GUI.DrawTextureWithTexCoords(guiRect, tileset.Texture, uvRect);
                }
            }

            GUI.color = new Color(1, 1, 1, 0.2f);
            if (GUI.Button(selRect, "*"))
                return true;
            GUI.color = Color.white;

            GUILayout.Space(100);

            return false;
        }
#endif
        private bool Exist(byte position) => HasBit(tempBitmask, position);

        private byte tempBitmask;


        public override string Name => "Rule tile";

        public override void SetTile(ATileDriverData data)
        {
            var tile = data.tile;

            tempBitmask = data.bitmask;

            var uv = tile.GetUV(data.variation);
            var uvMin = uv.Min;
            var uvMax = uv.Max;
            var uvSize = uvMax - uvMin;

            var y0 = uvMin.y;
            var y1 = uvMin.y + uvSize.y * 1 / 10;
            var y2 = uvMin.y + uvSize.y * 2 / 10;
            var y3 = uvMin.y + uvSize.y * 3 / 10;
            var y4 = uvMin.y + uvSize.y * 4 / 10;
            var y5 = uvMin.y+  uvSize.y * 5 / 10;
            var y6 = uvMin.y+  uvSize.y * 6 / 10;
            var y7 = uvMin.y+  uvSize.y * 7 / 10;
            var y8 = uvMin.y + uvSize.y * 8 / 10;
            var y9 = uvMin.y + uvSize.y * 9 / 10;
            var y10 = uvMax.y;

            var x0 = uvMin.x;
            var x1 = uvMin.x + uvSize.x * 1 /6;
            var x2 = uvMin.x + uvSize.x * 2 /6;
            var x3 = uvMin.x + uvSize.x * 3 /6;
            var x4 = uvMin.x + uvSize.x * 4 /6;
            var x5 = uvMin.x + uvSize.x * 5 /6;
            var x6 = uvMax.x;

            var meshDataParam = new MeshDataParameters();

            meshDataParam.vX0 = data.x;
            meshDataParam.vX1 = data.x + 1;
            meshDataParam.vY0 = data.y;
            meshDataParam.vY1 = data.y + 1;
            meshDataParam.color = data.color;
            meshDataParam.unit = data.tileset.GetTileUnit();
            /*
           1 | 2 | 4
           8 | t | 16
           32| 64| 128
           */
            //left top
            if(!Exist(TOP) && !Exist(LEFT) && Exist(BOTTOM | RIGHT | RIGHT_BOTTOM))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x0,y4),
                    new Vector2(x2,y6)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //top
            if (!Exist(TOP) && Exist(LEFT | BOTTOM | RIGHT | LEFT_BOTTOM | RIGHT_BOTTOM))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x2, y4),
                    new Vector2(x4, y6)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //top right
            if (!Exist(TOP) && !Exist(RIGHT) && Exist(BOTTOM | LEFT | LEFT_BOTTOM))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x4, y4),
                    new Vector2(x6, y6)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //left
            if (!Exist(LEFT) && Exist(BOTTOM | RIGHT | RIGHT_BOTTOM | TOP | RIGHT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x0, y2),
                    new Vector2(x2, y4)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //center
            if (Exist(LEFT | BOTTOM | RIGHT | LEFT_BOTTOM | RIGHT_BOTTOM | TOP | LEFT_TOP | RIGHT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x2, y2),
                    new Vector2(x4, y4)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            // right
            if (!Exist(RIGHT)  && Exist(BOTTOM | LEFT | LEFT_BOTTOM | TOP | LEFT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x4, y2),
                    new Vector2(x6, y4)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //left bottom
            if (!Exist(BOTTOM) && !Exist(LEFT) && Exist(TOP | RIGHT | RIGHT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x0, y0),
                    new Vector2(x2, y2)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //bottom
            if (!Exist(BOTTOM) && Exist(LEFT | TOP | RIGHT | LEFT_TOP | RIGHT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x2, y0),
                    new Vector2(x4, y2)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //bottom right
            if (!Exist(BOTTOM) && !Exist(RIGHT) && Exist(TOP | LEFT | LEFT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x4, y0),
                    new Vector2(x6, y2)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }

            //rb corner
            if (Exist(LEFT | BOTTOM | RIGHT | LEFT_BOTTOM | TOP | LEFT_TOP | RIGHT_TOP) && !Exist(RIGHT_BOTTOM))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x0, y8),
                    new Vector2(x2, y10)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //lb corner
            if (Exist(LEFT | BOTTOM | RIGHT | RIGHT_BOTTOM | TOP | LEFT_TOP | RIGHT_TOP) && !Exist(LEFT_BOTTOM))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x2, y8),
                    new Vector2(x4, y10)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //rt corner
            if (Exist(LEFT | BOTTOM | RIGHT | LEFT_BOTTOM | RIGHT_BOTTOM | TOP | LEFT_TOP ) && !Exist(RIGHT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x0, y6),
                    new Vector2(x2, y8)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }
            //lt corner
            if (Exist(LEFT | BOTTOM | RIGHT | LEFT_BOTTOM | RIGHT_BOTTOM | TOP  | RIGHT_TOP) && !Exist( LEFT_TOP))
            {
                meshDataParam.uv = ATileUV.Generate(
                    new Vector2(x2, y6),
                    new Vector2(x4, y8)
                    );

                data.mesh.AddSquare(meshDataParam);

                return;
            }

            //subtiles

            var bitmask1_1 = !Exist(LEFT) && !Exist(TOP);
            var bitmask1_2 = Exist(LEFT) && Exist(TOP) && !Exist(LEFT_TOP);
            var bitmask1_3 = Exist(LEFT) && !Exist(TOP);
            var bitmask1_4 = !Exist(LEFT) && Exist(TOP);
            var bitmask1_5 = Exist(LEFT) && Exist(TOP) &&  Exist(LEFT_TOP);

            var bitmask2_1 = !Exist(RIGHT) && !Exist(TOP);
            var bitmask2_2 = Exist(RIGHT) && Exist(TOP) && !Exist(RIGHT_TOP);
            var bitmask2_3 = Exist(RIGHT) && !Exist(TOP);
            var bitmask2_4 = !Exist(RIGHT) && Exist(TOP);
            var bitmask2_5 = Exist(RIGHT) && Exist(TOP) && Exist(RIGHT_TOP);

            var bitmask3_1 = !Exist(LEFT) && Exist(BOTTOM);
            var bitmask3_2 = Exist(LEFT)&& !Exist(BOTTOM);
            var bitmask3_3 = Exist(LEFT) && Exist(BOTTOM) && !Exist(LEFT_BOTTOM);
            var bitmask3_4 = !Exist(LEFT) && !Exist(BOTTOM);
            var bitmask3_5 = Exist(LEFT) && Exist(BOTTOM) && Exist(LEFT_BOTTOM);

            var bitmask4_1 = !Exist(RIGHT) && Exist(BOTTOM);
            var bitmask4_2 = Exist(RIGHT) && !Exist(BOTTOM);
            var bitmask4_3 = Exist(RIGHT) && Exist(BOTTOM) && !Exist(RIGHT_BOTTOM);
            var bitmask4_4 = !Exist(RIGHT) && !Exist(BOTTOM);
            var bitmask4_5 = Exist(RIGHT) && Exist(BOTTOM) && Exist(RIGHT_BOTTOM);

            var bitmask1 = !Exist(BOTTOM) && Exist(RIGHT) && Exist(LEFT);
            var bitmask2 = Exist(BOTTOM) && Exist(TOP) && !Exist(RIGHT);
            var bitmask3 = Exist(LEFT) && Exist(TOP) && !Exist(RIGHT_BOTTOM) && !Exist(RIGHT) && !Exist(BOTTOM);

            var bitmask4 = Exist(BOTTOM) && Exist(TOP) && !Exist(LEFT);
            var bitmask5 = Exist(RIGHT) && Exist(TOP) && !Exist(LEFT_BOTTOM) && !Exist(LEFT) && !Exist(BOTTOM);

            var bitmask6 = Exist(LEFT) && Exist(BOTTOM) && !Exist(RIGHT_TOP) && !Exist(RIGHT) && !Exist(TOP);
            var bitmask7 = Exist(LEFT) && Exist(RIGHT) && !Exist(TOP);

            var bitmask8 = Exist(RIGHT) && Exist(BOTTOM) && !Exist(LEFT_TOP) && !Exist(LEFT) && !Exist(TOP);

            var leftTopUV = ATileUV.Identity;
            var rightTopUV = ATileUV.Identity;
            var leftBottomUV = ATileUV.Identity;
            var rightBottomUV = ATileUV.Identity;
            //LeftTop
            if (bitmask1_1)
            {
                leftTopUV = ATileUV.Generate(
                    new Vector2(x0,y5),
                    new Vector2(x1,y6));
            }
            else if (bitmask1_2){
                leftTopUV = ATileUV.Generate(
                         new Vector2(x2, y7),
                         new Vector2(x3, y8));
            }
            else if (bitmask1_3)
            {
                leftTopUV = ATileUV.Generate(
                         new Vector2(x2, y5),
                         new Vector2(x3, y6));
            }
            else if (bitmask1_4)
            {
                leftTopUV = ATileUV.Generate(
                         new Vector2(x0, y3),
                         new Vector2(x1, y4));
            }
            else if (bitmask2)
            {
                leftTopUV = ATileUV.Generate(
                        new Vector2(x4, y3),
                        new Vector2(x5, y4));
            }
            else if (bitmask3)
            {
                leftTopUV = ATileUV.Generate(
                        new Vector2(x4, y1),
                        new Vector2(x5, y2));
            }
            else if (bitmask1)
            {
                leftTopUV = ATileUV.Generate(
                        new Vector2(x2, y1),
                        new Vector2(x3, y2));
            }
            else if (bitmask1_5)
            {
                leftTopUV = ATileUV.Generate(
                         new Vector2(x2, y3),
                         new Vector2(x3, y4));
            }
            //RightTop
            if (bitmask2_1)
            {
                rightTopUV = ATileUV.Generate(
                    new Vector2(x5, y5),
                    new Vector2(x6, y6));
            }
            else if (bitmask2_2)
            {
                rightTopUV = ATileUV.Generate(
                    new Vector2(x1, y7),
                    new Vector2(x2, y8));
            }
            else if (bitmask2_3)
            {
                rightTopUV = ATileUV.Generate(
                    new Vector2(x3, y5),
                    new Vector2(x4, y6));
            }
            else if (bitmask2_4)
            {
                rightTopUV = ATileUV.Generate(
                    new Vector2(x5, y3),
                    new Vector2(x6, y4));
            }
            else if (bitmask4)
            {
                rightTopUV = ATileUV.Generate(
                        new Vector2(x1, y3),
                        new Vector2(x2, y4));
            }
            else if (bitmask5)
            {
                rightTopUV = ATileUV.Generate(
                        new Vector2(x1, y1),
                        new Vector2(x2, y2));
            }
            else if (bitmask1)
            {
                rightTopUV = ATileUV.Generate(
                        new Vector2(x3, y1),
                        new Vector2(x4, y2));
            }
            else if (bitmask2_5)
            {
                rightTopUV = ATileUV.Generate(
                    new Vector2(x3, y3),
                    new Vector2(x4, y4));
            }
            //LeftBottom
            if (bitmask3_1)
            {
                leftBottomUV = ATileUV.Generate(
                 new Vector2(x0, y2),
                 new Vector2(x1, y3));
            }
            else if(bitmask3_2)
            {
                leftBottomUV = ATileUV.Generate(
                 new Vector2(x2, y0),
                 new Vector2(x3, y1));
            }
            else if (bitmask3_3)
            {
                leftBottomUV = ATileUV.Generate(
                 new Vector2(x2, y8),
                 new Vector2(x3, y9));
            }
            else if (bitmask3_4)
            {
                leftBottomUV = ATileUV.Generate(
                 new Vector2(x0, y0),
                 new Vector2(x1, y1));
            }
            else if (bitmask6)
            {
                leftBottomUV = ATileUV.Generate(
              new Vector2(x4, y4),
              new Vector2(x5, y5));
            }
            else if (bitmask2)
            {
                leftBottomUV = ATileUV.Generate(
              new Vector2(x4, y2),
              new Vector2(x5, y3));
            }
            else if (bitmask7)
            {
                leftBottomUV = ATileUV.Generate(
              new Vector2(x2, y4),
              new Vector2(x3, y5));
            }
            else if (bitmask3_5)
            {
                leftBottomUV = ATileUV.Generate(
                 new Vector2(x2, y2),
                 new Vector2(x3, y3));
            }
            //RightBottom
            if (bitmask4_1)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x5, y2),
                 new Vector2(x6, y3));
            }
            else if (bitmask4_2)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x3, y0),
                 new Vector2(x4, y1));
            }
            else if (bitmask4_3)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x1, y8),
                 new Vector2(x2, y9));
            }
            else if (bitmask4_4)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x5, y0),
                 new Vector2(x6, y1));
            }
            else if (bitmask8)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x1, y4),
                 new Vector2(x2, y5));
            }
            else if (bitmask4)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x1, y2),
                 new Vector2(x2, y3));
            }
            else if (bitmask7)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x3, y4),
                 new Vector2(x4, y5));
            }
            else if (bitmask4_5)
            {
                rightBottomUV = ATileUV.Generate(
                 new Vector2(x3, y2),
                 new Vector2(x4, y3));
            }

            AddSubtile(meshDataParam, data, leftTopUV, SubtilePosition.LEFT_TOP);
            AddSubtile(meshDataParam, data, rightTopUV, SubtilePosition.RIGHT_TOP);
            AddSubtile(meshDataParam, data, leftBottomUV, SubtilePosition.LEFT_BOTTOM);
            AddSubtile(meshDataParam, data, rightBottomUV, SubtilePosition.RIGHT_BOTTOM);
        }

        public enum SubtilePosition
        {
            LEFT_TOP,
            RIGHT_TOP,
            LEFT_BOTTOM,
            RIGHT_BOTTOM,
        }

        protected void AddSubtile(MeshDataParameters meshDataParam, ATileDriverData data, ATileUV uv, SubtilePosition position)
        {
            switch (position)
            {
                case SubtilePosition.LEFT_TOP:
                    meshDataParam.vX0 = data.x;
                    meshDataParam.vX1 = data.x + 0.5f;
                    meshDataParam.vY0 = data.y + 0.5f;
                    meshDataParam.vY1 = data.y + 1f;
                    break;
                case SubtilePosition.RIGHT_TOP:
                    meshDataParam.vX0 = data.x + 0.5f;
                    meshDataParam.vX1 = data.x + 1f;
                    meshDataParam.vY0 = data.y + 0.5f;
                    meshDataParam.vY1 = data.y + 1f;
                    break;
                case SubtilePosition.LEFT_BOTTOM:
                    meshDataParam.vX0 = data.x;
                    meshDataParam.vX1 = data.x + 0.5f;
                    meshDataParam.vY0 = data.y;
                    meshDataParam.vY1 = data.y + 0.5f;
                    break;
                case SubtilePosition.RIGHT_BOTTOM:
                    meshDataParam.vX0 = data.x + 0.5f;
                    meshDataParam.vX1 = data.x + 1f;
                    meshDataParam.vY0 = data.y;
                    meshDataParam.vY1 = data.y + 0.5f;
                    break;
            }

            meshDataParam.uv = uv;

            data.mesh.AddSquare(meshDataParam);
        }

        protected void AddSubtiles(MeshDataParameters meshDataParam, ATileDriverData data, ATileUV lb, ATileUV lt, ATileUV rb, ATileUV rt)
        {
            AddSubtile(meshDataParam, data, lb, SubtilePosition.LEFT_BOTTOM);

            AddSubtile(meshDataParam, data, rb, SubtilePosition.RIGHT_BOTTOM);

            AddSubtile(meshDataParam, data, lt, SubtilePosition.LEFT_TOP);

            AddSubtile(meshDataParam, data, rt, SubtilePosition.RIGHT_TOP);
        }
    }
}
