using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class TestScript : MonoBehaviour
{
    static Random rnd = new Random();

    // Inputs
    [HideInInspector] public string RS_h    = "RS_h";   // Also handles horizontal mouse movement
    [HideInInspector] public string RS_v    = "RS_v";   // Also handles vertical mouse movement
    [HideInInspector] public string LS_h    =  "LS_h";  // Also handles A and D
    [HideInInspector] public string LS_v    = "LS_v";   // Also handles W and S
    [HideInInspector] public string LS_b    = "LS_b";   // Also handles Space
    [HideInInspector] public string RS_b    = "RS_b";   // Also handles Space // Space being handled by LS_b & RS_b is because on KB/M pressing Space = pressing LS_b + RS_b
    [HideInInspector] public string DPad_h  = "DPad_h"; // Also handles J and L
    [HideInInspector] public string DPad_v  = "DPad_v"; // Also handles I and K
    [HideInInspector] public string AButton = "A";      // Also handles Down Arrow
    [HideInInspector] public string BButton = "B";      // Also handles Right Arrow
    [HideInInspector] public string XButton = "X";      // Also handles Left Arrow
    [HideInInspector] public string YButton = "Y";      // Also handles Up Arrow
    [HideInInspector] public string LT      = "LT";     // Also handles LMB
    [HideInInspector] public string RT      = "RT";     // Also handles RMB
    [HideInInspector] public string LB      = "LB";     // Also handles Q
    [HideInInspector] public string RB      = "RB";     // Also handles E
    [HideInInspector] public string Escape  = "Escape"; // Also handles Start Button (controller)


    // Input Variables
    [HideInInspector] public float inputRAxisX = 0f;    // range -1f to +1f // Use GetAxis
    [HideInInspector] public float inputRAxisY = 0f;    // range -1f to +1f // Use GetAxis
    [HideInInspector] public float inputLAxisX = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public float inputLAxisY = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public float inputDAxisX = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public float inputDAxisY = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public bool inputA       = false; // is key Pressed
    [HideInInspector] public bool inputB       = false; // is key Pressed
    [HideInInspector] public bool inputX       = false; // is key Pressed
    [HideInInspector] public bool inputY       = false; // is key Pressed
    [HideInInspector] public bool inputLB      = false; // is key Pressed
    [HideInInspector] public bool inputRB      = false; // is key Pressed
    [HideInInspector] public bool inputLT      = false; // is key Pressed
    [HideInInspector] public bool inputRT      = false; // is key Pressed // May have to tweak this + LT in Input Manager, is currently set to Key or Mouse Button, but may need to be Axis
    [HideInInspector] public bool inputLS_b    = false; // is key Pressed
    [HideInInspector] public bool inputRS_b    = false; // is key Pressed
    [HideInInspector] public bool inputEscape  = false; // is key Pressed

    // Microgame Variables
    [HideInInspector] public bool microgamesActive = false;
    Dictionary<string, bool> Sets = new Dictionary<string, bool>();
    List<string> inactiveSets = new List<string>();
    List<string> activeSets = new List<string>();

    // Start is called before the first frame update
    void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        AddToSetsDict();
        InitialInactiveSets();
        StartCoroutine(tick("Initial", 3));
    }
    void AddToSetsDict()
    {
        Sets.Add("Set1", false);  // WASD / L-Joystick Set
        //Sets.Add("Set2", false);  // Mouse / R-Joystick Set
        Sets.Add("Set3", false);  // IJKL / D-Pad Set
        //Sets.Add("Set4", false);  // Arrow Keys / ABXY Set
        //Sets.Add("Set5", false);  // QE / Bumpers Set
        //Sets.Add("Set6", false);  // LMB/RMB / LT/RT Set
        Sets.Add("Set7", false);  // Space // Joystick Buttons Set
    }

    // Update is called once per frame
    void Update()
    {
        if (microgamesActive == true)
        {
            ProcessInputs();
            MicrogamesInputs();
            MicrogameFailsafe();
        }
    }

    void ProcessInputs()
    {
        inputRAxisX = Input.GetAxis(RS_h);
        inputRAxisY = Input.GetAxis(RS_v);
        inputLAxisX = Input.GetAxisRaw(LS_h);
        inputLAxisY = Input.GetAxisRaw(LS_v);
        inputDAxisX = Input.GetAxisRaw(DPad_h);
        inputDAxisY = Input.GetAxisRaw(DPad_v);
        inputA = Input.GetButtonDown(AButton);
        inputB = Input.GetButtonDown(BButton);
        inputX = Input.GetButtonDown(XButton);
        inputY = Input.GetButtonDown(YButton);
        inputLB = Input.GetButtonDown(LB);
        inputRB = Input.GetButtonDown(RB);
        inputLT = Input.GetButtonDown(LT);
        inputRT = Input.GetButtonDown(RT);
        inputLS_b = Input.GetButtonDown(LS_b);
        inputRS_b = Input.GetButtonDown(RS_b);
        inputEscape = Input.GetButtonDown(Escape);
    }

    void MicrogameFailsafe() // This is to ensure that there is at least 1 microgame running at all times
    {
        if (activeSets.Count == 0)
        {
            Debug.Log("Microgame Failsafe Ran");
            MicrogameStarter();
        }
    }

    void MicrogamesInputs()
    {
        if (Sets["Set1"] == true)
        {
            MicrogameSet1();
        }
        /*if (Sets["Set2"] == true)
        {
            MicrogameSet2();
        }*/
        if (Sets["Set3"] == true)
        {
            MicrogameSet3();
        }
        /*if (Sets["Set4"] == true)
        {
            MicrogameSet4();
        }
        if (Sets["Set5"] == true)
        {
            MicrogameSet5();
        }
        if (Sets["Set6"] == true)
        {
            MicrogameSet6();
        }*/
        if (Sets["Set7"] == true)
        {
            MicrogameSet7();
        }
    }

    void MicrogameSet1()
    {
        float LHX = inputLAxisX;
        float LHY = inputLAxisY;
        // Move Undertale heart based on LHX + LHY
        /*Vector3 calc;
        Vector3 move;
        move = (heartTx.right * LHX) + (heartTx.up * LHY); //(or forward instead of up?)
        if (move.magnitude > 1f)
        {
            move = move.normalized;
        }
        calc = move * Time.deltaTime; // also multiply by a speed var if you want
        controller.Move(calc);*/ // Need to set up heartTx and controller in Initialize as their GameObjects

        /*
        activeSets.Remove("Set1");
        inactiveSets.Add("Set1");
        */
    }

    void MicrogameSet2()
    {


        /*
        activeSets.Remove("Set2");
        inactiveSets.Add("Set2");
        */
    }

    void MicrogameSet3()
    {
        float DPX = inputDAxisX;
        float DHY = inputDAxisY;
        // Pick an input combination randomly

        /*
        activeSets.Remove("Set3");
        inactiveSets.Add("Set3");
        */
    }

    void MicrogameSet4()
    {


        /*
        activeSets.Remove("Set4");
        inactiveSets.Add("Set4");
        */
    }

    void MicrogameSet5()
    {


        /*
        activeSets.Remove("Set5");
        inactiveSets.Add("Set5");
        */
    }

    void MicrogameSet6()
    {


        /*
        activeSets.Remove("Set6");
        inactiveSets.Add("Set6");
        */
    }

    void MicrogameSet7()
    {
        if (inputLS_b && inputRS_b)
        {
            Debug.Log("Set 7 Completed!");
            activeSets.Remove("Set7");
            inactiveSets.Add("Set7");
        }
    }

    void InitialInactiveSets()
    {
        foreach (KeyValuePair<string, bool> kvp in Sets)
        {
            if (kvp.Value == false)
            {
                inactiveSets.Add(kvp.Key);
            }
        }
    }

    void MicrogameStarter()
    {
        int temp = inactiveSets.Count;
        Debug.Log("Inactive Sets Counter: " + inactiveSets.Count);
        if (inactiveSets.Count > 0)
        {
            int microgameRnd = rnd.Next(0, temp);
            //Debug.Log(microgameRnd);
            //Debug.Log(inactiveSets[microgameRnd]);
            StartCoroutine(tick(inactiveSets[microgameRnd], 3));
            activeSets.Add(inactiveSets[microgameRnd]);
            inactiveSets.RemoveAt(microgameRnd);
        }
        else
        {
            Debug.Log("All available microgames are running!");
        }
    }

    private IEnumerator microgameCooldown()
    { 
        int mgCD = rnd.Next(1, 11);
        Debug.Log("Microgame Cooldown is running for " + mgCD + " seconds!");
        yield return new WaitForSeconds(mgCD);
        MicrogameStarter();
        StartCoroutine(microgameCooldown());
    }
    
    private IEnumerator tick(string timerID, int timeRemaining)
    // This IEnumerator is a timer counting down to 0 and will work with any integer length of seconds and multiple timers can be run at once
    {
        while (timeRemaining > 0)
        {
            Debug.Log(timerID + ": " + timeRemaining);
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }
        if (timeRemaining == 0)
        {
            Debug.Log(timerID + ": " + timeRemaining);
            switch (timerID)
            {
                case "Set7":
                    // Start Set 7
                    Debug.Log("Set 7 Microgame is now ACTIVE!");
                    Sets["Set7"] = true;
                    break;
                case "Set6":
                    // Start Set 6
                    Debug.Log("Set 6 Microgame is now ACTIVE!");
                    Sets["Set6"] = true;
                    break;
                case "Set5":
                    // Start Set 5
                    Debug.Log("Set 5 Microgame is now ACTIVE!");
                    Sets["Set5"] = true;
                    break;
                case "Set4":
                    // Start Set 4
                    Debug.Log("Set 4 Microgame is now ACTIVE!");
                    Sets["Set4"] = true;
                    break;
                case "Set3":
                    // Start Set 3
                    Debug.Log("Set 3 Microgame is now ACTIVE!");
                    Sets["Set3"] = true;
                    break;
                case "Set2":
                    // Start Set 2
                    Debug.Log("Set 2 Microgame is now ACTIVE!");
                    Sets["Set2"] = true;
                    break;
                case "Set1":
                    // Start Set 1
                    Debug.Log("Set 1 Microgame is now ACTIVE!");
                    Sets["Set1"] = true;
                    break;
                case "Initial":
                    // Start Microgames
                    Debug.Log("Microgames are now ACTIVE!");
                    microgamesActive = true;
                    MicrogameStarter();
                    StartCoroutine(microgameCooldown());
                    break;
            }
        }
    }
}
