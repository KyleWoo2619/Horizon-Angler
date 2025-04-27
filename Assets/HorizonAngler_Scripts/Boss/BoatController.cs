using UnityEngine;

public class BoatController : MonoBehaviour
{
    public PropellerBoats ship;
    public bool isFishing = false;
    public bool controlsActive = true;

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