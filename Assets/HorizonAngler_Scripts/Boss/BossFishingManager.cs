using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine.Video;

public class BossFishingManager : MonoBehaviour
{
    [Header("References")]
    public InitiateMicrogames minigameManager;
    public Test2Script minigameController;
    public FishingProgress fishingProgress;
    public BossSplinePhaseManager splineManager;
    public BossMusicManager musicManager;
    public ForbiddenDirectionWarning boundaryWarning;

    [Header("Player Boat References")]
    public BoatController boatController;
    public PropellerBoats propellerBoats;
    public BoatCameraFollow boatCameraFollow;
    private GameObject playerBoatGameObject;
    private Rigidbody playerBoatRigidbody;
    private BoatForcedDocking boatForcedDocking;
    private bool hasWon = false;
    private float boatReactivationTimer = 0f;
    private float maxReactivationTime = 10f; 

    [Header("Boss Zones")]
    public GameObject firstBossZone;
    public GameObject secondBossZone;
    public GameObject thirdBossZone;  // Added third boss zone

    [Header("Boss Fight Settings")]
    public GameObject bossModel;
    public Animator bossAnimator;
    
    private int currentPhase = 0;
    private bool isFightActive = false;
    
    private void Start()
    {
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

        // ONLY find boat references if they are null
        if (boatController == null)
            boatController = FindObjectOfType<BoatController>();

        if (propellerBoats == null)
            propellerBoats = FindObjectOfType<PropellerBoats>();

        if (boatCameraFollow == null)
            boatCameraFollow = FindObjectOfType<BoatCameraFollow>();
            
        // Initialize boss zones - only first zone should be active at start
        if (firstBossZone != null)
            firstBossZone.SetActive(true);
            
        if (secondBossZone != null)
            secondBossZone.SetActive(false);
            
        if (thirdBossZone != null)
            thirdBossZone.SetActive(false);
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
        
        // If we've won, start a timer to ensure boat controls get reactivated
        if (hasWon)
        {
            boatReactivationTimer += Time.deltaTime;
            
            // Try to reactivate every second
            if (boatReactivationTimer % 1f < Time.deltaTime)
            {
                Debug.Log($"Periodic boat reactivation check ({boatReactivationTimer:F1}s / {maxReactivationTime:F1}s)");
                RestoreAllBoatControls();
            }
            
            // Stop checking after max time
            if (boatReactivationTimer >= maxReactivationTime)
            {
                hasWon = false;
                Debug.Log("Stopped reactivation timer after maximum time");
            }
        }
    }
    
    public void StartBossFight()
    {
        minigameController.ClearAll();
        isFightActive = true;
        currentPhase = 0;

        // Find and STORE references to the player boat and components
        if (boatController != null)
        {
            playerBoatGameObject = boatController.gameObject;
            playerBoatRigidbody = playerBoatGameObject.GetComponent<Rigidbody>();
            boatForcedDocking = playerBoatGameObject.GetComponent<BoatForcedDocking>();
            
            Debug.Log($"Stored boat references - GameObject: {playerBoatGameObject.name}, " +
                    $"Rigidbody: {(playerBoatRigidbody != null)}, " +
                    $"ForcedDocking: {(boatForcedDocking != null)}");
        }
        else
        {
            Debug.LogWarning("BoatController not found, trying alternative search...");
            
            // Try to find the boat by tag
            GameObject taggedBoat = GameObject.FindGameObjectWithTag("Player");
            if (taggedBoat != null)
            {
                playerBoatGameObject = taggedBoat;
                playerBoatRigidbody = playerBoatGameObject.GetComponent<Rigidbody>();
                boatController = playerBoatGameObject.GetComponent<BoatController>();
                propellerBoats = playerBoatGameObject.GetComponent<PropellerBoats>();
                boatForcedDocking = playerBoatGameObject.GetComponent<BoatForcedDocking>();
                boatCameraFollow = FindObjectOfType<BoatCameraFollow>();
            }
        }

        // Set boss zone type for fishing
        fishingProgress.activeZoneType = InitiateMicrogames.FishZoneType.BossPond;
        
        if (splineManager != null)
            splineManager.PlayNextPhase();
        
        if (musicManager != null)
            musicManager.InitiateBossMusic();
    }
    
    // Event handler for spline completion
    private void OnSplineCompleted(int phaseIndex)
    {
        if (!isFightActive) return;
        
        Debug.Log($"BossFishingManager received spline completion event for phase {phaseIndex}");
        
        // Handle Boss Zone Switches
        if (phaseIndex == 1)
        {
            Debug.Log("Finished First Escape! Switching to second boss zone...");
            
            if (firstBossZone != null)
                firstBossZone.SetActive(false);
            
            if (secondBossZone != null)
                secondBossZone.SetActive(true);
        }
        else if (phaseIndex == 3)
        {
            Debug.Log("Finished Second Escape! Switching to third boss zone...");
            
            if (secondBossZone != null)
                secondBossZone.SetActive(false);
            
            if (thirdBossZone != null)
                thirdBossZone.SetActive(true);
        }

        // Handle Minigame Activation
        if (phaseIndex == 0 || phaseIndex == 2 || phaseIndex == 4)
        {
            StartMinigame();
        }
    }
    
    private void StartMinigame()
    {
        StartCoroutine(InitiateMinigameDirectly());
    }
    
    private IEnumerator InitiateMinigameDirectly()
    {
        minigameManager.MGCanvas.SetActive(true);
        minigameManager.CCanvas.SetActive(false);
        minigameManager.CTC.SetActive(false);
        minigameManager.WFB.SetActive(false);
        minigameManager.FTB.SetActive(false);

        if (fishingProgress != null)
            fishingProgress.Initialize();

        yield return new WaitForSeconds(0.5f);
        ConfigureBossMinigames();

        if (minigameController != null)
            minigameController.Initialize();
    }
    
    private void ConfigureBossMinigames()
    {
        // Configure which minigame sets to use based on current phase
        // Progressively increase difficulty with each phase
        switch (currentPhase)
        {
            case 0: // First boss phase - easier
                minigameManager.enableSet1 = true;
                minigameManager.enableSet2 = true;
                minigameManager.enableSet3 = true;
                minigameManager.enableSet4 = true;
                minigameManager.enableSet5 = true;
                minigameManager.enableSet6 = true;
                minigameManager.enableSet7 = true;
                break;
                
            case 1: // Second boss phase - medium
                minigameManager.enableSet1 = true;
                minigameManager.enableSet2 = true;
                minigameManager.enableSet3 = true;
                minigameManager.enableSet4 = true;
                minigameManager.enableSet5 = true;
                minigameManager.enableSet6 = true;
                minigameManager.enableSet7 = true;
                break;
                
            case 2: // Final boss phase - hardest with all microgames
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
        minigameManager.SetActiveMicrogameSets(fishingProgress.activeZoneType);
    }
    
    // Called when player successfully completes a minigame phase
    private void OnBossFightSuccess()
    {
        Debug.Log("Boss Fight Success - FORCIBLY disabling all microgames");
        ForceStopAllMicrogames();
        Debug.Log("Boss Fight Success! Current Phase: " + currentPhase);

        // Increment phase
        currentPhase++;

        if (currentPhase < 3)
        {
            // Instead of moving immediately, STUN first
            StartCoroutine(PlayStunnedThenNextPhase());
        }
        else
        {
            Debug.Log("Final boss phase completed! Attempting to reactivate boat controls...");
            
            // Play stun first, then sink
            StartCoroutine(PlayStunnedThenSink());
        }
    }

    private IEnumerator PlayStunnedThenNextPhase()
    {
        Debug.Log("Playing stunned animation before next phase");

        if (bossAnimator != null)
        {
            bossAnimator.Play("AnglerStunned");
        }

        // Play the jumpscare sound from music manager
        if (musicManager != null)
        {
            musicManager.PlayJumpscareSound();
        }

        yield return new WaitForSeconds(2.0f); // Wait for stun animation to finish

        if (bossAnimator != null)
        {
            bossAnimator.Play("IdleState"); // Return to Idle
        }

        ActivateBoatControls();

        if (splineManager != null)
        {
            splineManager.PlayNextPhase();
        }
    }

    private IEnumerator PlayStunnedThenSink()
    {
        Debug.Log("Playing stunned animation before sinking");

        if (bossAnimator != null)
        {
            bossAnimator.Play("AnglerStunned");
        }

        if (musicManager != null)
        {
            musicManager.PlayJumpscareSound();
        }

        yield return new WaitForSeconds(2.0f);

        if (bossAnimator != null)
        {
            bossAnimator.Play("IdleState");
        }

        ActivateBoatControls();

        if (bossModel != null)
        {
            StartCoroutine(SinkBossAndPlayCutscene());
        }
        else
        {
            PlayVictoryCutscene();
        }
    }


    // Simple method to activate boat controls
    private void ActivateBoatControls()
    {
        Debug.Log("Directly activating boat controls");
        
        if (playerBoatGameObject != null)
        {
            // End any forced docking
            if (boatForcedDocking != null)
            {
                boatForcedDocking.EndDocking();
                Debug.Log("Ended forced docking");
            }
            
            // Get components
            BoatController boatCtrl = playerBoatGameObject.GetComponent<BoatController>();
            PropellerBoats propBoats = playerBoatGameObject.GetComponent<PropellerBoats>();
            
            // Set controls active flags
            if (boatCtrl != null)
            {
                boatCtrl.controlsActive = true;
                boatCtrl.enabled = true; // Try both approaches
                Debug.Log("Set BoatController active");
            }
            
            if (propBoats != null)
            {
                propBoats.controlsActive = true;
                propBoats.enabled = true; // Try both approaches
                Debug.Log("Set PropellerBoats active");
            }
            
            // Make the Rigidbody non-kinematic
            if (playerBoatRigidbody != null)
            {
                playerBoatRigidbody.isKinematic = false;
                Debug.Log("Set Rigidbody isKinematic to false");
            }
            
            // Camera follow activation
            if (boatCameraFollow != null)
            {
                boatCameraFollow.enabled = true;
                boatCameraFollow.followEnabled = true;
                Debug.Log("Set BoatCameraFollow active");
            }
        }
        else
        {
            Debug.LogError("No player boat reference to activate controls!");
        }
    }

    public void ForceStopAllMicrogames()
    {
        if (minigameController != null)
        {
            var setKeys = new List<string>();
            foreach (var key in minigameController.Sets.Keys)
            {
                setKeys.Add(key);
            }
            
            foreach (var key in setKeys)
            {
                minigameController.Sets[key] = false;
            }
            
            minigameController.activeSets.Clear();
            minigameController.microgamesActive = false;
            minigameController.StopAllCoroutines();
            minigameController.ClearAll();
        }
        
        if (minigameManager != null)
        {
            minigameManager.MGCanvas.SetActive(false);
            minigameManager.fishingStarted = false;
            minigameManager.FullFishingReset();
        }
    }
    
    // Called when player fails a minigame phase
    private void OnBossFightFailure()
    {
        minigameManager.MGCanvas.SetActive(false);
        minigameController.ClearAll();
        minigameManager.fishingStarted = false;
        
        if (boundaryWarning != null)
        {
            boundaryWarning.PlayerDeathSequence();
        }
    }
    
    private void OnBossFightComplete(bool victory)
    {
        isFightActive = false;
        RestoreAllBoatControls();
        
        if (victory)
        {
            Debug.Log("Boss fight complete with victory!");
            hasWon = true;
            boatReactivationTimer = 0f;
            
            ForceStopAllMicrogames();

            if (bossModel != null)
                StartCoroutine(SinkBossAndPlayCutscene());
            else
                PlayVictoryCutscene();
        }
    }

    private IEnumerator SinkBossAndPlayCutscene()
    {
        Debug.Log("SinkBossAndPlayCutscene started");
        ForceStopAllMicrogames();
        
        // Disable the third boss zone after winning
        if (thirdBossZone != null)
        {
            thirdBossZone.SetActive(false);
            Debug.Log("Deactivated third boss zone after victory");
        }
        else
        {
            Debug.LogWarning("thirdBossZone reference is null!");
        }
        
        // Check boss model
        if (bossModel == null)
        {
            Debug.LogError("Boss model is null! Cannot sink boss. Check your boss model reference.");
            PlayVictoryCutscene(); // Try to play cutscene anyway
            yield break;
        }
        
        Debug.Log($"Starting boss sink. Initial position: {bossModel.transform.position}");
        
        // Try to disable any components that might interfere with position
        Rigidbody bossRb = bossModel.GetComponent<Rigidbody>();
        if (bossRb != null)
        {
            bossRb.isKinematic = true;
            Debug.Log("Set boss Rigidbody to kinematic");
        }
        
        // Disable any scripts on the boss that might be controlling its position
        MonoBehaviour[] bossScripts = bossModel.GetComponents<MonoBehaviour>();
        List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
        
        foreach (MonoBehaviour script in bossScripts)
        {
            // Don't disable this script
            if (script is BossFishingManager)
                continue;
                
            if (script.enabled)
            {
                script.enabled = false;
                disabledScripts.Add(script);
                Debug.Log($"Temporarily disabled script: {script.GetType().Name}");
            }
        }
        
        // Store initial state
        float sinkDuration = 5.0f;
        float sinkDepth = -15.0f;  // Negative to sink DOWN
        float elapsedTime = 0f;
        Vector3 startPosition = bossModel.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y + sinkDepth, startPosition.z);
        
        Debug.Log($"Sink parameters: Start Y={startPosition.y}, Target Y={endPosition.y}, Duration={sinkDuration}s");
        
        // Play sound if available
        AudioSource bossAudio = bossModel.GetComponent<AudioSource>();
        if (bossAudio != null && bossAudio.clip != null)
        {
            bossAudio.Play();
            Debug.Log("Playing boss audio");
        }
        
        // Sinking animation
        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / sinkDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);
            
            // Store original position before change for logging
            Vector3 beforePos = bossModel.transform.position;
            
            // Apply position change
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, smoothT);
            bossModel.transform.position = newPosition;
            
            // Check if position actually changed
            if (Vector3.Distance(beforePos, bossModel.transform.position) < 0.01f)
            {
                Debug.LogWarning($"Boss position didn't change! Something is preventing movement. " +
                                $"Attempted to set Y={newPosition.y}, but Y={bossModel.transform.position.y}");
            }
            
            // Add rotation
            bossModel.transform.Rotate(0, 10 * Time.deltaTime, 0);
            
            // Progress logging
            if (Mathf.Floor(elapsedTime * 2) > Mathf.Floor((elapsedTime - Time.deltaTime) * 2))
            {
                Debug.Log($"Boss sinking: Current Y={bossModel.transform.position.y}, Target Y={endPosition.y}, " +
                        $"Progress={t*100:F1}%, Distance moved={Vector3.Distance(startPosition, bossModel.transform.position)}");
            }
            
            yield return null;
        }
        
        // Force final position
        Debug.Log($"Sink animation complete. Setting final position Y={endPosition.y}");
        bossModel.transform.position = endPosition;
        
        // Re-enable scripts we disabled
        foreach (MonoBehaviour script in disabledScripts)
        {
            script.enabled = true;
            Debug.Log($"Re-enabled script: {script.GetType().Name}");
        }
        
        Debug.Log($"Boss final position: {bossModel.transform.position}");
        
        // Wait before cutscene
        Debug.Log("Waiting 2 seconds before playing cutscene...");
        yield return new WaitForSeconds(2.0f);
        
        // Direct cutscene call
        Debug.Log("Now calling PlayVictoryCutscene()...");
        PlayVictoryCutscene();
    }

    private void PlayVictoryCutscene()
    {
        Debug.Log("PlayVictoryCutscene called");
        
        CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>(true);
        
        if (cutsceneManager != null)
        {
            Debug.Log($"Found CutsceneManager on object: {cutsceneManager.gameObject.name}");
            
            // Make sure its GameObject is active
            if (!cutsceneManager.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("CutsceneManager GameObject is inactive. Activating it.");
                cutsceneManager.gameObject.SetActive(true);
            }
            
            // Try the new direct method
            Debug.Log("Calling cutsceneManager.PlayVictoryCutscene()");
            cutsceneManager.PlayVictoryCutscene();
        }
        else
        {
            Debug.LogError("CutsceneManager not found in scene!");
            RestoreAllBoatControls();
        }
    }

    private void OnCutsceneComplete(VideoPlayer vp)
    {
        vp.loopPointReached -= OnCutsceneComplete;
        Debug.Log("Cutscene completed, scheduling boat control restoration");
        StartCoroutine(DelayedRestoreControls(3.0f));
    }

    private IEnumerator DelayedRestoreControls(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Delayed boat control restoration triggered");
        RestoreAllBoatControls();
    }

    private void RestoreAllBoatControls()
    {
        Debug.Log("BossFishingManager.RestoreAllBoatControls called");
        
        // Try to get the boat controller components
        BoatController[] allBoatControllers = FindObjectsOfType<BoatController>(true);
        PropellerBoats[] allPropellerBoats = FindObjectsOfType<PropellerBoats>(true);
        BoatCameraFollow[] allCameraFollows = FindObjectsOfType<BoatCameraFollow>(true);
        BoatForcedDocking[] allForcedDockings = FindObjectsOfType<BoatForcedDocking>(true);
        
        Debug.Log($"Found: {allBoatControllers.Length} BoatControllers, " +
                 $"{allPropellerBoats.Length} PropellerBoats, " +
                 $"{allCameraFollows.Length} BoatCameraFollows");
        
        // Activate boat controls on all found components
        foreach (var controller in allBoatControllers) 
        {
            controller.controlsActive = true;
            controller.enabled = true;
            Debug.Log($"Activated BoatController on {controller.gameObject.name}");
            
            // Make sure Rigidbody is not kinematic
            Rigidbody rb = controller.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                Debug.Log($"Set Rigidbody isKinematic=false on {controller.gameObject.name}");
            }
        }
        
        foreach (var propeller in allPropellerBoats)
        {
            propeller.controlsActive = true;
            propeller.enabled = true;
            Debug.Log($"Activated PropellerBoats on {propeller.gameObject.name}");
        }
        
        foreach (var cameraFollow in allCameraFollows)
        {
            cameraFollow.enabled = true;
            cameraFollow.followEnabled = true;
            Debug.Log($"Activated BoatCameraFollow on {cameraFollow.gameObject.name}");
        }
        
        foreach (var docking in allForcedDockings)
        {
            docking.EndDocking();
            Debug.Log($"Ended docking on {docking.gameObject.name}");
        }
    }

    // Connect to spline manager
    void OnEnable()
    {
        if (splineManager != null)
        {
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

    // Called from boundary warning system
    public void OnBoundaryDeathTriggered()
    {
        if (!isFightActive) return;
        
        Debug.Log("Boundary death triggered, cleaning up minigames");
        
        // End the minigame if active
        if (minigameManager.MGCanvas.activeSelf)
        {
            minigameManager.MGCanvas.SetActive(false);
            minigameController.ClearAll();
            minigameManager.fishingStarted = false;
        }
        
        // Force stop all microgames
        ForceStopAllMicrogames();
    }
}