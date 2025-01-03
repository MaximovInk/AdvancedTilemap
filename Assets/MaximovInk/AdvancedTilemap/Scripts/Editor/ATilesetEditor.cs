using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{

    [CustomEditor(typeof(ATileset))]
    public class ATilesetEditor : Editor
    {
        private ATileset _tileset;
        private ATilesetEditorData _data;

        private void OnEnable()
        {
            _tileset = (ATileset)target;
            _data.PreviewScale = EditorUtils.PREVIEW_SCALE_DEFAULT;

        }

        public override void OnInspectorGUI()
        {
            if (ATilesetGUI.DrawGUI(_tileset, ref _data))
            {
                EditorUtility.SetDirty(_tileset);
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            _tileset = (ATileset)target;

            if (_tileset == null || _tileset.Texture == null)
                return null;

            var tex = new Texture2D(_tileset.Texture.width, _tileset.Texture.height);
            EditorUtility.CopySerialized(_tileset.Texture, tex);

            return tex;
        }
    }
}