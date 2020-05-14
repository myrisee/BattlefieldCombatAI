using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WheeledVehicleController : VehicleController
{

    private void Start()
    {
        // Compute distance between axles distance
        wheelbase = Vector3.Distance(Axles[0].RightWheel.transform.position, Axles[1].RightWheel.transform.position);

        // Compute turn radius
        turnRadius = wheelbase / Mathf.Sin(MaxSteeringAngle * Mathf.Deg2Rad);

        Debug.Log(gameObject.name + ": turning radius = " + turnRadius + ", wheelbase = " + wheelbase);
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, CenterOfGravityOffset, 0);

        pathfinder = GetComponent<VehicleInput>().GetPathfinder();
    }

    public override void SetSteeringAngle(float input)
    {
        if (input == 0)
            // Return steering to neutral
            steeringAngle = Mathf.Lerp(steeringAngle, 0, Time.deltaTime);
        else
            // Apply steering input
            steeringAngle = Mathf.Clamp(steeringAngle + input * SteeringSpeed * Time.deltaTime, -MaxSteeringAngle, MaxSteeringAngle);

        foreach (Axle axleInfo in Axles)
        {
            if (axleInfo.Steering)
            {
                axleInfo.LeftWheel.steerAngle = steeringAngle;
                axleInfo.RightWheel.steerAngle = steeringAngle;

                Vector3 euler = axleInfo.LeftWheel.transform.localEulerAngles;
                axleInfo.LeftWheel.transform.localEulerAngles = new Vector3(euler.x, steeringAngle, euler.z);
                euler = axleInfo.RightWheel.transform.localEulerAngles;
                axleInfo.RightWheel.transform.localEulerAngles = new Vector3(euler.x, steeringAngle, euler.z);
            }
        }
    }

    public override void SetThrottle(float throttle)
    {
        // Calculate speed
        speed = Vector3.Distance(transform.position, lastPos) / Time.deltaTime;
        lastPos = transform.position;

        float trq = Torque * throttle * TorqueCurve.Evaluate(speed / TopSpeed);

        foreach (Axle axleInfo in Axles)
        {
            if (!((throttle > 0.0f && speed > -5.0f) || (throttle < 0.0f && speed < 5.0f)))
            {
                //if (DebugText)
                //DebugText.text = "BRAKE Torque: -" + BrakeTorque + ", speed: " + speed;
                // Brake
                axleInfo.LeftWheel.motorTorque = axleInfo.RightWheel.motorTorque = 0;
                axleInfo.LeftWheel.brakeTorque = axleInfo.RightWheel.brakeTorque = BrakeTorque;
            }
            else
            {
                axleInfo.LeftWheel.brakeTorque = axleInfo.RightWheel.brakeTorque = 0;
            }

            if (axleInfo.Driven)
            {
                if ((throttle > 0.0f && speed > -5.0f) || (throttle < 0.0f && speed < 5.0f))
                {
                    //if (DebugText)
                    //DebugText.text = "ACC Torque: " + trq + ", speed: " + speed;
                    // Accelerate
                    axleInfo.LeftWheel.brakeTorque = axleInfo.RightWheel.brakeTorque = 0;
                    axleInfo.LeftWheel.motorTorque = axleInfo.RightWheel.motorTorque = trq;
                }

            }
            ApplyLocalPositionToVisuals(axleInfo.LeftWheel);
            ApplyLocalPositionToVisuals(axleInfo.RightWheel);

        }

    }

    /// <summary>
    /// Applies the wheel collider rotation to the physical models of wheels.
    /// </summary>
    /// <param name="collider">The wheelcollider whose physical wheel needs to be rotated</param>
    protected override void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    /// <summary>
    /// Sets the immediate steering angle in degrees.
    /// Called by FixedUpdate() in VehicleInput.cs
    /// </summary>
    /// <param name="angle">The angle of the wheels</param>
    public override void SetDirectSteeringAngle(float angle)
    {
        foreach (Axle axleInfo in Axles)
        {
            if (axleInfo.Steering)
            {
                axleInfo.LeftWheel.steerAngle = angle;
                axleInfo.RightWheel.steerAngle = angle;

                Vector3 euler = axleInfo.LeftWheel.transform.localEulerAngles;
                axleInfo.LeftWheel.transform.localEulerAngles = new Vector3(euler.x, angle, euler.z);
                euler = axleInfo.RightWheel.transform.localEulerAngles;
                axleInfo.RightWheel.transform.localEulerAngles = new Vector3(euler.x, angle, euler.z);
            }
        }
    }

    public override void UpdateVehicle()
    {
        // If not using pathfinding, head straight to current destination
        var destination = pathfinder != null ? pathfinder.transform.position : currentDestination;

        Debug.DrawRay(transform.position, transform.forward * 100, Color.blue);
        Debug.DrawRay(transform.position, destination - transform.position, Color.white);

        Vector3 fwd = Vector3.ProjectOnPlane(transform.forward * 100.0f, Vector3.up);
        Vector3 dir = Vector3.ProjectOnPlane((destination - transform.position).normalized * 100.0f, Vector3.up);

        var angle = Vector3.SignedAngle(fwd, dir, transform.up);
        var distance = Vector3.Distance(transform.position, destination);

        if (distance > TurnRadius || (distance < TurnRadius && Mathf.Abs(angle) < 15.0f))
        {
            if (isReversing)
            {
                if (distance > TurnRadius * 2)
                {
                    isReversing = false;
                }
                else
                {
                    ReverseUntilOutsideRadius(angle, distance);
                    return;
                }
            }
            // Simply go forward while turning towards destination
            DriveForwardToTarget(angle, distance);
        }
        else if (Mathf.Abs(angle) > 170)
        {
            // Destination is close and behind the vehicle, reverse
            ReverseToTarget(angle, distance);
        }
        else
        {
            // Reverse until outside of turn radius, then DriveForwardToTarget()
            isReversing = true;
            ReverseUntilOutsideRadius(angle, distance);
        }

        // PATHFINDING
        // Set agent speed depending on distance from vehicle
        if (pathfinder != null)
        {
            if (distance > TurnRadius * 2)
                pathfinder.speed = 0; // Slow agent down
            else
                pathfinder.speed = TopSpeed; // Speed agent up
        }
        // Check if the destination is reached
        if (Vector3.Distance(transform.position, currentDestination) < 1)
        {
            destinationGiven = false;
        }
    }

}
