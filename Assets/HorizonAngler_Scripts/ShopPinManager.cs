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

    public void SetupPin()
    {
        var save = GameManager.Instance?.currentSaveData;

        // Hide Boss and BlackPond by default
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
                    SetPinActive(false); // Show but locked
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

        Debug.Log($"[MapTravelPoint] {regionType} - Active: {gameObject.activeSelf}, Interactable: {travelButton?.interactable}");
    }

    private void SetPinActive(bool unlocked)
    {
        gameObject.SetActive(true);
        if (travelButton != null)
            travelButton.interactable = unlocked;
        if (pinImage != null)
            pinImage.color = unlocked ? Color.white : Color.gray;
    }

    public void ForceUnlock()
    {
        if (travelButton != null)
            travelButton.interactable = true;
        if (pinImage != null)
            pinImage.color = Color.white;
    }
}
