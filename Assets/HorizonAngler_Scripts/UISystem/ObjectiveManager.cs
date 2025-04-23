using UnityEngine;
using TMPro;
using System.Collections;
using StarterAssets;

public class ObjectiveTextManager : MonoBehaviour
{
    public enum LevelType { Tutorial, Pond, River, Ocean, Boss }

    [Header("Level Type")]
    public LevelType currentLevel;

    [Header("References")]
    public CanvasGroup objectiveCanvas;
    public TextMeshProUGUI objectiveText;
    public CanvasGroup objectiveUpdateSign;

    private HAPlayerController player;
    private SaveData saveData;

    private void Start()
    {
        player = FindObjectOfType<HAPlayerController>();
        saveData = GameManager.Instance?.currentSaveData;

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
        if (saveData == null) return;

        switch (currentLevel)
        {
            case LevelType.Tutorial:
                objectiveText.text = "It's your day off. Time to relax and do what you love — go fish at the pond and enjoy the quiet.";
                break;

            case LevelType.Pond:
                if (saveData.hasTurnedInScroll)
                {
                    objectiveText.text = "Travel to the river for more exciting fishing.";
                }
                else if (saveData.hasCaughtPondBoss)
                {
                    objectiveText.text = "Talk to the shopkeeper about the strange scroll.";
                }
                else if (saveData.canFishPondBoss)
                {
                    objectiveText.text = "Check out the strange spot for newer fish.";
                }
                else
                {
                    objectiveText.text = "Enjoy fishing at the pond.";
                }
                break;

            case LevelType.River:
                if (saveData.hasTurnedInHair)
                {
                    objectiveText.text = "Travel to the ocean. There are more fishes awaiting.";
                }
                else if (saveData.hasCaughtRiverBoss)
                {
                    objectiveText.text = "Talk to the shopkeeper about the gruesome clump of hair.";
                }
                else if (saveData.canFishRiverBoss)
                {
                    objectiveText.text = "You feel something big swimming somewhere in the river...";
                }
                else
                {
                    objectiveText.text = "River currents are tricky. Time your casts!";
                }
                break;

            case LevelType.Ocean:
                objectiveText.text = "Strange glowing fish can be found here.";
                break;

            case LevelType.Boss:
                objectiveText.text = "This is it. Be ready for the final battle.";
                break;

            default:
                objectiveText.text = "Explore the waters.";
                break;
        }
    }

    public void SetScrollAcquired()
    {
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
