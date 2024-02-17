using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public static class GUIUtilites
    {
        public static void TransformGUIFromUITransform(Rect rectTile, UVTransform transform)
        {
            if (transform._rot90)
                GUIUtility.RotateAroundPivot(90, rectTile.center);

            var scale = Vector2.one;
            if (transform._flipHorizontal)
                scale.x = -1;
            if (transform._flipVertical)
                scale.y = -1;

            GUIUtility.ScaleAroundPivot(scale, rectTile.center);
        }
    }
}
