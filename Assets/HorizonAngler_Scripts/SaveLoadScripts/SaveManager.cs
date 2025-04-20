using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "savefile.json");

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game saved at " + SavePath);
    }

    public static SaveData Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log("Game Loaded.");
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to load save data. Creating new data. Error: {ex.Message}");
                return new SaveData();
            }
        }
        else
        {
            Debug.LogWarning("Save file not found, creating new data.");
            return new SaveData();
        }
    }


    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }
}
