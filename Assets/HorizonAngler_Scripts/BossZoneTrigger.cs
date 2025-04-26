using System.Collections;
using UnityEngine;

public class BossZoneTrigger : MonoBehaviour
{
    public BossSplinePhaseManager bossManager;
    public BoatForcedDocking boatDocking;
    public BoatController boatController;
    public PropellerBoats propellerBoats;
    public BoatCameraFollow boatCameraFollow;
    public Transform dockingTargetPoint;
    public Transform bossCameraTarget;
    public BossMusicManager musicManager;

    private Collider triggerCollider;
    private Renderer triggerRenderer;
    private bool dockingStarted = false;
    private Camera mainCamera;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        triggerRenderer = GetComponent<Renderer>();
        mainCamera = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !dockingStarted)
        {
            dockingStarted = true;

            // Play one-shot ambient sound when player enters the trigger zone
            if (musicManager != null)
            {
                musicManager.OnTriggerEntered();
            }
            else if (bossManager != null && bossManager.musicManager != null)
            {
                bossManager.musicManager.OnTriggerEntered();
            }

            if (boatController != null)
                boatController.enabled = false;

            if (propellerBoats != null)
                propellerBoats.enabled = false;

            if (boatDocking != null && dockingTargetPoint != null)
            {
                boatDocking.BeginDocking(dockingTargetPoint);
                StartCoroutine(WaitForDockingAndCamera());
            }

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

    private IEnumerator WaitForDockingAndCamera()
    {
        // Wait until boat finishes docking
        while (boatDocking.isDocking)
        {
            yield return null;
        }

        // Smoothly rotate boat towards docking Y rotation
        yield return StartCoroutine(RotateBoatToTargetY());

        // FULLY disable BoatCameraFollow script to stop LateUpdate interference
        if (boatCameraFollow != null)
            boatCameraFollow.enabled = false;  // <-- critical

        // Now safely move camera manually
        if (bossCameraTarget != null && mainCamera != null)
        {
            yield return StartCoroutine(MoveCameraToTarget());
        }

        // Start the boss music AFTER camera movement is complete
        if (musicManager != null)
        {
            Debug.Log("Initiating boss music from trigger after camera move");
            musicManager.InitiateBossMusic();
        }
        else if (bossManager != null && bossManager.musicManager != null)
        {
            Debug.Log("Initiating boss music from boss manager after camera move");
            bossManager.musicManager.InitiateBossMusic();
        }
        else
        {
            Debug.LogError("No music manager found - both direct reference and boss manager reference are null");
        }

        // Small delay before starting spline animation
        yield return new WaitForSeconds(0.5f);

        // Only after moving the camera, trigger boss entrance
        bossManager.PlayNextPhase();
    }

    private IEnumerator RotateBoatToTargetY()
    {
        Quaternion startRot = boatDocking.transform.rotation;
        Quaternion targetRot = Quaternion.Euler(
            startRot.eulerAngles.x,
            dockingTargetPoint.rotation.eulerAngles.y,
            startRot.eulerAngles.z);

        float elapsed = 0f;
        float rotateDuration = 2f;

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            boatDocking.transform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / rotateDuration);
            yield return null;
        }

        boatDocking.transform.rotation = targetRot;
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
        float moveDuration = 2f;

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
}
