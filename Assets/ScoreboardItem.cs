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
    public TMP_Text playerIDText;
    public TMP_Text killsText;
    public TMP_Text deathsText;
    public static ScoreboardItem instance;

    public void Initialize(Player player)
    {
        playerIDText.text = player.NickName;  // Set player name
        //killsText.text = kills.ToString();    // Set kills
        //deathsText.text = deaths.ToString();  // Set deaths
    }

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
