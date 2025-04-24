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
    public CanvasGroup confirmationCanvasGroup;
    public CanvasGroup blackFadeCanvasGroup;
    public float dialogFadeDuration = 0.5f;
    public float blackFadeDuration = 1.5f;

    [Header("Color Settings")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    [Header("Boss Fade Elements")]
    public CanvasGroup bossTravelCanvasGroup;
    public float fadeDuration = 1f;

    private void Start()
    {
        SetupButton();
        if (blackFadeCanvasGroup != null)
        {
            blackFadeCanvasGroup.alpha = 0f;
            blackFadeCanvasGroup.gameObject.SetActive(false);
        }
    }

    public void SetupButton()
    {
        var save = GameManager.Instance?.currentSaveData;

        // Disable Boss and BlackPond by default
        if (regionType == RegionType.Boss || regionType == RegionType.BlackPond)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }

        if (save != null)
        {
            if (save.readyForFight)
            {
                if (regionType == RegionType.Boss)
                {
                    gameObject.SetActive(true);
                    SetPinActive(true);
                }
                else if (regionType == RegionType.BlackPond)
                {
                    gameObject.SetActive(true);
                    SetPinActive(false);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
            else if (save.hasTurnedInRod)
            {
                if (regionType == RegionType.BlackPond)
                {
                    gameObject.SetActive(true);
                    SetPinActive(true);
                }
                else if (regionType == RegionType.Shop)
                {
                    gameObject.SetActive(true);
                    SetPinActive(true);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                switch (regionType)
                {
                    case RegionType.Shop:
                        SetPinActive(save.arrivedAtShop);
                        break;
                    case RegionType.Pond:
                        SetPinActive(save.arrivedAtShop);
                        break;
                    case RegionType.River:
                        SetPinActive(save.hasTurnedInScroll);
                        break;
                    case RegionType.Ocean:
                        SetPinActive(save.hasTurnedInHair);
                        break;
                }
            }
        }

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

        if (regionType == RegionType.Boss && confirmationCanvasGroup != null)
        {
            confirmationPopup.SetActive(true);
            confirmationCanvasGroup.alpha = 0f;
            StartCoroutine(FadeInConfirmation());
        }
        else
        {
            confirmationPopup.SetActive(true);
        }

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
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
                StartCoroutine(FadeOutConfirmation());
            }
            else
            {
                confirmationPopup.SetActive(false);
            }
        });
    }

    private IEnumerator FadeInConfirmation()
    {
        float elapsed = 0f;
        while (elapsed < dialogFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
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
            if (blackFadeCanvasGroup != null)
            {
                blackFadeCanvasGroup.gameObject.SetActive(true);
                StartCoroutine(FadeToBlackAndLoadBoss());
            }
            else if (bossTravelCanvasGroup != null)
            {
                StartCoroutine(FadeAndLoadBossScene());
            }
            else
            {
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
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Boss");
        asyncLoad.allowSceneActivation = false;

        float elapsed = 0f;
        while (elapsed < blackFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            blackFadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / blackFadeDuration);
            if (elapsed > blackFadeDuration * 0.75f && confirmationCanvasGroup != null)
            {
                float confirmFadeProgress = (elapsed - (blackFadeDuration * 0.75f)) / (blackFadeDuration * 0.25f);
                confirmationCanvasGroup.alpha = Mathf.Lerp(1f, 0f, confirmFadeProgress);
            }
            yield return null;
        }

        blackFadeCanvasGroup.alpha = 1f;
        if (confirmationCanvasGroup != null)
            confirmationCanvasGroup.alpha = 0f;

        yield return new WaitForSeconds(0.5f);
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