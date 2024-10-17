using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public PlayerActions playerActions;

    public GameObject playerCamera;

    public void IsLocalPlayer()
    {
        playerActions.enabled = true;
        playerCamera.SetActive(true);
    }
}
