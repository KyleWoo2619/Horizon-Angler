using UnityEngine;

public class BoatStabilizer : MonoBehaviour
{
    public Vector3 desiredCenterOfMass = new Vector3(0, -3.04f, -2.11f);
    public bool applyAntiRollForces = true;
    public float antiRollForce = 10.0f;
    public float waterLevel = 0.0f;
    
    private Rigidbody rb;
    private bool hasLogged = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            ForceCenterOfMass();
        }
    }
    
    void FixedUpdate()
    {
        // Verify center of mass hasn't changed
        if (rb != null && rb.centerOfMass != desiredCenterOfMass)
        {
            ForceCenterOfMass();
        }
        
        // Optional anti-roll forces
        if (applyAntiRollForces && rb != null)
        {
            ApplyAntiRollForces();
        }
        
        // Log center of mass periodically for debugging
        if (!hasLogged && Time.frameCount % 300 == 0)
        {
            Debug.Log($"Current center of mass: {rb.centerOfMass}, World position: {rb.worldCenterOfMass}");
            hasLogged = true;
        }
        else if (Time.frameCount % 300 != 0)
        {
            hasLogged = false;
        }
    }
    
    void ForceCenterOfMass()
    {
        rb.centerOfMass = desiredCenterOfMass;
        rb.WakeUp(); // Ensure the rigidbody is active
        Debug.Log($"[BoatStabilizer] Force-set center of mass to {desiredCenterOfMass}");
    }
    
    void ApplyAntiRollForces()
    {
        // Get current rotation
        Vector3 rotation = transform.rotation.eulerAngles;
        
        // Normalize to -180 to 180 range for X and Z
        float xAngle = (rotation.x > 180) ? rotation.x - 360 : rotation.x;
        float zAngle = (rotation.z > 180) ? rotation.z - 360 : rotation.z;
        
        // Apply counter-torque to resist tilting
        if (Mathf.Abs(xAngle) > 5 || Mathf.Abs(zAngle) > 5)
        {
            Vector3 stabilizingTorque = new Vector3(-xAngle, 0, -zAngle) * antiRollForce;
            rb.AddTorque(stabilizingTorque * Time.fixedDeltaTime, ForceMode.Acceleration);
            
            if (Mathf.Abs(xAngle) > 20 || Mathf.Abs(zAngle) > 20)
            {
                Debug.LogWarning($"[BoatStabilizer] Excessive tilt detected! X: {xAngle}, Z: {zAngle}, applying correction.");
            }
        }
    }
    
    // Add this method for debugging in the inspector
    public void InspectorForceReset()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        ForceCenterOfMass();
    }
}