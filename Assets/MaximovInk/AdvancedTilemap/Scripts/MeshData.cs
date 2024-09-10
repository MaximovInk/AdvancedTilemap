using UnityEngine;
using System.Collections.Generic;

namespace MaximovInk.AdvancedTilemap
{
    public struct MeshDataParameters
    {
        public float vX0, vX1;
        public float vY0, vY1;

        public ATileUV uv;

        public Color32 color;

        public UVTransform transformData;

        public float z;

        public Vector2 unit;

        public void SetQuadData(float vX0, float vX1, float vY0, float vY1, Vector2 uvMin, Vector2 uvMax)
        {
            this.vX0 = vX0;
            this.vX1 = vX1;
            this.vY0 = vY0;
            this.vY1 = vY1;
            uv.Min = uvMin;
            uv.Max = uvMax;
        }
    }
    [System.Serializable]
    public class MeshData
    {
        [HideInInspector,SerializeField]
        private List<Vector3> vertices = new List<Vector3>();
        [HideInInspector,SerializeField]
        private List<int> triangles = new List<int>();
        [HideInInspector,SerializeField]
        private List<Vector2> uv = new List<Vector2>();
        [HideInInspector,SerializeField]
        private Mesh mesh;
        [HideInInspector, SerializeField]
        private List<Color32> colors = new List<Color32>();

        private const float GAP_FIX = 0f;

        public void AddSquare(MeshDataParameters data)
        {
            var tileUV = data.uv;
            tileUV = UVUtils.ApplyTransforms(tileUV, data.transformData);

            triangles.Add(vertices.Count + 0);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);
            triangles.Add(vertices.Count + 0);
            triangles.Add(vertices.Count + 2);
            triangles.Add(vertices.Count + 3);

            var quadUnit = data.unit;

            if (quadUnit == Vector2.zero)
                quadUnit = Vector2.one;

            vertices.Add(new Vector3(data.vX0*  quadUnit.x, data.vY0*  quadUnit.y, data.z));
            vertices.Add(new Vector3(data.vX0 * quadUnit.x, data.vY1*  quadUnit.y, data.z));
            vertices.Add(new Vector3(data.vX1 * quadUnit.x, data.vY1 * quadUnit.y, data.z));
            vertices.Add(new Vector3(data.vX1 * quadUnit.x, data.vY0 * quadUnit.y, data.z));

            var uvOffset = GAP_FIX;

            uv.Add(new Vector2(tileUV.LeftBottom.x + uvOffset,     tileUV.LeftBottom.y + uvOffset));
            uv.Add(new Vector2(tileUV.LeftTop.x + uvOffset,        tileUV.LeftTop.y - uvOffset));
            uv.Add(new Vector2(tileUV.RightTop.x - uvOffset,       tileUV.RightTop.y - uvOffset));
            uv.Add(new Vector2(tileUV.RightBottom.x - uvOffset,    tileUV.RightBottom.y + uvOffset));

            colors.Add(data.color);
            colors.Add(data.color);
            colors.Add(data.color);
            colors.Add(data.color);
        }

        public Mesh GetMesh()
        {
            if(mesh == null)
            {
                mesh = new Mesh();
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.uv = uv.ToArray();
            }

            return mesh;
        }

        public void ApplyData()
        {
            if (mesh == null)
                mesh = new Mesh();

            mesh.Clear();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uv.ToArray();
            mesh.colors32 = colors.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        public void Clear()
        {
            triangles.Clear();
            vertices.Clear();
            uv.Clear();
            colors.Clear();
        }
    }
}
