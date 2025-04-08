using UnityEngine;
using UnityEngine.InputSystem; // Needed for PlayerInput
using UnityEngine.EventSystems; // Optional: for setting default selected button

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Drag your Pause Canvas here
    private bool isPaused = false;
    private PlayerInput playerInput; // Your Input System reference
    public GameObject firstSelectedButton; // The button to select when the menu opens

    private void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("No PlayerInput found in scene!");
            return;
        }

        playerInput.actions["Pause"].performed += ctx => TogglePause(); // Connect Pause button
    }

    private void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true); // Show the pause UI
        Time.timeScale = 0; // Pause gameplay
        isPaused = true;

        Cursor.lockState = CursorLockMode.None; // Free the mouse
        Cursor.visible = true;

        // Optionally: set a default selected UI button
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        // EventSystem.current.SetSelectedGameObject(yourFirstButton); // <-- If you want keyboard/controller to highlight
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // Hide the pause UI
        Time.timeScale = 1; // Resume gameplay
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked; // Lock mouse again
        Cursor.visible = false;
    }
}
