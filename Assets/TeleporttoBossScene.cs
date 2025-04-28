using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToBossScene : MonoBehaviour
{
    [Header("Settings")]
    public string bossSceneName = "Boss"; // Name of the Boss scene in Build Settings
    public KeyCode key1 = KeyCode.LeftShift;
    public KeyCode key2 = KeyCode.B;

    void Update()
    {
        // Check if both keys are being pressed
        if (Input.GetKey(key1) && Input.GetKeyDown(key2))
        {
            Debug.Log("[TeleportToBossScene] Key combination detected. Loading Boss Scene...");
            SceneManager.LoadScene(bossSceneName);
        }
    }
}
