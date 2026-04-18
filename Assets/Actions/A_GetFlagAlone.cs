using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_GetFlagAlone")]
    public class A_GetFlagAlone : ActionAI
    {
        [SerializeField] private float updateInterval = 0.5f;

        public override void Init(Context context)
        {
            // TODO: Add INIT logic for A_GetFlagAlone
        }

        public override void Execute(Context context)
        {
            Vector2 flagLastKnownPos = context.brain.thisEnemy.flagLastKnownLocation;

            float timeSinceUpdate = Time.time - context.lastFlagUpdateTime;

            if (timeSinceUpdate >= updateInterval)
            {
                context.setTarget(flagLastKnownPos);
                context.lastFlagUpdateTime = Time.time;
            }
        }
    }
}