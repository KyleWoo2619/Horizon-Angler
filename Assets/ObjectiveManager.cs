using UnityEngine;
using TMPro;
using System.Collections;
using StarterAssets;
using UnityEngine.ProBuilder.MeshOperations;

public class ObjectiveTextManager : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup objectiveCanvas;           // <-- New: Whole CanvasGroup for hiding/showing
    public TextMeshProUGUI objectiveText;
    public CanvasGroup objectiveUpdateSign;

    private HAPlayerController player;
    private bool scrollAcquired = false;

    private void Start()
    {
        player = FindObjectOfType<HAPlayerController>();
        UpdateObjective();
        
        if (objectiveUpdateSign != null)
            objectiveUpdateSign.alpha = 0f;
        
        if (objectiveCanvas != null)
            objectiveCanvas.alpha = 1f;
    }

    private void Update()
    {
        if (player == null) return;

        HandleObjectiveCanvasToggle();
    }

    private void HandleObjectiveCanvasToggle()
    {
        if (player.isFishing || player.isInShop)
        {
            if (objectiveCanvas != null)
                objectiveCanvas.alpha = 0f;
        }
        else
        {
            if (objectiveCanvas != null)
                objectiveCanvas.alpha = 1f;

            UpdateObjective();
        }
    }

    private void UpdateObjective()
    {
        if (player.hasTurnedInScroll)
        {
            objectiveText.text = "Travel to the river for more exciting fishing.";
        }
        else if (player.caughtPondBoss)
        {
            objectiveText.text = "Talk to the shopkeeper about the strange scroll.";
        }
        else if (player.canFishPondBoss)
        {
            objectiveText.text = "Check out the strange spot for newer fish.";
        }
        else
        {
            objectiveText.text = "Enjoy fishing in the pond.";
        }
    }

    public void SetScrollAcquired()
    {
        scrollAcquired = true;
        UpdateObjective();
        ShowObjectiveUpdateSign();
    }

    public void SetBossUnlocked()
    {
        UpdateObjective();
        ShowObjectiveUpdateSign();
    }

    private void ShowObjectiveUpdateSign()
    {
        if (objectiveUpdateSign != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeObjectiveUpdateSign());
        }
    }

    private IEnumerator FadeObjectiveUpdateSign()
    {
        objectiveUpdateSign.alpha = 1f;

        yield return new WaitForSeconds(1.5f);

        float fadeDuration = 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            objectiveUpdateSign.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        objectiveUpdateSign.alpha = 0f;
    }
}
