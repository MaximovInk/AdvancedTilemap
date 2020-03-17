using System.Collections.Generic;
using UnityEngine;

namespace AdvancedTilemap.Lighting
{
	public class ALineRaycastLight : ARaycastLight
	{
        public float Size;
        public float Angle;
        public float LineDistance;

		protected override void CalculatePoints()
        {
            var steps = Mathf.RoundToInt(Size * Resolution);
            var stepSize = (Size/steps);

            points = new List<Vector2>();

            float angle = transform.eulerAngles.z - Angle;
            var dir = AngleToDirection(angle, true);

            var hit = Physics2D.Raycast(transform.position, dir, LineDistance, ObstaclesMask);
            Vector2 point;
            if (hit)
            {
                point = transform.InverseTransformPoint(hit.point);
                point = OffsetDirection(Vector2.zero, point, hit.distance, LineDistance);
            }
            else
            {
                point = dir * LineDistance;
            }
            points.Add(Vector2.zero);
            points.Add(point);


            for (int i = 1; i <= steps; i++)
            {
               
                hit = Physics2D.Raycast(transform.position + new Vector3(i * stepSize + stepSize, 0, 0), dir, LineDistance, ObstaclesMask);

                if (hit)
                {
                    point = transform.InverseTransformPoint(hit.point);
                    point = OffsetDirection(new Vector3(i*stepSize,0), point, hit.distance, LineDistance); 
                }
                else
                {
                    point = dir * LineDistance + new Vector2(i * stepSize + stepSize, 0);
                }

                points.Add(new Vector2(i * stepSize + stepSize, 0));
                points.Add(point);

                points.Add(new Vector2(i * stepSize + stepSize, 0));
                points.Add(point);
            }

            
		}

		protected override void GenerateMesh()
		{
            int vertexCount = points.Count * 4;
            vertices = new Vector3[vertexCount];
            triangles = new int[points.Count * 6];
            uv = new Vector2[vertexCount];


            for (int vert = 0, trig = 0; vert < points.Count - 5 && vert < vertexCount - 4 && trig < triangles.Length - 3; vert += 4, trig += 6)
            {
                vertices[vert + 0] = points[vert + 0];
                vertices[vert + 1] = points[vert + 1];
                vertices[vert + 2] = points[vert + 2];
                vertices[vert + 3] = points[vert + 3];

                triangles[trig + 0] = vert + 0;
                triangles[trig + 1] = vert + 2;
                triangles[trig + 2] = vert + 1;
                triangles[trig + 3] = vert + 2;
                triangles[trig + 4] = vert + 3;
                triangles[trig + 5] = vert + 1;
            }

            //ApplyData();
        }
	}
}
