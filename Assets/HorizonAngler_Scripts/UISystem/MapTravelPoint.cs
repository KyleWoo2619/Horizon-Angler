using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapTravelPoint : MonoBehaviour
{
    public enum RegionType { Pond, River, Ocean, Shop, BlackPond, Boss }

    [Header("Region Info")]
    public RegionType regionType;

    [Header("UI Components")]
    public Button travelButton;
    public Image pinImage;
    public GameObject confirmationPopup;
    public TextMeshProUGUI confirmationText;
    public Button yesButton;
    public Button noButton;
    
    [Header("Boss UI")]
    public CanvasGroup confirmationCanvasGroup; // For fading in confirmation
    public CanvasGroup blackFadeCanvasGroup; // Black screen for transitions
    public float dialogFadeDuration = 0.5f; // How fast the dialog fades in
    public float blackFadeDuration = 1.5f; // How fast the black screen fades in

    [Header("Color Settings")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    [Header("Boss Fade Elements")]
    public CanvasGroup bossTravelCanvasGroup; // Optional for Boss fade only
    public float fadeDuration = 1f;

    private void Start()
    {
        SetupButton();
        
        // Make sure black fade canvas starts invisible
        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void SetupButton()
    {
        var save = GameManager.Instance?.currentSaveData;

        // Step 1: Initially disable BlackPond and Boss completely
        if (regionType == RegionType.BlackPond || regionType == RegionType.Boss)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // Enable other region buttons and set their interactability
            gameObject.SetActive(true);
        }

        // Step 2: When rod is turned in, only show BlackPond and Shop
        if (save != null && save.hasTurnedInRod && !save.readyForFight)
        {
            if (regionType == RegionType.BlackPond)
            {
                // Enable BlackPond
                gameObject.SetActive(true);
                SetPinActive(true);
            }
            else if (regionType == RegionType.Shop)
            {
                // Enable Shop
                gameObject.SetActive(true);
                SetPinActive(true);
            }
            else if (regionType == RegionType.Pond || 
                    regionType == RegionType.River || 
                    regionType == RegionType.Ocean)
            {
                // Disable all other travel points
                gameObject.SetActive(false);
            }
        }
        
        // Step 3: When readyForFight is true, only show Boss and BlackPond (locked)
        else if (save != null && save.readyForFight)
        {
            if (regionType == RegionType.Boss)
            {
                // Enable Boss
                gameObject.SetActive(true);
                SetPinActive(true);
            }
            else if (regionType == RegionType.BlackPond)
            {
                // Show BlackPond but locked
                gameObject.SetActive(true);
                SetPinActive(false); // Locked
            }
            else
            {
                // Disable all other travel points
                gameObject.SetActive(false);
            }
        }
        
        // Initial game state - normal regions available based on progression
        else
        {
            switch (regionType)
            {
                case RegionType.Shop:
                    SetPinActive(save != null && save.arrivedAtShop);
                    break;
                case RegionType.Pond:
                    SetPinActive(save != null && save.arrivedAtShop);
                    break;
                case RegionType.River:
                    SetPinActive(save != null && save.hasTurnedInScroll);
                    break;
                case RegionType.Ocean:
                    SetPinActive(save != null && save.hasTurnedInHair);
                    break;
            }
        }

        // Add debug logging to verify state
        Debug.Log($"[MapTravelPoint] {regionType} - Active: {gameObject.activeSelf}, Interactable: {(travelButton != null ? travelButton.interactable : false)}");

        if (travelButton != null)
        {
            travelButton.onClick.RemoveAllListeners();
            travelButton.onClick.AddListener(OnTravelButtonClicked);
        }
        
        if (confirmationPopup != null)
        {
            confirmationPopup.SetActive(false);
        }
    }

    private void SetPinActive(bool unlocked)
    {
        gameObject.SetActive(true);
        if (travelButton != null)
            travelButton.interactable = unlocked;
        if (pinImage != null)
            pinImage.color = unlocked ? unlockedColor : lockedColor;
    }

    private void OnTravelButtonClicked()
    {
        if (!travelButton.interactable) return;

        string displayName = GetRegionDisplayName(regionType);
        confirmationText.text = regionType == RegionType.Boss
            ? "Are you ready to confront the terror of the Horizon Angler?"
            : $"Would you like to travel to the {displayName}?";

        // Special handling for Boss confirmation
        if (regionType == RegionType.Boss && confirmationCanvasGroup != null)
        {
            // Make confirmation popup visible but transparent
            confirmationPopup.SetActive(true);
            confirmationCanvasGroup.alpha = 0f;
            
            // Fade it in
            StartCoroutine(FadeInConfirmation());
        }
        else
        {
            // Standard behavior for other regions
            confirmationPopup.SetActive(true);
        }

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            // Don't immediately hide confirmation for Boss
            if (regionType != RegionType.Boss)
            {
                confirmationPopup.SetActive(false);
            }
            
            TravelToRegion(regionType);
        });

        noButton.onClick.AddListener(() =>
        {
            if (regionType == RegionType.Boss && confirmationCanvasGroup != null)
            {
                // For Boss, fade out confirmation
                StartCoroutine(FadeOutConfirmation());
            }
            else
            {
                // For other regions, just hide it
                confirmationPopup.SetActive(false);
            }
        });
    }

    private IEnumerator FadeInConfirmation()
    {
        float elapsed = 0f;
        
        while (elapsed < dialogFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            confirmationCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / dialogFadeDuration);
            yield return null;
        }
        
        confirmationCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutConfirmation()
    {
        float elapsed = 0f;
        
        while (elapsed < dialogFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            confirmationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / dialogFadeDuration);
            yield return null;
        }
        
        confirmationCanvasGroup.alpha = 0f;
        confirmationPopup.SetActive(false);
    }

    private void TravelToRegion(RegionType region)
    {
        Debug.Log($"Traveling to {region}...");

        Time.timeScale = 1f;

        if (region == RegionType.Boss)
        {
            // Fade to black and load boss scene
            if (blackFadeCanvasGroup != null)
            {
                blackFadeCanvasGroup.gameObject.SetActive(true);
                StartCoroutine(FadeToBlackAndLoadBoss());
            }
            else if (bossTravelCanvasGroup != null) 
            {
                // Fallback to the original fade canvas if available
                StartCoroutine(FadeAndLoadBossScene());
            }
            else
            {
                // No fade, just load
                SceneManager.LoadScene("Boss");
            }
            return;
        }

        LoadingManager loadingManager = FindObjectOfType<LoadingManager>();

        switch (region)
        {
            case RegionType.River:
                loadingManager.LoadSceneWithLoadingScreen("River");
                break;
            case RegionType.Ocean:
                loadingManager.LoadSceneWithLoadingScreen("Ocean");
                break;
            case RegionType.Shop:
                loadingManager.LoadSceneWithLoadingScreen("Shop");
                break;
            case RegionType.Pond:
                loadingManager.LoadSceneWithLoadingScreen("Pond");
                break;
            case RegionType.BlackPond:
                loadingManager.LoadSceneWithLoadingScreen("BlackPond");
                break;
            default:
                Debug.Log("Region travel not implemented.");
                break;
        }
    }

    private IEnumerator FadeToBlackAndLoadBoss()
    {
        // Keep confirmation visible during the black fade
        // Only fade out confirmation near the end of the black fade

        // Begin loading the boss scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Boss");
        asyncLoad.allowSceneActivation = false; // Don't transition yet
        
        // Fade to black while keeping confirmation visible
        float elapsed = 0f;
        while (elapsed < blackFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            blackFadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / blackFadeDuration);
            
            // Only start fading out confirmation when we're 75% through the black fade
            if (elapsed > blackFadeDuration * 0.75f && confirmationCanvasGroup != null)
            {
                // Calculate how far we are through the final 25% of the fade
                float confirmFadeProgress = (elapsed - (blackFadeDuration * 0.75f)) / (blackFadeDuration * 0.25f);
                confirmationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, confirmFadeProgress);
            }
            
            yield return null;
        }
        
        // Ensure we're fully black and confirmation is hidden
        blackFadeCanvasGroup.alpha = 1f;
        if (confirmationCanvasGroup != null)
            confirmationCanvasGroup.alpha = 0f;
        
        // Slight pause at black screen before transition
        yield return new WaitForSeconds(0.5f);
        
        // Allow scene transition
        asyncLoad.allowSceneActivation = true;
    }

    private IEnumerator FadeAndLoadBossScene()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            bossTravelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        // Load scene once fully faded
        SceneManager.LoadScene("Boss");
    }

    private string GetRegionDisplayName(RegionType region)
    {
        return region switch
        {
            RegionType.River => "River",
            RegionType.Ocean => "The Deep Blue",
            RegionType.Shop => "Shop",
            RegionType.Pond => "Pond",
            RegionType.BlackPond => "Black Pond",
            RegionType.Boss => "Final Horizon",
            _ => "Unknown Region",
        };
    }

    public void ForceUnlock()
    {
        Unlock();
    }

    private void Unlock()
    {
        travelButton.interactable = true;
        if (pinImage != null)
            pinImage.color = unlockedColor;
    }
}