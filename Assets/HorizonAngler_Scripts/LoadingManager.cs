using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingScreen;

    // This method is called to load a new scene with the loading screen
    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 3f;

        while (operation.progress < 0.9f)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure it takes at least minLoadTime
        while (timer < minLoadTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // ACTIVATE SCENE NOW
        operation.allowSceneActivation = true;
    }

}
