using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        // SHIFT + T + X + O + U deletes the save file and reloads a fresh save
        if (Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.T) &&
            Input.GetKey(KeyCode.X) &&
            Input.GetKey(KeyCode.O) &&
            Input.GetKey(KeyCode.U))
        {
            Debug.LogWarning("[GameManager] SECRET COMBO ACTIVATED: Deleting save file...");
            SaveManager.DeleteSave();

            // Refresh all save-dependent data
            currentSaveData = SaveManager.Load();

            // Optional: Notify other systems to update their cached values
            FishingProgress.Instance?.Initialize(); // Resets progress bar, etc.
            Debug.Log("[GameManager] Save data reloaded and variables refreshed.");
        }
    }


    void OnApplicationQuit()
    {
        SaveManager.Save(currentSaveData);
        Debug.Log("Game saved on quit.");
    }

    public void RecordFishCatch(string fishName, string zone)
    {
        string date = System.DateTime.Now.ToString("MM/dd/yyyy");
        string time = System.DateTime.Now.ToString("HH:mm:ss");

        // Update total per-fish count
        if (currentSaveData.fishCaught.ContainsKey(fishName))
            currentSaveData.fishCaught[fishName]++;
        else
            currentSaveData.fishCaught[fishName] = 1;

        // Update recent catch log
        SaveData.RecentCatchEntry entry = new SaveData.RecentCatchEntry(fishName, date, time);
        currentSaveData.recentCatchLog.Insert(0, entry);
        if (currentSaveData.recentCatchLog.Count > 10)
            currentSaveData.recentCatchLog.RemoveAt(10);

        // Update encyclopedia entry
        if (currentSaveData.fishEncyclopedia.ContainsKey(fishName))
        {
            currentSaveData.fishEncyclopedia[fishName].UpdateCatch(date, time);
        }
        else
        {
            currentSaveData.fishEncyclopedia[fishName] = new SaveData.FishRecord(fishName, zone, date, time);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentSaveData != null)
        {
            string sceneName = scene.name;

            // Ignore saving scene name if it's a non-gameplay scene
            if (sceneName != "Title Screen" && sceneName != "Tutorial" && sceneName != "PostTutorial")
            {
                currentSaveData.lastScene = sceneName;
                SaveManager.Save(currentSaveData);
                Debug.Log($"[GameManager] Scene changed to: {sceneName} (saved)");
            }
            else
            {
                Debug.Log($"[GameManager] Scene '{sceneName}' is excluded from save tracking.");
            }
        }
    }


    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartNewGame()
    {
        Debug.Log("[GameManager] New Game started — deleting old save and loading fresh data.");
        
        // Delete the save file
        SaveManager.DeleteSave();

        // Load fresh save data
        currentSaveData = SaveManager.Load();

        // Optionally reset other systems (e.g. progress, UI)
        FishingProgress.Instance?.Initialize();
    }
}
