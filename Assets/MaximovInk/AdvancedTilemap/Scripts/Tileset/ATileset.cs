using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [CreateAssetMenu(menuName = "AdvancedTilemap/Tileset")]
    public class ATileset : ScriptableObject
    {
        public Texture2D Texture;

        public Vector2Int TileSize;

        public int TilesCount => _tiles.Count;

        public Vector2Int PixelPerUnit;

        public Vector2 TileTexUnit { 
            get {
                if (Texture == null)
                    return Vector2.zero;

                return new Vector2(
                    (float)TileSize.x/Texture.width,
                    (float)TileSize.y/Texture.height
                    );
            } 
        }

        [SerializeField]
        private List<ATile> _tiles = new List<ATile>();

        public ATileDriver TileDriver
        {
            get { return _tileDriver ??= Utilites.GetTileDriverInstance(_tileDriverID); }
        }

        public string TileDriverID => _tileDriverID;

        [SerializeField]
        private string _tileDriverID;

        private ATileDriver _tileDriver;

        public ushort AddTile()
        {
            _tiles.Add(TileDriver.GenerateTile(this));
            UpdateIDs();
            return _tiles[^1].ID;
        }

        public ushort AddTile(ATile tile)
        {
            _tiles.Add(tile);
            UpdateIDs();
            return _tiles[^1].ID;
        }

        public void SetTiles(List<ATile> tiles)
        {
            _tiles = tiles;

            UpdateIDs();
        }

        public void RemoveTile(ATile tile)
        {
            _tiles.Remove(tile);

            UpdateIDs();
        }

        public ATile GetTile(int id)
        {
            if (_tiles.Count == 0)
                return null;

            id = Mathf.Clamp(id-1, 0, _tiles.Count-1);

            return _tiles[id];
        }

        public void UpdateIDs()
        {
            for (var i = 0; i < _tiles.Count; i++)
            {
                _tiles[i].ID = (ushort)(i + 1);
            }
        }

        public int IndexOf(ATile tile)
        {
            return _tiles.IndexOf(tile);
        }

        public Vector2 GetTileUnit()
        {
            if (PixelPerUnit.x <= 0) PixelPerUnit.x = TileSize.x;
            if (PixelPerUnit.y <= 0) PixelPerUnit.y = TileSize.y;

            return new Vector2((float)TileSize.x / PixelPerUnit.x, (float)TileSize.y / PixelPerUnit.y);
        }

        public void SetTileDriver(ATileDriver tileDriver)
        {
            var aTileDriver = tileDriver is null ? string.Empty : tileDriver.Name;

            if (aTileDriver == _tileDriverID)
            {
                _tileDriver = tileDriver;
                return;
            }

            _tileDriver = tileDriver;
            _tileDriverID = aTileDriver;
        }

        public void ClearTiles()
        {
            _tiles.Clear();
        }
    }
}
