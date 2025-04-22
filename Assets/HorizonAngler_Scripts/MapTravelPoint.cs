using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using StarterAssets;

public class MapTravelPoint : MonoBehaviour
{
    public enum RegionType { Tutorial, Pond, River, Ocean, Shop }

    [Header("Region Info")]
    public RegionType regionType;

    [Header("UI Components")]
    public Button travelButton;             // The pin's Button
    public Image pinImage;                  // The pin's image to tint
    public GameObject confirmationPopup;    // The confirmation panel
    public TextMeshProUGUI confirmationText;
    public Button yesButton;
    public Button noButton;

    [Header("Color Settings")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    private HAPlayerController player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<HAPlayerController>();

        SetupButton();
    }

    private void SetupButton()
    {
        if (regionType == RegionType.River)
        {
            if (player != null && player.hasTurnedInScroll)
                Unlock();
            else
                Lock();
        }
        else if (regionType == RegionType.Shop)
        {
            if (player != null && player.hasMovedShop)
                Unlock();
            else
                Lock();
        }
        else if (regionType == RegionType.Pond)
        {
            Unlock(); // Always available
        }
        else if (regionType == RegionType.Ocean)
        {
            // Example: unlock if player caught river boss (you can customize this)
            if (GameManager.Instance != null && GameManager.Instance.currentSaveData.hasCaughtRiverBoss)
                Unlock();
            else
                Lock();
        }
        else
        {
            Lock();
        }

        travelButton.onClick.AddListener(OnTravelButtonClicked);
        confirmationPopup.SetActive(false);
    }

    private void Lock()
    {
        travelButton.interactable = false;
        if (pinImage != null)
            pinImage.color = lockedColor;
    }

    private void Unlock()
    {
        travelButton.interactable = true;
        if (pinImage != null)
            pinImage.color = unlockedColor;
    }

    private void OnTravelButtonClicked()
    {
        if (!travelButton.interactable) return;

        confirmationPopup.SetActive(true);
        string displayName = GetRegionDisplayName(regionType);
        confirmationText.text = $"Would you like to travel to the {displayName}?";

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

        Time.timeScale = 1f; // Unpause the game

        // Get the LoadingManager instance
        LoadingManager loadingManager = FindObjectOfType<LoadingManager>();

        // Call the loading screen method
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
                break;        default:
                Debug.Log("Region travel not implemented.");
                break;
        }
    }


    private string GetRegionDisplayName(RegionType region)
    {
        switch (region)
        {
            case RegionType.River: return "River";
            case RegionType.Ocean: return "The Deep Blue";
            case RegionType.Shop: return "Shop";
            case RegionType.Pond: return "Pond";
            case RegionType.Tutorial: return "Tutorial Region";
            default: return "Unknown Region";
        }
    }

    public void ForceUnlock()
    {
        Unlock();
    }
}
