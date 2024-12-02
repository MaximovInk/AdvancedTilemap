using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public static class GlobalSettings
    {
        public static Color TilemapColliderColor
        {
            get => Utilites.IntToColor32(PlayerPrefs.GetInt("ATilemapGCColor", Utilites.Color32ToInt(new Color32(0, 255, 0, 64))));
            set => PlayerPrefs.SetInt("ATilemapGCColor", Utilites.Color32ToInt(value));
        }

        public static Color TilemapGridColor
        {
            get => Utilites.IntToColor32(PlayerPrefs.GetInt("ATilemapGColor", Utilites.Color32ToInt(new Color32(13, 255, 234, 50))));
            set => PlayerPrefs.SetInt("ATilemapGColor", Utilites.Color32ToInt(value));
        }

        public static bool ShowGrid
        {
            get => PlayerPrefs.GetInt("ATilemapShowGrid", 0) != 0;
            set => PlayerPrefs.SetInt("ATilemapShowGrid", value ? 1 : 0);
        }
    }
}
