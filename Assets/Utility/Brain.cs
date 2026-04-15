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

        public HiveMind.squads squad;
        public HiveMind hiveMind; // Reference to MAIN hive mind
        public SquadLeader squadLeader; // Reference to my squad leader (relay)
        public float personalSmartness = 1.0f;

        // Start is called before the first frame update
        void Awake()
        {
            context = new Context(this);

            thisEnemy = GetComponent<EnemyLogic>();

            foreach (ActionAI action in actions)
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

            bestAction = null;
            float highestUtility = 0.0f;

            foreach (ActionAI action in actions)
            {
                // Skip if action isn't allowed for my squad
                if (hiveMind != null && !action.IsAllowedForSquad(squad))
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

            if (bestAction != null)
            {
                bestAction.Execute(context);
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

            // Hive mind integration
            if (hiveMind != null)
            {
                // Get shared knowledge
                //hiveMind.GetKnowledge(ref context);

                // Report what I see back to the hive
                if (context.playerRef != null)
                {
                    hiveMind.UpdateKnowledge(context);
                }

                // Add hive stats to context for actions to use
                //context.SetData("hive_smartness", hiveMind.smartness);
                //context.SetData("hive_coordination", hiveMind.coordination);
                //context.SetData("squad_assignment", (int)squad);
            }
        }

        void OnDestroy()
        {
            if (squadLeader != null)
            {
                squadLeader.RemoveSubordinate(this);
            }
        }
    }
}
