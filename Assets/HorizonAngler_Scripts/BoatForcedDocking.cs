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

        Vector3 currentPosition = transform.position;
        Vector3 targetPos = new Vector3(targetPoint.position.x, currentPosition.y, targetPoint.position.z);

        transform.position = Vector3.MoveTowards(currentPosition, targetPos, moveSpeed * Time.deltaTime);

        Vector3 direction = (targetPos - currentPosition).normalized;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion currentRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(direction, transform.up);
            Vector3 euler = targetRotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentRotation.eulerAngles.x, 
                                                   Mathf.LerpAngle(currentRotation.eulerAngles.y, euler.y, rotateSpeed * Time.deltaTime),
                                                   currentRotation.eulerAngles.z);
        }

        if (Vector3.Distance(currentPosition, targetPos) < 0.1f)
        {
            isDocking = false;
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
            rb.isKinematic = false;
        }
    }

    public void EndDocking()
    {
        isDocking = false;
    }
}
