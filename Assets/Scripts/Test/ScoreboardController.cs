using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun.UtilityScripts;
using UnityEngine.InputSystem;

public class ScoreboardController : MonoBehaviourPunCallbacks
{
    public static ScoreboardController Instance;

    [Header("UI References")]
    [Tooltip("The main holder GameObject for the scoreboard UI.")]
    public GameObject scoreboardHolder;

    [Tooltip("The UI panel where the scoreboard rows are displayed.")]
    public GameObject scoreboardPanel;

    [Tooltip("Prefab for a row displaying player data.")]
    public GameObject scoreboardRowPrefab;

    private PlayerInputActions inputActions;
    private bool showScoreboard = false;

    // Dictionary to keep track of instantiated scoreboard rows by actor number
    private Dictionary<int, ScoreboardItem> scoreboardRows = new Dictionary<int, ScoreboardItem>();

    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the input actions
        inputActions = new PlayerInputActions();

        // Subscribe to the Scoreboard action's performed event
        inputActions.Player.Scoreboard.performed += ToggleScoreMenu;

        // Subscribe to the PlayerNameSet event
        PlayerVariables.PlayerNameSet += UpdateScoreboard;

        UpdateScoreboard();
    }

    public override void OnEnable()
    {
        // Enable the input actions
        inputActions.Enable();
    }

    public override void OnDisable()
    {
        // Disable the input actions when not in use
        inputActions.Disable();

        // Unsubscribe from the PlayerNameSet event to prevent memory leaks
        PlayerVariables.PlayerNameSet -= UpdateScoreboard;
    }

    private void ToggleScoreMenu(InputAction.CallbackContext context)
    {
        showScoreboard = !showScoreboard;

        if (showScoreboard)
        {
            ShowScoreboard();
        }
        else
        {
            HideScoreboard();
        }
    }

    //Displays the scoreboard UI and updates its content.
    private void ShowScoreboard()
    {
        scoreboardHolder.SetActive(true); // Show the scoreboard UI
        UpdateScoreboard(); // Refresh scoreboard data
        //StartCoroutine(UpdateScoreboardWithDelay(0.5f)); // Refresh scoreboard data after delay

        // Unlock and show the cursor so the player can interact with the scoreboard
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //Hides the scoreboard UI.
    private void HideScoreboard()
    {
        scoreboardHolder.SetActive(false); // Hide the scoreboard UI

        // Lock and hide the cursor to resume gameplay controls
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    public void UpdateScoreboard()
    {
        if (scoreboardPanel == null || scoreboardRowPrefab == null)
        {
            Debug.LogError("ScoreboardPanel or ScoreboardRowPrefab is not assigned.");
            return;
        }

        // Clear existing rows to prevent duplication
        foreach (Transform child in scoreboardPanel.transform)
        {
            Destroy(child.gameObject);
        }
        scoreboardRows.Clear();

        // Loop through all players in the Photon room and create/update rows
        foreach (var player in PhotonNetwork.PlayerList)
        {
            // Find the PlayerVariables component associated with the player
            PlayerVariables playerVars = FindPlayerVariables(player.ActorNumber);
            if (playerVars != null)
            {
                // Instantiate a new scoreboard row prefab
                GameObject row = Instantiate(scoreboardRowPrefab, scoreboardPanel.transform);
                ScoreboardItem rowScript = row.GetComponent<ScoreboardItem>();

                if (rowScript != null)
                {
                    // Update the row UI with the player's stats
                    rowScript.SetPlayerInfo(playerVars.playerName, playerVars.kills, playerVars.deaths);
                    scoreboardRows[player.ActorNumber] = rowScript;
                }
                else
                {
                    Debug.LogError("ScoreboardRowPrefab does not have a ScoreboardItem component.");
                }
            }
            else
            {
                Debug.LogWarning($"PlayerVariables not found for Actor {player.ActorNumber}");
            }
        }
    }

    private PlayerVariables FindPlayerVariables(int actorNumber)
    {
        foreach (PlayerVariables vars in FindObjectsOfType<PlayerVariables>())
        {
            if (vars.photonView.Owner.ActorNumber == actorNumber)
            {
                return vars;
            }
        }
        return null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (showScoreboard)
        {
            UpdateScoreboard();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (showScoreboard)
        {
            UpdateScoreboard();
        }
    }
}
