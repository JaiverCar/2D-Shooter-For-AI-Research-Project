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
        ActionAI previousAction;

        public EnemyLogic thisEnemy;

        public HiveMind.squads squad = HiveMind.squads.s_Scouts;
        private HiveMind.squads oldSquad = HiveMind.squads.s_Scouts;
        public float personalConnection = 1.0f;

        private bool wasSeingPlayer = false;

        public bool isConnectedToHive = false;
        float hiveCheckTimer = 0f;
        const float hiveCheckInterval = 10f;

        void Awake()
        {
            context = new Context(this);

            thisEnemy = GetComponent<EnemyLogic>();

            foreach (ActionAI action in actions)
            {
                action.Init(context);
            }
        }

        void Start()
        {
            isConnectedToHive = HiveMind.Instance != null &&
                                 HiveMind.Instance.RecievesSignal(personalConnection);

            // save the squad we are assigned to
            oldSquad = squad;
        }

        // Update is called once per frame
        void Update()
        {
            hiveCheckTimer += Time.deltaTime;
            if (hiveCheckTimer >= hiveCheckInterval)
            {
                hiveCheckTimer = 0f;
                isConnectedToHive = HiveMind.Instance != null &&
                                    HiveMind.Instance.RecievesSignal(personalConnection);
            }

            if (isConnectedToHive == true)
            {
                squad = oldSquad;
            }
            else
            {
                squad = HiveMind.squads.s_NoSquad;
            }


            if (thisEnemy == null)
            {
                thisEnemy = GetComponent<EnemyLogic>();
            }

            bestAction = null;
            float highestUtility = 0.0f;

            foreach (ActionAI action in actions)
            {
                // Skip if action isn't allowed for my squad
                if (HiveMind.Instance != null && !action.IsAllowedForSquad(squad))
                    continue;

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
                    if (bestAction == null)
                    {
                        bestAction = action;
                        continue;
                    }

                    if (priority > bestAction.GetPriority())
                    {
                        bestAction = action;
                    }
                    else if (priority == bestAction.GetPriority())
                    {
                        // Randomize if they are the same curve and priority
                        if (UnityEngine.Random.value > 0.5f)
                        {
                            bestAction = action;
                        }
                    }
                }
            }

            if(previousAction != null)
            {
                if(previousAction != bestAction)
                {
                    previousAction.OnExit(context);
                }
            }

            if(bestAction != null)
            {
                bestAction.Execute(context);
                previousAction = bestAction;
            }

            if (thisEnemy != null)
            {
                UpdateContext();
            }
        }

        void UpdateContext()
        {
            // Personal data
            context.SetData("health", thisEnemy.Health, thisEnemy.StartingHealth);
            context.SetData("speed", thisEnemy.Speed);
            context.SetData("aggroed", thisEnemy.Aggroed);
            context.SetData("flag", thisEnemy.seesFlag);
            context.SetData("hasFlag", thisEnemy.hasFlag);

            // if the connection to the mind is strong enough, get the information from the hive. 
            if (isConnectedToHive)
            {
                HiveMind.Instance.GetKnowledge(context);
            }
        }

        void OnDestroy()
        {
        }
    }
}
