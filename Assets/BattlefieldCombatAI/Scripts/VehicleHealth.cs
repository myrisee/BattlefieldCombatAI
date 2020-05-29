using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleHealth : MonoBehaviour
{
    public float health = 100;

    public bool isDead = false;

    public GameObject explosion;

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
        GetComponent<VehicleTarget>().RemoveAI();
        GetComponent<VehicleWeaponSystem>().KillAI();

        if(GetComponent<VehicleBase>() != null)
            GetComponent<VehicleBase>().KillAI();

        if (GetComponent<Helicopter_AI>() != null)
            GetComponent<Helicopter_AI>().KillAI();


        explosion.SetActive(true);

        

        GetComponent<AudioSource>().PlayOneShot(explosionSFX);
    }




}
