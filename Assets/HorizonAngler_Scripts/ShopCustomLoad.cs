using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ShopCustomLoad : MonoBehaviour
{
    public GameObject PondloadingScreen;
    public GameObject RiverloadingScreen;
    public GameObject OceanloadingScreen;
    public GameObject BPloadingScreen;
    public GameObject BossloadingScreen;

    // This method is called to load a new scene with the loading screen
    public void LoadPond(string sceneName)
    {
        StartCoroutine(PondLoadSceneAsync(sceneName));
    }

    public void LoadRiver(string sceneName)
    {
        StartCoroutine(RiverLoadSceneAsync(sceneName));
    }

    public void LoadOcean(string sceneName)
    {
        StartCoroutine(OceanLoadSceneAsync(sceneName));
    }
    public void LoadBP(string sceneName)
    {
        StartCoroutine(BPLoadSceneAsync(sceneName));
    }
    public void LoadBoss(string sceneName)
    {
        StartCoroutine(BossLoadSceneAsync(sceneName));
    }

    private IEnumerator PondLoadSceneAsync(string sceneName)
    {
        PondloadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 4f;

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

    private IEnumerator RiverLoadSceneAsync(string sceneName)
    {
        RiverloadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 4f;

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

    private IEnumerator OceanLoadSceneAsync(string sceneName)
    {
        OceanloadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 4f;

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

    private IEnumerator BPLoadSceneAsync(string sceneName)
    {
        BPloadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 4f;

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

    private IEnumerator BossLoadSceneAsync(string sceneName)
    {
        BossloadingScreen.SetActive(true);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float timer = 0f;
        float minLoadTime = 4f;

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
