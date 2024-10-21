using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    //Player Health Pool
    public float playerHealth = 50f;
    public int playerDeaths;
    public TextMeshProUGUI deathDisplay;

    //Respawn
    public Transform respawnPoint; 
    public float respawnOffsetY = 1f;
    public LayerMask groundLayer;

    // References to components
    private Rigidbody playerRb;

    private void Awake()
    {
        playerDeaths = 0;
        playerRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Out of health
        if (playerHealth <= 0f)
        {
            Die();
        }
    }

    //What happens when the enemy is shot at
    public void TakeDamage(float damage)
    {
        //Decreasing health
        playerHealth -= damage;

        Debug.Log(playerHealth);

        //Out of health
        if (playerHealth <= 0f)
        {
            Die();
        }
    }

    //Once they have taken enough damage to die
    void Die()
    {
        DeathCounter();

        //Handle respawning logic
        RespawnPlayer();

        //Testing for death count
        Debug.Log(playerDeaths);

        playerHealth = 50;
    }

    //For 3D RB add 2D for other rb option
    void OnCollisionEnter(Collision collision) //Being triggered near collision areas. Will need to look at.
    {
        Debug.Log(playerHealth);

        if(collision.gameObject.name == "Bullet")
        {
            playerHealth -= 10f;
        }
    }

    public void DeathCounter()
    {
        playerDeaths++;
        Debug.Log("Deaths: " + playerDeaths);
    }
    private void RespawnPlayer()
    {
        Debug.Log("Respawning Player...");

        //Calculate the respawn position
        Vector3 respawnPosition = respawnPoint.position + new Vector3(0, respawnOffsetY, 0);

        //Ensure the player isn't clipping into the ground using a raycast
        float raycastDistance = 0.65f;
        if (Physics.Raycast(respawnPosition, Vector3.down, out RaycastHit hit, raycastDistance, groundLayer))
        {
            // If ground detected close to the respawn position, adjust the player's Y position
            respawnPosition.y = hit.point.y + respawnOffsetY;
        }

        //Move the player to the respawn point
        transform.position = respawnPosition;
    }
}
