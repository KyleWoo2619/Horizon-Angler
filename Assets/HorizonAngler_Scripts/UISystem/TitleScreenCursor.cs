using UnityEngine;

public class TitleScreenCursor : MonoBehaviour
{
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None; // Unlock the mouse
        Cursor.visible = true;                  // Show the mouse
    }
}
