using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    //[SerializeField] private Transform player;
    //[SerializeField] private Transform respawnPoint;
    ////private Rigidbody playerRb;

    ////private void Start()
    ////{
    ////    // Get the Rigidbody component from the player
    ////    playerRb = player.GetComponent<Rigidbody>();
    ////}

    //private void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log("Triggered by: " + other.gameObject.name);

    //    player.transform.position = respawnPoint.transform.position;

    //    //Sync physics transforms to avoid any issues
    //    Physics.SyncTransforms();

    //    //if (other.CompareTag("Player"))
    //    //{
    //    //    //Reset the player's Rigidbody velocity and position
    //    //    if (playerRb != null)
    //    //    {
    //    //        playerRb.velocity = Vector3.zero;
    //    //        playerRb.angularVelocity = Vector3.zero;
    //    //        player.transform.position = respawnPoint.position;
    //    //    }
    //    //    else
    //    //    {
    //    //        //Fallback if Rigidbody is not found
    //    //        player.transform.position = respawnPoint.transform.position;
    //    //    }

    //    //    //Sync physics transforms to avoid any issues
    //    //    Physics.SyncTransforms();
    //    //}
    //}
}
