using UnityEngine;
using UnityEngine.UI;

public class ShopPinManager : MonoBehaviour
{
    public enum RegionType { Pond, River, Ocean, Shop, BlackPond, Boss }

    [Header("Region Info")]
    public RegionType regionType;

    [Header("UI Components")]
    public Button travelButton;
    public Image pinImage;

    private void Start()
    {
        SetupPin();
    }

    private void OnEnable()
    {
        // Refresh pin state whenever this becomes active (e.g., returning from dialogue)
        if (GameManager.Instance != null)
        {
            SetupPin();
        }
    }

    public void SetupPin()
    {
        var save = GameManager.Instance?.currentSaveData;

        if (save != null)
        {
            // Special handling for BlackPond visibility through Image component
            if (regionType == RegionType.BlackPond)
            {
                if (!save.hasTurnedInRod)
                {
                    // Completely hide BlackPond until rod is turned in
                    if (pinImage != null)
                        pinImage.enabled = false;
                    if (travelButton != null)
                        travelButton.interactable = false;
                    return;
                }
                else if (save.readyForFight)
                {
                    // Show as decorative when readyForFight
                    if (pinImage != null)
                        pinImage.enabled = true;
                    SetPinDecorative();
                    return;
                }
                else
                {
                    // Show as normal interactive pin after rod turned in but before readyForFight
                    if (pinImage != null)
                        pinImage.enabled = true;
                    SetPinActive(true);
                    return;
                }
            }

            if (save.readyForFight)
            {
                // During readyForFight: Only Boss is functional, others hidden
                if (regionType == RegionType.Boss)
                {
                    gameObject.SetActive(true);
                    SetPinActive(true);
                }
                else // Shop, Pond, River, Ocean
                {
                    gameObject.SetActive(false);
                }
            }
            else if (save.hasTurnedInRod)
            {
                if (regionType == RegionType.Shop)
                {
                    SetPinActive(true);
                }
                else
                {
                    SetPinActive(false); // Hide other regions
                }
            }
            else
            {
                // Default state based on region type
                if (regionType == RegionType.Boss)
                {
                    SetPinActive(false); // Keep inactive but don't disable GameObject
                }
                else
                {
                    switch (regionType)
                    {
                        case RegionType.Shop:
                        case RegionType.Pond:
                            SetPinActive(save.arrivedAtShop);
                            break;
                        case RegionType.River:
                            SetPinActive(save.hasTurnedInScroll);
                            break;
                        case RegionType.Ocean:
                            SetPinActive(save.hasTurnedInHair);
                            break;
                        default:
                            SetPinActive(false);
                            break;
                    }
                }
            }
        }

        Debug.Log($"[MapTravelPoint] {regionType} - Active: {gameObject.activeSelf}, Interactable: {travelButton?.interactable}");
    }

    private void SetPinActive(bool unlocked)
    {
        if (travelButton != null)
            travelButton.interactable = unlocked;
        if (pinImage != null)
            pinImage.color = unlocked ? Color.white : Color.gray;
    }

    private void SetPinDecorative()
    {
        // Make pin visible with white color but completely non-functional
        if (travelButton != null)
        {
            travelButton.interactable = false;
            // Remove all onClick listeners so button does nothing when clicked
            travelButton.onClick.RemoveAllListeners();
        }
        if (pinImage != null)
        {
            pinImage.enabled = true; // Ensure image is visible
            pinImage.color = Color.white; // Keep it looking active
        }
    }

    public void ForceUnlock()
    {
        if (travelButton != null)
            travelButton.interactable = true;
        if (pinImage != null)
            pinImage.color = Color.white;
    }
}
