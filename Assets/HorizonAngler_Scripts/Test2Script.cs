using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class Test2Script : MonoBehaviour
{
    static Random rnd = new Random();

    // Inputs
    [HideInInspector] public string RS_h    = "RS_h";   // Also handles horizontal mouse movement
    [HideInInspector] public string RS_v    = "RS_v";   // Also handles vertical mouse movement
    [HideInInspector] public string LS_h    = "LS_h";   // Also handles A and D
    [HideInInspector] public string LS_v    = "LS_v";   // Also handles W and S
    [HideInInspector] public string LS_b    = "LS_b";   // Also handles Space
    [HideInInspector] public string RS_b    = "RS_b";   // Also handles Space
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

    [Header("Sets Parents")]
    public GameObject Set1Parent;
    public GameObject Set2Parent;
    public GameObject Set3Parent;
    public GameObject Set4Parent;
    public GameObject Set5Parent;
    public GameObject Set6Parent;
    public GameObject Set7Parent;

    [Header("Timers")]
    public GameObject Set1Timer;
    public GameObject Set2Timer;
    public GameObject Set3Timer;
    public GameObject Set4Timer;
    public GameObject Set5Timer;
    public GameObject Set6Timer;
    public GameObject Set7Timer;
    [Space(5)]
    public TextMeshProUGUI Set1TimerText;
    public TextMeshProUGUI Set2TimerText;
    public TextMeshProUGUI Set3TimerText;
    public TextMeshProUGUI Set4TimerText;
    public TextMeshProUGUI Set5TimerText;
    public TextMeshProUGUI Set6TimerText;
    public TextMeshProUGUI Set7TimerText;
    List<GameObject> Timers = new List<GameObject>();
    List<TextMeshProUGUI> TimerTexts = new List<TextMeshProUGUI>();

    [Header("Set 1 Variables")]
    public RectTransform heartTx;
    public Rigidbody2D set1Controller;
    //public Set1Obstacle Set1Obstacle;
    public Transform obstaclesParent;
    List<Transform> initialObstacles = new List<Transform>();
    [SerializeField] List<Transform> obstacles = new List<Transform>();
    List<Vector2> obstaclesPos = new List<Vector2>();

    // Set 3 Combos
    public enum Combo1
    {
        STEP1,
        STEP2,
        STEP3
    }

    public enum Combo2
    {
        STEP1,
        STEP2,
        STEP3,
        STEP4
    }

    public enum Combo3
    {
        STEP1,
        STEP2,
        STEP3,
        STEP4,
        STEP5
    }

    [Header("Set 3 Variables")]
    
    public GameObject Combo1Parent;
    public GameObject Combo2Parent;
    public GameObject Combo3Parent;
    [HideInInspector] public bool qDPadUp;
    [HideInInspector] public bool qDPadDown;
    [HideInInspector] public bool qDPadRight;
    [HideInInspector] public bool qDPadLeft;
    [HideInInspector] public Combo1 combo1;
    [HideInInspector] public Combo2 combo2;
    [HideInInspector] public Combo3 combo3;
    [HideInInspector] public int comboNum;
    List<Enum> combos = new List<Enum>();
    List<Image> combo1Buttons = new List<Image>();
    List<Image> combo2Buttons = new List<Image>();
    List<Image> combo3Buttons = new List<Image>();
    private bool Set3CompletedRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        AddToSetsDict();
        MicrogamesSetup();
    }

    public void ClearAll()
    {
        StopAllCoroutines();
        obstacles.Clear();
        inactiveSets.Clear();
        activeSets.Clear();
        foreach (var t in Timers)
        {
            t.SetActive(false);
        }
        Sets["Set1"] = false;
        //Sets["Set2"] = false;
        Sets["Set3"] = false;
        //Sets["Set4"] = false;
        //Sets["Set5"] = false;
        //Sets["Set6"] = false;
        Sets["Set7"] = false;
        Set1Parent.SetActive(false);
        Set2Parent.SetActive(false);
        Set3Parent.SetActive(false);
        Set4Parent.SetActive(false);
        Set5Parent.SetActive(false);
        Set6Parent.SetActive(false);
        Set7Parent.SetActive(false);
    }

    public void Initialize()
    {
        InitialInactiveSets();
        StartCoroutine(tick("Initial", 3));
    }
    void AddToSetsDict()
    {
        Sets.Add("Set1", false);  // WASD / L-Joystick Set
        //Sets.Add("Set2", false);  // Mouse / R-Joystick Set // Mathf.Clamp
        Sets.Add("Set3", false);  // IJKL / D-Pad Set
        //Sets.Add("Set4", false);  // Arrow Keys / ABXY Set
        //Sets.Add("Set5", false);  // QE / Bumpers Set
        //Sets.Add("Set6", false);  // LMB/RMB / LT/RT Set
        Sets.Add("Set7", false);  // Space // Joystick Buttons Set
    }

    void MicrogamesSetup()
    {
        InitialSetup();
        InitialSetupSet1();
    }

    void InitialSetup()
    {
        Set1Parent.SetActive(false);
        Set2Parent.SetActive(false);
        Set3Parent.SetActive(false);
        Set4Parent.SetActive(false);
        Set5Parent.SetActive(false);
        Set6Parent.SetActive(false);
        Set7Parent.SetActive(false);

        Timers.Add(Set1Timer);
        Timers.Add(Set2Timer);
        Timers.Add(Set3Timer);
        Timers.Add(Set4Timer);
        Timers.Add(Set5Timer);
        Timers.Add(Set6Timer);
        Timers.Add(Set7Timer);
        TimerTexts.Add(Set1TimerText);
        TimerTexts.Add(Set2TimerText);
        TimerTexts.Add(Set3TimerText);
        TimerTexts.Add(Set4TimerText);
        TimerTexts.Add(Set5TimerText);
        TimerTexts.Add(Set6TimerText);
        TimerTexts.Add(Set7TimerText);

        foreach (var t in Timers)
        {
            t.SetActive(false);
        }
    }

    void InitialSetupSet1()
    {
        foreach (Transform child in obstaclesParent)
        {
            initialObstacles.Add(child);
        }
        foreach (Transform child in initialObstacles)
        {
            obstaclesPos.Add(child.localPosition);
        }

        combos.Add(combo1);
        combos.Add(combo2);
        combos.Add(combo3);

        foreach (Transform child in Combo1Parent.transform)
        {
            combo1Buttons.Add(child.GetComponent<Image>());
            child.GetComponent<Image>().color = Color.white;
        }
        foreach (Transform child in Combo2Parent.transform)
        {
            combo2Buttons.Add(child.GetComponent<Image>());
            child.GetComponent<Image>().color = Color.white;
        }
        foreach (Transform child in Combo3Parent.transform)
        {
            combo3Buttons.Add(child.GetComponent<Image>());
            child.GetComponent<Image>().color = Color.white;
        }
    }

    void Set1Setup()
    {
        foreach (Transform child in obstaclesParent)
        {
            obstacles.Add(child);
        }

        foreach (Transform child in obstacles)
        {
            child.localPosition = new Vector2(obstaclesPos[obstacles.IndexOf(child)].x, obstaclesPos[obstacles.IndexOf(child)].y);
            child.gameObject.SetActive(false);
        }
    }

    void Set3Setup()
    {
        combo1 = Combo1.STEP1;
        combo2 = Combo2.STEP1;
        combo3 = Combo3.STEP1;

        qDPadUp = false;
        qDPadDown = false;
        qDPadRight = false;
        qDPadLeft = false;

        Combo1Parent.SetActive(false);
        Combo2Parent.SetActive(false);
        Combo3Parent.SetActive(false);

        for (int i = 0; i < combo1Buttons.Count; i++)
        {
            Color color = combo1Buttons[i].color;
            color = Color.white;
            combo1Buttons[i].color = color;
            color.a = 1f;
            combo1Buttons[i].color = color;
        }
        for (int i = 0; i < combo2Buttons.Count; i++)
        {
            Color color = combo2Buttons[i].color;
            color = Color.white;
            combo2Buttons[i].color = color;
            color.a = 1f;
            combo2Buttons[i].color = color;
        }
        for (int i = 0; i < combo3Buttons.Count; i++)
        {
            Color color = combo3Buttons[i].color;
            color = Color.white;
            combo3Buttons[i].color = color;
            color.a = 1f;
            combo3Buttons[i].color = color;
        }

        comboNum = rnd.Next(0, combos.Count);
        // Display the visual for the combo here
        switch (comboNum)
        {
            case 2:
                Combo3Parent.SetActive(true);
                break;
            case 1:
                Combo2Parent.SetActive(true);
                break;
            case 0:
                Combo1Parent.SetActive(true);
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (microgamesActive == true)
        {
            Set1Inputs();
        }
    }

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
        inputA      = Input.GetButtonDown(AButton);
        inputB      = Input.GetButtonDown(BButton);
        inputX      = Input.GetButtonDown(XButton);
        inputY      = Input.GetButtonDown(YButton);
        inputLB     = Input.GetButtonDown(LB);
        inputRB     = Input.GetButtonDown(RB);
        inputLT     = Input.GetButtonDown(LT);
        inputRT     = Input.GetButtonDown(RT);
        inputLS_b   = Input.GetButtonDown(LS_b);
        inputRS_b   = Input.GetButtonDown(RS_b);
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
    void Set1Inputs()
    {
        if (Sets["Set1"] == true)
        {
            Set1Parent.SetActive(true);
            MicrogameSet1();
        }
    }

    void MicrogamesInputs()
    {
        /*if (Sets["Set2"] == true)
        {
            Set2Parent.SetActive(true);
            MicrogameSet2();
        }*/
        if (Sets["Set3"] == true)
        {
            Set3Parent.SetActive(true);
            MicrogameSet3();
        }
        /*if (Sets["Set4"] == true)
        {
            Set4Parent.SetActive(true);
            MicrogameSet4();
        }
        if (Sets["Set5"] == true)
        {
            Set5Parent.SetActive(true);
            MicrogameSet5();
        }
        if (Sets["Set6"] == true)
        {
            Set6Parent.SetActive(true);
            MicrogameSet6();
        }*/
        if (Sets["Set7"] == true)
        {
            Set7Parent.SetActive(true);
            MicrogameSet7();
        }
    }

    void MicrogameSet1()
    {
        float LHX = inputLAxisX;
        float LHY = inputLAxisY;
        // Move Undertale heart based on LHX + LHY
        Vector2 calc;
        Vector2 move;
        float speed = 100f;
        move = (heartTx.right * LHX) + (heartTx.up * LHY);
        if (move.magnitude > 1f)
        {
            move = move.normalized;
        }
        calc = move * speed * Time.fixedDeltaTime;
        set1Controller.velocity = (calc * 100);
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
        float DPY = inputDAxisY;
        // Pick an input combination randomly

        if (DPX == 0)
        {
            leftChkDown = false;
            rightChkDown = false;
        }
        if (DPY == 0)
        {
            upChkDown = false;
            downChkDown = false;
        }
        DPadUp(DPY);
        DPadDown(DPY);
        DPadRight(DPX);
        DPadLeft(DPX);
        NotUp();
        NotDown();
        NotRight();
        NotLeft();

        switch (comboNum)
        {
            case 2:
                Debug.Log("Combo 3!");
                StartCoroutine(Set3Combo3(DPX, DPY));
                break;
            case 1:
                Debug.Log("Combo 2!");
                StartCoroutine(Set3Combo2(DPX, DPY));
                break;
            case 0:
                Debug.Log("Combo 1!");
                StartCoroutine(Set3Combo1(DPX, DPY));
                break;
        }
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
            Sets["Set7"] = false;
            activeSets.Remove("Set7");
            inactiveSets.Add("Set7");
            Set7Parent.SetActive(false);
        }
    }

    private bool upChkDown, downChkDown, rightChkDown, leftChkDown;
    private bool qNotUp, qNotDown, qNotLeft, qNotRight;

    void DPadUp(float DPY)
    {
        if (DPY == 1 && !upChkDown)
        {
            upChkDown = true;
            StartCoroutine(DPadUpCo());
            Debug.Log("DPad Up was pressed!");
        }
    }

    IEnumerator DPadUpCo()
    {
        qDPadUp = true;
        yield return new WaitForEndOfFrame();
        qDPadUp = false;
    }

    void NotUp()
    {
        if (qDPadDown || qDPadLeft || qDPadRight)
        {
            qNotUp = true;
        }
        else
        {
            qNotUp = false;
        }
    }

    void DPadDown(float DPY)
    {
        if (DPY == -1 && !downChkDown)
        {
            downChkDown = true;
            StartCoroutine(DPadDownCo());
            Debug.Log("DPad Down was pressed!");
        }
    }

    IEnumerator DPadDownCo()
    {
        qDPadDown = true;
        yield return new WaitForEndOfFrame();
        qDPadDown = false;
    }
    void NotDown()
    {
        if (qDPadUp || qDPadLeft || qDPadRight)
        {
            qNotDown = true;
        }
        else
        {
            qNotDown = false;
        }
    }

    void DPadRight(float DPX)
    {
        if (DPX == 1 && !rightChkDown)
        {
            rightChkDown = true;
            StartCoroutine(DPadRightCo());
            Debug.Log("DPad Right was pressed!");
        }
    }
    IEnumerator DPadRightCo()
    {
        qDPadRight = true;
        yield return new WaitForEndOfFrame();
        qDPadRight = false;
    }
    void NotRight()
    {
        if (qDPadDown || qDPadLeft || qDPadUp)
        {
            qNotRight = true;
        }
        else
        {
            qNotRight = false;
        }
    }

    void DPadLeft(float DPX)
    {
        if (DPX == -1 && !leftChkDown)
        {
            leftChkDown = true;
            StartCoroutine(DPadLeftCo());
            Debug.Log("DPad Left was pressed!");
        }
    }
    IEnumerator DPadLeftCo()
    {
        qDPadLeft = true;
        yield return new WaitForEndOfFrame();
        qDPadLeft = false;
    }
    void NotLeft()
    {
        if (qDPadDown || qDPadUp || qDPadRight)
        {
            qNotLeft = true;
        }
        else
        {
            qNotLeft = false;
        }
    }

    void Set3Mistake()
    {
        // Penalty to progress bar
        Debug.Log("Wrong DPad Input!");
        Set3Setup();
    }

    IEnumerator Set3Combo1(float DPX, float DPY) // --------------------------- Combo 1: UP DOWN LEFT
    {
        switch (combo1)
        {
            case (Combo1.STEP1):
                if (qDPadUp)
                {
                    // Highlight that the button has been pressed
                    combo1Buttons[0].color = new Color32(163, 233, 181, 255);
                    combo1 = Combo1.STEP2;
                }
                if (qNotUp)
                {
                    // Highlight the button in red
                    combo1Buttons[0].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo1.STEP2):
                yield return new WaitUntil(() => DPY == 0);
                if (qDPadDown)
                {
                    // Highlight that the button has been pressed
                    combo1Buttons[1].color = new Color32(163, 233, 181, 255);
                    combo1 = Combo1.STEP3;
                }
                if (qNotDown)
                {
                    // Highlight the button in red
                    combo1Buttons[1].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo1.STEP3):
                yield return new WaitUntil(() => DPY == 0);
                if (qDPadLeft)
                {
                    // Highlight that the button has been pressed
                    combo1Buttons[2].color = new Color32(163, 233, 181, 255);
                    StartCoroutine(Set3Completed());
                }
                if (qNotLeft)
                {
                    if (!Set3CompletedRunning)
                    {
                        // Highlight the button in red
                        combo1Buttons[2].color = new Color32(230, 35, 22, 255);
                        yield return new WaitForSeconds(0.5f);
                        Set3Mistake();
                    }
                }
                break;
        }
    }

    IEnumerator Set3Combo2(float DPX, float DPY) // --------------------------- Combo 2: RIGHT RIGHT LEFT UP
    {
        switch (combo2)
        {
            case (Combo2.STEP1):
                if (qDPadRight)
                {
                    // Highlight that the button has been pressed
                    combo2Buttons[0].color = new Color32(163, 233, 181, 255);
                    combo2 = Combo2.STEP2;
                }
                if (qNotRight)
                {
                    // Highlight the button in red
                    combo2Buttons[0].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo2.STEP2):
                yield return new WaitUntil(() => DPX == 0);
                if (qDPadRight)
                {
                    // Highlight that the button has been pressed
                    combo2Buttons[1].color = new Color32(163, 233, 181, 255);
                    combo2 = Combo2.STEP3;
                }
                if (qNotRight)
                {
                    // Highlight the button in red
                    combo2Buttons[1].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo2.STEP3):
                yield return new WaitUntil(() => DPX == 0);
                if (qDPadLeft)
                {
                    // Highlight that the button has been pressed
                    combo2Buttons[2].color = new Color32(163, 233, 181, 255);
                    combo2 = Combo2.STEP4;
                }
                if (qNotLeft)
                {
                    // Highlight the button in red
                    combo2Buttons[2].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo2.STEP4):
                yield return new WaitUntil(() => DPX == 0);
                if (qDPadUp)
                {
                    // Highlight that the button has been pressed
                    combo2Buttons[3].color = new Color32(163, 233, 181, 255);
                    StartCoroutine(Set3Completed());
                }
                if (qNotUp)
                {
                    if (!Set3CompletedRunning)
                    {
                        // Highlight the button in red
                        combo2Buttons[3].color = new Color32(230, 35, 22, 255);
                        yield return new WaitForSeconds(0.5f);
                        Set3Mistake();
                    }
                }
                break;
        }
    }

    IEnumerator Set3Combo3(float DPX, float DPY) // --------------------------- Combo 3: DOWN UP LEFT UP RIGHT
    {
        switch (combo3)
        {
            case (Combo3.STEP1):
                if (qDPadDown)
                {
                    // Highlight that the button has been pressed
                    combo3Buttons[0].color = new Color32(163, 233, 181, 255);
                    combo3 = Combo3.STEP2;
                }
                if (qNotDown)
                {
                    // Highlight the button in red
                    combo3Buttons[0].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo3.STEP2):
                yield return new WaitUntil(() => DPY == 0);
                if (qDPadUp)
                {
                    // Highlight that the button has been pressed
                    combo3Buttons[1].color = new Color32(163, 233, 181, 255);
                    combo3 = Combo3.STEP3;
                }
                if (qNotUp)
                {
                    // Highlight the button in red
                    combo3Buttons[1].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo3.STEP3):
                yield return new WaitUntil(() => DPY == 0);
                if (qDPadLeft)
                {
                    // Highlight that the button has been pressed
                    combo3Buttons[2].color = new Color32(163, 233, 181, 255);
                    combo3 = Combo3.STEP4;
                }
                if (qNotLeft)
                {
                    // Highlight the button in red
                    combo3Buttons[2].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo3.STEP4):
                yield return new WaitUntil(() => DPX == 0);
                if (qDPadUp)
                {
                    // Highlight that the button has been pressed
                    combo3Buttons[3].color = new Color32(163, 233, 181, 255);
                    combo3 = Combo3.STEP5;
                }
                if (qNotUp)
                {
                    // Highlight the button in red
                    combo3Buttons[3].color = new Color32(230, 35, 22, 255);
                    yield return new WaitForSeconds(0.5f);
                    Set3Mistake();
                }
                break;
            case (Combo3.STEP5):
                yield return new WaitUntil(() => DPY == 0);
                if (qDPadRight)
                {
                    // Highlight that the button has been pressed
                    combo3Buttons[4].color = new Color32(163, 233, 181, 255);
                    StartCoroutine(Set3Completed());
                }
                if (qNotRight)
                {
                    if (!Set3CompletedRunning)
                    {
                        // Highlight the button in red
                        combo3Buttons[4].color = new Color32(230, 35, 22, 255);
                        yield return new WaitForSeconds(0.5f);
                        Set3Mistake();
                    }
                }
                break;
        }
    }

    IEnumerator Set3Completed()
    {
        // Bonus to progress
        Set3CompletedRunning = true;
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Set 3 Completed!");
        Sets["Set3"] = false;
        activeSets.Remove("Set3");
        inactiveSets.Add("Set3");
        Set3Parent.SetActive(false);
        Set3CompletedRunning = false;
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
        Debug.Log("Inactive Sets Counter: " + inactiveSets.Count);
        if (inactiveSets.Count > 0)
        {
            int microgameRnd = rnd.Next(0, inactiveSets.Count);
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

    void InitiateTimer(string timerID, int time)
    {
        switch (timerID)
        {
            case "Set7":
                Timers[6].SetActive(true);
                TimerTexts[6].text = time.ToString();
                break;
            case "Set6":
                Timers[5].SetActive(true);
                TimerTexts[5].text = time.ToString();
                break;
            case "Set5":
                Timers[4].SetActive(true);
                TimerTexts[4].text = time.ToString();
                break;
            case "Set4":
                Timers[3].SetActive(false);
                TimerTexts[3].text = time.ToString();
                break;
            case "Set3":
                Timers[2].SetActive(true);
                TimerTexts[2].text = time.ToString();
                break;
            case "Set2":
                Timers[1].SetActive(true);
                TimerTexts[1].text = time.ToString();
                break;
            case "Set1":
                Timers[0].SetActive(true);
                TimerTexts[0].text = time.ToString();
                break;
            case "Initial":
                break;
        }
    }

    private IEnumerator tick(string timerID, int timeRemaining)
    // This IEnumerator is a timer counting down to 0 and will work with any integer length of seconds and multiple timers can be run at once
    {
        while (timeRemaining > 0)
        {
            InitiateTimer(timerID, timeRemaining);
            Debug.Log(timerID + ": " + timeRemaining);
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }
        if (timeRemaining == 0)
        {
            InitiateTimer(timerID, timeRemaining);
            Debug.Log(timerID + ": " + timeRemaining);
            switch (timerID)
            {
                case "Set7":
                    // Start Set 7
                    Debug.Log("Set 7 Microgame is now ACTIVE!");
                    Timers[6].SetActive(false);
                    Sets["Set7"] = true;
                    break;
                case "Set6":
                    // Start Set 6
                    Debug.Log("Set 6 Microgame is now ACTIVE!");
                    Timers[5].SetActive(false);
                    Sets["Set6"] = true;
                    break;
                case "Set5":
                    // Start Set 5
                    Debug.Log("Set 5 Microgame is now ACTIVE!");
                    Timers[4].SetActive(false);
                    Sets["Set5"] = true;
                    break;
                case "Set4":
                    // Start Set 4
                    Debug.Log("Set 4 Microgame is now ACTIVE!");
                    Timers[3].SetActive(false);
                    Sets["Set4"] = true;
                    break;
                case "Set3":
                    // Start Set 3
                    Debug.Log("Set 3 Microgame is now ACTIVE!");
                    Timers[2].SetActive(false);
                    Set3Setup();
                    Sets["Set3"] = true;
                    break;
                case "Set2":
                    // Start Set 2
                    Debug.Log("Set 2 Microgame is now ACTIVE!");
                    Timers[1].SetActive(false);
                    Sets["Set2"] = true;
                    break;
                case "Set1":
                    // Start Set 1
                    Debug.Log("Set 1 Microgame is now ACTIVE!");
                    Timers[0].SetActive(false);
                    Sets["Set1"] = true;
                    Set1Setup();
                    StartCoroutine(Set1Obstacles());
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

    IEnumerator Set1Obstacles()
    {
        float timeRemaining = rnd.Next(1, 3);
        yield return new WaitForSeconds(timeRemaining);
        int nextObstacle = rnd.Next(0, obstacles.Count);
        obstacles[nextObstacle].gameObject.SetActive(true);
        obstacles[nextObstacle].GetComponent<Set1Obstacle>().MoveToHeart();
        obstacles.RemoveAt(nextObstacle);
        if (obstacles.Count > 0)
        {
            StartCoroutine(Set1Obstacles());
        }
        else
        {
            yield return new WaitForSeconds(2);
            Debug.Log("Set 1 Completed!");
            Sets["Set1"] = false;
            activeSets.Remove("Set1");
            inactiveSets.Add("Set1");
            Set1Parent.SetActive(false);
        }
    }
}
