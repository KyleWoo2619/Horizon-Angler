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
    
    private void Start()
    {
        // Subscribe to the completed event if possible
        if (splineAnimate != null)
        {
            splineAnimate.Completed += OnSplineCompleted;
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
        
        // Start a delay before moving to the next phase
    //     if (!isWaitingForPhaseComplete)
    //     {
    //         StartCoroutine(WaitBeforeNextPhase());
    //     }
    // }
    
    // private IEnumerator WaitBeforeNextPhase()
    // {
    //     isWaitingForPhaseComplete = true;
        
    //     // Wait a moment between phases
    //     yield return new WaitForSeconds(1.0f);
        
    //     // If there are more phases, automatically play the next one
    //     if (currentPhase < phases.Length)
    //     {
    //         PlayNextPhase();
    //     }
        
    //     isWaitingForPhaseComplete = false;
    }

    public void PlayNextPhase()
    {
        if (currentPhase < phases.Length)
        {
            // Trigger sound effects BEFORE starting the spline
            if (musicManager != null)
            {
                musicManager.OnSplinePhaseStart(currentPhase);
            }

            splineAnimate.Container = phases[currentPhase].spline;
            splineAnimate.Duration = phases[currentPhase].duration;
            
            // Enable fish movement for the new phase
            if (fishMovement != null)
            {
                fishMovement.OnSplineStart();
            }
            
            splineAnimate.Restart(true);
            currentPhase++;
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
}

[System.Serializable]
public struct BossPhase
{
    public SplineContainer spline;
    public float duration;
}