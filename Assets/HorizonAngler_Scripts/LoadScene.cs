using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // put scenes in same hierarchy & put the name of the scene to load
    public void LoadingScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
