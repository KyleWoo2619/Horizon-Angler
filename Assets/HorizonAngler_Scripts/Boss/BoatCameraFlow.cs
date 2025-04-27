using UnityEngine;

public class BoatCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -10f);
    public float followSpeed = 5f;

    [Header("Control")]
    public bool followEnabled = true; // NEW: you can toggle it ON/OFF

    void LateUpdate()
    {
        if (!followEnabled || !target) return; 

        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Only move XZ, keep Y height from boat + offset
        Vector3 smoothedPosition = new Vector3(
            Mathf.Lerp(transform.position.x, desiredPosition.x, followSpeed * Time.deltaTime),
            desiredPosition.y,  // Immediate match Y (wave bouncing)
            Mathf.Lerp(transform.position.z, desiredPosition.z, followSpeed * Time.deltaTime)
        );

        transform.position = smoothedPosition;
        transform.LookAt(target);
    }

    public void EnableFollow(bool enabled)
    {
        followEnabled = enabled;
    }

    void OnDisable()
    {
        // Reset follow position one last time
        followEnabled = false;
    }
}
