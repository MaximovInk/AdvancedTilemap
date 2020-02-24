using UnityEngine;

namespace AdvancedTilemap.Extra
{
	[ExecuteAlways]
	public class TransformChangedEvent : MonoBehaviour
	{
		public delegate void OnTransformChanged();

		public event OnTransformChanged TransformChanged;

		private Vector3 lastPos;
		private Quaternion lastRot;
		private Vector3 lastScale;

		private void Start()
		{
			DoCacheTransformData();
		}

		private void DoCacheTransformData()
		{
			lastPos = transform.position;
			lastRot = transform.rotation;
			lastScale = transform.localScale;
		}

		private void Update()
		{
			if (lastPos != transform.position || lastRot != transform.rotation || lastScale != transform.localScale)
			{
				DoCacheTransformData();
				TransformChanged?.Invoke();
			}
		}
	}
}