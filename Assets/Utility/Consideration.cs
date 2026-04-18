using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    [System.Serializable]
    public class Consideration
    {
        [SerializeField]
        public AnimationCurve curve;
        public string contextKey;
        public int priority = 0;

        [SerializeField]
        public List<HiveMind.squads> allowedSquads = new List<HiveMind.squads>();

        public float EvaluateCurve(Context context)
        {
            float inputValue = context.GetData<float>(contextKey);

            float utility = curve.Evaluate(inputValue);
            return Mathf.Clamp01(utility);
        }

        void Reset()
        {
            curve = new AnimationCurve(
                new Keyframe(0f, 1f), // At normalized distance 0, utility is 1
                new Keyframe(1f, 0f)  // At normalized distance 1, utility is 0
            );
        }
    }
}
