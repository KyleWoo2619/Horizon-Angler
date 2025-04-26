using UnityEngine;
using UnityEngine.Splines;

public class BossSplineFishMovement : MonoBehaviour
{
    [Header("Vertical Movement")]
    public float verticalAmplitude = 0.5f;
    public float verticalSpeed = 1.0f;
    
    [Header("Randomization")]
    public float speedVariation = 0.3f;
    public float directionChangeTime = 3.0f;
    
    [Header("References")]
    public SplineAnimate splineAnimate;
    
    private Vector3 splinePosition;
    private float randomSpeedMultiplier = 1.0f;
    private float timer = 0f;
    private bool isMovingOnSpline = true; // Assume it's moving by default
    
    void Start()
    {
        SetNewRandomValues();
        
        // Subscribe only to Completed event
        if (splineAnimate != null)
        {
            splineAnimate.Completed += OnSplineCompleted;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (splineAnimate != null)
        {
            splineAnimate.Completed -= OnSplineCompleted;
        }
    }
    
    void OnSplineCompleted()
    {
        // When spline completes, stop the vertical movement
        isMovingOnSpline = false;
    }
    
    void LateUpdate()
    {
        // Skip the vertical movement if explicitly not moving on spline
        if (!isMovingOnSpline) return;
        
        // Get the base position from the transform (already positioned by SplineAnimate)
        splinePosition = transform.position;
        
        // Update timer for random changes
        timer += Time.deltaTime;
        if (timer >= directionChangeTime)
        {
            SetNewRandomValues();
            timer = 0f;
        }
        
        // Apply vertical bobbing movement
        float verticalOffset = Mathf.Sin(Time.time * verticalSpeed * randomSpeedMultiplier) * verticalAmplitude;
        
        // Apply the movement on top of the spline position
        transform.position = new Vector3(
            splinePosition.x,
            splinePosition.y + verticalOffset,
            splinePosition.z
        );
    }
    
    void SetNewRandomValues()
    {
        randomSpeedMultiplier = Random.Range(1.0f - speedVariation, 1.0f + speedVariation);
        directionChangeTime = Random.Range(2.0f, 4.0f);
    }
    
    // Public method to manually set movement state
    public void SetMovementEnabled(bool enabled)
    {
        isMovingOnSpline = enabled;
    }
    
    // Call this when a new spline phase starts
    public void OnSplineStart()
    {
        isMovingOnSpline = true;
    }
}