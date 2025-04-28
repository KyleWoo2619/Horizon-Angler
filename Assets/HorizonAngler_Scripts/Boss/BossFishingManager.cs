using System.Collections;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEngine.Video;
using UnityEngine.AI;

public class BossFishingManager : MonoBehaviour
{
    [Header("References")]
    public InitiateMicrogames minigameManager;
    public Test2Script minigameController;
    public FishingProgress fishingProgress;
    public BossSplinePhaseManager splineManager;
    public BossMusicManager musicManager;
    public ForbiddenDirectionWarning boundaryWarning;
    public GameObject AllSounds;

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

    [Header("Lightning Attack VFX")]
    public GameObject lightningVFX1;
    public GameObject lightningVFX2;
    public AudioSource lightningAudio; // You can set this to an AudioSource on one of the lightning VFXs

    [Header("Boss Zones")]
    public GameObject firstBossZone;
    public GameObject secondBossZone;
    public GameObject thirdBossZone;  // Added third boss zone

    [Header("Boss Fight Settings")]
    public GameObject bossModel;
    public Animator bossAnimator;
    public CutsceneManager cutsceneManager;
    public CanvasGroup BlackoutCanvas;
    
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
        if (boundaryWarning != null && boundaryWarning.isDeathSequencePlaying) return;
        
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

        SetBossAnimationFight();

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
        Debug.Log("Boss Fight Success! Current Boss Manager Phase: " + currentPhase);

        // Increment internal boss manager phase
        currentPhase++;

        if (splineManager != null && splineManager.currentPhase >= 5)
        {
            // Boss spline finished all 5 splines, and we just won the FINAL minigame
            Debug.Log("Final boss minigame success! Boss will sink and trigger ending.");
            StartCoroutine(PlayStunnedThenSink());
        }
        else
        {
            // Otherwise normal progress to next phase
            StartCoroutine(PlayStunnedThenNextPhase());
        }
    }

    private IEnumerator PlayStunnedThenNextPhase()
    {
        Debug.Log("Playing stunned animation before next phase");

        if (lightningVFX1 != null) lightningVFX1.SetActive(true);
        if (lightningVFX2 != null) lightningVFX2.SetActive(true);

        // Play sound
        if (lightningAudio != null) lightningAudio.Play();

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

        if (lightningVFX1 != null) lightningVFX1.SetActive(false);
        if (lightningVFX2 != null) lightningVFX2.SetActive(false);

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

         if (lightningVFX1 != null) lightningVFX1.SetActive(true);
        if (lightningVFX2 != null) lightningVFX2.SetActive(true);

        // Play sound
        if (lightningAudio != null) lightningAudio.Play();

        if (bossAnimator != null)
        {
            bossAnimator.Play("AnglerDeath");
        }

        if (musicManager != null)
        {
            musicManager.PlayJumpscareSound();
        }

        yield return new WaitForSeconds(2.0f);

        ActivateBoatControls();

        if (lightningVFX1 != null) lightningVFX1.SetActive(false);
        if (lightningVFX2 != null) lightningVFX2.SetActive(false);

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

        // Make sure third boss zone is disabled
        if (thirdBossZone != null)
        {
            thirdBossZone.SetActive(false);
            Debug.Log("Deactivated third boss zone after victory");
        }

        // Force stop microgames
        ForceStopAllMicrogames();

        // Freeze boss physics
        Rigidbody bossRb = bossModel.GetComponent<Rigidbody>();
        if (bossRb != null)
        {
            bossRb.isKinematic = true;
        }

        // Sinking animation
        float sinkDuration = 7.0f;
        float sinkDepth = -80.0f;
        float elapsedTime = 0f;

        Vector3 startPosition = bossModel.transform.position;
        Vector3 endPosition = new Vector3(startPosition.x, startPosition.y + sinkDepth, startPosition.z);

        if (BlackoutCanvas != null)
            BlackoutCanvas.alpha = 0f;

        while (elapsedTime < sinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / sinkDuration);
            bossModel.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            bossModel.transform.Rotate(-2 * Time.deltaTime, 0, 0);
            yield return null;

            if (BlackoutCanvas != null)
            {
                BlackoutCanvas.alpha = t; // Simple linear fade
            }
        }

        bossModel.transform.position = endPosition;
        if (BlackoutCanvas != null)
            BlackoutCanvas.alpha = 1f;

        Debug.Log("Boss sunk. Now starting cutscene...");

        // Fade out music
        if (musicManager != null)
        {
            musicManager.FadeOutAllMusic();
            Debug.Log("Fading out all music for cutscene transition");
        }

        // Actually play the cutscene here
        PlayVictoryCutscene();
        AllSounds.SetActive(false);
    }



    private void PlayVictoryCutscene()
    {
        Debug.Log("PlayVictoryCutscene called");
        
        if (cutsceneManager != null)
        {
            Debug.Log($"Found CutsceneManager on object: {cutsceneManager.gameObject.name}");
            
            // Make sure its GameObject is active
            if (!cutsceneManager.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("CutsceneManager GameObject is inactive. Activating it.");
                cutsceneManager.gameObject.SetActive(true);
            }
            
            musicManager.FadeOutAllMusic();
            // Try the new direct method
            Debug.Log("Calling cutsceneManager.PlayVictoryCutscene()");
            cutsceneManager.PlayVictoryCutsceneSimple();
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

    private void SetBossAnimationFight()
    {
        if (bossAnimator != null)
        {
            bossAnimator.Play("AnglerFight");
            Debug.Log("Boss Animator: Playing AnglerFight Animation");
        }
    }

    public void DisableFishingUI()
    {
        if (minigameManager != null)
        {
            if (minigameManager.MGCanvas != null)
                minigameManager.MGCanvas.SetActive(false);

            if (minigameManager.CCanvas != null)
                minigameManager.CCanvas.SetActive(false);

            if (minigameManager.CTC != null)
                minigameManager.CTC.SetActive(false);

            if (minigameManager.WFB != null)
                minigameManager.WFB.SetActive(false);

            if (minigameManager.FTB != null)
                minigameManager.FTB.SetActive(false);

            minigameManager.fishingStarted = false;
        }

        if (minigameController != null)
        {
            minigameController.ClearAll();
            minigameController.StopAllCoroutines();
        }

        Debug.Log("Disabled all Fishing UI and Minigame during Death.");
    }
}