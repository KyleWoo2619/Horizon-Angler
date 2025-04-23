using UnityEngine;
using UnityEngine.InputSystem;

public class ShopPauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject shopPauseCanvas;

    [Header("Input Action")]
    public InputAction pauseAction; // Single action for ESC, P, Start, etc.

    private bool isPaused = false;

    private void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;
        pauseAction.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        TogglePauseUI();
    }

    private void TogglePauseUI()
    {
        isPaused = !isPaused;

        if (shopPauseCanvas != null)
            shopPauseCanvas.SetActive(isPaused);

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
