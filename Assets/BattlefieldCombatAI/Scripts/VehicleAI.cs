using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAI : MonoBehaviour
{
    public float minEngageDist = 200f;

    public float engageDivider = 1;

    public float aiCycleTick = 1f;

    float tick;

    private VehicleTarget vehicleTarget;

    private VehicleBase vehicleBase;

    RaycastHit hitInfo;

    private AIBase targetEnemy;

    private void Awake()
    {
        vehicleTarget = GetComponent<VehicleTarget>();

        vehicleBase = GetComponent<VehicleBase>();
    }


    private void Update()
    {
        if(tick > aiCycleTick && vehicleTarget.currentEnemyTarget != null)
        {
            if(Physics.Linecast(vehicleTarget.eyeTransform.position,vehicleTarget.currentEnemyTarget.transform.position, out hitInfo))
            {
                if (hitInfo.transform != null)
                    targetEnemy = hitInfo.transform.GetComponentInParent<VehicleTarget>();

                if(targetEnemy != null)
                {
                    if(targetEnemy == vehicleTarget.currentEnemyTarget.targetScript)
                    {
                        engageDivider = 1f;
                    }
                }
                else
                {
                    engageDivider = 0.1f;
                }
            }
            

            tick = 0;

            if(vehicleTarget.currentEnemyTarget != null)
            {
                if(Vector3.Distance(this.transform.position,vehicleTarget.currentEnemyTarget.transform.position) > (minEngageDist * engageDivider))
                {
                    vehicleBase.SetDestination(vehicleTarget.currentEnemyTarget.transform.position);
                }
                else
                {
                    vehicleBase.Stop();
                }
            }
        }

        tick += Time.deltaTime;
    }
}
