using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    public class EditorTexButton
    {
        private GUIStyle _style;


        public EditorTexButton(int[] array, int w, Color a, Color h, Color n, Color b)
        {
            var normal = MKEditorStyles.GetTexture(array, w, n, b);
            var active = MKEditorStyles.GetTexture(array, w, a, b);
            var hover = MKEditorStyles.GetTexture(array, w, h, b);

            _style = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = normal
                },
                active = new GUIStyleState()
                {
                    background = active
                },
                hover = new GUIStyleState()
                {
                    background = hover
                }
            };
        }

        public GUIStyle GetStyle()
        {
            return _style;
        }

        public bool IsValid => _style.normal.background != null;
    }
}
