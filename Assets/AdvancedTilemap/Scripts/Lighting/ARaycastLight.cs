using UnityEngine;

namespace AdvancedTilemap.Lighting
{
	public class ARaycastLight : ALight
	{
		[Range(0f, 1f)]
		public float Resolution = 0.5f;

		public LayerMask ObstaclesMask;

		public float OffsetRay;

		protected Vector2 AngleToDirection(float angleDeg, bool isGlobal)
		{
			if (isGlobal)
				angleDeg += transform.eulerAngles.z;

			return new Vector2(Mathf.Sin(angleDeg * Mathf.Deg2Rad), Mathf.Cos(angleDeg * Mathf.Deg2Rad));
		}

		protected Vector2 OffsetDirection(Vector2 originPosition, Vector2 direction, float distance,float max)
		{
			var angle =  90- Utilites.AngleInDeg(originPosition, direction);

			return originPosition + AngleToDirection(angle,false) * (Mathf.Min(max, distance + OffsetRay));
		}

	}
}
