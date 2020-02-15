using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTilemap.Lighting
{
    [ExecuteAlways]
    public class ARadialRaycastLight : ARaycastLight
    {
        public float Angle = 360;
        public float Radius = 1;

        protected override void SmoothPoints(ref List<Vector2> points)
        {
            for (int j = 0; j < SmoothIterations; j++)
            {
                for (int i = 1; i < points.Count - 1; i++)
                {
                    var point = points[i];
                    var pointNext = points[i + 1];

                    points[i] = (point + pointNext) / 2f;
                }
            }
        }

        protected override void BeginUpdateMesh()
        {
            if (MaskMaterial == null)
                MaskMaterial = MeshMaterial;

            int steps = Mathf.RoundToInt(Angle * Resolution);
            float stepSize = Angle / steps;

            List<Vector2> points = new List<Vector2>();
            List<float> distances = new List<float>();

           
            for (int i = 0; i <= steps; i++)
            {

                float angle = transform.eulerAngles.z - Angle / 2 + stepSize * i;
                var dir = AngleToDirection(angle, true);
                var hit = Physics2D.Raycast(transform.position, dir, Radius, ObstaclesMask);

                Vector2 point;
                float distance;

                if (hit)
                {
                    point = hit.point;
                    distance = hit.distance;
                }
                else
                {
                    point = (Vector2)transform.position + dir * Radius;
                    distance = Radius;
                }

                points.Add(point);
                distances.Add(distance);
                Debug.DrawLine(transform.position, point, Color.red);

            }

            SmoothPoints(ref points);

            int vertexCount = points.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount - 2) * 3];
            Vector2[] uv = new Vector2[vertexCount];

            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < vertexCount - 1; i++)
            {
                var point = transform.InverseTransformPoint(points[i]);

                vertices[i + 1] = point;

                uv[i + 1] = new Vector2(point.x / (Radius*2) + 0.5f, point.y / (Radius*2) + 0.5f);

                if (i < vertexCount - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            mesh.Clear();
            maskMesh.Clear();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            maskMesh.vertices = vertices;
            maskMesh.triangles = triangles;
            maskMesh.uv = uv;
            maskMesh.RecalculateNormals();
            mesh.RecalculateTangents();
            ApplyMesh();
            ApplyToMask();
        }




    }
    
}
