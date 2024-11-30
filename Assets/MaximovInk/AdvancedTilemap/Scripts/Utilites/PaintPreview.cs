﻿using System.Drawing;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    
    public class PaintPreview : MonoBehaviour, ITilemap
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshData meshData;
        private MaterialPropertyBlock materialProperty;

        public void Validate()
        {
           // gameObject.hideFlags = HideFlags.HideAndDontSave;

            var paints = FindObjectsByType<PaintPreview>( FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int i = 0; i < paints.Length; i++)
            {
                if (paints[i] == this) continue;

                DestroyImmediate(paints[i].gameObject);
            }
        }

        public void SetMaterial(Material material, Texture2D texture)
        {
            if (texture == null || material == null) return;

            CheckInit();

            meshRenderer.sharedMaterial = material;
            if (materialProperty == null)
                materialProperty = new MaterialPropertyBlock();
            meshRenderer.GetPropertyBlock(materialProperty);
            materialProperty.SetTexture("_MainTex", texture);
            meshRenderer.SetPropertyBlock(materialProperty);
        }

        public void SetPosition(Vector3 position)
        {
            var tileUnitHalf = tileUnit / 2f;

            var offset = new Vector3(
                _maxX * tileUnitHalf.x,
                _maxY * tileUnitHalf.y,
                0
                );

            if (_maxX % 2 != 0) offset.x -= tileUnitHalf.x;
            if (_maxY % 2 != 0) offset.y -= tileUnitHalf.y;

            transform.position = position - offset;
        }

        private void CheckInit()
        {
            if (meshData != null) return;

            meshData = new MeshData();

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = meshData.GetMesh();
        }

        private int _maxX = 1;
        private int _maxY = 1;

        private Vector2 tileUnit = Vector2.one;


        public void Clear()
        {
            CheckInit();

            meshData.Clear();
        }

        private ATileDriverData _driverData;


        public void SetDriverData(ATileDriverData data)
        {
            _driverData = data;
        }

        public void SetTile(int x, int y, ushort tileID, UVTransform data = default)
        {
            if (_driverData.tileset == null) return;
            if (_driverData.tile.TileDriver == null) return;

            tileUnit = _driverData.tileset.GetTileUnit();

            _driverData.mesh = meshData;

            _driverData.variation = 0;
            _driverData.x = x;
            _driverData.y = y;

            _driverData.tile.TileDriver.SetTile(_driverData);
        }

        public void SetColor(int x, int y, Color32 color)
        {
            _driverData.color = color;
        }

        public void Apply()
        {
            meshData.ApplyData();
        }

        public void GenerateBlock(int size, ATileDriverData tileDriverData)

        {
            if (tileDriverData.tileset == null) return;
            if (tileDriverData.tile.TileDriver == null) return;

            CheckInit();
            meshData.Clear();

            int x = 0;
            int y = 0;

            tileUnit = tileDriverData.tileset.GetTileUnit();

            _maxX = size;
            _maxY = size;

            tileDriverData.mesh = meshData;

            for (int ix = x; ix < _maxX; ix++)
            {
                for (int iy = y; iy < _maxY; iy++)
                {
                    var driverData = tileDriverData;

                    driverData.x = ix;
                    driverData.y = iy;

                    driverData.bitmask = 0;

                    if (ix < _maxX - 1)
                        driverData.bitmask |= 16;
                    if (ix > x)
                        driverData.bitmask |= 8;
                    if (iy > y)
                        driverData.bitmask |= 64;
                    if (iy < _maxY - 1)
                        driverData.bitmask |= 2;

                    if (iy < _maxY - 1 && ix > x)
                        driverData.bitmask |= 1;
                    if (iy < _maxY - 1 && ix < _maxX - 1)
                        driverData.bitmask |= 4;

                    if (iy > y && ix > x)
                        driverData.bitmask |= 32;
                    if (iy > y && ix < _maxX - 1)
                        driverData.bitmask |= 128;

                    if (ix > x && iy > y && ix < _maxX - 1 && iy < _maxY - 1)
                        driverData.bitmask = 255;

                    driverData.variation = 0;

                    tileDriverData.tile.TileDriver.SetTile(driverData);
                }
            }

            meshData.ApplyData();
        }
    }
}
