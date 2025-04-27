using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ForbiddenDirectionWarning : MonoBehaviour
{
    [Header("References")]
    public Transform playerShip;
    public CanvasGroup warningCanvas;
    public CanvasGroup blackoutCanvas;
    public CanvasGroup boundaryCanvas;
    
    [Header("Death Sequence References")]
    public GameObject bossModel;
    public Animator bossAnimator;
    public GameObject deathScreen;
    public Transform deathCameraPosition;
    public BossSplinePhaseManager bossPhaseManager; // Only need this now

    [Header("Death Sequence Sound")]
    public AudioClip deathSFX;
    public AudioSource audioSource;

    [Header("Settings")]
    public float maxBlackoutTime = 20f;
    public float blackoutFadeOutSpeed = 1f;
    public float boundaryFadeSpeed = 2f;
    public float deathSequenceDelay = 1.5f;

    private float darknessTimer = 0f;
    private bool insideBoundary = false;
    public bool isDeathSequencePlaying = false;
    private BossFishingManager bossFishingManager;

    void Start()
    {
        bossFishingManager = FindObjectOfType<BossFishingManager>();

        if (bossModel != null)
            bossModel.SetActive(false);

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
            var deathCanvasGroup = deathScreen.GetComponent<CanvasGroup>();
            if (deathCanvasGroup != null)
                deathCanvasGroup.alpha = 0f;
        }
    }

    void Update()
    {
        if (playerShip == null || isDeathSequencePlaying) return;

        float yRotation = NormalizeAngle(playerShip.eulerAngles.y);
        float absY = Mathf.Abs(yRotation);

        if (absY <= 90f)
        {
            warningCanvas.alpha = Mathf.Clamp01(1f - (absY / 90f));
            darknessTimer += Time.deltaTime;

            if (darknessTimer >= maxBlackoutTime)
                PlayerDeathSequence();
        }
        else
        {
            warningCanvas.alpha = 0f;
            darknessTimer -= Time.deltaTime * blackoutFadeOutSpeed;
        }

        darknessTimer = Mathf.Clamp(darknessTimer, 0f, maxBlackoutTime);
        blackoutCanvas.alpha = darknessTimer / maxBlackoutTime;

        boundaryCanvas.alpha = Mathf.MoveTowards(
            boundaryCanvas.alpha,
            insideBoundary ? 1f : 0f,
            boundaryFadeSpeed * Time.deltaTime
        );
    }

    public void OnBoundaryEntered()
    {
        if (!insideBoundary)
        {
            insideBoundary = true;
            StartCoroutine(BoundaryDeathCheck());
        }
    }

    public void OnBoundaryExited()
    {
        insideBoundary = false;
    }

    private IEnumerator BoundaryDeathCheck()
    {
        float timer = 0f;
        float deathThreshold = 5f;

        while (insideBoundary && !isDeathSequencePlaying)
        {
            timer += Time.deltaTime;
            if (timer >= deathThreshold)
            {
                PlayerDeathSequence();
                yield break;
            }
            yield return null;
        }
    }

    public void PlayerDeathSequence()
    {
        if (isDeathSequencePlaying) return;
        isDeathSequencePlaying = true;

        if (audioSource != null && deathSFX != null)
            audioSource.PlayOneShot(deathSFX);

        DisableAllWarningUI();

        if (bossFishingManager != null)
            bossFishingManager.OnBoundaryDeathTriggered();
            bossFishingManager.ForceStopAllMicrogames();  // <--- ADD THIS
            bossFishingManager.DisableFishingUI();

        StartCoroutine(ExecuteDeathSequence());
    }

    private void DisableAllWarningUI()
    {
        SetCanvasGroup(warningCanvas, false);
        SetCanvasGroup(blackoutCanvas, false);
        SetCanvasGroup(boundaryCanvas, false);
    }

    private void SetCanvasGroup(CanvasGroup group, bool visible)
    {
        if (group == null) return;
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
        if (!visible)
            group.gameObject.SetActive(false);
    }

    private IEnumerator ExecuteDeathSequence()
    {
        var deathCanvasGroup = deathScreen.GetComponent<CanvasGroup>();
        if (deathCanvasGroup != null)
            deathCanvasGroup.alpha = 1f;

        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera != null)
        {
            var followScript = mainCamera.GetComponent<BoatCameraFollow>();
            if (followScript != null)
                followScript.followEnabled = false;

            if (deathCameraPosition != null)
            {
                mainCamera.transform.position = deathCameraPosition.position;
                mainCamera.transform.rotation = deathCameraPosition.rotation;
            }
        }

        if (bossModel != null)
            bossModel.SetActive(true);

        if (bossPhaseManager != null)
            bossPhaseManager.PlayDeathPhase();
        else
            Debug.LogError("BossSplinePhaseManager not found!");

        if (bossAnimator != null)
            bossAnimator.Play("AnglerEat");

        // Wait for death spline duration properly
        yield return StartCoroutine(WaitForDeathSplineCompletion());

        // Play death video
        var videoPlayer = deathScreen.transform.Find("RawImage")?.GetComponent<UnityEngine.Video.VideoPlayer>();
        if (videoPlayer != null)
            videoPlayer.Play();

        // Fade in video
        var rawImageGroup = deathScreen.transform.Find("RawImage")?.GetComponent<CanvasGroup>();
        if (rawImageGroup != null)
            StartCoroutine(FadeCanvasGroup(rawImageGroup, 1f));

        yield return new WaitForSeconds(3f);

        // Fade in DeathPanel
        var deathPanelGroup = deathScreen.transform.Find("DeathPanel")?.GetComponent<CanvasGroup>();
        if (deathPanelGroup != null)
            StartCoroutine(FadeCanvasGroup(deathPanelGroup, 2f));

        // Pause game and unlock cursor
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator WaitForDeathSplineCompletion()
    {
        bool splineFinished = false;

        bossPhaseManager.OnSplinePhaseCompleted += (phaseIndex) =>
        {
            if (phaseIndex == 5) // Death spline is at index 5
                splineFinished = true;
        };

        // Safety check: if something wrong happens, max wait 10 sec
        float maxWait = 2.3f;
        float elapsed = 0f;

        while (!splineFinished && elapsed < maxWait)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float duration)
    {
        if (group == null) yield break;
        
        group.alpha = 0f;
        group.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
