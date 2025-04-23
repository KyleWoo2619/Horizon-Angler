using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LegacyPauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;

    [Header("Input Action")]
    public InputAction pauseAction; // This will show '+' in inspector and allow rebinding like your screenshot

    private bool isPaused = false;

    void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPausePerformed;
    }

    void OnDisable()
    {
        pauseAction.performed -= OnPausePerformed;
        pauseAction.Disable();
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(isPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    public void QuitToMainMenu(string sceneName = "Title Screen")
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
