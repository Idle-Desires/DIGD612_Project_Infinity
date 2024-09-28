using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    [Header("Grappling Hook Settings")]
    public float maxDistance = 100f; // Maximum distance the hook can reach
    public float pullSpeed = 10f;    // Speed at which the player is pulled
    public LayerMask grappleableLayers; // Layers that the hook can attach to

    [Header("Hook Projectile")]
    public GameObject hookPrefab;     // Prefab for the grappling hook projectile
    public Transform gunTip;         // The point from where the hook is fired
    public LineRenderer lineRenderer; // Line renderer for the rope

    private SpringJoint springJoint;
    private Camera playerCamera;
    private Vector3 grapplePoint;
    private bool isGrappling = false;

    private PlayerInputActions inputActions;

    void Awake()
    {
        // Initialize the Input Actions
        inputActions = new PlayerInputActions();

        // Bind the Grapple action
        inputActions.Player.Grapple.performed += ctx => ToggleGrapple();
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

    void Start()
    {
        playerCamera = Camera.main;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            // Assign a rope material if desired
            // lineRenderer.material = ropeMaterial;
        }
    }

    void Update()
    {
        if (isGrappling)
        {
            DrawRope();
        }
    }

    void ToggleGrapple()
    {
        if (!isGrappling)
            StartGrapple();
        else
            StopGrapple();
    }

    // Rest of the code will go here
    void StartGrapple()
    {
        RaycastHit hit;
        Vector3 direction = playerCamera.transform.forward;

        if (Physics.Raycast(playerCamera.transform.position, direction, out hit, maxDistance, grappleableLayers))
        {
            grapplePoint = hit.point;
            isGrappling = true;

            // Instantiate the hook projectile (optional for visual purposes)
            if (hookPrefab != null && gunTip != null)
            {
                GameObject hook = Instantiate(hookPrefab, gunTip.position, Quaternion.identity);
                Rigidbody hookRb = hook.GetComponent<Rigidbody>();
                if (hookRb != null)
                {
                    hookRb.AddForce(direction * 1000); // Adjust force as needed
                }
                // Optionally, set the grapplingHook reference in the hook projectile
                // hook.GetComponent<HookProjectile>().grapplingHook = this;
            }

            // Create a SpringJoint to pull the player
            springJoint = gameObject.AddComponent<SpringJoint>();
            springJoint.autoConfigureConnectedAnchor = false;
            springJoint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            // Adjust the SpringJoint properties
            springJoint.maxDistance = distanceFromPoint * 0.8f;
            springJoint.minDistance = distanceFromPoint * 0.25f;

            springJoint.spring = 4.5f;
            springJoint.damper = 7f;
            springJoint.massScale = 4.5f;

            // Enable the line renderer
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
            }
        }
    }

    void DrawRope()
    {
        if (lineRenderer == null) return;

        Vector3 currentPosition = gunTip != null ? gunTip.position : transform.position;
        lineRenderer.SetPosition(0, currentPosition);
        lineRenderer.SetPosition(1, grapplePoint);
    }

    void StopGrapple()
    {
        isGrappling = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        if (springJoint)
        {
            Destroy(springJoint);
        }
    }

}
