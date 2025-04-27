using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class BossFishingManager : MonoBehaviour
{
    [Header("References")]
    public InitiateMicrogames minigameManager;
    public Test2Script minigameController;
    public FishingProgress fishingProgress;
    public BossSplinePhaseManager splineManager;
    public BossMusicManager musicManager;
    public ForbiddenDirectionWarning boundaryWarning;
    
    [Header("Boss Fight Settings")]
    public GameObject bossModel;
    public Animator bossAnimator;
    public SplineContainer deathSpline;  // Spline for boss attack on minigame failure
    
    [Header("UI References")]
    public GameObject bossFightProgressUI; // The progress slider from fishing minigame
    
    private int currentPhase = 0;
    private bool isFightActive = false;
    
    private void Start()
    {
        // Initialize references if not set in inspector
        if (minigameManager == null)
            minigameManager = FindObjectOfType<InitiateMicrogames>();
        
        if (minigameController == null && minigameManager != null)
            minigameController = minigameManager.T2S;
            
        if (fishingProgress == null && minigameManager != null)
            fishingProgress = minigameManager.FProgress;
            
        if (splineManager == null)
            splineManager = FindObjectOfType<BossSplinePhaseManager>();
            
        if (musicManager == null)
            musicManager = FindObjectOfType<BossMusicManager>();
            
        if (boundaryWarning == null)
            boundaryWarning = FindObjectOfType<ForbiddenDirectionWarning>();
    }
    
    private void Update()
    {
        if (!isFightActive || fishingProgress == null) return;
        
        // Monitor progress during active minigame
        if (minigameManager.MGCanvas.activeSelf)
        {
            if (fishingProgress.progress >= 100f && !fishingProgress.progressMaxHandled)
            {
                fishingProgress.progressMaxHandled = true;
                OnBossFightSuccess();
            }
            else if (fishingProgress.progress <= 0f)
            {
                OnBossFightFailure();
            }
        }
    }
    
    public void StartBossFight()
    {
        isFightActive = true;
        currentPhase = 0;
        
        // Set boss zone type for fishing
        fishingProgress.activeZoneType = InitiateMicrogames.FishZoneType.BossPond; // Or appropriate boss type
        
        // Start first spline
        if (splineManager != null)
        {
            splineManager.PlayNextPhase(); // This plays the first spline
        }
            
        // Start music
        if (musicManager != null)
        {
            musicManager.InitiateBossMusic();
        }
    }
    
    // Event handler for spline completion
    private void OnSplineCompleted(int phaseIndex)
    {
        if (!isFightActive) return;
        
        Debug.Log($"BossFishingManager received spline completion event for phase {phaseIndex}");
        
        // If it was a "jumpscare" spline (0, 2, 4), start minigame
        // Note: phaseIndex is the phase that just completed
        if (phaseIndex == 0 || phaseIndex == 2 || phaseIndex == 4)
        {
            StartMinigame();
        }
    }
    
    private void StartMinigame()
    {
        // Activate minigame but skip the fishing sequence
        StartCoroutine(InitiateMinigameDirectly());
    }
    
    private IEnumerator InitiateMinigameDirectly()
    {
        // Set up the minigame UI
        minigameManager.MGCanvas.SetActive(true);
        minigameManager.CCanvas.SetActive(false);
        
        // Skip fishing animations by hiding these UI elements
        minigameManager.CTC.SetActive(false);
        minigameManager.WFB.SetActive(false);
        minigameManager.FTB.SetActive(false);
        
        // Initialize fishing progress (resets the progress bar)
        fishingProgress.Initialize();
        
        // Initial countdown
        minigameController.Initialize();
        
        // Wait for countdown to finish (~ 3 seconds)
        yield return new WaitForSeconds(3.5f);
        
        // Configure which minigames to use for this boss phase
        ConfigureBossMinigames();
    }
    
    private void ConfigureBossMinigames()
    {
        // Configure which minigame sets to use based on current phase
        // This is where you customize which sets to activate
        
        // Example: Use more difficult games for later phases
        switch (currentPhase)
        {
            case 0: // First boss phase
                minigameManager.enableSet1 = true;
                minigameManager.enableSet3 = true;
                minigameManager.enableSet5 = true;
                
                minigameManager.enableSet2 = false;
                minigameManager.enableSet4 = false;
                minigameManager.enableSet6 = false;
                minigameManager.enableSet7 = false;
                break;
                
            case 1: // Second boss phase
                minigameManager.enableSet2 = true;
                minigameManager.enableSet4 = true;
                minigameManager.enableSet6 = true;
                
                minigameManager.enableSet1 = false;
                minigameManager.enableSet3 = false;
                minigameManager.enableSet5 = false;
                minigameManager.enableSet7 = false;
                break;
                
            case 2: // Final boss phase
                // Enable all minigames for final phase
                minigameManager.enableSet1 = true;
                minigameManager.enableSet2 = true;
                minigameManager.enableSet3 = true;
                minigameManager.enableSet4 = true;
                minigameManager.enableSet5 = true;
                minigameManager.enableSet6 = true;
                minigameManager.enableSet7 = true;
                break;
        }
        
        minigameManager.useInspectorOverrides = true;
        
        // Now, we need to set the active fish zone type and call SetActiveMicrogameSets
        // to update the ActiveMicrogameSets list based on our manual overrides
        minigameManager.SetActiveMicrogameSets(fishingProgress.activeZoneType);
    }
    
    // Called when player successfully completes a minigame phase
    private void OnBossFightSuccess()
    {
        // Turn off minigame
        minigameManager.MGCanvas.SetActive(false);
        minigameController.ClearAll();
        
        // Disable fishing UI
        minigameManager.fishingStarted = false;
        
        // Increment phase
        currentPhase++;
        
        if (currentPhase < 3)
        {
            // Play next "running away" spline
            splineManager.PlayNextPhase();
        }
        else
        {
            // Final victory
            OnBossFightComplete(true);
        }
    }
    
    // Called when player fails a minigame phase
    private void OnBossFightFailure()
    {
        // Turn off minigame
        minigameManager.MGCanvas.SetActive(false);
        minigameController.ClearAll();
        
        // Disable fishing UI
        minigameManager.fishingStarted = false;
        
        // Use boundary warning death sequence
        if (boundaryWarning != null)
        {
            boundaryWarning.PlayerDeathSequence();
        }
        else
        {
            // Fallback if boundary warning not found
            Debug.LogWarning("BoundaryWarning not found, cannot play death sequence!");
        }
    }
    
    // Called from boundary warning system
    public void OnBoundaryDeathTriggered()
    {
        if (!isFightActive) return;
        
        // End the minigame if active
        if (minigameManager.MGCanvas.activeSelf)
        {
            minigameManager.MGCanvas.SetActive(false);
            minigameController.ClearAll();
            minigameManager.fishingStarted = false;
        }
        
        // No additional logic needed since boundary warning handles the sequence
    }
    
    private void OnBossFightComplete(bool victory)
    {
        isFightActive = false;
        
        if (victory)
        {
            // Start the boss sinking sequence
            if (bossModel != null)
            {
                StartCoroutine(SinkBossAndPlayCutscene());
            }
            else
            {
                // If no boss model, just play the cutscene immediately
                PlayVictoryCutscene();
            }
        }
    }
    
    private IEnumerator SinkBossAndPlayCutscene()
    {
        float sinkDuration = 3.0f;  // How long the sinking should take
        float sinkDepth = -15.0f;   // How far down the boss should sink
        float elapsedTime = 0f;
        Vector3 startPosition = bossModel.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y + sinkDepth, startPosition.z);
        
        // Optionally, play any relevant sounds
        AudioSource bossAudio = bossModel.GetComponent<AudioSource>();
        if (bossAudio != null && bossAudio.clip != null)
        {
            bossAudio.Play();
        }
        
        // Sink the boss
        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / sinkDuration;
            
            // Use a smooth easing function
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            // Move the boss down
            bossModel.transform.position = Vector3.Lerp(startPosition, endPosition, smoothT);
            
            // Optional: Add some rotation for dramatic effect
            bossModel.transform.Rotate(0, 10 * Time.deltaTime, 0);
            
            yield return null;
        }
        
        // Short delay before playing cutscene
        yield return new WaitForSeconds(1.0f);
        
        // Play victory cutscene
        PlayVictoryCutscene();
    }
    
    private void PlayVictoryCutscene()
    {
        // Show cutscene
        CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
        if (cutsceneManager != null)
            cutsceneManager.PlayCutscene();
        // The reward canvas will show automatically after cutscene via CutsceneManager
    }
    
    // Connect to spline manager
    void OnEnable()
    {
        if (splineManager != null)
        {
            // This is the correct syntax to subscribe to the event
            splineManager.OnSplinePhaseCompleted += OnSplineCompleted;
        }
    }

    void OnDisable()
    {
        if (splineManager != null)
        {
            splineManager.OnSplinePhaseCompleted -= OnSplineCompleted;
        }
    }
}