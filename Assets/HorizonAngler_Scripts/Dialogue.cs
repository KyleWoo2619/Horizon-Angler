using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public string[] lines;
    public float textSpeed;

    public AudioSource endDialogueSound; // Sound played when dialogue ends

    private int index;

    void Start()
    {
        textComponent.text = string.Empty;
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
            StartCoroutine(HandleDialogueEnd());
        }
    }

    IEnumerator HandleDialogueEnd()
    {
        if (endDialogueSound != null)
        {
            endDialogueSound.Play(); // Play sound
        }

        yield return new WaitForSeconds(1f); // Shorter delay (you can adjust this)

        // Hide dialogue and reset for next time
        gameObject.SetActive(false);    // <-- Hide the Dialogue UI
        index = 0;                      // <-- Reset dialogue
        textComponent.text = string.Empty; 
    }
}
