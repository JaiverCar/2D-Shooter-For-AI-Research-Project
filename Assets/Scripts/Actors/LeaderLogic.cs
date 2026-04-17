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

                // and they are within our tether radius AND do not have a leader yet
                if (dist < tetherRadius && ally.HasLeader() == false)
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

    //private float segments = 60.0f;
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    float angleStep = 360.0f / segments;
    //    Vector3 prevPoint = transform.position + new Vector3(tetherRadius, 0, 0);

    //    for (int i = 1; i <= segments; i++)
    //    {
    //        float angle = Mathf.Deg2Rad * angleStep * i;

    //        Vector3 newPoint = transform.position + new Vector3(
    //            Mathf.Cos(angle) * tetherRadius,
    //            Mathf.Sin(angle) * tetherRadius,
    //            0
    //        );

    //        Gizmos.DrawLine(prevPoint, newPoint);
    //        prevPoint = newPoint;
    //    }
    //}
}