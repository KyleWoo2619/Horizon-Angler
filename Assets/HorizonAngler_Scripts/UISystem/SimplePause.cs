using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SimplePauseManager : MonoBehaviour
{
    [Header("Pause UI")]
    public GameObject pauseMenuPanel;
    
    [Header("Input")]
    public InputAction pauseAction;
    
    [Header("References")]
    public PostTutorialDialogueManager dialogueManager;
    
    private bool isPaused = false;
    private CutsceneManager cutsceneManager;
    
    private void Awake()
    {
        pauseAction.Enable();
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
            
        cutsceneManager = FindObjectOfType<CutsceneManager>();
        
        // If dialogue manager not assigned, try to find it
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<PostTutorialDialogueManager>();
    }
    
    private void OnEnable()
    {
        pauseAction.performed += ctx => TogglePause();
    }
    
    private void OnDisable()
    {
        pauseAction.performed -= ctx => TogglePause();
        pauseAction.Disable();
    }
    
    public void TogglePause()
    {
        // Don't allow pausing during cutscenes
        if (cutsceneManager != null && cutsceneManager.IsCutscenePlaying())
            return;
            
        isPaused = !isPaused;
        
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    
    private void PauseGame()
    {
        // Show cursor and make it interactive for menu navigation
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Pause time
        Time.timeScale = 0f;
        
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        
        // Disable dialogue interaction if dialogue manager exists
        if (dialogueManager != null)
        {
            dialogueManager.SetDialogueInteractionEnabled(false);
        }
    }
    
    public void ResumeGame()
    {
        // Hide cursor when returning to gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Re-enable dialogue interaction
        if (dialogueManager != null)
        {
            dialogueManager.SetDialogueInteractionEnabled(true);
        }
    }

    
    // Button callback methods for UI
    public void OnResumeButtonClicked()
    {
        ResumeGame();
    }
    
    public void OnMainMenuButtonClicked()
    {
        ResumeGame(); // Make sure to reset timeScale
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); // Replace with your main menu scene name
    }
    
    public void OnQuitButtonClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}