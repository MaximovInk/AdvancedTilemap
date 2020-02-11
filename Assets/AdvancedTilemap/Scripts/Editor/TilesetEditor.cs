using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace AdvancedTilemap
{
    [CustomPropertyDrawer(typeof(Tile))]
    public class TileDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.DrawRect(new Rect(position.x,position.y+18,position.width,position.height-18),Color.white);
            EditorGUI.HelpBox(new Rect(position.x, position.y + 18, position.width, position.height - 18), "",MessageType.None);

            position.x += 4f;
            position.width -= 8f;
            position.y += 4f;

            var rect0 = new Rect(position.x, position.y + 18, position.width, 16);
            var rect1 = new Rect(position.x, position.y + 36, position.width, 16);
            var rect2 = new Rect(position.x, position.y + 54, position.width, 16);
            var rect3 = new Rect(position.x, position.y + 72, position.width, 16);
            var rect4 = new Rect(position.x, position.y + 88, position.width, 16);
            var rect5 = new Rect(position.x, position.y + 106, position.width, 16);
            var rect6 = new Rect(position.x, position.y + 124, position.width, 16);
            var rect7 = new Rect(position.x, position.y + 142, position.width, 16);
            var rect8 = new Rect(position.x, position.y + 160, position.width, 256-22);

            EditorGUI.indentLevel++;

            var name = property.FindPropertyRelative("Name");
            EditorGUI.PropertyField(rect0, name);
            EditorGUI.PropertyField(rect1, property.FindPropertyRelative("Type"));
            EditorGUI.PropertyField(rect2, property.FindPropertyRelative("VariationsCount"));
            EditorGUI.PropertyField(rect3, property.FindPropertyRelative("Position"));
            EditorGUI.PropertyField(rect4, property.FindPropertyRelative("Size"));
            EditorGUI.PropertyField(rect5, property.FindPropertyRelative("BlendOverlap"));
            EditorGUI.PropertyField(rect6, property.FindPropertyRelative("OverlapDepthIsIndex"));
            EditorGUI.PropertyField(rect7, property.FindPropertyRelative("OverlapDepth"));

            var tileset = property.FindPropertyRelative("tileset").objectReferenceValue as Tileset;
            var tile = tileset.GetTile(name.stringValue);

            var zoom = TilesetEditor.TileViewZoom;

            var texRatio = tile.TexRatio();

            TilesetEditor.TileViewZoom = GUI.HorizontalSlider(new Rect(rect8.x, rect8.y, rect8.width, 18), zoom, 0.05f,10);

            TilesetEditor.ScroolTileView = GUI.BeginScrollView(
           new Rect(rect8.x, rect8.y + 18, rect8.width, rect8.height),
           TilesetEditor.ScroolTileView,
           new Rect(rect8.x, rect8.y, 256*texRatio*zoom, 256*zoom),
           true,
           true
           );
            GUI.DrawTextureWithTexCoords(new Rect(rect8.x, rect8.y, 256*texRatio*zoom, 256*zoom), tileset.Texture, tile.TextureRect());

            GUI.EndScrollView();

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();


        }
    }

    [CustomEditor(typeof(Tileset))]
    public class TilesetEditor : Editor
    {
        private Tileset tileset;
        private ReorderableList tilesList;

        public static Vector2 ScroolTileView;
        public static float TileViewZoom = 1;

        private void OnEnable()
        {
            tileset = target as Tileset;

            tilesList = new ReorderableList(tileset.tiles,typeof(Tile),true,true,true,true);

            tilesList.drawHeaderCallback += DrawHeader;
            tilesList.drawElementCallback += DrawElement;

            tilesList.onAddCallback += AddItem;
            tilesList.onRemoveCallback += RemoveItem;

            tilesList.elementHeightCallback += (index) => indexFoldout == index && foldout ? 418 : 35;

            tilesList.onReorderCallbackWithDetails += (ReorderableList list,int a,int b) => {
                indexFoldout = b;

                //Auto replace depth if equals index

                for (int i = 0; i < list.count; i++)
                {
                    var tile = (list.list[i] as Tile);
                    if (tile.OverlapDepthIsIndex)
                    {
                        tile.OverlapDepth = i;
                    }
                }

            };
        }

        private void OnDisable()
        {
            tilesList.drawHeaderCallback -= DrawHeader;
            tilesList.drawElementCallback -= DrawElement;

            tilesList.onAddCallback -= AddItem;
            tilesList.onRemoveCallback -= RemoveItem;
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Blocks");
        }

        private int indexFoldout = -1;
        private bool foldout = false;

        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var isFoldout = EditorGUI.Foldout(new Rect(rect.x+10,rect.y,rect.width*0.3f,15), foldout && indexFoldout == index,"idx:" + index+")" + tileset.tiles[index].Name,true);

            if (isFoldout)
            {
                indexFoldout = index;
                foldout = true;
            }
            if (!isFoldout && index == indexFoldout)
            {
                foldout = false;
            }

            GUI.DrawTextureWithTexCoords(
                              new Rect(rect.x + rect.width*0.3f, rect.y,30,30),
                              tileset.Texture ,
                              tileset.tiles[index].GetTexPreview());

            if (foldout && index == indexFoldout)
            {
                EditorGUI.PropertyField(rect, serializedObject.FindProperty("tiles").GetArrayElementAtIndex(index));
            }
            

        }

        private void AddItem(ReorderableList list)
        {
            tileset.tiles.Add(new Tile() { tileset = tileset, OverlapDepth = tileset.tiles.Count });

            EditorUtility.SetDirty(target);
        }

        private void RemoveItem(ReorderableList list)
        {
            tileset.tiles.RemoveAt(list.index);

            EditorUtility.SetDirty(target);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            tileset.Texture = EditorGUILayout.ObjectField("Texture", tileset.Texture, typeof(Texture2D), false) as Texture2D;

            tileset.TileSize = EditorGUILayout.Vector2IntField("Tile size(px):", tileset.TileSize);

            tilesList.DoLayoutList();

           

            serializedObject.ApplyModifiedProperties();
        }
    }
}













