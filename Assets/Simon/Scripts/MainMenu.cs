using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Name of the scene you want to load
    public string sceneToLoad;

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene to load is not set!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        
    }

    public void LockIn()
    {
        SceneManager.LoadScene("Arena");
    }

    public void Options()
    {
        
    }
}