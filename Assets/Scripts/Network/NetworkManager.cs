using Photon.Pun;
using Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager nmInstance;
    [SerializeField] string gameVersion;
    string connectionStatus;
    public string roomName; //rework for the user to enter in a room name

    public int playerID;

    //prefab + variables for it
    public GameObject playerPrefab;
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    public Vector3 position = new Vector3(1, 3, 1); //spawn height
    public Vector3 startRespawnPosition;

    private void Awake()
    {
        if (nmInstance == null)
        {
            nmInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
        PhotonNetwork.JoinLobby();

        connectionStatus = "Connecting to Lobby";
    }

    public override void OnJoinedLobby()
    {
        connectionStatus = "Lobby Joined";
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionStatus = "Failed to Join Room";
        PhotonNetwork.CreateRoom(roomName);
        connectionStatus = "Creating Room";
    }

    public override void OnJoinedRoom()
    {
        connectionStatus = "Room Joined";

        // Assign playerID based on Photon actor number (unique ID)
        playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        connectionStatus = $"PlayerID : {playerID}";

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // Instantiate the player prefab at a specific position
        //GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, startRespawnPosition, Quaternion.identity);
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);

        // Retrieve the player's ID from Photon (should match playerID assigned above)
        int newPlayerID = PhotonNetwork.LocalPlayer.ActorNumber;

        Debug.Log(newPlayerID);

        // Add the player to the GameManager with their respawn point
        GameManager.instance.AddPlayer(newPlayerID, player.transform);

        //// Sync player stats (kills, deaths) with Photon custom properties
        //Photon.Realtime.Player photonPlayer = PhotonNetwork.LocalPlayer;
        //ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable
        //{
        //    { "kills", 0 },
        //    { "deaths", 0 }
        //};
        //photonPlayer.SetCustomProperties(initialProps);

        // Refresh the scoreboard UI after adding a new player
        //GameManager.instance.UpdateScoreboard();
    }

    private void OnGUI()
    {
        GUILayout.Label(connectionStatus);
    }
}
