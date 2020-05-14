using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleHealth : MonoBehaviour
{
    public float health = 100;

    public bool isDead = false;

    public GameObject explosionPrefab;

    public AudioClip explosionSFX;

    public void Damage(float h)
    {
        
        health -= h;

        if(health < 0f)
        {
            KillAI();
        }
    }

    public void KillAI()
    {
        if (isDead)
            return;

        isDead = true;
        GetComponent<VehicleAI>().RemoveAI();
        GetComponent<VehicleWeaponSystem>().KillAI();
        GetComponent<VehicleBase>().KillAI();

        Instantiate(explosionPrefab, this.transform.position, Quaternion.identity);

        GetComponent<AudioSource>().PlayOneShot(explosionSFX);
    }




}
