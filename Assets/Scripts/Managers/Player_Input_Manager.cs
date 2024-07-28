using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Input_Manager : MonoBehaviour
{
    //
    [Tooltip("Camera sensitivity multiplyer")]
    public float LookSensitivity = 1f;

    [Tooltip("Invert vertical input axis")]
    public bool InvertYAxis = false;

    [Tooltip("Invert horizontal input axis")]
    public bool InvertXAxis = false;

    [Tooltip("Controller input trigger limit")]
    public float TriggerAxisThreshold = 0.5f;

    Game_Manager m_Game_Manager;
    Player_Character_Controller m_Player_Character_Controller;
    bool m_FireInputHeld;


    //
    void Start()
    {
      m_Player_Character_Controller = GetComponent<Player_Character_Controller>();
      //DebugUtility.HandleErrorIfNullGetComponent<Player_Character_Controller, Player_Input_Manager>(m_Player_Character_Controller, this, gameObject);


    }

    //
    void Update()
    {
        
    }
















}
