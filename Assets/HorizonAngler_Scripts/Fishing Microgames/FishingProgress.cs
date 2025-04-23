using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InitiateMicrogames;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class FishingProgress : MonoBehaviour
{
    private HAPlayerController playerController;
    public GameObject BossfishLocation;
    public GameObject TutorialFishLocation;
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
        { "Set2", 1.5f },     // Whirlpool 
        { "Set3", 1.3f },     // Button Combo
        { "Set4", 0.3f },     // Memory
        { "Set5", 1f },       // Mashing
        { "Set6", 0.1f },     // Rod Alignment 
        { "Set7", 1.8f },     // Reaction  
    };

    [Header("Microgame Bonus Values")]
    public Dictionary<string, float> microgameBonusValues = new Dictionary<string, float>()
    {
        { "Set1", 15f }, // Undertale
        { "Set2", 22f }, // Whirlpool 
        { "Set3", 15f }, // Button Combo
        { "Set4", 30f }, // Memory
        { "Set5", 20f }, // Mashing
        { "Set6", 25f }, // Rod Alignment
        { "Set7", 20f }  // Reaction
    };


    public static FishingProgress Instance;

    [Header("Passive Progress Settings")]
    public float passiveProgressRate = 1f;

    [Header("Fish Caught UI")]
    public Image fishCaughtImage;
    public TextMeshProUGUI fishCaughtText;

    [Header("Caught Screen Variants")]
    public GameObject basicWinScreen;
    public GameObject basicLoseScreen;
    public GameObject bossCatchScreen;

    [System.Serializable]
    public class Fish
    {
        public string fishName;
        public Sprite fishSprite;
        public string description;
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
    public bool progressMaxHandled = false;

    private float fishCaughtInputDelay = 0.5f; // Half second delay
    private float fishCaughtTimer = 0f;

    [Header("Notifications")]
    public GameObject notificationCanvas; // The whole canvas (to turn on/off)
    public TextMeshProUGUI notificationText; // The text inside it
    public TextMeshProUGUI notificationTextShadow; // The shadow text inside it
    public CanvasGroup notificationGroup;

    [HideInInspector]
    public FishZoneType activeZoneType;
    private bool isShowingBossUnlockNotification = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        T2S = GetComponent<Test2Script>();
        Initialize();
        basicWinScreen?.SetActive(false);
        basicLoseScreen?.SetActive(false);
        bossCatchScreen?.SetActive(false);

        if (notificationCanvas != null)
        {
            notificationCanvas.SetActive(false);
            notificationGroup.alpha = 0f;
        }

        FishBossAI bossAI = FindObjectOfType<FishBossAI>();
        if (bossAI != null)
        {
            bossAI.DisableIfBossCaught();

            var save = GameManager.Instance.currentSaveData;
            bool goToBoss = save.currentLevel switch
            {
                "Pond" => save.canFishPondBoss,
                "River" => save.canFishRiverBoss,
                "Ocean" => save.canFishOceanBoss,
                _ => false
            };

            if (goToBoss)
                bossAI.GoToBossLocation();
        }
    }

    public void Initialize()
    {
        progress = 50f;
        hasCaughtFish = false;
        progressMaxHandled = false;
    }

    void Update()
    {
        // Always check if we're waiting on player input to hide win/fail screen
        if (fishCaughtScreenActive)
        {
            fishCaughtTimer += Time.unscaledDeltaTime;

            // Add check for boss notification here
            if (fishCaughtTimer > fishCaughtInputDelay && Input.anyKeyDown)
            {
                Debug.Log("[FishingProgress] Hiding win/lose screen due to input.");
                
                // Check if notification is active - if so, don't hide yet
                if (!isShowingBossUnlockNotification)
                {
                    HideFishCaughtScreen();
                }
                else
                {
                    Debug.Log("[FishingProgress] Can't hide screen during boss notification - ignoring input");
                    
                    // Special case for Exit Fishing (X key)
                    if (Input.GetKeyDown(KeyCode.X))
                    {
                        Debug.Log("Exit Fishing pressed during boss notification - forcing cleanup");
                        
                        // Force cleanup
                        isShowingBossUnlockNotification = false;
                        fishCaughtScreenActive = false;
                        
                        basicWinScreen?.SetActive(false);
                        basicLoseScreen?.SetActive(false);
                        bossCatchScreen?.SetActive(false);
                        
                        // Let the player exit fishing normally
                        HAPlayerController player = FindObjectOfType<HAPlayerController>();
                        if (player != null)
                        {
                            player.EndFishing();
                        }
                    }
                }
            }
        }

        // Skip update unless we're fishing
        if (InitiateMicrogames.Instance == null || !InitiateMicrogames.Instance.MGCanvas.activeSelf)
        {
            return;
        }

        ProgressSliderVisual();
        ProgressTracker();

        if (T2S.microgamesActive)
        {
            if (IsAnyMicrogameTrulyActive())
            {
                // Debug.Log("Microgame ACTIVE: Decaying progress...");
                ProgressDecay();
            }
            else
            {
                // Debug.Log("Microgame TIMER only: Passive progress...");
                PassiveProgress();
            }
        }
        else
        {
            // Debug.Log("No microgames: Passive progress...");
            PassiveProgress();
        }

        progress = Mathf.Clamp(progress, 0f, 100f);
    }


    private bool IsBossFish(string fishName)
    {
        // Just check if it's Froggy or other boss fish names
        return fishName == "Sun-Infused Croakmaw" || fishName == "OtherBossName"; // add more boss fish names here
    }

    void HideFishCaughtScreen()
    {
        Debug.Log("[FishingProgress] Hiding win/lose screen due to input.");
        
        // Prevent hiding during boss notification
        if (isShowingBossUnlockNotification)
        {
            Debug.Log("[FishingProgress] Can't hide screen during boss notification - ignoring input");
            return;
        }

        Debug.Log($"Screen states before hiding - basicWinScreen: {(basicWinScreen?.activeSelf)}, " +
                $"basicLoseScreen: {(basicLoseScreen?.activeSelf)}, " + 
                $"bossCatchScreen: {(bossCatchScreen?.activeSelf)}");

        basicWinScreen?.SetActive(false);
        basicLoseScreen?.SetActive(false);
        bossCatchScreen?.SetActive(false);

        fishCaughtScreenActive = false;

        if (InitiateMicrogames.Instance != null)
        {
            // Enable cast screen *after* win screen disappears
            InitiateMicrogames.Instance.MGCanvas.SetActive(false);
            InitiateMicrogames.Instance.CCanvas.SetActive(true);
            InitiateMicrogames.Instance.CTC?.SetActive(true);
            InitiateMicrogames.Instance.fishingStarted = true;
            InitiateMicrogames.Instance.StartCastLockout();
        }

        if (currentCaughtFish != null && IsBossFish(currentCaughtFish.fishName))
        {
            BossfishLocation.SetActive(false);

            HAPlayerController player = FindObjectOfType<HAPlayerController>();
            if (player != null)
            {
                player.EndFishing();
                player.caughtPondBoss = true;
                player.inFishZone = false;
                player.canFish = false;

                if (player.fishingPromptUI != null)
                    player.fishingPromptUI.SetActive(false);
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

        StartCoroutine(DelayedClear());
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
        if (progress >= 100f && !hasCaughtFish && !progressMaxHandled)
        {
            progressMaxHandled = true;
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
        hasCaughtFish = true;

        if (activeZoneType == FishZoneType.Tutorial)
        {
            hasCaughtFish = true;
            HAPlayerController player = FindObjectOfType<HAPlayerController>();
            player.inFishZone = false;
            player.canFish = false;
            player.EndFishing();
            

            Debug.Log("Tutorial fish caught! Triggering tutorial cutscene...");

            CutsceneManager cutsceneManager = FindObjectOfType<CutsceneManager>();
            if (cutsceneManager != null)
            {
                GameObject musicObject = GameObject.FindWithTag("Music");
                if (musicObject != null)
                {
                    AudioSource musicSource = musicObject.GetComponent<AudioSource>();
                    if (musicSource != null && musicSource.isPlaying)
                    {
                        musicSource.Stop();
                    }
                }
                cutsceneManager.PlayCutscene();
                cutsceneManager.videoPlayer.loopPointReached += OnTutorialCutsceneFinished;
            }
            else
            {
                Debug.LogWarning("CutsceneManager not found for tutorial region!");
            }
            TutorialFishLocation.SetActive(false);

            
            T2S.microgamesActive = false;
            T2S.ClearAll();
            
        }
        else
        {
            Debug.Log("Player successfully caught the fish!");

            hasCaughtFish = true;

            PickRandomFish();
            Debug.Log($"[FishingProgress] Picked random fish: {currentCaughtFish?.fishName}");

            ShowFishCaughtScreen();

            T2S.microgamesActive = false;
            T2S.ClearAll();
        }
    }

    private void OnTutorialCutsceneFinished(VideoPlayer vp)
    {
        Debug.Log("Cutscene ended. Loading MainRegion scene...");
        
        // Unsubscribe so it doesn't fire again
        vp.loopPointReached -= OnTutorialCutsceneFinished;

        // Load the new scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("PostTutorial"); // replace with actual scene name
    }
    
    void OnProgressMin()
    {
        InitiateMicrogames.Instance.MGCanvas.SetActive(false);
        InitiateMicrogames.Instance.CCanvas.SetActive(false);
        basicLoseScreen.SetActive(true);
        fishCaughtScreenActive = true;
        fishCaughtTimer = 0f;

        Debug.Log("Player failed to catch the fish...");
        T2S.microgamesActive = false;
        T2S.ClearAll();

        HAPlayerController player = FindObjectOfType<HAPlayerController>();
        if (player != null)
        {
            player.PlayFishingIdle();
        }
        InitiateMicrogames.Instance.fishingStarted = false; // Prevent CTC from returning on fail
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
        Debug.Log($"[FishingProgress] Preparing to show caught screen...");

        if (currentCaughtFish == null)
        {
            Debug.LogWarning("[FishingProgress] currentCaughtFish is null, aborting screen display.");
            return;
        }

        if (InitiateMicrogames.Instance != null)
        {
            InitiateMicrogames.Instance.MGCanvas.SetActive(false);
            InitiateMicrogames.Instance.CCanvas.SetActive(false);

            // Hide Click to Cast prompt
            InitiateMicrogames.Instance.CTC.SetActive(false);
        }

        Debug.Log($"[FishingProgress] Caught fish: {currentCaughtFish.fishName}, Image: {(fishCaughtImage != null ? "OK" : "Missing")}, Text: {(fishCaughtText != null ? "OK" : "Missing")}");

        if (IsBossFish(currentCaughtFish.fishName))
            bossCatchScreen?.SetActive(true);
        else
            basicWinScreen?.SetActive(true);

        if (fishCaughtImage != null) fishCaughtImage.sprite = currentCaughtFish.fishSprite;
        if (fishCaughtText != null) fishCaughtText.text = $"Caught {currentCaughtFish.fishName}!";

        string time = System.DateTime.Now.ToString("HH:mm:ss");
        string date = System.DateTime.Now.ToString("MM/dd/yyyy");

        fishingLog.Add(new FishingLogEntry(
            currentCaughtFish.fishName,
            currentCaughtFish.fishSprite,
            date,
            time,
            activeZoneType
        ));

        GameManager.Instance?.RecordFishCatch(currentCaughtFish.fishName, activeZoneType.ToString());

        var encyUI = FindObjectOfType<FishEncyclopediaUI>();
        encyUI?.NotifyFishDiscovered(currentCaughtFish.fishName);

        Debug.Log($"[FishingProgress] Showing win screen for {currentCaughtFish.fishName} at {time} on {date}");

        HAPlayerController player = FindObjectOfType<HAPlayerController>();
        if (player != null)
            player.PlayFishingIdle();

        StartCoroutine(EnableFishCaughtInputNextFrame());
        InitiateMicrogames.Instance.CTC?.SetActive(false); // Force hide Click to Cast

        CheckUnlockBossFishing();
        InitiateMicrogames.Instance.fishingStarted = false; // Prevent CTC from returning during win screen
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

        var lastEntry = fishingLog[fishingLog.Count - 1];
        var fishName = lastEntry.fishName;
        var fishZone = lastEntry.zoneCaught;

        bool isBoss = IsBossFish(fishName);
        bool bossUnlocked = false;

        // Check and update save data
        switch (fishZone)
        {
            case FishZoneType.Pond:
                if (isBoss)
                {
                    GameManager.Instance.currentSaveData.hasCaughtPondBoss = true;
                }
                else if (!GameManager.Instance.currentSaveData.canFishPondBoss &&
                         GetNormalFishCaughtCount(fishZone) >= 3)
                {
                    GameManager.Instance.currentSaveData.canFishPondBoss = true;
                    bossUnlocked = true;
                }
                break;

            case FishZoneType.River:
                if (isBoss)
                {
                    GameManager.Instance.currentSaveData.hasCaughtRiverBoss = true;
                }
                else if (!GameManager.Instance.currentSaveData.canFishRiverBoss &&
                         GetNormalFishCaughtCount(fishZone) >= 3)
                {
                    GameManager.Instance.currentSaveData.canFishRiverBoss = true;
                    bossUnlocked = true;
                }
                break;

            case FishZoneType.Ocean:
                if (isBoss)
                {
                    GameManager.Instance.currentSaveData.hasCaughtOceanBoss = true;
                }
                else if (!GameManager.Instance.currentSaveData.canFishOceanBoss &&
                         GetNormalFishCaughtCount(fishZone) >= 3)
                {
                    GameManager.Instance.currentSaveData.canFishOceanBoss = true;
                    bossUnlocked = true;
                }
                break;
        }

        if (bossUnlocked)
        {
            string message = fishZone switch
            {
                FishZoneType.Pond => "You feel a sudden stir in the water. A new fishing spot appeared.",
                FishZoneType.River => "You sense a powerful current... something new awaits in the river.",
                FishZoneType.Ocean => "A massive shape moves in the deep... a new fishing spot appears.",
                _ => "Boss fishing unlocked!"
            };

            ShowNotification(message);

            // NEW: Trigger boss fish AI to swim to the catch location
            FishBossAI bossAI = FindObjectOfType<FishBossAI>();
            if (bossAI != null)
                bossAI.GoToBossLocation();
        }


        // Save after any potential changes
        SaveManager.Save(GameManager.Instance.currentSaveData);
    }

    public void ShowNotification(string message)
    {
        Debug.Log($"Trying to show notification: {message}");

        if (notificationCanvas != null && notificationText != null)
        {
            Debug.Log("Notification references are good! Showing message.");
            
            // Check if this is a boss unlock notification
            if (message.Contains("new fishing spot") || message.Contains("stir in the water"))
            {
                isShowingBossUnlockNotification = true;
                Debug.Log("Boss unlock notification detected - special handling active");
            }
            
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

    // Modify the NotificationRoutine method to handle the fish caught screen when a notification is shown
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
        
        // This is the critical part - AFTER the notification is done
        if (isShowingBossUnlockNotification)
        {
            Debug.Log("[NotificationRoutine] Boss notification complete, forcing screen cleanup");
            
            isShowingBossUnlockNotification = false;
            
            // Force hide the fish caught screen
            basicWinScreen?.SetActive(false);
            basicLoseScreen?.SetActive(false); 
            bossCatchScreen?.SetActive(false);
            
            fishCaughtScreenActive = false;
            
            // Re-enable the casting UI
            if (InitiateMicrogames.Instance != null)
            {
                InitiateMicrogames.Instance.MGCanvas.SetActive(false);
                InitiateMicrogames.Instance.CCanvas.SetActive(true);
                InitiateMicrogames.Instance.CTC?.SetActive(true);
                InitiateMicrogames.Instance.fishingStarted = true;
            }
            
            // Add this to ensure proper clean up
            StartCoroutine(DelayedClear());
        }
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

    public void Clear()
    {
        progress = 0f;
        progressSlider.value = 0f;

        memorySetupActive = false;
        hasCaughtFish = false;
        fishCaughtScreenActive = false;

        Debug.Log("[FishingProgress] Reset complete.");
    }


    private IEnumerator DelayedClear()
    {
        yield return null; // Wait 1 frame
        Clear();
    }
    private IEnumerator EnableFishCaughtInputNextFrame()
    {
        yield return null;
        fishCaughtScreenActive = true;
        fishCaughtTimer = 0f;
    }
}
