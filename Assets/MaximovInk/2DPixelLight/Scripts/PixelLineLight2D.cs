using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.PixelLight
{
    public class PixelLineLight2D : PixelRaycastLight2D
    {
        public float Width;
        public float Angle;
        public float RayDistance;

        [SerializeField]
        public int _smoothIterations = 1;

        public int StepCount => Mathf.RoundToInt(Width * _resolution * 10);

        protected void SmoothPoints(ref List<Vector2> points)
        {
            for (var j = 0; j < _smoothIterations; j++)
            {
                for (var i = 0; i < points.Count - 2; i+=2)
                {
                    var point = points[i+1];
                    var pointNext = points[i + 3];

                    points[i+1] = (point + pointNext) / 2f;
                }
            }
        }

        protected override void CalculatePoints()
        {
            var steps = StepCount;
            var stepSize = (Width / steps);

            points = new List<Vector2>();

            var angle = Angle;
            var dir = Utility.AngleToDirection(angle);

            var raycasts = new RaycastHit2D[steps];

            var pos = Vector3.zero;

            var offset = -Width / 2f;

            offset += -Mathf.Abs(Mathf.Sin(transform.position.x) * stepSize) ;

            var posOffsetV = new Vector3(offset, 0, 0);

            for (int i = 0; i < steps; i++)
            {
                var pointOrigin = pos + new Vector3(i * stepSize, 0) + posOffsetV;

                raycasts[i] = Physics2D.Raycast(transform.position + pointOrigin, dir, RayDistance, _obstaclesMask);
            }

            for (int i = 0; i < steps - 1; i++)
            {
                var point1Origin = pos + new Vector3(i * stepSize, 0) +posOffsetV;

                points.Add(point1Origin);

                if (raycasts[i])
                {
                    points.Add(
                        Utility.OffsetDirection(
                            point1Origin, 
                            transform.InverseTransformPoint(raycasts[i].point),
                            _offsetRay, 
                            raycasts[i].distance, 
                            RayDistance));

                }
                else
                {
                    points.Add(point1Origin + (Vector3)dir * RayDistance);
                }
            }
            SmoothPoints(ref points);
            
            var newPoints = new List<Vector2>();

            for (int i = 0; i < points.Count - 3; i += 2)
            {
                 newPoints.Add(points[i]);
                 newPoints.Add(points[i + 1] );

                 newPoints.Add(points[i + 2]);
                 newPoints.Add(points[i + 3]);
            }
            
            
            points = newPoints;

             

        }

        protected override void GenerateMesh()
        {
            int vertexCount = points.Count * 4;
            vertices = new Vector3[vertexCount];
            triangles = new int[points.Count * 6];
            uv = new Vector2[vertexCount];

            for (int vert = 0, trig = 0; vert < points.Count - 5 && vert < vertexCount - 4 && trig < triangles.Length - 3; vert += 4, trig += 6)
            {
                vertices[vert + 0] = (Vector3)points[vert + 0];
                vertices[vert + 1] = (Vector3)points[vert + 1];
                vertices[vert + 2] = (Vector3)points[vert + 2];
                vertices[vert + 3] = (Vector3)points[vert + 3];

                triangles[trig + 0] = vert + 0;
                triangles[trig + 1] = vert + 2;
                triangles[trig + 2] = vert + 1;
                triangles[trig + 3] = vert + 2;
                triangles[trig + 4] = vert + 3;
                triangles[trig + 5] = vert + 1;
            }
        }
    }
}
