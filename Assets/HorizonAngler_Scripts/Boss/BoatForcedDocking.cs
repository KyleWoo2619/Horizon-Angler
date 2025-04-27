using UnityEngine;

public class BoatForcedDocking : MonoBehaviour
{
    public Transform targetPoint;
    public float moveSpeed = 5f;
    public float rotateSpeed = 2f;
    public bool isDocking = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isDocking || targetPoint == null) return;

        // Get current position
        Vector3 currentPosition = transform.position;
        
        // Set target position (only X and Z, preserve Y)
        Vector3 targetPos = new Vector3(targetPoint.position.x, currentPosition.y, targetPoint.position.z);
        
        // Move towards target, preserving Y position
        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPos, moveSpeed * Time.deltaTime);
        transform.position = newPosition;

        // Calculate direction to face (in X and Z plane only)
        Vector3 direction = (targetPos - currentPosition).normalized;
        
        // Only rotate if we have a valid direction vector
        if (direction.sqrMagnitude > 0.001f)
        {
            // Calculate target rotation for Y-axis only (to face the target)
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            
            // Get current rotation
            Quaternion currentRotation = transform.rotation;
            
            // Smoothly rotate only the Y component
            float newYRotation = Mathf.LerpAngle(currentRotation.eulerAngles.y, 
                                               targetRotation.eulerAngles.y, 
                                               rotateSpeed * Time.deltaTime);
            
            // Apply rotation (keeping X and Z unchanged)
            transform.rotation = Quaternion.Euler(
                currentRotation.eulerAngles.x, 
                newYRotation,
                currentRotation.eulerAngles.z
            );
        }

        // Check if we've reached target in X-Z plane
        float distanceXZ = Vector2.Distance(
            new Vector2(currentPosition.x, currentPosition.z),
            new Vector2(targetPos.x, targetPos.z)
        );
        
        if (distanceXZ < 0.1f)
        {
            isDocking = false;
            Debug.Log("Reached target position in XZ plane, docking complete");
        }
    }

    public void BeginDocking(Transform newTarget)
    {
        targetPoint = newTarget;
        isDocking = true;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        Debug.Log("Beginning docking to target: " + newTarget.position);
    }

    public void EndDocking()
    {
        isDocking = false;
        Debug.Log("Ending docking");
    }
}