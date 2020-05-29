using System.Collections;
using System.Collections.Generic;
using TacticalAI;
using UnityEngine;
using UnityEngine.UI;


public class Helicopter_AI : MonoBehaviour,IMissileLock
{
    public ParticleSystem flareParticle;
    public int flareCount = 5;
    public float flareTriggerDistance = 100f;
    bool missileLock;
    List<BulletScript> incomingMissiles = new List<BulletScript>();

    float dirNum;

    [SerializeField]
    VehicleState _state;

    public VehicleState state
    {
        set
        {
            _state = value;

            if (value == VehicleState.Idle)
            {
               
            }
            else
            {

            }
        }
        get
        {
            return _state;
        }
    }

    void Start()
    {
        
        
    }
    bool engage;
    private void Update()
    {
        VehicleUpdate();

        if(missileLock)
        {
            for(int i=0;i<incomingMissiles.Count;i++)
            {
                if(Vector3.Distance(this.transform.position,incomingMissiles[i].transform.position) < flareTriggerDistance && flareCount > 0)
                {
                    flareCount--;
                    flareParticle.enableEmission = true;
                    StartCoroutine(DestroyMissile(incomingMissiles[i]));
                }
            }
        }
    }

    IEnumerator DestroyMissile(BulletScript missile)
    {
        missile.homingTrackingSpeed = 0;
        incomingMissiles.Remove(missile);

        if (incomingMissiles.Count == 0)
            missileLock = false;

        yield return new WaitForSeconds(1f);

        flareParticle.enableEmission = false;
    }


    void VehicleUpdate()
    {
        if (GetComponent<VehicleTarget>().currentEnemyTarget.transform == null)
            return;

        Vector3 heading = GetComponent<VehicleTarget>().currentEnemyTarget.transform.position - transform.position;
        dirNum = AngleDir(transform.forward, heading, transform.up);
    }

   

    public Vector2 direction()
    {
        return new Vector2(dirNum, 1);
    }

    public float throttle()
    {
        return 70f;
    }

    public void KillAI()
    {
        GetComponent<HelicopterController>().enabled = false;
        GetComponent<Rigidbody>().drag = 0;
        GetComponent<Rigidbody>().angularDrag = 0.1f;
        GetComponent<AudioSource>().loop = false;

    }


    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0)
        {
            return 1f;
        }
        else if (dir < 0)
        {
            return -1f;
        }
        else
        {
            return 0f;
        }
    }

    public void MissileLock(BulletScript missile)
    {
        missileLock = true;

        incomingMissiles.Add(missile);
    }

    public void RadarLock()
    {
        throw new System.NotImplementedException();
    }
}
