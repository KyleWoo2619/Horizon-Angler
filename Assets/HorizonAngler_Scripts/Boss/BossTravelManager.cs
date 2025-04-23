using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class BossTravelManager : MonoBehaviour
{
    public GameObject warningPanel;
    public TextMeshProUGUI warningText;
    public Button yesButton;
    public Button backButton;
    public CanvasGroup warningCanvasGroup;
    public float fadeDuration = 1f;

    private void Start()
    {
        warningPanel.SetActive(false);
        yesButton.onClick.AddListener(OnYesPressed);
        backButton.onClick.AddListener(() => warningPanel.SetActive(false));
    }

    public void ShowWarning()
    {
        warningText.text = "Are you ready to confront the terror of Horizon Angler?";
        warningPanel.SetActive(true);
        StartCoroutine(FadeCanvasGroup(warningCanvasGroup, 0f, 1f, fadeDuration));
    }

    private void OnYesPressed()
    {
        StartCoroutine(FadeAndLoadBossScene());
    }

    private IEnumerator FadeAndLoadBossScene()
    {
        yield return FadeCanvasGroup(warningCanvasGroup, 1f, 0f, fadeDuration);
        SceneManager.LoadScene("Boss"); // Replace with actual scene name
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration)
    {
        float elapsed = 0f;
        group.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        group.alpha = end;
    }
}
