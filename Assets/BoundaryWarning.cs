using UnityEngine;
using UnityEngine.UI;

public class ForbiddenDirectionWarning : MonoBehaviour
{
    [Header("References")]
    public Transform playerShip;
    public CanvasGroup warningCanvas;    // For rotation warning ("No Light Lies Beyond")
    public CanvasGroup blackoutCanvas;   // Black screen fade
    public CanvasGroup boundaryCanvas;   // Boundary crossing warning

    [Header("Settings")]
    public float maxBlackoutTime = 20f;        // Time (seconds) to reach full blackout
    public float blackoutFadeOutSpeed = 1f;    // How fast darkness timer fades when safe
    public float boundaryFadeSpeed = 2f;       // How fast boundary text fades in/out

    private float darknessTimer = 0f;
    private bool insideBoundary = false;

    void Update()
    {
        if (playerShip == null) return;

        float yRotation = NormalizeAngle(playerShip.eulerAngles.y);
        float absY = Mathf.Abs(yRotation);

        // --- Warning Text: Fade based on facing forbidden direction ---
        if (absY <= 90f)
        {
            warningCanvas.alpha = Mathf.Clamp01(1f - (absY / 90f));
            darknessTimer += Time.deltaTime;
        }
        else
        {
            warningCanvas.alpha = 0f;
            darknessTimer -= Time.deltaTime * blackoutFadeOutSpeed;
        }

        // --- Blackout Darkness ---
        darknessTimer = Mathf.Clamp(darknessTimer, 0f, maxBlackoutTime);
        blackoutCanvas.alpha = darknessTimer / maxBlackoutTime;

        // --- Boundary Canvas Fade ---
        if (insideBoundary)
        {
            boundaryCanvas.alpha = Mathf.MoveTowards(boundaryCanvas.alpha, 1f, boundaryFadeSpeed * Time.deltaTime);
        }
        else
        {
            boundaryCanvas.alpha = Mathf.MoveTowards(boundaryCanvas.alpha, 0f, boundaryFadeSpeed * Time.deltaTime);
        }
    }

    public void OnBoundaryEntered()
    {
        if (!insideBoundary)
        {
            insideBoundary = true;
            Debug.Log("Player entered boundary zone!");
        }
    }

    public void OnBoundaryExited()
    {
        if (insideBoundary)
        {
            insideBoundary = false;
            Debug.Log("Player exited boundary zone!");
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
