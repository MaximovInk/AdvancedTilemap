using UnityEngine;

namespace AdvancedTilemap
{
    public static class Utilites
    {
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

        public static Vector3 DirFromAngle(float angleInDegrees)
        {
            return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
        }

        public static int GetGridX(Vector2 localPos)
        {
            return Mathf.FloorToInt((localPos.x + Vector2.kEpsilon));
        }

        public static int GetGridY(Vector2 localPos)
        {
            return Mathf.FloorToInt((localPos.y + Vector2.kEpsilon));
        }

        public static Vector2Int GetGridPosition(Vector2 localPos)
        {
            return new Vector2Int(GetGridX(localPos),GetGridY(localPos));
        }

        static public int GetMouseGridX(this ATilemap tilemap, Camera camera)
        {
            var mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

            return GetGridX(tilemap.transform.InverseTransformPoint(mousePos));
        }

        static public int GetMouseGridY(this ATilemap tilemap, Camera camera)
        {
            var mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

            return GetGridY(tilemap.transform.InverseTransformPoint(mousePos));
        }

        public static Vector2 BoundsMin(this Camera camera)
        {
            return (Vector2)camera.transform.position - camera.Extents();
        }

        public static Vector2 BoundsMax(this Camera camera)
        {
            return (Vector2)camera.transform.position + camera.Extents();
        }

        public static Vector2 Extents(this Camera camera)
        {
            if (camera.orthographic)
                return new Vector2(camera.orthographicSize * Screen.width / Screen.height, camera.orthographicSize);
            else
            {
                //Debug.LogError("Camera is not orthographic!", camera);
                return new Vector2();
            }
        }
    }
}
