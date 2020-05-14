using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedVehicle : VehicleBase
{
   
    void Start()
    {
        // Compute distance between axles distance
        wheelBase = Vector3.Distance(axles[0].RightWheel.transform.position, axles[axles.Count - 1].RightWheel.transform.position);

        // Compute turn radius
        turnRadius = wheelBase / Mathf.Sin(maxSteer * Mathf.Deg2Rad) / 2f;

       

        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, -2, 0);

        currentDestination = this.transform.position;

        Handbrake = true;
    }

    void FixedUpdate()
    {
        if (state != VehicleState.Idle)
        {
            agent.SetDestination(currentDestination);

            VehicleUpdate();

            agent.transform.localPosition = Vector3.zero;
        }
    }

    

    public override void SetDestination(Vector3 temp)
    {
        currentDestination = temp;

        agent.SetDestination(currentDestination);

        agent.isStopped = false;

        state = VehicleState.Forward;

        Handbrake = false;
    }

    

    protected override void VehicleUpdate()
    {

        var destination = agent.transform.position + agent.desiredVelocity;

        //Debug.DrawRay(transform.position, transform.forward * 100, Color.blue);
        //Debug.DrawRay(transform.position, destination - transform.position, Color.white);

        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward * 100.0f, Vector3.up);
        Vector3 dir = Vector3.ProjectOnPlane((destination - agent.transform.position).normalized * 100.0f, Vector3.up);


        var angle = Vector3.SignedAngle(fwd, dir, transform.up);

        if (isBlocked)
        {
            distance = Vector3.Distance(this.transform.position, obstaclePos);
        }
        else
        {
            distance = Vector3.Distance(this.transform.position, destination);
        }

        if (distance > turnRadius || (distance < turnRadius && Mathf.Abs(angle) < 15.0f))
        {
            if (state == VehicleState.Reverse)
            {
                if (distance > turnRadius * turnRadiusMultiplier)
                {
                    state = VehicleState.Forward;
                    Handbrake = false;
                }
                else
                {
                    Reverse(angle, distance);
                    return;
                }
            }
            // Simply go forward while turning towards destination
            DriveForward(angle, distance);
        }
        else if (Mathf.Abs(angle) > 170)
        {
            // Destination is close and behind the vehicle, reverse
            ReverseToTarget(angle, distance);
        }
        else
        {
            // Reverse until outside of turn radius, then DriveForwardToTarget()
            state = VehicleState.Reverse;
            Reverse(angle, distance);
        }


        // Check if the destination is reached
        if (Vector3.Distance(frontDetection.position, currentDestination) < 6f)
        {
            agent.isStopped = true;

            state = VehicleState.Idle;

            Handbrake = true;
        }


    }

   

    
   

    protected override void SetDirectSteeringAngle(float angle)
    {

        Axle axleInfo = axles[0];

        if (axleInfo.Steering)
        {
            axleInfo.LeftWheel.steerAngle = angle;
            axleInfo.RightWheel.steerAngle = angle;

            Vector3 euler = axleInfo.LeftWheel.transform.localEulerAngles;
            axleInfo.LeftWheel.transform.localEulerAngles = new Vector3(euler.x, angle, euler.z);
            euler = axleInfo.RightWheel.transform.localEulerAngles;
            axleInfo.RightWheel.transform.localEulerAngles = new Vector3(euler.x, angle, euler.z);
        }

        axleInfo = axles[1];

        if (axleInfo.Steering)
        {
            axleInfo.LeftWheel.steerAngle = -angle;
            axleInfo.RightWheel.steerAngle = -angle;

            Vector3 euler = axleInfo.LeftWheel.transform.localEulerAngles;
            axleInfo.LeftWheel.transform.localEulerAngles = new Vector3(euler.x, -angle, euler.z);
            euler = axleInfo.RightWheel.transform.localEulerAngles;
            axleInfo.RightWheel.transform.localEulerAngles = new Vector3(euler.x, -angle, euler.z);
        }

        steeringAngle = angle;
    }

    protected override void ApplyLocalPositionToVisuals(Axle item)
    {
        
    }
}
