using System;
using UnityEngine;

namespace MaximovInk.MKRendering
{
    [Serializable]
    public struct MKRenderingSettings
    {
        public LayerMask LightingMask;
        public Material LightMeshMaterial;

        [Range(0,75)]
        public float EditorUpdateRateFPS;
        [Range(0,75)]
        public float RuntimeUpdateRateFPS;
    }

    [ExecuteAlways]
    public class MKRenderingManager : MonoBehaviourSingletonAuto<MKRenderingManager>
    {
        public MKRenderingSettings Settings => _settings;
        [SerializeField]
        private MKRenderingSettings _settings;

        private MKCamera[] _cameras;

        private void Awake()
        {
            FindAllCameras();
        }

        private void FindAllCameras()
        {
            _cameras = FindObjectsByType<MKCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        private void OnValidate()
        {
            FindAllCameras();

            foreach (var cam in _cameras)
            {
                cam.UpdateView();
            }
        }
    }
}
