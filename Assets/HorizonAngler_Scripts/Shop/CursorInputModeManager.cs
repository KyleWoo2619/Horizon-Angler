using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CursorInputModeManager : MonoBehaviour
{
    [Header("UI Setup")]
    public GameObject firstSelectedUI;

    private bool mouseMode = true;
    private bool hasFocusedUI = false;

    void Start()
    {
        SetMouseMode(true); // Start in mouse mode
    }

    void Update()
    {
        // Check for mouse movement or scroll or click
        if (Mouse.current != null && (
            Mouse.current.delta.ReadValue() != Vector2.zero ||
            Mouse.current.scroll.ReadValue().y != 0 ||
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame))
        {
            if (!mouseMode)
            {
                SetMouseMode(true);
            }
        }

        // Check for controller input only if currently in mouse mode
        if (mouseMode && Gamepad.current != null && (
            Gamepad.current.leftStick.ReadValue() != Vector2.zero ||
            Gamepad.current.dpad.ReadValue() != Vector2.zero ||
            Gamepad.current.buttonSouth.wasPressedThisFrame ||
            Gamepad.current.buttonNorth.wasPressedThisFrame ||
            Gamepad.current.buttonEast.wasPressedThisFrame ||
            Gamepad.current.buttonWest.wasPressedThisFrame))
        {
            SetMouseMode(false);
        }
    }

    void SetMouseMode(bool enable)
    {
        mouseMode = enable;

        if (enable)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Let UI interact via mouse
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (!hasFocusedUI && firstSelectedUI != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelectedUI);
                hasFocusedUI = true;
            }
        }
    }
}
