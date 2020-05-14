using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AIBase : MonoBehaviour
{
    public int myTeamID;
    public int[] alliedTeamsIDs;
    public int[] enemyTeamsIDs;
    public Transform targetObjectTransform;
    public Transform eyeTransform;
    public Transform myLOSTarget;
    public float targetPriority = 1;
    public float distToLoseAwareness = 35f;
    public float timeBetweenLOSChecks = 0.5f;
    public int timeBeforeTargetExpiration = 15;
    public float timeBetweenTargetChecksIfEngaging = 7;
    public float timeBetweenTargetChecksIfNotEngaging = 12;
    public float maxDistToNoticeTarget = 9999f;
    public float shoutDist = 50;

    public float timeBetweenReactingToSounds = 15;
    protected bool shouldReactToNewSound = true;

    public float maxLineOfSightChecksPerFrame = 3;




    public float myFieldOfView = 130;




    public bool canAcceptDynamicObjectRequests = false;

    public TacticalAI.Target currentEnemyTarget;

    public bool debugFieldOfView;

    protected int myUniqueID;
    protected TacticalAI.Target[] enemyTargets = null;
    protected bool engaging = false;
    protected List<TacticalAI.Target> listOfCurrentlyNoticedTargets = new List<TacticalAI.Target>();
    //Summery, in case I forget: The agent will lose track of targets that move far enough away from their last known position
    protected List<Vector3> lastKnownTargetPositions = new List<Vector3>();
    protected float effectiveFOV;
    protected List<int> targetIDs = new List<int>();

    [HideInInspector]
    public LayerMask layerMask;


    public abstract void UpdateEnemyAndAllyLists(TacticalAI.Target[] a, TacticalAI.Target[] e);
    public abstract void HearSound(Vector3 soundPos);
    public abstract void WarnOfGrenade(Transform t, float d);
    public abstract void ApplyDamage(float h);
   
    public abstract bool UseDynamicObject(Transform newMovementObjectTransform, string newAnimationToUse, string methodToCall, bool requireEngaging);
    public abstract bool UseDynamicObject(Transform newMovementObjectTransform, string newAnimationToUse, string methodToCall, bool requireEngaging, float timeToWait);
    protected abstract void NoticeATarget(TacticalAI.Target newTarget);
    protected abstract void CheckIfWeStillHaveAwareness();

    public virtual void CheckForLOSAwareness(bool shouldCheck360Degrees)
    {
        if (enemyTargets != null)
        {
            for (int i = 0; i < enemyTargets.Length; i++)
            {
                //Debug
                if (debugFieldOfView)
                {
                    UnityEngine.Debug.DrawRay(eyeTransform.position, eyeTransform.forward * 20, Color.green, timeBetweenLOSChecks);
                    Vector3 tarVec = Quaternion.AngleAxis(effectiveFOV, Vector3.up) * eyeTransform.forward;
                    UnityEngine.Debug.DrawRay(eyeTransform.position, tarVec * 20, Color.green, timeBetweenLOSChecks);
                    tarVec = Quaternion.AngleAxis(-effectiveFOV, Vector3.up) * eyeTransform.forward;
                    UnityEngine.Debug.DrawRay(eyeTransform.position, tarVec * 20, Color.green, timeBetweenLOSChecks);
                }

                //Check for line of sight	
                //Sometimes we may not want to restrict the agent's senses to their field of view.	
                //Stupid checks to make sure we still have the transforms because Unity can't pass a function telling us that a scene is about to be loaded
                if (eyeTransform && enemyTargets[i].transform && (shouldCheck360Degrees || Vector3.Angle(eyeTransform.forward, enemyTargets[i].transform.position - eyeTransform.position) < effectiveFOV) && Vector3.SqrMagnitude(eyeTransform.position - enemyTargets[i].transform.position) < maxDistToNoticeTarget)
                {
                    //UnityEngine.Debug.Log("Kontrol");
                    //(Vector3.Angle(eyeTransform.forward, enemyTargets[i].transform.position - eyeTransform.position));
                    //print(shouldCheck360Degrees);
                    if (!Physics.Linecast(eyeTransform.position, enemyTargets[i].transform.position, layerMask))
                    {
                        //UnityEngine.Debug.Log("Kontrol 2");
                        NoticeATarget(enemyTargets[i]);
                    }
                }
            }
        }
    }

    public virtual void SetNewTeam(int newTeam)
    {
        TacticalAI.ControllerScript.currentController.ChangeAgentsTeam(myUniqueID, newTeam);
        myTeamID = newTeam;
    }

    public virtual int GetUniqueID()
    {
        return myUniqueID;
    }

    public virtual int[] GetEnemyTeamIDs()
    {
        return enemyTeamsIDs;
    }

}
