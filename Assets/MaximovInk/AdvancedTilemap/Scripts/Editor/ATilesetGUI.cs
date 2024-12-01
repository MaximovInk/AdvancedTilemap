using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public static class ATilesetGUI
    {
        private const float SPACING = 10f;
        private static bool _isDirty;
        private static ATileDriver[] _tileDrivers;

        public static void UpdateTileDrivers()
        {
            _tileDrivers = Utilites.GetAllDriversOfProject();
        }

        private static ATileDriver TileDriverPopup(ref ATilesetEditorData editorData)
        {
            var tileDriverID = editorData.TileDriverID;
            var foundTileDriver = _tileDrivers.FirstOrDefault(n => n.ID == tileDriverID);
            if (foundTileDriver is not null)
            {
                editorData.TileDriverIndex = Array.IndexOf(_tileDrivers, foundTileDriver);
            }

            var prevIndex = editorData.TileDriverIndex;
            editorData.TileDriverIndex =
                EditorGUILayout.Popup(Mathf.Clamp(editorData.TileDriverIndex, 0, _tileDrivers.Length), _tileDrivers.Select(n => n.ID).ToArray());
            if (prevIndex != editorData.TileDriverIndex || foundTileDriver is null)
            {
                foundTileDriver = _tileDrivers[editorData.TileDriverIndex];
                editorData.TileDriverID = foundTileDriver.ID;
            }

            return foundTileDriver;
        }

        public static bool DrawGUI(ATileset tileset, ref ATilesetEditorData editorData)
        {
            _isDirty = false;

            if (tileset == null) return _isDirty;

            UpdateTileDrivers();

            BeginChangeCheck();

            tileset.Texture =
                (Texture2D)EditorGUILayout.ObjectField("Texture: ", tileset.Texture, typeof(Texture2D), false);

            if (tileset.Texture != null)
            {
                if (GUILayout.Button("Optimize atlas settings"))
                {
                    TextureUtilites.OptimizeTextureImportSettings(tileset.Texture);
                }
            }

            if (tileset.Texture == null) return _isDirty;

            tileset.TileSize = EditorGUILayout.Vector2IntField("TileSize", tileset.TileSize);

            tileset.TileSize.Clamp(new Vector2Int(1, 1), new Vector2Int(tileset.Texture.width, tileset.Texture.height));

            tileset.PixelPerUnit = EditorGUILayout.Vector2IntField("PixelPerUnit", tileset.PixelPerUnit);

            EndChangeCheck();

            var texSize = new Vector2Int(tileset.Texture.width, tileset.Texture.height);

            for (int i = 0; i < tileset.TilesCount; i++)
            {
                var tileValidate = tileset.GetTile(i);

                for (int tileUV = 0; tileUV < tileValidate.Variations.Count; tileUV++)
                {
                    var uv = tileValidate.Variations[tileUV];

                    if (uv.TextureSize.x == 0 || uv.TextureSize.y == 0)
                        uv.TextureSize = texSize;

                    if (uv.TextureSize != texSize)
                    {
                        uv.UpdateTextureSize(texSize);
                        _isDirty = true;
                    }

                    tileValidate.SetUV(uv, tileUV);
                }
            }

            editorData.ShowTilesAsList = GUILayout.Toggle(editorData.ShowTilesAsList, "ShowAsList");

            EditorUtils.DrawPreviewTileset(tileset, ref editorData.SelectedTile, ref editorData.PreviewScale,
                ref editorData.ScrollViewValue, ref editorData.ShowTilesAsList);

            GUILayout.Space(SPACING);

            if (_tileDrivers == null || _tileDrivers.Length == 0)
            {
                return _isDirty;
            }

            GUILayout.BeginHorizontal();

            var tileDriver = TileDriverPopup(ref editorData);

            if (tileDriver is null)
            {
                Debug.LogError(
                    $"TileDriver is null! ID:({editorData.TileDriverID}) Idx:({editorData.TileDriverIndex})");

                GUILayout.EndHorizontal();

                return _isDirty;
            }

            if (GUILayout.Button("Generate tiles"))
            {
                tileset.SetTiles(tileDriver.GenerateTiles(tileset));
                _isDirty = true;
            }


            if (GUILayout.Button("Add tile"))
            {
                var id = tileset.AddTile(tileDriver);
                tileset.UpdateIDs();
                editorData.SelectedTile = id;

                _isDirty = true;
            }

            GUILayout.EndHorizontal();

            GUI.color = Color.red;



            if (editorData.InvokeClearAll)
            {
                GUILayout.Label("CLEAR ALL Tiles?");

                if (GUILayout.Button("Clear"))
                {
                    tileset.ClearTiles();
                    editorData.SelectedTile = 0;

                    editorData.InvokeClearAll = false;
                    _isDirty = true;
                }
            }
            else
            {
                if (GUILayout.Button("Clear"))
                {
                    editorData.InvokeClearAll = true;
                }
            }

            GUI.color = Color.white;

            if (!(editorData.SelectedTile > 0 && editorData.SelectedTile < tileset.TilesCount + 1)) return _isDirty;

            var tile = tileset.GetTile(editorData.SelectedTile);

            GUILayout.Space(SPACING);

            DrawTileEditor(tileset, tile, ref editorData);


            return _isDirty;
        }

        private static void DrawTileEditor(ATileset tileset, ATile tile, ref ATilesetEditorData data)
        {
           
            GUILayout.BeginVertical("helpBox");
            {
                if (DrawTileEditorHeader(tileset, tile, ref data))
                {
                    BeginChangeCheck();

                    DrawTileEditorVariables(tile, ref data);
                    if(tile.TileDriver.HasDrawTileProperties)
                       DrawTileEditorCustom(tileset, tile);
                    DrawTileEditorParameterContainer(tile, ref data);

                    EndChangeCheck();

                    DrawTileEditorVariations(tileset, tile, ref data);
                }
            }

            GUILayout.EndVertical();

        }

        private static void DrawTileEditorCustom(ATileset tileset, ATile tile)
        {
            tile.TileDriver.DrawTileProperties(tileset, tile);
        }

        private static bool DrawTileEditorHeader(ATileset tileset, ATile tile, ref ATilesetEditorData data)
        {
            var isDraw = true;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label($"Tile [{data.SelectedTile}]", EditorStyles.boldLabel);

               

                GUILayout.Space(SPACING);

                GUI.color = Color.red;
                if (GUILayout.Button("Remove"))
                {
                    tileset.RemoveTile(tile);
                    data.SelectedTile = 0;
                    isDraw = false;
                }

                GUI.color = Color.white;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label($"{tile.TileDriver.ID}", EditorStyles.boldLabel);

            GUILayout.Label($" {tile.TileDriver.Description}");

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            return isDraw;
        }

        private static void DrawTileEditorVariations(ATileset tileset, ATile tile, ref ATilesetEditorData data)
        {
            if (tile.Variations.Count == 0)
            {
                tile.AddVariation();
                _isDirty = true;
            }

            while (tile.Variations.Count > tile.Probabilities.Count)
            {
                tile.Probabilities.Add(1f);

                _isDirty = true;
            }

            while (tile.Variations.Count < tile.Probabilities.Count)
            {
                tile.Probabilities.RemoveAt(tile.Probabilities.Count - 1);
                _isDirty = true;
            }

            if (GUILayout.Button("Add variation"))
            {
                tile.AddVariation();

                _isDirty = true;
            }

            data.SelectedTileScroll =
                GUILayout.BeginScrollView(data.SelectedTileScroll, GUIStyle.none, GUI.skin.horizontalScrollbar);

            data.TilesWidth = (Screen.width - 150) / (tile.TileDriver.UVInTilesX * 50);
            data.TilesWidth = (int)(data.TilesWidth * 0.5f);

            var beginH = true;
            GUILayout.BeginHorizontal();

            for (int i = 0, tilesCounter = 0; i < tile.Variations.Count; i++)
            {
                if (!beginH && tilesCounter == 0)
                {
                    beginH = true;
                    GUILayout.BeginHorizontal();
                }

                tilesCounter++;

                GUILayout.BeginVertical();
                if (tile.TileDriver.DrawTileGUIPreview(tileset, tile, (byte)i))
                {
                    var index = i;
                    ATilesetSelector.Init(tileset, uv => { 

                        //Debug.Log($"{tile.ID} {uv.Min} {uv.Max} {i} {index}");
                        tile.TileDriver.SelectTile(tileset, uv, tile, index);
                        EditorUtility.SetDirty(tileset);
                    });
                }

                GUI.color = Color.red;

                if (tile.Variations.Count > 1)
                    if (GUILayout.Button($"{i} Remove"))
                    {
                        
                        tile.Variations.RemoveAt(i);
                        i--;

                        _isDirty = true;
                    }

                GUI.color = Color.white;

                tile.Probabilities[i] = EditorGUILayout.Slider(tile.Probabilities[i], 0, 1);

                GUILayout.EndVertical();

                if (beginH && tilesCounter >= data.TilesWidth)
                {
                    beginH = false;
                    GUILayout.EndHorizontal();
                    tilesCounter = 0;
                }
            }

            if (beginH)
                GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            GUILayout.EndScrollView();

        }

        private static void DrawTileEditorVariables(ATile tile, ref ATilesetEditorData data)
        {
            GUILayout.Label($"TileDriver: {tile.TileDriver.ID}");

            GUILayout.Space(10);

            tile.Prefab = EditorGUILayout.ObjectField(tile.Prefab, typeof(ATilePrefab),false) as ATilePrefab;
            tile.BitmaskMode = (BitmaskMode)EditorGUILayout.EnumPopup("Bitmask mode", tile.BitmaskMode);
            tile.ColliderDisabled = !EditorGUILayout.Toggle("Collider enabled", !tile.ColliderDisabled);
            tile.RandomVariations = EditorGUILayout.Toggle("variations enabled:", tile.RandomVariations);
           // tile.ID = (ushort)EditorGUILayout.IntField("ID:", tile.ID); // or refresh id's
           EditorGUILayout.LabelField($"ID: {tile.ID}");
        }

        private static void DrawTileEditorParameterContainer(ATile tile, ref ATilesetEditorData data)
        {
            tile.ParameterContainer ??= new ParameterContainer();

            var parameterContainer = tile.ParameterContainer;

            GUILayout.BeginVertical("helpBox");
            GUILayout.Label("parameters:");

            data.ShowHiddenParameters = GUILayout.Toggle(data.ShowHiddenParameters, "Show hidden parameters");

            for (int i = 0; i < parameterContainer.parameters.Count; i++)
            {
                if (parameterContainer.parameters[i].isHidden && !data.ShowHiddenParameters) continue;

                if (DrawParameter(parameterContainer.parameters[i]))
                {
                    parameterContainer.parameters.RemoveAt(i);
                    i--;
                }
            }

            GUILayout.BeginHorizontal();

            data.SelectedParameterType = (ParameterType)EditorGUILayout.EnumPopup(data.SelectedParameterType);

            if (GUILayout.Button("Add new parameter"))
            {
                parameterContainer.parameters.Add(new Parameter() { name = "newParam", type = data.SelectedParameterType });
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private static bool DrawParameter(Parameter param)
        {
            GUILayout.BeginVertical("helpBox");

            param.name = GUILayout.TextArea(param.name);

            param.type = (ParameterType)EditorGUILayout.EnumPopup("Type:", param.type);

            ParseParam(param);

            if (GUILayout.Button("Remove"))
                return true;

            GUILayout.EndVertical();

            return false;
        }

        private static void ParseParam(Parameter param)
        {
            switch (param.type)
            {
                case ParameterType.None:
                    GUILayout.Label("NONE");
                    break;
                case ParameterType.Int:
                    param.intValue = EditorGUILayout.IntField("Value", param.intValue);
                    break;
                case ParameterType.Float:
                    param.floatValue = EditorGUILayout.FloatField("Value", param.floatValue);
                    break;
                case ParameterType.Bool:
                    param.boolValue = EditorGUILayout.Toggle("Value", param.boolValue);
                    break;
                case ParameterType.Object:
                    param.objectValue = EditorGUILayout.ObjectField("Value", param.objectValue, typeof(UnityEngine.Object), false);
                    break;
                case ParameterType.String:
                    param.stringValue = EditorGUILayout.TextArea("Value", param.stringValue);
                    break;
                default:
                    break;
            }
        }

        private static void BeginChangeCheck() => EditorGUI.BeginChangeCheck();

        private static void EndChangeCheck()
        {
            _isDirty |= EditorGUI.EndChangeCheck();
        }

    }
}
