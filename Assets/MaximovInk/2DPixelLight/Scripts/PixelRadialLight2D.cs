using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.PixelLight
{
    public class PixelRadialLight2D : PixelRaycastLight2D
    {
        [Range(0,360)]
        [SerializeField] private float _angle;

        [SerializeField] private float _radius;

        
        [SerializeField, Range(1,20)] private int _rayPerDeg = 1;

        [SerializeField, Range(1,20)]
        private int _smoothIterations = 1;

        protected void SmoothPoints(ref List<Vector2> points)
        {
            for (var j = 0; j < _smoothIterations; j++)
            {
                for (var i = 1; i < points.Count - 1; i++)
                {
                    var point = points[i];
                    var pointNext = points[i + 1];

                    points[i] = (point + pointNext) / 2f;
                }
            }
        }

        protected override void CalculatePoints()
        {
            var steps = Mathf.RoundToInt(_angle * _rayPerDeg * _resolution);
            var stepSize = _angle / steps;

            points = new List<Vector2>();
            var distances = new List<float>();


            for (var i = 0; i <= steps; i++)
            {
                var angle = transform.eulerAngles.z - _angle / 2 + stepSize * i;
                var dir = AngleToDirection(angle);
                var hit = Physics2D.Raycast(transform.position, dir, _radius, _obstaclesMask);

                Vector2 point;
                float distance;

                if (hit)
                {
                    point =  Utility.OffsetDirection(transform.position, hit.point,_offsetRay, hit.distance, _radius);
                    distance = hit.distance;
                }
                else
                {
                    point = (Vector2)transform.position + dir * _radius;
                    distance = _radius;
                }

                points.Add(point);
                distances.Add(distance);

            }

            base.CalculatePoints();

            SmoothPoints(ref points);
        }

        protected override void GenerateMesh()
        {
            if (points.Count == 0)
            {
                vertices = Array.Empty<Vector3>();
                triangles = Array.Empty<int>();
                uv = Array.Empty<Vector2>();

                return;
            }

            var vertexCount = points.Count + 1;
            vertices = new Vector3[vertexCount];
            triangles = new int[(vertexCount - 2) * 3];
            uv = new Vector2[vertexCount];

            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0.5f);

            for (var i = 0; i < vertexCount - 1; i++)
            {
                var point = transform.InverseTransformPoint(points[i]);

                vertices[i + 1] = point;

                uv[i + 1] = new Vector2(point.x / (_radius * 2) + 0.5f, point.y / (_radius * 2) + 0.5f);

                if (i >= vertexCount - 2) continue;

                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            base.GenerateMesh();
        }
    }
}
