using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ScoreboardManager : MonoBehaviour
{
    public GameObject scoreboardPanel; // Reference to the scoreboard panel
    public GameObject playerRowPrefab; // Prefab for each player row
    //public Transform playerListContainer; // Container for player rows

    private PlayerInputActions inputActions;
    private bool isScoreboardVisible = false;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Scoreboard.performed += OnShowScoreboard;
        inputActions.Player.Scoreboard.canceled += OnHideScoreboard;
    }

    private void OnEnable() { inputActions.Enable(); }
    private void OnDisable() { inputActions.Disable(); }

    private void OnShowScoreboard(InputAction.CallbackContext context) { ShowScoreboard(); }
    private void OnHideScoreboard(InputAction.CallbackContext context) { HideScoreboard(); }

    void ShowScoreboard()
    {
        isScoreboardVisible = true;
        scoreboardPanel.SetActive(true);
        //UpdateScoreboard();
    }

    void HideScoreboard()
    {
        isScoreboardVisible = false;
        scoreboardPanel.SetActive(false);
    }

    private void ToggleScoreboard()
    {
        isScoreboardVisible = !isScoreboardVisible; // Toggle visibility state
        scoreboardPanel.SetActive(isScoreboardVisible); // Show or hide the scoreboard canvas
        playerRowPrefab.SetActive(isScoreboardVisible);

        if (isScoreboardVisible)
        {
            // Update the scoreboard when it's shown
            //GameManager.instance.UpdateScoreboard();
        }
    }

    //void UpdateScoreboard()
    //{
    //    // Clear any existing player rows
    //    foreach (Transform child in playerListContainer)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    // Loop through all players in the room
    //    foreach (Player player in PhotonNetwork.PlayerList)
    //    {
    //        // Instantiate a new player row
    //        GameObject playerRow = Instantiate(playerRowPrefab, playerListContainer);

    //        // Get the TextMeshProUGUI components from the player row
    //        TextMeshProUGUI[] textFields = playerRow.GetComponentsInChildren<TextMeshProUGUI>();
    //        textFields[0].text = player.NickName; // Player name

    //        // Get the player's stats from GameManager
    //        int playerID = player.ActorNumber;
    //        GameManager.PlayerStats stats = GameManager.instance.GetPlayerStats(playerID);

    //        // Update text fields with player stats
    //        textFields[0].text = player.NickName; // Player name
    //        textFields[1].text = stats.kills.ToString(); // Display kills
    //        textFields[2].text = stats.deaths.ToString(); // Display deaths

    //        //if (stats != null)
    //        //{
    //        //    textFields[1].text = stats.kills.ToString(); // Display kills
    //        //    textFields[2].text = stats.deaths.ToString(); // Display deaths
    //        //}
    //        //else
    //        //{
    //        //    textFields[1].text = "0"; // Default value if no stats found
    //        //    textFields[2].text = "0"; // Default value if no stats found
    //        //}
    //    }
    //}
}
