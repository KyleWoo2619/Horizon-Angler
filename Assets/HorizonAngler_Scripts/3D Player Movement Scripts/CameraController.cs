using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Put this script on your player camera
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    private const float YMin = -50.0f;
    private const float YMax = 50.0f;

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    [Header("Transforms (Drag these in the Inspector)")]
    public Transform lookAt;
    public Transform Player;

    [Header("Inputs")]
    public string lookX = "Look X";
    public string lookY = "Look Y";
    public KeyCode keyToggleCursor = KeyCode.BackQuote;

    // Input Variables
    [HideInInspector] public float inputLookX = 0.0f;
    [HideInInspector] public float inputLookY = 0.0f;
    [HideInInspector] public bool inputKeyDownCursor = false;

    [Header("Sensitivity")]
    public float sensitivity = 10.0f;

    [Header("Camera Distance")]
    public float distance = 10.0f;

    [Header("Reference Variables")]
    public bool cursorActive = false;

    void Start()
    {
        SetLockCursor(false);
    }

    void Update()
    {
        ProcessInputs();
    }

    void LateUpdate()
    {
        CameraControls();
    }

    void ProcessInputs()
    {
        inputLookX = Input.GetAxis(lookX);
        inputLookY = Input.GetAxis(lookY);
        inputKeyDownCursor = Input.GetKeyDown(keyToggleCursor);

        if (inputKeyDownCursor )
        {
            ToggleLockCursor();
        }
    }

    void CameraControls()
    {
        currentX += inputLookX * sensitivity * Time.deltaTime;
        currentY += inputLookY * -1 * sensitivity * Time.deltaTime;

        currentY = Mathf.Clamp(currentY, YMin, YMax);

        Vector3 Direction = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        transform.position = lookAt.position + rotation * Direction;

        transform.LookAt(lookAt.position);
    }

    // lock/hide or show/unlock cursor
    public void SetLockCursor(bool doLock)
    {
        cursorActive = doLock;
        RefreshCursor();
    }

    void ToggleLockCursor()
    {
        cursorActive = !cursorActive;
        RefreshCursor();
    }

    void RefreshCursor()
    {
        if (!cursorActive && Cursor.lockState != CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.Locked; }
        if (cursorActive && Cursor.lockState != CursorLockMode.None) { Cursor.lockState = CursorLockMode.None; }
    }
}
