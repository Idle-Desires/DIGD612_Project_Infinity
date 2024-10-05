using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    private PlayerController controller; // Reference to the PlayerController script
    private PlayerInputActions inputActions; // Input actions generated by the Unity Input System
    public Transform orientation; // Orientation transform (used to determine forward direction)
    public Transform playerCam; // Camera transform (used for camera-based dashing direction)
    private Rigidbody rb; // Rigidbody for applying forces during dash

    [Header("Dashing")]
    public float dashForce; // The force applied during a dash
    public float dashUpwardForce; // Additional upward force applied during a dash
    public float maxDashYSpeed; // Maximum Y velocity during a dash
    public float dashDuration; // Duration for how long the dash lasts

    [Header("Settings")]
    public bool useCameraForward = true; // Determines if the dash direction is based on camera direction
    public bool allowAllDirections = true; // Allow dashing in any direction
    public bool disableGravity = false; // Disable gravity during the dash
    public bool resetVel = true; // Reset velocity before applying the dash force

    [Header("Cooldown")]
    public float dashCd; // Dash cooldown time in seconds
    private float dashCdTimer; // Timer to track cooldown progress

    // Input values for movement
    private Vector2 moveInput; // Stores the movement input from the player

    private void Awake()
    {
        // Initialize the input actions
        inputActions = new PlayerInputActions();

        // Bind the dash action to the Dash() method
        inputActions.Player.Dash.performed += ctx => Dash();

        // Bind the movement input and store it in moveInput
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero; // Reset movement when input stops
    }

    private void OnEnable()
    {
        // Enable the input actions when the script is active
        inputActions.Enable();
    }

    private void OnDisable()
    {
        // Disable the input actions when the script is inactive
        inputActions.Disable();
    }

    private void Start()
    {
        // Get references to the Rigidbody and PlayerController components
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        // If the dash cooldown timer is greater than 0, reduce it by the time passed
        if (dashCdTimer > 0)
        {
            dashCdTimer -= Time.deltaTime;
        }
    }

    // Method to execute the dash ability
    private void Dash()
    {
        // If the cooldown timer is still active, prevent dashing
        if (dashCdTimer > 0)
        {
            return;
        }
        else
        {
            // Reset the cooldown timer to start the cooldown
            dashCdTimer = dashCd;
        }

        // Set dashing state to true and update the player's max Y speed during dash
        controller.dashing = true;
        controller.maxYSpeed = maxDashYSpeed;

        // Determine whether to dash based on camera direction or player orientation
        Transform forwardT;
        if (useCameraForward)
        {
            forwardT = playerCam; // Dash in the direction the camera is facing
        }
        else
        {
            forwardT = orientation; // Dash based on player orientation (ignores camera)
        }

        // Calculate the dash direction using the chosen forward transform
        Vector3 direction = GetDirection(forwardT);

        // Combine dash force and upward force
        Vector3 forceToApply = direction * dashForce + orientation.up * dashUpwardForce;

        // Disable gravity during the dash if the setting is enabled
        if (disableGravity)
        {
            rb.useGravity = false;
        }

        // Store the force to be applied later (to delay force application for stability)
        delayedForceToApply = forceToApply;
        Invoke(nameof(DelayedDashForce), 0.025f); // Apply force after a short delay

        // Reset dash effects after dash duration ends
        Invoke(nameof(ResetDash), dashDuration);
    }

    // Variable to store the dash force that will be applied
    private Vector3 delayedForceToApply;

    // Method to apply the stored dash force after a delay
    private void DelayedDashForce()
    {
        // Optionally reset the player's velocity before applying the dash force
        if (resetVel)
        {
            rb.velocity = Vector3.zero;
        }

        // Apply the calculated dash force to the Rigidbody
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    // Method to reset the dash after it finishes
    private void ResetDash()
    {
        // Reset dashing state and max Y speed to their default values
        controller.dashing = false;
        controller.maxYSpeed = 0;

        // Re-enable gravity if it was disabled during the dash
        if (disableGravity)
        {
            rb.useGravity = true;
        }
    }

    // Method to calculate the dash direction based on player input and orientation
    private Vector3 GetDirection(Transform forwardT)
    {
        // Get the horizontal and vertical input from the Input System
        float horizontalInput = moveInput.x;
        float verticalInput = moveInput.y;

        // Create a direction vector based on input
        Vector3 direction = new Vector3();

        // If dashing in all directions is allowed, calculate based on input
        if (allowAllDirections)
        {
            direction = forwardT.forward * verticalInput + forwardT.right * horizontalInput;
        }
        else
        {
            direction = forwardT.forward; // Only dash forward
        }

        // If no input is provided, default to forward direction
        if (verticalInput == 0 && horizontalInput == 0)
        {
            direction = forwardT.forward;
        }

        // Return the normalized direction vector (ensures consistent magnitude)
        return direction.normalized;
    }
}
