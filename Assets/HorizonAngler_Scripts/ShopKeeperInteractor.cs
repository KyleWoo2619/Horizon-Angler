using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class ShopKeeperInteractor : MonoBehaviour
{
    [SerializeField] private GameObject interactPromptUI;
    [SerializeField] private GameObject shopCanvas;
    [SerializeField] private Transform shopCameraTarget; // <-- Drag your shop camera look transform here
    [SerializeField] private HAPlayerController playerController;

    private bool canInteract = false;
    private PlayerInput playerInput;

    private void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();

        if (playerInput != null)
        {
            playerInput.actions["Interact"].performed += ctx => TryOpenShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            interactPromptUI.SetActive(false);
        }
    }

    private void TryOpenShop()
    {
        if (!canInteract) return;

        interactPromptUI.SetActive(false);
        shopCanvas.SetActive(true);

        // Save original camera
        playerController.SaveCameraState();

        // Move camera to shop look position
        playerController.SetCameraPosition(shopCameraTarget.position, shopCameraTarget.rotation);

        // Enter shop mode
        playerController.EnterShop();

        // Lock camera rotation (no mouse movement while in shop)
        playerController.LockCameraPosition = true;

        // Unlock Mouse
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }
}
