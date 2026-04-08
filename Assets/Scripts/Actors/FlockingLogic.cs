using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
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
    // a referance to this enemy's avoidance radius
    private float separationRadius = 0;

    // a referance to our leaders position
    public Vector3 leaderPos = Vector3.zero;

    // a referance to the tether radius of this enemies leader, if it has a leader
    public float tetherRadius = 0;

    // a list to store all our allies, that are within our echoRadius 
    private List<EnemyLogic> nearbyAllies = new List<EnemyLogic>();

    // to save a reference to our flock controller
    private FlockController flockController;

    // the goal point that the enemy will try to steer to
    private Vector3 goal = Vector3.zero;

    private Vector3 direction = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        // get a ref to our own EnemyLogic component
        thisEnemy = GetComponentInParent<EnemyLogic>();

        // this ref should never be null
        Debug.Assert(thisEnemy != null);

        // the echo radius should always be greater than zero
        Debug.Assert(echoRadius > 0);

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

        bool hasGroup = true;
        // return if we found 0 allies nearby
        if (nearbyAllies.Count == 0)
        {
            hasGroup = false;
        }

        // zero out the direction vector to contruct it again
        direction = Vector3.zero;


        // zero out the gaol vector to contruct it again
        goal = Vector3.zero;

        // get the value of all our modifiers
        float tStrenght = flockController.GetTetherStrength(); // tether strength
        float sStrenght = flockController.GetSeparationStrength(); // separation strength
        float cStrenght = flockController.GetCohesionStrength(); // cohesion strength
        float aStrenght = flockController.GetAlignmentStrength(); // alignment strength

        // if this enemy has NO leader or NO group
        if (thisEnemy.HasLeader() == false && hasGroup == false)
        {
            // just wander
        }
        // if this enemy has a leader, but NO group 
        if (thisEnemy.HasLeader() == true && hasGroup == false)
        {
            // do wander and do tether

            Vector3 tDir = DoTether(); // the direction our tether is pulling us in

            direction += (tDir * tStrenght);
        }
        // if this enemy has NO leader, but has a group
        if (thisEnemy.HasLeader() == false && hasGroup == true)
        {
            // do wander, cohesion, separation, and alignment BUT NOT tether

            Vector3 sPos = DoSeparation(); // the direction to move away from our allies 

            Vector3 cPos = DoCohesion(); // the position to move towards our allies

            goal += (sPos * sStrenght) + (cPos * cStrenght);

            //Vector3 aDir = DoAlignment(); // the direction to turn to align with our nearby allies
            //direction += (aDir * aStrenght);

            // testing
            if (Vector3.Distance(cPos, ourPos) > 10.0f)
            {
                Vector3 cDir = (cPos - ourPos).normalized;

                direction += cDir;
            }
            if (Vector3.Distance(sPos, ourPos) < 5.0f)
            {
                Vector3 sDir = (sPos - ourPos).normalized;

                direction += sDir;
            }
            //direction += (sDir * sStrenght) + (cDir * cStrenght);
        }
        // if this enemy has a leader and a group
        if (thisEnemy.HasLeader() == true && hasGroup == true)
        {
            // do wander, cohesion, separation, alignment, and tether

            Vector3 sPos = DoSeparation(); // the direction to move away from our allies 

            Vector3 cPos = DoCohesion(); // the position to move towards our allies

            goal += (sPos * sStrenght) + (cPos * cStrenght);

            //Vector3 aDir = DoAlignment(); // the direction to turn to align with our nearby allies
            //direction += (aDir * aStrenght);

            Vector3 tDir = DoTether(); // the direction our tether is pulling us in
            direction += (tDir * tStrenght);

            // testing
            if (Vector3.Distance(cPos, ourPos) > 10.0f)
            {
                Vector3 cDir = (cPos - ourPos).normalized;

                direction += cDir;
            }
            if (Vector3.Distance(sPos, ourPos) < 5.0f)
            {
                Vector3 sDir = (sPos - ourPos).normalized;

                direction += sDir;
            }
            //direction += (sDir * sStrenght) + (cDir * cStrenght);
        }

        // get the weighted average of all our calcuated directions
        //direction /= (tStrenght + aStrenght + sStrenght +cStrenght);


        // get the weighted average of all our calcuated positions
        goal /= (sStrenght + cStrenght);

        // too unstable

        //Vector3 rPos = DoWander(); // a random position to wander to
        //float rStrenght = flockController.GetWanderStrength();

        //// reset the goal
        //goal = Vector3.zero;

        //// get a weighted averge of all the positions and directions we calculated
        //goal += (cStrenght * cPos) + (sStrenght * sPos) + (rStrenght * rPos);//(aStrenght * aDir) + (tStrenght * tDir);
        //goal /= (cStrenght + sStrenght + rStrenght); //+ tStrenght + aStrenght);

        //direction = Vector3.zero;

        //direction += (aStrenght * aDir) + (tStrenght * tDir);
        //direction /=  (tStrenght + aStrenght);
    }

    public Vector3 GetDirection()
    {
        return direction;
    }

    public Vector3 GetGoal()
    {
        return goal;
    }

    // Cohesion gets enemies to come together 
    private Vector3 DoCohesion()
    {
        Vector3 sumPos = Vector3.zero;

        // added up all the positions from our nearbyAllies
        foreach (EnemyLogic ally in nearbyAllies)
        {
            sumPos += ally.transform.position;
        }

        // divide to get the average
        sumPos /= nearbyAllies.Count;

        return sumPos;
    }

    // Separation gets enemes to move away from each other 
    private Vector3 DoSeparation() 
    {
        // get the average positon of our nearby allies
        Vector3 targaet = DoCohesion();

        // get the direction to our target DO NOT normalize
        Vector3 ourPos = transform.position;
        Vector3  targetDirection = (targaet - ourPos);

        // get a target position in the oppisite direction from our nearby allies but of same distance away from us
        Vector3 oppTargetPos = ourPos - targetDirection;

        return oppTargetPos;
    }

    // Alignment gets enemes to try and face the same direction 
    private Vector3 DoAlignment()
    {
        Vector2 sumFacing = Vector2.zero;

        // added up all the facing vectors from our nearbyAllies
        foreach (EnemyLogic ally in nearbyAllies)
        {
            sumFacing += ally.GetComponent<Rigidbody2D>().velocity;
        }

        // divide to get the average
        sumFacing /= nearbyAllies.Count;

        // normalize cause we only care about the direction
        sumFacing = sumFacing.normalized;

        return new Vector3(sumFacing.x, sumFacing.y, 0.0f);
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
    private Vector3 DoTether()
    {
        Vector3 leaderDirection = Vector3.zero;

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

    public void SetSeparationRadius(float newRadius)
    {
        separationRadius = newRadius;
    }

    private float segments = 60.0f;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, GetComponent<Rigidbody2D>().velocity.normalized);

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
}