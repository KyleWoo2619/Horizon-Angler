using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public Dictionary<string, int> fishCaught = new Dictionary<string, int>();
    public List<RecentCatchEntry> recentCatchLog = new List<RecentCatchEntry>();

    public bool canFishPondBoss = false;
    public bool canFishRiverBoss = false;
    public bool canFishOceanBoss = false;

    public bool hasCaughtPondBoss = false;
    public bool hasCaughtRiverBoss = false;
    public bool hasCaughtOceanBoss = false;

    public string currentLevel = "Pond"; // Or whatever your level naming convention is

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

    public SaveData()
    {
        fishCaught = new Dictionary<string, int>();
        recentCatchLog = new List<RecentCatchEntry>();
    }
}
