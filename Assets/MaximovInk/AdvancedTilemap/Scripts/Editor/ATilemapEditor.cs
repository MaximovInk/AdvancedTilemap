﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

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

            var sceneCam = sceneView.camera;
            var spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            return spawnPos;
        }

        [MenuItem("GameObject/AdvancedTilemap", priority =10)]
        public static void CreateTilemap(MenuCommand menuCommand)
        {
            var tilemapGo = new GameObject("AdvancedTilemap");
            tilemapGo.AddComponent<ATilemap>();

            var pos  = ScreenViewPos();

            pos.z = 0f;

            tilemapGo.transform.position = pos;

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

_tilemapData.PropertiesFoldout = true;

            layers = serializedObject.FindProperty("Layers");
            list = new ReorderableList(serializedObject, layers, true, true, true, true)
            {
                drawHeaderCallback = DrawHeaderCallback,
                onAddCallback = AddCallback,
                onReorderCallbackWithDetails = ReorderCallback,
                onRemoveCallback = RemoveCallback,
                drawElementCallback = DrawElementCallback,
                onSelectCallback = SelectCallback
            };

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

            if (_tilemapData.LayerSelected < 0 || _tilemapData.LayerSelected >= tilemap.Layers.Count) return;

            SelectLayer();
        }

        private void SelectLayer()
        {
            var layer = tilemap.Layers[_tilemapData.LayerSelected];

            EditorGUIUtility.PingObject(layer);

        }

        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var textRect = new Rect(rect.x + 10, rect.y, rect.width - 80, rect.height);
            var text1Rect = new Rect(rect.x + rect.width - 70, rect.y, 30, rect.height);
           
            var text2Rect = new Rect(rect.x + rect.width - 40, rect.y, 40, rect.height);
            var pos = tilemap.Layers[index].transform.position;

            GUI.Label(textRect, tilemap.Layers[index].gameObject.name);
            GUI.Label(text1Rect, $"Z: {pos.z}");
        }

        private void RemoveCallback(ReorderableList list)
        {
            tilemap.RemoveLayer(list.index);
        }

        private void ReorderCallback(ReorderableList list, int oldIndex, int newIndex)
        {
            var oldLayer = tilemap.Layers[list.index];

            oldLayer.transform.SetSiblingIndex(newIndex);
        }

        private void AddCallback(ReorderableList list)
        {
            if (tilemap.Layers.Count > 0)
            {
                var prevLayer = tilemap.Layers[^1];

                var layer = tilemap.MakeLayer();

                layer.Tileset = prevLayer.Tileset;
                layer.Material = prevLayer.Material;

                return;
            }

            tilemap.MakeLayer();
        }

        private void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Layers");
        }

        private void DrawMainParameters()
        {
            tilemap.DisplayChunksInHierarchy = EditorGUILayout.Toggle("Display chunks in hierarchy", tilemap.DisplayChunksInHierarchy);
            tilemap.UndoEnabled = EditorGUILayout.Toggle("Undo/Redo recording:", tilemap.UndoEnabled);
            tilemap.SortingOrder = EditorGUILayout.IntField("Sorting order", tilemap.SortingOrder);
            tilemap.AutoTrim = EditorGUILayout.Toggle("Auto trim", tilemap.AutoTrim);
            GUILayout.Space(10);


            GlobalSettings.ShowGrid = EditorGUILayout.Toggle("Show grid", GlobalSettings.ShowGrid);

            if (GlobalSettings.ShowGrid)
            {
                GlobalSettings.TilemapGridColor = EditorGUILayout.ColorField("Grid Color", GlobalSettings.TilemapGridColor);
            }
        }

        private void DrawLoader()
        {
            var loader = tilemap.ChunkLoader;

            loader.Enabled = EditorGUILayout.Toggle("Chunk loader", loader.Enabled);

            GUILayout.BeginVertical(EditorStyles.helpBox);

            if (loader.Enabled)
            {
                loader.Target = EditorGUILayout.ObjectField("Target", loader.Target, typeof(Transform), true) as Transform;
                loader.TargetOffset = EditorGUILayout.Vector2IntField("Offset", loader.TargetOffset);

            }
            GUILayout.EndVertical();

            tilemap.ChunkLoader = loader;
        }

        private void DrawLiquid()
        {

            GUILayout.Label("Liquid steps:");
            tilemap.LiquidStepsDuration = EditorGUILayout.Slider(tilemap.LiquidStepsDuration, 0.001f, 1f);

            if (GUILayout.Button("Refresh all layers"))
            {
                tilemap.Refresh(true);
            }
        }

        private void DrawLighting()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

          
            EditorGUI.BeginChangeCheck();

            var light = tilemap.Lighting;

            light.Enabled = EditorGUILayout.Toggle("Lighting", light.Enabled);

            light.LightMaterial = EditorGUILayout.ObjectField("Material", light.LightMaterial, typeof(Material), true) as Material;

            light.LightingMask = EditorGUILayout.LayerField("Layer", light.LightingMask);

            tilemap.Lighting = light;

            if (EditorGUI.EndChangeCheck())
            {
                tilemap.UpdateLighting();
            }

            GUILayout.EndVertical();
        }

        private void DrawTilemapParameters()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);

            DrawMainParameters();

            GUILayout.BeginVertical();

            DrawLighting();

            GUILayout.Space(20);

            DrawLoader();

            GUILayout.EndVertical();

            DrawLiquid();

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

            tilemap.Layers ??= new List<ALayer>();

            for (var i = 0; i < tilemap.Layers.Count; i++)
            {
                if (tilemap.Layers[i] != null) continue;

                tilemap.Layers.RemoveAt(i);

                i--;

            }

            list.DoLayoutList();

            GUILayout.Label("Layers");

            serializedObject.ApplyModifiedProperties();

            if (_tilemapData.LayerSelected > -1 && _tilemapData.LayerSelected < tilemap.Layers.Count)
            {
                var layer = tilemap.Layers[_tilemapData.LayerSelected];

                var mat = layer.Material;

               ALayerGUI.DrawGUI(layer, ref _layerData);

               if (mat != layer.Material) _invokePreviewRegen = true;
            }
        }

        private void OnSceneGUI()
        {
            if (_tilemapData.LayerSelected < 0 || _tilemapData.LayerSelected >= tilemap.Layers.Count) return;
            if (_layerData.SelectedToolbar < 0) return;

            var layer = tilemap.Layers[_tilemapData.LayerSelected];

            ALayerGUI.SceneGUI(layer, ref _layerData);

          //  layer.DrawGizmos();

            if (_invokePreviewRegen && _layerData.Tool!=null)
            {
                _invokePreviewRegen = false;
                //ALayerGUI.GenPreviewTextureBrush(ref _layerData);
                _layerData.Tool.GenPreviewTextureBrush(ref _layerData);
     
            }

            if (_layerData.RepaintInvoke)
            {
                _layerData.RepaintInvoke = false;
                Repaint();
            }
        }
    }
}
