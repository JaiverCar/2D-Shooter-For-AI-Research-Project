using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    public abstract class ActionAI : ScriptableObject
    {

        public Consideration consideration;
        public virtual void Init(Context context)
        {
            // nothing in base class
        }

        public float CalculateUtility(Context context) => consideration.EvaluateCurve(context); // change the value here

        public virtual void Execute(Context context)
        {
            // nothing in base class
        }
    }
}
