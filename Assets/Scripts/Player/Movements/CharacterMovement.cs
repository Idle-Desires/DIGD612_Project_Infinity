using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    //PlayerInput playerInput;
    //private InputAction moveAction;
    public InputControls input = null;
    //MasterControls moveAction;

    private Vector2 moveVector = Vector2.zero;
    private Rigidbody rb = null;
    //private float moveSpeed = 10f;
    Vector3 moveDirection;

    private void Awake()
    {
        input = new InputControls();
        rb = GetComponent<Rigidbody>();

        //moveAction = inputControls.FindActionMap(MasterControls).FindAction(Movement);

        rb.freezeRotation = true;
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        input.Enable();
        input.MasterControls.Movement.performed += MovementPerformed;//GetKeyDown
        input.MasterControls.Movement.canceled += MovementCanceled;//GetKeyUp
    }

    // Update is called once per frame
    private void OnDisable()
    {
        input.Disable();
        input.MasterControls.Movement.performed -= MovementPerformed;//GetKeyDown
        input.MasterControls.Movement.canceled -= MovementCanceled;//GetKeyUp

    }

    //Movement button functionality for the up key
    private void MovementCanceled(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        moveVector = Vector2.zero;
    }

    //Movement button functionality for the down key
    private void MovementPerformed(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());
        moveVector = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()//best for RB
    {
        Debug.Log(moveVector);
        //rb.velocity = moveVector * moveSpeed;

        MovePlayer();
    }

    private void MovePlayer()
    {
        //Vector3 playerVelocity = new Vector3(moveInput.x * moveSpeed, rb.velocity.y, moveInput.y * moveSpeed);

        //Vector2 direction = moveAction.ReadValue<Vector2>();
        //transform.position += new Vector3(direction.x, 0, direction.y) * moveSpeed * Time.deltaTime;
    }
}
