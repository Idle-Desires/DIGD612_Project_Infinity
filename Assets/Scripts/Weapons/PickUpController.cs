using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PickUpController : MonoBehaviour
{
    // Define the type of pickup: Health or Ammo
    public enum PickupType { SmallHealth, BigHealth, Ammo }
    public PickupType pickupType;  // To differentiate between health and ammo pickups

    // Amount of health the player receives for small and big health pickups
    public float smallHealthAmount = 10f;
    public float bigHealthAmount = 20f;

    // Amount of ammo the player receives when picking up an ammo pickup
    public int ammoAmount = 40;

    // Time in seconds after which the pickup will respawn
    public float respawnTime = 5f;

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

                // Handle health pickups
                if (pickupType == PickupType.SmallHealth)
                {
                    // Heal player by small amount
                    GameManager.instance.HealPlayer(playerID, smallHealthAmount);
                }
                else if (pickupType == PickupType.BigHealth)
                {
                    // Heal player by big amount
                    GameManager.instance.HealPlayer(playerID, bigHealthAmount);
                }
                // Handle ammo pickups
                else if (pickupType == PickupType.Ammo)
                {
                    WeaponsController weaponController = other.GetComponentInChildren<WeaponsController>();
                    if (weaponController != null)
                    {
                        weaponController.ammoTotal = 60;
                        weaponController.magSize = 20;
                        weaponController.bulletsLeftInMagazine = weaponController.magSize;
                        weaponController.Reload();
                    }
                }

                // Start the respawn process (instead of destroying the object)
                StartCoroutine(RespawnPickup());
            }
        }
    }

    // Coroutine to handle the respawn of the pickup after a certain time
    private IEnumerator RespawnPickup()
    {
        // Disable the pickup item by turning off its collider and renderer
        pickupCollider.enabled = false;
        pickupRenderer.enabled = false;

        // Wait for the specified respawn time
        yield return new WaitForSeconds(respawnTime);

        // Re-enable the pickup item by turning its collider and renderer back on
        pickupCollider.enabled = true;
        pickupRenderer.enabled = true;
    }
}
