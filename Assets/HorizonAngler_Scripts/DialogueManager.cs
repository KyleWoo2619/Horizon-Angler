using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueMangager : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public GameObject dialogueButton;
    public GameObject upgradeButton;
    public GameObject ShopBaseDialogue;
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
            ShopBaseDialogue.SetActive(true); // Show the dialogue UI
            dialogueButton.SetActive(true); // Show the button to exit dialogue
            upgradeButton.SetActive(true);
            gameObject.SetActive(false); // Hide the Dialogue UI
        }
    }
}
