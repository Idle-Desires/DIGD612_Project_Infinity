using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed = 10f; //For set speed make private or testing speed make public

    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    //Movement position
    public Vector2 movement = Vector2.zero;

    Rigidbody rb = null;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    //bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode escapeKey = KeyCode.Escape;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode jumpCKey = KeyCode.JoystickButton0; //Xbox A button
    public KeyCode crouchKey = KeyCode.LeftControl;
    //Basic Keybinding to set interaction to e if needed, can be deleted otherwise

    [Header("Input Controls")]
    public InputControls inputControls = null;

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
        inputControls.Enable();

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        //Setting player movment to true to enable jump 
        ResetJump();

        startYScale = transform.localScale.y; //For crouching
    }

    private void OnEnable()
    {
        inputControls.Enable();
        inputControls.MasterControls.Movement.performed += MovementPerformed;//GetKeyDown
        inputControls.MasterControls.Movement.canceled += MovementCanceled;//GetKeyUp
    }

    private void OnDisable()
    {
        inputControls.Disable();
        inputControls.MasterControls.Movement.performed -= MovementPerformed;//GetKeyDown
        inputControls.MasterControls.Movement.canceled -= MovementCanceled;//GetKeyUp

    }

    //Movement button functionality for the up key
    private void MovementCanceled(InputAction.CallbackContext context)
    {
        Vector2 movement = Vector2.zero;
    }

    //Movement button functionality for the down key
    private void MovementPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        movement = context.ReadValue<Vector2>();

        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
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
        ResetJump();
    }

    //Jump button functionality for the down key
    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Jump Pressed");
        //Debug.Log(context.control.device.displayName);

        //readyToJump = false;

        //Reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        //Invoke(nameof(ResetJump), jumpCooldown);

        //Determine what controller it is
        if (context.control.device is UnityEngine.InputSystem.XInput.XInputControllerWindows)
        {
            Debug.Log("Xbox");
        }
        else if (context.control.device is UnityEngine.InputSystem.DualShock.DualShockGamepad)
        {
            Debug.Log("Playstation");
        }
    }

    private void Update()
    {
        //Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        //grounded = Physics.CheckCapsule(transform.position, transform.position - new Vector3(0,(playerHeight * 0.25f + 0.2f),0),radius, whatIsGround);

        SpeedControl();

        //Handle Drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //Limit Velocity
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void ResetJump()
    {
        //readyToJump = true;
    }

    private void FixedUpdate()
    {
        
    }
}
