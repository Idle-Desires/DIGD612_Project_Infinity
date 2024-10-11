using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine.InputSystem;

public class Leaderboard : MonoBehaviour
{
    public GameObject scoreboardHolder;
    private PlayerInputActions inputActions;

    public static bool showScoreboard = false;

    [Header("Options")]
    public float refreshRate = 1f;

    [Header("UI")]
    public GameObject[] slots;

    [Space]
    public TextMeshProUGUI[] scoreText;
    public TextMeshProUGUI[] nameText;

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new PlayerInputActions();

        // Subscribe to the PauseMenu action
        inputActions.Player.Scoreboard.performed += ToggleScoreMenu;
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

    private void ToggleScoreMenu(InputAction.CallbackContext context)
    {
        // Toggle between pause and resume when the action is performed
        if (showScoreboard)
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
        scoreboardHolder.SetActive(false); // Hide the pause menu UI
        //Time.timeScale = 1f; // Resume game time
        showScoreboard = false;

        // Optionally lock and hide the cursor again after resuming
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        scoreboardHolder.SetActive(true); // Show the pause menu UI
        //Time.timeScale = 0f; // Freeze the game
        showScoreboard = true;

        // Unlock and show the cursor so the player can interact with the menu
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }

    private void Start()
    {
        InvokeRepeating(nameof(RefreshRate), 1f, refreshRate);
    }

    public void Refresh()
    {
        foreach (var slot in slots) 
        { 
            slot.SetActive(false);
        }

        var sortedPlayerList = (from player in PhotonNetwork.PlayerList orderby player.GetScore() descending select player).ToList();

        int i = 0;
        foreach (var player in sortedPlayerList)
        {
            slots[i].SetActive(true);

            if (player.NickName == "")
            {
                player.NickName = "Unnamed";
            }

            nameText[i].text = player.NickName;
            scoreText[i].text = player.GetScore().ToString();

            i++;
        }
    }

    //private void Update()
    //{
    //    scoreboardHolder.SetActive(Input.GetKeyUp(KeyCode.Tab));
    //}
}
