using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncSceneLoader : MonoBehaviour
{
    public string sceneToLoad;
    public bool playCutsceneBeforeLoad = false;
    public bool playCutsceneAfterLoad = false;

    [Header("Audio")]
    public AudioSource musicSource;
    public float fadeDuration = 1.5f;

    [Header("UI Fade")]
    public Image blackScreenImage;  // Assign your UI > Image here (black)

    private CutsceneManager cutsceneManager;

    private void Start()
    {
        if (musicSource == null)
        {
            GameObject musicObject = GameObject.FindWithTag("Music");
            if (musicObject)
                musicSource = musicObject.GetComponent<AudioSource>();
        }

        cutsceneManager = FindObjectOfType<CutsceneManager>();

        if (blackScreenImage != null)
            blackScreenImage.color = new Color(0, 0, 0, 0); // Start transparent
    }

    public void StartSceneLoad()
    {
        StartCoroutine(LoadSceneAsync(sceneToLoad));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        if (playCutsceneBeforeLoad)
        {
            yield return FadeOutMusicAndScreen();
            yield return PlayCutsceneAndWait();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        if (playCutsceneAfterLoad)
        {
            yield return FadeOutMusicAndScreen();
            yield return PlayCutsceneAndWait();
        }

        asyncLoad.allowSceneActivation = true;
    }

    IEnumerator PlayCutsceneAndWait()
    {
        if (cutsceneManager == null)
        {
            Debug.LogWarning("CutsceneManager not found.");
            yield break;
        }

        cutsceneManager.PlayCutscene();

        while (cutsceneManager.IsCutscenePlaying())
        {
            yield return null;
        }
    }

    IEnumerator FadeOutMusicAndScreen()
    {
        float startVolume = (musicSource != null) ? musicSource.volume : 1f;
        float startAlpha = (blackScreenImage != null) ? blackScreenImage.color.a : 0f;

        float t = 0f;
        while (t < fadeDuration)
        {
            float lerp = t / fadeDuration;

            if (musicSource != null)
                musicSource.volume = Mathf.Lerp(startVolume, 0f, lerp);

            if (blackScreenImage != null)
            {
                Color color = blackScreenImage.color;
                color.a = Mathf.Lerp(startAlpha, 1f, lerp);
                blackScreenImage.color = color;
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (musicSource != null) musicSource.volume = 0f;
        if (blackScreenImage != null)
        {
            Color finalColor = blackScreenImage.color;
            finalColor.a = 1f;
            blackScreenImage.color = finalColor;
        }
    }
}
