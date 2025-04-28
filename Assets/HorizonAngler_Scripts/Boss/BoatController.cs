using UnityEngine;

public class BoatController : MonoBehaviour
{
    public PropellerBoats ship;
    public bool isFishing = false;
    public bool controlsActive = true;
    public FloatingGameEntityRealist floatingEntity;

    private void Start()
    {
        // Find the floating entity if not assigned
        if (floatingEntity == null)
            floatingEntity = GetComponent<FloatingGameEntityRealist>();
            
        // Verify center of mass is set correctly
        VerifyCenterOfMass();
    }
    
    private void VerifyCenterOfMass()
    {
        if (floatingEntity != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Check if center of mass is at default (0,0,0)
                if (rb.centerOfMass == Vector3.zero && floatingEntity.CenterOfMassOffset != Vector3.zero)
                {
                    // Reset it to the intended value
                    rb.centerOfMass = floatingEntity.CenterOfMassOffset;
                    Debug.Log($"BoatController: Fixed center of mass to {floatingEntity.CenterOfMassOffset}");
                }
            }
        }
    }
    private void Update()
    {
        if (isFishing || ship == null || !controlsActive) return;

        HandleSteering();
        HandleThrottle();
    }

    void HandleSteering()
    {
        if (Input.GetKey(KeyCode.A))
            ship.RudderLeft();
        else if (Input.GetKey(KeyCode.D))
            ship.RudderRight();
    }

    void HandleThrottle()
    {
        if (Input.GetKey(KeyCode.W))
        {
            // W key - apply forward thrust
            ship.ThrottleUp();
        }
        else if (Input.GetKey(KeyCode.S))
        {
            // S key - apply reverse thrust or brake if moving forward
            if (ship.engine_rpm > 0)
            {
                // If moving forward, brake first
                ship.Brake();
            }
            else
            {
                // Otherwise, increase reverse thrust
                ship.ThrottleDown();
            }
        }
        else
        {
            // No keys pressed - always apply brake
            ship.Brake();
        }
    }

    public void SetFishingState(bool fishing)
    {
        isFishing = fishing;
    }
}