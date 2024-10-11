using Photon.Pun;
using Photon;
using Photon.Realtime;
using System.Collections;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable; // Alias for Photon Hashtable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun.Demo.PunBasics;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    //Singleton instance to ensure only one NetworkManager exists
    public static NetworkManager nmInstance;

    [Header("Connection Settings")]
    [Tooltip("Version of the game for Photon matchmaking.")]
    [SerializeField] private string gameVersion = "1.0";

    [Tooltip("Desired room name. Set via UI or Inspector.")]
    public string roomName = "DefaultRoom";

    [Header("Player Setup")]
    [Tooltip("Prefab for the player. Must be located in a 'Resources' folder.")]
    public GameObject playerPrefab;
    public int playerID;

    [Tooltip("Spawn points where players can appear.")]
    public Transform[] spawnPoints;

    [Header("Debug")]
    [Tooltip("Displays connection status on the screen.")]
    public string connectionStatus = "Not Connected";

    // Static counter to assign unique player numbers
    //private static int nextPlayerNumber = 1;

    // PhotonView component reference
    private PhotonView pv;

    int previousNumber;
    int assignedNumber = 0;

    string nickname = "unnamed";
    int nameCount = 1;

    private List<string> playerNames = new List<string>();

    private void Awake()
    {
        // Implement Singleton pattern
        if (nmInstance == null)
        {
            nmInstance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        // Ensure PhotonView is attached
        pv = GetComponent<PhotonView>();
        if (pv == null)
        {
            Debug.LogError("PhotonView component missing on NetworkManager. Adding one automatically.");
            pv = gameObject.AddComponent<PhotonView>();
            pv.ViewID = 0; // Assign a unique ViewID if necessary
        }
    }

    public void ChangeNickname()
    {
        nickname = "Player " + assignedNumber.ToString();
        Debug.Log("Right Player Number " + assignedNumber);
        Debug.Log("Right Player Name " + nickname);
        
        if (playerNames.Contains(nickname))
        {
            nameCount++;
            assignedNumber++;
            nickname = "Player " + assignedNumber.ToString();
            Debug.Log("Oh no list test");
        }

        //nickname = newName;
        playerNames.Add(nickname); // Add the new nickname to the list
        nameCount++;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnConnectToServer();
    }

    private void OnConnectToServer()
    {
        connectionStatus = "Connecting to Server";

        //Setting the version of the game
        PhotonNetwork.GameVersion = gameVersion;

        //Connect to the photon servers
        PhotonNetwork.ConnectUsingSettings();

    }

    public override void OnConnectedToMaster()
    {
        connectionStatus = "Connected to Master";
        Debug.Log("Connected to Master Server.");
        PhotonNetwork.JoinLobby();
        connectionStatus = "Connecting to Lobby";
    }

    public override void OnJoinedLobby()
    {
        connectionStatus = "Joined Lobby";
        Debug.Log("Joined Lobby.");
        PhotonNetwork.JoinRandomRoom(); // Attempt to join a random room
        connectionStatus = "Joining Random Room...";
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionStatus = "Failed to Join Random Room. Creating Room...";
        Debug.Log($"Failed to join random room: {message}. Creating a new room.");

        // Create a new room with the specified room name
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = 20 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        connectionStatus = "Joined Room.";
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        playerID = PhotonNetwork.LocalPlayer.ActorNumber; // Assign playerID based on Photon ActorNumber
        connectionStatus = $"PlayerID: {playerID}";
        assignedNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        // Spawn player in the game
        SpawnPlayer();

        // Assign a unique player number
        AssignPlayerNumber();
    }

    private void AssignPlayerNumber()
    {
        ChangeNickname();
        PhotonNetwork.NickName = nickname;
        Debug.Log("AssignedPlayerNumber Method " + PhotonNetwork.NickName);

        // Custom properties and RPC calls remain unchanged
        //previousNumber = nameCount - 1;
        PhotonHashtable props = new PhotonHashtable { { "PlayerNumber", assignedNumber } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"Assigned PlayerNumber {assignedNumber} to {PhotonNetwork.NickName}");

        pv.RPC("SyncPlayerNumber", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber, assignedNumber);
    }

    // RPC to sync player number across all clients
    [PunRPC]
    private void SyncPlayerNumber(int actorNumber, int playerNumber)
    {
        // Find the corresponding player and update their nickname
        if (PhotonNetwork.CurrentRoom.Players.TryGetValue(actorNumber, out Player player))
        {
            player.NickName = nickname;

            Debug.Log($"[RPC] Assigned {nickname} to Actor {actorNumber}");
        }
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not set in NetworkManager.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in NetworkManager.");
            return;
        }

        // Select a random spawn point from the available spawn points
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instantiate the player prefab at the selected spawn point
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Player instantiated at {spawnPoint.position}");

        nickname = "Player " + assignedNumber;

        Debug.Log("SpawnPlayer Nickname Test" + nickname);
        player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.All, nickname);
        //player.GetComponent<PhotonView>().RPC("SetNicknameDisplay", RpcTarget.All, nickname);
    }

    public void RespawnPlayer(PhotonView playerView)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not set in NetworkManager.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in NetworkManager.");
            return;
        }

        // Select a new random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Move the player to the new spawn point
        playerView.transform.position = spawnPoint.position;
        playerView.transform.rotation = spawnPoint.rotation;

        Debug.Log($"Player {playerView.Owner.NickName} respawned at {spawnPoint.position}");
    }

    // Sync kills and deaths over the network when they occur
    public void UpdatePlayerVariables(int actorNumber, int kills, int deaths)
    {
        PhotonView playerView = GetPlayerPhotonView(actorNumber);
        if (playerView != null)
        {
            playerView.RPC("UpdateVariables", RpcTarget.AllBuffered, kills, deaths);
            Debug.Log($"Updated variables for Player {playerView.Owner.NickName}: Kills={kills}, Deaths={deaths}");
        }
        else
        {
            Debug.LogError($"PhotonView not found for ActorNumber {actorNumber}");
        }
    }

    // Finds the player's PhotonView based on their ActorNumber
    private PhotonView GetPlayerPhotonView(int actorNumber)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.ActorNumber == actorNumber)
            {
                return PhotonView.Find(player.ActorNumber);
            }
        }
        return null;
    }

    private void OnGUI()
    {
        GUILayout.Label(connectionStatus);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        // Remove the player's nickname from the list when they leave
        playerNames.Remove(otherPlayer.NickName);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        // Update the list when a new player joins
        if (!playerNames.Contains(newPlayer.NickName))
        {
            playerNames.Add(newPlayer.NickName);
        }
    }
}

