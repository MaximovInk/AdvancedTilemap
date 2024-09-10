using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public static class Utilites
    {
        public static int GetGridX(ALayer layer, Vector2 localPos)
        {
            var tileUnit = layer.Tileset.GetTileUnit();
            return Mathf.FloorToInt((localPos.x / tileUnit.x) );
        }

        public static int GetGridY(ALayer layer, Vector2 localPos)
        {
            var tileUnit = layer.Tileset.GetTileUnit();
            return Mathf.FloorToInt((localPos.y /tileUnit.y));
        }

        public static Vector2Int ConvertGlobalCoordsToGrid(ALayer layer, Vector2 worldPos)
        {
            var localPos = layer.transform.InverseTransformPoint(worldPos);
            var posX = GetGridX(layer, localPos);
            var posY = GetGridY(layer, localPos);
            return new Vector2Int(posX, posY);
        }

        public static void DrawLine(ALayer layer, Vector2Int from, Vector2Int to, ushort ID, Color32 color, bool erase = false)
        {
            var x = from.x;
            var y = from.y;
            var x2 = to.x;
            var y2 = to.y;

            var w = x2 - x;
            var h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            var numerator = longest >> 1;
            for (var i = 0; i <= longest; i++)
            {
                if (erase)
                    layer.SetTile(x, y,0);
                else
                {
                    layer.SetTile(x, y, ID);
                    layer.SetColor(x, y, color);
                }
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }

        private static void SetTile(int x,int y, ALayer layer, int size, ushort ID, Color32 color,UVTransform tileData = default)
        {
            var min = size / 2;
            var max = size - min;

            for (var ix = x-min; ix < x+max; ix++)
            {
                for (var iy = y - min; iy < y + max; iy++)
                {
                    layer.SetTile(ix, iy, ID, tileData);
                    layer.SetColor(ix, iy, color);
                }
            }
        }

        public static void DrawLine(ALayer layer, Vector2Int from, Vector2Int to, int size, ushort ID, Color32 color, UVTransform tileData = default)
        {
            var x = from.x;
            var y = from.y;
            var x2 = to.x;
            var y2 = to.y;

            var w = x2 - x;
            var h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            var longest = Math.Abs(w);
            var shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            var numerator = longest >> 1;
            for (var i = 0; i <= longest; i++)
            {
                SetTile(x, y, layer, size, ID, color,tileData);
                
                
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
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
                return new Vector2();
            }
        }

        public static Vector2Int GetGridPosition(ALayer layer, Vector2 localPos)
        {
            return new Vector2Int(GetGridX(layer, localPos), GetGridY(layer, localPos));
        }

        private static ATileDriver[] _drivers;

        public static ATileDriver[] GetAllDriversOfProject()
        {
            var tileDriverBaseClassType = typeof(ATileDriver);

            var assembly = tileDriverBaseClassType.Assembly;

            var drivers = assembly.GetTypes().Where(n => n.IsSubclassOf(tileDriverBaseClassType)).ToArray();

            var driverInstances = new ATileDriver[drivers.Length];

            for (var i = 0; i < drivers.Length; i++)
            {
                var driver = (ATileDriver)Activator.CreateInstance(drivers[i]);

                driverInstances[i] = driver;
            }

            return driverInstances;
        }

        public static ATileDriver GetTileDriverInstance(string id)
        {

            var tileDriverBaseClassType = typeof(ATileDriver);

            var assembly = tileDriverBaseClassType.Assembly;

            var drivers = assembly.GetTypes().Where(n => n.IsSubclassOf(tileDriverBaseClassType)).ToArray();

            if (_drivers == null || drivers.Length != _drivers.Length)
            {
                _drivers = new ATileDriver[drivers.Length];

                for (var i = 0; i < drivers.Length; i++)
                {
                    var driver = (ATileDriver)Activator.CreateInstance(drivers[i]);

                    _drivers[i] = driver;
                }
            }

            return _drivers.FirstOrDefault(n => n.Name == id);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Color32ToInt(Color32 color)
        {
            return (color.r << 24 | color.g << 16 | color.b << 8 | color.a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 IntToColor32(int value)
        {
            return new Color32((byte)((value >> 24) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF));
        }
    }
}
