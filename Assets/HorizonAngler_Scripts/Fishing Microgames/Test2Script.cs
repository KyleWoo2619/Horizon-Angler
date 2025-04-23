using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Random = System.Random;

public class Test2Script : MonoBehaviour
{
    public enum FishingZoneType { Pond, River, Ocean, BossPond, BossRiver, BossOcean }

    static Random rnd = new Random();

    public Set6ObstacleWaveManager OWM;

    // Inputs
    [HideInInspector] public string RS_h = "RS_h";   // Also handles horizontal mouse movement
    [HideInInspector] public string RS_v = "RS_v";   // Also handles vertical mouse movement
    [HideInInspector] public string LS_h = "LS_h";   // Also handles A and D
    [HideInInspector] public string LS_v = "LS_v";   // Also handles W and S
    [HideInInspector] public string LS_b = "LS_b";   // Also handles Space
    [HideInInspector] public string RS_b = "RS_b";   // Also handles Space
    [HideInInspector] public string DPad_h = "DPad_h"; // Also handles J and L
    [HideInInspector] public string DPad_v = "DPad_v"; // Also handles I and K
    [HideInInspector] public string AButton = "A";      // Also handles Down Arrow
    [HideInInspector] public string BButton = "B";      // Also handles Right Arrow
    [HideInInspector] public string XButton = "X";      // Also handles Left Arrow
    [HideInInspector] public string YButton = "Y";      // Also handles Up Arrow
    [HideInInspector] public string LT = "LT";     // Also handles LMB
    [HideInInspector] public string RT = "RT";     // Also handles RMB
    [HideInInspector] public string LB = "LB";     // Also handles Q
    [HideInInspector] public string RB = "RB";     // Also handles E
    [HideInInspector] public string Escape = "Escape"; // Also handles Start Button (controller)

    // Input Variables
    [HideInInspector] public float inputRAxisX = 0f;    // range -1f to +1f // Use GetAxis
    [HideInInspector] public float inputRAxisY = 0f;    // range -1f to +1f // Use GetAxis
    [HideInInspector] public float inputLAxisX = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public float inputLAxisY = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public float inputDAxisX = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public float inputDAxisY = 0f;    // range -1f to +1f // Use GetAxisRaw
    [HideInInspector] public bool inputA = false; // is key Pressed
    [HideInInspector] public bool inputB = false; // is key Pressed
    [HideInInspector] public bool inputX = false; // is key Pressed
    [HideInInspector] public bool inputY = false; // is key Pressed
    [HideInInspector] public bool inputLB = false; // is key Pressed
    [HideInInspector] public bool inputRB = false; // is key Pressed
    [HideInInspector] public bool inputLT = false; // is key Pressed
    [HideInInspector] public bool inputRT = false; // is key Pressed // May have to tweak this + LT in Input Manager, is currently set to Key or Mouse Button, but may need to be Axis
    [HideInInspector] public bool inputLS_b = false; // is key Pressed
    [HideInInspector] public bool inputRS_b = false; // is key Pressed
    [HideInInspector] public bool inputEscape = false; // is key Pressed

    // Microgame Variables
    [HideInInspector] public bool microgamesActive = false;
    public Dictionary<string, bool> Sets = new Dictionary<string, bool>();
    public List<string> inactiveSets = new List<string>();
    public List<string> activeSets = new List<string>();

    [Header("Sets Parents")]
    public GameObject Set1Parent;
    public GameObject Set2Parent;
    public GameObject Set3Parent;
    public GameObject Set4Parent;
    public GameObject Set5Parent;
    public GameObject Set6Parent;
    public GameObject Set7Parent;

    [Header("Timers")]
    public GameObject InitialTimer;
    public GameObject Set1Timer;
    public GameObject Set2Timer;
    public GameObject Set3Timer;
    public GameObject Set4Timer;
    public GameObject Set5Timer;
    public GameObject Set6Timer;
    public GameObject Set7Timer;
    [Space(5)]
    public TextMeshProUGUI InitialTimerText;
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

    [Header("Set 3 Variables")]
    private bool Set3CompletedRunning = false;
    private List<string> dpadCombo = new List<string>();
    private int dpadStep = 0;
    private bool dpadInputActive = false;
    public GameObject dpadComboParent;  // Parent where icons will spawn
    public GameObject dpadComboArrowPrefab; // Prefab for one arrow
    public Sprite upArrowSprite;
    public Sprite downArrowSprite;
    public Sprite leftArrowSprite;
    public Sprite rightArrowSprite;
    private List<Image> dpadArrowImages = new List<Image>();
    [HideInInspector] public bool dpadAxisInUseX = false;
    [HideInInspector] public bool dpadAxisInUseY = false;


    [Header("Set 4 Variables")]
    public GameObject MemoryParent;
    public List<Button> memoryButtons = new List<Button>(); // A, B, X, Y buttons
    public TextMeshProUGUI memoryText;
    private List<string> memoryCombo = new List<string>();
    private List<string> playerInputCombo = new List<string>();
    private bool memoryInputActive = false;
    private int memoryStep = 0;
    private bool memorySetupDone = false;
    private Coroutine memoryShowCoroutine;
    Dictionary<Button, Coroutine> greyCoroutines = new Dictionary<Button, Coroutine>();

    [Header("Set 5 Variables")]
    public GameObject mashUIParent; // UI Panel that says "MASH Q/E!"
    private int mashCounter = 0;
    private int targetMashCount = 15;
    private bool mashActive = false;
    private string lastInput = "";
    private RectTransform mashUIRect; // For vibration effect
    public Slider mashSlider;
    public Image leftMashButtonImage;
    public Image rightMashButtonImage;
    public Image mashSliderFillImage;

    [Header("Set 5 Button Sprites")]
    public Sprite QNotPressed;
    public Sprite QPressed;
    public Sprite ENotPressed;
    public Sprite EPressed;


    // Start is called before the first frame update
    void Start()
    {
        AddToSetsDict();
        MicrogamesSetup();
    }

    public void ClearAll()
    {
        StopAllCoroutines();
        OWM.ResetObstaclesAndState();
        obstacles.Clear();
        inactiveSets.Clear();
        activeSets.Clear();
        foreach (var t in Timers)
        {
            t.SetActive(false);
        }
        Sets["Set1"] = false;
        Sets["Set2"] = false;
        Sets["Set3"] = false;
        Sets["Set4"] = false;
        Sets["Set5"] = false;
        Sets["Set6"] = false;
        Sets["Set7"] = false;
        Set1Parent.SetActive(false);
        Set2Parent.SetActive(false);
        Set3Parent.SetActive(false);
        Set4Parent.SetActive(false);
        Set5Parent.SetActive(false);
        Set6Parent.SetActive(false);
        Set7Parent.SetActive(false);

        memoryInputActive = false;
        memorySetupDone = false;
    }

    public void Initialize()
    {
        InitialInactiveSets();
        StartCoroutine(tick("Initial", 3));
    }
    void AddToSetsDict()
    {
        Sets.Add("Set1", false);  // WASD / L-Joystick Set
        Sets.Add("Set2", false);  // Mouse / R-Joystick Set // Mathf.Clamp
        Sets.Add("Set3", false);  // IJKL / D-Pad Set // Swap this to Arrow Keys
        Sets.Add("Set4", false);  // Arrow Keys / ABXY Set // Swap this to IJKL
        Sets.Add("Set5", false);  // QE / Bumpers Set
        Sets.Add("Set6", false);  // LMB/RMB / LT/RT Set
        Sets.Add("Set7", false);  // Space // Joystick Buttons Set
    }

    void MicrogamesSetup()
    {
        InitialSetup();
        InitialSetupSet1();
        InitialSetupSet4();
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

        Timers.Add(InitialTimer);
        Timers.Add(Set1Timer);
        Timers.Add(Set2Timer);
        Timers.Add(Set3Timer);
        Timers.Add(Set4Timer);
        Timers.Add(Set5Timer);
        Timers.Add(Set6Timer);
        Timers.Add(Set7Timer);
        TimerTexts.Add(InitialTimerText);
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

    void InitialSetupSet4()
    {
        foreach (Transform child in MemoryParent.transform)
        {
            Button btn = child.GetComponent<Button>();
            if (btn != null)
            {
                memoryButtons.Add(btn);
                btn.image.color = Color.white;
            }
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

    // Called at the start of Set 3
    void Set3Setup()
    {
        dpadCombo.Clear();
        dpadStep = 0;
        dpadArrowImages.Clear(); // <<<<<<< ADD THIS LINE

        // Clear old UI arrows
        foreach (Transform child in dpadComboParent.transform)
        {
            Destroy(child.gameObject);
        }

        string[] directions = { "Up", "Down", "Left", "Right" };
        int comboLength = rnd.Next(5, 9);

        for (int i = 0; i < comboLength; i++)
        {
            string dir = directions[rnd.Next(0, directions.Length)];
            dpadCombo.Add(dir);

            // Spawn UI Arrows
            GameObject arrow = Instantiate(dpadComboArrowPrefab, dpadComboParent.transform);
            Image img = arrow.GetComponent<Image>();

            switch (dir)
            {
                case "Up": img.sprite = upArrowSprite; break;
                case "Down": img.sprite = downArrowSprite; break;
                case "Left": img.sprite = leftArrowSprite; break;
                case "Right": img.sprite = rightArrowSprite; break;
            }

            dpadArrowImages.Add(img); // <<<<<<< Correct way
        }

        dpadInputActive = true;

        HighlightCurrentArrow(); // << Highlight the first arrow!
    }

    void Set5Setup()
    {
        mashCounter = 0;
        targetMashCount = UnityEngine.Random.Range(10, 16); // Random between 10 and 15 alternations
        mashUIParent.SetActive(true);
        mashActive = true;
        mashUIRect = mashUIParent.GetComponent<RectTransform>();
        lastInput = "";
        mashSlider.value = 0f;
        mashSlider.maxValue = 1f;
        // Set the initial color to RED
        if (mashSliderFillImage != null)
        {
            mashSliderFillImage.color = new Color(1f, 0.3f, 0.3f); // light red
        }
    }


    void FixedUpdate()
    {
        if (!microgamesActive)
            return;

        if (Sets.ContainsKey("Set1") && Sets["Set1"])
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
            DecayMashSlider();
        }
    }

    void ProcessInputs()
    {
        // Right Stick / Mouse Combined Input (for microgame spinning, etc.)
        float joyRX = Input.GetAxis("RS_h");
        float joyRY = Input.GetAxis("RS_v");
        float mouseX = Input.GetAxis("Mouse_X");
        float mouseY = Input.GetAxis("Mouse_Y");
        inputRAxisX = joyRX + mouseX;
        inputRAxisY = joyRY + mouseY;

        // Left Stick / WASD Combined Input (for Undertale-style Set 1 movement)
        float kbLX = Input.GetAxisRaw("LS_h_KB");
        float joyLX = Input.GetAxisRaw("LS_h");
        float kbLY = Input.GetAxisRaw("LS_v_KB");
        float joyLY = Input.GetAxisRaw("LS_v");
        inputLAxisX = kbLX + joyLX;
        inputLAxisY = kbLY + joyLY;

        // D-Pad / IJKL Combined Input with axis debounce
        float dpadHX = Input.GetAxisRaw("DPad_h") + Input.GetAxisRaw("DPad_h_KB");
        float dpadVY = Input.GetAxisRaw("DPad_v") + Input.GetAxisRaw("DPad_v_KB");

        inputDAxisX = 0f;
        inputDAxisY = 0f;

        // Handle horizontal axis debounce
        if (Mathf.Abs(dpadHX) > 0.5f)
        {
            if (!dpadAxisInUseX)
            {
                inputDAxisX = Mathf.Sign(dpadHX);
                dpadAxisInUseX = true;
            }
        }
        else
        {
            dpadAxisInUseX = false;
        }

        // Handle vertical axis debounce
        if (Mathf.Abs(dpadVY) > 0.5f)
        {
            if (!dpadAxisInUseY)
            {
                inputDAxisY = Mathf.Sign(dpadVY);
                dpadAxisInUseY = true;
            }
        }
        else
        {
            dpadAxisInUseY = false;
        }

        // ABXY Buttons
        inputA = Input.GetButtonDown(AButton);
        inputB = Input.GetButtonDown(BButton);
        inputX = Input.GetButtonDown(XButton);
        inputY = Input.GetButtonDown(YButton);

        // Shoulder Buttons (LB, RB)
        inputLB = Input.GetButtonDown(LB);
        inputRB = Input.GetButtonDown(RB);

        // Trigger Inputs: combined axis + mouse button detection
        inputLT = Input.GetAxis("LT_Axis") > 0.5f || Input.GetButtonDown(LT);
        inputRT = Input.GetAxis("RT_Axis") > 0.5f || Input.GetButtonDown(RT);
        Debug.Log("LT Axis: " + Input.GetAxis("LT_Axis"));
        Debug.Log("RT Axis: " + Input.GetAxis("RT_Axis"));


        // Joystick button presses (LS/RS press or space fallback)
        inputLS_b = Input.GetButtonDown(LS_b);
        inputRS_b = Input.GetButtonDown(RS_b);

        // Escape (used for exiting microgame)
        inputEscape = Input.GetButtonDown(Escape);

        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown("joystick button " + i))
                Debug.Log("Joystick Button Pressed: " + i);
        }

        string[] knownAxes = {
            "RS_h", "RS_v",
            "LS_h", "LS_v",
            "DPad_h", "DPad_v",
            "LT_Axis", "RT_Axis"
        };

        foreach (string axis in knownAxes)
        {
            float val = Input.GetAxis(axis);
            if (Mathf.Abs(val) > 0.1f)
                Debug.Log(axis + " = " + val);
        }

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
        if (Sets.ContainsKey("Set1") && Sets["Set1"])
        {
            Set1Parent.SetActive(true);
            MicrogameSet1();
        }
    }


    void MicrogamesInputs()
    {
        foreach (var set in new[] { "Set1", "Set2", "Set3", "Set4", "Set5", "Set6", "Set7" })
        {
            if (Sets.ContainsKey(set) && Sets[set])
            {
                switch (set)
                {
                    case "Set1": Set1Parent.SetActive(true); MicrogameSet1(); break;
                    case "Set2": Set2Parent.SetActive(true); MicrogameSet2(); break;
                    case "Set3": Set3Parent.SetActive(true); MicrogameSet3(); break;
                    case "Set4": Set4Parent.SetActive(true); MicrogameSet4(); break;
                    case "Set5": Set5Parent.SetActive(true); MicrogameSet5(); break;
                    case "Set6": Set6Parent.SetActive(true); MicrogameSet6(); break;
                    case "Set7": Set7Parent.SetActive(true); MicrogameSet7(); break;
                }
            }
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
        if (!dpadInputActive) return;

        float dpadX = inputDAxisX;
        float dpadY = inputDAxisY;

        if (Mathf.Abs(dpadX) > 0.5f)
        {
            if (dpadX > 0)
                RegisterDpadInput("Right");
            else
                RegisterDpadInput("Left");
        }
        else if (Mathf.Abs(dpadY) > 0.5f)
        {
            if (dpadY > 0)
                RegisterDpadInput("Up");
            else
                RegisterDpadInput("Down");
        }
    }

    void MicrogameSet4()
    {
        if (!memorySetupDone)
        {
            memorySetupDone = true;
            Set4Setup(); // Setup only once
        }
        else
        {
            CaptureMemoryInput();
        }
    }

    void MicrogameSet5()
    {
        if (!mashActive)
            return;

        bool qPressed = Input.GetKey(KeyCode.Q); // You can also use inputLB if you prefer
        bool ePressed = Input.GetKey(KeyCode.E); // Or inputRB

        // Update Q button sprite
        if (leftMashButtonImage != null)
        {
            leftMashButtonImage.sprite = qPressed ? QPressed : QNotPressed;
        }

        // Update E button sprite
        if (rightMashButtonImage != null)
        {
            rightMashButtonImage.sprite = ePressed ? EPressed : ENotPressed;
        }

        // Register Mash normally
        if (inputLB && lastInput != "Q")
        {
            RegisterMash("Q");
        }
        else if (inputRB && lastInput != "E")
        {
            RegisterMash("E");
        }
    }

    void DecayMashSlider()
    {
        if (!mashActive)
            return;

        float decayRate = Mathf.Lerp(0.05f, 0.3f, mashSlider.value); // Higher value = faster decay
        mashSlider.value -= decayRate * Time.deltaTime;

        // Clamp the slider so it doesn't go below 0
        if (mashSlider.value < 0f)
            mashSlider.value = 0f;
    }

    void MicrogameSet6()
    {
        if (!Set6Parent.activeSelf)
            Set6Parent.SetActive(true);

        // Reset rod when re-entering Set6
        Set6RodAlignment rod = FindObjectOfType<Set6RodAlignment>();
        if (rod != null && rod.IsMicrogameComplete())
        {
            FindObjectOfType<FishingProgress>().MicrogameBonus("Set6");
            Sets["Set6"] = false;
            activeSets.Remove("Set6");
            inactiveSets.Add("Set6");
            Set6Parent.SetActive(false);

            rod.ResetRod(); // <<< Reset the rod here
            StartCoroutine(DelayedResetSet6Obstacles());
        }
    }
    IEnumerator DelayedResetSet6Obstacles()
    {
        yield return null; // Wait one frame
        OWM.ResetObstaclesAndState();
    }


    void MicrogameSet7()
    {
        if (inputLS_b && inputRS_b)
        {
            Debug.Log("Set 7 Completed!");
            FindObjectOfType<FishingProgress>().MicrogameBonus("Set7");
            Sets["Set7"] = false;
            activeSets.Remove("Set7");
            inactiveSets.Add("Set7");
            Set7Parent.SetActive(false);
        }
    }

    void RegisterDpadInput(string input)
    {
        if (dpadCombo[dpadStep] == input)
        {
            Debug.Log("Correct Input: " + input);

            dpadArrowImages[dpadStep].color = new Color32(163, 233, 181, 255); // Green

            dpadStep++;

            if (dpadStep < dpadCombo.Count)
            {
                HighlightCurrentArrow();
            }
            else
            {
                StartCoroutine(Set3Completed());
            }
        }
        else
        {
            Debug.Log("Incorrect! Flash Red.");
            StartCoroutine(FlashRedThenReset());
        }
    }

    void HighlightCurrentArrow()
    {
        for (int i = 0; i < dpadArrowImages.Count; i++)
        {
            if (i == dpadStep)
                dpadArrowImages[i].color = Color.yellow; // Highlight active
            else if (dpadArrowImages[i].color != new Color32(163, 233, 181, 255))
                dpadArrowImages[i].color = Color.white; // Reset others unless completed
        }
    }

    IEnumerator FlashRedThenReset()
    {
        dpadInputActive = false; // Stop inputs

        // Flash the current arrow red
        if (dpadStep < dpadArrowImages.Count)
        {
            dpadArrowImages[dpadStep].color = new Color32(230, 35, 22, 255); // Bright Red
        }

        yield return new WaitForSeconds(0.5f); // Wait a moment so the player notices

        Set3Setup(); // Restart combo
    }

    IEnumerator Set3Completed()
    {
        dpadInputActive = false;
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Set 3 Completed!");
        FindObjectOfType<FishingProgress>().MicrogameBonus("Set3");
        Sets["Set3"] = false;
        activeSets.Remove("Set3");
        inactiveSets.Add("Set3");
        Set3Parent.SetActive(false);
    }

    void Set4Setup()
    {
        greyCoroutines.Clear();

        foreach (Button btn in memoryButtons)
        {
            btn.image.color = Color.white; // reset buttons to normal
        }

        if (memoryShowCoroutine != null)
        {
            StopCoroutine(memoryShowCoroutine);
            memoryShowCoroutine = null;
        }

        memoryCombo.Clear();
        playerInputCombo.Clear();
        memoryStep = 0;

        int comboLength = rnd.Next(3, 6); // Random between 3 and 5
        string[] options = { "A", "B", "X", "Y" };

        for (int i = 0; i < comboLength; i++)
        {
            memoryCombo.Add(options[rnd.Next(0, options.Length)]);
        }

        memoryShowCoroutine = StartCoroutine(ShowMemoryCombo());
    }

    IEnumerator ShowMemoryCombo()
    {
        memoryText.text = "Ready...";
        yield return StartCoroutine(FadeText(memoryText, 0f, 1f, 0.3f));
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(FadeText(memoryText, 1f, 0f, 0.3f));
        yield return new WaitForSeconds(0.2f);

        memoryText.text = "Set...";
        yield return StartCoroutine(FadeText(memoryText, 0f, 1f, 0.3f));
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(FadeText(memoryText, 1f, 0f, 0.3f));
        yield return new WaitForSeconds(0.2f);

        memoryText.text = "Go!";
        yield return StartCoroutine(FadeText(memoryText, 0f, 1f, 0.3f));

        List<string> comboCopy = new List<string>(memoryCombo);

        foreach (string btn in comboCopy)
        {
            Button flashBtn = GetButtonFromName(btn);
            Color originalColor = flashBtn.image.color;
            Vector3 originalScale = flashBtn.transform.localScale;

            // Pick color based on button
            Color flashColor = Color.white;
            switch (btn)
            {
                case "A":
                    flashColor = new Color32(0, 255, 0, 255); // Green
                    break;
                case "B":
                    flashColor = new Color32(255, 0, 0, 255); // Red
                    break;
                case "X":
                    flashColor = new Color32(0, 128, 255, 255); // Blue
                    break;
                case "Y":
                    flashColor = new Color32(255, 255, 0, 255); // Yellow
                    break;
            }

            // Flash color and slightly grow the button
            flashBtn.image.color = flashColor;
            flashBtn.transform.localScale = originalScale * 1.2f; // Grow by 20%

            yield return new WaitForSeconds(0.3f);

            // Reset color and size
            flashBtn.image.color = originalColor;
            flashBtn.transform.localScale = originalScale;

            yield return new WaitForSeconds(0.2f);
        }

        memoryInputActive = true;
        memorySetupDone = true;
    }

    Button GetButtonFromName(string btnName)
    {
        foreach (Button btn in memoryButtons)
        {
            if (btn.name == btnName)
                return btn;
        }
        return null;
    }

    void CaptureMemoryInput()
    {
        if (!memoryInputActive)
            return; // <<<<<< Don't capture input if not allowed yet!

        if (Input.GetButtonDown(AButton))
        {
            playerInputCombo.Add("A");
            GreyOutButton("A");
        }
        if (Input.GetButtonDown(BButton))
        {
            playerInputCombo.Add("B");
            GreyOutButton("B");
        }
        if (Input.GetButtonDown(XButton))
        {
            playerInputCombo.Add("X");
            GreyOutButton("X");
        }
        if (Input.GetButtonDown(YButton))
        {
            playerInputCombo.Add("Y");
            GreyOutButton("Y");
        }

        if (playerInputCombo.Count >= memoryCombo.Count)
        {
            memoryInputActive = false;
            StartCoroutine(CheckMemoryCombo());
        }
    }

    void GreyOutButton(string btnName)
    {
        if (!memoryInputActive) return;

        Button btn = GetButtonFromName(btnName);
        if (btn != null)
        {
            // If there's already a running grey flash, stop it
            if (greyCoroutines.ContainsKey(btn) && greyCoroutines[btn] != null)
            {
                StopCoroutine(greyCoroutines[btn]);
            }

            // Start a new grey flash and store it
            greyCoroutines[btn] = StartCoroutine(FlashGrey(btn));
        }
    }

    IEnumerator FlashGrey(Button btn)
    {
        btn.image.color = Color.gray;
        yield return new WaitForSeconds(0.2f);

        if (memoryInputActive)
        {
            btn.image.color = Color.white;
        }

        // After the flash is done, clear the reference
        if (greyCoroutines.ContainsKey(btn))
        {
            greyCoroutines[btn] = null;
        }
    }

    IEnumerator CheckMemoryCombo()
    {
        bool success = true;

        for (int i = 0; i < memoryCombo.Count; i++)
        {
            if (playerInputCombo[i] != memoryCombo[i])
            {
                success = false;
                break;
            }
        }

        Color flashColor = success ? new Color32(163, 233, 181, 255) : new Color32(230, 35, 22, 255);

        foreach (Button btn in memoryButtons)
        {
            btn.image.color = flashColor;
        }

        memoryText.text = success ? "Success!" : "Wrong!";

        yield return new WaitForSeconds(1f);

        if (success)
        {
            Debug.Log("Set 4 Completed!");
            FindObjectOfType<FishingProgress>().MicrogameBonus("Set4");
            Sets["Set4"] = false;
            activeSets.Remove("Set4");
            inactiveSets.Add("Set4");
            Set4Parent.SetActive(false);
            memoryText.text = "";
            memorySetupDone = false; // reset setup flag
        }
        else
        {
            foreach (Button btn in memoryButtons)
            {
                btn.image.color = Color.white;
            }
            memorySetupDone = false; // reset setup flag
            Set4Setup();
        }
    }

    IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
    {
        Color color = text.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            text.color = color;
            yield return null;
        }

        color.a = endAlpha; // Final correction
        text.color = color;
    }
    void ResetMashButtons()
    {
        leftMashButtonImage.color = Color.white;
        leftMashButtonImage.transform.localScale = Vector3.one;

        rightMashButtonImage.color = Color.white;
        rightMashButtonImage.transform.localScale = Vector3.one;
    }

    void RegisterMash(string input)
    {
        lastInput = input;

        // Increase slider fill
        mashSlider.value += 0.05f; // Tune this amount for difficulty

        UpdateMashButtonsVisuals(); // Update button states

        StartCoroutine(VibrateMashUI());

        if (mashSlider.value >= 1f)
        {
            Debug.Log("Set 5 Completed!");
            FindObjectOfType<FishingProgress>().MicrogameBonus("Set5");
            mashActive = false;
            Sets["Set5"] = false;
            activeSets.Remove("Set5");
            inactiveSets.Add("Set5");
            ResetMashButtons();
            mashUIParent.SetActive(false);
            Set5Parent.SetActive(false);
        }
    }

    void UpdateMashButtonsVisuals()
    {
        if (mashSlider == null || mashSliderFillImage == null)
            return;

        float fill = mashSlider.value;

        // Define the color stages
        Color redColor = new Color(1f, 0.3f, 0.3f);     // Light red
        Color yellowColor = new Color(1f, 1f, 0.4f);    // Soft yellow
        Color greenColor = new Color(0.6f, 1f, 0.6f);   // Light green

        Color dynamicColor;

        if (fill < 0.5f)
        {
            // Blend from Red to Yellow (0% to 50%)
            dynamicColor = Color.Lerp(redColor, yellowColor, fill / 0.5f);
        }
        else
        {
            // Blend from Yellow to Green (50% to 100%)
            dynamicColor = Color.Lerp(yellowColor, greenColor, (fill - 0.5f) / 0.5f);
        }

        // Apply to the slider's fill image
        mashSliderFillImage.color = dynamicColor;

        // (keep button scaling based on last input)
        if (lastInput == "Q")
        {
            leftMashButtonImage.transform.localScale = Vector3.one * 1f;
            rightMashButtonImage.transform.localScale = Vector3.one * 1.2f;
        }
        else if (lastInput == "E")
        {
            rightMashButtonImage.transform.localScale = Vector3.one * 1f;
            leftMashButtonImage.transform.localScale = Vector3.one * 1.2f;
        }
    }

    IEnumerator VibrateMashUI()
    {
        Vector3 originalPos = mashUIRect.localPosition;
        float shakeAmount = 5f;

        // Shake randomly around original position
        mashUIRect.localPosition = originalPos + (Vector3)UnityEngine.Random.insideUnitCircle * shakeAmount;
        yield return new WaitForSeconds(0.05f);

        mashUIRect.localPosition = originalPos; // Reset position
    }

    void InitialInactiveSets()
    {
        inactiveSets.Clear();
        activeSets.Clear();

        if (InitiateMicrogames.Instance == null)
        {
            Debug.LogWarning("InitiateMicrogames instance not found!");
            return;
        }

        List<string> activeFromZone = InitiateMicrogames.Instance.ActiveMicrogameSets;

        // Clear and repopulate Sets dictionary safely
        Sets.Clear();
        foreach (string setName in activeFromZone)
        {
            Sets[setName] = false; // Initially inactive
            inactiveSets.Add(setName);
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
        while (true)
        {
            int mgCD = rnd.Next(1, 11);
            Debug.Log("Microgame Cooldown is running for " + mgCD + " seconds!");
            yield return new WaitForSeconds(mgCD);

            MicrogameStarter();
        }
    }

    void InitiateTimer(string timerID, int time)
    {
        switch (timerID)
        {
            case "Set7":
                Timers[7].SetActive(true);
                TimerTexts[7].text = time.ToString();
                break;
            case "Set6":
                Timers[6].SetActive(true);
                TimerTexts[6].text = time.ToString();
                break;
            case "Set5":
                Timers[5].SetActive(true);
                TimerTexts[5].text = time.ToString();
                break;
            case "Set4":
                Timers[4].SetActive(true);
                TimerTexts[4].text = time.ToString();
                break;
            case "Set3":
                Timers[3].SetActive(true);
                TimerTexts[3].text = time.ToString();
                break;
            case "Set2":
                Timers[2].SetActive(true);
                TimerTexts[2].text = time.ToString();
                break;
            case "Set1":
                Timers[1].SetActive(true);
                TimerTexts[1].text = time.ToString();
                break;
            case "Initial":
                Timers[0].SetActive(true);
                TimerTexts[0].text = time.ToString();
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
                    Timers[7].SetActive(false);
                    Sets["Set7"] = true;
                    break;
                case "Set6":
                    // Start Set 6
                    Debug.Log("Set 6 Microgame is now ACTIVE!");
                    Timers[6].SetActive(false);
                    Sets["Set6"] = true;
                    break;
                case "Set5":
                    // Start Set 5
                    Debug.Log("Set 5 Microgame is now ACTIVE!");
                    Timers[5].SetActive(false);
                    Set5Setup();
                    Sets["Set5"] = true;
                    break;
                case "Set4":
                    // Start Set 4
                    Debug.Log("Set 4 Microgame is now ACTIVE!");
                    Timers[4].SetActive(false);
                    Sets["Set4"] = true;
                    break;
                case "Set3":
                    // Start Set 3
                    Debug.Log("Set 3 Microgame is now ACTIVE!");
                    Timers[3].SetActive(false);
                    Set3Setup();
                    Sets["Set3"] = true;
                    break;
                case "Set2":
                    // Start Set 2
                    Debug.Log("Set 2 Microgame is now ACTIVE!");
                    Timers[2].SetActive(false);
                    Sets["Set2"] = true;
                    break;
                case "Set1":
                    // Start Set 1
                    Debug.Log("Set 1 Microgame is now ACTIVE!");
                    Timers[1].SetActive(false);
                    Sets["Set1"] = true;
                    Set1Setup();
                    StartCoroutine(Set1Obstacles());
                    break;
                case "Initial":
                    Debug.Log("Microgames are now ACTIVE!");
                    Timers[0].SetActive(false);
                    microgamesActive = true;
                    MicrogameStarter();
                    StartCoroutine(microgameCooldown());  // <-- This is fine, once.
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
            FindObjectOfType<FishingProgress>().MicrogameBonus("Set1");
            Sets["Set1"] = false;
            activeSets.Remove("Set1");
            inactiveSets.Add("Set1");
            Set1Parent.SetActive(false);
        }
    }

    public void ConfigureFishingZone(FishingZoneType zoneType)
    {
        // Always-included sets
        List<string> requiredSets = new List<string> { "Set3", "Set4", "Set5", "Set7" };

        // Zone-specific sets
        switch (zoneType)
        {
            case FishingZoneType.Pond:
            case FishingZoneType.BossPond:
                requiredSets.Add("Set6");
                break;
            case FishingZoneType.River:
            case FishingZoneType.BossRiver:
                requiredSets.Add("Set1");
                break;
            case FishingZoneType.Ocean:
            case FishingZoneType.BossOcean:
                requiredSets.Add("Set2");
                break;
        }

        // Final boss will manually include all 7 — not handled here

        // Reset sets
        Sets.Clear();
        inactiveSets.Clear();
        activeSets.Clear();

        // Add only required sets to dict and inactive list
        foreach (string set in requiredSets)
        {
            Sets.Add(set, false);
            inactiveSets.Add(set);
        }

        // Disable all parent objects
        Set1Parent.SetActive(false);
        Set2Parent.SetActive(false);
        Set3Parent.SetActive(false);
        Set4Parent.SetActive(false);
        Set5Parent.SetActive(false);
        Set6Parent.SetActive(false);
        Set7Parent.SetActive(false);

        Debug.Log("Fishing zone configured for: " + zoneType);
    }

}