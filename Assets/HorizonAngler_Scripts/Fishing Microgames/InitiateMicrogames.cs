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


    // Inputs
    [HideInInspector] public string LMB     = "LMB";
    [HideInInspector] public string AButton = "A";
    [HideInInspector] public string Space   = "Space";

    // Input Variables
    [HideInInspector] public bool inputA     = false; // is key Pressed
    [HideInInspector] public bool inputLMB   = false; // is key Pressed
    [HideInInspector] public bool inputSpace = false; // is key Pressed

    private bool casted;
    private HAPlayerController playerController;
    private float castLockoutTimer = 0f;
    public enum FishZoneType
    {
        Pond,
        River,
        Ocean,
        BossPond,
        BossRiver,
        BossOcean
    }
    public static InitiateMicrogames Instance { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        T2S = MicrogameManager.GetComponent<Test2Script>();
        FProgress = MicrogameManager.GetComponent<FishingProgress>();
        CTC.SetActive(false);
        WFB.SetActive(false);
        FTB.SetActive(false);
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
            if (!T2S.microgamesActive)
            {
                CCanvas.SetActive(true);

                if (!casted && !FProgress.fishCaughtScreenActive && castLockoutTimer <= 0f)
                {
                    MGCanvas.SetActive(false);
                    ProcessInputs();

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
        else
        {
            CCanvas.SetActive(false); // Hide casting canvas if not allowed
        }
    }

    public void StartCastLockout()
    {
        castLockoutTimer = 0.25f; // 0.25 seconds lockout
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
        }
    }

    IEnumerator Bait()
    {
        CTC.SetActive(false);

        // Play Casting Animation after clicking
        if (playerController != null)
            playerController.PlayCasting();

        WFB.SetActive(true);
        yield return new WaitForSeconds(5f);
        StartCoroutine(TookBait());
    }

    IEnumerator TookBait()
    {
        WFB.SetActive(false);

        // Play Bait Took Animation after waiting
        if (playerController != null)
            playerController.PlayBaitTook();

        FTB.SetActive(true);
        yield return new WaitForSeconds(1f);

        FTB.SetActive(false);
        
        if (playerController != null);
            playerController.PlayFighting();

        CCanvas.SetActive(false);
        MGCanvas.SetActive(true);

        FProgress.Initialize();
        T2S.Initialize();
        yield return new WaitForSeconds(4);

        casted = false;
    }

}