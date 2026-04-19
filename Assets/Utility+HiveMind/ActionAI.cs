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
    public abstract class ActionAI : ScriptableObject
    {
        // The consideration value for this action
        [SerializeField]
        public Consideration consideration;

        // Initialize function for this action
        //Params: context - enemies context
        public virtual void Init(Context context)
        {
            // nothing in base class
        }

        // Calculates the utility value for this action
        // based on consideration
        // Params: context - enemies context
        // Returns: utility value of action
        public float CalculateUtility(Context context)
        {
            return consideration.EvaluateCurve(context);
        }

        // Pulls the priority value of this action
        // form the consideration
        // Returns: actions priority
        public int GetPriority()
        {
            return consideration.priority;
        }

        // Checks if this action is an allowed action by a given squad
        // based on the consideration values
        // Params: squad - the squad to check
        // Returns: true if action is allowed for squad, false otherwise
        public virtual bool IsAllowedForSquad(HiveMind.squads squad) 
        { 
            if( consideration.allowedSquads.Contains(squad))
            {
                return true;
            }

            return false;
        }

        // Actions execution code
        public abstract void Execute(Context context);

        // Actions OnExit code
        public virtual void OnExit(Context context)
        {
            // nothing in base class
        }
    }
}
