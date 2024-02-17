using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [ExecuteAlways]
    public class ATilemap : MonoBehaviour
    {
        public const int LIQUID_DEAD_Y = -100;

        public int SortingOrder { get { return sortingOrder; } set { var changed = sortingOrder != value; sortingOrder = value; if(changed) UpdateRenderer(); } }
        public bool UndoEnabled { get { return undoEnabled; } set { var changed = undoEnabled != value; undoEnabled = value; if(changed) UpdateUndoStack(); } }
        public bool AutoTrim
        {
            get => _autoTrim; set
            {
               var changed = AutoTrim != value;
                _autoTrim = value;
                if (changed) Trim();
            }
        }

        public bool DisplayChunksInHierarchy
        {
            get { return displayChunksHierarchy; }
            set
            {
                var changed = displayChunksHierarchy != value;
                displayChunksHierarchy = value;
                if (changed)
                    UpdateChunksFlags();
            }
        }

        public float LiquidStepsDuration { get; set; } = 0.1f;

        [SerializeField]
        private bool displayChunksHierarchy = true;
        [SerializeField]
        private int sortingOrder;
        [SerializeField]
        private bool undoEnabled;
        [SerializeField]
        private bool _autoTrim;

        public void Refresh(bool immediate = false)
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.Refresh(immediate);
            }
        }

        private void Trim()
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.Trim();
            }

        }

        private void UpdateUndoStack()
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateUndoStack();
            }
        }

        private void UpdateRenderer()
        {
            foreach(ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateRenderer();
            }
        }

        private void UpdateChunksFlags()
        {
            foreach (ALayer layer in layers)
            {
                if (layer == null) continue;

                layer.UpdateChunksFlags();
            }
        }

        public List<ALayer> layers = new List<ALayer>();

        public ALayer MakeLayer()
        {
            var go = new GameObject();
            var layer = go.AddComponent<ALayer>();

            go.name = $"layer{layers.Count}";
            layer.transform.SetParent(transform);
            layer.transform.localPosition = Vector3.zero;
            layer.Tilemap = this;
            layers.Add(layer);

            return layer;
        }

        public void RemoveLayer(int index)
        {
            DestroyImmediate(layers[index].gameObject);
            layers.RemoveAt(index);
        }

        public void TrimAll(bool immediate = false)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (immediate)
                    layers[i].Trim();
                else
                    layers[i].TrimInvoke = true;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                    layers[i].Clear();
            }
        }
    }
}