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

        public EnemyLogic thisEnemy;

        // Start is called before the first frame update
        void Awake()
        {
            context = new Context(this);
            
            thisEnemy = GetComponent<EnemyLogic>();
            
            foreach(ActionAI action in actions)
            {
                action.Init(context);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (thisEnemy == null)
            {
                thisEnemy = GetComponent<EnemyLogic>();
            }

            if (thisEnemy != null)
            {
                UpdateContext();
            }

            bestAction = null;
            float highestUtility = 0.0f;

            foreach(ActionAI action in actions)
            {
                float utilVal = action.CalculateUtility(context);
                int priority = (int)action.GetPriority();

                // Check if this action has a higher utility than the current best
                if (utilVal > highestUtility)
                {
                    highestUtility = utilVal;
                    bestAction = action;
                }

                // If the utility value is the same, check priority
                if (utilVal == highestUtility)
                {
                    if(bestAction == null)
                    {
                        bestAction = action;
                        continue;
                    }

                    if(priority > bestAction.GetPriority())
                    {
                        bestAction = action;
                    }
                    else if(priority == bestAction.GetPriority())
                    {
                        // Randomize if they are the same curve and priority
                        if (UnityEngine.Random.value > 0.5f)
                        {
                            bestAction = action;
                        }
                    }
                }
            }

            if(bestAction != null)
            {
                bestAction.Execute(context);
            }
        }

        void UpdateContext()
        {
            context.SetData("health", thisEnemy.Health / 3.0f);
            context.SetData("speed", thisEnemy.Speed);
            context.SetData("aggroed", thisEnemy.Aggroed);
            context.SetData("flag", thisEnemy.doAstar);
        }
    }
}
