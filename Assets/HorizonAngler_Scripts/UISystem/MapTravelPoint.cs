using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MapTravelPoint : MonoBehaviour
{
    public enum RegionType {Pond, River, Ocean, Shop, BlackPond, Boss }

    [Header("Region Info")]
    public RegionType regionType;

    [Header("UI Components")]
    public Button travelButton;
    public Image pinImage;
    public GameObject confirmationPopup;
    public TextMeshProUGUI confirmationText;
    public Button yesButton;
    public Button noButton;

    [Header("Color Settings")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    [Header("Boss Fade Elements")]
    public CanvasGroup bossTravelCanvasGroup; // Optional for Boss fade only
    public float fadeDuration = 1f;

    private void Start()
    {
        SetupButton();
    }

    public void SetupButton()
    {
        var save = GameManager.Instance?.currentSaveData;

        if (save != null && save.hasTurnedInRod)
        {
            if (regionType == RegionType.BlackPond)
            {
                gameObject.SetActive(true);
                SetPinActive(true);
            }
            else if (regionType == RegionType.Pond)
            {
                gameObject.SetActive(false);
            }
            else if (regionType == RegionType.Shop)
            {
                gameObject.SetActive(true);
                SetPinActive(save.arrivedAtShop);
            }
            else
            {
                SetPinActive(false);
            }
        }
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
                case RegionType.BlackPond:
                    gameObject.SetActive(false); // Hide until Rod turned in
                    break;
                case RegionType.River:
                    SetPinActive(save != null && save.hasTurnedInScroll);
                    break;
                case RegionType.Ocean:
                    SetPinActive(save != null && save.hasTurnedInHair);
                    break;
                case RegionType.Boss:
                    gameObject.SetActive(save.AllCollected);
                    SetPinActive(save.AllCollected);
                    break;
            }
        }

        // Make sure button click listener is always added, regardless of conditions above
        travelButton.onClick.RemoveAllListeners(); // avoid duplicates
        travelButton.onClick.AddListener(OnTravelButtonClicked);
        confirmationPopup.SetActive(false);

        Debug.Log($"[MapTravelPoint] SetupButton called for: {regionType}");
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

        confirmationPopup.SetActive(true);
        string displayName = GetRegionDisplayName(regionType);
        confirmationText.text = regionType == RegionType.Boss
            ? "Are you ready to confront the terror of the Horizon Angler?"
            : $"Would you like to travel to the {displayName}?";

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();

        yesButton.onClick.AddListener(() =>
        {
            confirmationPopup.SetActive(false);
            TravelToRegion(regionType);
        });

        noButton.onClick.AddListener(() =>
        {
            confirmationPopup.SetActive(false);
        });
    }

    private void TravelToRegion(RegionType region)
    {
        Debug.Log($"Traveling to {region}...");

        Time.timeScale = 1f;

        if (region == RegionType.Boss)
        {
            if (bossTravelCanvasGroup != null)
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
