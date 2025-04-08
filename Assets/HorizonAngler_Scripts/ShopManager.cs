using UnityEngine;
using TMPro;
using StarterAssets;

public class ShopManager : MonoBehaviour
{
    public GameObject sampleText; // Normal idle message
    public GameObject dialogueText; // Dialogue box
    public GameObject exitShopButton;
    public GameObject[] conversationButtons; // Talk, Upgrade, etc.

    private HAPlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<HAPlayerController>();
    }

    public void StartTalk()
    {
        sampleText.SetActive(false);
        dialogueText.SetActive(true);

        foreach (GameObject btn in conversationButtons)
        {
            btn.SetActive(false);
        }

        exitShopButton.SetActive(false);
    }

    public void EndTalk()
    {
        dialogueText.SetActive(false);
        sampleText.SetActive(true);

        foreach (GameObject btn in conversationButtons)
        {
            btn.SetActive(true);
        }

        exitShopButton.SetActive(true);
    }

    public void ExitShop()
    {
        // Hide shop UI
        gameObject.SetActive(false);

        // Tell player to exit shop properly
        if (playerController != null)
        {
            playerController.ExitShop();
        }

        // Lock Mouse again
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
