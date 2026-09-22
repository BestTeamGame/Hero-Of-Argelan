using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // сюда перетащить Player
    public float smoothTime = 0.15f; // чем меньше, тем резче камера догоняет
    public Vector3 offset = new Vector3(0f, 1f, -10f); // -10 по Z обязательно для 2D

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}