using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Drag the Player object here

    [Header("Settings")]
    public float smoothSpeed = 0.125f; // Higher = faster, Lower = smoother
    public Vector3 offset = new Vector3(0, 0, -10); // Keeps camera back so it can see 2D objects

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate where the camera WANTS to be
        Vector3 desiredPosition = target.position + offset;

        // Smoothly move from current position to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Apply the new position
        transform.position = smoothedPosition;
    }
}