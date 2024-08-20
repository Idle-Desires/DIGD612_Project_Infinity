using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;//Input system use

public class InputManager : MonoBehaviour
{
    //Input actions reference
    public static InputControls inputControls; //the input manager script under input folder
    //private Rigidbody rb;

    //Movement position
    public Vector2 movement;

    // Start is called before the first frame update
    void Awake()
    {
        //Input creation 
        inputControls = new InputControls();

        //Jump control input
        inputControls.MasterControls.Jump.performed += JumpPerformed;//GetKeyDown
        inputControls.MasterControls.Jump.canceled += JumpCanceled;//GetKeyUp

        //Attack control input
        inputControls.MasterControls.Attack.performed += AttackPerformed;//GetKeyDown
        inputControls.MasterControls.Attack.canceled += AttackCanceled;//GetKeyUp

        //Movement control input
        inputControls.MasterControls.Movement.performed += MovementPerformed;//GetKeyDown
        inputControls.MasterControls.Movement.canceled += MovementCanceled;//GetKeyUp

        //Turning on the controls
        //inputControls.Enable();

        //rb = this.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputControls.Enable();
    }

    // Update is called once per frame
    private void OnDisable()
    {
        inputControls.Disable();

    }

    //Movement button functionality for the up key
    private void MovementCanceled(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        movement = Vector2.zero;
    }

    //Movement button functionality for the down key
    private void MovementPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        movement = context.ReadValue<Vector2>();
    }

    //Attack button functionality for the up key
    private void AttackCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Attack Canceled");
    }

    //Attack button functionality for the down key
    private void AttackPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Attack Performed");
        Debug.Log(context.control.device.displayName);//Determines the device being used like mouse or gamepad
    }

    //Jump button functionality for the up key
    private void JumpCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("Jump Canceled");
    }

    //Jump button functionality for the down key
    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Jump Pressed");
        //Debug.Log(context.control.device.displayName);

        //rb.AddForce(Vector3.up * 5, ForceMode.Impulse);

        //Determine what controller it is
        if(context.control.device is UnityEngine.InputSystem.XInput.XInputControllerWindows)
        {
            Debug.Log("Xbox");
        }
        else if (context.control.device is UnityEngine.InputSystem.DualShock.DualShockGamepad)
        {
            Debug.Log("Playstation");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Vector2 direction = moveAction.ReadValue<Vector2>();
        //transform.position += new Vector3(direction.x, 0, direction.y) * moveSpeed * Time.deltaTime;
    }
}
