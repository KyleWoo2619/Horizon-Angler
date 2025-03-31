using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class InitiateMicrogames : MonoBehaviour
{
    static Random rnd = new Random();
    public GameObject MGM;
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

    // Start is called before the first frame update
    void Awake()
    {
        T2S = MGM.GetComponent<Test2Script>();
        FProgress = MGM.GetComponent<FishingProgress>();
        CTC.SetActive(false);
        WFB.SetActive(false);
        FTB.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!T2S.microgamesActive)
        {
            CCanvas.SetActive(true);
        }
        if (!T2S.microgamesActive && !casted)
        {
            MGCanvas.SetActive(false);
            ProcessInputs();
            Cast();
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
        int baitTime = rnd.Next(1, 3);
        yield return new WaitForSeconds(baitTime);
        StartCoroutine(TookBait());
    }

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
}
