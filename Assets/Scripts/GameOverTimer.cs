using Photon.Chat.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverTimer : MonoBehaviour
{
    public float timeRemaining = 10f;  // Set initial time in seconds
    public TextMeshProUGUI timerText;  // Reference to a UI Text element to display the countdown (optional)
    public string sceneToLoad;  // Name of the scene to load when the timer reaches zero

    private bool timerIsRunning = false;

    void Start()
    {
        // Start the countdown
        timerIsRunning = true;
    }

    void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                // Reduce time by the amount of time passed in the current frame
                timeRemaining -= Time.deltaTime;

                // Update the timer UI text (optional)
                if (timerText != null)
                {
                    DisplayTime(timeRemaining);
                }
            }
            else
            {
                // Time has run out
                timeRemaining = 0;
                timerIsRunning = false;

                // Load the next scene
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;  // To make the timer look a bit better in UI
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);  // Calculate minutes
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);  // Calculate seconds

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);  // Update text
    }
}
