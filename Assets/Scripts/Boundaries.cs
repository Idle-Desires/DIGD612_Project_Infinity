using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Boundaries : MonoBehaviour
{
    //Respawn
    public Transform respawn;
    public GameObject playerObj;
    public float respawnOffsetY = 1f;
    public LayerMask groundLayer;

    [SerializeField] public PlayerStats otherPlayer;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trig Enter");
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            PlayerController playerControls = other.GetComponent<PlayerController>();

            //Get PlayerStats from the player who hit the boundary
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                //Call the DeathCounter method to increase deaths
                playerStats.DeathCounter();
            }
            else
            {
                Debug.LogError("PlayerStats component not found on the player object.");
            }

            //How far the player is from the ground
            float raycastDistance = 0.65f;

            //Ensure the player is not clipping into the ground using a raycast
            Vector3 respawnPosition = respawn.position + new Vector3(0, respawnOffsetY, 0);
            if (Physics.Raycast(respawnPosition, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
            {
                //If ground detected close to respawn position, set player's Y position above it
                respawnPosition.y = hit.point.y + respawnOffsetY;
            }

            other.transform.position = respawnPosition;
        }
    }
}
