// Create this as a new script: PostPondFadeIn.cs
using System.Collections;
using UnityEngine;

public class PostPondFadeIn : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 2f;
    
    void Start()
    {
        // Check if we should fade in
        if (TransitionManager.ShouldFadeInNextScene)
        {
            // Reset the flag
            TransitionManager.ShouldFadeInNextScene = false;
            
            // Make sure the canvas group is set to black (fully opaque)
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1f;
                StartCoroutine(FadeIn());
            }
            else
            {
                Debug.LogError("No CanvasGroup assigned for fade effect!");
            }
        }
        else
        {
            // If we're not transitioning from the final cutscene,
            // make sure the fade canvas is invisible
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }
        }
    }
    
    private IEnumerator FadeIn()
    {
        float startTime = Time.time;
        
        while (Time.time < startTime + fadeDuration)
        {
            float elapsedTime = Time.time - startTime;
            float normalizedTime = elapsedTime / fadeDuration;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, normalizedTime);
            yield return null;
        }
        
        // Ensure we reach zero opacity
        fadeCanvasGroup.alpha = 0f;
        Debug.Log("Fade-in complete");
    }
}