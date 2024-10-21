using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

[System.Serializable]
public class IndividualPlayerControls
{
    public int playerID;
    public InputDevice inputDevice;
    InputUser inputUser;

    public PlayerInputActions playerInput;
    public ControllerType controllerType;

    public enum ControllerType 
    {
        Keyboard,
        Playstation,
        Xbox
    }

    public void SetupPlayer(InputAction.CallbackContext obj, int ID)
    {
        playerID = ID;
        inputDevice = obj.control.device;

        playerInput = new PlayerInputActions();

        //How to assign a single device to a player
        inputUser = InputUser.PerformPairingWithDevice(inputDevice);
        inputUser.AssociateActionsWithUser(playerInput);

        playerInput.Enable();
    }

    void SetControllerType()
    {
        if (inputDevice is UnityEngine.InputSystem.DualShock.DualShockGamepad) 
        {
            controllerType = ControllerType.Playstation;
        }
        else if (inputDevice is UnityEngine.InputSystem.XInput.XInputControllerWindows)
        {
            controllerType = ControllerType.Xbox;
        }
        else if (inputDevice is UnityEngine.InputSystem.Keyboard)
        {
            controllerType = ControllerType.Keyboard;
        }
    }

    public void DisableControls() 
    { 
        playerInput.Disable();
    }
}
