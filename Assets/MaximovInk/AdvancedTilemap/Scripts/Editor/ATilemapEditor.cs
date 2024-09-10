using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace MaximovInk.AdvancedTilemap
{

    [System.Serializable]
    public struct ATilemapEditorData
    {
        public bool PropertiesFoldout;
        public int LayerSelected;
    }

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
        public static void CreateTilemap(MenuCommand menuCommand)
        {
            var tilemapGo = new GameObject("AdvancedTilemap");
            tilemapGo.AddComponent<ATilemap>();
            tilemapGo.transform.position = ScreenViewPos();

            if (menuCommand.context is GameObject selectedGameObject)
                tilemapGo.transform.SetParent(selectedGameObject.transform);

            Selection.activeObject = tilemapGo;
        }

        private ATilemap tilemap;
        ReorderableList list;
        SerializedProperty layers;
        //private int layerSelected = -1;

        private static ALayerEditorData _layerData;
        private static ATilemapEditorData _tilemapData;

        private bool _invokePreviewRegen = false;

        private void OnEnable()
        {
            tilemap = (ATilemap)target;

            layers = serializedObject.FindProperty("layers");
            list = new ReorderableList(serializedObject, layers, true, true, true, true);

            list.drawHeaderCallback = DrawHeaderCallback;
            list.onAddCallback = AddCallback;
            list.onReorderCallbackWithDetails = ReorderCallback;
            list.onRemoveCallback = RemoveCallback;
            list.drawElementCallback = DrawElementCallback;
            list.onSelectCallback = SelectCallback;

            _layerData.PreviewScale = EditorUtils.PREVIEW_SCALE_DEFAULT;
 

            ALayerGUI.Enable(ref _layerData);


            _tilemapData.LayerSelected = 0;
            if (list.count > _tilemapData.LayerSelected)
            {
                list.Select(_tilemapData.LayerSelected);
                SelectLayer();
            }
        }

        private void OnDisable()
        {
            ALayerGUI.Disable(ref _layerData);
        }

        private void SelectCallback(ReorderableList list)
        {
            if (_tilemapData.LayerSelected != list.index) _layerData.selectedTile = 0;

            _tilemapData.LayerSelected = list.index;

            _invokePreviewRegen = true;

            if (_tilemapData.LayerSelected < 0 || _tilemapData.LayerSelected >= tilemap.layers.Count) return;

            SelectLayer();
        }

        private void SelectLayer()
        {
            var layer = tilemap.layers[_tilemapData.LayerSelected];

            EditorGUIUtility.PingObject(layer);

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

        private void DrawTilemapParameters()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            tilemap.DisplayChunksInHierarchy = EditorGUILayout.Toggle("Display chunks in hierarchy", tilemap.DisplayChunksInHierarchy);
            tilemap.UndoEnabled = EditorGUILayout.Toggle("Undo/Redo recording:", tilemap.UndoEnabled);
            tilemap.SortingOrder = EditorGUILayout.IntField("Sorting order", tilemap.SortingOrder);
            tilemap.AutoTrim = EditorGUILayout.Toggle("Auto trim", tilemap.AutoTrim);
            GUILayout.Space(10);
            tilemap.ShowGrid = EditorGUILayout.Toggle("Show grid", tilemap.ShowGrid);

            GUILayout.BeginVertical();

            GUILayout.Space(20);

            tilemap.LightingEnabled = EditorGUILayout.Toggle("Lighting", tilemap.LightingEnabled);

            if (tilemap.LightingEnabled)
            {
               GUILayout.BeginVertical(EditorStyles.helpBox);

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

                GUILayout.EndVertical();
            }

            var loader = tilemap.ChunkLoader;

            GUILayout.Space(20);


            loader.Enabled = EditorGUILayout.Toggle("Chunk loader", loader.Enabled);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            if (loader.Enabled)
            {
                loader.Target = EditorGUILayout.ObjectField("Target", loader.Target, typeof(Transform), true) as Transform;
                loader.TargetOffset = EditorGUILayout.Vector2IntField("Offset", loader.TargetOffset);

            }
            GUILayout.EndVertical();

            tilemap.ChunkLoader = loader;

            GUILayout.EndVertical();

            GUILayout.Label("Liquid steps:");
            tilemap.LiquidStepsDuration = EditorGUILayout.Slider(tilemap.LiquidStepsDuration, 0.001f, 1f);

            if (GUILayout.Button("Refresh all layers"))
            {
                tilemap.Refresh(true);
            }

            GUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _tilemapData.PropertiesFoldout = EditorGUILayout.Foldout(_tilemapData.PropertiesFoldout, "Tilemap parameters");

            if (_tilemapData.PropertiesFoldout)
            {
               DrawTilemapParameters();
            }

            GUILayout.Space(20);

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

            GUILayout.Label("Layers");

            serializedObject.ApplyModifiedProperties();

            if (_tilemapData.LayerSelected > -1 && _tilemapData.LayerSelected < tilemap.layers.Count)
            {
                var layer = tilemap.layers[_tilemapData.LayerSelected];

               ALayerGUI.DrawGUI(layer, ref _layerData);
            }
        }

        private void OnSceneGUI()
        {
            if (_tilemapData.LayerSelected < 0 || _tilemapData.LayerSelected >= tilemap.layers.Count) return;
            if (_layerData.SelectedToolbar < 0) return;

            var layer = tilemap.layers[_tilemapData.LayerSelected];

            ALayerGUI.SceneGUI(layer, ref _layerData);

          //  layer.DrawGizmos();

            if (_invokePreviewRegen)
            {
                _invokePreviewRegen = false;
                ALayerGUI.GenPreviewTextureBrush(ref _layerData);
     
            }

            if (_layerData.RepaintInvoke)
            {
                _layerData.RepaintInvoke = false;
                Repaint();
            }
        }
    }
}
