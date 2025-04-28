using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage cutsceneRawImage;
    public Canvas cutsceneCanvas;
    public RawImage CreditImage;
    public Canvas creditCanvas;
    public GameObject rewardUIPanel;
    public LoadingManager loadingManager;
    
    private PlayerInput playerInput;
    private bool isCutscenePlaying = false;
    public bool playingCreditsAfterVictory = false;
    private bool isCreditPlaying = false;
    private bool rewardUIActive = false;
    private float rewardTimer = 0f;
    private float rewardInputDelay = 0.5f;
    
    // Safety timer system
    private float creditsCutsceneMaxDuration = 68f; // Adjusted to your video length
    private float creditsTimer = 0f;
    private bool isMonitoringCredits = false;
    public bool hasCompletedBossFight = false; // Track if this is after boss fight
    public bool returnToTitleAfterCredits = false;

    private void Start()
    {
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(false);
            
        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(false);
            
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnCutsceneFinished;
            
        if (loadingManager == null)
            loadingManager = FindObjectOfType<LoadingManager>();
            
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                Debug.Log("Successfully hooked PlayCutscene input!");
                playerInput.actions["PlayCutscene"].performed += ctx => PlayCutscene();
            }
        }
    }

    private void Update()
    {
        // Monitor credits playback with safety timer
        if (isMonitoringCredits && isCreditPlaying)
        {
            creditsTimer += Time.unscaledDeltaTime;
            
            // Log progress periodically
            if (creditsTimer % 5f < Time.unscaledDeltaTime)
            {
                Debug.Log($"Credits cutscene playing: {creditsTimer:F1}s / {creditsCutsceneMaxDuration:F1}s");
            }
            
            // Safety timeout - force completion if video doesn't trigger event
            if (creditsTimer >= creditsCutsceneMaxDuration)
            {
                Debug.Log("Credits cutscene safety timeout reached - forcing completion");
                ShowRewardUIAfterCredits();
            }
        }
        
        // Handle reward UI key input
        if (rewardUIActive)
        {
            rewardTimer += Time.unscaledDeltaTime;
            
            // Only process input after a short delay to prevent accidental presses
            if (rewardTimer > rewardInputDelay)
            {
                // Check for any key press (Input.anyKeyDown is more reliable)
                if (Input.anyKeyDown)
                {
                    Debug.Log("Key pressed on reward UI - proceeding to shop");
                    HideRewardUI();
                    
                    // If this was after a boss fight, load shop scene
                    if (hasCompletedBossFight)
                    {
                        LoadShopScene();
                    }
                }
            }
        }
    }

    public void PlayCutscene()
    {
        if (isCutscenePlaying) return;

        Debug.Log("Playing Cutscene!");

        cutsceneRawImage.gameObject.SetActive(true);
        videoPlayer.Play();
        Time.timeScale = 0f;
        isCutscenePlaying = true;
    }

    private void OnCutsceneFinished(VideoPlayer vp)
    {
        Debug.Log($"OnCutsceneFinished called. playingCreditsAfterVictory: {playingCreditsAfterVictory}, isCreditPlaying: {isCreditPlaying}, returnToTitleAfterCredits: {returnToTitleAfterCredits}");
        
        if (playingCreditsAfterVictory)
        {
            Debug.Log("Victory Cutscene finished, now starting Credits Cutscene");
            playingCreditsAfterVictory = false;
            
            // This is for boss fight - play credits first, then show reward UI later
            PlayCreditCutscene(false); // Don't return to title after credits
            return;
        }

        if (isCreditPlaying)
        {
            Debug.Log("Credits Cutscene finished via event callback");

            isMonitoringCredits = false;
            isCreditPlaying = false;
            isCutscenePlaying = false;

            if (returnToTitleAfterCredits)
            {
                Debug.Log("Credits finished after PostPond - Going to Title Screen");
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
                return;
            }
            else
            {
                Debug.Log("Credits finished after Boss - Showing Reward UI to go to Shop");
                // Set this flag to track that we've completed a boss fight
                hasCompletedBossFight = true;
                ShowRewardUIAfterCredits();
                return;
            }
        }

        cutsceneRawImage.gameObject.SetActive(false);
        isCutscenePlaying = false;
        Debug.Log("Regular Cutscene Finished!");
        
        ShopInteractionManager shopManager = FindObjectOfType<ShopInteractionManager>();
        if (shopManager != null)
        {
            shopManager.ResumeDialogueAfterCutscene();
        }
        else
        {
            // For PostPond cutscene, if we've just watched the getting-eaten cutscene
            FishingProgress fishingProgress = FindObjectOfType<FishingProgress>();
            if (fishingProgress != null && fishingProgress.activeZoneType == InitiateMicrogames.FishZoneType.PostPond)
            {
                Debug.Log("PostPond regular cutscene finished - playing credits");
                PlayCreditCutscene(true); // This is a key change - pass true for returnToTitleAfterCredits
            }
            else
            {
                // Regular cutscene (non-boss, non-postpond)
                ShowRewardUI();
            }
        }
    }
    
    private void ShowRewardUIAfterCredits()
    {
        if (rewardUIPanel != null)
        {
            Debug.Log("Showing Reward UI after Credits!");

            // TURN OFF monitoring flags!
            isMonitoringCredits = false;
            isCreditPlaying = false;
            isCutscenePlaying = false;

            CanvasGroup rewardGroup = rewardUIPanel.GetComponent<CanvasGroup>();

            rewardUIPanel.SetActive(true);
            rewardUIActive = true;
            rewardTimer = 0f;
            Time.timeScale = 0f; // Keep game paused

            if (rewardGroup != null)
            {
                StartCoroutine(FadeInCanvasGroup(rewardGroup, 1.5f)); // <-- 1.5 seconds fade-in
            }
            else
            {
                Debug.LogWarning("Reward UI Panel does not have a CanvasGroup!");
            }
        }
        else
        {
            Debug.LogWarning("No RewardUIPanel found after Credits!");
            if (loadingManager != null)
                loadingManager.LoadSceneWithLoadingScreen("Shop");
        }
    }

    private void ShowRewardUI()
    {
        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(true);
            rewardUIActive = true;
            rewardTimer = 0f;
            hasCompletedBossFight = false; // Not a boss fight completion
        }
        else
        {
            Time.timeScale = 1f; // Resume game if no UI to show
        }
    }

    private void HideRewardUI()
    {
        if (rewardUIPanel != null)
        {
            CanvasGroup rewardGroup = rewardUIPanel.GetComponent<CanvasGroup>();

            if (rewardGroup != null)
            {
                StartCoroutine(FadeOutAndLoadScene(rewardGroup, 1.0f)); // 1 second fade-out
            }
            else
            {
                // If no canvas group, fallback immediately
                rewardUIPanel.SetActive(false);
                rewardUIActive = false;
                Time.timeScale = 1f;

                if (loadingManager != null)
                    loadingManager.LoadSceneWithLoadingScreen("Shop");
            }
        }
    }

    public bool IsCutscenePlaying()
    {
        return isCutscenePlaying;
    }

    public void PlayVictoryCutscene()
    {
        Debug.Log("PlayVictoryCutscene called");
        
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(true);
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            // Force a clean event connection
            videoPlayer.loopPointReached -= OnCutsceneFinished;
            videoPlayer.loopPointReached += OnCutsceneFinished;
            
            // This is a critical flag that triggers the credits to play after this cutscene
            playingCreditsAfterVictory = true;
            videoPlayer.Play();
            Time.timeScale = 0f; // Make sure game is paused
            isCutscenePlaying = true;
            
            Debug.Log("Victory cutscene started playing");
        }
    }

    public void PlayPostPondFinalCreditsThenTitle()
    {
        Debug.Log("PlayPostPondFinalCreditsThenTitle called - will go directly to title screen afterward");
        
        // Make sure we disable any previous UI elements
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(false);
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(false);
        
        // Show Credit Cutscene UI
        if (creditCanvas != null)
            creditCanvas.gameObject.SetActive(true);
        if (CreditImage != null)
            CreditImage.gameObject.SetActive(true);
        
        // Clear previous event handlers
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnCutsceneFinished;
            
            // Still try to set up the direct event handler (might work in some cases)
            videoPlayer.loopPointReached += OnPostPondCreditsComplete;
            
            // Play the credits
            videoPlayer.Play();
            isCutscenePlaying = true;
            
            // Start the timer coroutine - this is our guaranteed fallback
            StartCoroutine(PostPondCreditsTimer(68f)); // 68 seconds matches your creditsCutsceneMaxDuration
            
            Debug.Log("PostPond credits started with timer fallback, will go to title screen when complete");
        }
    }

    private IEnumerator PostPondCreditsTimer(float duration)
    {
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Use unscaledDeltaTime to work even if game is paused
            
            // Log progress occasionally
            if (timer % 10f < Time.unscaledDeltaTime)
            {
                Debug.Log($"PostPond credits timer: {timer:F1}s / {duration:F1}s");
            }
            
            yield return null;
        }
        
        Debug.Log("PostPond credits timer complete - loading title screen now");
        
        // Reset all relevant flags
        isCreditPlaying = false;
        isCutscenePlaying = false;
        isMonitoringCredits = false;
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Load title screen
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
    }

    // Special handler for PostPond credits completion that goes straight to title screen
    private void OnPostPondCreditsComplete(VideoPlayer vp)
    {
        Debug.Log("OnPostPondCreditsComplete called - loading title screen directly");
        
        // Remove event handler to prevent multiple calls
        vp.loopPointReached -= OnPostPondCreditsComplete;
        
        // Reset time scale
        Time.timeScale = 1f;
        
        // Stop all coroutines to prevent the timer from also triggering the scene load
        StopAllCoroutines();
        
        // Go directly to title screen
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
    }

        public void PlayVictoryCutsceneSimple()
    {
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(true);
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            // Important: do NOT trigger credits after
            playingCreditsAfterVictory = false;
            isCreditPlaying = false;

            videoPlayer.loopPointReached -= OnCutsceneFinished;
            videoPlayer.loopPointReached += OnCutsceneFinished;

            videoPlayer.Play();
            Time.timeScale = 0f;
            isCutscenePlaying = true;

            Debug.Log("Playing simple victory cutscene only (no credits)");
        }
    }

    public void PlayCreditCutscene(bool goToTitleScreen = false)
    {
        Debug.Log("PlayCreditCutscene called");

        isCreditPlaying = true;
        isCutscenePlaying = true;
        returnToTitleAfterCredits = goToTitleScreen; // <-- ADD THIS

        // Hide Victory Cutscene UI
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(false);
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(false);

        // Show Credit Cutscene UI
        if (creditCanvas != null)
            creditCanvas.gameObject.SetActive(true);
        if (CreditImage != null)
            CreditImage.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnCutsceneFinished;
            videoPlayer.loopPointReached += OnCutsceneFinished;
            
            isMonitoringCredits = true;
            creditsTimer = 0f;
            
            videoPlayer.Play();
            Debug.Log("Credits cutscene started playing with safety timer active");
        }
    }

    private void LoadShopScene()
    {
        Debug.Log("LoadShopScene called");
        
        if (loadingManager == null)
        {
            loadingManager = FindObjectOfType<LoadingManager>();
            if (loadingManager == null)
            {
                Debug.LogError("LoadingManager not found! Attempting direct scene load as fallback.");
                UnityEngine.SceneManagement.SceneManager.LoadScene("Shop");
                return;
            }
        }
        
        loadingManager.LoadSceneWithLoadingScreen("Shop");
    }

    private void OnDestroy()
    {
        // Clean up event subscription
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnCutsceneFinished;
        }
    }

    private IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("No CanvasGroup provided for fade-in!");
            yield break;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled so it works during paused time
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f; // Ensure fully visible
    }

    private IEnumerator FadeOutAndLoadScene(CanvasGroup canvasGroup, float duration)
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("No CanvasGroup provided for fade-out!");
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = 1f - t;  // Gradually fade out
            yield return null;
        }

        // Now AFTER fade is complete, disable
        canvasGroup.alpha = 0f;

        // Delay one more frame before disabling GameObject
        yield return null;

        rewardUIPanel.SetActive(false);
        rewardUIActive = false;
        Time.timeScale = 1f; // Resume game

        Debug.Log("Fade-out complete, now loading Shop scene...");

        if (loadingManager != null)
            loadingManager.LoadSceneWithLoadingScreen("Shop");
    }

    public void HideRewardUIAfterPondBoss()
    {
        if (rewardUIPanel != null)
        {
            CanvasGroup rewardGroup = rewardUIPanel.GetComponent<CanvasGroup>();

            if (rewardGroup != null)
            {
                StartCoroutine(FadeOutRewardUIOnly(rewardGroup, 1.0f)); // 1 second fade-out
            }
            else
            {
                rewardUIPanel.SetActive(false);
                rewardUIActive = false;
                Time.timeScale = 1f;

                // Disable cutscene canvas manually
                if (cutsceneCanvas != null)
                    cutsceneCanvas.gameObject.SetActive(false);
                if (cutsceneRawImage != null)
                    cutsceneRawImage.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator FadeOutRewardUIOnly(CanvasGroup canvasGroup, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        yield return null;

        rewardUIPanel.SetActive(false);
        rewardUIActive = false;
        Time.timeScale = 1f;

        // Disable cutscene canvas after fade
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(false);
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(false);

        Debug.Log("Reward UI after PondBoss faded out. Stayed in scene without loading shop.");
    }
}