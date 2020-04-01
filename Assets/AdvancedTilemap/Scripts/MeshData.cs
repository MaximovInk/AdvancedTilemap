using System;
using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTilemap
{
    [Serializable]
    public class MeshData
    {
        [HideInInspector, SerializeField]
        private List<Vector3> vertices = new List<Vector3>();
        [HideInInspector, SerializeField]
        private List<int> triangles = new List<int>();
        [HideInInspector, SerializeField]
        private List<Vector2> uv = new List<Vector2>();
        [HideInInspector,SerializeField]
        private List<Color32> colors = new List<Color32>();

        private Mesh Mesh { get { if (mesh == null) mesh = new Mesh(); return mesh; } }
        [HideInInspector, SerializeField]
        private Mesh mesh;

        //Pixel perfect offset
        private const float PP_OFFSET = 0.001f;

        public void AddSquare(Vector2 texturePos, Vector2 tileUnit, float vX0, float vY0, float vX1, float vY1, float z, float uvMinX,float uvMinY, float uvMaxX, float uvMaxY, Color32 color)
        {
            lock (mesh)
            {
                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);
                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 2);
                triangles.Add(vertices.Count + 3);
                vertices.Add(new Vector3(vX0, vY0, z));
                vertices.Add(new Vector3(vX0, vY1, z));
                vertices.Add(new Vector3(vX1, vY1, z));
                vertices.Add(new Vector3(vX1, vY0, z));
                uv.Add(new Vector2(tileUnit.x * texturePos.x + tileUnit.x * uvMinX + PP_OFFSET, tileUnit.y * texturePos.y + tileUnit.y * uvMinY + PP_OFFSET));
                uv.Add(new Vector2(tileUnit.x * texturePos.x + tileUnit.x * uvMinX + PP_OFFSET, tileUnit.y * texturePos.y + tileUnit.y * uvMaxY - PP_OFFSET));
                uv.Add(new Vector2(tileUnit.x * texturePos.x + tileUnit.x * uvMaxX - PP_OFFSET, tileUnit.y * texturePos.y + tileUnit.y * uvMaxY - PP_OFFSET));
                uv.Add(new Vector2(tileUnit.x * texturePos.x + tileUnit.x * uvMaxX - PP_OFFSET, tileUnit.y * texturePos.y + tileUnit.y * uvMinY + PP_OFFSET));
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
                colors.Add(color);
            }

        }

        public void Clear()
        {
            triangles.Clear();
            vertices.Clear();
            uv.Clear();
            colors.Clear();
        }

        public void ApplyToMesh()
        {
            lock (mesh)
            {
                Mesh.Clear();
                Mesh.vertices = vertices.ToArray();
                Mesh.triangles = triangles.ToArray();
                Mesh.uv = uv.ToArray();
                Mesh.colors32 = colors.ToArray();
                Mesh.RecalculateNormals();
                Mesh.RecalculateTangents();
            }
            
        }

        public Mesh GetMesh()
        {
            return Mesh;
        }
    }
}
