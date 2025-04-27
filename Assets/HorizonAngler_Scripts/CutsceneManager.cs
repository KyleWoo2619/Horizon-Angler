using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage cutsceneRawImage;
    public Canvas cutsceneCanvas;
    public RawImage CreditImage;
    public Canvas creditCanvas;

    [Header("Post-Cutscene UI")]     // <-- New
    public GameObject rewardUIPanel; // <-- Drag your "You got item!" UI here
    private bool rewardUIActive = false; 
    private float rewardInputDelay = 0.5f; 
    private float rewardTimer = 0f;

    private PlayerInput playerInput;
    private bool isCutscenePlaying = false;

    private bool playingCreditsAfterVictory = false;
    private LoadingManager loadingManager; // For loading shop after credits
    private bool isCreditPlaying = false;

    private void Start()
    {
        cutsceneRawImage.gameObject.SetActive(false);
        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(false); // Make sure it's hidden initially

        videoPlayer.loopPointReached += OnCutsceneFinished;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                Debug.Log("Successfully hooked PlayCutscene input!");
                playerInput.actions["PlayCutscene"].performed += ctx => PlayCutscene();
            }
            else
            {
                Debug.LogError("PlayerInput component missing on Player!");
            }
        }
        else
        {
            Debug.LogError("Player GameObject not found! Make sure Player is tagged correctly.");
        }

        loadingManager = FindObjectOfType<LoadingManager>();
    }

    private void Update()
    {
        if (rewardUIActive)
        {
            rewardTimer += Time.unscaledDeltaTime;  // Important! Cutscene is paused so use unscaled time
            if (rewardTimer > rewardInputDelay && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                HideRewardUI();
            }
        }
    }

    public void PlayCutscene()
    {
        if (isCutscenePlaying) return;

        Debug.Log("Playing Cutscene!");

        cutsceneRawImage.gameObject.SetActive(true);
        videoPlayer.Play();
        Time.timeScale = 0f; // Pause the game
        isCutscenePlaying = true;
    }

    // In CutsceneManager.cs, update the OnCutsceneFinished method:
    private void OnCutsceneFinished(VideoPlayer vp)
    {
        if (playingCreditsAfterVictory)
        {
            Debug.Log("Victory Cutscene finished, now starting Credits Cutscene");

            playingCreditsAfterVictory = false; // Reset flag
            PlayCreditCutscene();
            return; // Stop here, don't do reward UI yet
        }

        if (isCreditPlaying)
        {
            Debug.Log("Credits Cutscene finished, loading Shop scene...");
            if (loadingManager != null)
                loadingManager.LoadSceneWithLoadingScreen("Shop");
            else
                Debug.LogError("LoadingManager not found!");

            return;
        }

        cutsceneRawImage.gameObject.SetActive(false);
        isCutscenePlaying = false;
        Debug.Log("Cutscene Finished!");
        
        // Resume normal flow if no credits
        ShopInteractionManager shopManager = FindObjectOfType<ShopInteractionManager>();
        if (shopManager != null)
        {
            shopManager.ResumeDialogueAfterCutscene();
        }
        ShowRewardUI();
    }

    // And update ShowRewardUI to handle if there's no reward panel:
    private void ShowRewardUI()
    {
        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(true);
            rewardUIActive = true;
            rewardTimer = 0f;
            Time.timeScale = 0f;  // Keep game paused while showing reward
        }
        else
        {
            // If there's no reward panel, just resume time
            Time.timeScale = 1f;
        }
    }

    private void HideRewardUI()
    {
        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(false);
            rewardUIActive = false;
            Time.timeScale = 1f;  // Resume game
        }
    }

    public bool IsCutscenePlaying()
    {
        return isCutscenePlaying;
    }

    // Add this alternative method for direct play from other scripts
    public void PlayVictoryCutscene()
    {
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(true);
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            playingCreditsAfterVictory = true; // Set flag
            videoPlayer.Play();
        }
    }

    public void PlayCreditCutscene()
    {
        isCreditPlaying = true;

        // --- Hide Victory Cutscene UI ---
        if (cutsceneCanvas != null)
            cutsceneCanvas.gameObject.SetActive(false);
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(false);

        // --- Show Credit Cutscene UI ---
        if (creditCanvas != null)
            creditCanvas.gameObject.SetActive(true);
        if (CreditImage != null)
            CreditImage.gameObject.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }


}
