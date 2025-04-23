using System;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    // Tracks how many of each fish has been caught (for quick lookup)
    public Dictionary<string, int> fishCaught = new Dictionary<string, int>();

    // 10 most recent fish caught, shown in Recent Catches log
    public List<RecentCatchEntry> recentCatchLog = new List<RecentCatchEntry>();

    // Encyclopedia entries for detailed tracking
    public Dictionary<string, FishRecord> fishEncyclopedia = new Dictionary<string, FishRecord>();

    // Boss unlock/caught tracking
    public bool canFishPondBoss = false;
    public bool canFishRiverBoss = false;
    public bool canFishOceanBoss = false;

    public bool hasCaughtPondBoss = false;
    public bool hasCaughtRiverBoss = false;
    public bool hasCaughtOceanBoss = false;
    public bool dredgedHand = false;
    public bool AllCollected = false;

    // Last known level the player was in
    public string currentLevel = "Pond";

    // Recent catch log entry (lightweight)
    [System.Serializable]
    public class RecentCatchEntry
    {
        public string fishName;
        public string date;
        public string time;

        public RecentCatchEntry(string fishName, string date, string time)
        {
            this.fishName = fishName;
            this.date = date;
            this.time = time;
        }
    }

    // Full fish encyclopedia entry
    [System.Serializable]
    public class FishRecord
    {
        public string fishName;
        public string firstCatchDate;
        public string firstCatchTime;
        public string mostRecentCatchDate;
        public string mostRecentCatchTime;
        public string zoneCaught; // Pond, River, Ocean, Boss
        public int totalCaught;
        public bool hasBeenCaught;

        public FishRecord(string name, string zone, string date, string time)
        {
            fishName = name;
            zoneCaught = zone;
            firstCatchDate = date;
            firstCatchTime = time;
            mostRecentCatchDate = date;
            mostRecentCatchTime = time;
            totalCaught = 1;
            hasBeenCaught = true;
        }

        public void UpdateCatch(string date, string time)
        {
            mostRecentCatchDate = date;
            mostRecentCatchTime = time;
            totalCaught++;
        }
    }

    public SaveData()
    {
        fishCaught = new Dictionary<string, int>();
        recentCatchLog = new List<RecentCatchEntry>();
        fishEncyclopedia = new Dictionary<string, FishRecord>();
    }
}
