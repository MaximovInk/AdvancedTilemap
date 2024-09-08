using UnityEngine;

namespace MaximovInk.PixelLight
{
    public class PixelRaycastLight2D : PixelLight2D
    {
        [SerializeField, Range(0.02f, 1f)]
        protected float _resolution = 0.5f;
        [SerializeField] 
        protected float _offsetRay;
        [SerializeField]
        protected LayerMask _obstaclesMask;


        protected Vector2 AngleToDirection(float angleDeg)
        {
            angleDeg += transform.eulerAngles.z;

            return Utility.AngleToDirection(angleDeg);
        }
    }
}
