using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    [Header("Target Resolution Settings")]
    public int targetWidth = 1920;
    public int targetHeight = 1080;
    public bool fullscreen = true;

    private void Awake()
    {
        // Apply resolution once
        SetResolution();
        DontDestroyOnLoad(gameObject); // Optional if you want it persistent
    }

    private void SetResolution()
    {
        Screen.SetResolution(targetWidth, targetHeight, fullscreen);
        Debug.Log($"Resolution set to {targetWidth}x{targetHeight}, Fullscreen: {fullscreen}");
    }
}
