using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviour
{
    // Prefab for scoreboard rows to display player stats
    public GameObject scoreboardRowPrefab;
    // Container to hold the scoreboard rows in the UI
    public Transform scoreboardContainer;

    // Class to hold individual player stats including health, kills, and deaths
    public class PlayerStats
    {
        public float health = 50f; // Default health for players
        public int kills = 0; // Number of kills
        public int deaths = 0; // Number of deaths
        public Transform respawnPoint; // Location where the player respawns
    }

    // Dictionary to store player stats for each player by their ID
    private Dictionary<int, PlayerStats> playerStats = new Dictionary<int, PlayerStats>();
    // Singleton instance for global access to the GameManager
    public static GameManager instance;

    public int death;
    public int kill;

    private void Awake()
    {
        // Check if an instance already exists
        if (instance == null)
        {
            instance = this; // Set this instance as the singleton
            DontDestroyOnLoad(gameObject); // Persist this object between scenes
        }
        else
        {
            Destroy(gameObject); // Destroy this object if another instance exists
        }
    }

    // Method to add a new player and initialize their stats
    public void AddPlayer(int playerID, Transform respawnPoint)
    {
        if (!playerStats.ContainsKey(playerID))
        {
            // Add new player stats to the dictionary
            playerStats[playerID] = new PlayerStats { respawnPoint = respawnPoint };
            Debug.Log("Player " + playerID);
        }
    }

    public void SetPlayerStats(int playerID, int kills, int deaths)
    {
        Photon.Realtime.Player player = GetPhotonPlayerByID(playerID);
        if (player != null)
        {
            Hashtable props = new Hashtable
        {
            { "kills", kills },
            { "deaths", deaths }
        };
            player.SetCustomProperties(props);  // Sync properties
        }
    }

    // Method to record a kill for a player
    public void RecordKill(int playerID)
    {
        if (playerStats.ContainsKey(playerID))
    {
        playerStats[playerID].kills++;
        kill++;

        SetPlayerStats(playerID, playerStats[playerID].kills, playerStats[playerID].deaths);  // Sync
        //RefreshPlayerRow(playerID);  // Update UI
    }
    }

    // Method to record a death for a player and handle respawning
    public void RecordDeath(int playerID)
    {
        if (playerStats.ContainsKey(playerID))
        {
            playerStats[playerID].deaths++; // Increment the player's death count

            // Update Photon custom properties for deaths
            //Photon.Realtime.Player player = GetPhotonPlayerByID(playerID);

            //if (player != null)
            //{
            //    ExitGames.Client.Photon.Hashtable props = player.CustomProperties;
            //    props["deaths"] = playerStats[playerID].deaths; // Sync deaths property
            //    player.SetCustomProperties(props);
            //}

            //UpdateScoreboard(); // Update the scoreboard to reflect the changes
            SetPhotonCustomProperties(playerID);
            Debug.Log("Player " + playerID + ". Deaths " + playerStats[playerID].deaths);

            death++;

            // Call respawn method or any additional logic as needed
            StartCoroutine(RespawnAfterDelay(playerID, 3f)); // Example respawn delay
        }
    }

    // Coroutine to wait before respawning a player
    private IEnumerator RespawnAfterDelay(int playerID, float delay)
    {
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        RespawnPlayer(playerID); // Call the respawn method
    }

    // Method to deal damage to a player
    public void TakeDamage(int playerID, float damageAmount)
    {
        if (playerStats.ContainsKey(playerID))
        {
            playerStats[playerID].health -= damageAmount; // Decrease player's health by the damage amount
            if (playerStats[playerID].health <= 0)
            {
                RecordDeath(playerID); // Record death if health reaches 0
            }
        }
    }

    // Method to heal a player
    public void HealPlayer(int playerID, float healAmount)
    {
        if (playerStats.ContainsKey(playerID))
        {
            playerStats[playerID].health += healAmount;

            //Test that method was triggered and variable set
            Debug.Log($"Player {playerID} healed by {healAmount}, health: {playerStats[playerID].health}");

            if (playerStats[playerID].health > 50f) // Assuming max health is 50
            {
                playerStats[playerID].health = 50f;
            }
        }
    }

    // Method to respawn a player at their respawn point
    public void RespawnPlayer(int playerID)
    {
        if (playerStats.TryGetValue(playerID, out PlayerStats stats))
        {
            GameObject player = GetPlayerByID(playerID); // Get the player GameObject
            if (player != null && stats.respawnPoint != null)
            {
                // Set respawn position above the respawn point
                Vector3 respawnPosition = stats.respawnPoint.position + Vector3.up;
                player.transform.position = respawnPosition; // Move player to respawn position
                stats.health = 50f; // Reset player health
                Debug.Log("Player respawned: " + playerID);
            }
        }
    }

    // Method to update the scoreboard UI
    //public void UpdateScoreboard()
    //{
    //    // Clear existing rows from the scoreboard
    //    foreach (Transform child in scoreboardContainer)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    // Loop through all players in the Photon network
    //    foreach (var player in PhotonNetwork.PlayerList)
    //    {
    //        // Get the player's stats
    //        int kills = GetKills(player.ActorNumber);
    //        int deaths = GetDeaths(player.ActorNumber);

    //        // Create a new scoreboard row and set player info
    //        ScoreboardItem row = Instantiate(scoreboardRowPrefab, scoreboardContainer).GetComponent<ScoreboardItem>();
    //        //row.Initialize(player, kills, deaths); // Set initial stats
    //    }
    //}

    // When player stats are updated, refresh the specific player's row
    //public void RefreshPlayerRow(int playerID)
    //{
    //    // Find the specific scoreboard row for this player
    //    foreach (Transform rowTransform in scoreboardContainer)
    //    {
    //        ScoreboardItem row = rowTransform.GetComponent<ScoreboardItem>();
    //        if (row.playerIDText.text == PhotonNetwork.GetPhotonView(playerID).Owner.NickName)
    //        {
    //            row.UpdateStats(GetKills(playerID), GetDeaths(playerID)); // Update the row
    //        }
    //    }
    //}

    //Method to get the current health of a player
    public float GetPlayerHealth(int playerID)
    {
        if (playerStats.ContainsKey(playerID))
        {
            return playerStats[playerID].health;
        }

        return -1f; // Return -1 if player not found
    }

    // Retrieve player stats for UI or other purposes
    public PlayerStats GetPlayerStats(int playerID)
    {
        if (playerStats.ContainsKey(playerID))
        {
            return playerStats[playerID];
        }

        return null;
    }

    // Example method to retrieve a player GameObject based on their playerID
    private GameObject GetPlayerByID(int playerID)
    {
        // Iterate over all players in the room
        //foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        //{
        //    if (player.ActorNumber == playerID)
        //    {
        //        // Return the player GameObject corresponding to the playerID
        //        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        //        foreach (PhotonView view in photonViews)
        //        {
        //            if (view.Owner.ActorNumber == playerID)
        //            {
        //                return view.gameObject;
        //            }
        //        }
        //    }
        //}

        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in photonViews)
        {
            if (view.Owner.ActorNumber == playerID)
            {
                return view.gameObject;
            }
        }
        return null;

        //Test that method was triggered and variable set
        //Debug.LogError("Player with ID " + playerID + " not found.");
        //return null;
    }

    // Method to get the number of kills for a specific player
    private int GetKills(int playerID) => playerStats.ContainsKey(playerID) ? playerStats[playerID].kills : 0;

    // Method to get the number of deaths for a specific player
    private int GetDeaths(int playerID) => playerStats.ContainsKey(playerID) ? playerStats[playerID].deaths : 0;

    private void SetPhotonCustomProperties(int playerID)
    {
        Photon.Realtime.Player photonPlayer = GetPhotonPlayerByID(playerID);
        if (photonPlayer != null)
        {
            Hashtable hash = new Hashtable
            {
                {"kills", GetKills(playerID)},
                {"deaths", GetDeaths(playerID)}
            };
            photonPlayer.SetCustomProperties(hash);
        }
    }

    public Photon.Realtime.Player GetPhotonPlayerByID(int playerID)
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == playerID)
            {
                return player; // Return the Photon player object
            }
        }

        Debug.LogError("Photon Player with ID " + playerID + " not found.");
        return null;
    }
}
