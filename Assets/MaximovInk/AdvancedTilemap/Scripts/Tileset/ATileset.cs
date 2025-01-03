﻿using System.Collections.Generic;
using System.Linq;
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

        public List<ATile> GetTiles() => _tiles;

        [SerializeField]
        private List<ATile> _tiles = new List<ATile>();

        public ushort AddTile(ATileDriver tileDriver)
        {
            _tiles.Add(tileDriver.GenerateTile(this));
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

        public ATile GetTile(string name)
        {
            if (_tiles.Count == 0)
                return null;

            return _tiles.FirstOrDefault(n => n.Name == name);
        }

        public bool HasTile(string name)
        {
            return _tiles.Any(n => n.Name == name);
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
        
        public void ClearTiles()
        {
            _tiles.Clear();
        }
    }
}
