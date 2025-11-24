using UnityEngine;
using UnityEditor;

public class PropellerBoats : MonoBehaviour
{
    public Transform[] propellers;
    public Transform[] rudder;
    private Rigidbody rb;

    public float engine_rpm { get; private set; }
    float throttle;

    public float propellers_constant = 0.6F;
    public float engine_max_rpm = 600.0F;
    public float acceleration_cst = 1.0F;
    public float drag = 0.01F;

    public bool controlsActive = true;

    float angle;
    Quaternion[] rudderBaseRotation;

    void Awake()
    {
        engine_rpm = 0F;
        throttle = 0F;
        rb = GetComponent<Rigidbody>();
        angle = 0f;

        // Store initial rudder rotation to use as offset
        rudderBaseRotation = new Quaternion[rudder.Length];
        for (int i = 0; i < rudder.Length; i++)
        {
            rudderBaseRotation[i] = rudder[i].localRotation;
        }
    }

    void Update()
    {
        if (!controlsActive) return;
        
        // Visual propeller rotation only
        float frame_rpm = engine_rpm * Time.deltaTime;
        for (int i = 0; i < propellers.Length; i++)
        {
            propellers[i].localRotation = Quaternion.Euler(
                propellers[i].localRotation.eulerAngles + new Vector3(0, 0, -frame_rpm)
            );
        }

        // Visual rudder rotation
        for (int i = 0; i < rudder.Length; i++)
        {
            rudder[i].localRotation = rudderBaseRotation[i] * Quaternion.Euler(0, angle, 0);
        }
    }

    void FixedUpdate()
    {
        if (!controlsActive) return;

        // Apply thrust forces (physics)
        for (int i = 0; i < propellers.Length; i++)
        {
            rb.AddForceAtPosition(
                Quaternion.Euler(0, angle, 0) * -propellers[i].forward * propellers_constant * engine_rpm,
                propellers[i].position
            );
        }

        // Decay throttle
        throttle *= (1.0F - drag * 0.001F);
        engine_rpm = throttle * engine_max_rpm;

        // Gradually straighten rudder
        angle = Mathf.Lerp(angle, 0.0F, 0.02F);

        // Input Handling
        float moveInput = Input.GetAxis("Vertical");
        float steerInput = Input.GetAxis("Horizontal");

        if (moveInput > 0.1f)
            ThrottleUp();
        else if (moveInput < -0.1f)
            ThrottleDown();
        else
            Brake();

        if (steerInput > 0.1f)
            RudderRight();
        else if (steerInput < -0.1f)
            RudderLeft();
    }

    public void ThrottleUp()
    {
        throttle += acceleration_cst * 0.001F;
        throttle = Mathf.Clamp(throttle, -1f, 1f);
    }

    public void ThrottleDown()
    {
        throttle -= acceleration_cst * 0.001F;
        throttle = Mathf.Clamp(throttle, -1f, 1f);
    }

    public void Brake()
    {
        throttle = Mathf.MoveTowards(throttle, 0f, acceleration_cst * 0.002f);
    }

    public void RudderRight()
    {
        angle -= 0.3F;
        angle = Mathf.Clamp(angle, -90F, 90F);
    }

    public void RudderLeft()
    {
        angle += 0.3F;
        angle = Mathf.Clamp(angle, -90F, 90F);
    }

    void OnDrawGizmos()
    {
    #if UNITY_EDITOR
        if (propellers != null && propellers.Length > 0)
        {
            UnityEditor.Handles.Label(propellers[0].position, engine_rpm.ToString());
        }
    #endif
    }
}
