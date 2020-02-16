using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdvancedTilemap
{
    public static class Utilites
    {
        private const float viewAngle = 360f;

        //This returns the angle in radians
        public static float AngleInRad(Vector3 vec1, Vector3 vec2)
        {
            return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
        }

        //This returns the angle in degrees
        public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
        {
            return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
        }

        public static Mesh GenCircle(float radius, Color Color,float resolution = 0.1f, float fade = 1)
        {
            var steps = Mathf.RoundToInt(viewAngle * resolution);

            //Debug.Log("steps:" + steps);

            if (steps < 6)
            {
                return new Mesh();
            }

            var stepAngleSize = viewAngle / steps;
            //Debug.Log("stepAngleSize:" + stepAngleSize);

            List<Vector3> points = new List<Vector3>();

            for (int i = 0; i <= steps; i++)
            {
                float angle = stepAngleSize * i;
                //Debug.Log("angle:" + angle);

                var point = DirFromAngle(angle)* radius;

                points.Add(point);
            }

            int vertexCount = points.Count + 1;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount - 2) * 3];
            Color32[] colors32 = new Color32[vertexCount];

            vertices[0] = Vector3.zero;
            colors32[0] = Color;
            for (int i = 0; i < vertexCount - 1; i++)
            {
                vertices[i + 1] = points[i];
                colors32[i + 1] = Color32.Lerp(Color, new Color(Color.r,Color.g,Color.b,0), fade);
                if (i < vertexCount - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

 
             var   mesh = new Mesh();
  
            mesh.vertices = vertices; 
            mesh.colors32 = colors32;
            mesh.triangles = triangles;
       

            mesh.RecalculateNormals();
            return mesh;
        }


        public static Vector3 DirFromAngle(float angleInDegrees)
        {
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
        }

    }
}
