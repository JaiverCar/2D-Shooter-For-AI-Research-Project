using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * NOTE: Part of this code was reconstructed from youtube tutorials on action curves
 * Video 1: https://www.youtube.com/watch?v=sISJdLO3JYM
 * Video 2: https://www.youtube.com/watch?v=S4oyqrsU2WU
 * 
 * The file was otherwise completely written by us (James Hardy, Javier Carballo Flor)
**************************************************************************************/

namespace UtilityAI
{
    public class Brain : MonoBehaviour
    {
        // List of actions that can be performed by this enemy
        [Header("Brain Settings")]
        public List<ActionAI> actions = null;

        // Best and Previous best action references
        ActionAI bestAction;
        ActionAI previousAction;

        // Action Name for displaying on degbug UI
        public string CurrentActionName => bestAction != null ? bestAction.name : "None";

        // Reference to the enemy who's brain this is and its context
        public EnemyLogic thisEnemy;
        public Context context;

        // Vars to check hive connection
        public float personalConnection = 1.0f;
        public bool isConnectedToHive = false;
        float hiveCheckTimer = 0f;
        const float hiveCheckInterval = 10f;

        // Current assigned squad
        public HiveMind.squads squad = HiveMind.squads.s_Scouts;

        // Sprites orignal color
        public Color ogColor;

        void Awake()
        {
            // Create context for this brain
            context = new Context(this);

            // Attempt to get EnemyLogic component
            thisEnemy = GetComponent<EnemyLogic>();

            // Initialize all actions
            foreach (ActionAI action in actions)
            {
                action.Init(context);
            }

            // Store original color of this enemies sprite
            if (thisEnemy != null)
            {
                ogColor = thisEnemy.GetComponent<SpriteRenderer>().color;
            }
        }

        void Start()
        {
            // Check connection to the hive
            isConnectedToHive = HiveMind.Instance != null && HiveMind.Instance.RecievesSignal(personalConnection);
        }

        void Update()
        {
            // Every n minutes check if the enemy should be connected to the hive mind
            if (squad != HiveMind.squads.s_nonDrone)
            {
                hiveCheckTimer += Time.deltaTime;
                if (hiveCheckTimer >= hiveCheckInterval)
                {
                    hiveCheckTimer = 0f;
                    isConnectedToHive = HiveMind.Instance != null && HiveMind.Instance.RecievesSignal(personalConnection);
                }

                // if the enemy is not connected to the hive mind, remove squad assignment
                // else, change color dependent on squad assignment
                if (isConnectedToHive == false)
                {
                    HiveMind.Instance.SwitchSquad(this, HiveMind.squads.s_NoSquad);
                }
                else if (squad == HiveMind.squads.s_NoSquad)
                {
                    squad = HiveMind.squads.s_Scouts;
                    HiveMind.Instance.SwitchSquad(this, HiveMind.squads.s_Scouts);
                }
            }

            // If enemy reference is null, try to get it agian
            if (thisEnemy == null)
            {
                thisEnemy = GetComponent<EnemyLogic>();
                if (thisEnemy != null)
                {
                    ogColor = thisEnemy.GetComponent<SpriteRenderer>().color;
                }
            }

            // Set enemy color based on squad assignment
            if (thisEnemy != null && squad != HiveMind.squads.s_nonDrone)
            {
                Color newColor;
                switch (squad)
                {
                    case HiveMind.squads.s_NoSquad:
                    {
                        newColor = Color.red;
                        break;
                    }
                    case HiveMind.squads.s_FlagAttackers:
                    {
                        newColor = Color.yellow;
                        break;
                    }
                    case HiveMind.squads.s_PlayerAttackers:
                    {
                        newColor = Color.cyan;
                        break;
                    }
                    default:
                    {
                        newColor = new Color(192, 192, 192);
                        break;
                    }
                }

                thisEnemy.GetComponent<SpriteRenderer>().color = newColor;
            }


            // Check each action for which will be best to perform
            bestAction = null;
            float highestUtility = 0.0f;

            foreach (ActionAI action in actions)
            {
                // Skip if action isn't allowed for this enemies squad
                if (HiveMind.Instance != null && !action.IsAllowedForSquad(squad))
                {
                    continue;
                }

                float utilVal = action.CalculateUtility(context);
                int priority = (int)action.GetPriority();

                // Check if this action has a higher utility than the current best
                if (utilVal > highestUtility)
                {
                    highestUtility = utilVal;
                    bestAction = action;
                }

                // If the utility value is the same
                if (utilVal == highestUtility)
                {
                    // early continue if bestAction is null
                    if (bestAction == null)
                    {
                        bestAction = action;
                        continue;
                    }

                    // Check priority, if new action has higher priorty,
                    // set it as best action
                    if (priority > bestAction.GetPriority())
                    {
                        bestAction = action;
                    }
                    // Randomize if they are the same curve and priority
                    else if (priority == bestAction.GetPriority())
                    {
                        if (UnityEngine.Random.value > 0.5f)
                        {
                            bestAction = action;
                        }
                    }
                }
            }

            // if there was a previous action, and the new action is
            // not the same call OnExit for the previous action
            if(previousAction != null)
            {
                if(previousAction != bestAction)
                {
                    thisEnemy.actionChange = true;
                    previousAction.OnExit(context);
                }
            }

            // Exectute the best action if one was found, update previousAction
            if(bestAction != null)
            {
                bestAction.Execute(context);
                previousAction = bestAction;
            }

            // Update the enemies context
            if (thisEnemy != null)
            {
                UpdateContext();
            }
        }

        // Updates this brains context
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
    }
}
