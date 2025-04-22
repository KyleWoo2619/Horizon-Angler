using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Set6RodAlignment : MonoBehaviour
{
    private Test2Script gameManager;

    [Header("Rod Settings")]
    public Transform rod;
    public Slider progressSlider;
    public Image progressFillImage;

    [Header("Behavior Settings")]
    public float rotationSpeed = 100f;
    public float friction = 1f;
    public float inputAcceleration = 50f;
    public float idleJerkCooldown = 0.5f;
    public float progressGainRate = 0.4f;
    public float progressLossMultiplier = 2f;
    public float progressLossOnHit = 20f;

    private float currentAngularVelocity = 0f;
    private float idleTimer = 0f;
    private bool inputDetectedThisFrame = false;
    private float currentZAngle = 0f;


    void Start()
    {
        gameManager = FindObjectOfType<Test2Script>();

        float initialJerk = Random.value < 0.5f ? -75f : 75f;
        currentAngularVelocity = initialJerk;

        StartCoroutine(ResetSliderOnStart());
    }

    IEnumerator ResetSliderOnStart()
    {
        yield return null;
        if (progressSlider != null)
            progressSlider.value = 0f;
    }

    void Update()
    {
        if (gameManager == null || !gameManager.microgamesActive || !gameManager.Sets.ContainsKey("Set6") || !gameManager.Sets["Set6"])
            return;

        HandleInput();
        ApplyRotation();
        UpdateProgress();
    }

    void HandleInput()
    {
        float input = 0f;
        if (Input.GetButton("LMB")) input -= 1f;
        if (Input.GetButton("RMB")) input += 1f;
        if (Input.GetButton("LT")) input -= 1f;
        if (Input.GetButton("RT")) input += 1f;

        if (input != 0f)
        {
            currentAngularVelocity += input * inputAcceleration * Time.deltaTime;
            inputDetectedThisFrame = true;

            if (!(currentZAngle >= 50f && currentZAngle <= 80f))
                idleTimer = 0f;
        }
        else
        {
            inputDetectedThisFrame = false;
        }
    }


    void ApplyRotation()
    {
        currentAngularVelocity = Mathf.MoveTowards(currentAngularVelocity, 0f, friction * Time.deltaTime);

        if (!inputDetectedThisFrame)
        {
            float wobble = Mathf.Sin(Time.time * 2f) * 10f;
            currentAngularVelocity += wobble * Time.deltaTime;
        }

        rod.Rotate(Vector3.forward, -currentAngularVelocity * Time.deltaTime);

        // Clamp AFTER rotation is applied
        Vector3 currentRotation = rod.eulerAngles;
        float clampedZ = Mathf.Clamp(currentRotation.z, 35f, 95f);
        rod.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, clampedZ);

        // Store clamped angle to reuse in other logic
        currentZAngle = clampedZ;
        if (currentZAngle > 180f) currentZAngle -= 360f;

        if (currentZAngle != currentRotation.z)
            currentAngularVelocity = 0f;

        bool inIdleZone = currentZAngle >= 50f && currentZAngle <= 80f;

        if (!inputDetectedThisFrame || inIdleZone)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleJerkCooldown)
            {
                float jerk = inputDetectedThisFrame && inIdleZone
                    ? Random.Range(-90f, 90f) // Stronger jerk if holding input in idle zone
                    : Random.Range(-50f, 50f); // Default wobble if no input

                currentAngularVelocity += jerk;
                idleTimer = 0f;
            }
        }

    }



    void UpdateProgress()
    {
        float zAngle = rod.eulerAngles.z;
        if (zAngle < 0f) zAngle += 360f;

        bool isInSafeZone = (zAngle >= 50f && zAngle <= 80f);
        float targetRate = isInSafeZone ? progressGainRate : -progressGainRate * progressLossMultiplier;

        progressSlider.value += targetRate * Time.deltaTime;
        progressSlider.value = Mathf.Clamp01(progressSlider.value);

        if (progressFillImage != null)
        {
            if (isInSafeZone)
            {
                float pulse = Mathf.PingPong(Time.time * 2f, 0.3f);
                progressFillImage.color = new Color(0.6f - pulse, 1f, 0.6f - pulse);
            }
            else
            {
                progressFillImage.color = new Color(1f, 0.4f, 0.4f);
            }
        }
    }

    public void OnRodHit()
    {
        progressSlider.value -= progressLossOnHit / 100f;
        progressSlider.value = Mathf.Clamp01(progressSlider.value);
    }

    public bool IsMicrogameComplete()
    {
        return progressSlider.value >= 1f;
    }

    public void ResetRod()
    {
        if (progressSlider != null)
            progressSlider.value = 0f;

        currentAngularVelocity = 0f;
        idleTimer = 0f;
        inputDetectedThisFrame = false;

        // Reset rotation
        rod.localRotation = Quaternion.Euler(0f, 0f, 65f); // or your default angle
    }

}
