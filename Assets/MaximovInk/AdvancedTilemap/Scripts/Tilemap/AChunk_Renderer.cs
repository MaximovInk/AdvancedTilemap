
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {

        [HideInInspector, SerializeField]
        private MeshData _meshData;

        public MeshData GetMeshData() => _meshData;

        [SerializeField, HideInInspector] protected MeshFilter _meshFilter;
        [SerializeField, HideInInspector] protected MeshRenderer _meshRenderer;

        private MaterialPropertyBlock materialProperty;

        public void Refresh(bool immediate = false)
        {
            _data.IsDirty = true;
            if (immediate)
            {
                if (Layer.UpdateVariationsOnRefresh)
                    GenerateVariations();
                Update();
            }
        }

        private void CheckRenderer()
        {
            if (this == null) return;

            if (_meshFilter == null)
                _meshFilter = gameObject.GetComponent<MeshFilter>();

            if (_meshRenderer == null)
                _meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (_meshFilter == null)
                _meshFilter = gameObject.AddComponent<MeshFilter>();

            if (_meshRenderer == null)
                _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        public void CheckDataValidate()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.tiles[i] == 0)
                    continue;

                if (_data.tiles[i] > Layer.Tileset.TilesCount)
                    SetTile(i % CHUNK_SIZE, i / CHUNK_SIZE,0);
            }
        }

        public void UpdateRenderer()
        {
            if (this == null) return;

            CheckRenderer();

            if (Layer == null) return;

            _meshRenderer.sharedMaterial = Layer.Material;

            if (materialProperty == null)
                materialProperty = new MaterialPropertyBlock();

            _meshRenderer.GetPropertyBlock(materialProperty);

            if (Layer.Tileset?.Texture == null)
                return;


            materialProperty.SetTexture("_MainTex", Layer.Tileset.Texture);
            materialProperty.SetColor("_Color", Layer.TintColor);

            _meshRenderer.SetPropertyBlock(materialProperty);

            _meshRenderer.sortingOrder = Layer.Tilemap.SortingOrder;
        }

        public void ValidateVariations()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.tiles[i] == 0) continue;

                var tile = Layer.Tileset.GetTile(_data.tiles[i]);

                _data.variations[i] = tile.ValidateVariationID(_data.variations[i]);
            }
        }

        public void GenerateVariations()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.tiles[i] == 0) continue;

                _data.variations[i] = Layer.Tileset.GetTile(_data.tiles[i]).GenVariation();
            }
        }

        private void OnDrawGizmos()
        {
            if (Layer == null) Layer = GetComponentInParent<ALayer>();

            if (Layer.Tileset == null) return;

            if (!Layer.ShowChunkBounds) return;

            Gizmos.color = Color.blue;
            var min = transform.position;
            var max = min + (Vector3)Layer.Tileset.GetTileUnit() * CHUNK_SIZE;

            Gizmos.DrawLine(min, new Vector3(min.x, max.y));
            Gizmos.DrawLine(new Vector3(min.x, max.y), max);
            Gizmos.DrawLine(max, new Vector3(max.x, min.y));
            Gizmos.DrawLine(new Vector3(max.x, min.y), min);
        }
    }
}
