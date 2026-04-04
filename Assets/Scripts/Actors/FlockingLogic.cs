using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.HableCurve;



public class FlockingLogic : MonoBehaviour
{
    public void DumbTest()
    {
        Debug.Log("THIS WOEKDSS");
    }

    // a referance to this enemy's echo radius
    private float echoRadius = 0;

    // a list to store all our allies, that are within our echoRadius 
    private List<EnemyLogic> nearbyAllies = new List<EnemyLogic>();

    // Start is called before the first frame update
    void Start()
    {
        // get the "echo" radius from the enemy we're attached to
        echoRadius = GetComponentInParent<EnemyLogic>().GetEchoRadius();

        // the echo radius should always be greater than zero
        Debug.Assert(echoRadius > 0);
    }

    // Update is called once per frame
    void Update()
    {
        // clear the list so we can update it
        nearbyAllies.Clear();

        // get all our allies in the scene.
        var allies = FindObjectsOfType<EnemyLogic>();

        Vector3 ourPos = transform.position;

        // find all our allies that are within our echoRadius
        foreach (EnemyLogic ally in allies)
        {
            if (ally != GetComponentInParent<EnemyLogic>())
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
            return;
        }

        Vector3 cPos = DoCohesion();

        Vector3 sPos = DoSeparation();

        Vector3 aPos = DoAlignment();
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
        // returns the vector from cohesion pointing in  the opposite direction 
        return DoCohesion() * -1;
    }

    // Separation gets enemes to try and face the same direction 
    private Vector3 DoAlignment()
    {
        Vector2 sumFacing = Vector2.zero;

        // added up all the facing vectors from our nearbyAllies
        foreach (EnemyLogic ally in nearbyAllies)
        {
            sumFacing += ally.GetComponent<Rigidbody2D>().velocity;
            sumFacing = sumFacing.normalized;
        }

        // divide to get the average
        sumFacing /= nearbyAllies.Count;

        return new Vector3(sumFacing.x, sumFacing.y, 0.0f);
    }


    private float segments = 60.0f;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, GetComponent<Rigidbody2D>().velocity.normalized);

        Gizmos.color = Color.red;
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

    public void SetEchoRadius(float newRadius)
    {
        echoRadius = newRadius;
    }
}