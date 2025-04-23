using UnityEngine;
using TMPro;
using StarterAssets;

public class ShopManager : MonoBehaviour
{
    public GameObject exitShopButton;
    private HAPlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<HAPlayerController>();
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
