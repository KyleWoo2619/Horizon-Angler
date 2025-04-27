using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RawImage cutsceneRawImage;
    public Canvas cutsceneCanvas;

    private PlayerInput playerInput;
    private bool isCutscenePlaying = false;

    private void Start()
    {
        cutsceneRawImage.gameObject.SetActive(false);

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

    private void PlayCutscene()
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
    }
}
