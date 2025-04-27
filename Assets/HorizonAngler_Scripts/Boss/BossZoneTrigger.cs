using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossZoneTrigger : MonoBehaviour
{
    [Header("References")]
    public BossFishingManager bossFishingManager;
    public BossSplinePhaseManager bossManager;
    public BossMusicManager musicManager;
    
    [Header("Player Boat Components")]
    public BoatForcedDocking boatDocking;
    public BoatController boatController;
    public PropellerBoats propellerBoats;
    public BoatCameraFollow boatCameraFollow;
    
    [Header("Docking Settings")]
    public Transform dockingTargetPoint;
    public Transform bossCameraTarget;
    
    [Header("General Settings")]
    public bool autoStartBossFight = false;
    public float delayBeforeStart = 2.0f;
    public bool destroyAfterTrigger = true;
    
    private Collider triggerCollider;
    private Renderer triggerRenderer;
    public bool hasTriggered {get; private set;}= false;
    private Camera mainCamera;
    public GameObject playerBoat;
    private Vector3 originalBoatPosition;
    private Quaternion originalBoatRotation;
    private Transform originalCameraTransform;
    
    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        triggerRenderer = GetComponent<Renderer>();
        mainCamera = Camera.main;
        
        // Find managers if not assigned
        if (bossFishingManager == null)
            bossFishingManager = FindObjectOfType<BossFishingManager>();
            
        if (bossManager == null)
            bossManager = FindObjectOfType<BossSplinePhaseManager>();
        
        if (musicManager == null)
        {
            musicManager = FindObjectOfType<BossMusicManager>();
            
            // If we found it on bossManager but not directly, use that
            if (musicManager == null && bossManager != null)
                musicManager = bossManager.musicManager;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check for player tag
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            
            // Store reference to player boat
            playerBoat = other.gameObject;
            
            // Store original position and rotation for potential reset
            originalBoatPosition = playerBoat.transform.position;
            originalBoatRotation = playerBoat.transform.rotation;
            
            // Find components if not assigned
            if (boatController == null)
                boatController = playerBoat.GetComponent<BoatController>();
                
            if (propellerBoats == null)
                propellerBoats = playerBoat.GetComponent<PropellerBoats>();
                
            if (boatDocking == null)
                boatDocking = playerBoat.GetComponent<BoatForcedDocking>();
                
            if (boatCameraFollow == null)
                boatCameraFollow = FindObjectOfType<BoatCameraFollow>();
            
            // Create BoatForcedDocking if needed
            if (boatDocking == null)
            {
                boatDocking = playerBoat.AddComponent<BoatForcedDocking>();
            }
            
            // Start docking sequence
            StartCoroutine(DockingSequence());
            
            // Notify music manager
            if (musicManager != null)
            {
                musicManager.OnTriggerEntered();
            }
            
            // Disable trigger to prevent multiple triggers
            DisableTrigger();
        }
    }
    
    private void DisableTrigger()
    {
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        if (triggerRenderer != null)
            triggerRenderer.enabled = false;
    }
    
    private IEnumerator DockingSequence()
    {
        // Disable player controls using flags instead of disabling components
        if (boatController != null)
            boatController.controlsActive = false;
                
        if (propellerBoats != null)
            propellerBoats.controlsActive = false;
                
        // Make the boat kinematic to prevent physics issues
        Rigidbody rb = playerBoat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Begin docking if forced docking is available
        if (boatDocking != null && dockingTargetPoint != null)
        {
            Debug.Log("Beginning docking sequence to point: " + dockingTargetPoint.position);
            boatDocking.BeginDocking(dockingTargetPoint);
            
            // Wait until docking is complete
            while (boatDocking.isDocking)
            {
                yield return null;
            }
            
            Debug.Log("Docking completed, now rotating boat");
            
            if (boatDocking != null)
            {
                boatDocking.EndDocking();
                Debug.Log("Boat docking fully ended. Now safe to rotate");
            }
            
            // Make sure this is called AFTER docking has completed
            yield return StartCoroutine(RotateBoatToTargetY());
        }

        // Store camera reference if main camera exists
        if (mainCamera != null)
        {
            originalCameraTransform = mainCamera.transform.parent;
        }

        // FULLY disable BoatCameraFollow script to stop LateUpdate interference
        if (boatCameraFollow != null)
        {
            boatCameraFollow.followEnabled = false;
        }

        // Now safely move camera manually
        if (bossCameraTarget != null && mainCamera != null)
        {
            yield return StartCoroutine(MoveCameraToTarget());
        }

        // Delay before starting boss fight
        yield return new WaitForSeconds(delayBeforeStart);
        
        // Start boss fight logic - choose appropriate manager
         if (bossFishingManager != null && bossFishingManager.bossModel != null)
        {
            bossFishingManager.bossModel.SetActive(true);
        }
        else if (bossManager != null && bossManager.GetComponent<BossFishingManager>() != null)
        {
            GameObject bossModel = bossManager.GetComponent<BossFishingManager>().bossModel;
            if (bossModel != null)
                bossModel.SetActive(true);
        }
        
        // Start boss fight logic - choose appropriate manager
        if (bossFishingManager != null)
        {
            bossFishingManager.StartBossFight();
        }
        else if (bossManager != null)
        {
            // Start music
            if (musicManager != null)
            {
                musicManager.InitiateBossMusic();
            }
            
            bossManager.currentPhase = 0;
            // Play the first spline
            bossManager.PlayNextPhase();
        }
        
        // If configured to destroy after trigger, do so now that docking is complete
        if (destroyAfterTrigger)
        {
            // Check if we're not a parent of the boss model
            bool isBossParent = false;
            
            if (bossFishingManager != null && bossFishingManager.bossModel != null)
            {
                Transform parent = bossFishingManager.bossModel.transform.parent;
                while (parent != null)
                {
                    if (parent == this.transform)
                    {
                        isBossParent = true;
                        break;
                    }
                    parent = parent.parent;
                }
            }
            
            if (!isBossParent)
            {
                Destroy(gameObject, 0.5f);
            }
            else
            {
                // Just disable the trigger components instead
                if (triggerCollider != null)
                    triggerCollider.enabled = false;
                if (triggerRenderer != null)
                    triggerRenderer.enabled = false;
                
                // Also disable this script but keep the object
                this.enabled = false;
            }
        }
    }
    
    private IEnumerator ManualDocking()
    {
        float dockingDuration = 3.0f;
        float elapsedTime = 0f;
        
        Vector3 startPosition = playerBoat.transform.position;
        Quaternion startRotation = playerBoat.transform.rotation;

        Vector3 targetPosition = dockingTargetPoint.position;
        Quaternion targetRotation = dockingTargetPoint.rotation;

        Rigidbody rb = playerBoat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Freeze physics while docking
        }

        while (elapsedTime < dockingDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / dockingDuration);

            // Move and rotate simultaneously
            playerBoat.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            playerBoat.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Final exact snap
        playerBoat.transform.position = targetPosition;
        playerBoat.transform.rotation = targetRotation;

        Debug.Log("Manual docking completed: Boat aligned to target position and rotation.");

        if (rb != null)
        {
            rb.isKinematic = false; // Restore physics if needed
        }
    }
    
    private IEnumerator RotateBoatToTargetY()
    {
        if (playerBoat == null || dockingTargetPoint == null)
        {
            Debug.LogError("Missing playerBoat or dockingTargetPoint for rotation!");
            yield break;
        }

        Debug.Log($"Starting rotation adjustment - Current Y: {playerBoat.transform.rotation.eulerAngles.y}, Target Y: {dockingTargetPoint.rotation.eulerAngles.y}");
        
        // Get current and target Y rotation
        float startYRotation = playerBoat.transform.rotation.eulerAngles.y;
        float targetYRotation = dockingTargetPoint.rotation.eulerAngles.y;
        
        // Store the rigidbody and temporarily disable it
        Rigidbody rb = playerBoat.GetComponent<Rigidbody>();
        bool wasKinematic = false;
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }
        
        // Find water physics components and temporarily disable them
        MonoBehaviour[] allComponents = playerBoat.GetComponents<MonoBehaviour>();
        List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>();
        
        foreach (MonoBehaviour comp in allComponents)
        {
            // Skip this component and essential components
            if (comp is BossZoneTrigger || comp is BoatForcedDocking)
                continue;
                
            // Check for typical boat physics component names
            string compName = comp.GetType().Name.ToLower();
            if (compName.Contains("float") || compName.Contains("water") || 
                compName.Contains("buoy") || compName.Contains("wave") || 
                compName.Contains("physics"))
            {
                if (comp.enabled)
                {
                    comp.enabled = false;
                    disabledComponents.Add(comp);
                    Debug.Log("Temporarily disabled: " + comp.GetType().Name);
                }
            }
        }
        
        float elapsed = 0f;
        float rotateDuration = 1.0f;

        // Force-rotate the boat
        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / rotateDuration);
            
            // Calculate new Y rotation
            float newYRotation = Mathf.LerpAngle(startYRotation, targetYRotation, t);
            
            // Get current X and Z rotation
            Vector3 currentEuler = playerBoat.transform.rotation.eulerAngles;
            
            // Apply rotation (only changing Y)
            Quaternion newRotation = Quaternion.Euler(currentEuler.x, newYRotation, currentEuler.z);
            playerBoat.transform.rotation = newRotation;
            
            Debug.Log($"Rotating: Current={newYRotation}, Target={targetYRotation}, Progress={t * 100}%");
            yield return null;
        }
        
        // Final exact rotation
        Vector3 finalEuler = playerBoat.transform.rotation.eulerAngles;
        playerBoat.transform.rotation = Quaternion.Euler(finalEuler.x, targetYRotation, finalEuler.z);
        
        Debug.Log($"Final rotation applied: {playerBoat.transform.rotation.eulerAngles.y}");
        
        // Wait a frame to ensure the rotation is applied
        yield return null;
        
        // Double-check and enforce rotation if needed
        if (Mathf.Abs(Mathf.DeltaAngle(playerBoat.transform.rotation.eulerAngles.y, targetYRotation)) > 1f)
        {
            Debug.LogWarning("Rotation didn't stick! Forcing exact rotation...");
            Vector3 enforceEuler = playerBoat.transform.rotation.eulerAngles;
            playerBoat.transform.rotation = Quaternion.Euler(enforceEuler.x, targetYRotation, enforceEuler.z);
        }
        
        // Wait one more frame to verify
        yield return null;
        Debug.Log($"Verified final Y rotation: {playerBoat.transform.rotation.eulerAngles.y}");
        
        // Re-enable the previously disabled components
        foreach (MonoBehaviour comp in disabledComponents)
        {
            comp.enabled = true;
            Debug.Log("Re-enabled: " + comp.GetType().Name);
        }
        
        // Restore rigidbody state
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
        }
    }

    private IEnumerator MoveCameraToTarget()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Main camera is null!");
            yield break;
        }
        
        if (bossCameraTarget == null)
        {
            Debug.LogError("Boss camera target is null!");
            yield break;
        }

        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        Vector3 endPos = bossCameraTarget.position;
        Quaternion endRot = bossCameraTarget.rotation;

        float elapsed = 0f;
        float moveDuration = 2.0f;

        Debug.Log("Starting camera move from: " + startPos + " to: " + endPos);

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / moveDuration);
            
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            Quaternion newRot = Quaternion.Slerp(startRot, endRot, t);
            
            mainCamera.transform.position = newPos;
            mainCamera.transform.rotation = newRot;
            
            yield return null;
        }

        // Ensure final position is exact
        mainCamera.transform.position = endPos;
        mainCamera.transform.rotation = endRot;
        
        Debug.Log("Camera move complete - Final position: " + mainCamera.transform.position);
    }
    
    // Method to reset player boat (e.g., for game restart)
    public void ResetPlayerBoat()
    {
        if (playerBoat != null)
        {
            playerBoat.transform.position = originalBoatPosition;
            playerBoat.transform.rotation = originalBoatRotation;
            
            // Re-enable boat controls using flags
            if (boatController != null)
                boatController.controlsActive = true;
                    
            if (propellerBoats != null)
                propellerBoats.controlsActive = true;
            
            // End forced docking
            if (boatDocking != null)
                boatDocking.EndDocking();
            
            // Reset rigidbody
            Rigidbody rb = playerBoat.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
        
        // Re-enable camera follow
        if (boatCameraFollow != null)
        {
            boatCameraFollow.enabled = true;
            boatCameraFollow.followEnabled = true;
        }
        
        // Reset camera position if needed
        if (mainCamera != null && originalCameraTransform != null)
        {
            mainCamera.transform.position = originalCameraTransform.position;
            mainCamera.transform.rotation = originalCameraTransform.rotation;
        }
        
        // Reset trigger
        if (triggerCollider != null)
            triggerCollider.enabled = true;
            
        if (triggerRenderer != null)
            triggerRenderer.enabled = true;
        
        // Reset trigger state
        hasTriggered = false;
    }
    
    // Public method to manually trigger the boss fight (skips docking)
    public void ManuallyTriggerBossFight()
    {
        if (!hasTriggered)
        {
            hasTriggered = true;
            
            // Notify music manager
            if (musicManager != null)
            {
                musicManager.OnTriggerEntered();
            }
            
            // Start the boss fight immediately
            if (bossFishingManager != null)
            {
                bossFishingManager.StartBossFight();
            }
            else if (bossManager != null)
            {
                if (musicManager != null)
                {
                    musicManager.InitiateBossMusic();
                }
                
                bossManager.PlayNextPhase();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw docking position in editor
        if (dockingTargetPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(dockingTargetPoint.position, 0.5f);
            
            // Draw direction arrow
            Gizmos.color = Color.green;
            Gizmos.DrawRay(dockingTargetPoint.position, dockingTargetPoint.forward * 2f);
        }
        
        // Draw camera position in editor
        if (bossCameraTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(bossCameraTarget.position, 0.3f);
            
            // Draw view direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(bossCameraTarget.position, bossCameraTarget.forward * 2f);
        }
    }

    public void ReactivateBoatControls()
    {
        Debug.Log("BossZoneTrigger.ReactivateBoatControls called");
        
        if (playerBoat != null)
        {
            Debug.Log("Reactivating boat controls using direct reference from BossZoneTrigger");
            
            // Re-enable boat controls using flags
            if (boatController != null)
            {
                boatController.controlsActive = true;
                Debug.Log("Set BoatController.controlsActive = true");
            }
                    
            if (propellerBoats != null)
            {
                propellerBoats.controlsActive = true;
                Debug.Log("Set PropellerBoats.controlsActive = true");
            }
            
            // End forced docking
            if (boatDocking != null)
            {
                boatDocking.EndDocking();
                Debug.Log("Ended forced docking");
            }
            
            // Reset rigidbody
            Rigidbody rb = playerBoat.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                Debug.Log("Set Rigidbody isKinematic to false");
            }
            
            // Re-enable camera follow
            if (boatCameraFollow != null)
            {
                boatCameraFollow.enabled = true;
                boatCameraFollow.followEnabled = true;
                Debug.Log("Re-enabled BoatCameraFollow");
            }
        }
        else
        {
            Debug.LogError("Cannot reactivate boat controls - no playerBoat reference");
        }
    }
    }