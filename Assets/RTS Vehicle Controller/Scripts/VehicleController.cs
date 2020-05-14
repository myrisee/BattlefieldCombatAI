using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.AI;



public abstract class VehicleController : MonoBehaviour
{
    #region public members
    public List<Axle> Axles;    

    public float Torque = 400f;
    public float BrakeTorque = 6000f;
    public int TopSpeed = 100;

    [Tooltip("Torque by speed")]
    public AnimationCurve TorqueCurve;
    

    [HideInInspector]
    public bool IsSelected = false;

    public float steeringAngle = 0;
    [Tooltip("In degrees")]
    public float MaxSteeringAngle = 45;
    [Tooltip("Degrees per second")]
    public float SteeringSpeed = 80;

    public float Wheelbase
    {
        get { return wheelbase; }
    }
    protected float wheelbase;
    public float TurnRadius
    {
        get { return turnRadius; }
    }
    protected float turnRadius;

    [Tooltip("Offset of the center of gravity for this vehicle (on the Y axis)")]
    public int CenterOfGravityOffset = -2;

    [Space(20)]

    public UnityEngine.UI.Text DebugText;
    #endregion public members


    protected float speed = 0;
    // For speed calculation
    protected Vector3 lastPos;

    [HideInInspector]
    public Vector3 currentDestination;
    [HideInInspector]
    public bool destinationGiven = false;

    protected bool isReversing = false;
    protected NavMeshAgent pathfinder;


    /// <summary>
    /// Sets the throttle input to the vehicle, clamped to range of [-1,1]
    /// </summary>
    /// <param name="throttle">Throttle input value, between -1 and 1</param>
    public abstract void SetThrottle(float throttle);

    /// <summary>
    /// Sets the steering angle based on input, clamped to fit the MaxSteeringAngle.
    /// Called by FixedUpdate() in VehicleInput.cs
    /// </summary>
    /// <param name="input">The input value given to the vehicle</param>
    public abstract void SetSteeringAngle(float input);

    /// <summary>
    /// Sets the direct steering angle by providing the angle of the steered wheels in degrees.
    /// </summary>
    /// <param name="angle">Angle for steered wheels (positive left, negative right)</param>
    public abstract void SetDirectSteeringAngle(float angle);

    /// <summary>
    /// Updates the vehicle movement (throttle and steering) when a destination is given.
    /// </summary>
    public abstract void UpdateVehicle();

    /// <summary>
    /// Toggles the vehicle selection flag. If selected the vehicle will project a circle around it.
    /// </summary>
    /// <param name="value">True if selected</param>
    public void SetSelected(bool value)
    {
        IsSelected = value;
        //SelectionIndicator.SetActive(IsSelected);
    }

    /// <summary>
    /// Applies brake torque to all wheels
    /// </summary>
    public void SetHandbrake(bool isOn)
    {
        foreach (Axle axleInfo in Axles)
        {
            axleInfo.LeftWheel.brakeTorque = isOn ? BrakeTorque : 0;
            axleInfo.RightWheel.brakeTorque = isOn ? BrakeTorque : 0;
        }
    }

    #region RTS style movement

    protected void ReverseUntilOutsideRadius(float angle, float distance)
    {
        // steering control
        if (DebugText)
            DebugText.text = "(3) Angle: " + angle;

        if (angle > 3.0f)
            SetDirectSteeringAngle(-MaxSteeringAngle);
        else if (angle < -3.0f)
            SetDirectSteeringAngle(MaxSteeringAngle);
        else
            SetDirectSteeringAngle(0);


        // throttle control
        if (DebugText)
            DebugText.text += ", distance: " + distance;

        SetThrottle(-1.0f);
    }

    protected void ReverseToTarget(float angle, float distance)
    {
        // steering control
        if (DebugText)
            DebugText.text = "(2) Angle: " + angle;

        SetDirectSteeringAngle(0);

        // throttle control
        if (DebugText)
            DebugText.text += ", distance: " + distance;
        if (distance > 5.0f)
        {
            SetThrottle(-1.0f);
        }
        else
        {
            // Destination reached, snap out of it
            //destinationGiven = false;
            SetThrottle(0);
        }
    }

    protected void DriveForwardToTarget(float angle, float distance)
    {
        // steering control
        if (DebugText)
            DebugText.text = "(1) Angle: " + angle;

        if (angle > 3.0f)
            SetDirectSteeringAngle(MaxSteeringAngle);
        else if (angle < -3.0f)
            SetDirectSteeringAngle(-MaxSteeringAngle);
        else
            SetDirectSteeringAngle(Mathf.Lerp(-MaxSteeringAngle, MaxSteeringAngle, (angle + 3.0f) / 6.0f));

        // throttle control
        if (DebugText)
            DebugText.text += ", distance: " + distance;
        if (distance > 10.0f)
        {
            SetThrottle(1.0f);
        }
        else
        {
            // Destination reached, snap out of it
            //destinationGiven = false;
            SetThrottle(0);
        }
    }

    #endregion RTS style movement    

    protected abstract void ApplyLocalPositionToVisuals(WheelCollider collider);

}