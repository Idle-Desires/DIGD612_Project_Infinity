using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // For PhotonHashtable
using UnityEngine;
using TMPro; // For TextMeshPro
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Stores and synchronizes player-specific variables such as name, kills, deaths, and health.
/// Handles taking damage, updating stats, and respawning.
/// </summary>
public class PlayerVariables : MonoBehaviourPunCallbacks, IPunObservable
{
    // Player's name displayed on the scoreboard
    public string playerName;
    public TextMeshPro nicknameText;

    // Player statistics
    public int kills = 0;
    public int deaths = 0;

    // Player number assigned upon joining
    public int playerNumber;

    [Header("Health Settings")]
    [Tooltip("Maximum health of the player.")]
    public int maxHealth = 50;

    [Tooltip("Current health of the player.")]
    [SerializeField]
    public int currentHealth;

    public TextMeshProUGUI healthDisplay; // TextMeshPro component for health display
    //internal static PlayerVariables pvInstance;
    public static PlayerVariables pvInstance;

    // Event to notify when player name is set
    public delegate void OnPlayerNameSet();
    public static event OnPlayerNameSet PlayerNameSet;

    // Events for decoupling systems
    public delegate void OnHealthChanged(int newHealth);
    public event OnHealthChanged HealthChanged;

    public delegate void OnPlayerDied();
    public event OnPlayerDied PlayerDied;

    /// <summary>
    /// Initializes player variables based on ownership and synchronizes them.
    /// </summary>
    private void Start()
    {
        if (photonView.IsMine)
        {
            //// The local player sets their own name based on PhotonNetwork.NickName
            //playerName = PhotonNetwork.NickName;
            //Debug.Log($"[Owner] PlayerVariables Initialized: {playerName}");

            //// Initialize health
            //currentHealth = maxHealth;
            //UpdateHealthDisplay();
            // The local player sets their own name based on PhotonNetwork.NickName
            playerName = PhotonNetwork.NickName;
            Debug.Log($"[Owner] PlayerVariables Initialized: {playerName}");

            // Initialize health
            currentHealth = maxHealth;
            UpdateHealthDisplay();

            // Initialize kills and deaths from CustomProperties
            InitializeStats();
        }
        else
        {
            //// For non-local players, retrieve the player number from custom properties
            //if (photonView.Owner.CustomProperties.ContainsKey("PlayerNumber"))
            //{
            //    playerNumber = (int)photonView.Owner.CustomProperties["PlayerNumber"];
            //    playerName = $"Player {playerNumber}";
            //    Debug.Log($"[Non-Owner] PlayerVariables Initialized: {playerName}");
            //}
            //else
            //{
            //    playerName = "Unknown Player";
            //    Debug.LogWarning($"[Non-Owner] PlayerVariables Initialized with Unknown Name for Actor {photonView.Owner.ActorNumber}");
            //}
            // For non-local players, retrieve the player number and stats from custom properties
            RetrieveRemotePlayerData();

            // Initialize health
            //currentHealth = maxHealth;
            //UpdateHealthDisplay();
        }
    }

    private void InitializeStats()
    {
        Hashtable initialStats = new Hashtable
        {
            { "Kills", kills },
            { "Deaths", deaths }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(initialStats);
    }

    private void RetrieveRemotePlayerData()
    {
        // Retrieve the player number from custom properties
        if (photonView.Owner.CustomProperties.ContainsKey("PlayerNumber"))
        {
            playerNumber = (int)photonView.Owner.CustomProperties["PlayerNumber"];
            playerName = $"Player {playerNumber}";
            Debug.Log($"[Non-Owner] PlayerVariables Initialized: {playerName}");
        }
        else
        {
            playerName = "Unknown Player";
            Debug.LogWarning($"[Non-Owner] PlayerVariables Initialized with Unknown Name for Actor {photonView.Owner.ActorNumber}");
        }

        // Retrieve kills and deaths from custom properties
        if (photonView.Owner.CustomProperties.ContainsKey("Kills"))
        {
            kills = (int)photonView.Owner.CustomProperties["Kills"];
        }

        if (photonView.Owner.CustomProperties.ContainsKey("Deaths"))
        {
            deaths = (int)photonView.Owner.CustomProperties["Deaths"];
        }

        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    /// <summary>
    /// Called when a player's custom properties are updated.
    /// Used to update playerName when "PlayerNumber" is set.
    /// </summary>
    /// <param name="targetPlayer">The player whose properties were updated.</param>
    /// <param name="changedProps">The properties that were changed.</param>
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        //base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        //// Check if the updated player is this player and if "PlayerNumber" was changed
        //if (targetPlayer == photonView.Owner && changedProps.ContainsKey("PlayerNumber"))
        //{
        //    playerNumber = (int)changedProps["PlayerNumber"];
        //    playerName = $"Player {playerNumber}";
        //    Debug.Log($"[OnPlayerPropertiesUpdate] PlayerNumber updated: {playerNumber}, PlayerName updated: {playerName}");

        //    // Update health display if necessary
        //    UpdateHealthDisplay();
        //}
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (targetPlayer != photonView.Owner)
            return;

        // Update PlayerNumber and PlayerName if PlayerNumber has changed
        if (changedProps.ContainsKey("PlayerNumber"))
        {
            playerNumber = (int)changedProps["PlayerNumber"];
            playerName = $"Player {playerNumber}";
            Debug.Log($"[OnPlayerPropertiesUpdate] PlayerNumber updated: {playerNumber}, PlayerName updated: {playerName}");
            UpdateNicknameDisplay();
        }

        // Update Kills if changed
        if (changedProps.ContainsKey("Kills"))
        {
            kills = (int)changedProps["Kills"];
            Debug.Log($"{playerName} Kills updated to: {kills}");
            // Update the scoreboard or UI as needed
            ScoreboardController.Instance?.UpdateScoreboard();
        }

        // Update Deaths if changed
        if (changedProps.ContainsKey("Deaths"))
        {
            deaths = (int)changedProps["Deaths"];
            Debug.Log($"{playerName} Deaths updated to: {deaths}");
            // Update the scoreboard or UI as needed
            ScoreboardController.Instance?.UpdateScoreboard();
        }
    }

    /// <summary>
    /// Synchronizes player health across the network.
    /// Kills and deaths are handled via RPCs, so they are not serialized here.
    /// </summary>
    /// <param name="stream">The PhotonStream to read from or write to.</param>
    /// <param name="info">Information about the message.</param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            // Only send currentHealth to other players
            stream.SendNext(currentHealth);
        }
        else
        {
            // Receive currentHealth from other players
            currentHealth = (int)stream.ReceiveNext();

            // Update health display
            UpdateHealthDisplay();
        }
    }

    /// <summary>
    /// Method to handle taking damage. Invokes an RPC to synchronize damage across all clients.
    /// </summary>
    /// <param name="damage">Amount of damage taken.</param>
    /// <param name="attackerView">PhotonView of the attacking player.</param>
    public void TakeDamage(int damage, PhotonView attackerView)
    {
        if (!photonView.IsMine)
            return;

        // Invoke RPC to handle damage on all clients
        photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage, attackerView.ViewID);
    }

    /// <summary>
    /// RPC to apply damage to the player, update health, handle death, and notify scoreboard.
    /// </summary>
    /// <param name="damage">Amount of damage taken.</param>
    /// <param name="attackerViewID">ViewID of the attacker.</param>
    [PunRPC]
    void RPC_TakeDamage(int damage, int attackerViewID)
    {
        //Debug.Log(currentHealth);
        //currentHealth -= damage;
        //Debug.Log(currentHealth);
        //UpdateHealthDisplay();

        //if (currentHealth <= 0)
        //{
        //    HandleDeath(attackerViewID);
        //    Debug.Log("DEATH");
        //}

        //Debug.Log($"{playerName} took {damage} damage. Current Health: {currentHealth}");
        Debug.Log($"{playerName} is taking {damage} damage.");

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthDisplay();

        // Notify any listeners about the health change
        HealthChanged?.Invoke(currentHealth);

        Debug.Log($"{playerName} took {damage} damage. Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            HandleDeath(attackerViewID);
            Debug.Log($"{playerName} has died.");
        }
    }

    /// <summary>
    /// Handles player death: increments kill and death counts, updates scoreboard, and respawns the player.
    /// </summary>
    /// <param name="attackerViewID">ViewID of the attacking player.</param>
    void HandleDeath(int attackerViewID)
    {
        //Debug.Log($"{playerName} has died.");

        //// Find the attacker's PhotonView using the ViewID
        //PhotonView attackerPV = PhotonView.Find(attackerViewID);
        //if (attackerPV != null)
        //{
        //    PlayerVariables attackerStats = attackerPV.GetComponent<PlayerVariables>();
        //    if (attackerStats != null)
        //    {
        //        // Increment attacker's kill count via RPC
        //        attackerStats.photonView.RPC("RPC_AddKill", RpcTarget.AllBuffered);
        //        Debug.Log($"{attackerStats.playerName} killed {playerName}");
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"PlayerVariables not found on attacker PhotonView {attackerViewID}");
        //    }
        //}
        //else
        //{
        //    Debug.LogWarning($"PhotonView with ViewID {attackerViewID} not found.");
        //}

        //// Increment this player's death count via RPC
        //photonView.RPC("RPC_AddDeath", RpcTarget.AllBuffered);

        //// Respawn the player
        //Respawn();
        // Find the attacker's PhotonView using the ViewID
        PhotonView attackerPV = PhotonView.Find(attackerViewID);
        if (attackerPV != null)
        {
            PlayerVariables attackerStats = attackerPV.GetComponent<PlayerVariables>();
            if (attackerStats != null)
            {
                // Increment attacker's kill count via CustomProperties
                attackerStats.IncrementKill();
                Debug.Log($"{attackerStats.playerName} killed {playerName}");
            }
            else
            {
                Debug.LogWarning($"PlayerVariables not found on attacker PhotonView {attackerViewID}");
            }
        }
        else
        {
            Debug.LogWarning($"PhotonView with ViewID {attackerViewID} not found.");
        }

        // Increment this player's death count via CustomProperties
        IncrementDeath();

        // Trigger death event
        PlayerDied?.Invoke();

        // Respawn the player
        Respawn();
    }

    /// <summary>
    /// RPC to increment the player's kill count and update the scoreboard.
    /// </summary>
    //[PunRPC]
    //void RPC_AddKill()
    //{
    //    kills++;
    //    Debug.Log($"{playerName} now has {kills} kills.");

    //    // Update the scoreboard
    //    if (ScoreboardController.Instance != null)
    //    {
    //        ScoreboardController.Instance.UpdateScoreboard();
    //    }
    //    else
    //    {
    //        Debug.LogWarning("ScoreboardController Instance is null.");
    //    }
    //}

    ///// <summary>
    ///// RPC to increment the player's death count and update the scoreboard.
    ///// </summary>
    //[PunRPC]
    //public void RPC_AddDeath()
    //{
    //    deaths++;
    //    Debug.Log($"{playerName} now has {deaths} deaths.");

    //    // Update the scoreboard
    //    if (ScoreboardController.Instance != null)
    //    {
    //        ScoreboardController.Instance.UpdateScoreboard();
    //    }
    //    else
    //    {
    //        Debug.LogWarning("ScoreboardController Instance is null.");
    //    }
    //}

    public void IncrementKill()
    {
        if (photonView.IsMine)
        {
            kills++;
            Hashtable props = new Hashtable { { "Kills", kills } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log($"{playerName} now has {kills} kills.");

            // Update the scoreboard
            ScoreboardController.Instance?.UpdateScoreboard();
        }
    }

    public void IncrementDeath()
    {
        if (photonView.IsMine)
        {
            deaths++;
            Hashtable props = new Hashtable { { "Deaths", deaths } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            Debug.Log($"{playerName} now has {deaths} deaths.");

            // Update the scoreboard
            ScoreboardController.Instance?.UpdateScoreboard();
        }
    }

    /// <summary>
    /// Initiates the respawn process for the player.
    /// </summary>
    void Respawn()
    {
        StartCoroutine(RespawnCoroutine());
    }

    /// <summary>
    /// Coroutine to handle respawning the player after a delay.
    /// </summary>
    /// <returns></returns>
    private System.Collections.IEnumerator RespawnCoroutine()
    {
        // Disable the player object (e.g., hide or deactivate the player)
        gameObject.SetActive(false);

        // Wait for a few seconds before respawning
        yield return new WaitForSeconds(2f);

        // Reset health
        currentHealth = maxHealth;
        UpdateHealthDisplay();

        // Move to a spawn point
        NetworkManager.nmInstance.RespawnPlayer(photonView);

        // Re-enable the player object
        gameObject.SetActive(true);

        Debug.Log($"{playerName} has respawned.");
    }

    // Method to update the health display with TextMeshPro
    void UpdateHealthDisplay()
    {
        if (healthDisplay != null)
        {
            healthDisplay.text = $"Health: {currentHealth}/{maxHealth}";
        }
    }

    //[PunRPC]
    //public void SetNickname(string name)
    //{
    //    playerName = name;
    //    Debug.Log("Set Nickname Player Name " +  playerName);

    //    nicknameText.text = playerName;
    //    Debug.Log(playerName);
    //}

    private void UpdateNicknameDisplay()
    {
        if (nicknameText != null)
        {
            nicknameText.text = playerName;
        }
    }

    /// <summary>
    /// Sets the player's nickname via RPC. This can be called when the player joins or changes their name.
    /// </summary>
    /// <param name="name">The new nickname.</param>
    [PunRPC]
    public void SetNickname(string name)
    {
        playerName = name;
        Debug.Log("Set Nickname Player Name: " + playerName);

        UpdateNicknameDisplay();
    }
}
