using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public Animator boatAnimator; // Assign your boat animation or video here

    // This method is called to load a new scene with the loading screen
    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true);
        boatAnimator.SetBool("isLoading", true); // Trigger your animation

        // Load the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // Optionally, update a progress bar or animation speed
            // float progress = Mathf.Clamp01(operation.progress / 0.9f);

            float minLoadTime = 3f; // Set your desired minimum load duration here
            float timer = 0f;

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

        }

        loadingScreen.SetActive(false);
        boatAnimator.SetBool("isLoading", false); // Stop your animation
    }
}
