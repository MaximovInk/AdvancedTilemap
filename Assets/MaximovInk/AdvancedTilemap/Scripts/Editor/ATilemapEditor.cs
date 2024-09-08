using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace MaximovInk.AdvancedTilemap
{
    [CustomEditor(typeof(ATilemap))]
    public class ATilemapEditor : Editor
    {
        private static Vector3 ScreenViewPos()
        {
            if (SceneView.sceneViews.Count == 0)
                return Vector3.zero;

            var sceneView = (SceneView.sceneViews[0] as SceneView);

            Camera sceneCam = sceneView.camera;
            Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            return spawnPos;
        }

        [MenuItem("GameObject/AdvancedTilemap", priority =10)]
        public static void CreateTilemap()
        {
            var go = new GameObject();
            go.AddComponent<ATilemap>();
            go.name = "AdvancedTilemap";
            go.transform.position = ScreenViewPos();
            Selection.activeObject = go;
        }

        private ATilemap tilemap;
        ReorderableList list;
        SerializedProperty layers;
        private int layerSelected = -1;

        private ALayerEditorData _data;

        private bool _invokePreviewRegen = false;

        private void OnEnable()
        {
            layers = serializedObject.FindProperty("layers");
            list = new ReorderableList(serializedObject, layers, true, true, true, true);

            list.drawHeaderCallback = DrawHeaderCallback;
            list.onAddCallback = AddCallback;
            list.onReorderCallbackWithDetails = ReorderCallback;
            list.onRemoveCallback = RemoveCallback;
            list.drawElementCallback = DrawElementCallback;
            list.onSelectCallback = SelectCallback;

            _data.PreviewScale = 0.8f;

            ALayerGUI.Enable(ref _data);
        }

        private void OnDisable()
        {
            ALayerGUI.Disable(ref _data);
        }

        private void SelectCallback(ReorderableList list)
        {
            if (layerSelected != list.index) _data.selectedTile = 0;

            layerSelected = list.index;

            _invokePreviewRegen = true;
        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var textRect = new Rect(rect.x + 10, rect.y, rect.width - 80, rect.height);
            var text1Rect = new Rect(rect.x + rect.width - 70, rect.y, 30, rect.height);
           
            var text2Rect = new Rect(rect.x + rect.width - 40, rect.y, 40, rect.height);
            var pos = tilemap.layers[index].transform.position;

            GUI.Label(textRect, tilemap.layers[index].gameObject.name);
            GUI.Label(text1Rect, $"Z: {pos.z}");
        }

        private void RemoveCallback(ReorderableList list)
        {
            tilemap.RemoveLayer(list.index);
        }

        private void ReorderCallback(ReorderableList list, int oldIndex, int newIndex)
        {
            var oldLayer = tilemap.layers[list.index];

            oldLayer.transform.SetSiblingIndex(newIndex);
        }

        private void AddCallback(ReorderableList list)
        {
            tilemap.MakeLayer();
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Layers");
        }

        public override void OnInspectorGUI()
        {
            tilemap = (ATilemap)target;

            serializedObject.Update();

            if (tilemap.layers == null) tilemap.layers = new List<ALayer>();

            for (int i = 0; i < tilemap.layers.Count; i++)
            {
                if (tilemap.layers[i] == null)
                {
                    tilemap.layers.RemoveAt(i);

                    i--;
                }

            }

            list.DoLayoutList();

            GUILayout.BeginVertical();

            GUILayout.Label("Tilemap parameters:");

            tilemap.DisplayChunksInHierarchy = EditorGUILayout.Toggle("Display chunks in hierarchy", tilemap.DisplayChunksInHierarchy);
            tilemap.UndoEnabled = EditorGUILayout.Toggle("Undo/Redo recording:", tilemap.UndoEnabled);
            tilemap.SortingOrder = EditorGUILayout.IntField("Sorting order", tilemap.SortingOrder);
            tilemap.AutoTrim = EditorGUILayout.Toggle("Auto trim", tilemap.AutoTrim);

            GUILayout.BeginVertical();

            GUILayout.Space(20);

            GUILayout.Label("Lighting");

            tilemap.LightingEnabled = EditorGUILayout.Toggle("Enabled", tilemap.LightingEnabled);

            if (tilemap.LightingEnabled)
            {
                var light = tilemap.Lighting;

                light.ForegroundLayer = EditorGUILayout.ObjectField("Foreground", light.ForegroundLayer, typeof(ALayer), true) as ALayer;
                light.BackgroundLayer = EditorGUILayout.ObjectField("Background", light.BackgroundLayer, typeof(ALayer), true) as ALayer;

                if (light.ForegroundLayer == null || light.ForegroundLayer.Tilemap != tilemap)
                    light.ForegroundLayer = null;
                if (light.BackgroundLayer == null || light.BackgroundLayer?.Tilemap != tilemap)
                    light.BackgroundLayer = null;

                light.LightMaterial = EditorGUILayout.ObjectField("Material", light.LightMaterial, typeof(Material), true) as Material;

                light.LightingMask = EditorGUILayout.LayerField("Layer", light.LightingMask);

                tilemap.Lighting = light;
            }

            var loader = tilemap.ChunkLoader;

            GUILayout.Space(20);

            GUILayout.Label("Loader");

            loader.Enabled = EditorGUILayout.Toggle("Enabled", loader.Enabled);

            if (loader.Enabled)
            {
                loader.Target = EditorGUILayout.ObjectField("Target", loader.Target, typeof(Transform), true) as Transform;
                loader.TargetOffset = EditorGUILayout.Vector2IntField("Offset", loader.TargetOffset);
            }

            tilemap.ChunkLoader = loader;

            GUILayout.EndVertical();

            GUILayout.Label("Liquid steps:");
            tilemap.LiquidStepsDuration = EditorGUILayout.Slider(tilemap.LiquidStepsDuration, 0.001f, 1f);

            if (GUILayout.Button("Refresh all layers"))
            {
                tilemap.Refresh(true);
            }

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();


            if (layerSelected > -1 && layerSelected < tilemap.layers.Count)
            {
                var layer = tilemap.layers[layerSelected];

               ALayerGUI.DrawGUI(layer, ref _data);
            }
        }

        private void OnSceneGUI()
        {
            if (layerSelected < 0 || layerSelected >= tilemap.layers.Count) return;
            if (_data.SelectedToolbar < 0) return;

            var layer = tilemap.layers[layerSelected];

            ALayerGUI.SceneGUI(layer, ref _data);

            if (_invokePreviewRegen)
            {
                _invokePreviewRegen = false;
                ALayerGUI.GenPreviewTextureBrush(ref _data);
            }

            if (_data.RepaintInvoke)
            {
                _data.RepaintInvoke = false;
                Repaint();
            }
        }
    }
}
