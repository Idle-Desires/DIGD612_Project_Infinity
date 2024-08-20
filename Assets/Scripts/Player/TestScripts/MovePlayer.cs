using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;//Input system use

public class MovePlayer : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    private Vector2 moveInputValue;

    private void OnMove(InputValue value)
    {
        moveInputValue = value.Get<Vector2>();
    }

    private void MovementLogic()
    {
        Vector2 result = moveInputValue * speed * Time.fixedDeltaTime;
        rb.velocity = result;
    }

    private void FixedUpdate()
    {
        MovementLogic();
    }
}
