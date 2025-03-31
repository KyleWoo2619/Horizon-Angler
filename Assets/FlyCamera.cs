using UnityEngine;

public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;

    private float yaw;
    private float pitch;

    void Update()
    {
        // Mouse look
        yaw += lookSpeed * Input.GetAxis("Mouse X");
        pitch -= lookSpeed * Input.GetAxis("Mouse Y");
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        // WASD Movement
        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S
        float y = 0f;

        // Space = Up, Left Shift = Down
        if (Input.GetKey(KeyCode.Space)) y += 1;
        if (Input.GetKey(KeyCode.LeftShift)) y -= 1;

        Vector3 move = transform.right * x + transform.up * y + transform.forward * z;
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
