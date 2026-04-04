using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingLogic : MonoBehaviour
{
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

        Vector3 cPos = Cohesion();

        Debug.Log(cPos);
    }

    private Vector3 Cohesion()
    {
        Vector3 sumPos = Vector3.zero;

        // added up all the positions from our nearbyAllies
        foreach (EnemyLogic ally in nearbyAllies)
        {
            sumPos += ally.transform.position;
        }

        sumPos /= nearbyAllies.Count;

        return sumPos;
    }
}
