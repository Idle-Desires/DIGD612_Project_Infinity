using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Teleport : MonoBehaviour
{
    //Respawn
    public Transform respawnPoint;
    public Transform spawnPoint;

    // References to the collider and renderer of the pickup object
    private Collider pickupCollider;
    private Renderer pickupRenderer;

    // Start is called when the script is initialized
    void Start()
    {
        // Get references to the object's collider and renderer so they can be enabled/disabled later
        pickupCollider = GetComponent<Collider>();
        pickupRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player collides with the pickup
        if (other.CompareTag("Player"))
        {
            // Get the PhotonView component to retrieve the player ID
            PhotonView photonView = other.GetComponent<PhotonView>();
            if (photonView != null)
            {
                int playerID = photonView.Owner.ActorNumber; // Get player ID from Photon

                //Vector3 respawnPosition = respawnPoint.position + new Vector3(0.5f, 3.3f, -0.3f);

                //Move the player to the respawn point
                //transform.position = respawnPosition;

                //// Move the player to the new spawn point
                photonView.transform.position = spawnPoint.position;
                photonView.transform.rotation = spawnPoint.rotation;

                //Debug.Log($"Player {playerID.Owner.NickName} respawned at {spawnPoint.position}");
            }
        }
    }
}
