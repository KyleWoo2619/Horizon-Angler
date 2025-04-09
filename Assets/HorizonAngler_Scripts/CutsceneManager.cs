using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage cutsceneRawImage;
    public Canvas cutsceneCanvas;

    [Header("Post-Cutscene UI")]     // <-- New
    public GameObject rewardUIPanel; // <-- Drag your "You got item!" UI here
    private bool rewardUIActive = false; 
    private float rewardInputDelay = 0.5f; 
    private float rewardTimer = 0f;

    private PlayerInput playerInput;
    private bool isCutscenePlaying = false;

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

    private void OnCutsceneFinished(VideoPlayer vp)
    {
        cutsceneRawImage.gameObject.SetActive(false);
        Time.timeScale = 1f; // Resume the game
        isCutscenePlaying = false;

        Debug.Log("Cutscene Finished!");

        ShowRewardUI(); // <-- NEW
    }

    private void ShowRewardUI()   // <-- NEW
    {
        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(true);
            rewardUIActive = true;
            rewardTimer = 0f;
            Time.timeScale = 0f;  // Pause game again while showing reward
        }
    }

    private void HideRewardUI()   // <-- NEW
    {
        if (rewardUIPanel != null)
        {
            rewardUIPanel.SetActive(false);
            rewardUIActive = false;
            Time.timeScale = 1f;  // Resume game
        }
    }
}
