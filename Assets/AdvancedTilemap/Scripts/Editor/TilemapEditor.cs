using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEditorInternal;

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
        public static int brushSize = 1;
        public static int eraseSize = 1;

        private ReorderableList layersList;
        private float startHeight;

        public int selectedLayer = -1;

        private void OnEnable()
        {
            tilemap = target as ATilemap;
            tilemap.OnValidate();

            layersList = new ReorderableList(tilemap.Layers, typeof(Tile), true, true, true, true);

            layersList.drawHeaderCallback += DrawHeader;
            layersList.drawElementCallback += DrawElement;

            layersList.onAddCallback += AddItem;
            layersList.onRemoveCallback += RemoveItem;

            startHeight = layersList.elementHeight;

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

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            Tools.current = Tool.None;

            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseUp:
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;
            }

            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0)
            {
                GUIUtility.hotControl = controlID;

                Vector3 mousePosition = e.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
                mousePosition = ray.origin;
                Vector2 localPos = tilemap.transform.InverseTransformPoint(mousePosition);

                var gridX = Utils.GetGridX(localPos);
                var gridY = Utils.GetGridY(localPos);

                if (e.shift)
                {
                    
                    int brushMin = eraseSize / 2;
                    int brushMax = eraseSize - brushMin;

                    for (int ix = gridX - brushMin; ix < gridX + brushMax; ix++)
                    {
                        for (int iy = gridY - brushMin; iy < gridY + brushMax; iy++)
                        {
                            tilemap.SetColor(ix, iy, Color.white, selectedLayer);
                            tilemap.Erase(ix, iy, selectedLayer);
                        }
                    }
                }
                else if(e.control)
                {
                    //tilemap.SetLight(gridX, gridY,Color.white,selectedLayer);
                    //tilemap.Layers[selectedLayer].UpdateLight();

                    selectedTile = tilemap.GetTile(gridX, gridY, selectedLayer);
                }
                else
                {
                   
                    int brushMin = brushSize / 2;
                    int brushMax = brushSize - brushMin;

                    for (int ix = gridX - brushMin; ix < gridX + brushMax; ix++)
                    {
                        for (int iy = gridY - brushMin; iy < gridY + brushMax; iy++)
                        {
                            tilemap.SetTile(ix, iy, selectedTile,selectedLayer);
                            tilemap.SetColor(ix, iy, paintColor, selectedLayer);
                        }
                    }
                }
                EditorUtility.SetDirty(tilemap);
                e.Use();

            }
        }
        
        private Vector2 scrollPos;

        private static bool showHelp = false;

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

            var tempLayer = selectedLayer + 1;

            var layersNames = new string[] { "none" }.Concat(tilemap.Layers.Select(n => n.name)).ToArray();

            if (GUILayout.Button("Refresh map"))
            {
                tilemap.RefreshAll(true);
            }
            if (GUILayout.Button("Clear all"))
            {
                tilemap.ClearAll();
            }

            tempLayer = EditorGUILayout.Popup("Selected layer:", tempLayer, layersNames);

            selectedLayer = tempLayer - 1;

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
                        GUILayout.Label("RMB - place pixel \nRMB+Shift - erase pixel \nRMB+Control - pick color");
                    }

                    GUILayout.Label("Paint:");
                    scrollPos = GUILayout.BeginScrollView(scrollPos);

                    GUILayout.BeginHorizontal();

                    if (tilemap.Layers[selectedLayer].Tileset != null)
                    {
                        for (int i = 0; i < tilemap.Layers[selectedLayer].Tileset.tiles.Count; i++)
                        {

                            if (GUILayout.Button("", GUILayout.Width(40), GUILayout.Height(40)))
                            {
                                selectedTile = tilemap.Layers[selectedLayer].Tileset.GetTileAt((byte)i);
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

                    brushSize = EditorGUILayout.IntSlider(brushSize, 1, 64);
                    eraseSize = EditorGUILayout.IntSlider(eraseSize, 1, 64);

                    GUILayout.EndVertical();
                }

                tilemap.Layers[selectedLayer].name = EditorGUILayout.TextField( "Name:", tilemap.Layers[selectedLayer].name);
                tilemap.Layers[selectedLayer].Tileset = EditorGUILayout.ObjectField( "Tileset:", tilemap.Layers[selectedLayer].Tileset, typeof(Tileset), false) as Tileset;
                tilemap.Layers[selectedLayer].Material = EditorGUILayout.ObjectField( "Material:", tilemap.Layers[selectedLayer].Material, typeof(Material), false) as Material;
                tilemap.Layers[selectedLayer].TintColor = EditorGUILayout.ColorField( "Tint color:", tilemap.Layers[selectedLayer].TintColor);
                tilemap.Layers[selectedLayer].LayerMask = EditorGUILayout.LayerField( "LayerMask:", tilemap.Layers[selectedLayer].LayerMask);
                tilemap.Layers[selectedLayer].PhysicsMaterial2D = EditorGUILayout.ObjectField( "Name:", tilemap.Layers[selectedLayer].PhysicsMaterial2D, typeof(PhysicsMaterial2D), false) as PhysicsMaterial2D;
                tilemap.Layers[selectedLayer].IsTrigger = EditorGUILayout.Toggle( "Is trigger:", tilemap.Layers[selectedLayer].IsTrigger);
                tilemap.Layers[selectedLayer].ColliderEnabled = EditorGUILayout.Toggle( "Collider enabled:", tilemap.Layers[selectedLayer].ColliderEnabled);
                tilemap.Layers[selectedLayer].Tag = EditorGUILayout.TagField("Tag:", tilemap.Layers[selectedLayer].Tag);


                /*EditorGUI.indentLevel++;

                EditorGUI.DrawRect(new Rect(rect.x, rect.y + 18, rect.width, rect.height - 18), Color.white);
                EditorGUI.HelpBox(new Rect(rect.x, rect.y + 18, rect.width, rect.height - 18), "", MessageType.None);

                tilemap.Layers[index].name = EditorGUI.TextField(rect0, "Name:", tilemap.Layers[index].name);
                tilemap.Layers[index].Tileset = EditorGUI.ObjectField(rect1, "Name:", tilemap.Layers[index].Tileset, typeof(Tileset), false) as Tileset;
                tilemap.Layers[index].Material = EditorGUI.ObjectField(rect2, "Name:", tilemap.Layers[index].Material, typeof(Material), false) as Material;
                tilemap.Layers[index].TintColor = EditorGUI.ColorField(rect3, "Tint color:", tilemap.Layers[index].TintColor);
                tilemap.Layers[index].LayerMask = EditorGUI.LayerField(rect4, "LayerMask:", tilemap.Layers[index].LayerMask);
                //tilemap.Layers[index].ZOrder = EditorGUI.FloatField(rect5, "Z order:", tilemap.Layers[index].ZOrder);
                tilemap.Layers[index].PhysicsMaterial2D = EditorGUI.ObjectField(rect5, "Name:", tilemap.Layers[index].PhysicsMaterial2D, typeof(PhysicsMaterial2D), false) as PhysicsMaterial2D;
                tilemap.Layers[index].IsTrigger = EditorGUI.Toggle(rect6, "Is trigger:", tilemap.Layers[index].IsTrigger);
                tilemap.Layers[index].ColliderEnabled = EditorGUI.Toggle(rect7, "Collider enabled:", tilemap.Layers[index].ColliderEnabled);
                tilemap.Layers[index].Tag = EditorGUI.TagField(rect8, "Tag:", tilemap.Layers[index].Tag);

                EditorGUI.indentLevel--;*/


                /*
                mapBox = EditorGUILayout.Foldout(mapBox, "Map", true);
                if (mapBox)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.Label("Map properties:");
                    EditorGUILayout.Space();

                    //tilemap.Tag = EditorGUILayout.TagField("Tag:", tilemap.Tag);
                    tilemap.DisplayChunksInHierarchy = GUILayout.Toggle(tilemap.DisplayChunksInHierarchy, "Display chunks in hierarchy");
                    tilemap.AutoTrim = EditorGUILayout.Toggle("Auto trim:", tilemap.AutoTrim);

                    if (GUILayout.Button("Refresh map"))
                    {
                        tilemap.RefreshAll(true);
                    }
                    if (GUILayout.Button("Clear all"))
                    {
                        tilemap.Clear();
                    }
                    GUILayout.EndVertical();
                }

                rendererBox = EditorGUILayout.Foldout(rendererBox, "Renderer", true);
                if (rendererBox)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.Label("Renderer properties:");
                    EditorGUILayout.Space();

                    tilemap.SortingOrder = EditorGUILayout.IntField("Sorting order:", tilemap.SortingOrder);
                    //tilemap.Material = EditorGUILayout.ObjectField("Material:", tilemap.Material, typeof(Material), false) as Material;
                    //tilemap.TintColor = EditorGUILayout.ColorField("Color:", tilemap.TintColor);
                    GUILayout.EndVertical();
                }

                colliderBox = EditorGUILayout.Foldout(colliderBox, "Collider", true);
                if (colliderBox)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    GUILayout.Label("Collider properties:");
                    EditorGUILayout.Space();

                    //tilemap.ColliderEnabled = EditorGUILayout.Toggle("Collider generation:", tilemap.ColliderEnabled);
                    //tilemap.IsTrigger = GUILayout.Toggle(tilemap.IsTrigger, "Is trigger");
                    //tilemap.LayerMask = EditorGUILayout.LayerField("Layer", tilemap.LayerMask);
                    //tilemap.PhysicsMaterial2D = (PhysicsMaterial2D)EditorGUILayout.ObjectField("Physics material 2D:", tilemap.PhysicsMaterial2D, typeof(PhysicsMaterial2D), false);

                    GUILayout.EndVertical();
                }

                lightingBox = EditorGUILayout.Foldout(lightingBox, "Lighing", true);
                if (lightingBox)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    GUILayout.Label("Lighting properties:");
                    EditorGUILayout.Space();

                    if (GUILayout.Button("Clear all"))
                    {
                        tilemap.Clear();
                    }
                    GUILayout.EndVertical();
                }
                */

            }


        }
    }
}
