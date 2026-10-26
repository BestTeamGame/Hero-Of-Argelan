using UnityEngine;

namespace Project.Scripts.PlayerScripts
{
    public sealed class PlayerCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 0f, -10f);
        [SerializeField, Min(0f)] private float smoothTime = 0.15f;

        private Vector3 velocity;

        private void LateUpdate()
        {
            if (!target)
                return;

            Vector3 desiredPosition = target.position + offset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothTime
            );
        }
    }
}