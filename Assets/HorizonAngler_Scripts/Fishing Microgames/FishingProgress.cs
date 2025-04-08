using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static InitiateMicrogames;

public class FishingProgress : MonoBehaviour
{
    public float progress;
    public Slider progressSlider;
    [HideInInspector] public Test2Script T2S;

    // Decay variables
    [HideInInspector] public float baseDecayRate = 3f;
    public float minDecayMultiplier = 1f;
    public float maxDecayMultiplier = 2f;

    public bool memorySetupActive = false;
    Dictionary<string, float> setDecayWeights = new Dictionary<string, float>()
    {
        { "Set1", -0.5f },
        { "Set3", 1.3f },
        { "Set4", 0.5f },
        { "Set5", 1f },
        { "Set7", 1.8f }
    };

    public static FishingProgress Instance;

    [Header("Passive Progress Settings")]
    public float passiveProgressRate = 1f;

    [Header("Fish Caught UI")]
    public GameObject fishCaughtCanvas;
    public Image fishCaughtImage;
    public TextMeshProUGUI fishCaughtText;

    [System.Serializable]
    public class Fish
    {
        public string fishName;
        public Sprite fishSprite;
    }

    public List<Fish> pondFishPool = new List<Fish>();    
    public List<Fish> riverFishPool = new List<Fish>();   
    public List<Fish> oceanFishPool = new List<Fish>();   
    public List<Fish> bossPondFishPool = new List<Fish>();
    public List<Fish> bossRiverFishPool = new List<Fish>();
    public List<Fish> bossOceanFishPool = new List<Fish>();

    private List<Fish> activeFishPool;

    private Fish currentCaughtFish;
    public bool fishCaughtScreenActive = false;

    [System.Serializable]
    public class FishingLogEntry
    {
        public string fishName;
        public Sprite fishSprite;
        public string dateCaught;
        public string timeCaught;

        public FishingLogEntry(string name, Sprite sprite, string date, string time)
        {
            fishName = name;
            fishSprite = sprite;
            dateCaught = date;
            timeCaught = time;
        }
    }

    [Header("Fishing Log")]
    public List<FishingLogEntry> fishingLog = new List<FishingLogEntry>();

    private bool hasCaughtFish = false;

    private float fishCaughtInputDelay = 0.5f; // Half second delay
    private float fishCaughtTimer = 0f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        T2S = GetComponent<Test2Script>();
        Initialize();
        fishCaughtCanvas.SetActive(false);
    }

    public void Initialize()
    {
        progress = 50f;
        hasCaughtFish = false;
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

        if (fishCaughtScreenActive)
        {
            fishCaughtTimer += Time.deltaTime;
            if (fishCaughtTimer > fishCaughtInputDelay && Input.anyKeyDown)
            {
                HideFishCaughtScreen();
            }
        }
    }

    void HideFishCaughtScreen()
    {
        fishCaughtCanvas.SetActive(false);
        fishCaughtScreenActive = false;

        // Hide the Microgame UI
        if (InitiateMicrogames.Instance != null)
        {
            InitiateMicrogames.Instance.MGCanvas.SetActive(false);
            InitiateMicrogames.Instance.CCanvas.SetActive(true); // Show Cast Prompt Canvas
            InitiateMicrogames.Instance.StartCastLockout();
        }
    }

    public void Set1ObstaclePenalty()
    {
        progress -= 5f;
    }

    void PassiveProgress()
    {
        if (!T2S.microgamesActive) // Only gain passive progress when microgames are inactive
        {
            progress += passiveProgressRate * Time.deltaTime;
        }
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
        if (progress >= 100f && !hasCaughtFish)
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

        progress -= totalDecay * baseDecayRate * Time.deltaTime * normalizedMultiplier;
    }

    void OnProgressMax()
    {
        Debug.Log("Player successfully caught the fish!");

        hasCaughtFish = true;

        PickRandomFish();
        ShowFishCaughtScreen();

        T2S.microgamesActive = false;
        T2S.ClearAll();
    }

    void OnProgressMin()
    {
        Debug.Log("Player failed to catch the fish...");
        T2S.microgamesActive = false;
        T2S.ClearAll();
    }

    void PickRandomFish()
    {
        if (activeFishPool == null || activeFishPool.Count == 0)
        {
            Debug.LogWarning("Active fish pool is empty!");
            return;
        }

        int index = Random.Range(0, activeFishPool.Count);
        currentCaughtFish = activeFishPool[index];
    }

    public void SetActiveFishPool(FishZoneType zoneType)
    {
        switch (zoneType)
        {
            case FishZoneType.Pond:
                activeFishPool = pondFishPool;
                break;
            case FishZoneType.River:
                activeFishPool = riverFishPool;
                break;
            case FishZoneType.Ocean:
                activeFishPool = oceanFishPool;
                break;
            case FishZoneType.BossPond:
                activeFishPool = bossPondFishPool;
                break;
            case FishZoneType.BossRiver:
                activeFishPool = bossRiverFishPool;
                break;
            case FishZoneType.BossOcean:
                activeFishPool = bossOceanFishPool;
                break;
            default:
                activeFishPool = pondFishPool;
                break;
        }
    }

    void ShowFishCaughtScreen()
    {
        if (currentCaughtFish != null && fishCaughtImage != null && fishCaughtText != null)
        {
            fishCaughtCanvas.SetActive(true);
            fishCaughtImage.sprite = currentCaughtFish.fishSprite;
            fishCaughtText.text = $"Caught {currentCaughtFish.fishName}!";
            fishCaughtScreenActive = true;
            fishCaughtTimer = 0f;
            // Save to Fishing Log
            string caughtTime = System.DateTime.Now.ToString("HH:mm:ss");
            string caughtDate = System.DateTime.Now.ToString("MM/dd/yyyy");

            fishingLog.Add(new FishingLogEntry(
                currentCaughtFish.fishName,
                currentCaughtFish.fishSprite,
                caughtDate,
                caughtTime
            ));

            Debug.Log($"Caught {currentCaughtFish.fishName} at {caughtTime} on {caughtDate}");
        }
    }
}