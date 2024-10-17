using Photon.Pun;
using TMPro;  // For TextMeshPro
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    public TextMeshProUGUI HealthDisplay;  // Reference to the health display text object
    private int playerID;                  // Player's ID in the Photon network

    void Start()
    {
        // Get the player ID from Photon (assuming each player has a PhotonView)
        PhotonView photonView = GetComponent<PhotonView>();

        if (photonView != null && photonView.IsMine)
        {
            playerID = photonView.Owner.ActorNumber;  // Get the player's unique ID

            Debug.Log("Player ID: " + playerID);

            // Ensure the player was added to the GameManager
            if (GameManager.instance.GetPlayerHealth(playerID) == -1f)
            {
                Debug.LogError("Player " + playerID + " not found in GameManager!");
            }
        }

        // Optionally, initialize the health display with current health
        //UpdateHealthDisplay();
    }

    void Update()
    {
        // Periodically update the health display based on the player's current health
        //UpdateHealthDisplay();
    }

    void UpdateHealthDisplay()
    {
        // Retrieve player's health from the GameManager
        float health = GameManager.instance.GetPlayerHealth(playerID);

        if (health != -1f)
        {
            // Update the text with the player's current health
            HealthDisplay.SetText("Health: " + health.ToString("0"));
        }
        else
        {
            Debug.LogWarning("Player health not found for playerID: " + playerID);
        }
    }
}
