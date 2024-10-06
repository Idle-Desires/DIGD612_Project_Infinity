using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerClass { Sniper, Soldier }
public class ClassManager : MonoBehaviour
{
    ClassManager instance;
    public PlayerClass selectedClass; //on  your player

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);
    }

    public void SelectSniper()
    {
        //called from button
        //reference to your players
        //set their class as c
        selectedClass = PlayerClass.Sniper;
    }

    public void SelectSoldier()
    {
        //called from button
        //reference to your players
        //set their class as c
        selectedClass = PlayerClass.Soldier;
    }
}
