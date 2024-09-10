using System;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public partial class AChunk
    {

        [HideInInspector, SerializeField]
        private MeshData _meshData;

        public MeshData GetMeshData() => _meshData;

        [SerializeField, HideInInspector] private MeshFilter meshFilter;
        [SerializeField, HideInInspector] private MeshRenderer meshRenderer;

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

            if (meshFilter == null)
                meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();

            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        public void CheckDataValidate()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.data[i] == 0)
                    continue;

                if (_data.data[i] > Layer.Tileset.TilesCount)
                    SetTile(i % CHUNK_SIZE, i / CHUNK_SIZE,0);
            }
        }

        public void UpdateRenderer()
        {
            if (this == null) return;

            CheckRenderer();

            if (Layer == null) return;

            meshRenderer.sharedMaterial = Layer.Material;

            if (materialProperty == null)
                materialProperty = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(materialProperty);

            if (Layer.Tileset?.Texture == null)
                return;


            materialProperty.SetTexture("_MainTex", Layer.Tileset.Texture);
            materialProperty.SetColor("_Color", Layer.TintColor);

            meshRenderer.SetPropertyBlock(materialProperty);

            meshRenderer.sortingOrder = Layer.Tilemap.SortingOrder;
        }

        public void ValidateVariations()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.data[i] == 0) continue;

                var tile = Layer.Tileset.GetTile(_data.data[i]);

                _data.variations[i] = tile.ValidateVariationID(_data.variations[i]);
            }
        }

        public void GenerateVariations()
        {
            for (int i = 0; i < _data.ArraySize; i++)
            {
                if (_data.data[i] == 0) continue;

                _data.variations[i] = Layer.Tileset.GetTile(_data.data[i]).GenVariation();
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
