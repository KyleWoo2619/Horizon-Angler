using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ShopPauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject shopPauseCanvas;

    [Header("UI Elements")]
    public GameObject firstSelectedButton;
    public GameObject encyclopedia;
    public GameObject encyBtnContainer;
    public GameObject pauseBtnContainer;
    public GameObject optionsMenu;
    public GameObject controlsMenu;
    public GameObject map;

    [Header("Encyclopedia")]
    public FishEncyclopediaUI encyclopediaUI;

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
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        // Update encyclopedia if it exists
        if (encyclopediaUI != null)
        {
            encyclopediaUI.UpdatePageUI();
        }

        // Show the pause UI
        shopPauseCanvas.SetActive(true);
        
        // No need to pause time in shop scene, but keep it for consistency
        Time.timeScale = 0;
        isPaused = true;

        // Show and unlock cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Set default selected UI button for controller navigation
        if (firstSelectedButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    public void ResumeGame()
    {
        // Hide pause UI and reset all menus
        shopPauseCanvas.SetActive(false);
        
        // Reset all sub-menus if they exist
        if (encyclopedia != null) encyclopedia.SetActive(false);
        if (encyBtnContainer != null) encyBtnContainer.SetActive(false);
        if (pauseBtnContainer != null) pauseBtnContainer.SetActive(true);
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (controlsMenu != null) controlsMenu.SetActive(false);
        if (map != null) map.SetActive(true);
        
        // Resume time
        Time.timeScale = 1;
        isPaused = false;

        // For shop, we want the cursor to remain visible and unlocked
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Reset encyclopedia if it exists
        if (encyclopediaUI != null)
        {
            encyclopediaUI.ResetToFirstPage();
        }
    }

    // Method for UI buttons to call
    public void OnResumeButtonClicked()
    {
        ResumeGame();
    }

    // Add methods for accessing sub-menus
    public void OpenEncyclopedia()
    {
        if (encyclopedia != null) encyclopedia.SetActive(true);
        if (encyBtnContainer != null) encyBtnContainer.SetActive(true);
        if (pauseBtnContainer != null) pauseBtnContainer.SetActive(false);
        if (map != null) map.SetActive(false);
    }

    public void OpenOptions()
    {
        if (optionsMenu != null) optionsMenu.SetActive(true);
        if (pauseBtnContainer != null) pauseBtnContainer.SetActive(false);
        if (map != null) map.SetActive(false);
    }

    public void OpenControls()
    {
        if (controlsMenu != null) controlsMenu.SetActive(true);
        if (pauseBtnContainer != null) pauseBtnContainer.SetActive(false);
        if (map != null) map.SetActive(false);
    }

    public void ReturnToPauseMenu()
    {
        if (encyclopedia != null) encyclopedia.SetActive(false);
        if (encyBtnContainer != null) encyBtnContainer.SetActive(false);
        if (optionsMenu != null) optionsMenu.SetActive(false);
        if (controlsMenu != null) controlsMenu.SetActive(false);
        if (pauseBtnContainer != null) pauseBtnContainer.SetActive(true);
        if (map != null) map.SetActive(true);
    }
}