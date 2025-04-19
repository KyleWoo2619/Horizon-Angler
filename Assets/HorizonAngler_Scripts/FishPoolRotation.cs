using UnityEngine;

public class FishPoolRotation : MonoBehaviour
{
    public Transform centerPoint;  // The point around which fish will rotate (usually the FishPool itself)
    public float rotationSpeed = 20f;  // Degrees per second
    public float radius = 3f;  // How wide the circle is

    private Transform[] fishChildren;

    void Start()
    {
        // Get all child fish
        int fishCount = transform.childCount;
        fishChildren = new Transform[fishCount];
        for (int i = 0; i < fishCount; i++)
        {
            fishChildren[i] = transform.GetChild(i);

            // Set initial positions in a circle
            float angle = i * Mathf.PI * 2 / fishCount;
            Vector3 newPos = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            fishChildren[i].localPosition = newPos;
        }
    }

    void Update()
    {
        if (centerPoint == null)
            centerPoint = transform;  // Default to self if not assigned

        // Rotate each fish around center
        foreach (Transform fish in fishChildren)
        {
            fish.RotateAround(centerPoint.position, Vector3.up, rotationSpeed * Time.deltaTime);

            // Optional: Make fish always face forward
            fish.LookAt(centerPoint.position);
            fish.Rotate(0, 180, 0);  // Flip because LookAt faces *inward* normally
        }
    }
}
