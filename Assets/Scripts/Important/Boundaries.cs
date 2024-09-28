using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundaries : MonoBehaviour
{
    //Respawn
    public Transform respawn;
    public GameObject playerObj;
    public float respawnOffsetY = 1f;
    public LayerMask groundLayer;

    [SerializeField] public PlayerStats otherPlayer;

    //Player ID
    public int playerID;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit boundary, respawning");

            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            PlayerController playerControls = other.GetComponent<PlayerController>();

            // Get the GameManager instance
            GameManager gmPlayer = GameManager.instance;

            if (gmPlayer != null)
            {
                // Increase the player's death count
                gmPlayer.RecordDeath(playerID);
                //Debug.LogError(playerID);
            }
            else
            {
                Debug.LogError("GameManager instance not found.");
            }

            // Handle player respawn
            float raycastDistance = 0.65f;

            //Ensure the player is not clipping into the ground using a raycast
            Vector3 respawnPosition = respawn.position + new Vector3(0, respawnOffsetY, 0);
            if (Physics.Raycast(respawnPosition, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
            {
                //If ground detected close to respawn position, set player's Y position above it
                respawnPosition.y = hit.point.y + respawnOffsetY;
            }

            // Set player's position to the respawn location
            other.transform.position = respawnPosition;
        }
    }
}
