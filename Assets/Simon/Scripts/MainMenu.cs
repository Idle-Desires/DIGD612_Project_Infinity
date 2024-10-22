using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //Name of the scene you want to load
    public string sceneToLoad;
    public string characterSelect = "none";
    public string sceneSelect = "none";
    public GameObject error;

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

    public void CharacterSelection(string character)
    {
        characterSelect = character;
    }

    public void SceneSelection(string scene)
    {
        sceneSelect = scene;
        sceneToLoad = sceneSelect;
    }

    public void LockIn()
    {
        if (characterSelect != "none" && sceneSelect != "none")
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            error.SetActive(true);
        }
    }

    public void Options()
    {
        
    }
}