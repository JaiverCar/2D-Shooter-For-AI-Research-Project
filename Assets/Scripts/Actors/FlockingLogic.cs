using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.HableCurve;
using Random = UnityEngine.Random;



public class FlockingLogic : MonoBehaviour
{
    // a referance to our own enemylogic component
    private EnemyLogic thisEnemy;

    // a referance to this enemy's echo radius
    private float echoRadius = 0;
    // a referance to this enemy's maximum separation radius
    private float maxSeparationRadius = 0;
    // a referance to this enemy's separation radius
    private float separationRadius = 0;

    // a referance to our leaders position
    public Vector3 leaderPos = Vector3.zero;

    // a referance to the tether radius of this enemies leader, if it has a leader
    public float tetherRadius = 0;

    // a list to store all our allies, that are within our echoRadius 
    private List<EnemyLogic> nearbyAllies = new List<EnemyLogic>();

    // to save a reference to our flock controller
    private FlockController flockController;

    // the new direction for the enemy to face after all the flock calculations
    private Vector3 direction = Vector3.zero;

    // local variable used to smooth cohesion
    private Vector3 smoother;
    
    // the rate of our smoothing of cohesion
    private float smoothRate = 0.5f;

    // bools for controlling which behavior we want to do at any time
    private bool doCohesion = true;
    private bool doSeparation = true;
    private bool doAlignment = true;
    private bool doTether = true;
    private bool doReducedSeparationRadius = false;

    // the amount to reduce the seperation radius by
    private float separationRadiusReductionRatio = 0.6f;

    // Start is called before the first frame update
    void Start()
    {
        // get a ref to our own EnemyLogic component
        thisEnemy = GetComponentInParent<EnemyLogic>();

        // this ref should never be null
        Debug.Assert(thisEnemy != null);

        // get a ref to our flock controller
        flockController = FindObjectOfType<FlockController>();

        // the flock controller should never be null
        Debug.Assert(flockController != null);
    }

    // Update is called once per frame
    void Update()
    {
        // if this enemy has a leader
        if (thisEnemy.HasLeader())
        {
            // get the tether radius from our leader
            tetherRadius = thisEnemy.GetLeader().GetTetherRadius();

            // if we have a leader, then the tether radius should never be 0
            Debug.Assert(tetherRadius > 0);

            // save a ref to our leaders position
            leaderPos = thisEnemy.GetLeader().transform.position;
        }
        // if this enemy does NOT have a leader
        else
        {
            leaderPos = Vector3.zero;
            tetherRadius = 0;
        }


        // clear the list so we can update it
        nearbyAllies.Clear();

        // get all our allies in the scene.
        var allies = FindObjectsOfType<EnemyLogic>();

        Vector3 ourPos = transform.position;

        // find all our allies that are within our echoRadius
        foreach (EnemyLogic ally in allies)
        {
            if (ally != thisEnemy)
            {
                Vector3 allyPos = ally.transform.position;
                float dist = Vector3.Distance(ourPos, allyPos);

                if (dist < echoRadius)
                {
                    nearbyAllies.Add(ally);
                }
            }
        }


        // return if we found 0 allies nearby
        if (nearbyAllies.Count == 0)
        {
            // just keep going in the same direction
            direction = transform.up;

            // UNLESS we have a leader
            if (thisEnemy.HasLeader() && doTether == true)
            {
                //In which case we still want to do our tether 
                direction = FindTetherVec() * flockController.GetTetherWeight();
            }

            return;
        }

        //// zero out the direction vector to contruct it again
        direction = Vector3.zero;

        Vector3 weightedCohesion = Vector3.zero;
        Vector3 weightedSeparation = Vector3.zero;
        Vector3 weightedAlignment = Vector3.zero;
        Vector3 weightedTether = Vector3.zero;

        if (doCohesion == true)
        {
            weightedCohesion = FindCohesionVec() * flockController.GetCohesionWeight();
        }
        if (doSeparation == true)
        {
            // if we need to reduce the separation radius
            if (doReducedSeparationRadius == true)
            {
                // then our separation redius is the max radius multiplied by a ratio
                separationRadius = maxSeparationRadius * separationRadiusReductionRatio;
            }
            else
            {
                // otherwise our separation redius is the max radius
                separationRadius = maxSeparationRadius;
            }

            weightedSeparation = FindSeparationVec() * flockController.GetSeparationWeight();
        }
        if (doAlignment == true)
        {
            weightedAlignment = FindAlignmentVec() * flockController.GetAlignmentWeight();
        }
        if (doTether == true)
        {
            weightedTether = FindTetherVec() * flockController.GetTetherWeight();
        }

        direction += weightedCohesion + weightedSeparation + weightedAlignment + weightedTether;

        DoSpeedReduction();
    }

    public Vector3 GetDirection()
    {
        return direction;
    }

    Vector3 test;

    // Cohesion gets enemies to come together 
    private void DoSpeedReduction()
    {
        EnemyLogic closestAlly = null;

        float closestDist = echoRadius;

        // find our closest ally that is infront of us
        foreach (EnemyLogic ally in nearbyAllies)
        {
            // get the direction to our ally
            Vector3 allyDir = (ally.transform.position - transform.position).normalized;

            float allydp = Vector3.Dot(transform.up, allyDir);

            // if they are infront of us
            if (allydp > 0.2f)
            {
                // get our distance to that ally
                float allyDist = Vector3.Distance(ally.transform.position, transform.position);

                // and if they are closer than our closestAlly
                if (allyDist < closestDist)
                {
                    // update our closest ally
                    closestDist = allyDist;
                    closestAlly = ally;
                }
            }
        }

        // if there are NO allies infront of us, then return to normal speed 
        if (closestAlly == null)
        {
            // lerp between our current speed and our max speed
            float newSpeed = Mathf.Lerp(thisEnemy.GetCurrentSpeed(), thisEnemy.GetMaxSpeed(), Time.deltaTime * 10f);

            // set the new speed
            thisEnemy.SetCurrentSpeed(newSpeed);
            return;
        }

        // divide to get the average
        //sumPos /= frontAllies;
        Vector3 closestAllyPos = closestAlly.transform.position;

        test = closestAllyPos;

        // find we we are facing the average position of our nearbyAllies
        Vector3 dir = (closestAllyPos - transform.position).normalized;
        float dp = Vector3.Dot(transform.up, dir);

        // if we are facing that position, slow down based on our distance
        if (dp > 0.2f)
        {
            // get our distance to this average point
            float dist = Vector3.Distance(closestAllyPos, transform.position);

            // find how close we are to the average poistion of our nearbyAllies
            float t = Mathf.InverseLerp(separationRadius + 0.1f, echoRadius, dist);

            // get a new target speed based on that distance
            float targetSpeed = Mathf.Lerp(0.0f, thisEnemy.GetMaxSpeed(), t);

            // lerp between our current speed and our target speed
            float newSpeed = Mathf.Lerp(thisEnemy.GetCurrentSpeed(), targetSpeed, Time.deltaTime * 10f);

            // if the new speed is too small, just set it to zero
            if (newSpeed < 0.01 || dist < separationRadius + 0.1f)
            {
                newSpeed = 0.0f;
            }

            // set the new speed
            thisEnemy.SetCurrentSpeed(newSpeed);
        }
        // if we are NOT facing the position, go back to normal speed
        else
        {
            // lerp between our current speed and our max speed
            float newSpeed = Mathf.Lerp(thisEnemy.GetCurrentSpeed(), thisEnemy.GetMaxSpeed(), Time.deltaTime * 10f);

            // set the new speed
            thisEnemy.SetCurrentSpeed(newSpeed);
        }
    }

    // Cohesion gets enemies to come together 
    private Vector3 FindCohesionVec()
    {
        Vector3 sumPos = Vector3.zero;

        // added up all the positions from our nearbyAllies
        foreach (EnemyLogic ally in nearbyAllies)
        {
            sumPos += ally.transform.position;
        }

        // divide to get the average
        sumPos /= nearbyAllies.Count;

        // get the offset from this enemy
        sumPos -= transform.position;

        // smooth from where this enemy is currently facing to where it should be facing for cohesion
        sumPos = Vector3.SmoothDamp(transform.up, sumPos, ref smoother, smoothRate);

        return sumPos;
    }

    // Separation gets enemes to move away from each other 
    private Vector3 FindSeparationVec() 
    {
        // the sum positions of all allies we will avoid
        Vector3 sumPos = Vector3.zero;
        // the number allies we need to avoid
        int sepCount = 0;

        Vector3 ourPos = transform.position;

        // find the vector and count of separation
        foreach (EnemyLogic ally in nearbyAllies)
        {
            Vector3 allyPos = ally.transform.position;

            float allyDist = Vector3.Distance(allyPos, ourPos);
            if (allyDist < separationRadius)
            {
                sepCount++;
                sumPos += (ourPos - allyPos);
            }
        }

        // average the speration vector
        if (sepCount != 0)
        {
            sumPos /= sepCount;
        }

        return sumPos;
    }

    // Alignment gets enemes to try and face the same direction 
    private Vector3 FindAlignmentVec()
    {
        Vector3 sumFacing = Vector3.zero;

        // added up all the facing vectors from our nearbyAllies
        foreach (EnemyLogic ally in nearbyAllies)
        {
            sumFacing += ally.transform.up;
        }

        // divide to get the average
        sumFacing /= nearbyAllies.Count;

        // normalize cause we only care about the direction
        sumFacing = sumFacing.normalized;

        return sumFacing;
    }

    // Wander gives them a random point to make their movements "wiggle"
    private Vector3 DoWander()
    {
        Vector3 randPos = Vector3.zero;

        randPos.x = Random.Range(-1000.0f, 1000.0f);
        randPos.y = Random.Range(-1000.0f, 1000.0f);

        return randPos;
    }

    // Tether makes sure that they stay within bounds of their leader
    private Vector3 FindTetherVec()
    {
        // we do not set this to 0 so that enemies still move within the tether radius of their leaders
        Vector3 leaderDirection = transform.up;

        // if the tether radius is set to 0 or anything less, it will not affect the movement of the enemy
        if (tetherRadius > 0)
        {
            // get the distance between this enemy and it's leader
            Vector3 ourPos = transform.position;
            float dist = Vector3.Distance(ourPos, leaderPos);

            // if we went past the tether radius of our leader
            if (dist > tetherRadius)
            {
                // get the normalized direction back to our leader
                leaderDirection = (leaderPos - ourPos).normalized;
            }
        }

        return leaderDirection;
    }

    public void SetEchoRadius(float newRadius)
    {
        echoRadius = newRadius;
    }

    public void SetMaxSeparationRadius(float newRadius)
    {
        maxSeparationRadius = newRadius;
    }

    public void DoCohesion(bool enabled)
    {
        doCohesion = enabled; 
    }

    public void DoSeparation(bool enabled)
    {
        doSeparation = enabled;
    }

    public void DoReducedSeparationRadius(bool enabled)
    {
        doReducedSeparationRadius = enabled;
    }

    public void DoAlignment(bool enabled)
    {
        doAlignment = enabled;
    }

    public void DoTether(bool enabled)
    {
        doTether = enabled;
    }


    private float segments = 60.0f;
    private void OnDrawGizmos()
    {
        if (this.name == "BaseEnemy (3)")
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(test, 0.5f);
        }


        if (doCohesion == true)
        {
            Gizmos.color = Color.green;
            float angleStep = 360.0f / segments;
            Vector3 prevPoint = transform.position + new Vector3(echoRadius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * angleStep * i;

                Vector3 newPoint = transform.position + new Vector3(
                    Mathf.Cos(angle) * echoRadius,
                    Mathf.Sin(angle) * echoRadius,
                    0
                );

                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        if (doSeparation == true)
        {
            float radius = maxSeparationRadius;

            if (doReducedSeparationRadius == true)
            {
                radius = maxSeparationRadius * separationRadiusReductionRatio;
            }

            Gizmos.color = Color.red;
            float angleStep = 360.0f / segments;
            Vector3 prevPoint = transform.position + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = Mathf.Deg2Rad * angleStep * i;

                Vector3 newPoint = transform.position + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );

                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}