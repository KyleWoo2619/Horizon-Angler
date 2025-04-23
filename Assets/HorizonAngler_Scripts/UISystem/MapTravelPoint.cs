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

    private void SetupButton()
    {
        var save = GameManager.Instance?.currentSaveData;

        if (regionType == RegionType.Shop)
        {
            SetPinActive(save != null && save.arrivedAtShop);
        }
        else if (regionType == RegionType.Pond)
        {
            if (save != null && save.hasTurnedInRod)
            {
                gameObject.SetActive(false);
            }
            else
            {
                SetPinActive(save != null && save.arrivedAtShop);
            }
        }
        else if (regionType == RegionType.BlackPond)
        {
            gameObject.SetActive(true); // Always visible after unlocking
            SetPinActive(save != null && save.hasTurnedInRod);
        }
        else if (regionType == RegionType.River)
        {
            SetPinActive(save != null && save.hasTurnedInScroll);
        }
        else if (regionType == RegionType.Ocean)
        {
            SetPinActive(save != null && save.hasTurnedInHair);
        }
        else if (regionType == RegionType.Boss)
        {
            if (save != null && save.AllCollected)
            {
                SetPinActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        travelButton.onClick.AddListener(OnTravelButtonClicked);
        confirmationPopup.SetActive(false);
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
