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


    void ProcessInputs()
    {
        inputA     = Input.GetButtonDown(AButton);
        inputSpace = Input.GetButtonDown(Space);
        inputLMB   = Input.GetButtonDown(LMB);
    }

    void Cast()
    {
        // Display "Click to cast!"
        CTC.SetActive(true);
        if (inputA || inputSpace || inputLMB)
        {
            casted = true;
            StartCoroutine(Bait());
        }
    }

    IEnumerator Bait()
    {
        // Display "Waiting for a bite..."
        CTC.SetActive(false);
        WFB.SetActive(true);
        yield return new WaitForSeconds(rnd.Next(1, 3)); // Tweak this for how long you want the "Waiting for bite..." phase to last.
        StartCoroutine(TookBait());
    }

    public bool IsCasted() { return casted; }

    IEnumerator TookBait()
    {
        // Display "A fish took the bait!"
        WFB.SetActive(false);
        FTB.SetActive(true);
        yield return new WaitForSeconds(1);
        CCanvas.SetActive(false);
        FTB.SetActive(false);
        MGCanvas.SetActive(true);
        FProgress.Initialize();
        T2S.Initialize();
        yield return new WaitForSeconds(4);
        casted = false;
    }
    public void StartCastLockout()
    {
        castLockoutTimer = 0.25f; // 0.25 seconds lockout
    }
}