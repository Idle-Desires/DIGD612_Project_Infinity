using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Photon.Pun;
using UnityEngine.InputSystem.HID;

public class WeaponsController : MonoBehaviour
{
    //Reference to the input actions created under PlayerInputActions
    private PlayerInputActions inputActions;
    [SerializeField] private PlayerVariables playerStats;

    private EnemyStats enemy;
    private PlayerVariables otherPlayer;
    //public GameObject hitVFX;

    [Header("Recoil Settings")]
    //[Range(0f, 1f)]
    //public float recoilPercent = 0.3f;
    [Range(0f, 2f)]
    public float recoverPercent = 0.7f;
    [Space]
    public float recoilUp = 1f;
    public float recoilBack = 0f;
    private Vector3 originalPosition;
    private Vector3 recoilVelocity = Vector3.zero;
    private bool recoiling;
    public bool recovering;
    private float recoilLength;
    private float recoverLength;

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
    public int damage = 10; //use to be float
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

            originalPosition = transform.localPosition;
            recoilLength = 0;
            recoverLength = 1/fireRate * recoverPercent;

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

        if (recoiling) 
        { 
            Recoil();
        }

        if (recovering)
        {
            Recovering();
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
        bulletsLeftInMagazine--; // Use one bullet from the magazine
        recoiling = true;
        recovering = false;

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit rayHit, range))
        {
            //PhotonNetwork.Instantiate(hitVFX.name, rayHit.point, Quaternion.identity);
            EnemyStats enemy = rayHit.transform.GetComponent<EnemyStats>();
            //PlayerVariables otherPlayer = rayHit.transform.GetComponent<PlayerVariables>(); //PlayerStats otherPlayer = rayHit.transform.GetComponent<PlayerStats>();

            // Apply damage if we hit an enemy or another player
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                //rayHit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
            }
            //else if (otherPlayer != null)
            //{
            //    otherPlayer.TakeDamage(damage, photonView);
            //    //rayHit.transform.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, damage);
            //}

            ////Damage to other players
            //if (photonView != null)
            //{
            //    PlayerVariables hitPlayerVariables = photonView.GetComponent<PlayerVariables>();
            //    if (hitPlayerVariables != null)
            //    {
            //        hitPlayerVariables.TakeDamage(damage, this.photonView);
            //    }
            //}

            PhotonView hitPhotonView = rayHit.collider.GetComponent<PhotonView>();
            if (hitPhotonView != null && !hitPhotonView.IsMine)
            {
                PlayerVariables hitPlayerVariables = hitPhotonView.GetComponent<PlayerVariables>();
                if (hitPlayerVariables != null)
                {
                    hitPlayerVariables.TakeDamage(damage, this.photonView);
                }
            }

            // Apply impact force if the object has a Rigidbody
            if (rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForce(-rayHit.normal * impactForce);
            }
        }

        // Raycast to detect if we hit something
        //if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit rayHit, range))
        //{
        //    EnemyStats enemy = rayHit.transform.GetComponent<EnemyStats>();
        //    PlayerStats otherPlayer = rayHit.transform.GetComponent<PlayerStats>();

        //    // Apply damage if we hit an enemy or another player
        //    if (enemy != null)
        //    {
        //        enemy.TakeDamage(damage);
        //    }
        //    else if (otherPlayer != null)
        //    {
        //        otherPlayer.TakeDamage(damage);
        //    }

        //    // Apply impact force if the object has a Rigidbody
        //    if (rayHit.rigidbody != null)
        //    {
        //        rayHit.rigidbody.AddForce(-rayHit.normal * impactForce);
        //    }
        //}
    }

    void ProjectileShot()
    {
        ammoDisplay.SetText("(" + bulletsLeftInMagazine + " / " + magSize + ") " + ammoTotal);
        Debug.Log("Projectile Test");

        recoiling = true;
        recovering = false;

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

    void Recoil()
    {
        Vector3 finalPosition = new Vector3(originalPosition.x, originalPosition.y + recoilUp, originalPosition.z - recoilBack);

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoilLength);

        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = true;
        }
    }

    void Recovering()
    {
        Vector3 finalPosition = originalPosition;

        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalPosition, ref recoilVelocity, recoverLength);

        if (transform.localPosition == finalPosition)
        {
            recoiling = false;
            recovering = false;
        }
    }
}
