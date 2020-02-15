using AdvancedTilemap.Lighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedTilemap.Lighting
{
	public class ALineRaycastLight : ARaycastLight
	{
        public float Size;
        public float Offset;
        public float Angle;
        public float LineDistance;


		protected override void BeginUpdateMesh()
        {
            var steps = Mathf.RoundToInt(Size * Resolution);
            var stepSize = (Size/steps);

            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i <= steps; i++)
            {

                float angle = transform.eulerAngles.z - Angle;
                var dir = AngleToDirection(angle, true);
                var hit = Physics2D.Raycast(transform.position+new Vector3(i*stepSize, 0,0), dir, LineDistance, ObstaclesMask);
                Vector2 point;
                if (hit)
                {
                    point = transform.InverseTransformPoint (hit.point);
                }
                else
                {
                    point = dir * LineDistance + new Vector2(i*stepSize, 0);
                }
                

                Debug.DrawLine((Vector2)transform.position+new Vector2(i*stepSize, 0), (Vector2)transform.position+ point, Color.red);

                points.Add(new Vector2(i*stepSize, 0));
                points.Add(point);

                hit = Physics2D.Raycast(transform.position + new Vector3(i * stepSize + stepSize, 0, 0), dir, LineDistance, ObstaclesMask);

                if (hit)
                {
                    point = transform.InverseTransformPoint(hit.point);
                }
                else
                {
                    point = dir * LineDistance + new Vector2(i * stepSize + stepSize, 0);
                }


                Debug.DrawLine((Vector2)transform.position + new Vector2(i * stepSize + stepSize, 0), (Vector2)transform.position + point, Color.red);

                points.Add(new Vector2(i * stepSize + stepSize, 0));
                points.Add(point);
            }

            int vertexCount = points.Count * 4;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[points.Count * 6];
            Vector2[] uv = new Vector2[vertexCount];


            for (int vert = 0,trig = 0; vert < points.Count-5 && vert < vertexCount-4 && trig < triangles.Length- 3; vert+=4,trig+=6)
            {
                vertices[vert + 0] = points[vert + 0];
                vertices[vert + 1] = points[vert + 1];
                vertices[vert + 2] = points[vert + 2];
                vertices[vert + 3] = points[vert + 3];

                //uv[i] = new Vector2(point.x / (Radius * 2) + 0.5f, point.y / (Radius * 2) + 0.5f);

                triangles[trig + 0] = vert + 0;
                triangles[trig + 1] = vert + 2;
                triangles[trig + 2] = vert + 1;
                triangles[trig + 3] = vert + 2;
                triangles[trig + 4] = vert + 3;
                triangles[trig + 5] = vert + 1;
            }

            /*for (int i = 0; i < vertexCount - 1; i++)
            {
                var point = transform.InverseTransformPoint(points[i]);

                vertices[i] = point;

                uv[i] = new Vector2(point.x / (Radius * 2) + 0.5f, point.y / (Radius * 2) + 0.5f);

                triangles[i * 6 + 0] = i + 0;
                triangles[i * 6 + 1] = i + 2;
                triangles[i * 6 + 2] = i + 1;
                triangles[i * 6 + 0] = i + 2;
                triangles[i * 6 + 1] = i + 3;
                triangles[i * 6 + 2] = i + 1;
            }*/

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

            base.BeginUpdateMesh();
		}

		protected override void EndUpdateMesh()
		{
			base.EndUpdateMesh();
		}
	}
}
