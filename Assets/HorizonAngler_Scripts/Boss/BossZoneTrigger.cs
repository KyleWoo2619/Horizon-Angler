using System.Collections;
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
    private bool hasTriggered = false;
    private Camera mainCamera;
    private GameObject playerBoat;
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
        // Disable player controls
        if (boatController != null)
            boatController.enabled = false;
            
        if (propellerBoats != null)
            propellerBoats.enabled = false;
            
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
            boatDocking.BeginDocking(dockingTargetPoint);
            
            // Wait until docking is complete
            while (boatDocking.isDocking)
            {
                yield return null;
            }
        }
        else
        {
            // Alternative manual docking if BoatForcedDocking is not available
            yield return StartCoroutine(ManualDocking());
        }
        
        // Smoothly rotate boat towards docking Y rotation
        yield return StartCoroutine(RotateBoatToTargetY());

        // Store camera reference if main camera exists
        if (mainCamera != null)
        {
            originalCameraTransform = mainCamera.transform.parent;
        }

        // FULLY disable BoatCameraFollow script to stop LateUpdate interference
        if (boatCameraFollow != null)
        {
            boatCameraFollow.followEnabled = false;
            boatCameraFollow.enabled = false;  // Complete disable
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
        
        while (elapsedTime < dockingDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dockingDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth easing
            
            playerBoat.transform.position = Vector3.Lerp(startPosition, dockingTargetPoint.position, t);
            playerBoat.transform.rotation = Quaternion.Slerp(startRotation, dockingTargetPoint.rotation, t);
            
            yield return null;
        }
        
        // Set final position and rotation
        playerBoat.transform.position = dockingTargetPoint.position;
        playerBoat.transform.rotation = dockingTargetPoint.rotation;
    }
    
    private IEnumerator RotateBoatToTargetY()
    {
        Quaternion startRot = playerBoat.transform.rotation;
        Quaternion targetRot = Quaternion.Euler(
            startRot.eulerAngles.x,
            dockingTargetPoint.rotation.eulerAngles.y,
            startRot.eulerAngles.z);

        float elapsed = 0f;
        float rotateDuration = 1.5f;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            playerBoat.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / rotateDuration);
            yield return null;
        }

        playerBoat.transform.rotation = targetRot;
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
            
            // Re-enable boat controls
            if (boatController != null)
                boatController.enabled = true;
                
            if (propellerBoats != null)
                propellerBoats.enabled = true;
            
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
}