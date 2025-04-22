using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MapManager : MonoBehaviour
{
    public GameObject mapUI; // Drag your Map Canvas here
    private bool isMapOpen = false;
    private PlayerInput playerInput;

    public float zoomSpeed = 2f; 
    public float moveSpeed = 500f; 

    private RectTransform mapRectTransform;

    private void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("No PlayerInput found in scene!");
            return;
        }

        playerInput.actions["Map"].performed += ctx => ToggleMap();

        // Find the map image RectTransform
        mapRectTransform = mapUI.GetComponentInChildren<RectTransform>();
    }

    private void Update()
    {
        if (!isMapOpen) return;

        HandleZoom();
        HandlePan();
    }

    private void ToggleMap()
    {
        if (isMapOpen)
            CloseMap();
        else
            OpenMap();
    }

    public void OpenMap()
    {
        mapUI.SetActive(true);
        Time.timeScale = 0; 
        isMapOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseMap()
    {
        mapUI.SetActive(false);
        Time.timeScale = 1;
        isMapOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            mapRectTransform.localScale += Vector3.one * scroll * zoomSpeed * Time.unscaledDeltaTime;
            // Clamp zoom
            float clampedZoom = Mathf.Clamp(mapRectTransform.localScale.x, 0.5f, 2.5f);
            mapRectTransform.localScale = Vector3.one * clampedZoom;
        }
    }

    private void HandlePan()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 move = Mouse.current.delta.ReadValue();
            mapRectTransform.anchoredPosition += move * moveSpeed * Time.unscaledDeltaTime;
        }
    }
}
