using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class WeaponsController : MonoBehaviour
{
    //Reference to the input actions created under PlayerInputActions and Rigidbody component
    private PlayerInputActions inputActions;

    //Weapon and player related variables
    private bool attack;
    private bool reload;

    //Gun vaiables
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 5f;
    private float nextTimeToFire = 0f;

    //Reticle variable
    public Camera fpscam;

    //Gun variables
    public int magazineSize;
    public int bulletsPerShot; //bullets per tap of the mouse or attack button
    public float timeBetweenShooting;
    public float reloadTime;
    public float timeBetweenShots;
    int bulletsLeft;
    int bulletsShot;

    //Booleans to determine true or false settings
    bool shooting;
    //bool readyToShoot;
    bool reloading;

    //References to other game objects in the scene
    public RaycastHit rayHit;

    //Initialize at the start of the game
    private void Awake()
    {
        bulletsLeft = magazineSize;
        //readyToShoot = true;

        //Initialize the input actions, ridigbody and collider
        inputActions = new PlayerInputActions();

        //Attack input
        inputActions.Player.Attack.performed += ctx => attack = true;
        inputActions.Player.Attack.canceled += ctx => attack = false;

        //Reload input
        inputActions.Player.Reload.performed += ctx => reload = true;
        inputActions.Player.Reload.canceled += ctx => reload = false;
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

    //Checking states for shooting or reloading the gun
   public void Update()
    {
        if (reload && bulletsLeft < magazineSize && !reloading)
        {
            Debug.Log("Reloading");
            Reload();
        }

        if (attack && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            bulletsShot = bulletsPerShot;
            Shoot();
        }
    }

    //What happens when the player shots the gun
    void Shoot()
    {
        //Prevent more shooting whilst shooting
        //readyToShoot = false;
        Debug.Log(bulletsLeft + " / " + magazineSize);

        if (Physics.Raycast(fpscam.transform.position, fpscam.transform.forward, out rayHit, range))
        {
            Debug.Log(rayHit.transform.name);

            EnemyStats enemy = rayHit.transform.GetComponent<EnemyStats>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForce(-rayHit.normal * impactForce);
            }
        }

        //Keeping track of how many bullets the user has left
        bulletsLeft--;
        bulletsShot--;

        //Delay to calling the reset function for shooting
        Invoke("ResetShot", timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    //Reset state to be able to shoot. Will be looked into for improving the guns
    //private void ResetShot()
    //{
    //    readyToShoot = true;
    //}

    //Changes the state and calls method to reset variables
    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    //Resets variables
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
