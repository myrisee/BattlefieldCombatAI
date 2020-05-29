using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VehicleWeaponSystem : MonoBehaviour
{
    public Transform bulletSpawn;

    public AudioClip fireSfx;

    public float fireRate = 0.3f;

    public float reloadTime = 10f;

    public int bulletCount = 10;

    public float fireAccuracy = 1f;

    public int bulletID = 3;

    bool isReloading = false;

    int bulletRemanining;

    public bool isRocketLauncher = false;

    TurretController turretController;

    VehicleHealth vehicleHealth;

    AudioSource audioSource;
    

    Transform target;

    bool isEngaging;

    float cooldown,reload;

    Quaternion fireRotation;

    private void Awake()
    {
        turretController = GetComponent<TurretController>();

        vehicleHealth = GetComponent<VehicleHealth>();

        audioSource = GetComponent<AudioSource>();

        bulletRemanining = bulletCount;
    }
    void Update()
    {
        if (vehicleHealth.isDead)
            return;

        if(target && !Physics.Linecast(bulletSpawn.position, target.position, TacticalAI.ControllerScript.currentController.GetLayerMask()))
        {
            turretController.AimPosition = target.position;
        }

        cooldown += Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (vehicleHealth.isDead)
            return;

        if (isEngaging && cooldown > fireRate && !isReloading)
        {
            if(bulletRemanining > 0)
            {
                Fire();
            }
            else
            {
                Reload();
            }
        }

        if(isReloading && reload > 0.2f)
        {
            reload -= Time.deltaTime;
        }else if(isReloading && reload < 0.2f)
        {
            isReloading = false;
            
        }
    }

    public void KillAI()
    {
        turretController.enabled = false;
    }

    public void SetTarget(Transform _target)
    {
        if (_target == null)
            return;

        target = _target;

        if(turretController != null)
            turretController.IsIdle = false;
    }

    public void RemoveTarget()
    {
        target = null;

        if (turretController != null)
            turretController.IsIdle = true;
    }

    public void SetEngage(bool temp)
    {
        
        isEngaging = temp;

        if(temp == false)
        {
            RemoveTarget();
        }
    }

    void Reload()
    {
        reload = reloadTime + Random.Range(0.5f,2f);

        bulletRemanining = Random.Range(bulletCount / 3, bulletCount);

        isReloading = true;
    }


    public void Fire()
    {
        if (!Physics.Linecast(bulletSpawn.position, target.position, TacticalAI.ControllerScript.currentController.GetLayerMask()))
        {
            cooldown = 0f;

            if (fireSfx)
                audioSource.PlayOneShot(fireSfx);

            fireRotation = Quaternion.LookRotation(bulletSpawn.forward);

            fireRotation *= Quaternion.Euler(Random.Range(-fireAccuracy, fireAccuracy), Random.Range(-fireAccuracy, fireAccuracy), 0);

            GameObject bullet = BulletPooling.instance.GetBullet(bulletID);

            bullet.transform.position = bulletSpawn.position;

            bullet.transform.rotation = fireRotation;

            bullet.SetActive(true);
            //If this is using the TacticalAI Bullet Script and is a rocket launcher
            if (isRocketLauncher && bullet.GetComponent<TacticalAI.BulletScript>())
            {
                bullet.GetComponent<TacticalAI.BulletScript>().SetAsHoming(target);
            }

            bulletRemanining--;
        }
        else
        {
            
        }

        
    }
}



