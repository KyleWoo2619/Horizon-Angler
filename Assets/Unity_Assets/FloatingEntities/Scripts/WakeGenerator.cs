using UnityEngine;

public class WakeGenerator : MonoBehaviour
{
    public Vector3 offset;
    public OceanAdvancedURP oceanURP;  // Drag & drop this in Inspector

    private Vector3 lastPosition;
    private float speed;

    void Awake()
    {
        lastPosition = transform.position;
        speed = 0.0f;
    }

    void Update()
    {
        speed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        if (Time.time % 0.2f < 0.01f && oceanURP != null)
        {
            Vector3 p = transform.position + transform.rotation * offset;
            float waterY = oceanURP.GetWaterHeight(p);
            if (waterY > p.y)
            {
                // Placeholder: register splash here if you plan to implement that in URP
                // e.g., oceanURP.RegisterInteraction(p, Mathf.Clamp01(speed / 15f) * 0.5f);
                Debug.DrawLine(p, p + Vector3.up * 2, Color.cyan, 1f);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.rotation * offset, 0.5f);
    }
}
