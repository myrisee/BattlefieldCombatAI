using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TacticalAI;

[Serializable]
public class AICommandBehaviour : PlayableBehaviour
{
	public AICommand.CommandType commandType;
	public TargetScript targetUnit;
    public Transform targetDestination;
    public Vector3 targetPosition;

	[HideInInspector]
	public bool commandExecuted = false; //the user shouldn't author this, the Mixer does

	//No logic in here, all logic is in the Track Mixer (AICommandMixerBehaviour)
	//This is because each clip needs more than one clip in Edit mode,
	//to be able to tween the Units' positions from one clip to the other
}