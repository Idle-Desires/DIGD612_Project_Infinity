using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardRow : MonoBehaviour
{
    public TextMeshProUGUI PlayerIDText;
    public TextMeshProUGUI PlayerKillText;
    public TextMeshProUGUI PlayerDeathText;

    public void SetPlayerInfo(int playerId, int kills, int deaths)
    {
        PlayerIDText.text = "Player " + playerId;
        PlayerKillText.text = kills.ToString();
        PlayerDeathText.text = deaths.ToString();
    }
}
