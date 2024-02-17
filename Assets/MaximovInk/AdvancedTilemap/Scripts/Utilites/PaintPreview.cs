using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class PaintPreview : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshData meshData;
        private MaterialPropertyBlock materialProperty;

        private void Awake()
        {
            var others = FindObjectsByType<PaintPreview>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var other in others)
            {
                if (other != this)
                    DestroyImmediate(other);
            }

            gameObject.hideFlags = HideFlags.HideAndDontSave;
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

        private void CheckInit()
        {
            if (meshData != null) return;

            meshData = new MeshData();

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshFilter.mesh = meshData.GetMesh();
        }

        public void GenerateBlock(int size, ATileDriverData tileDriverData)

        {
            if (tileDriverData.tileset == null) return;
            if (tileDriverData.tileset.TileDriver == null) return;

            CheckInit();
            meshData.Clear();

            int x = 0;
            int y = 0;
            int maxX = 1;
            int maxY = 1;

            if (size > 1)
            {
                int halfSize = size / 2;
                x = -halfSize;
                y = -halfSize;
                maxX = halfSize;
                maxY = halfSize;

                if (size % 2 != 0)
                {
                    maxX++; maxY++;
                }
            }

            tileDriverData.mesh = meshData;

            /*
            1 | 2 | 4
            8 | t | 16
            32| 64| 128

            */

            for (int ix = x; ix < maxX; ix++)
            {
                for (int iy = y; iy < maxY; iy++)
                {
                    var driverData = tileDriverData;

                    driverData.x = ix;
                    driverData.y = iy;

                    driverData.bitmask = 0;

                    if (ix < maxX - 1)
                        driverData.bitmask |= 16;
                    if (ix > x)
                        driverData.bitmask |= 8;


                    if (iy > y)
                        driverData.bitmask |= 64;

                    if (iy < maxY - 1)
                        driverData.bitmask |= 2;

                    if (iy < maxY - 1 && ix > x)
                        driverData.bitmask |= 1;
                    if (iy < maxY - 1 && ix < maxX - 1)
                        driverData.bitmask |= 4;

                    if (iy > y && ix > x)
                        driverData.bitmask |= 32;
                    if (iy > y && ix < maxX - 1)
                        driverData.bitmask |= 128;

                    if (ix > x && iy > y && ix < maxX - 1 && iy < maxY - 1)
                        driverData.bitmask = 255;

                    tileDriverData.tileset.TileDriver.SetTile(driverData);
                }
            }

            meshData.ApplyData();
        }
    }
}
