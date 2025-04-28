using System.Collections;
using UnityEngine;
using TMPro;

public class ShopInteractionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public GameObject specialButton;
    public TextMeshProUGUI specialButtonText;
    public TextMeshProUGUI textComponent;
    public GameObject dialogueButton;
    public GameObject shopBaseDialogue;
    public TextMeshProUGUI rodNameText;
    public UnityEngine.UI.Image rodImage;
    public Sprite upgradedRodSprite;
    public Sprite professionalRodSprite;
    public Sprite defaultRodSprite;

    [Header("Dialogue Settings")]
    public float textSpeed = 0.05f;
    private bool waitingForCutscene = false;

    [Header("Cutscenes")]
    public CutsceneManager boneRodCutsceneManager;
    public CutsceneManager finalCutsceneManager;
    public GameObject loadingScreen;

    [Header("Lighting References")]
    public GameObject mainLights;
    public GameObject emergencyLights;

    [Header("Legendary Canvas")]
    public GameObject legendaryCanvas;
    public GameObject reelObject;
    public GameObject spineObject;
    public GameObject hooksObject;
    public GameObject rodObject;
    public float visualDisplayTime = 5f;
    [SerializeField] public CanvasGroup legendaryCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Base Dialogue Content")]
    public string[] firstVisitBaseDialogue;
    public string[] initialBaseDialogue;
    public string[] afterPondBossDialogue;
    public string[] afterRiverBossDialogue;
    public string[] afterOceanBossDialogue;
    public string[] afterDredgedHandDialogue;
    public string[] afterAllCollectedDialogue;
    public string[] finalStateDialogue;

    [Header("Special Dialogue Content")]
    public string[] firstVisitSpecialDialogue;
    public string[] scrollDialogue;
    public string[] hairDialogue;
    public string[] boneRodDialogue;
    public string[] handDialogue;
    public string[] vesselDialogue;
    public string[] postBossDialogue;

    // New variables for scene preloading
    private AsyncOperation preloadedSceneOperation;
    private bool scenePreloadComplete = false;

    private SaveData saveData;
    private string[] currentLines;
    private int dialogueIndex;
    private bool isTyping = false;
    private Coroutine typingCoroutine;
    private string lastSpecialPlayed = "";

    void Start()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene.");
            return;
        }

        saveData = SaveManager.Load();
        if (saveData == null)
        {
            Debug.LogError("Save data could not be loaded.");
            return;
        }

        if (!saveData.arrivedAtShop)
        {
            if (rodImage != null) rodImage.enabled = false;
            if (rodNameText != null) rodNameText.text = "";
        }

        if (saveData.hasTurnedInRod)
        {
            SetEmergencyLighting();
        }
        else
        {
            SetNormalLighting();
        }
        // Hide legendary items at start
        if (legendaryCanvas != null)
        {
            legendaryCanvas.SetActive(false);
        }
        
        if (reelObject != null) reelObject.SetActive(false);
        if (spineObject != null) spineObject.SetActive(false);
        if (hooksObject != null) hooksObject.SetActive(false);
        if (rodObject != null) rodObject.SetActive(false);

        UpdateShopState();
        dialoguePanel.SetActive(false);
        textComponent.text = string.Empty;
    }

    void Update()
    {
        // Only handle mouse clicks when dialogue is active and we're not waiting for a cutscene
        if (dialoguePanel.activeSelf && !waitingForCutscene && Input.GetMouseButtonDown(0))
        {
            if (!isTyping || textComponent.text == currentLines[dialogueIndex])
            {
                NextLine();
            }
            else
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                    isTyping = false;
                }
                textComponent.text = currentLines[dialogueIndex];
            }
        }
    }

    public void UpdateShopState()
    {
        UpdateSpecialButton();
        UpdateRodVisuals();
    }

    void UpdateRodVisuals()
    {
        if (!saveData.arrivedAtShop)
        {
            if (rodImage != null) rodImage.enabled = false;
            if (rodNameText != null) rodNameText.text = "";
            return;
        }

        if (rodImage != null) rodImage.enabled = true;

        if (saveData.hasTurnedInHair)
        {
            rodNameText.text = "Professional Rod";
            rodImage.sprite = professionalRodSprite;
        }
        else if (saveData.hasTurnedInScroll)
        {
            rodNameText.text = "Enhanced Fishing Rod";
            rodImage.sprite = upgradedRodSprite;
        }
        else
        {
            rodNameText.text = "Basic Fishing Rod";
            rodImage.sprite = defaultRodSprite;
        }
    }

    void UpdateSpecialButton()
    {
        // Hide special button by default
        specialButton.SetActive(false);

        // Show special button based on game progression
        if (!saveData.arrivedAtShop)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Ask him where this place is.";
        }
        if (saveData.hasCaughtPondBoss && !saveData.hasCaughtRiverBoss && !saveData.hasTurnedInScroll)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Ask about the strange scroll.";
        }
        else if (saveData.hasCaughtRiverBoss && !saveData.hasCaughtOceanBoss && !saveData.hasTurnedInHair)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Ask about... the hair.";
        }
        else if (saveData.hasCaughtOceanBoss && !saveData.dredgedHand && !saveData.hasTurnedInRod)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Ask about the spine.";
        }
        else if (saveData.dredgedHand && !saveData.AllCollected)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Show the hand to the shopkeeper.";
        }
        else if (saveData.AllCollected && !saveData.BecameHorizonAngler)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Prepare the vessel.";
        }
        else if (saveData.BecameHorizonAngler)
        {
            specialButton.SetActive(true);
            specialButtonText.text = "Hand him the lower half.";
        }
    }

    public void OnTalkButton()
    {
        string[] selectedDialogue = null;

        if (!saveData.arrivedAtShop)
        {
            selectedDialogue = firstVisitBaseDialogue;
        }
        else if (saveData.arrivedAtShop && !saveData.hasCaughtPondBoss)
        {
            selectedDialogue = initialBaseDialogue;
        }
        else if (saveData.hasCaughtPondBoss && !saveData.hasCaughtRiverBoss)
        {
            selectedDialogue = afterPondBossDialogue;
        }
        else if (saveData.hasCaughtRiverBoss && !saveData.hasCaughtOceanBoss)
        {
            selectedDialogue = afterRiverBossDialogue;
        }
        else if (saveData.hasCaughtOceanBoss && !saveData.dredgedHand)
        {
            selectedDialogue = afterOceanBossDialogue;
        }
        else if (saveData.dredgedHand && !saveData.AllCollected)
        {
            selectedDialogue = afterDredgedHandDialogue;
        }
        else if (saveData.AllCollected && !saveData.dredgedHand)
        {
            selectedDialogue = afterAllCollectedDialogue;
        }
        else
        {
            selectedDialogue = firstVisitBaseDialogue;
        }

        // Choose a random line from the selected dialogue
        if (selectedDialogue != null && selectedDialogue.Length > 0)
        {
            string[] randomLine = new string[1];
            randomLine[0] = selectedDialogue[Random.Range(0, selectedDialogue.Length)];
            StartDialogue(randomLine);
        }
        else
        {
            Debug.LogWarning("No dialogue lines found for current state.");
        }
    }

    public void OnSpecialButton()
    {
        // Play special dialogue based on game progress
        if (!saveData.arrivedAtShop)
        {
            lastSpecialPlayed = "FirstVisit";
            StartDialogue(firstVisitSpecialDialogue);
            return;
        }
        if (saveData.hasCaughtPondBoss && !saveData.hasCaughtRiverBoss)
        {
            lastSpecialPlayed = "Scroll";
            StartDialogue(scrollDialogue);
        }
        else if (saveData.hasCaughtRiverBoss && !saveData.hasCaughtOceanBoss)
        {
            lastSpecialPlayed = "Hair";
            StartDialogue(hairDialogue);
        }
        else if (saveData.hasCaughtOceanBoss && !saveData.dredgedHand)
        {
            lastSpecialPlayed = "BoneRod";
            StartDialogue(boneRodDialogue);
        }
        else if (saveData.dredgedHand && !saveData.AllCollected)
        {
            lastSpecialPlayed = "Hand";
            StartDialogue(handDialogue);
        }
        else if (saveData.AllCollected && !saveData.BecameHorizonAngler)
        {
            lastSpecialPlayed = "Vessel";
            StartDialogue(vesselDialogue);
        }
        else if (saveData.BecameHorizonAngler)
        {
            lastSpecialPlayed = "Final";
            StartDialogue(postBossDialogue);
            // Start preloading the scene when final dialogue is triggered
            PrepareLoadingScreenDuringCutscene();
        }
    }

    // New method to prepare loading screen and preload next scene
    public void PrepareLoadingScreenDuringCutscene()
    {
        Debug.Log("Preparing loading screen while cutscene is playing");
        
        // Make sure we have a reference to the loading screen
        if (loadingScreen == null)
        {
            loadingScreen = GameObject.Find("FinalLoadingScreen");
            if (loadingScreen == null)
            {
                Debug.LogError("Could not find FinalLoadingScreen in scene!");
                return;
            }
        }
        
        // Make sure loading screen has a canvas with proper sorting order
        Canvas loadingCanvas = loadingScreen.GetComponent<Canvas>();
        if (loadingCanvas == null)
        {
            loadingCanvas = loadingScreen.AddComponent<Canvas>();
            loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        loadingCanvas.sortingOrder = 5; // High enough to be on top after cutscene ends
        
        // Make sure it has a canvas group
        CanvasGroup loadingGroup = loadingScreen.GetComponent<CanvasGroup>();
        if (loadingGroup == null)
        {
            loadingGroup = loadingScreen.AddComponent<CanvasGroup>();
        }
        
        // Set it to be invisible initially but active
        loadingScreen.SetActive(true);
        loadingGroup.alpha = 0f;
        
        // Start preloading the next scene
        StartCoroutine(PreloadNextScene());
    }

    // New method to preload next scene
    private IEnumerator PreloadNextScene()
    {
        // Start loading the next scene asynchronously
        Debug.Log("Preloading PostPond scene");
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("PostPond");
        asyncLoad.allowSceneActivation = false; // Don't activate until we're ready
        
        // Store this for later use
        preloadedSceneOperation = asyncLoad;
        
        while (!asyncLoad.isDone && asyncLoad.progress < 0.9f)
        {
            Debug.Log($"Preloading progress: {asyncLoad.progress * 100:F0}%");
            yield return null;
        }
        
        Debug.Log("Preloading complete - scene ready to activate when needed");
        scenePreloadComplete = true;
    }

    void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogError("Attempted to start dialogue with null or empty lines array");
            return;
        }

        // Setup dialogue
        currentLines = lines;
        dialogueIndex = 0;
        textComponent.text = string.Empty;

        // Hide shop UI
        shopBaseDialogue.SetActive(false);
        dialogueButton.SetActive(false);
        
        // Show dialogue panel
        dialoguePanel.SetActive(true);

        // Start typing the first line
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        textComponent.text = string.Empty;
        
        foreach (char c in currentLines[dialogueIndex].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;
    }

    void NextLine()
    {
        // Don't proceed if waiting for cutscene
        if (waitingForCutscene) return;

        dialogueIndex++;

        // Check if we've reached the end of dialogue
        if (dialogueIndex >= currentLines.Length)
        {
            // End of dialogue, show shop UI again
            dialoguePanel.SetActive(false);
            shopBaseDialogue.SetActive(true);
            dialogueButton.SetActive(true);

            // Handle First Visit
            if (lastSpecialPlayed == "FirstVisit" && !saveData.arrivedAtShop)
            {
                saveData.arrivedAtShop = true;
                if (GameManager.Instance != null)
                    GameManager.Instance.currentSaveData.arrivedAtShop = true;

                SaveManager.Save(saveData);
                Debug.Log("Player has arrived at the shop for the first time.");

                if (rodImage != null && defaultRodSprite != null)
                {
                    rodImage.enabled = true;
                    rodImage.sprite = defaultRodSprite;
                }
                if (rodNameText != null)
                {
                    rodNameText.text = "Basic Fishing Rod";
                }
            }

            // Handle special dialogue progression
            if (lastSpecialPlayed == "Scroll" && !saveData.hasTurnedInScroll)
            {
                saveData.hasTurnedInScroll = true;
                GameManager.Instance.currentSaveData.hasTurnedInScroll = true;
                Debug.Log("hasTurnedInScroll set to TRUE");
            }
            else if (lastSpecialPlayed == "Hair" && !saveData.hasTurnedInHair)
            {
                saveData.hasTurnedInHair = true;
                GameManager.Instance.currentSaveData.hasTurnedInHair = true;
                Debug.Log("hasTurnedInHair set to TRUE");
            }
            else if (lastSpecialPlayed == "BoneRod" && !saveData.hasTurnedInRod)
            {
                saveData.hasTurnedInRod = true;
                GameManager.Instance.currentSaveData.hasTurnedInRod = true;
                StartCoroutine(DelayedPinUpdate());
                Debug.Log("hasTurnedInRod set to TRUE");
            }

            if (lastSpecialPlayed == "Hand" && !saveData.AllCollected)
            {
                saveData.AllCollected = true;
                GameManager.Instance.currentSaveData.AllCollected = true;
                Debug.Log("AllCollected has been set to TRUE.");
            }
            else if (lastSpecialPlayed == "Vessel" && !saveData.readyForFight)
            {
                saveData.readyForFight = true;
                GameManager.Instance.currentSaveData.readyForFight = true;
                Debug.Log("readyForFight has been set to TRUE.");
            }

            SaveManager.Save(saveData);
            UpdateShopState(); // <- updates rod visuals + special button logic
            lastSpecialPlayed = "";
            return;
        }

        // Handle special visuals for Hand dialogue
        int zeroBasedIndex = dialogueIndex - 1;
        if (lastSpecialPlayed == "Hand")
        {
            if (zeroBasedIndex == 3) { ShowLegendaryItem(reelObject); return; }
            if (zeroBasedIndex == 5) { ShowLegendaryItem(spineObject); return; }
            if (zeroBasedIndex == 8) { ShowLegendaryItem(hooksObject); return; }
            if (zeroBasedIndex == 9) { ShowLegendaryItem(rodObject); return; }
        }

        // Handle BoneRod cutscene
        if (lastSpecialPlayed == "BoneRod" && dialogueIndex == 4)
        {
            CutsceneManager cutscene = FindObjectOfType<CutsceneManager>();
            if (cutscene != null)
            {
                waitingForCutscene = true;
                cutscene.PlayCutscene();
                SetEmergencyLighting();
                return;
            }
        }

        // Handle final boss cutscene
        if (lastSpecialPlayed == "Final" && dialogueIndex == currentLines.Length - 1)
        {
            if (finalCutsceneManager != null)
            {
                waitingForCutscene = true;
                
                // Make sure cutscene canvas is on top temporarily
                Canvas cutsceneCanvas = finalCutsceneManager.cutsceneCanvas.GetComponent<Canvas>();
                if (cutsceneCanvas != null)
                {
                    cutsceneCanvas.sortingOrder = 10; // Higher than loading screen
                }
                
                // Same for credit canvas
                if (finalCutsceneManager.creditCanvas != null)
                {
                    Canvas creditCanvas = finalCutsceneManager.creditCanvas.GetComponent<Canvas>();
                    if (creditCanvas != null)
                    {
                        creditCanvas.sortingOrder = 10;
                    }
                }
                
                finalCutsceneManager.PlayCutscene();
                return;
            }
        }

        // Continue dialogue
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator DelayedPinUpdate()
    {
        yield return new WaitForSeconds(0.1f); // Small delay ensures all pins have initialized

        foreach (MapTravelPoint pin in FindObjectsOfType<MapTravelPoint>())
        {
            pin.SetupButton();  // Now it's public, so this works
        }
    }

    void ShowLegendaryItem(GameObject itemToShow)
    {
        if (itemToShow == null)
        {
            Debug.LogError("Item reference is null");
            NextLine();
            return;
        }

        Debug.Log($"Showing item: {itemToShow.name}");

        // Hide dialogue
        dialoguePanel.SetActive(false);

        // Ensure legendary canvas is active
        if (legendaryCanvas != null)
            legendaryCanvas.SetActive(true);

        // Hide all legendary items first
        reelObject?.SetActive(false);
        spineObject?.SetActive(false);
        hooksObject?.SetActive(false);
        rodObject?.SetActive(false);

        // Show the desired item
        itemToShow.SetActive(true);

        // Fade in the canvas
        StartCoroutine(FadeCanvasGroup(legendaryCanvasGroup, 0f, 1f, fadeDuration));

        // Wait before fading out
        waitingForCutscene = true;
        StartCoroutine(ContinueAfterFadeDisplay(itemToShow));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration)
    {
        float elapsed = 0f;
        group.alpha = start;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        group.alpha = end;
    }

    IEnumerator ContinueAfterFadeDisplay(GameObject itemToHide)
    {
        yield return new WaitForSeconds(visualDisplayTime);

        // Fade out
        yield return FadeCanvasGroup(legendaryCanvasGroup, 1f, 0f, fadeDuration);

        // Cleanup
        itemToHide.SetActive(false);
        if (legendaryCanvas != null)
            legendaryCanvas.SetActive(false);

        waitingForCutscene = false;
        dialoguePanel.SetActive(true);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine());
    }

    void ReturnToDialogue()
    {
        Debug.Log("Returning to dialogue");
        
        // Hide legendary items
        if (legendaryCanvas != null)
        {
            legendaryCanvas.SetActive(false);
        }
        
        if (reelObject != null) reelObject.SetActive(false);
        if (spineObject != null) spineObject.SetActive(false);
        if (hooksObject != null) hooksObject.SetActive(false);
        if (rodObject != null) rodObject.SetActive(false);
        
        // Show dialogue panel
        dialoguePanel.SetActive(true);
        
        // Reset waiting flag
        waitingForCutscene = false;
        
        // Continue dialogue
        NextLine();
    }

    public void ResumeDialogueAfterCutscene()
    {
        if (lastSpecialPlayed == "Final")
        {
            Debug.Log("Final Cutscene ended - starting final loading screen.");
            StartCoroutine(StartFinalLoadingSequence());
        }
        else
        {
            waitingForCutscene = false;
            NextLine();
        }
    }

    private IEnumerator StartFinalLoadingSequence()
    {
        Debug.Log("StartFinalLoadingSequence called");
        
        // Make sure game time is running
        Time.timeScale = 1f;
        
        // Ensure loading screen is prepared
        if (loadingScreen == null || !loadingScreen.activeInHierarchy)
        {
            Debug.LogError("Loading screen not properly prepared! Attempting to recover...");
            PrepareLoadingScreenDuringCutscene();
            yield return new WaitForSeconds(0.5f);
        }
        
        // Get the canvas group
        CanvasGroup loadingGroup = loadingScreen.GetComponent<CanvasGroup>();
        if (loadingGroup == null)
        {
            Debug.LogError("No CanvasGroup on loading screen! Adding one...");
            loadingGroup = loadingScreen.AddComponent<CanvasGroup>();
        }
        
        // Make sure loading screen canvas is on top now that cutscene is finished
        Canvas loadingCanvas = loadingScreen.GetComponent<Canvas>();
        if (loadingCanvas != null)
        {
            loadingCanvas.sortingOrder = 15; // Higher than cutscene now
            Debug.Log($"Set loading screen canvas sorting order to {loadingCanvas.sortingOrder}");
        }
        
        // Instantly show the loading screen
        loadingGroup.alpha = 1f;
        Debug.Log("Loading screen is now visible");
        
        // Wait for the dramatic text to be visible
        Debug.Log("Displaying final text for 5 seconds...");
        yield return new WaitForSecondsRealtime(5f);
        
        // Wait for preloading to complete if it hasn't already
        if (!scenePreloadComplete && preloadedSceneOperation != null)
        {
            Debug.Log("Waiting for scene preloading to complete...");
            while (preloadedSceneOperation.progress < 0.9f)
            {
                Debug.Log($"Loading progress: {preloadedSceneOperation.progress * 100:F0}%");
                yield return null;
            }
        }
        else if (preloadedSceneOperation == null)
        {
            // If no preload was started for some reason, start loading now
            Debug.Log("No preloaded scene found, starting async load now");
            preloadedSceneOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("PostPond");
            preloadedSceneOperation.allowSceneActivation = false;
            
            while (preloadedSceneOperation.progress < 0.9f)
            {
                Debug.Log($"Loading progress: {preloadedSceneOperation.progress * 100:F0}%");
                yield return null;
            }
        }
        
        // Set a static flag so the PostPond scene knows to fade in from black
        // (You'll need to add a static class to store this flag)
        TransitionManager.ShouldFadeInNextScene = true;
        
        // Finally activate the new scene without fading out
        if (preloadedSceneOperation != null)
        {
            Debug.Log("Activating preloaded PostPond scene");
            preloadedSceneOperation.allowSceneActivation = true;
        }
        else
        {
            Debug.LogError("No preloaded scene operation! Loading scene directly.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("PostPond");
        }
    }

    void UpgradeRodVisuals()
    {
        Debug.Log("UpgradeRodVisuals called");

        if (rodNameText != null)
        {
            if (lastSpecialPlayed == "Scroll")
                rodNameText.text = "Enhanced Fishing Rod";
            else if (lastSpecialPlayed == "Hair")
                rodNameText.text = "Professional Rod";
        }

        if (rodImage != null)
        {
            if (lastSpecialPlayed == "Scroll" && upgradedRodSprite != null)
                rodImage.sprite = upgradedRodSprite;
            else if (lastSpecialPlayed == "Hair" && professionalRodSprite != null)
                rodImage.sprite = professionalRodSprite;
        }
    }

    private void SetEmergencyLighting()
    {
        if (mainLights != null) mainLights.SetActive(false);
        if (emergencyLights != null) emergencyLights.SetActive(true);
    }

    private void SetNormalLighting()
    {
        if (mainLights != null) mainLights.SetActive(true);
        if (emergencyLights != null) emergencyLights.SetActive(false);
    }
}