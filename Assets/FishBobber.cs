using UnityEngine;

public class FishBobber : MonoBehaviour
{
    public float bobHeight = 0.5f;       // How high the fish bobs
    public float bobSpeed = 1.5f;         // How fast the fish bobs

    private Vector3 startPos;             // Original starting position
    private float randomOffset;           // Random time offset for variety

    void Start()
    {
        startPos = transform.localPosition;
        randomOffset = Random.Range(0f, 2f * Mathf.PI); // Make each fish bob differently
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed + randomOffset) * bobHeight;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}
