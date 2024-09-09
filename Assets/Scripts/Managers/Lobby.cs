using UnityEngine;
using UnityEngine.SceneManagement; // For loading scenes
using UnityEngine.UI; // For UI Button

public class Lobby : MonoBehaviour
{
    //Name of the scene you want to load
    public string sceneToLoad;

    //Call this function when the button is clicked to quit the game
    public void Exit()
    {
        //This will only work when the game is built, it won't exit the editor
        Application.Quit();
    }

    //Call this function to load a different scene
    public void ChangeScene()
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
}
