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
    Vector3 position = new Vector3(1, 3, 1); //spawn height

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
        playerID = PhotonNetwork.PlayerList.Length - 1;
        connectionStatus = $"PlayerID : {playerID}";

        SpawnPlayer();
    }

    void SpawnPlayer()
    {

        //Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minZ, maxZ));
        PhotonNetwork.Instantiate(playerPrefab.name, position, Quaternion.identity);
    }

    private void OnGUI()
    {
        GUILayout.Label(connectionStatus);
    }
}
