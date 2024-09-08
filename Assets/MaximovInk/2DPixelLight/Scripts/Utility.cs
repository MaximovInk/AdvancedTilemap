using UnityEngine;

namespace MaximovInk.PixelLight
{
    public static class Utility
    {
        public static Vector2 AngleToDirection(float angleDeg)
        {
            return new Vector2(Mathf.Sin(angleDeg * Mathf.Deg2Rad), Mathf.Cos(angleDeg * Mathf.Deg2Rad));
        }

        public static Vector2 OffsetDirection(Vector2 originPosition, Vector2 direction, float offsetRay, float distance, float max)
        {
            var angle = 90f - AngleInDeg(originPosition, direction);

            return originPosition + AngleToDirection(angle) * (Mathf.Min(max, distance + offsetRay));
        }

        public static float AngleInRad(Vector3 vec1, Vector3 vec2)
        {
            return Mathf.Atan2(vec2.y - vec1.y, vec2.x - vec1.x);
        }

        public static float AngleInDeg(Vector3 vec1, Vector3 vec2)
        {
            return AngleInRad(vec1, vec2) * 180 / Mathf.PI;
        }

        public static Vector3 DirFromAngle(float angleInDegrees)
        {
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
        }
    }
}
