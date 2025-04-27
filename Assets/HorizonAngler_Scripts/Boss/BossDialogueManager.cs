using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.LowLevel;  // For Image

public class BossDialogueManager : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public GameObject ShopBaseDialogue;
    public GameObject dialogueButton;
    public GameObject upgradeButton;

    public Image fishingRodIcon;         // <-- Drag your Fishing Rod UI Image here
    public Sprite upgradedRodSprite;     // <-- Drag your new upgraded sprite here

    public TextMeshProUGUI fishingRodNameText;
    public string upgradedRodName = "Enhanced Fishing Rod";

    public string[] lines;
    public float textSpeed;
    private int index;

    void Start()
    {
        textComponent.text = string.Empty;
    }

    void OnEnable()
    {
        StartDialogue();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    void StartDialogue()
    {
        index = 0;
        textComponent.text = string.Empty;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            StartCoroutine(TypeLine());
        }
        else
        {
            FinishDialogue();
        }
    }

    void FinishDialogue()
    {
        ShopBaseDialogue.SetActive(true);
        dialogueButton.SetActive(true);
        upgradeButton.SetActive(true);

        // Swap the fishing rod sprite!
        if (fishingRodIcon != null && upgradedRodSprite != null)
        {
            fishingRodIcon.sprite = upgradedRodSprite;
            Debug.Log("Fishing Rod upgraded!");
        }
        else
        {
            Debug.LogWarning("Fishing rod icon or new sprite missing!");
        }

        if (fishingRodNameText != null)
        {
            fishingRodNameText.text = upgradedRodName; // Update the name text
            Debug.Log("Fishing Rod name updated!");
        }
        else
        {
            Debug.LogWarning("Fishing rod name text component missing!");
        }

        HAPlayerController player = FindObjectOfType<HAPlayerController>();
        if (player != null)
        {
            player.hasTurnedInScroll = true; // Set the variable to true
            Debug.Log("Player has turned in the scroll!");
        }
        else
        {
            Debug.LogWarning("Player not found in the scene!");
        }

        gameObject.SetActive(false); // Hide the dialogue
    }
}
