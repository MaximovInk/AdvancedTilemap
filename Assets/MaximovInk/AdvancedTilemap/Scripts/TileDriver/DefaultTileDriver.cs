using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{

    public class DefaultTileDriver : ATileDriver
    {
        public override string Name => "Default";

        public override void SetTile(ATileDriverData data)
        {
            var tile = data.tile;

            var uv = tile.GetUV(data.variation);

            data.mesh.AddSquare(new MeshDataParameters
            {
                vX0 = data.x,
                vY0 = data.y,
                vX1 = data.x + 1,
                vY1 = data.y + 1,
                uv = uv,
                color = data.color,
                transformData = data.tileData,
                unit = data.tileset.GetTileUnit()
            });
        }


#if UNITY_EDITOR
        public override bool DrawTileGUIPreview(ATileset tileset, ATile tile, byte variationID = 0)
        {
            var uv = tile.GetUV(variationID);
            var uvMin = uv.Min;
            var uvSize = uv.Max - uvMin;

            var aspect = tileset.TileSize.y / tileset.TileSize.x;

            var width = 50;
            var height = width * aspect;

            GUILayout.Label("", GUILayout.Width(width), GUILayout.Height((height)));

            var rect = GUILayoutUtility.GetLastRect();

            GUI.DrawTextureWithTexCoords(rect, tileset.Texture, new Rect(uvMin, uvSize));

            if (GUI.Button(rect, "", GUIStyle.none))
                return true;

            return false;
        }
#endif
    }
}
