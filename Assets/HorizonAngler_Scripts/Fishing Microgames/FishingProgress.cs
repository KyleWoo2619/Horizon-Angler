using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishingProgress : MonoBehaviour
{
    public float progress;
    public Slider progressSlider;
    [HideInInspector] public Test2Script T2S;

    // Decay variables
    public float baseDecayRate = 0.3f;
    public float minDecayMultiplier = 0.5f;
    public float maxDecayMultiplier = 2f;

    public bool memorySetupActive = false;
    Dictionary<string, float> setDecayWeights = new Dictionary<string, float>()
    {
        { "Set1", -0.2f },
        { "Set3", 1f },
        { "Set4", 0.3f },
        { "Set5", 1f },
        { "Set7", 1.2f }
    };

    public static FishingProgress Instance;

    [Header("Passive Progress Settings")]
    public float passiveProgressRate = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        T2S = GetComponent<Test2Script>();
        Initialize();
    }

    public void Initialize()
    {
        progress = 50f;
    }

    void Update()
    {
        ProgressSliderVisual();
        ProgressTracker();

        if (T2S.microgamesActive && IsAnyMicrogameTrulyActive())
        {
            ProgressDecay();
        }
        else
        {
            PassiveProgress();
        }

        // Always clamp progress to 0-100
        progress = Mathf.Clamp(progress, 0f, 100f);
    }

    public void Set1ObstaclePenalty()
    {
        progress -= 5f;
    }

    void PassiveProgress()
    {
        progress += passiveProgressRate * Time.deltaTime;
    }

    bool IsAnyMicrogameTrulyActive()
    {
        foreach (var set in T2S.Sets)
        {
            if (set.Value == true)
                return true;
        }
        return false;
    }

    public void MicrogameBonus()
    {
        progress += 15f;
    }

    void ProgressSliderVisual()
    {
        progressSlider.value = progress;
    }

    void ProgressTracker()
    {
        if (progress >= 100f)
        {
            OnProgressMax();
        }
        else if (progress <= 0f)
        {
            OnProgressMin();
        }
    }

    void ProgressDecay()
    {
        if (!T2S.microgamesActive)
            return;

        float totalDecay = 0f;
        foreach (string setName in T2S.activeSets)
        {
            if (T2S.Sets.ContainsKey(setName) && T2S.Sets[setName])
            {
                if (setDecayWeights.ContainsKey(setName))
                {
                    totalDecay += setDecayWeights[setName];
                }
                else
                {
                    totalDecay += 1f;
                }
            }
        }

        float activeCount = T2S.activeSets.Count;
        float normalizedMultiplier = Mathf.Lerp(minDecayMultiplier, maxDecayMultiplier, activeCount / 5f);

        progress -= totalDecay * baseDecayRate * normalizedMultiplier * Time.deltaTime;
    }

    void OnProgressMax()
    {
        Debug.Log("Player successfully caught the fish!");
        T2S.microgamesActive = false;
        T2S.ClearAll();
    }

    void OnProgressMin()
    {
        Debug.Log("Player failed to catch the fish...");
        T2S.microgamesActive = false;
        T2S.ClearAll();
    }
}