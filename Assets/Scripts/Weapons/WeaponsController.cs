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

    public EnemyStats enemy;

    //Type of gun being held
    public string gunName;

    //Bullet
    private GameObject currentBullet;       // Reference to the current bullet

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

    //Projectile gun
    public GameObject bulletPrefab;    // The bullet prefab
    public Transform bulletSpawnPoint; // The point where bullets will spawn
    public float bulletSpeed = 60f;    // Speed of the bullet

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
            bulletsLeft = magazineSize;
            //readyToShoot = true;

            ammoDisplay.SetText(bulletsLeft + " / " + magazineSize);

            //Initialize the input actions, ridigbody and collider
            inputActions = new PlayerInputActions();

            //Attack input
            inputActions.Player.Attack.performed += ctx => attack = true;
            inputActions.Player.Attack.canceled += ctx => attack = false;

            //Reload input
            inputActions.Player.Reload.performed += ctx => reload = true;
            inputActions.Player.Reload.canceled += ctx => reload = false;
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

    //Checking states for shooting or reloading the gun
   public void Update()
    {
        //Set ammo display 
        if (ammoDisplay != null)
        {
            ammoDisplay.SetText(bulletsLeft + " / " + magazineSize);
        }

        if (reload && bulletsLeft < magazineSize && !reloading)
        {
            Debug.Log("Reloading");
            Reload();
        }

        if (bulletsLeft == 0 && !reloading)
        {
            Debug.Log("Auto Reloading");
            Reload();
        }

        if (attack && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            bulletsShot = bulletsPerShot;

            //Different shooting mechanics trying to see if it picks up the gameobject with the right operation
            if(gameObject.tag == "HitScan")
            {
                Debug.Log("HitScan");
                Shoot();
            }
            else
            {
                Debug.Log("Projectile");
                ProjectileShot();
                //DestroyImmediate(bulletPrefab,true);
            }
        }


    }

    //What happens when the player shots the gun
    void Shoot()
    {
        //Prevent more shooting whilst shooting
        //readyToShoot = false;
        Debug.Log(bulletsLeft + " / " + magazineSize);

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out rayHit, range))
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
    }

    void ProjectileShot()
    {
        Debug.Log(bulletsLeft + " / " + magazineSize);

        // Destroy the current bullet if it exists
        if (currentBullet != null)
        {
            Destroy(currentBullet,0.5f);
        }

        // Instantiate the bullet at the spawn point
        currentBullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Get the Rigidbody component from the bullet and apply force to shoot it forward
        Rigidbody rb = currentBullet.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(bulletSpawnPoint.forward * bulletSpeed, ForceMode.Impulse);
        }

        bulletsLeft--;
        bulletsShot++;

        //if (bulletsShot > 0 && bulletsLeft > 0)
        //{
        //    Invoke("Shoot", timeBetweenShots);
        //}
    }

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

    //If the bullet hits anything with a RB it will be destroyed
    void OnCollisionEnter(Collision collision)//for 3D RB add 2D for other rb option
    {
        Destroy(currentBullet, 0.5f);
    }
}
