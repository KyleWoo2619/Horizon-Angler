using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class FishEncyclopediaUI : MonoBehaviour
{
    [System.Serializable]
    public class EncyclopediaPage
    {
        public TextMeshProUGUI fishName;
        public Image fishImage;
        public TextMeshProUGUI zone;
        public TextMeshProUGUI count;
        public TextMeshProUGUI firstCaught;
        public TextMeshProUGUI lastCaught;
        public TextMeshProUGUI description;
        public GameObject unknownOverlay;
        public TextMeshProUGUI pageNumberText;
        public GameObject alertIcon; // Alert icon for new fish
    }

    public List<EncyclopediaPage> pages; // Should contain exactly 2 (left/right)
    public Button nextButton;
    public Button prevButton;
    public GameObject encyclopediaAlertIcon;
    public GameObject nextButtonAlertIcon;

    private HashSet<string> newlyDiscoveredFish = new HashSet<string>();

    private List<string> fishOrder = new List<string>(); // Ordered list of all 16 fish names
    private int currentPageIndex = 0; // Even index (0, 2, 4...)

    void Start()
    {
        InitializeFishOrder();
        UpdatePageUI();
    }

    void InitializeFishOrder()
    {
        fishOrder = new List<string>
        {
            "Blue Gill Fish", "Koifish", "Largemouth Bass", "Mullet",
            "Trout", "Pike", "Perch", "River Eel",
            "Lionfish", "Mackerel", "Swordfish", "Manta Ray",
            "Sun-Infused Croakmaw", "River Boss", "Ocean Boss", "Final Boss"
        };
    }

    public void FlipPage(int direction)
    {
        currentPageIndex += direction * 2;
        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, fishOrder.Count - 2);
        UpdatePageUI();
    }

    public void UpdatePageUI()
    {
        bool hasNewFishOnPage = false;

        for (int i = 0; i < 2; i++)
        {
            int fishIndex = currentPageIndex + i;
            if (fishIndex < fishOrder.Count)
            {
                string fishName = fishOrder[fishIndex];

                // Before removing, pass whether it should still show the alert on this page
                bool isNew = newlyDiscoveredFish.Contains(fishName);
                UpdateSinglePage(pages[i], fishName, isNew);

                if (isNew)
                {
                    hasNewFishOnPage = true;
                    newlyDiscoveredFish.Remove(fishName);
                }
            }
            else
            {
                ClearPage(pages[i]);
            }

            pages[i].pageNumberText.text = (fishIndex < fishOrder.Count) ? (fishIndex + 1).ToString() : "";
        }

        prevButton.interactable = currentPageIndex > 0;
        nextButton.interactable = currentPageIndex < fishOrder.Count - 2;

        // Only enable next button alert if there are unseen fish not on current page
        nextButtonAlertIcon?.SetActive(newlyDiscoveredFish.Count > 0);
        encyclopediaAlertIcon?.SetActive(hasNewFishOnPage || newlyDiscoveredFish.Count > 0);
    }

    void UpdateSinglePage(EncyclopediaPage page, string fishName, bool showAlert)
    {
        var data = GameManager.Instance.currentSaveData;

        if (data.fishEncyclopedia.ContainsKey(fishName))
        {
            var record = data.fishEncyclopedia[fishName];
            page.fishName.text = record.fishName;
            page.fishImage.sprite = FishSpriteLookup(record.fishName);
            page.zone.text = record.zoneCaught;
            page.count.text = $"{record.totalCaught} caught";
            page.firstCaught.text = record.firstCatchDate + " " + record.firstCatchTime;
            page.lastCaught.text = record.mostRecentCatchDate + " " + record.mostRecentCatchTime;
            page.description.text = GetFishDescription(fishName);
            page.unknownOverlay.SetActive(false);
            page.alertIcon?.SetActive(newlyDiscoveredFish.Contains(fishName));
        }
        else
        {
            page.fishName.text = "???";
            page.fishImage.sprite = unknownFishSprite;
            page.zone.text = "???";
            page.count.text = "???";
            page.firstCaught.text = "???";
            page.lastCaught.text = "???";
            page.description.text = "You haven’t caught this fish yet.";
            page.unknownOverlay.SetActive(true);
            page.alertIcon?.SetActive(showAlert);
        }
    }

    void ClearPage(EncyclopediaPage page)
    {
        page.fishName.text = "";
        page.fishImage.sprite = null;
        page.zone.text = "";
        page.count.text = "";
        page.firstCaught.text = "";
        page.lastCaught.text = "";
        page.description.text = "";
        page.unknownOverlay.SetActive(true);
        page.alertIcon?.SetActive(false);
    }

    public void NotifyFishDiscovered(string fishName)
    {
        if (!newlyDiscoveredFish.Contains(fishName))
            newlyDiscoveredFish.Add(fishName);

        // Make sure alert icons are active right away
        encyclopediaAlertIcon?.SetActive(true);
        nextButtonAlertIcon?.SetActive(true);
    }


    public void ResetToFirstPage()
    {
        currentPageIndex = 0;
        UpdatePageUI();
    }

    Sprite FishSpriteLookup(string fishName)
    {
        var fishingProgress = FishingProgress.Instance;
        if (fishingProgress == null) return null;

        List<FishingProgress.Fish>[] allFishPools = new List<FishingProgress.Fish>[] {
            fishingProgress.pondFishPool,
            fishingProgress.riverFishPool,
            fishingProgress.oceanFishPool,
            fishingProgress.bossPondFishPool,
            fishingProgress.bossRiverFishPool,
            fishingProgress.bossOceanFishPool
        };

        foreach (var pool in allFishPools)
        {
            foreach (var fish in pool)
            {
                if (fish.fishName == fishName)
                    return fish.fishSprite;
            }
        }

        Debug.LogWarning($"Fish sprite not found for: {fishName}");
        return null;
    }

    string GetFishDescription(string fishName)
    {
        var fishingProgress = FishingProgress.Instance;
        if (fishingProgress == null) return "Description not available.";

        List<FishingProgress.Fish>[] allFishPools = new List<FishingProgress.Fish>[] {
            fishingProgress.pondFishPool,
            fishingProgress.riverFishPool,
            fishingProgress.oceanFishPool,
            fishingProgress.bossPondFishPool,
            fishingProgress.bossRiverFishPool,
            fishingProgress.bossOceanFishPool
        };

        foreach (var pool in allFishPools)
        {
            foreach (var fish in pool)
            {
                if (fish.fishName == fishName)
                    return fish.description;
            }
        }

        return "Description not available.";
    }

    public Sprite unknownFishSprite; // Assign in inspector
}