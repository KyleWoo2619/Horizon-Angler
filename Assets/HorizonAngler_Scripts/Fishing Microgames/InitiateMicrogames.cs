using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using UnityEngine.SceneManagement;
using StarterAssets;

public class InitiateMicrogames : MonoBehaviour
{
    static Random rnd = new Random();
    public GameObject MicrogameManager;
    [HideInInspector] public Test2Script T2S;
    [HideInInspector] public FishingProgress FProgress;

    public GameObject MGCanvas, CCanvas;
    public GameObject CTC, WFB, FTB;
    public GameObject TookBaitAnim;


    // Inputs
    [HideInInspector] public string LMB     = "LMB";
    [HideInInspector] public string AButton = "A";
    [HideInInspector] public string Space   = "Space";

    // Input Variables
    [HideInInspector] public bool inputA     = false; // is key Pressed
    [HideInInspector] public bool inputLMB   = false; // is key Pressed
    [HideInInspector] public bool inputSpace = false; // is key Pressed

    private bool casted;
    public bool fishingStarted = false;
    private HAPlayerController playerController;
    private float castLockoutTimer = 0f;
    public enum FishZoneType
    {
        Tutorial,
        Pond,
        River,
        Ocean,
        BlackPond,
        BossPond,
        BossRiver,
        BossOcean,
        PostPond
    }
    public static InitiateMicrogames Instance { get; private set; }
    [Header("Inspector Microgame Set Overrides")]
    public bool useInspectorOverrides = false;
    [Header("Microgame Set Checkboxes")]
    public bool enableSet1 = true;
    public bool enableSet2 = true;
    public bool enableSet3 = true;
    public bool enableSet4 = true;
    public bool enableSet5 = true;
    public bool enableSet6 = true;
    public bool enableSet7 = true;



    public List<string> ActiveMicrogameSets { get; private set; } = new List<string>();


    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        T2S = MicrogameManager.GetComponent<Test2Script>();
        FProgress = MicrogameManager.GetComponent<FishingProgress>();
        CTC.SetActive(false);
        WFB.SetActive(false);
        FTB.SetActive(false);
        MGCanvas.SetActive(false); 
        playerController = FindObjectOfType<HAPlayerController>();
    }



    // Update is called once per frame
    void Update()
    {
        if (castLockoutTimer > 0f)
            castLockoutTimer -= Time.deltaTime;

        bool isSampleScene = SceneManager.GetActiveScene().name == "SampleScene";

        // Check if player is allowed to fish (in a Fish Zone OR testing in SampleScene)
        if ((isSampleScene || (playerController != null && playerController.inFishZone)))
        {
            if (!T2S.microgamesActive && !FProgress.fishCaughtScreenActive && fishingStarted)
            {
                // Debug.Log("Microgame inactive. Checking cast conditions...");
                CCanvas.SetActive(true);



                if (!casted && !FProgress.fishCaughtScreenActive && castLockoutTimer <= 0f)
                {
                    // Debug.Log("Waiting for cast input...");
                    MGCanvas.SetActive(false);
                    ProcessInputs();

                    if (fishingStarted)
                    {
                        // Set Fish Pool based on Zone Type
                        if (playerController != null)
                        {
                            FProgress.SetActiveFishPool(playerController.currentZoneType);
                            FProgress.activeZoneType = playerController.currentZoneType;
                        }
                        else
                        {
                            FProgress.SetActiveFishPool(InitiateMicrogames.FishZoneType.Pond); // Default to Pond for SampleScene
                        }

                        Cast();
                    }
                }

            }
        }
        else
        {
            CCanvas.SetActive(false); // Hide casting canvas if not allowed
        }
    }

    public void StartCastLockout()
    {
        castLockoutTimer = 0.25f; // 0.25 seconds lockout
    }

    public void SetActiveMicrogameSets(FishZoneType zoneType)
    {
        ActiveMicrogameSets.Clear();

        if (useInspectorOverrides)
        {
            if (enableSet1) ActiveMicrogameSets.Add("Set1");
            if (enableSet2) ActiveMicrogameSets.Add("Set2");
            if (enableSet3) ActiveMicrogameSets.Add("Set3");
            if (enableSet4) ActiveMicrogameSets.Add("Set4");
            if (enableSet5) ActiveMicrogameSets.Add("Set5");
            if (enableSet6) ActiveMicrogameSets.Add("Set6");
            if (enableSet7) ActiveMicrogameSets.Add("Set7");

            Debug.Log("[Inspector Override] Active Microgame Sets: " + string.Join(", ", ActiveMicrogameSets));
            return;
        }


        // Default behavior (based on zone)
        ActiveMicrogameSets.Add("Set3");
        ActiveMicrogameSets.Add("Set4");
        ActiveMicrogameSets.Add("Set5");
        ActiveMicrogameSets.Add("Set7");

        switch (zoneType)
        {
            case FishZoneType.Pond:
            case FishZoneType.BossPond:
                ActiveMicrogameSets.Add("Set6");
                break;
            case FishZoneType.River:
            case FishZoneType.BossRiver:
                ActiveMicrogameSets.Add("Set1");
                break;
            case FishZoneType.Ocean:
            case FishZoneType.BossOcean:
                ActiveMicrogameSets.Add("Set2");
                break;
        }

        Debug.Log($"[FishingZone: {zoneType}] Active Microgame Sets: {string.Join(", ", ActiveMicrogameSets)}");
    }



    void ProcessInputs()
    {
        inputA     = Input.GetButtonDown(AButton);
        inputSpace = Input.GetButtonDown(Space);
        inputLMB   = Input.GetButtonDown(LMB);
    }

    void Cast()
    {
        // Play FishingIdle at the start
        // if (playerController != null)
            // playerController.PlayFishingIdle();

        CTC.SetActive(true);  // Optional: You can delete if you don't want text anymore

        if (inputA || inputSpace || inputLMB)
        {
            casted = true;
            StartCoroutine(Bait());
            if (FishingAudioManager.Instance != null)
            {
                StartCoroutine(DelayedCastSound());
            }
        }
    }

    IEnumerator Bait()
    {
        CTC.SetActive(false);

        // Play Casting Animation after clicking
        if (playerController != null)
            playerController.PlayCasting();

        WFB.SetActive(true);
        yield return new WaitForSeconds(3f);
        StartCoroutine(TookBait());
    }

    IEnumerator TookBait()
    {
        WFB.SetActive(false);

        // Play Bait Took Animation after waiting
        if (playerController != null)
            playerController.PlayBaitTook();

        FTB.SetActive(true);
        TookBaitAnim.SetActive(true);
        yield return new WaitForSeconds(1f);
        
        TookBaitAnim.SetActive(false);
        FTB.SetActive(false);
        
        if (playerController != null);
            playerController.PlayFighting();
            
        if (FishingAudioManager.Instance != null)
        {
            FishingAudioManager.Instance.StartFishingSound();
        }

        CCanvas.SetActive(false);
        MGCanvas.SetActive(true);

        FProgress.Initialize();
        T2S.Initialize();
        yield return new WaitForSeconds(4);

        casted = false;
    }

    public void NotifyFishingStarted()
    {
        fishingStarted = true;
        playerController.isFishing = true;
        Debug.Log("[Fishing] Started, casting UI will be shown.");
        CCanvas.SetActive(true); // << Show click-to-cast UI now that fishing has begun
    }

    public void ResetMinigame()
    {
        casted = false;
        playerController.isFishing = false;

        // Reset input states
        inputA = false;
        inputLMB = false;
        inputSpace = false;

        // Reset UI
        CTC.SetActive(false); // Click to Cast text
        WFB.SetActive(false); // Waiting for Bite
        FTB.SetActive(false); // Fish Took Bait
        MGCanvas.SetActive(false); // Minigame UI
        TookBaitAnim.SetActive(false);

        CCanvas.SetActive(false);
    }
    
    private IEnumerator DelayedCastSound()
    {
        yield return new WaitForSeconds(0.9f);
        FishingAudioManager.Instance.PlayCastSound();
    }
    public void FullFishingReset()
    {
        Debug.Log("[Fishing Reset] Fully resetting fishing system.");
        casted = false;
        fishingStarted = false;

        inputA = inputLMB = inputSpace = false;

        CTC.SetActive(false);
        WFB.SetActive(false);
        FTB.SetActive(false);
        MGCanvas.SetActive(false);
        TookBaitAnim.SetActive(false);
        CCanvas.SetActive(false);

        if (T2S != null)
        {
            T2S.microgamesActive = false;
            T2S.ClearAll();
        }

        if (FProgress != null)
        {
            FProgress.Clear();
        }
    }
}