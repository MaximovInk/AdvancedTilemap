using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {
        [HideInInspector,SerializeField]
        private List<ATilePrefab> _tilePrefabs = new();

        private bool TryGetPrefabAt(int x, int y, out ATilePrefab prefab)
        {
            var pos = new Vector2Int(x, y);

            for (int i = 0; i < _tilePrefabs.Count; i++)
            {
                if (_tilePrefabs[i].Position == pos)
                {
                    prefab = _tilePrefabs[i];
                    return true;
                }
            }

            prefab = null;
            return false;
        }

        private void TryRemovePrefabAt(int x, int y)
        {
            var pos = new Vector2Int(x, y);

            if (!TryGetPrefabAt(x,y, out var tile)) return;

            if (Application.isPlaying)
            {
                Destroy(tile.gameObject);
            }
            else
            {
                DestroyImmediate(tile.gameObject);

            }
            _tilePrefabs.Remove(tile);
        }

        private void SpawnPrefabAt(ATile tile, int x, int y)
        {
            var pos = new Vector2Int(x, y);

            TryRemovePrefabAt(x,y);
            
            if(tile.Prefab == null)return;

            var instance = Instantiate(tile.Prefab, transform);
            instance.transform.SetLocalPositionAndRotation(
                pos * new Vector2(1,1) + new Vector2(0.5f,0.5f),
                Quaternion.identity);

            _tilePrefabs.Add(instance);

            instance.Init(this, x,y, GetTile(x,y));
        }
    }
}
