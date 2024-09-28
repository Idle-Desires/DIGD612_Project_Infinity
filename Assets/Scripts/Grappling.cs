using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Grappling : MonoBehaviour
{
    //References
    private PlayerController controller;
    private PlayerInputActions inputActions;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr;

    //Grappling
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private Vector3 grapplePoint;

    //cooldown
    public float grapplingCd;
    private float grapplingCdTimer;

    private bool grappling;

    void Awake()
    {
        // Initialize the Input Actions
        inputActions = new PlayerInputActions();

        // Bind the Grapple action
        inputActions.Player.Grapple.performed += ctx => StartGrapple();
    }

    void OnEnable()
    {
        // Enable the Player action map
        inputActions.Player.Enable();
    }

    void OnDisable()
    {
        // Disable the Player action map
        inputActions.Player.Disable();
    }

    private void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Update()
    {
        //if (Input.GetKeyUp(KeyCode.G)) 
        //{ 
        //    StartGrapple();
        //}

        if (grapplingCdTimer > 0) 
        { 
            grapplingCdTimer -= Time.deltaTime;
        }
    }

    private void LateUpdate()
    {
        if (grappling) 
        { 
            lr.SetPosition(0, gunTip.position);
        }
    }

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0)
        {
            return;
        }

        grappling = true;

        controller.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else 
        { 
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;  
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        controller.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        controller.JumpToPosition(grapplePoint, highestPointOnArc);

        Invoke(nameof(StopGrapple), 1f);
    }

    public void StopGrapple()
    {
        controller.freeze = false;

        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }

    public bool IsGrappling()
    {
        return grappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
