using System.Collections;
using System.Collections.Generic;
using TacticalAI;
using UnityEngine;

public class VehicleAI : AIBase
{
    public VehicleWeaponSystem weaponSystem;
    public VehicleHealth vehicleHealth;

    private void Awake()
    {
        if (!targetObjectTransform)
            targetObjectTransform = this.transform;

        if (!myLOSTarget)
            myLOSTarget = this.transform;

        if (ControllerScript.currentController)
            myUniqueID = ControllerScript.currentController.AddTarget(myTeamID, targetObjectTransform, this);
        else
            Debug.LogWarning("No AI Controller Found!");

        layerMask = ControllerScript.currentController.GetLayerMask();

        effectiveFOV = myFieldOfView / 2;
        maxDistToNoticeTarget = maxDistToNoticeTarget * maxDistToNoticeTarget;

        
    }

    void Start()
    {
        //Start some loops
        if (this.gameObject)
        {
            StartCoroutine(LoSLoop());
            StartCoroutine(TargetSelectionLoop());

        }
    }

    IEnumerator LoSLoop()
    {
        //Check to see if we can see enemy targets every x seconds
        yield return new WaitForSeconds(Random.value);

        while (!vehicleHealth.isDead)
        {
            CheckForLOSAwareness(false);
            yield return new WaitForSeconds(timeBetweenLOSChecks);
        }
    }

    IEnumerator TargetSelectionLoop()
    {
        //Pick which target we will fire at and take cover from.
        //Update our target more frequently if we are engaging
        yield return new WaitForSeconds(Random.value);
        while (!vehicleHealth.isDead)
        {
           

            if (engaging)
                yield return new WaitForSeconds(timeBetweenTargetChecksIfEngaging);
            else
                yield return new WaitForSeconds(timeBetweenTargetChecksIfNotEngaging);

            
            ChooseTarget();
        }
    }

    public override void ApplyDamage(float h)
    {
        
    }

    public void RemoveAI()
    {
        TacticalAI.ControllerScript.currentController.RemoveTargetFromTargetList(myUniqueID);

    }

    public override void HearSound(Vector3 soundPos)
    {
        
    }

    public override void UpdateEnemyAndAllyLists(Target[] a, Target[] e)
    {
        if (vehicleHealth.isDead)
            return;
         //allyTransforms = a;
            enemyTargets = e;

            //If we don't have any targets left, exit the engaging state
            if (enemyTargets.Length == 0)
            {
                weaponSystem.SetEngage(false);
                engaging = false;
            }

            TacticalAI.Target[] lastTargets = listOfCurrentlyNoticedTargets.ToArray();
            listOfCurrentlyNoticedTargets = new List<TacticalAI.Target>();
            Vector3[] lastlastKnownTargetPositions = lastKnownTargetPositions.ToArray();
            lastKnownTargetPositions = new List<Vector3>();

            //Put all targets that still exist in new list
            for (int i = 0; i < lastTargets.Length; i++)
            {
                for (int x = 0; x < enemyTargets.Length; x++)
                {
                    if (lastTargets[i].uniqueIdentifier == enemyTargets[x].uniqueIdentifier)
                    {
                        listOfCurrentlyNoticedTargets.Add(enemyTargets[x]);
                        lastKnownTargetPositions.Add(lastlastKnownTargetPositions[i]);
                        break;
                    }
                }
            }

            //Check to see if we can see any targets.  If engaging, we aren't limited by what direction we are looking in.
            if (engaging)
            {
                CheckForLOSAwareness(true);
            }
            else
            {
                CheckForLOSAwareness(false);
            }

            ChooseTarget();
        
    }

    public override bool UseDynamicObject(Transform newMovementObjectTransform, string newAnimationToUse, string methodToCall, bool requireEngaging)
    {
        return false;
    }

    public override bool UseDynamicObject(Transform newMovementObjectTransform, string newAnimationToUse, string methodToCall, bool requireEngaging, float timeToWait)
    {
        return false;
    }

    public override void WarnOfGrenade(Transform t, float d)
    {
       
    }

    protected override void NoticeATarget(Target newTarget)
    {
        int IDToAdd = newTarget.uniqueIdentifier;

        //Make sure we haven't seen this target already
        for (int i = 0; i < targetIDs.Count; i++)
        {
            if (targetIDs[i] == IDToAdd)
            {
                return;
            }
        }

        lastKnownTargetPositions.Add(newTarget.transform.position);
        listOfCurrentlyNoticedTargets.Add(newTarget);
        targetIDs.Add(IDToAdd);

        ChooseTarget();

        //If we aren't already engaging in combat, start engaging.
        if (!engaging)
        {
            weaponSystem.SetEngage(true);
            engaging = true;
        }
    }

    void ChooseTarget()
    {
        if (eyeTransform && !vehicleHealth.isDead)
        {
            float currentEnemyScore = 0;
            float enemyScoreCheckingNow = 0;
            Transform enemyTransformCheckingNow = eyeTransform;
            currentEnemyTarget = null;
            bool foundTargetWithLoS = false;
            int i = 0;

            
                CheckIfWeStillHaveAwareness();

            for (i = 0; i < listOfCurrentlyNoticedTargets.Count; i++)
            {
                if (listOfCurrentlyNoticedTargets[i].transform)
                {
                    enemyTransformCheckingNow = listOfCurrentlyNoticedTargets[i].transform;

                    //Only add points if we have LoS
                    if (!Physics.Linecast(eyeTransform.position, enemyTransformCheckingNow.position, layerMask))
                    {
                        //Get initial score based on distance
                        enemyScoreCheckingNow = Vector3.SqrMagnitude(enemyTransformCheckingNow.position - targetObjectTransform.position);
                        //enemyScoreCheckingNow = Vector3.Distance(enemyTransformCheckingNow.position, targetObjectTransform.position);

                        //Divide by priority
                        enemyScoreCheckingNow = enemyScoreCheckingNow / (listOfCurrentlyNoticedTargets[i].targetScript.GetComponent<AIBase>().targetPriority);

                        //See if this score is low enough to warrent changing target
                        if (enemyScoreCheckingNow < currentEnemyScore || currentEnemyScore == 0 || !foundTargetWithLoS)
                        {

                            currentEnemyTarget = listOfCurrentlyNoticedTargets[i];
                            currentEnemyScore = enemyScoreCheckingNow;
                            foundTargetWithLoS = true;
                        }
                    }
                    //Settle for targets we can't see, if we have to.
                    else if (!foundTargetWithLoS)
                    {
                        enemyScoreCheckingNow = Vector3.SqrMagnitude(enemyTransformCheckingNow.position - targetObjectTransform.position);
                        if (enemyScoreCheckingNow < currentEnemyScore || currentEnemyScore < 0 || !foundTargetWithLoS)
                        {
                            currentEnemyTarget = listOfCurrentlyNoticedTargets[i];
                            currentEnemyScore = enemyScoreCheckingNow;
                        }
                    }
                }
            }

            if (currentEnemyTarget != null)
            {
                //AlertAlliesOfEnemy_Shout();
            }

            //If all of the above fails, pick a random target- even if it's one we haven't seen
            if (currentEnemyTarget == null && enemyTargets.Length > 0)
            {

                currentEnemyTarget = enemyTargets[Random.Range(0, enemyTargets.Length - 1)];
            }

            if (currentEnemyTarget != null)
            {
                weaponSystem.SetTarget(currentEnemyTarget.transform);

                //myAIBaseScript.SetMyTarget(currentEnemyTarget.transform, currentEnemyTarget.targetScript.myLOSTarget);
            }
            if (currentEnemyTarget == null)
            {
                weaponSystem.SetEngage(false);

            }
        }
    }


    protected override void CheckIfWeStillHaveAwareness()
    {
        Transform enemyTransformCheckingNow;
        int i = 0;

        for (i = 0; i < listOfCurrentlyNoticedTargets.Count; i++)
        {
            enemyTransformCheckingNow = listOfCurrentlyNoticedTargets[i].transform;
            if (eyeTransform && enemyTransformCheckingNow && !Physics.Linecast(eyeTransform.position, enemyTransformCheckingNow.position, layerMask))
            {
                lastKnownTargetPositions[i] = enemyTransformCheckingNow.position;
            }
            else if (enemyTransformCheckingNow && Vector3.Distance(enemyTransformCheckingNow.position, lastKnownTargetPositions[i]) > distToLoseAwareness)
            {
                listOfCurrentlyNoticedTargets.RemoveAt(i);
                lastKnownTargetPositions.RemoveAt(i);
                i -= 1;
            }
        }
        if (listOfCurrentlyNoticedTargets.Count == 0)
        {
            weaponSystem.SetEngage(false);
            engaging = false;
            listOfCurrentlyNoticedTargets = new List<TacticalAI.Target>();
            lastKnownTargetPositions = new List<Vector3>();
            targetIDs = new List<int>();
        }
    }
}
