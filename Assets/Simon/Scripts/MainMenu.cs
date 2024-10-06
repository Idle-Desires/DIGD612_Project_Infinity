using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour

{
    public void StartGanme()
    {
        SceneManager.LoadScene("character");
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