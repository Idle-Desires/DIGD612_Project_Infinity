using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerInputManager : MonoBehaviour
{
    //Player List
    public List<IndividualPlayerControls> players = new List<IndividualPlayerControls>();
    int maxPlayers = 4;

    public PlayerInputActions inputActions;

    //Delegate can be used for scripts relating to checking/waiting for player to join
    public delegate void OnPlayerJoined(int playerID);
    public OnPlayerJoined onPlayerJoined;

    private void Awake()
    {
        InitializeInputs();


    }

    //Input setup for detecting different players
    void InitializeInputs()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.JoinButton.performed += JoinButtonPerformed;
        inputActions.Enable();
    }

    private void JoinButtonPerformed(InputAction.CallbackContext obj)
    {
        if (players.Count >= maxPlayers)
        {
            return;
        }

        //Check if the device is already assigned
        foreach (IndividualPlayerControls player in players)
        {
            if (player.inputDevice == obj.control.device)
            {
                //Device is already assigned
                return;
            }
        }

        IndividualPlayerControls newPlayer = new IndividualPlayerControls();
        newPlayer.SetupPlayer(obj, players.Count);
        players.Add(newPlayer);

        if(onPlayerJoined != null)
        {
            onPlayerJoined.Invoke(newPlayer.playerID);
        }
    }
}
