using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    public abstract class ActionAI : ScriptableObject
    {
        [SerializeField]
        public Consideration consideration;
        public virtual void Init(Context context)
        {
            // nothing in base class
        }

        public float CalculateUtility(Context context) => consideration.EvaluateCurve(context); // change the value here

        public int GetPriority() => consideration.priority;

        public virtual bool IsAllowedForSquad(HiveMind.squads squad) 
        { 
            if( consideration.allowedSquads.Contains(squad))
            {
                return true;
            }

            return false;
        }

        public abstract void Execute(Context context);

        public virtual void OnExit(Context context)
        {
            // nothing in base class
        }
    }
}
