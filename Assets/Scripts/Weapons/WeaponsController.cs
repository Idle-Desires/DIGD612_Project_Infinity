using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Photon.Pun;

public class WeaponsController : MonoBehaviour
{
    //Reference to the input actions created under PlayerInputActions
    private PlayerInputActions inputActions;
    [SerializeField] private PlayerStats playerStats;

    private EnemyStats enemy;
    private PlayerStats otherPlayer;

    //Type of gun being held
    public string gunName;

    //Bullet
    private GameObject currentBullet;       // Reference to the current bullet
    public GameObject bulletPrefab;    // The bullet prefab
    public Transform bulletSpawnPoint; // The point where bullets will spawn
    public float bulletSpeed = 60f;    // Speed of the bullet

    //Input
    private bool attack;
    private bool reload;

    //Gun vaiables
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 5f;
    private float nextTimeToFire = 0f;

    //Gun variables
    //public int bulletsPerShot; //bullets per tap of the mouse or attack button
    //public float timeBetweenShooting;
    //public float reloadTime = 1f; 
    //public float timeBetweenShots;
    //public int bulletsLeft;
    //int bulletsShot;

    // Magazine system variables
    public int magSize = 20;  // Number of bullets per magazine
    public int ammoTotal = 60;     // Total number of bullets player has
    public float reloadTime = 1f;  // Time it takes to reload

    // Ammo tracking
    public int bulletsLeftInMagazine;  // Bullets left in the current magazine
    public bool reloading = false;     // Whether the player is reloading

    //Booleans to determine true or false settings
    bool shooting;
    // Shooting mode tracking
    //private ShootingMode currentShootingMode; // Track the current shooting mode

    //References to other game objects in the scene
    public RaycastHit rayHit;

    //Graphics
    public TextMeshProUGUI ammoDisplay;
    //public TextMeshProUGUI deathDisplay;

    //Camera
    public Camera fpsCam;
    PhotonView photonView;

    public enum ShootingMode { HitScan, Projectile }
    private ShootingMode currentShootingMode;  // Track the current shooting mode

    //Initialize at the start of the game
    private void Awake()
    {
        //PhotonView check
        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            ammoTotal = 60;
            //Debug.Log(bulletsLeftInMagazine + " & " + magSize);
            bulletsLeftInMagazine = magSize;
            ammoDisplay.SetText("(" + bulletsLeftInMagazine + " / " + magSize + ") " + ammoTotal);

            // Initialize input actions
            inputActions = new PlayerInputActions();

            // Attack input mapped to single-shot shooting
            inputActions.Player.Attack.performed += ctx => attack = true;
            inputActions.Player.Attack.canceled += ctx => attack = false;

            // Reload input
            inputActions.Player.Reload.performed += ctx => Reload();
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
        reloading = false;
    }

    //private void OnDisable()
    //{
    //    // Disable the Player input action map
    //    inputActions.Player.Disable();
    //}

    //Checking states for shooting or reloading the gun
   public void Update()
    {
        //Set ammo display 
        if (ammoDisplay != null)
        {
            ammoDisplay.SetText("(" + bulletsLeftInMagazine + " / " + magSize + ") " + ammoTotal);
        }

        // Check for reload input and magazine state
        if (reload && bulletsLeftInMagazine < magSize && !reloading)
        {
            Reload();
            return;
        }

        // Automatically reload if magazine is empty and player still has ammo
        if (bulletsLeftInMagazine == 0 && ammoTotal > 0 && !reloading)
        {
            Reload();
        }

        if(attack == true)
        {
            ShootOnce();
        }
    }

    // Method to shoot only once when the mouse is clicked
    public void ShootOnce()
    {
        // Check if enough time has passed, magazine has bullets, and not reloading
        if (Time.time >= nextTimeToFire && bulletsLeftInMagazine > 0 && !reloading)
        {
            nextTimeToFire = Time.time + 1f / fireRate;

            if (currentShootingMode == ShootingMode.HitScan)
            {
                Shoot(); // Call hitscan method
            }
            else if (currentShootingMode == ShootingMode.Projectile)
            {
                // Only call ProjectileShot if we are in projectile mode
                ProjectileShot();
            }
        }
    }

    public void SetShootingMode(ShootingMode mode)
    {
        if (mode == ShootingMode.HitScan && currentBullet != null)
        {
            Destroy(currentBullet); // Destroy the active projectile if it exists
            currentBullet = null; // Reset currentBullet
        }

        currentShootingMode = mode; // Update shooting mode
    }

    //What happens when the player shots the gun
    void Shoot()
    {
        // Reset currentBullet as we are not using projectiles
        //currentBullet = null;

        bulletsLeftInMagazine--; // Use one bullet from the magazine

        // If we were previously in projectile mode, destroy the current bullet (to stop it from being in the scene)
        //if (currentBullet != null)
        //{
        //    Destroy(currentBullet);  // Stop any active projectile bullets
        //}

        // Raycast to detect if we hit something
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit rayHit, range))
        {
            EnemyStats enemy = rayHit.transform.GetComponent<EnemyStats>();
            PlayerStats otherPlayer = rayHit.transform.GetComponent<PlayerStats>();

            // Apply damage if we hit an enemy or another player
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            else if (otherPlayer != null)
            {
                otherPlayer.TakeDamage(damage);
            }

            // Apply impact force if the object has a Rigidbody
            if (rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForce(-rayHit.normal * impactForce);
            }
        }

        // Reset currentBullet as we are not using projectiles
        //currentBullet = null;
    }

    void ProjectileShot()
    {
        ammoDisplay.SetText("(" + bulletsLeftInMagazine + " / " + magSize + ") " + ammoTotal);
        Debug.Log("Projectile Test");
        if (currentBullet != null)
        {
            Destroy(currentBullet, 0.5f); // Destroy existing projectile if any
        }

        bulletsLeftInMagazine--; // Use one bullet from the magazine

        // Instantiate the bullet at the spawn point
        currentBullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Get the Rigidbody component and apply force to shoot the bullet forward
        Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(bulletSpawnPoint.forward * bulletSpeed, ForceMode.Impulse);
        }
    }

    //Changes the state and calls method to reset variables
    public void Reload() //private void
    {
        if (ammoTotal > 0 && !reloading) // Check if player has enough ammo to reload
        {
            reloading = true;
            Invoke("ReloadFinished", reloadTime);  // Delay reloading to simulate reload time
        }
    }

    //Resets variables
    private void ReloadFinished()
    {
        int bulletsToReload = magSize - bulletsLeftInMagazine;  // How many bullets we need to fill the magazine

        if (ammoTotal >= bulletsToReload)
        {
            // If enough ammo remains, reload full magazine
            ammoTotal -= bulletsToReload;
            bulletsLeftInMagazine = magSize;
        }
        else
        {
            // If not enough ammo, fill the magazine with whatever ammo is left
            bulletsLeftInMagazine += ammoTotal;
            ammoTotal = 0;  // Player is out of ammo
        }

        reloading = false;  // Reloading is complete
    }

    //If the bullet hits anything with a RB it will be destroyed
    void OnCollisionEnter(Collision collision)//for 3D RB add 2D for other rb option
    {
        Destroy(currentBullet, 0.5f);
    }
}
