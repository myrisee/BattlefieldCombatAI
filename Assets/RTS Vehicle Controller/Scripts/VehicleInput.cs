using System;
using UnityEngine;
using UnityEngine.AI;

public class VehicleInput : MonoBehaviour
{
    [Tooltip("Reference to the NavMeshPathfinder prefab instantiated by this " +
        "vehicle used navmesh pathfinding")]
    public GameObject PathfinderPrefab;
    [Tooltip("Does this vehicle use NavMesh - based pathfinding")]
    public bool UsesPathfinding = true;

    [Tooltip("True if the vehicle is controller directly by keyboard input, " +
        "false if controlled by right clicking a destination on the map")]
    public bool ManualInput = true;
    [Tooltip("Draw debug rays seen in the scene view")]
    public bool ShowDebugRays = true;

    private VehicleController controller;
    private float throttle, steering;

    private NavMeshAgent pathfinder;

    void Start()
    {
        controller = GetComponent<VehicleController>();
        if (UsesPathfinding)
            pathfinder = GameObject.Instantiate(PathfinderPrefab, transform.position, transform.rotation).GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (controller.IsSelected && Input.GetKeyDown(KeyCode.M))
        {
            controller.destinationGiven = false;
            // Toggle manual mode
            ManualInput = !ManualInput;
        }
    }

    public NavMeshAgent GetPathfinder()
    {
        return pathfinder;
    }

    void FixedUpdate()
    {       
        if (ManualInput && controller.IsSelected)
        {
            // Read manual input
            throttle = Input.GetAxis("Vertical");
            controller.SetThrottle(throttle);
            steering = Input.GetAxis("Horizontal");
            controller.SetSteeringAngle(steering);
        }
        else
        {
            // Set automatic steering and throttle input
            if (controller.destinationGiven)
            {
                controller.UpdateVehicle();                                
            }
            else
            {
                // Keep the handbrake on :)
                controller.SetHandbrake(true);
            }
        }
    }

   

    /// <summary>
    /// Gives the vehicle a destination on the map to move to.
    /// </summary>
    /// <param name="position">The Vector3 coordinate of the destination</param>
    public void SetDestination(Vector3 position)
    {
        if (!controller.IsSelected)
            return;

        if (pathfinder != null)
        {
            // Reset pathfinder to current vehicle position
            pathfinder.transform.position = transform.position;
            pathfinder.SetDestination(position);
        }

        controller.currentDestination = position;
        controller.destinationGiven = true;

        ManualInput = false;
    }


}
