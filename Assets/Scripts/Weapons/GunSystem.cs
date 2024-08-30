using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    //Gun variables
    public int damage;
    public int magazineSize;
    public int bulletsPerShot; //bullets per tap of the mouse or attack button
    public float timeBetweenShooting;
    public float spread;
    public float range;
    public float reloadTime;
    public float timeBetweenShots;
    public bool allowButtonHold;
    int bulletsLeft;
    int bulletsShot;

    //Booleans to determine true or false settings
    bool shooting;
    //bool readyToShoot;
    bool reloading;

    //References to other game objects in the scene
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    private void Awake()
    {
        bulletsLeft = magazineSize;
        //readyToShoot = true;
    }

    private void Update()
    {
        MyInput();

        //SetText
        //text.SetText(bulletsLeft + " / " + magazineSize);
    }

    private void MyInput()
    {
        //Checking if the mouse button has been held down for shooting function
        if (allowButtonHold)
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }

        //Shooting Gun
        if (Input.GetKeyDown(KeyCode.R) && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerShot;
            Shoot();
        }
    }

    private void Shoot()
    {
        //Prevent more shooting whilst shooting
        //readyToShoot = false;

        //spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Calculate Direction with spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        //Damaging Enemies
        if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);

            if (rayHit.collider.CompareTag("Enemy"))
            {
                rayHit.collider.GetComponent<EnemyStats>().TakeDamage(damage);
            }

            EnemyStats enemy = rayHit.transform.GetComponent<EnemyStats>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        //Keeping track of how many bullets the user has left
        bulletsLeft--;
        bulletsShot--;

        //Delay to calling the reset function for shooting
        Invoke("ResetShot", timeBetweenShooting);

        if(bulletsShot > 0 && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }
    }

    //private void ResetShot()
    //{
    //    readyToShoot = true;
    //}

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadTime()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
