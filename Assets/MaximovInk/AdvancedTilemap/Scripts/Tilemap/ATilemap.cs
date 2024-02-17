using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.AdvancedTilemap
{
    [ExecuteAlways]
    public class ATilemap : MonoBehaviour
    {
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