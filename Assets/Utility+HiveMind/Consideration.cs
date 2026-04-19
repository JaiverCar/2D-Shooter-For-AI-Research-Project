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
    [System.Serializable]
    public class Consideration
    {
        // Vars for determining utility value
        [SerializeField]
        public AnimationCurve curve;
        public string contextKey;
        public int priority = 0;

        // Var for determining if the enemies squad allows for them to carry out the parent action
        [SerializeField]
        public List<HiveMind.squads> allowedSquads = new List<HiveMind.squads>();

        // Evaluates the curves value at a given value from the context's data set
        // Params: context - enemies context
        // Returns: utility value at the given value
        public float EvaluateCurve(Context context)
        {
            float inputValue = context.GetData<float>(contextKey);

            float utility = curve.Evaluate(inputValue);
            return Mathf.Clamp01(utility);
        }
    }
}
