using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class WeaponSwap : MonoBehaviour
{
    //Current Weapon 
    public int selectedWeapon = 0;

    //Input Action Asset reference
    private PlayerInputActions inputActions;

    //input variables
    private bool swapGP;
    private Vector2 mouseScroll;

    //Tracking variable
    private bool swappedGP;
    PhotonView photonView;

    private void Awake()
    {
        //PhotonView check
        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            //Initialize the input actions, ridigbody and collider
            inputActions = new PlayerInputActions();

            //Swapping with mouse scroll wheel
            inputActions.Player.Scroll.performed += ctx => mouseScroll = ctx.ReadValue<Vector2>();
            inputActions.Player.Scroll.canceled += ctx => mouseScroll = Vector2.zero;

            //Swapping with Y on controller
            inputActions.Player.Swap.performed += ctx => swapGP = true;
            inputActions.Player.Swap.canceled += ctx => swapGP = false;
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

    private void OnDisable()
    {
        // Disable the Player input action map
        inputActions.Player.Disable();
    }

    private void Update()
    {
        int previousWeapon = selectedWeapon;

        // Mouse scroll wheel forward direction
        if (mouseScroll.y > 0f)
        {
            NextWeapon();
        }

        // Mouse scroll wheel backward direction
        if (mouseScroll.y < 0f)
        {
            PreviousWeapon();
        }

        // Controller swap input: register only one swap per button press
        if (swapGP && !swappedGP)
        {
            NextWeapon();
            swappedGP = true;  // Ensure we only swap once
        }
        else if (!swapGP)
        {
            swappedGP = false; // Reset flag when button is released
        }

        // Change weapon if selectedWeapon has been updated
        if (previousWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    //Method to switch to the next weapon
    void NextWeapon()
    {
        if (selectedWeapon >= transform.childCount - 1)
        {
            selectedWeapon = 0;
        }
        else
        {
            selectedWeapon++;
        }
    }

    //Method to switch to the previous weapon
    void PreviousWeapon()
    {
        if (selectedWeapon <= 0)
        {
            selectedWeapon = transform.childCount - 1;
        }
        else
        {
            selectedWeapon--;
        }
    }

    void SelectWeapon()
    {
        int i = 0;

        //Loops through the gun game objects under the weapon holder
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
}
