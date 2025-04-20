using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public SaveData currentSaveData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentSaveData = SaveManager.Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnApplicationQuit()
    {
        SaveManager.Save(currentSaveData);
        Debug.Log("Game saved on quit.");
    }

    public void RecordFishCatch(string fishName)
    {
        string caughtDate = System.DateTime.Now.ToString("MM/dd/yyyy");
        string caughtTime = System.DateTime.Now.ToString("HH:mm:ss");

        // Update total per-fish caught count
        if (currentSaveData.fishCaught.ContainsKey(fishName))
            currentSaveData.fishCaught[fishName]++;
        else
            currentSaveData.fishCaught[fishName] = 1;

        // Update recent catch log
        SaveData.RecentCatchEntry entry = new SaveData.RecentCatchEntry(fishName, caughtDate, caughtTime);
        currentSaveData.recentCatchLog.Insert(0, entry);
        if (currentSaveData.recentCatchLog.Count > 10)
            currentSaveData.recentCatchLog.RemoveAt(10);
    }

    // WE ALSO NEED TO SAVE THE LEVEL (SCENE) THE PLAYER IS IN HERE SOMEWHERE SOMEHOW
}
