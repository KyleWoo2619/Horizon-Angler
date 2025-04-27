using UnityEngine;
using System.Collections;
using UnityEngine.Splines;

public class BossSplinePhaseManager : MonoBehaviour
{
    public SplineAnimate splineAnimate;
    public BossPhase[] phases;
    public int currentPhase = 0;
    
    // Add reference to the music manager
    public BossMusicManager musicManager;
    // Add reference to fish movement
    public BossSplineFishMovement fishMovement;

    private bool isWaitingForPhaseComplete = false;

    public delegate void SplineCompletedEvent(int phaseIndex);
    public event SplineCompletedEvent OnSplinePhaseCompleted;
    
    private void Start()
    {
        // Subscribe to the completed event if possible
        if (splineAnimate != null)
        {
            splineAnimate.Completed += OnSplineCompleted;
            
            // Log the subscription
            Debug.Log("BossSplinePhaseManager subscribed to splineAnimate.Completed event");
        }
        
        // Check if we have any subscribers to our own event
        if (OnSplinePhaseCompleted != null)
        {
            Debug.Log("BossSplinePhaseManager has subscribers to OnSplinePhaseCompleted");
        }
        else
        {
            Debug.LogWarning("BossSplinePhaseManager has NO subscribers to OnSplinePhaseCompleted");
        }
    }
    
    private void OnDestroy()
    {
        if (splineAnimate != null)
        {
            splineAnimate.Completed -= OnSplineCompleted;
        }
    }
    
    private void OnSplineCompleted()
    {
        // When a spline completes, notify music manager of the phase that just completed
        if (musicManager != null && currentPhase > 0)
        {
            musicManager.OnPhaseComplete(currentPhase - 1);
        }
        
        // Notify listeners (including BossFishingManager) that the spline is complete
        if (OnSplinePhaseCompleted != null)
        {
            OnSplinePhaseCompleted(currentPhase - 1);
        }
    }

    public void PlayNextPhase()
    {
        if (currentPhase < phases.Length - 1) // Only up to 5 normal phases
        {
            Debug.Log($"Starting spline phase {currentPhase}");
            
            if (musicManager != null)
                musicManager.OnSplinePhaseStart(currentPhase);
            
            var bossFishManager = FindObjectOfType<BossFishingManager>();
            if (bossFishManager != null && bossFishManager.bossModel != null)
                bossFishManager.bossModel.SetActive(true);

            splineAnimate.Container = phases[currentPhase].spline;
            splineAnimate.Duration = phases[currentPhase].duration;
            
            if (fishMovement != null)
                fishMovement.OnSplineStart();
            
            if (!splineAnimate.gameObject.activeSelf)
                splineAnimate.gameObject.SetActive(true);

            splineAnimate.Restart(true);
            currentPhase++;
        }
        else
        {
            Debug.LogWarning("Reached end of boss phases! No more phases to play.");
        }
    }


    // NEW! Give current phase duration for waiting after docking
    public float GetCurrentPhaseDuration()
    {
        if (currentPhase - 1 >= 0 && currentPhase - 1 < phases.Length)
            return phases[currentPhase - 1].duration;
        else
            return 0f;
    }

    public void PlayDeathPhase()
    {
        if (phases.Length > 5) // Make sure you have at least 6 splines (0-5)
        {
            Debug.Log("Playing Death Spline Phase!");

            splineAnimate.Container = phases[5].spline;
            splineAnimate.Duration = phases[5].duration;
            
            if (splineAnimate.gameObject.activeSelf == false)
                splineAnimate.gameObject.SetActive(true);
            
            splineAnimate.Restart(true);
        }
        else
        {
            Debug.LogError("No Death Phase spline assigned!");
        }
    }

}

[System.Serializable]
public struct BossPhase
{
    public SplineContainer spline;
    public float duration;
}