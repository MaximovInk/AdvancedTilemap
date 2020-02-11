using UnityEngine;

namespace AdvancedTilemap
{
    public static class Utils
    {
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
