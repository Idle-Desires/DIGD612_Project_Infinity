using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Photon.Pun;
using UnityEngine.InputSystem.HID;

public class WeaponsController : MonoBehaviour
{
    // Reference to the input actions created under PlayerInputActions
    private PlayerInputActions inputActions;

    [SerializeField] private PlayerVariables playerStats; // Assign via Inspector
    public LayerMask hitLayers;

    private EnemyStats enemy;
    private PlayerVariables otherPlayer;
    public GameObject hitVFX;

    [Header("Recoil Settings")]
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
    public int shooterViewID;

    //Input
    private bool attack;
    private bool reload;

    //Gun vaiables
    public int damage = 10; //use to be float
    public float range = 100f;
    public float fireRate = 15f;
    public float impactForce = 5f;
    private float nextTimeToFire = 0f;

    // Magazine system variables
    public int magSize = 20;  // Number of bullets per magazine
    public int ammoTotal = 60;     // Total number of bullets player has
    public float reloadTime = 1f;  // Time it takes to reload

    // Ammo tracking
    public int bulletsLeftInMagazine;  // Bullets left in the current magazine
    public bool reloading = false;     // Whether the player is reloading

    //Booleans to determine true or false settings
    bool shooting;

    //References to other game objects in the scene
    public RaycastHit rayHit;
    public enum ShootingMode { HitScan, Projectile }
    private ShootingMode currentShootingMode;  // Track the current shooting mode

    //Graphics
    public TextMeshProUGUI ammoDisplay;

    //Camera
    public Camera fpsCam;
    PhotonView photonView;

    //Initialize at the start of the game
    private void Awake()
    {
        //PhotonView check
        photonView = GetComponent<PhotonView>();

        if (photonView.IsMine)
        {
            //Initialize ammo
            ammoTotal = 60;
            bulletsLeftInMagazine = magSize;
            UpdateAmmoDisplay();

            //Store the original position of the gun for recoil calculations
            originalPosition = transform.localPosition;
            recoilLength = 0;
            recoverLength = 1 / fireRate * recoverPercent;

            //Initialize input actions
            inputActions = new PlayerInputActions();

            //Map attack input for shooting
            inputActions.Player.Attack.performed += ctx => attack = true;
            inputActions.Player.Attack.canceled += ctx => attack = false;

            //Map reload input
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
        if (inputActions != null)
        {
            inputActions.Player.Enable();
        }
        reloading = false;
    }

    private void OnDisable()
    {
        // Disable the Player input action map
        if (inputActions != null)
        {
            inputActions.Player.Disable();
        }
    }

    //Checking states for shooting or reloading the gun
    public void Update()
    {
        if (!photonView.IsMine) return; //Only allow the local player to control shooting

        //Update ammo display on UI
        if (ammoDisplay != null)
        {
            UpdateAmmoDisplay();
        }

        //Check for reload input and magazine state
        if (reload && bulletsLeftInMagazine < magSize && !reloading)
        {
            Reload();
            return;
        }

        //Automatically reload if magazine is empty and player still has ammo
        if (bulletsLeftInMagazine == 0 && ammoTotal > 0 && !reloading)
        {
            Reload();
        }

        if (attack)
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

    // Method to update ammo display
    private void UpdateAmmoDisplay()
    {
        ammoDisplay.SetText("(" + bulletsLeftInMagazine + " / " + magSize + ") " + ammoTotal);
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

        // Raycast from the camera to simulate bullet travel
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit rayHit, range))
        {
            Debug.Log($"Raycast hit: {rayHit.collider.gameObject.name}");

            // Check if the hit object has an EnemyStats component and apply damage
            EnemyStats enemy = rayHit.transform.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Hit Enemy: {rayHit.transform.name}, Damage: {damage}");
            }

            Debug.Log("Hit: " + rayHit.collider.gameObject.name);

            PlayerVariables player = rayHit.transform.GetComponent<PlayerVariables>();
            if (player != null)
            {
                player.Damage(damage);
                Debug.Log("Player Hit");
            }

            // Apply impact force if the hit object has a Rigidbody
            if (rayHit.rigidbody != null)
            {
                rayHit.rigidbody.AddForce(-rayHit.normal * impactForce);
                Debug.Log($"Applied impact force to: {rayHit.collider.gameObject.name}");
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any object.");
        }
    }

    void ProjectileShot()
    {
        UpdateAmmoDisplay();
        Debug.Log("Projectile Test");

        recoiling = true;
        recovering = false;

        if (currentBullet != null)
        {
            //PhotonNetwork.Destroy(currentBullet);
            Destroy(currentBullet, 0.5f);
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
        else
        {
            Debug.LogWarning("Projectile does not have a Rigidbody component.");
        }
    }

    //Changes the state and calls method to reset variables
    public void Reload() //private void
    {
        if (ammoTotal > 0 && !reloading) // Check if player has enough ammo to reload
        {
            reloading = true;
            Debug.Log("Reloading...");
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

    // Method to set the shooter’s PhotonView ID
    public void SetShooter(int viewID)
    {
        shooterViewID = viewID;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PhotonView targetView = collision.gameObject.GetComponent<PhotonView>();
            if (targetView != null && targetView.IsMine)
            {
                PlayerVariables targetVariables = collision.gameObject.GetComponent<PlayerVariables>();
                if (targetVariables != null)
                {
                    targetVariables.TakeDamage(damage, PhotonView.Find(this.GetComponent<PhotonView>().ViewID));
                    Debug.Log("Projectile hit a player and dealt damage.");
                }
            }

            // Destroy the projectile after impact
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
