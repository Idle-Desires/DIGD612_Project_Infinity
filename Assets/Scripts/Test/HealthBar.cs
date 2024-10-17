using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    //public TMP_Text healthText; // Reference to the TextMeshPro text field
    public TextMeshProUGUI healthText;

    // Method to set the initial/max health display
    public void SetMaxHealth(int health)
    {
        healthText.text = $"{health}/{health}"; // Display max health at the start
        //healthDisplay.SetText("Health: " + playerHealth.ToString())
    }

    // Method to update the health display
    public void SetHealth(int currentHealth, int maxHealth)
    {
        healthText.text = $"{currentHealth}/{maxHealth}"; // Display health as "current/max"
    }
}
