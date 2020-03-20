using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditorInternal;
using AdvancedTilemap.Lighting;

namespace AdvancedTilemap
{
    public enum BrushType
    {
        Squad,
        Circle,
        Triangle
    }

    [CustomEditor(typeof(ATilemap))]
    public class TilemapEditor : Editor
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

        [MenuItem("GameObject/AdvancedTilemap/Tilemap", priority = 10)]
        public static void CreateTilemap()
        {
            var go = new GameObject();
            go.AddComponent<ATilemap>();
            go.name = "Advanced Tilemap";
            go.transform.position = ScreenViewPos();

            Selection.activeObject = go;
        }
        [MenuItem("GameObject/AdvancedTilemap/Radial light(Raycast)", priority = 10)]
        public static void CreateRadialRaycastLight()
        {
            var go = new GameObject();
            go.AddComponent<ARadialRaycastLight>();
            go.name = "Radial light(Raycast)";
            go.transform.position = ScreenViewPos();

            Selection.activeObject = go;
        }
        [MenuItem("GameObject/AdvancedTilemap/Line light(Raycast)", priority = 10)]
        public static void CreateLineRaycastLight()
        {
            var go = new GameObject();
            go.AddComponent<ALineRaycastLight>();
            go.name = "Line light(Raycast)";
            go.transform.position = ScreenViewPos();

            Selection.activeObject = go;
        }
        [MenuItem("GameObject/AdvancedTilemap/LightCamera", priority = 10)]
        public static void CreateLightCamera()
        {
            var go = new GameObject();
            go.AddComponent<LightCamera>();
            go.name = "LightCamera";
            go.transform.position = ScreenViewPos();

            Selection.activeObject = go;
        }

        public static bool mainBox = true;
        public static bool rendererBox = true;
        public static bool lightingBox = true;
        public static bool colliderBox = true;
        public static bool mapBox = true;
        public static bool layersBox = true;
        public static bool paintBox = false;
        private static bool lastPaintBox = false;
        private ATilemap tilemap;
        public static byte selectedTile = 1;
        public static Color32 paintColor = Color.white;
        public static bool waterPlacing = false;
        public static int brushSize = 1;
        public static int eraseSize = 1;

        private ReorderableList layersList;

        public int selectedLayer = 0;

        private void OnEnable()
        {
            tilemap = target as ATilemap;
            tilemap.OnValidate();

            layersList = new ReorderableList(tilemap.Layers, typeof(Tile), true, true, true, true);

            layersList.drawHeaderCallback += DrawHeader;
            layersList.drawElementCallback += DrawElement;

            layersList.onAddCallback += AddItem;
            layersList.onRemoveCallback += RemoveItem;
            layersList.onReorderCallback += (index) => { tilemap.CalculateLayersOrder(); tilemap.RefreshAll(true); CalculateIndexes(); };
        }

        private void CalculateIndexes()
        {
            for (int i = 0; i < tilemap.Layers.Count; i++)
            {
                tilemap.Layers[i].Index = i;
            }
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Layers");
        }
        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            EditorGUI.LabelField(new Rect(rect.x + 10, rect.y, rect.width * 0.3f, 15), "idx:" + index + ")" + tilemap.Layers[index].name);

        }

        private void AddItem(ReorderableList list)
        {
            tilemap.Layers.Add(tilemap.CreateLayer());

            tilemap.CalculateLayersOrder();

            CalculateIndexes();
            EditorUtility.SetDirty(target);
        }
        private void RemoveItem(ReorderableList list)
        {
            DestroyImmediate(tilemap.Layers[list.index]);
            tilemap.Layers.RemoveAt(list.index);

            tilemap.CalculateLayersOrder();

            CalculateIndexes();
            EditorUtility.SetDirty(target);
        }

        private void GenTexture(bool erase = false)
        {
            tilemap.GenPreviewTextureBrush(erase ? eraseSize : brushSize, erase ? eraseSize : brushSize);
            tilemap.SetActivePreviewBrush(true);
        }

        private bool shiftBefore;
        private bool mouseUpBefore;
        private Vector2Int LastMousePos;

        private void DrawByErase(int xMin, int yMin, int xMax, int yMax)
        {
            for (int ix = xMin; ix < xMax; ix++)
            {
                for (int iy = yMin; iy < yMax; iy++)
                {
                    if (waterPlacing)
                    {
                        tilemap.SetLiquid(ix, iy, 0, selectedLayer);
                        continue;
                    }

                    tilemap.SetColor(ix, iy, Color.white, selectedLayer);
                    tilemap.Erase(ix, iy, selectedLayer);
                }
            }
        }

        private void DrawByBrush(int xMin, int yMin, int xMax, int yMax)
        {
            for (int ix = xMin; ix < xMax; ix++)
            {
                for (int iy = yMin; iy < yMax; iy++)
                {
                    if (waterPlacing)
                    {
                        tilemap.AddLiquid(ix, iy, 0.1f, selectedLayer);
                        continue;
                    }

                    tilemap.SetTile(ix, iy, selectedTile, selectedLayer);
                    tilemap.SetColor(ix, iy, paintColor, selectedLayer);
                }
            }
        }

        private void BresenhamLine(int x0, int y0, int x1, int y1, int min, int max, bool brush = true)
        {
            int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            for (; ; )
            {
                if (brush)
                    DrawByBrush(x0 - min, y0 - min, x0 + max, y0 + max);
                else
                    DrawByErase(x0 - min, y0 - min, x0 + max, y0 + max);
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }
        }

        private void OnSceneGUI()
        {
            if (Tools.current != Tool.None && Tools.current != Tool.Rect)
            {
                paintBox = false;
                return;
            }

            if (!paintBox)
                return;

            if (selectedLayer < 0 || selectedLayer > tilemap.Layers.Count || selectedTile == 0)
                return;


            Handles.BeginGUI();

            if (tilemap.UndoEnabled)
            {
                if (GUI.Button(new Rect(5, 5, 50, 25), "Undo"))
                {
                    tilemap.Undo();
                    tilemap.RefreshAll(true);
                }

                if (GUI.Button(new Rect(60, 5, 50, 25), "Redo"))
                {
                    tilemap.Redo();
                    tilemap.RefreshAll(true);
                }
            }

            Handles.EndGUI();

            Event e = Event.current;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            Tools.current = Tool.None;

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseUp:
                    GUIUtility.hotControl = 0;
                    if (tilemap.IsRecordingCommand)
                    {
                        tilemap.EndRecordCommand();
                    }
                    mouseUpBefore = true;
                    e.Use();
                    break;
            }

            Vector3 mousePosition = e.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            mousePosition = ray.origin;

            if (!tilemap.UpdatePreviewBrushPos(mousePosition))
            {
                GenTexture();
            }

            if (!e.shift && shiftBefore)
            {
                shiftBefore = false;
                GenTexture(false);
            }

            if (e.shift && !shiftBefore)
            {
                shiftBefore = true;
                GenTexture(true);
            }


            if (e.type == EventType.MouseDown && e.button == 0)
            {
                var localPos = tilemap.transform.InverseTransformPoint(mousePosition);
                var gridX = Utilites.GetGridX(localPos);
                var gridY = Utilites.GetGridY(localPos);
                LastMousePos = new Vector2Int(gridX, gridY);
            }

            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0)
            {
                if (mouseUpBefore && !tilemap.IsRecordingCommand)
                {
                    tilemap.BeginRecordCommand();

                    mouseUpBefore = false;
                }

                GUIUtility.hotControl = controlID;

                Vector2 localPos = tilemap.transform.InverseTransformPoint(mousePosition);

                var gridX = Utilites.GetGridX(localPos);
                var gridY = Utilites.GetGridY(localPos);

                if (e.shift)
                {
                    int eraseMin = eraseSize / 2;
                    int eraseMax = eraseSize - eraseMin;
                    BresenhamLine(LastMousePos.x, LastMousePos.y,gridX, gridY,eraseMin,eraseMax,false);
                }
                else if (e.control)
                {
                    selectedTile = tilemap.GetTile(gridX, gridY, selectedLayer);
                }
                else
                {

                    int brushMin = brushSize / 2;
                    int brushMax = brushSize - brushMin;
                    BresenhamLine(LastMousePos.x, LastMousePos.y, gridX, gridY, brushMin, brushMax);
                }

                LastMousePos = new Vector2Int(gridX, gridY);

                tilemap.Layers[selectedLayer].UpdateMesh();

                EditorUtility.SetDirty(tilemap);

                e.Use();

            }
        }

        private Vector2 scrollPos;

        private static bool showHelp = false;

        private void OnDisable()
        {
            tilemap.SetActivePreviewBrush(false);
        }

        public override void OnInspectorGUI()
        {
            layersBox = EditorGUILayout.Foldout(layersBox, "Layers edit", true);
            if (layersBox)
            {
                serializedObject.Update();
                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.Label("Layers:");
                EditorGUILayout.Space();

                layersList.DoLayoutList();


                GUILayout.EndVertical();
                serializedObject.ApplyModifiedProperties();
            }

            var tempLayer = selectedLayer+1;

            var layersNames = new string[] { "none" }.Concat(tilemap.Layers.Select(n => n.name)).ToArray();

            if (GUILayout.Button("Refresh map"))
            {
                tilemap.RefreshAll(true);
            }
            if (GUILayout.Button("Clear all"))
            {
                tilemap.ClearAll();
            }

            tilemap.LiquidStepsDuration         = EditorGUILayout.FloatField("Liquid steps duration", tilemap.LiquidStepsDuration);
            tilemap.ZBlockOffset                = EditorGUILayout.FloatField("Z block types offset", tilemap.ZBlockOffset);
            tilemap.SortingOrder                = EditorGUILayout.IntField("Sorting order", tilemap.SortingOrder);
            tilemap.ChunkLoadingOffset          = EditorGUILayout.IntField("Chunk loading offset", tilemap.ChunkLoadingOffset);
            tilemap.ChunkLoadingDuration        = EditorGUILayout.FloatField("Chunk loading rate", tilemap.ChunkLoadingDuration);
            tilemap.DisplayChunksInHierarchy    = EditorGUILayout.Toggle("Display chunks in hierarchy", tilemap.DisplayChunksInHierarchy);
            tilemap.AutoTrim                    = EditorGUILayout.Toggle("Auto trim", tilemap.AutoTrim);
            tilemap.UndoEnabled                 = EditorGUILayout.Toggle("Undo enabled", tilemap.UndoEnabled);
            tilemap.LiquidMinColor              = EditorGUILayout.ColorField("Liquid min color:", tilemap.LiquidMinColor);
            tilemap.LiquidMaxColor              = EditorGUILayout.ColorField("Liquid max color:", tilemap.LiquidMaxColor);
            tempLayer                           = EditorGUILayout.Popup("Selected layer:", tempLayer, layersNames);

            selectedLayer = tempLayer-1;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (selectedLayer >= 0 && selectedLayer < tilemap.Layers.Count)
            {
                paintBox = EditorGUILayout.Foldout(paintBox, "Paint", true);

                if (paintBox != lastPaintBox)
                {
                    lastPaintBox = paintBox;
                    if (paintBox)
                    {
                        Tools.current = Tool.Rect;
                    }
                }
                if (paintBox)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    showHelp = EditorGUILayout.Foldout(showHelp, "Help");
                    if (showHelp)
                    {
                        GUILayout.Label("RMB - place tile \nRMB+Shift - erase tile \nRMB+Control - pick color");
                    }

                    GUILayout.Label("Paint:");


                    waterPlacing = EditorGUILayout.Toggle("Placing water", waterPlacing);
                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    GUILayout.BeginHorizontal();

                    if (tilemap.Layers[selectedLayer].Tileset != null)
                    {
                        for (int i = 0; i < tilemap.Layers[selectedLayer].Tileset.tiles.Count; i++)
                        {

                            if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                            {
                                selectedTile = (byte)(i + 1);
                                GenTexture();

                            }

                            var rect = GUILayoutUtility.GetLastRect();

                            GUI.DrawTextureWithTexCoords(
                                rect,
                                tilemap.Layers[selectedLayer].Tileset.Texture,
                                tilemap.Layers[selectedLayer].Tileset.tiles[i].GetTexPreview());
                        }
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.EndScrollView();

                    paintColor = EditorGUILayout.ColorField(paintColor);
                    EditorGUI.BeginChangeCheck();
                    brushSize = EditorGUILayout.IntSlider(brushSize, 1, 64);
                    eraseSize = EditorGUILayout.IntSlider(eraseSize, 1, 64);
                    if (EditorGUI.EndChangeCheck())
                    {
                        GenTexture();
                    }
                    GUILayout.EndVertical();
                }

                tilemap.Layers[selectedLayer].name = EditorGUILayout.TextField("Name:", tilemap.Layers[selectedLayer].name);
                tilemap.Layers[selectedLayer].Tileset = EditorGUILayout.ObjectField("Tileset:", tilemap.Layers[selectedLayer].Tileset, typeof(Tileset), false) as Tileset;
                tilemap.Layers[selectedLayer].Material = EditorGUILayout.ObjectField("Material:", tilemap.Layers[selectedLayer].Material, typeof(Material), false) as Material;
                tilemap.Layers[selectedLayer].LiquidMaterial = EditorGUILayout.ObjectField("LiquidMaterial:", tilemap.Layers[selectedLayer].LiquidMaterial, typeof(Material), false) as Material;
                tilemap.Layers[selectedLayer].TintColor = EditorGUILayout.ColorField("Tint color:", tilemap.Layers[selectedLayer].TintColor);
                tilemap.Layers[selectedLayer].LayerMask = EditorGUILayout.LayerField("LayerMask:", tilemap.Layers[selectedLayer].LayerMask);
                tilemap.Layers[selectedLayer].PhysicsMaterial2D = EditorGUILayout.ObjectField("Name:", tilemap.Layers[selectedLayer].PhysicsMaterial2D, typeof(PhysicsMaterial2D), false) as PhysicsMaterial2D;
                tilemap.Layers[selectedLayer].IsTrigger = EditorGUILayout.Toggle("Is trigger:", tilemap.Layers[selectedLayer].IsTrigger);
                tilemap.Layers[selectedLayer].ColliderEnabled = EditorGUILayout.Toggle("Collider enabled:", tilemap.Layers[selectedLayer].ColliderEnabled);
                tilemap.Layers[selectedLayer].LiquidEnabled = EditorGUILayout.Toggle("Liquid enabled:", tilemap.Layers[selectedLayer].LiquidEnabled);
                tilemap.Layers[selectedLayer].Tag = EditorGUILayout.TagField("Tag:", tilemap.Layers[selectedLayer].Tag);

            }

            EditorGUILayout.EndVertical();
        }
    }
}
