using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    //Reference to the input actions created under PlayerInputActions and Rigidbody component
    private PlayerInputActions inputActions;
    Rigidbody rb;

    //Variables for move, look and jump inputs
    private Vector2 move;
    private Vector2 look;
    private bool jump;
    private bool crouch;

    //Variables to adjust player movement speed, look speed, jump force, crouch and jump cooldown
    public float moveSpeed = 5f;
    public float lookSpeed = 1f; //Sensitivey for Camera movement 
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    public float crouchSpeed = 2.5f;  //Reduced speed when crouching
    public float crouchHeight = 1f;   //Height of the player when crouching
    public float standingHeight = 2f; // eight of the player when standing
    public float groundDrag;

    //Camera references
    private Transform cameraTransform;
    //private Camera playerCamera;
    private float cameraVertical = 0f;  //Vertical rotation
    private float cameraHorizontal = 0f;    //Horizontal rotation
    [SerializeField] GameObject playerCam;

    //Specify what the ground layer is for the items in inspector view and a layermask
    [SerializeField] private bool isGrounded;
    public LayerMask groundLayer;

    //Ready to jump detection variable and game object collider
    private bool readyToJump = true;
    new CapsuleCollider collider; //To change the player's height //private start to new at front so test

    //Multiplayer values
    PhotonView photonView;

    private void Awake()
    {
        //PhotonView check
        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            //Initialize the input actions, ridigbody and collider
            inputActions = new PlayerInputActions();
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
            playerCam.SetActive(true);

            //Locking camera
            Cursor.lockState = CursorLockMode.Locked; //Middle of screen lock

            // Get the main camera's transform
            cameraTransform = Camera.main.transform;

            //Setup the move action to update once the input for the move vector is detected and reset the move vector (ctx = context)
            inputActions.Player.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            inputActions.Player.Move.canceled += ctx => move = Vector2.zero;

            //Setup the look action to update once the input for the look vector is detected and reset the look vector
            inputActions.Player.Look.performed += ctx => look = ctx.ReadValue<Vector2>();
            inputActions.Player.Look.canceled += ctx => look = Vector2.zero;

            //Setup the jump action to update once the input for the jump variable is detected by being set to true and reset the jump by setting to false
            inputActions.Player.Jump.performed += ctx => jump = true;
            inputActions.Player.Jump.canceled += ctx => jump = false;

            //Crouch input
            inputActions.Player.Crouch.performed += ctx => crouch = true;
            inputActions.Player.Crouch.canceled += ctx => crouch = false;
        }
        else
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        // Enable the Player input action map
        inputActions.Player.Enable();
    }

    //private void OnDisable()
    //{
    //     // Disable the Player input action map
    //     inputActions.Player.Disable();
    //}

    private void Update()
    {
        // Handling movement and look methods
        Move();
        SpeedControl();
        Look();

        //Check if player is on the ground
        isGrounded = IsGrounded();

        //Jump if player is on ground and jump input pressed
        if (jump && readyToJump && isGrounded)
        {
           Jump();
           readyToJump = false;
           Invoke(nameof(ResetJump), jumpCooldown); // Cooldown before next jump
        }

        HandleCrouch(); // Handle crouch state

        //Handle Drag
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
    }

    private void Move()
    {
        //Get the camera positons
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        //Normalize for consistent movement speed
        cameraForward.Normalize();
        cameraRight.Normalize();

        //Movement vector based on the input and camera direction
        Vector3 movement = cameraForward * move.y + cameraRight * move.x;

        //Use crouch speed if crouching
        float speed = crouch ? crouchSpeed : moveSpeed; 

        //Checking ground state
        if (isGrounded)
        {
            rb.AddForce(movement.normalized * moveSpeed * 5f, ForceMode.Force);// Original 10f
        }
        else if (!isGrounded)
        {
            rb.AddForce(movement.normalized * moveSpeed * 5f * airMultiplier, ForceMode.Force);
        }
    }

    private void Look()
    {
        // Update the camera's horizontal rotation (yaw)
        cameraHorizontal += look.x * lookSpeed;

        // Update the camera's vertical rotation (pitch) and clamp it
        cameraVertical -= look.y * lookSpeed;
        cameraVertical = Mathf.Clamp(cameraVertical, -90f, 90f);

        // Apply the updated rotations to the camera
        cameraTransform.localEulerAngles = new Vector3(cameraVertical, cameraHorizontal, 0f);
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

    private void Jump()
    {
        //Vertical velocity set for jumping
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);//Edit to be able to jump whilst moving (old GetComponent<Rigidbody>().velocity)
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    //Jump can occur again
    private void ResetJump()
    {
        readyToJump = true;
    }

    private bool IsGrounded()
    {
        //How far the player is from the ground
        float raycastDistance = 0.65f;

        //Check to see if the player is hitting the ground
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundLayer);
        //return Physics.CheckSphere(groundCheck.position, raycastDistance, groundLayer);
    }

    // Call this function to lock the jump for a short time
    public void LockJumpForDuration(float duration)
    {
        StartCoroutine(LockJump(duration));
    }

    private IEnumerator LockJump(float duration)
    {
        readyToJump = false;
        yield return new WaitForSeconds(duration);
        readyToJump = true;
    }

    private void HandleCrouch()
    {
        if (crouch)
        {
            // Reduce the height of the player when crouching
            collider.height = crouchHeight;
        }
        else
        {
            // Reset the height of the player when not crouching
            collider.height = standingHeight;
        }
    }
}
