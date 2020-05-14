using System;
using UnityEngine;
using TacticalAI;

[Serializable]
public class AICommand
{
	public CommandType commandType;

	public Vector3 destination;
    public Transform targetDestination;
	public TargetScript target;
	public string animationName;



	public AICommand(CommandType ty, Vector3 v, TargetScript ta,Transform transform)
	{
		commandType = ty;
		destination = v;
		target = ta;
        targetDestination = transform;
	}

	public AICommand(CommandType ty, Vector3 v)
	{
		commandType = ty;
		destination = v;
	}

    public AICommand(CommandType ty, Transform transform)
    {
        commandType = ty;
        targetDestination = transform;
    }

    public AICommand(CommandType ty, TargetScript ta)
	{
		commandType = ty;
		target = ta;
	}

	public AICommand(CommandType ty)
	{
		commandType = ty;
	}

	public enum CommandType
	{
		GoTo,
		GoToAndGuard,
		AttackTarget, //attacks a specific target, then becomes Guarding
		Stop,
        Animation,
		//Flee,
		Die,
	}

	public enum AnimationType
	{
		None,
        Interact,

        

	}

}