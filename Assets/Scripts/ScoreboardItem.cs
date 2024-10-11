using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System;

public class ScoreboardItem : MonoBehaviour
{
    //public TMP_Text playerIDText;
    [Header("UI Elements")]
    [Tooltip("Text element for the player's name.")]
    public TMP_Text playerNameText;

    [Tooltip("Text element for the player's kill count.")]
    public TMP_Text killsText;

    [Tooltip("Text element for the player's death count.")]
    public TMP_Text deathsText;

    //[Tooltip("Text element for the player's health.")]
    //public TMP_Text healthText;
    //public static ScoreboardItem instance;

    public void SetPlayerInfo(string playerName, int kills, int deaths)
    {
        Debug.Log($"Setting player info: {playerName}, Kills: {kills}, Deaths: {deaths}");

        playerNameText.text = playerName;
        killsText.text = kills.ToString();
        deathsText.text = deaths.ToString();
    }

    //public void Initialize(Player player)
    //{
    //    playerIDText.text = player.NickName;  // Set player name
    //    //killsText.text = kills.ToString();    // Set kills
    //    //deathsText.text = deaths.ToString();  // Set deaths
    //}

    //public void Initialize(Player player, int kills, int deaths)
    //{
    //    playerIDText.text = player.NickName;  // Set player name
    //    killsText.text = kills.ToString();    // Set kills
    //    deathsText.text = deaths.ToString();  // Set deaths
    //}

    //public void UpdateStats(int kills, int deaths)
    //{
    //    killsText.text = kills.ToString();    // Update kills
    //    deathsText.text = deaths.ToString();  // Update deaths
    //}

    //internal void Initialize(Player player)
    //{
    //    throw new NotImplementedException();
    //}
}
