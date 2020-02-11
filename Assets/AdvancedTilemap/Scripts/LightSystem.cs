using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AdvancedTilemap
{
    public class LightSystem : MonoBehaviour
    {
        public ATilemap tilemap;

        private List<ChunkPoolObject> chunks = new List<ChunkPoolObject>();

        public Material ShadowMaterial;
        public LayerMask LightingLayerMask;

        private Mesh Quad;

        private class ChunkPoolObject
        {
            public GameObject target;
            public Texture2D texture;
            public bool free;
            public bool updated = false;
            public int x;
            public int y;
        }

        private void Awake()
        {
            var triangles = new List<int>();
            var vertices = new List<Vector3>();

            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);
            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 2);
            triangles.Add(vertices.Count + 3);

            vertices.Add(new Vector3(0, 0, 0));
            vertices.Add(new Vector3(0, 1, 0));
            vertices.Add(new Vector3(1, 1, 0));
            vertices.Add(new Vector3(1, 0, 0));

            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();

            Quad = mesh;
        }

        private ChunkPoolObject GetOrCreateChunk(int x ,int y)
        {
            var gettedValue = chunks.Find(n => n.x == x && n.y == y || n.free);

            if (gettedValue == null)
            {
                gettedValue = new ChunkPoolObject();
                var go = new GameObject();
                go.transform.SetParent(transform);
             
                gettedValue.texture = new Texture2D(ATilemap.CHUNK_SIZE,ATilemap.CHUNK_SIZE);

                var meshR = go.AddComponent<MeshRenderer>();
                var meshF = go.AddComponent<MeshFilter>();

                meshF.sharedMesh = Quad;

                meshR.material = ShadowMaterial;
                meshR.material.mainTexture = gettedValue.texture;
                chunks.Add(gettedValue);
            }

            if (gettedValue.free)
            {
                gettedValue.x = x;
                gettedValue.y = y;
                gettedValue.free = false;
                gettedValue.target.name = "Light Chunk at " + x + "_" + "y";
                gettedValue.target.transform.position = 
                    new Vector3(tilemap.transform.position.x + x * ATilemap.CHUNK_SIZE, tilemap.transform.position.y + y * ATilemap.CHUNK_SIZE);
                gettedValue.target.transform.rotation = Quaternion.identity;
                gettedValue.target.transform.localScale = Vector3.one;
            }

            return gettedValue;
        }

        private void OnEnable()
        {
            tilemap.OnChunkLoaded += LoadChunk;
        }

        private void OnDisable()
        {
            
        }

        private void LateUpdate()
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                if (chunks[i].updated)
                {
                    // print("load chunk at :" + chunks[i].x + " " + chunks[i].y);

                    chunks[i].updated = false;
                }

            }
        }

        private void LoadChunk(int x, int y, int layer)
        {
            var chunk = GetOrCreateChunk(x, y);
            if (!chunk.updated)
            {
               // print("load chunk at :" + x + " " + y);
                chunk.updated = true;
            }


        }

        private void UnloadChunk(int x, int y, int layer)
        {
            var chunk = chunks.Find(n => n.x == x && n.y == y);

            if (chunk != null)
            {
                chunk.free = true;
            }
        }
    }
}
