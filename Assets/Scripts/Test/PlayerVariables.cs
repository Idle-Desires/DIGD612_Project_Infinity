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
    public int damage = 10;

    // Player number assigned upon joining
    public int playerNumber;

    private Rigidbody playerRb;

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

    //PhotonView photonView;

    /// <summary>
    /// Initializes player variables based on ownership and synchronizes them.
    /// </summary>
    private void Start()
    {
        //photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            // The local player sets their own name based on PhotonNetwork.NickName
            playerName = PhotonNetwork.NickName;
            Debug.Log($"[Owner] PlayerVariables Initialized: {playerName}");
            playerRb = GetComponent<Rigidbody>();

            playerNumber = PhotonNetwork.LocalPlayer.ActorNumber;

            // Initialize health
            currentHealth = maxHealth;
            UpdateHealthDisplay();

            // Initialize kills and deaths from CustomProperties
            InitializeStats();
        }
        else
        {
            // For non-local players, retrieve the player number and stats from custom properties
            RetrieveRemotePlayerData();
        }
    }

    private void InitializeStats()
    {
        Hashtable initialStats = new Hashtable
        {
            { "Kills", kills },
            { "Deaths", deaths },
            { "PlayerNumber", playerNumber } //new add
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

        // Retrieve kills from custom properties
        if (photonView.Owner.CustomProperties.ContainsKey("Kills"))
        {
            kills = (int)photonView.Owner.CustomProperties["Kills"];
        }

        // Retrieve deaths from custom properties
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
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
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
        if (photonView.IsMine)
        {
            photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
        }
        //if (!photonView.IsMine)
        //    return;

        //Debug.Log(damage + "taken");

        //// Invoke RPC to handle damage on all clients
        //photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.All, damage, attackerView.ViewID);
        //photonView.RPC("RPC_TakeDamage", RpcTarget.AllBuffered, damage);
    }

    /// <summary>
    /// RPC to apply damage to the player, update health, handle death, and notify scoreboard.
    /// </summary>
    /// <param name="damage">Amount of damage taken.</param>
    /// <param name="attackerViewID">ViewID of the attacker.</param>
    /// 
    [PunRPC]
    void RPC_TakeDamage(int damage, int attackerViewID) // void RPC_TakeDamage(int damage, int attackerViewID, PhotonMessageInfo info)
    {
        if (!photonView.IsMine)
            return;

        Debug.Log(playerName + "is processing RPC_TakeDamage with damage:" + damage);

        PhotonView attackerPV = PhotonView.Find(attackerViewID);
        Debug.Log(attackerPV);
        if (attackerPV == null)
        {
            Debug.LogWarning($"Attacker PhotonView with ViewID {attackerViewID} not found.");
            return;
        }

        PlayerVariables attackerStats = attackerPV.GetComponent<PlayerVariables>();
        if (attackerStats == null)
        {
            Debug.LogWarning($"PlayerVariables not found on attacker PhotonView {attackerViewID}");
            return;
        }

        // Apply damage
        currentHealth -= damage;
        //currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthDisplay();
        HealthChanged?.Invoke(currentHealth);
        Debug.Log(playerName + "took " + damage + "damage. Current Health:" + currentHealth);

        // Check if the player's health has dropped to zero or below
        if (currentHealth <= 0)
        {
            HandleDeath(attackerPV);
            Debug.Log($"{playerName} has died.");
        }
    }
    [PunRPC]
    public void Damage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            // Increment this player's death count via CustomProperties
            IncrementDeath();

            // Trigger death event
            PlayerDied?.Invoke();

            // Respawn the player
            Respawn();

            IncrementKill();
        }
    }

    /// <summary>
    /// Handles player death: increments kill and death counts, updates scoreboard, and respawns the player.
    /// </summary>
    /// <param name="attackerViewID">ViewID of the attacking player.</param>

    void HandleDeath(PhotonView attackerPV)
    {
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
                Debug.LogWarning($"PlayerVariables not found on attacker PhotonView {attackerPV.ViewID}");
            }
        }
        else
        {
            Debug.LogWarning("Attacker PhotonView is null. Cannot increment kill count.");
        }

        // Increment this player's death count via CustomProperties
        IncrementDeath();

        // Trigger death event
        PlayerDied?.Invoke();

        // Respawn the player
        Respawn();
    }

    public void IncrementKill()
    {
        if (!photonView.IsMine)
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
        if (!photonView.IsMine)
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
        //gameObject.SetActive(false);
        currentHealth = maxHealth;

        // Wait for a few seconds before respawning
        yield return new WaitForSeconds(1f);

        // Reset health
        UpdateHealthDisplay();

        // Move to a spawn point
        NetworkManager.nmInstance.RespawnPlayer(photonView);

        // Re-enable the player object
        //gameObject.SetActive(true);

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

    void OnCollisionEnter(Collision collision) //Was private
    {
        //if (collision.gameObject.CompareTag("Bullet"))
        //{
        //    TakeDamage(damage, photonView);
        //    Debug.Log(photonView);
        //    Debug.Log("Shooting damage");
        //}
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // Get the PhotonView from the bullet
            PhotonView bulletPhotonView = collision.gameObject.GetComponent<PhotonView>();
            if (bulletPhotonView != null)
            {
                // Call the TakeDamage method with the damage amount and the attacker's PhotonView
                //TakeDamage(damage, bulletPhotonView);
                //Debug.Log($"Damage applied by {bulletPhotonView.Owner.NickName}");
                Damage(damage);
            }
            else
            {
                Debug.LogWarning("Bullet does not have a PhotonView attached.");
            }
        }
    }
}
