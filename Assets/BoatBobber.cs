using UnityEngine;

public class BoatBobbing : MonoBehaviour
{
    [Header("Position Bobbing")]
    public float heightAmplitude = 0.25f;
    public float heightFrequency = 1f;

    [Header("Rotation Bobbing")]
    public float tiltAmplitude = 5f; // In degrees
    public float tiltFrequency = 0.5f;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        float bobOffset = Mathf.Sin(Time.time * heightFrequency) * heightAmplitude;

        // Position: up & down
        transform.localPosition = startPos + new Vector3(0f, bobOffset, 0f);

        // Rotation: front-to-back (pitch) and side-to-side (roll)
        float pitch = Mathf.Sin(Time.time * tiltFrequency) * tiltAmplitude;
        float roll = Mathf.Cos(Time.time * (tiltFrequency * 0.75f)) * (tiltAmplitude * 0.5f);

        Quaternion tilt = Quaternion.Euler(pitch, 0f, roll);
        transform.localRotation = startRot * tilt;
    }
}
