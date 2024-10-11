using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Photon.Pun;
using TMPro;

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
    private bool scoreboard;

    //Variables to adjust player movement speed, look speed, jump force, crouch and jump cooldown
    public float moveSpeed = 5f;
    public float lookSpeed = 1f; //Sensitivey for Camera movement 
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    public float crouchSpeed = 2.5f;  //Reduced speed when crouching
    public float crouchHeight = 1f;   //Height of the player when crouching
    public float standingHeight = 2f; // eight of the player when standing
    private bool isCrouching = false;
    public float groundDrag;

    //Dashing
    public float dashSpeed;
    public bool dashing;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private bool keepMomentum;
    public float dashSpeedChangeFactor;
    public float maxYSpeed;

    //Camera references
    private Transform cameraTransform;
    //private Camera playerCamera;
    private float cameraVertical = 0f;  //Vertical rotation
    private float cameraHorizontal = 0f;    //Horizontal rotation
    public GameObject playerCam; //[SerializeField]
    //public GameObject weaponHolder;

    //Specify what the ground layer is for the items in inspector view and a layermask
    [SerializeField] private bool isGrounded;
    public LayerMask groundLayer;

    //Ready to jump detection variable and game object collider
    private bool readyToJump = true;
    private bool doubleJumpUsed = false; // Track double jump usage
    private float doubleJumpPower = 12f;
    new CapsuleCollider collider; //To change the player's height //private start to new at front so test

    // Slide variables
    public float slideForce = 10f;
    public float slideDuration = 0.5f;
    public float slideCooldown = 1f;
    public float slideSpeedThreshold = 3f;  // Speed threshold to trigger a slide
    private bool isSliding = false;
    private bool canSlide = true;

    //Multiplayer values
    PhotonView photonView;
    //public TextMeshProUGUI healthDisplay;
    private int playerID; // Player's unique ID
    //private float playerHealth = 50f; // Variable to store the player's health
    public GameObject scoreboardUI;

    //Grapple Variables
    public bool freeze;
    public bool activeGrapple;

    private void Awake()
    {
        //PhotonView check
        photonView = GetComponent<PhotonView>();
        //weaponHolder.SetActive(true);

        if (photonView.IsMine)
        {
            //Initialize the input actions, ridigbody and collider
            inputActions = new PlayerInputActions();
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
            playerCam.SetActive(true);
            playerID = photonView.Owner.ActorNumber;
            //playerHealth = GameManager.instance.GetPlayerHealth(playerID);

            // Optionally, initialize player's health from the GameManager
            //playerHealth = GameManager.instance.GetPlayerHealth(playerID);

            // Set initial health display
            //UpdateHealthDisplay();

            //healthDisplay.SetText("Health: " + playerHealth.ToString());

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

            //Crouch input
            //inputActions.Player.Scoreboard.performed += ctx => scoreboard = true;
            //inputActions.Player.Scoreboard.canceled += ctx => scoreboard = false;
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
    //    // Disable the Player input action map
    //    inputActions.Player.Disable();
    //}

    private void Update()
    {
        // Handling movement and look methods
        Move();
        SpeedControl();
        Look();

        // Update playerHealth during gameplay
        //playerHealth = GameManager.instance.GetPlayerHealth(playerID);

        // Update the UI text to display the player's health
        //UpdateHealthDisplay();

        if (freeze) //freeze whilst grappling
        {
            //moveSpeed = 0;
            rb.velocity = Vector3.zero;
        }

        //Check if player is on the ground
        isGrounded = IsGrounded();

        if (isGrounded)
        {
            doubleJumpUsed = false; // Reset double jump when grounded
        }

        // Handle crouching, but don't crouch slide if already crouching
        HandleCrouch();

        // Check for jump conditions
        if (jump && readyToJump)
        {
            if (isGrounded)
            {
                Jump(jumpForce); // Regular jump
                readyToJump = false;
                Invoke(nameof(ResetJump), jumpCooldown);
            }
            else if (!doubleJumpUsed) // Double jump condition
            {
                Jump(doubleJumpPower); // Perform double jump with a higher force
                doubleJumpUsed = true;
                readyToJump = false;
                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        //// Handle sliding: only trigger when the player starts crouching WHILE moving
        //if (move != Vector2.zero && crouch && !isCrouching && canSlide && !isSliding)
        //{
        //    // Start sliding if player is moving and crouch is pressed
        //    StartCoroutine(Slide());
        //}
        //else if (crouch && move == Vector2.zero && !isSliding)
        //{
        //    // If crouching but not moving, just crouch (no slide)
        //    HandleCrouch();
        //}

        if(move != Vector2.zero && crouch && !isCrouching && canSlide && !isSliding)
        {
            // Start sliding if player is moving and crouch is pressed
            StartCoroutine(Slide());
        }
        else if (crouch && move == Vector2.zero && !isSliding)
        {
            // If crouching but not moving, just crouch (no slide)
            HandleCrouch();
        }

        //Handle Drag
        if (isGrounded && !activeGrapple && !dashing)
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
        if (activeGrapple)
        {
            return;
        }
        else if (dashing)
        {
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

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
            rb.AddForce(movement.normalized * moveSpeed * 10f, ForceMode.Force);// Original 10f
        }
        else if (!isGrounded)
        {
            rb.AddForce(movement.normalized * moveSpeed * 5f * airMultiplier, ForceMode.Force);
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (desiredMoveSpeedHasChanged) 
        {
            if (keepMomentum) 
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else 
            {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        //Debug.Log(moveSpeed);
        //Debug.Log(lastDesiredMoveSpeed);
        //Debug.Log(desiredMoveSpeed);
    }

    private float speedChangeFactor;
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
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
       //Debug.Log(moveSpeed);
        if (activeGrapple) 
        {
            return;
        }

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //Limit Velocity
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }

        // limit y vel
        if (maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
            rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
    }

    private void Jump(float force)
    {
        //Vertical velocity set for jumping
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);//Edit to be able to jump whilst moving (old GetComponent<Rigidbody>().velocity)
        //rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        rb.AddForce(transform.up * force, ForceMode.Impulse);
    }

    //Jump can occur again
    private void ResetJump()
    {
        readyToJump = true;
    }

    private bool IsGrounded()
    {
        //How far the player is from the ground
        //float raycastDistance = 0.65f;
        float raycastDistance = 1.2f; //Fixing the height of the capsule 

        //Check to see if the player is hitting the ground
        return Physics.Raycast(transform.position, Vector3.down, raycastDistance, groundLayer);
        //return Physics.CheckSphere(groundCheck.position, raycastDistance, groundLayer);
    }

    private IEnumerator Slide()
    {
        isSliding = true;
        canSlide = false;

        // Log when the player starts sliding
        Debug.Log("Player is crouch sliding!");

        // Apply slide force
        rb.AddForce(transform.forward * slideForce, ForceMode.Impulse);

        // Lower the player's height (optional for crouching animation)
        collider.height = crouchHeight;

        // Wait for the slide duration
        yield return new WaitForSeconds(slideDuration);

        // Log when the player finishes sliding
        Debug.Log("Player has stopped crouch sliding!");

        // Reset height
        collider.height = standingHeight;
        isSliding = false;

        // Slide cooldown before the player can slide again
        yield return new WaitForSeconds(slideCooldown);
        canSlide = true;
    }
    private void HandleCrouch()
    {
        // Only allow toggling crouch if the player is not sliding
        if (crouch && move == Vector2.zero && !isSliding)
        {
            if (isCrouching)
            {
                // Player stands up
                collider.height = standingHeight;
                isCrouching = false;
            }
            else
            {
                // Player crouches
                collider.height = crouchHeight;
                isCrouching = true;
            }
        }
    }

    //public void TakeDamage(float damageAmount)
    //{
    //    GameManager.instance.TakeDamage(playerID, damageAmount);
    //}

    // Method to update the health display text
    //public void UpdateHealthDisplay()
    //{
    //    if (healthDisplay != null)
    //    {
    //        healthDisplay.SetText("Health: " + playerHealth.ToString());
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Health display UI component is not assigned!");
    //    }
    //}

    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);

        Invoke(nameof(SetVelocity), 0.1f);
        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}
