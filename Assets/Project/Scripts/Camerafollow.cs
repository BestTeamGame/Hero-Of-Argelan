using UnityEngine;

namespace Project.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;         // ���� ���������� Player
        public float smoothTime = 0.15f; // ��� ������, ��� ����� ������ ��������
        public Vector3 offset = new(0f, 1f, -10f); // -10 �� Z ����������� ��� 2D

        private Vector3 velocity = Vector3.zero;

        void LateUpdate()
        {
            if (!target) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
    }
}