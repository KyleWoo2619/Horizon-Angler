using System.Collections;
using TMPro;
using UnityEngine;

public class LegendaryRewardManager : MonoBehaviour
{
    public CanvasGroup rewardCanvas;
    public TextMeshProUGUI rewardText;

    public void ShowReward(string source)
    {
        string message = source switch
        {
            "River" => "You found a gruesome clump of hair inside the malformed fish...",
            "Ocean" => "Out between the monstrous tentacles, you find a worn vertebrae...\nIt seems disturbingly human.",
            "BlackPond" => "The skeletal hand trembles as you grab onto it.\nIt's almost as if it wants to go somewhere... and knows the way."
        };

        rewardText.text = message;
        StartCoroutine(ShowThenHide());
    }

    private IEnumerator ShowThenHide()
    {
        rewardCanvas.gameObject.SetActive(true);
        yield return FadeCanvasGroup(rewardCanvas, 0, 1, 0.5f);
        yield return new WaitForSeconds(5f);
        yield return FadeCanvasGroup(rewardCanvas, 1, 0, 0.5f);
        rewardCanvas.gameObject.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvas, float from, float to, float duration)
    {
        float elapsed = 0f;
        canvas.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvas.alpha = to;
    }
}
