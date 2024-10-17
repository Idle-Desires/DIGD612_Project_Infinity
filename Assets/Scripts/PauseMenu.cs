using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public string sceneLoad;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new PlayerInputActions();

        // Subscribe to the PauseMenu action
        inputActions.Player.PauseMenu.performed += TogglePauseMenu;
    }

    private void OnEnable()
    {
        // Enable the input actions
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // Disable the input actions when not in use
        inputActions.Disable();
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        // Toggle between pause and resume when the action is performed
        if (GameIsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu UI
        //Time.timeScale = 1f; // Resume game time
        GameIsPaused = false;

        // Optionally lock and hide the cursor again after resuming
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true); // Show the pause menu UI
        //Time.timeScale = 0f; // Freeze the game
        GameIsPaused = true;

        // Unlock and show the cursor so the player can interact with the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Debug.Log("Load Menu");
        // You can add code to load the menu scene here
        // SceneManager.LoadScene(sceneLoad);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit(); // Quit the application
    }
}
