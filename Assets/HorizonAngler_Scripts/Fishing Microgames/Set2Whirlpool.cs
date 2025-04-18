using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Set2Whirlpool : MonoBehaviour
{
    public Slider progressSlider;
    public RectTransform whirlpoolVisual;
    public ParticleSystem splashParticles; // Optional: assign splash FX here
    public Image whirlpoolGlow; // Optional: assign visual UI glow here

    [Header("Tuning")]
    public float spinThreshold = 0.1f;
    public float spinMultiplier = 1f;
    public float decayRate = 300f;
    public float totalSpinNeeded = 3600f;
    public float visualSpinSmoothing = 5f;
    public float momentumDamping = 4f; // How quickly the momentum slows down

    private float currentSpin = 0f;
    private float accumulatedAngle = 0f;
    private float visualRotation = 0f;
    private float visualMomentum = 0f; // Degrees per second
    private float angularSpeed = 0f;

    private Vector2 lastInput = Vector2.right;

    void Update()
    {
        // Prevent divide-by-zero or NaN issues when unpausing
        if (Time.deltaTime < 0.0001f)
            return;


        var t2s = InitiateMicrogames.Instance.T2S;

        if (!t2s.microgamesActive || !t2s.Sets["Set2"])
            return;

        Vector2 currentInput = new Vector2(t2s.inputRAxisX, t2s.inputRAxisY);
        float angle = Vector2.SignedAngle(lastInput.normalized, currentInput.normalized);
        float deltaTime = Time.deltaTime;

        if (currentInput.magnitude > spinThreshold && Mathf.Abs(angle) > 5f)
        {
            accumulatedAngle += angle;
            currentSpin += Mathf.Abs(angle) * spinMultiplier;

            // Angular speed for visual effects
            angularSpeed = Mathf.Abs(angle) / deltaTime;

            // Update visual momentum
            visualMomentum = angle / deltaTime;

            if (splashParticles && !splashParticles.isPlaying)
                splashParticles.Play();
        }
        else
        {
            currentSpin -= decayRate * deltaTime;
            angularSpeed = Mathf.Lerp(angularSpeed, 0f, deltaTime * 5f);
            visualMomentum = Mathf.Lerp(visualMomentum, 0f, deltaTime * momentumDamping);

            if (splashParticles && splashParticles.isPlaying)
                splashParticles.Stop();
        }

        currentSpin = Mathf.Max(0f, currentSpin);
        float progress = Mathf.Clamp01(currentSpin / totalSpinNeeded);

        if (progressSlider != null)
            progressSlider.value = progress;

        // Visual rotation
        if (whirlpoolVisual != null)
        {
            visualRotation += visualMomentum * deltaTime;
            visualRotation = Mathf.Repeat(visualRotation, 360f); // Keep it between 0-360

            if (!float.IsNaN(visualRotation) && !float.IsInfinity(visualRotation))
                whirlpoolVisual.rotation = Quaternion.Euler(0f, 0f, visualRotation);
        }

        // Optional glow intensity based on angular speed
        if (whirlpoolGlow != null)
        {
            float speedFactor = Mathf.Clamp01(angularSpeed / 180f);
            whirlpoolGlow.color = new Color(1f, 1f, 1f, speedFactor);
        }

        lastInput = currentInput;

        if (progress >= 1f)
        {
            Debug.Log("Set 2 Completed!");
            t2s.Sets["Set2"] = false;
            t2s.activeSets.Remove("Set2");
            t2s.inactiveSets.Add("Set2");
            t2s.Set2Parent.SetActive(false);

            FindObjectOfType<FishingProgress>().MicrogameBonus("Set2");

            // Reset state
            currentSpin = 0f;
            accumulatedAngle = 0f;
            visualRotation = 0f;
            visualMomentum = 0f;
            angularSpeed = 0f;

            if (splashParticles) splashParticles.Stop();
        }
    }
}
