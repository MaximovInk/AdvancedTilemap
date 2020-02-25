using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using AdvancedTilemap.Liquid;

namespace AdvancedTilemap
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        public Layer Layer;

        public int GridPosX;

        public int GridPosY;

        [HideInInspector, SerializeField]
        private MeshData meshData;

        [HideInInspector, SerializeField]
        private MeshFilter meshFilter;

        [HideInInspector, SerializeField]
        private MeshRenderer meshRenderer;

        [HideInInspector, SerializeField]
        private byte[] tiles;
        [HideInInspector, SerializeField]
        private byte[] bitmasks;
        [HideInInspector, SerializeField]
        private byte[] variations;
        [HideInInspector, SerializeField]
        private Color32[] colors;

        private bool isDirty = false;
        private bool meshRebuild = false;
        private bool colliderRebuild = false;
        private bool lightUpdate = false;
        private bool lightRebuild = false;
        private bool fluidRebuild = false;

        private MaterialPropertyBlock prop;

        private PolygonCollider2D polygonCollider2D;
        [HideInInspector, SerializeField]
        private LiquidChunk liquidChunk;

        #region public_methods

        public bool IsVisible()
        {
            return meshRenderer.isVisible;
        }

        public bool IsEmpty()
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public void UpdateLiquid()
        {
            if (Layer.LiquidEnabled)
            {
                liquidChunk = GetComponentInChildren<LiquidChunk>();

                if (liquidChunk == null)
                {
                    //create new liquid chunk
                    var go = new GameObject();
                    go.transform.SetParent(transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localScale = Vector3.one;
                    go.transform.localRotation = Quaternion.identity;

                    liquidChunk = go.AddComponent<LiquidChunk>();
                    liquidChunk.Init(ATilemap.CHUNK_SIZE, ATilemap.CHUNK_SIZE);
                }
                liquidChunk.SetMaterial(Layer.LiquidMaterial);
            }
            else
            {
                if (liquidChunk != null)
                {
                    DestroyImmediate(liquidChunk.gameObject);
                }
            }
        }

        public void UpdateColliderProperties()
        {
            if (polygonCollider2D == null)
                return;

            polygonCollider2D.sharedMaterial = Layer.PhysicsMaterial2D;
            polygonCollider2D.isTrigger = Layer.IsTrigger;
        }

        public void UpdateFlags()
        {
            if (Layer.Tilemap.DisplayChunksInHierarchy)
            {
                gameObject.hideFlags &= ~HideFlags.HideInHierarchy;
            }
            else
            {
                gameObject.hideFlags |= HideFlags.HideInHierarchy;
            }
            tag = Layer.Tag;
            gameObject.layer = Layer.LayerMask;
        }

        public void RefreshAll(bool immediate = false)
        {
            colliderRebuild = true;
            meshRebuild = true;

            if (immediate)
                UpdateMesh();
        }

        public void UpdateRenderer(bool material = false, bool color = false, bool texture = false)
        {
            if (material)
                meshRenderer.sharedMaterial = Layer.Material;

            if (prop == null)
                prop = new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(prop);

            if (color)
                prop.SetColor("_Color", Layer.TintColor);

            if (texture && Layer.Tileset && Layer.Tileset.Texture != null)
                prop.SetTexture("_MainTex", Layer.Tileset.Texture);

            meshRenderer.SetPropertyBlock(prop);

            meshRenderer.sortingOrder = Layer.Tilemap.SortingOrder;
        }

        public void Init(int x, int y, Layer layer)
        {
            GridPosX = x;
            GridPosY = y;

            Layer = layer;
            meshData = new MeshData();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter.sharedMesh = meshData.GetMesh();
            gameObject.name = "Chunk " + GridPosX + " " + GridPosY;

            OnValidate();
            UpdateColliderComponent();
            UpdateFlags();
            UpdateRenderer();
            UpdateLiquid();
        }

        public void UpdateColliderComponent()
        {
            if (gameObject == null)
                return;

            if (polygonCollider2D != null && !Layer.ColliderEnabled)
                DestroyImmediate(polygonCollider2D);
            if (polygonCollider2D == null && Layer.ColliderEnabled)
            {
                polygonCollider2D = gameObject.GetComponent<PolygonCollider2D>();
                if(polygonCollider2D == null)
                    polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
                colliderRebuild = true;
            }
        }

        public void UpdateMesh()
        {
            if (meshRebuild)
                GenerateMesh();
            if (colliderRebuild || meshRebuild)
                GenerateCollider();
            meshData.ApplyToMesh();
        }

        public byte GetSettleCount(int x, int y)
        {
            return liquidChunk.GetSettleCount(x,y);
        }

        public void SetSettleCount(int x, int y,byte value)
        {
            liquidChunk.SetSettleCount(x, y,value);
        }

        public bool GetSettled(int x ,int y)
        {
            return liquidChunk.GetSettled(x,y);
        }

        public void SetSettled(int x, int y,bool value)
        {
            liquidChunk.SetSettled(x,y,value);
        }

        public float GetLiquid(int gx, int gy)
        {
            return liquidChunk.GetLiquid(gx,gy);
        }
        public void SetLiquid(int gx, int gy,float value)
        {
            liquidChunk.SetLiquid(gx, gy,value);
        }
        public void AddLiquid(int gx, int gy,float value)
        {
            liquidChunk.AddLiquid(gx, gy,value);
        }

        public byte GetBitmask(int gx, int gy)
        {
            return bitmasks[gx + gy * ATilemap.CHUNK_SIZE];
        }

        public void SetBitmask(int gx, int gy, byte bitmask)
        {
            bitmasks[gx + gy * ATilemap.CHUNK_SIZE] = bitmask;
            meshRebuild = true;
        }

        public void SetVariation(int gx, int gy, byte variation)
        {
            variations[gx + gy * ATilemap.CHUNK_SIZE] = variation;
            meshRebuild = true;
        }

        public byte GetVariation(int gx, int gy)
        {
            return variations[gx + gy * ATilemap.CHUNK_SIZE];
        }

        public byte GetTile(int gx, int gy)
        {
            return tiles[gx + gy * ATilemap.CHUNK_SIZE];
        }

        public void SetTile(int gx, int gy, byte tileIdx = 1)
        {
            var index = gx + gy * ATilemap.CHUNK_SIZE;

            if (tiles[index] == tileIdx)
            {
                isDirty = true;
                return;
            }

            if (tiles[index] != 0)
            {
                tiles[index] = tileIdx;
                meshRebuild = true;
                isDirty = true;
                return;
            }

            colliderRebuild = true;
            isDirty = true;

            tiles[index] = tileIdx;
            var tile = Layer.Tileset.GetTile(tileIdx);
            AddBlock(gx, gy, tile, colors[gx + gy * ATilemap.CHUNK_SIZE]);
            lightUpdate = true;
        }

        public void Erase(int gx, int gy)
        {
            isDirty = true;

            meshRebuild = true;

            colliderRebuild = true;

            tiles[gx + gy * ATilemap.CHUNK_SIZE] = 0;
            lightUpdate = true;
        }

        public void SetColor(int gx, int gy, Color32 color)
        {
            colors[gx + gy * ATilemap.CHUNK_SIZE] = color;
            meshRebuild = true;
        }

        public Color32 GetColor(int gx, int gy)
        {
            return colors[gx + gy * ATilemap.CHUNK_SIZE];
        }

        public void Load()
        {
            //gameObject.SetActive(true);
            meshRenderer.enabled = true;
            if (Layer.LiquidEnabled)
                liquidChunk.gameObject.SetActive(true);
        }

        public void Unload()
        {
            //gameObject.SetActive(false);
            meshRenderer.enabled = false;
            if (Layer.LiquidEnabled)
                liquidChunk.gameObject.SetActive(false);
        }

        #endregion

        #region mesh generation

        private void GenerateMesh()
        {

            meshRebuild = false;
            meshData.Clear();

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == 0)
                    continue;

                var gx = i % ATilemap.CHUNK_SIZE;
                var gy = i / ATilemap.CHUNK_SIZE;

                var tile = Layer.Tileset.GetTile(tiles[i]);

                AddBlock(gx, gy, tile, GetColor(gx, gy));
            }
        }

        private void AddBlock(int gx, int gy, Tile tile, Color32 color)
        {
            switch (tile.Type)
            {
                /*case BlockType.Single:
                    AddSingleBlock(gx, gy, tile);
                    break;*/
                case BlockType.Overlap:
                    AddOverlapBlock(gx, gy, tile, tile.BlendOverlap, color);
                    break;
                /* case BlockType.Multi:
                     break;
                 case BlockType.Slope:
                     break;*/
                default:
                    AddSingleBlock(gx, gy, tile, color);
                    break;
            }
        }

        private void AddSingleBlock(int gx, int gy, Tile tile, Color32 color)
        {
            meshData.AddSquare(tile.GetTexPos(), Layer.Tileset.TileTexUnit, gx, gy, gx + 1, gy + 1, tile.OverlapDepth * Layer.Tilemap.ZBlockOffset + Layer.ZOrder, 0, 0, 1, 1, color);
        }

        public Bounds GetBounds()
        {
            Bounds bounds = meshFilter.sharedMesh ? meshFilter.sharedMesh.bounds : default;
            if (bounds == default)
            {
                Vector3 vMinMax = Vector2.Scale(new Vector2(GridPosX < 0 ? ATilemap.CHUNK_SIZE : 0f, GridPosY < 0 ? ATilemap.CHUNK_SIZE : 0f), Vector3.one);
                bounds.SetMinMax(vMinMax, vMinMax);
            }
            for (int i = 0; i < tiles.Length; ++i)
            {
                if (tiles[i] == 0)
                    continue;

                int gx = i % ATilemap.CHUNK_SIZE;
                if (GridPosX >= 0) gx++;
                int gy = i / ATilemap.CHUNK_SIZE;
                if (GridPosY >= 0) gy++;
                Vector2 gridPos = Vector2.Scale(new Vector2(gx, gy), Vector3.one);
                bounds.Encapsulate(gridPos);
            }
            return bounds;
        }

        /* bitmask map:
        * 
        1  | 2  |  4
        8  |tile|  16
        32 | 64 |  128
            */
        private void AddOverlapBlock(int posX, int posY, Tile tile, bool blend, Color32 color)
        {
            var cellMax = posX + 1f;
            var cellMin = posY + 1f;
            var z = tile.OverlapDepth * Layer.Tilemap.ZBlockOffset + Layer.ZOrder;

            var texPos = tile.GetTexPos(GetVariation(posX, posY));

            meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX, posY, cellMax, cellMin, z, 0.5f, 0.5f, 1.5f, 1.5f, color);

            var bitmask = GetBitmask(posX, posY);

            //Left top corner
            if (!HasBit(bitmask, 1) && !HasBit(bitmask, 8) && !HasBit(bitmask, 2))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX - 0.5f, cellMin, posX, cellMin + 0.5f, z, 0f, 1.5f, 0.5f, 2f, color);
            }
            //Right top corner
            if (!HasBit(bitmask, 4) && !HasBit(bitmask, 16) && !HasBit(bitmask, 2))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax, cellMin, cellMax + 0.5f, cellMin + 0.5f, z, 1.5f, 1.5f, 2f, 2f, color);
            }
            //Left bottom corner
            if (!HasBit(bitmask, 32) && !HasBit(bitmask, 8) && !HasBit(bitmask, 64))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX - 0.5f, posY - 0.5f, posX, posY, z, 0f, 0f, 0.5f, 0.5f, color);
            }
            //Right bottom corner
            if (!HasBit(bitmask, 128) && !HasBit(bitmask, 16) && !HasBit(bitmask, 64))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax, posY - 0.5f, cellMax + 0.5f, posY, z, 1.5f, 0f, 2f, 0.5f, color);
            }

            if (blend)
            {
                //Left side
                if (!HasBit(bitmask, 8))
                {
                    //Left Top exists
                    if (HasBit(bitmask, 1))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX - 0.5f, posY + 0.5f, posX, cellMin, z, 0.5f, 2.5f, 1f, 3f, color);
                    }
                    //Left Top empty
                    else
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX - 0.5f, posY + 0.5f, posX, cellMin, z, 0f, 1f, 0.5f, 1.5f, color);
                    }
                    //Left Bottom exists
                    if (HasBit(bitmask, 32))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX - 0.5f, posY, posX, posY + 0.5f, z, 0.5f, 2f, 1f, 2.5f, color);
                    }
                    //Left Bottom empty
                    else
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX - 0.5f, posY, posX, posY + 0.5f, z, 0f, 0.5f, 0.5f, 1f, color);
                    }
                }
                //Bottom side
                if (!HasBit(bitmask, 64))
                {
                    //Left Bottom exists
                    if (HasBit(bitmask, 32))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX, posY - 0.5f, posX + 0.5f, posY, z, 0f, 2.5f, 0.5f, 3f, color);
                    }
                    //Left Bottom empty
                    else
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX, posY - 0.5f, posX + 0.5f, posY, z, 0.5f, 0f, 1f, 0.5f, color);
                    }
                    //Right Bottom empty
                    if (!HasBit(bitmask, 128))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax - 0.5f, posY - 0.5f, cellMax, posY, z, 1f, 0f, 1.5f, 0.5f, color);
                    }
                }
                //Right side
                if (!HasBit(bitmask, 16))
                {
                    //Right Bottom exists
                    if (HasBit(bitmask, 128))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax, posY, cellMax + 0.5f, posY + 0.5f, z, 0f, 2f, 0.5f, 2.5f, color);
                    }
                    //Right Bottom empty
                    else
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax, posY, cellMax + 0.5f, posY + 0.5f, z, 1.5f, 0.5f, 2f, 1f, color);
                    }

                    //Right Top empty
                    if (!HasBit(bitmask, 4))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax, posY + 0.5f, cellMax + 0.5f, cellMin, z, 1.5f, 1f, 2f, 1.5f, color);
                    }
                }
                //Top side
                if (!HasBit(bitmask, 2))
                {
                    //Right Top empty
                    if (!HasBit(bitmask, 4))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax - 0.5f, cellMin, cellMax, cellMin + 0.5f, z, 1f, 1.5f, 1.5f, 2f, color);
                    }
                    //Left Top Empty
                    if (!HasBit(bitmask, 1))
                    {
                        meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX, cellMin, posX + 0.5f, cellMin + 0.5f, z, 0.5f, 1.5f, 1f, 2f, color);
                    }

                }

                return;
            }
            //Left
            if (!HasBit(bitmask, 8))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX - 0.5f, posY, posX, cellMin, z, 0f, 0.5f, 0.5f, 1.5f, color);
            }
            //Bottom
            if (!HasBit(bitmask, 64))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX, posY - 0.5f, cellMax, posY, z, 0.5f, 0f, 1.5f, 0.5f, color);
            }
            //Right
            if (!HasBit(bitmask, 16))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, cellMax, posY, cellMax + 0.5f, cellMin, z, 1.5f, 0.5f, 2f, 1.5f, color);
            }
            //Top
            if (!HasBit(bitmask, 2))
            {
                meshData.AddSquare(texPos, Layer.Tileset.TileTexUnit, posX, cellMin, cellMax, cellMin + 0.5f, z, 0.5f, 1.5f, 1.5f, 2f, color);
            }
        }

        private bool HasBit(byte bitmask, byte position)
        {
            return (bitmask & position) == position;
        }

        /// TODO:
        private void AddSlopedBlock()
        {

        }
        /// TODO:
        private void AddMultiblock()
        {

        }

        private void OnDrawGizmos()
        {
            Handles.color = new Color(1,0,0,0.25f);
            Handles.DrawLine(transform.position, new Vector3(transform.position.x + ATilemap.CHUNK_SIZE, transform.position.y ));
            Handles.DrawLine(new Vector3(transform.position.x + ATilemap.CHUNK_SIZE, transform.position.y), new Vector3(transform.position.x + ATilemap.CHUNK_SIZE, transform.position.y + ATilemap.CHUNK_SIZE));
            Handles.DrawLine(transform.position, new Vector3(transform.position.x , transform.position.y + ATilemap.CHUNK_SIZE));
            Handles.DrawLine(new Vector3(transform.position.x, transform.position.y + ATilemap.CHUNK_SIZE), new Vector3(transform.position.x + ATilemap.CHUNK_SIZE, transform.position.y + ATilemap.CHUNK_SIZE));
        }

        #endregion

        #region other
        private void OnValidate()
        {
            if (tiles == null || tiles.Length == 0)
                tiles = new byte[ATilemap.CHUNK_SIZE * ATilemap.CHUNK_SIZE];
            if (bitmasks == null || bitmasks.Length == 0)
                bitmasks = new byte[ATilemap.CHUNK_SIZE * ATilemap.CHUNK_SIZE];
            if (variations == null || variations.Length == 0)
                variations = new byte[ATilemap.CHUNK_SIZE * ATilemap.CHUNK_SIZE];
            if (colors == null || colors.Length == 0)
            {
                colors = new Color32[ATilemap.CHUNK_SIZE * ATilemap.CHUNK_SIZE];
                for (int i = 0; i < colors.Length; i++)
                {
                    colors[i] = Color.white;
                }
                meshRebuild = true;
            }
            if(meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
            if(meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();

            /* if (lightChunk == null)
                 lightChunk = GetComponentInChildren<LightChunk>();*/



            if (!Application.isPlaying && !meshRenderer.enabled)
            {
                meshRenderer.enabled = true;
            }
        }

        private void Start()
        {
            OnValidate();
            UpdateColliderComponent();
        }

        private void Update()
        {
            if (!meshRenderer.enabled)
                return;

            if (isDirty || meshRebuild || colliderRebuild || lightUpdate || lightRebuild)
            {
                UpdateMesh();
            }

            if (liquidChunk != null && liquidChunk.genMesh)
                liquidChunk.ApplyData();
        }

        #endregion

        #region collider generation

        private void GenerateCollider()
        {
            colliderRebuild = false;


            if (polygonCollider2D == null)
                return;
            polygonCollider2D.pathCount = 0;
            List<ColliderSegment> segments = GetSegments();
            List<List<Vector2>> paths = FindPaths(segments);

            polygonCollider2D.pathCount = paths.Count;

            for (int p = 0; p < paths.Count; p++)
            {
                polygonCollider2D.SetPath(p, paths[p].ToArray());
            }
        }

        List<List<Vector2>> FindPaths(List<ColliderSegment> segments)
        {
            List<List<Vector2>> output = new List<List<Vector2>>();

            while (segments.Count > 0)
            {
                Vector2 currentpoint = segments[0].Point2;
                List<Vector2> currentpath = new List<Vector2> { segments[0].Point1, segments[0].Point2 };
                segments.Remove(segments[0]);

                bool currentpathcomplete = false;
                while (currentpathcomplete == false)
                {
                    currentpathcomplete = true;
                    for (int s = 0; s < segments.Count; s++)
                    {
                        if (segments[s].Point1 == currentpoint)
                        {
                            currentpathcomplete = false;
                            currentpath.Add(segments[s].Point2);
                            currentpoint = segments[s].Point2;
                            segments.Remove(segments[s]);
                        }
                        else if (segments[s].Point2 == currentpoint)
                        {
                            currentpathcomplete = false;
                            currentpath.Add(segments[s].Point1);
                            currentpoint = segments[s].Point1;
                            segments.Remove(segments[s]);
                        }
                    }
                }
                output.Add(currentpath);
            }
            return output;
        }

        private List<ColliderSegment> GetSegments()
        {
            List<ColliderSegment> segments = new List<ColliderSegment>();

            for (int i = 0; i < tiles.Length; i++)
            {
                if (tiles[i] == 0)
                    continue;

                int x = i % ATilemap.CHUNK_SIZE;
                int y = i / ATilemap.CHUNK_SIZE;

                //Top is empty
                if (y + 1 >= ATilemap.CHUNK_SIZE || tiles[x + (y + 1) * ATilemap.CHUNK_SIZE] == 0)
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y + 1), new Vector2(x + 1, y + 1)));
                }
                //Bottom is empty
                if (y - 1 < 0 || tiles[x + (y - 1) * ATilemap.CHUNK_SIZE] == 0)
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x + 1, y)));
                }
                //Right is empty
                if (x + 1 >= ATilemap.CHUNK_SIZE || tiles[x + 1 + y * ATilemap.CHUNK_SIZE] == 0)
                {
                    segments.Add(new ColliderSegment(new Vector2(x + 1, y), new Vector2(x + 1, y + 1)));
                }
                //Left is empty
                if (x - 1 < 0 || tiles[x - 1 + y * ATilemap.CHUNK_SIZE] == 0)
                {
                    segments.Add(new ColliderSegment(new Vector2(x, y), new Vector2(x, y + 1)));
                }
            }

            return segments;
        }

        private struct ColliderSegment
        {
            public Vector2 Point1;
            public Vector2 Point2;

            public ColliderSegment(Vector2 Point1, Vector2 Point2)
            {
                this.Point1 = Point1;
                this.Point2 = Point2;
            }
        }

        #endregion

    }
}