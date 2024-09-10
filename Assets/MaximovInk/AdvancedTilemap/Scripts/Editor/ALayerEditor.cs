using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace MaximovInk.AdvancedTilemap
{
    [CustomEditor(typeof(ALayer))]
    public class ALayerEditor : Editor
    {
        private ALayer layer;

        private ALayerEditorData _data;

        private void OnEnable()
        {
            layer = (ALayer)target;

            _data.Layer = layer;
            _data.PreviewScale = EditorUtils.PREVIEW_SCALE_DEFAULT;
                
            ALayerGUI.Enable(ref _data);
        }

        private void OnDisable()
        {
            ALayerGUI.Disable(ref _data);

            EditorUtils.SaveAsset(layer);
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();

            ALayerGUI.DrawGUI(layer, ref _data);
        }

        private void OnSceneGUI()
        {
            ALayerGUI.SceneGUI(layer, ref _data);

            if (_data.RepaintInvoke)
            {
                _data.RepaintInvoke = false;
                Repaint();
            }
        }
    }
}
