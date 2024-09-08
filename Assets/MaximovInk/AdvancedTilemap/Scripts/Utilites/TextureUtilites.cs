using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MaximovInk.AdvancedTilemap
{
    public static class TextureUtilites
    {
#if UNITY_EDITOR
        public static void OptimizeTextureImportSettings(Texture2D texture)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            if (!string.IsNullOrEmpty(assetPath))
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as UnityEditor.TextureImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                if (textureImporter.spriteImportMode == SpriteImportMode.None)
                    textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.mipmapEnabled = false;
                textureImporter.filterMode = FilterMode.Point;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                FixTextureSize(texture, textureImporter);
                AssetDatabase.ImportAsset(assetPath);
            }
        }
        static int[] textureSizes = new int[] {
        32,
        64,
        128,
        256,
        512,
        1024,
        2048,
        4096,
        8192
    };
        private static void FixTextureSize(Texture2D tex, TextureImporter importer)
        {
            int width = 0, height = 0, max;
            GetOriginalTextureSize(importer, ref width, ref height);

            max = Mathf.Max(width, height);
            int size = 1024; //Default size
            for (int i = 0; i < textureSizes.Length; i++)
            {
                if (textureSizes[i] >= max)
                {
                    size = textureSizes[i];
                    break;
                }
            }
            importer.maxTextureSize = size;
        }
        //https://forum.unity.com/threads/getting-original-size-of-texture-asset-in-pixels.165295/
        private delegate void GetWidthAndHeight(TextureImporter importer, ref int width, ref int height);
        private static GetWidthAndHeight getWidthAndHeightDelegate;
        public static void GetOriginalTextureSize(Texture2D texture, ref int width, ref int height)
        {
            if (texture == null)
                throw new NullReferenceException();

            var path = AssetDatabase.GetAssetPath(texture);
            if (string.IsNullOrEmpty(path))
                throw new Exception("Texture2D is not an asset texture.");

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                throw new Exception("Failed to get Texture importer for " + path);

            GetOriginalTextureSize(importer, ref width, ref height);
        }
        public static void GetOriginalTextureSize(TextureImporter importer, ref int width, ref int height)
        {
            if (getWidthAndHeightDelegate == null)
            {
                var method = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                getWidthAndHeightDelegate = Delegate.CreateDelegate(typeof(GetWidthAndHeight), null, method) as GetWidthAndHeight;
            }

            getWidthAndHeightDelegate(importer, ref width, ref height);
        }

#endif
    }
}
