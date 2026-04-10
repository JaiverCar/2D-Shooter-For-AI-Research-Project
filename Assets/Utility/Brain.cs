using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace UtilityAI
{
    public class Brain : MonoBehaviour
    {
        [Header("Brain Settings")]
        public Context context;
        public List<ActionAI> actions = null;

        ActionAI bestAction;

        EnemyLogic thisEnemy;

        // Start is called before the first frame update
        void Awake()
        {
            context = new Context(this);
            foreach(ActionAI action in actions)
            {
                action.Init(context);
            }

            EnemyLogic thisEnemy = GetComponent<EnemyLogic>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateContext();

            bestAction = null;
            float highestUtility = 0.0f;

            foreach(ActionAI action in actions)
            {
                float utilVal = action.CalculateUtility(context);

                if(utilVal > highestUtility)
                {
                    highestUtility = utilVal;
                    bestAction = action;
                }
            }

            if(bestAction != null)
            {
                bestAction.Execute(context);
            }

        }

        void UpdateContext()
        {
            context.SetData("health", thisEnemy.Health);
        }
    }
}
