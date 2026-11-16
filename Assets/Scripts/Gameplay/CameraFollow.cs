using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Keeps the orthographic camera centered on a target with a configurable offset.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, -10f);
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private float minY = -1f;

        private Vector3 velocity;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public Vector3 Offset
        {
            get => offset;
            set => offset = value;
        }

        public float MinY
        {
            get => minY;
            set => minY = value;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float y = Mathf.Max(target.position.y + offset.y, minY);
            Vector3 desired = new Vector3(target.position.x + offset.x, y, offset.z);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        }
    }
}
