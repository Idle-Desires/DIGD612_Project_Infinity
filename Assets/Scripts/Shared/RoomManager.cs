using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;
    [SerializeField] string gameVersion;

    public GameObject player;

    [Space]
    public Transform spawnPoint;

    [Space]
    public GameObject roomCam;

    [Space]
    public GameObject nameUI;
    public GameObject connectingUI;

    private string playerName = "None";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangePlayerName(string name)
    {
        playerName = name; 
    }

    //public void JoinRoomButtonPressed()
    //{
    //    Debug.Log("Connecting...");

    //    PhotonNetwork.ConnectUsingSettings();

    //    nameUI.SetActive(false);
    //    connectingUI.SetActive(true);
    //}

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting...");

        //Setting the version of the game
        PhotonNetwork.GameVersion = gameVersion;

        PhotonNetwork.ConnectUsingSettings();

        //nameUI.SetActive(false);
        //connectingUI.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to Server");

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        //PhotonNetwork.JoinOrCreateRoom("test", null, null);

        Debug.Log("Connected and in a room");

        //GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
        PhotonNetwork.JoinOrCreateRoom("Test", null, null);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        roomCam.SetActive(false);

        Debug.Log("Connected and in a room");

        //GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);

        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
        _player.GetComponent<PlayerSetup>().IsLocalPlayer();

        _player.GetComponent<PhotonView>().RPC("SetName",RpcTarget.All,playerName);
    }
}
