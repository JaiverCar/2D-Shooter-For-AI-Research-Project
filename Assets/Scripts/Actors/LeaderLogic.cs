using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderLogic : MonoBehaviour
{
    [SerializeField]
    private float tetherRadius = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // get all our allies in the scene.
        var allies = FindObjectsOfType<EnemyLogic>();

        // find all our allies that are within our tether radius
        Vector3 ourPos = transform.position;
        foreach (EnemyLogic ally in allies)
        {
            // if this ally is a grunt
            if (ally.isGrunt == true)
            {
                Vector3 allyPos = ally.transform.position;
                float dist = Vector3.Distance(ourPos, allyPos);

                // and they are within our tether radius
                if (dist < tetherRadius)
                {
                    // set it's leader to ourselves
                    ally.SetLeader(this);
                }
            }
        }
    }

    public float GetTetherRadius()
    {
        return tetherRadius;
    }
}
