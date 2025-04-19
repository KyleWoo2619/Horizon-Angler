using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InitiateMicrogames;

public class FishingProgress : MonoBehaviour
{
    private HAPlayerController playerController;
    public GameObject BossfishLocation;
    public float progress;
    public Slider progressSlider;

    [HideInInspector]
    public Test2Script T2S;

    // Decay variables
    [HideInInspector]
    public float baseDecayRate = 1f;
    public float minDecayMultiplier = 1f;
    public float maxDecayMultiplier = 2f;

    public bool memorySetupActive = false;
    Dictionary<string, float> setDecayWeights = new Dictionary<string, float>()
    {
        { "Set1", -0.5f },    // Undertale
        { "Set2", 0.1f },     // Whirlpool // Placeholder until Set2 is implemented
        { "Set3", 1.3f },     // Button Combo
        { "Set4", 0.3f },     // Memory
        { "Set5", 1f },       // Mashing
        { "Set6", 0.1f },     // Rod Alignment // Placeholder until Set6 is implemented
        { "Set7", 1.8f },     // Reaction  
    };

    [Header("Microgame Bonus Values")]
    public Dictionary<string, float> microgameBonusValues = new Dictionary<string, float>()
{
    { "Set1", 15f },
    { "Set2", 15f },
    { "Set3", 15f },
    { "Set4", 15f },
    { "Set5", 15f },
    { "Set6", 25f }, // boosted value for Set6
    { "Set7", 15f }
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

    public List<Fish> tutorialFishPool = new List<Fish>();
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
        public InitiateMicrogames.FishZoneType zoneCaught;

        public FishingLogEntry(
            string name,
            Sprite sprite,
            string date,
            string time,
            InitiateMicrogames.FishZoneType zone
        )
        {
            fishName = name;
            fishSprite = sprite;
            dateCaught = date;
            timeCaught = time;
            zoneCaught = zone;
        }
    }

    [Header("Fishing Log")]
    public List<FishingLogEntry> fishingLog = new List<FishingLogEntry>();

    private bool hasCaughtFish = false;

    private float fishCaughtInputDelay = 0.5f; // Half second delay
    private float fishCaughtTimer = 0f;

    [Header("Notifications")]
    public GameObject notificationCanvas; // The whole canvas (to turn on/off)
    public TextMeshProUGUI notificationText; // The text inside it
    public TextMeshProUGUI notificationTextShadow; // The shadow text inside it
    public CanvasGroup notificationGroup;

    [HideInInspector]
    public FishZoneType activeZoneType;

    [Header("Level Settings")]
    public bool isTutorialLevel = false;
    public string nextSceneToLoad = "PondScene"; // assign in Inspector
    [SerializeField] private AudioSource musicSource;

    [Header("Cutscene")]
    public CutsceneManager cutsceneManager;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        T2S = GetComponent<Test2Script>();
        Initialize();
        fishCaughtCanvas.SetActive(false);

        if (notificationCanvas != null)
        {
            notificationCanvas.SetActive(false);
            notificationGroup.alpha = 0f;
        }
    }

    public void Initialize()
    {
        progress = 50f;
        hasCaughtFish = false;
    }

    void Update()
    {
        if (InitiateMicrogames.Instance == null || !InitiateMicrogames.Instance.MGCanvas.activeSelf)
        {
            // Player hasn't casted yet
            return;
        }

        ProgressSliderVisual();
        ProgressTracker();

        if (T2S.microgamesActive)
        {
            if (IsAnyMicrogameTrulyActive())
            {
                Debug.Log("Microgame ACTIVE: Decaying progress...");
                ProgressDecay();
            }
            else
            {
                Debug.Log("Microgame TIMER only: Passive progress...");
                PassiveProgress();
            }
        }
        else
        {
            Debug.Log("No microgames: Passive progress...");
            PassiveProgress();
        }

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

    private bool IsBossFish(string fishName)
    {
        // Just check if it's Froggy or other boss fish names
        return fishName == "Sun-Infused Croakmaw" || fishName == "OtherBossName"; // add more boss fish names here
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

        if (currentCaughtFish != null && IsBossFish(currentCaughtFish.fishName))
        {
            Debug.Log("Caught a boss fish! Playing cutscene...");
            BossfishLocation.SetActive(false);

            HAPlayerController player = FindObjectOfType<HAPlayerController>();
            if (player != null)
            {
                player.EndFishing();
                player.caughtPondBoss = true;
                player.inFishZone = false;
                player.canFish = false;

                if (player.fishingPromptUI != null)
                {
                    player.fishingPromptUI.SetActive(false);
                }
            }
            CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
            if (cutsceneManager != null)
            {
                cutsceneManager.PlayCutscene();
            }
            else
            {
                Debug.LogWarning("CutsceneManager not found in scene!");
            }
        }
    }

    public void Set1ObstaclePenalty()
    {
        progress -= 5f;
    }

    void PassiveProgress()
    {
        float passiveMultiplier = 1f;

        if (T2S.microgamesActive && !IsAnyMicrogameTrulyActive())
        {
            passiveMultiplier = 0.7f; // Slow down passive gain while timer is up
        }

        progress += passiveProgressRate * passiveMultiplier * Time.deltaTime;
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

    public void MicrogameBonus(string setName)
    {
        float bonus = microgameBonusValues.ContainsKey(setName) ? microgameBonusValues[setName] : 15f;
        progress += bonus;
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
                if (setName == "Set5") // Special rule for Set5 (button masher)
                {
                    totalDecay += GetSet5DynamicDecay();
                }
                else if (setDecayWeights.ContainsKey(setName))
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
        float normalizedMultiplier = Mathf.Lerp(
            minDecayMultiplier,
            maxDecayMultiplier,
            activeCount / 5f
        );

        progress -= totalDecay * baseDecayRate * Time.deltaTime * normalizedMultiplier;
    }

    private float GetSet5DynamicDecay()
    {
        var mashSlider = T2S.mashSlider;

        if (mashSlider == null)
        {
            Debug.LogWarning("Mash slider reference missing!");
            return setDecayWeights["Set5"]; // Fallback to normal Set5 decay
        }

        float sliderFill = mashSlider.value; // 0 to 1

        // As the mash slider fills, we want less decay.
        // We'll lerp between max decay and minimal decay.
        float maxDecay = 1.5f; // Decay when mash slider is empty
        float minDecay = 0.3f; // Decay when mash slider is full

        float dynamicDecay = Mathf.Lerp(maxDecay, minDecay, Mathf.SmoothStep(0f, 1f, sliderFill));
        return dynamicDecay;
    }

    void OnProgressMax()
    {
        if (isTutorialLevel)
        {
            StartCoroutine(PlayCutsceneThenLoad());
        }
        else
        {
            Debug.Log("Player successfully caught the fish!");
            hasCaughtFish = true;
            PickRandomFish();
            ShowFishCaughtScreen();
        }

        T2S.microgamesActive = false;
        T2S.ClearAll();
    }

    IEnumerator PlayCutsceneThenLoad()
    {
        HAPlayerController player = FindObjectOfType<HAPlayerController>();
        if (player != null)
        {
            player.EndFishing();
            player.inFishZone = false;
            player.canFish = false;
            if (player.fishingPromptUI != null)
                player.fishingPromptUI.SetActive(false);
        }

        // Fade out music if available
        AudioSource music = GameObject.FindWithTag("Music")?.GetComponent<AudioSource>();
        if (music != null)
        {
            float startVol = music.volume;
            float duration = 1.5f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                music.volume = Mathf.Lerp(startVol, 0f, time / duration);
                yield return null;
            }
            music.volume = 0f;
        }

        // Play cutscene
        if (cutsceneManager != null)
        {
            cutsceneManager.PlayCutscene();
            while (cutsceneManager.IsCutscenePlaying())
            {
                yield return null;
            }
        }

        // Load next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneToLoad);
    }

    void OnProgressMin()
    {
        Debug.Log("Player failed to catch the fish...");
        T2S.microgamesActive = false;
        T2S.ClearAll();

        HAPlayerController player = FindObjectOfType<HAPlayerController>();
        if (player != null)
        {
            player.PlayFishingIdle();
        }
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
            case FishZoneType.Tutorial:
                activeFishPool = tutorialFishPool; 
                break;
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

            HAPlayerController player = FindObjectOfType<HAPlayerController>();
            fishingLog.Add(
                new FishingLogEntry(
                    currentCaughtFish.fishName,
                    currentCaughtFish.fishSprite,
                    caughtDate,
                    caughtTime,
                    activeZoneType
                )
            );

            Debug.Log($"Caught {currentCaughtFish.fishName} at {caughtTime} on {caughtDate}");

            if (player != null)
            {
                player.PlayFishingIdle();
            }
        }

        CheckUnlockBossFishing();
    }

    public int GetNormalFishCaughtCount(InitiateMicrogames.FishZoneType zoneType)
    {
        int count = 0;
        foreach (var entry in fishingLog)
        {
            if (entry.zoneCaught == zoneType)
            {
                count++;
            }
        }
        return count;
    }

    void CheckUnlockBossFishing()
    {
        HAPlayerController player = FindObjectOfType<HAPlayerController>();

        if (fishingLog.Count == 0)
            return;

        // Use the zone of the fish you just caught
        InitiateMicrogames.FishZoneType fishZone = fishingLog[fishingLog.Count - 1].zoneCaught;

        bool bossUnlocked = false;

        if (player != null)
        {
            switch (fishZone)
            {
                case InitiateMicrogames.FishZoneType.Pond:
                    if (!player.canFishPondBoss && GetNormalFishCaughtCount(fishZone) >= 3)
                    {
                        player.canFishPondBoss = true;
                        bossUnlocked = true;
                    }
                    break;
                case InitiateMicrogames.FishZoneType.River:
                    if (!player.canFishRiverBoss && GetNormalFishCaughtCount(fishZone) >= 3)
                    {
                        player.canFishRiverBoss = true;
                        bossUnlocked = true;
                    }
                    break;
                case InitiateMicrogames.FishZoneType.Ocean:
                    if (!player.canFishOceanBoss && GetNormalFishCaughtCount(fishZone) >= 3)
                    {
                        player.canFishOceanBoss = true;
                        bossUnlocked = true;
                    }
                    break;
            }
        }

        if (bossUnlocked)
        {
            string zoneName = "";

            switch (fishZone)
            {
                case InitiateMicrogames.FishZoneType.Pond:
                    zoneName = "You feel a sudden stir in the water. A new fishing spot appeared.";
                    break;
                case InitiateMicrogames.FishZoneType.River:
                    zoneName = "River Boss";
                    break;
                case InitiateMicrogames.FishZoneType.Ocean:
                    zoneName = "Ocean Boss";
                    break;
            }

            Debug.Log($"{zoneName} fishing is now UNLOCKED!");
            ShowNotification(zoneName);
        }
    }

    public void ShowNotification(string message)
    {
        Debug.Log($"Trying to show notification: {message}");

        if (notificationCanvas != null && notificationText != null)
        {
            Debug.Log("Notification references are good! Showing message.");
            notificationText.text = message;
            notificationTextShadow.text = message;
            StopAllCoroutines(); // Stop previous notification fades if any
            StartCoroutine(NotificationRoutine());
        }
        else
        {
            Debug.LogWarning("NotificationCanvas or NotificationText is missing!");
        }
    }

    public IEnumerator NotificationRoutine()
    {
        Debug.Log("Starting NotificationRoutine...");

        notificationCanvas.SetActive(true);

        if (notificationGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(notificationGroup, 0f, 1f, 0.5f));

            yield return new WaitForSeconds(2f);

            yield return StartCoroutine(FadeCanvasGroup(notificationGroup, 1f, 0f, 0.5f));
        }
        else
        {
            Debug.LogWarning("NotificationGroup is missing!");
        }

        notificationCanvas.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float start,
        float end,
        float duration
    )
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
