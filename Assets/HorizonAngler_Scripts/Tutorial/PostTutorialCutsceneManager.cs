using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PostTutorialCutsceneManager : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public RawImage cutsceneRawImage;
    public string nextSceneName; // Scene to load after cutscene ends
    
    [Header("Post-Cutscene UI")]
    public GameObject rewardUIPanel;
    private bool rewardUIActive = false;
    private float rewardInputDelay = 0.5f;
    private float rewardTimer = 0f;
    
    private bool isCutscenePlaying = false;

    private void Start()
    {
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(false);
            
        if (rewardUIPanel != null)
            rewardUIPanel.SetActive(false);
            
        if (videoPlayer != null)
            videoPlayer.loopPointReached += OnCutsceneFinished;
    }

    private void Update()
    {
        // Handle input for reward UI
        if (rewardUIActive)
        {
            rewardTimer += Time.unscaledDeltaTime;
            if (rewardTimer > rewardInputDelay && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
            {
                HideRewardUI();
                if (!string.IsNullOrEmpty(nextSceneName))
                {
                    LoadNextScene();
                }
            }
        }
    }

    public void PlayCutscene()
    {
        if (isCutscenePlaying) return;
        
        Debug.Log("Playing Post-Tutorial Cutscene");
        
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(true);
            
        if (videoPlayer != null)
            videoPlayer.Play();
            
        Time.timeScale = 0f; // Pause the game
        isCutscenePlaying = true;
    }

    private void OnCutsceneFinished(VideoPlayer vp)
    {
        if (cutsceneRawImage != null)
            cutsceneRawImage.gameObject.SetActive(false);
            
        isCutscenePlaying = false;
        Debug.Log("Post-Tutorial Cutscene Finished");
        
        if (rewardUIPanel != null)
        {
            ShowRewardUI();
        }
        else
        {
            Time.timeScale = 1f; // Resume the game
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                LoadNextScene();
            }
        }
    }

    private void ShowRewardUI()
    {
        rewardUIPanel.SetActive(true);
        rewardUIActive = true;
        rewardTimer = 0f;
        // Keep time scale at 0 for reward UI
    }

    private void HideRewardUI()
    {
        rewardUIPanel.SetActive(false);
        rewardUIActive = false;
        Time.timeScale = 1f; // Resume game
    }
    
    private void LoadNextScene()
    {
        Debug.Log("Loading next scene: " + nextSceneName);

        LoadingManager loadingManager = FindObjectOfType<LoadingManager>();
        if (loadingManager != null)
        {
            loadingManager.LoadSceneWithLoadingScreen(nextSceneName);
        }
        else
        {
            Debug.LogWarning("LoadingManager not found! Falling back to direct scene load.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    
    public bool IsCutscenePlaying()
    {
        return isCutscenePlaying;
    }
}