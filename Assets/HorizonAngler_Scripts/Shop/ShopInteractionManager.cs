using System.Collections;
using UnityEngine;
using TMPro;

public class ShopInteractionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public GameObject specialButton;
    public TextMeshProUGUI specialButtonText;
    public TextMeshProUGUI textComponent;
    public GameObject dialogueButton;
    public GameObject upgradeButton;
    public GameObject shopBaseDialogue;

    [Header("Dialogue Settings")]
    public float textSpeed = 0.05f;

    [Header("Base Dialogue Content")]
    public string[] initialBaseDialogue;
    public string[] afterPondBossDialogue;
    public string[] afterRiverBossDialogue;
    public string[] afterOceanBossDialogue;
    public string[] afterDredgedHandDialogue;
    public string[] afterAllCollectedDialogue;
    public string[] finalStateDialogue;

    [Header("Special Dialogue Content")]
    public string[] scrollDialogue;
    public string[] hairDialogue;
    public string[] boneRodDialogue;
    public string[] handDialogue;
    public string[] vesselDialogue;
    public string[] finalFightDialogue;

    private SaveData saveData;
    private string[] currentLines;
    private int dialogueIndex;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene.");
            return;
        }

        saveData = SaveManager.Load();

        if (saveData == null)
        {
            Debug.LogError("Save data could not be loaded.");
            return;
        }

        UpdateShopState();
        dialoguePanel.SetActive(false);
        textComponent.text = string.Empty;
    }

    void Update()
    {
        if (dialoguePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (!isTyping || textComponent.text == currentLines[dialogueIndex])
            {
                NextLine();
            }
            else
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                    isTyping = false;
                }
                textComponent.text = currentLines[dialogueIndex];
            }
        }
    }

    // Call this method whenever save data changes
    public void UpdateShopState()
    {
        UpdateSpecialButton();
        UpdateUpgradeButton();
    }

    void UpdateSpecialButton()
    {
        // Hide the special button by default
        specialButton.SetActive(false);

        // Only show special button based on game progression
        if (saveData.hasCaughtPondBoss && !saveData.hasCaughtRiverBoss)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Ask about the strange scroll";
        }
        else if (saveData.hasCaughtRiverBoss && !saveData.hasCaughtOceanBoss)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Ask about... the hair.";
        }
        else if (saveData.hasCaughtOceanBoss && !saveData.dredgedHand)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Ask about the spine.";
        }
        else if (saveData.dredgedHand && !saveData.AllCollected)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Show the hand to the shopkeeper.";
        }
        else if (saveData.AllCollected && !saveData.dredgedHand)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Prepare the vessel.";
        }
        else if (saveData.dredgedHand)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Get ready for the final fight.";
        }
    }

    void UpdateUpgradeButton()
    {
        // You can implement conditions to enable/disable the upgrade button here
        // Example: upgradeButton.SetActive(someCondition);
        
        // You can also change the text of the upgrade button based on game progress
        // Example: upgradeButtonText.text = "New Text";
    }

    public void OnTalkButton()
    {
        string[] selectedDialogue = null;

        if (!saveData.hasCaughtPondBoss)
        {
            selectedDialogue = initialBaseDialogue;
        }
        else if (saveData.hasCaughtPondBoss && !saveData.hasCaughtRiverBoss)
        {
            selectedDialogue = afterPondBossDialogue;
        }
        else if (saveData.hasCaughtRiverBoss && !saveData.hasCaughtOceanBoss)
        {
            selectedDialogue = afterRiverBossDialogue;
        }
        else if (saveData.hasCaughtOceanBoss && !saveData.dredgedHand)
        {
            selectedDialogue = afterOceanBossDialogue;
        }
        else if (saveData.dredgedHand && !saveData.AllCollected)
        {
            selectedDialogue = afterDredgedHandDialogue;
        }
        else if (saveData.AllCollected && !saveData.dredgedHand)
        {
            selectedDialogue = afterAllCollectedDialogue;
        }
        else
        {
            selectedDialogue = finalStateDialogue;
        }

        // Randomize one line if the array is valid
        if (selectedDialogue != null && selectedDialogue.Length > 0)
        {
            string[] randomSingleLine = new string[1];
            randomSingleLine[0] = selectedDialogue[Random.Range(0, selectedDialogue.Length)];
            StartDialogue(randomSingleLine);
        }
        else
        {
            Debug.LogWarning("No dialogue lines found for current state.");
        }
    }


    public void OnSpecialButton()
    {
        if (saveData.hasCaughtPondBoss && !saveData.hasCaughtRiverBoss)
        {
            StartDialogue(scrollDialogue);
        }
        else if (saveData.hasCaughtRiverBoss && !saveData.hasCaughtOceanBoss)
        {
            StartDialogue(hairDialogue);
        }
        else if (saveData.hasCaughtOceanBoss && !saveData.dredgedHand)
        {
            StartDialogue(boneRodDialogue);
        }
        else if (saveData.dredgedHand && !saveData.AllCollected)
        {
            StartDialogue(handDialogue);
        }
        else if (saveData.AllCollected && !saveData.dredgedHand)
        {
            StartDialogue(vesselDialogue);
        }
        else if (saveData.dredgedHand)
        {
            StartDialogue(finalFightDialogue);
        }
    }

    public void OnUpgradeButton()
    {
        // Implement your upgrade logic here
        Debug.Log("Upgrade button pressed");
        
        // Example: Show different upgrade dialogue based on game progress
        // StartDialogue(upgradeDialogue);
    }

    // Dialogue Manager Functionality
    void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("Attempted to start dialogue with null or empty lines array");
            return;
        }
        
        currentLines = lines;
        dialogueIndex = 0;
        
        dialoguePanel.SetActive(true);
        shopBaseDialogue.SetActive(false);
        dialogueButton.SetActive(false);
        upgradeButton.SetActive(false);
        
        textComponent.text = string.Empty;
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        textComponent.text = string.Empty;
        
        foreach (char c in currentLines[dialogueIndex].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;
    }

    void NextLine()
    {
        dialogueIndex++;
        
        if (dialogueIndex >= currentLines.Length)
        {
            dialoguePanel.SetActive(false);
            shopBaseDialogue.SetActive(true);
            dialogueButton.SetActive(true);
            upgradeButton.SetActive(true);
        }
        else
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeLine());
        }
    }
}