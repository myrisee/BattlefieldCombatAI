﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class VehicleBase : MonoBehaviour
{
    #region 
    [SerializeField]
    VehicleState _state;

    public VehicleState state
    {
        set
        {
            _state = value;

            if(value == VehicleState.Idle)
            {
                navMeshObstacle.carvingMoveThreshold = 10f;
                navMeshObstacle.carvingTimeToStationary = 0.05f;

                Handbrake = true;
            }
            else
            {
                navMeshObstacle.carvingMoveThreshold = 0.1f;
                navMeshObstacle.carvingTimeToStationary = 2f;
            }
        }
        get
        {
            return _state;
        }
    }

    public List<Axle> axles;

    public NavMeshAgent agent;

    public ParticleSystem particleSystem;

    protected NavMeshObstacle navMeshObstacle;

    protected VehicleHealth vehicleHealth;

    public float maxSteer = 30f;

    public float maxSpeed = 15f;

    public float brakeTorque = 5000;

    public float motorTorque = 5000;

    public float turnRadiusMultiplier = 1.3f;

    public AnimationCurve TorqueCurve;

    public bool _Handbrake;

    protected bool Handbrake
    {
        set
        {
            foreach (Axle item in axles)
            {
                item.LeftWheel.brakeTorque = value ? brakeTorque : 0;
                item.RightWheel.brakeTorque = value ? brakeTorque : 0;

                _Handbrake = value;

                item.LeftWheel.motorTorque = 0;
                item.RightWheel.motorTorque = 0;
            }

            //GetComponent<Rigidbody>().velocity = Vector3.zero;

        }
    }

    public Transform frontDetection;
    public Transform backDetection;

    [SerializeField]
    protected Vector3 currentDestination;
    #endregion

    protected Vector3 obstaclePos;

    protected bool isBlocked;

    protected float wheelBase;
    protected float turnRadius;
    protected float distance;
    protected float steeringAngle;
    protected float speed;
    protected float torque;
    protected Vector3 lastPos;


    public abstract void SetDestination(Vector3 pos);
    protected abstract void SetDirectSteeringAngle(float angle);
    protected abstract void ApplyLocalPositionToVisuals(Axle item);
    protected abstract void VehicleUpdate();

    protected void Awake()
    {
        vehicleHealth = GetComponent<VehicleHealth>();
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        state = VehicleState.Idle;
    }

    protected void DriveForward(float angle, float distance)
    { // steering control
        if (isFrontClear())
        {
            state = VehicleState.Forward;
            Handbrake = false;
            if (angle > 3.0f)
                SetDirectSteeringAngle(maxSteer);
            else if (angle < -3.0f)
                SetDirectSteeringAngle(-maxSteer);
            else
                SetDirectSteeringAngle(Mathf.Lerp(-maxSteer, maxSteer, (angle + 3.0f) / 6.0f));

            if (distance > 15.0f)
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
        else
        {
            Handbrake = false;
            state = VehicleState.Reverse;
        }


    }

    public void KillAI()
    {
        state = VehicleState.Idle;
    }

    public void Stop()
    {
        state = VehicleState.Idle;

        agent.isStopped = true;
    }


    protected void Reverse(float angle, float distance)
    {
        if (!isBackClear())
            state = VehicleState.Forward;

        if (angle > 3.0f)
            SetDirectSteeringAngle(-maxSteer);
        else if (angle < -3.0f)
            SetDirectSteeringAngle(maxSteer);
        else
            SetDirectSteeringAngle(0);

        SetThrottle(-1.0f);
    }

    protected void SetThrottle(float throttle)
    {
        speed = Vector3.Distance(transform.position, lastPos) / Time.deltaTime;

        lastPos = transform.position;

        torque = motorTorque * throttle * TorqueCurve.Evaluate(speed / maxSpeed);

        foreach (Axle axleInfo in axles)
        {
            if (axleInfo.Driven)
            {
                axleInfo.LeftWheel.brakeTorque = axleInfo.RightWheel.brakeTorque = 0;
                axleInfo.LeftWheel.motorTorque = axleInfo.RightWheel.motorTorque = torque;
            }

            ApplyLocalPositionToVisuals(axleInfo);

        }
    }


    //Checking the front of vehicle for obstacle avoidance
    public bool isFrontClear()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(frontDetection.position, frontDetection.forward, out hit, 4f))
        {
            obstaclePos = hit.point;

            isBlocked = true;

            //Debug.Log(hit.collider.name);

            return false;

        }
        else
        {
           

            isBlocked = false;

            return true;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("SignedAngle " + Vector3.Angle(this.transform.position, (collision.GetContact(0).point - this.transform.position)).ToString());

        //Debug.Log(Vector3.Dot(this.transform.forward, (collision.GetContact(0).point - this.transform.position)).ToString());

        if(Vector3.Dot(this.transform.forward, (collision.GetContact(0).point - this.transform.position)) < 0 && state == VehicleState.Reverse)
        {
            isBlocked = false;

            state = VehicleState.Forward;
        }
        else if(Vector3.Dot(this.transform.forward, (collision.GetContact(0).point - this.transform.position)) > 0 && state == VehicleState.Forward)
        {
            obstaclePos = collision.GetContact(0).point;

            isBlocked = true;

            state = VehicleState.Reverse;
        }

       

    }

    public bool isBackClear()
    {
        if (Physics.Linecast(backDetection.position, backDetection.position + backDetection.forward * 4))
        {
            isBlocked = false;

            return false;
        }
        else
        {
            return true;
        }
    }

    protected void ReverseToTarget(float angle, float distance)
    {
        SetDirectSteeringAngle(0);

        if (distance > 5.0f)
        {
            SetThrottle(-1.0f);
        }
        else
        {
            SetThrottle(0);
        }
    }


}
