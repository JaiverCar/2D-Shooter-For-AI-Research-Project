using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_GetFlag")]
    public class A_GetFlag : ActionAI
    {
        [SerializeField] private float updateThreshold = 1.0f;
        [SerializeField] private float updateInterval = 0.5f;

        public override void Init(Context context)
        {
        }

        public override void Execute(Context context)
        {
            Vector2 hiveLastKnownPos = context.GetData<Vector2>("hiveLastKnownFlagPos");

            float distanceMoved = Vector3.Distance(hiveLastKnownPos, context.lastFlagPosition);
            float timeSinceUpdate = Time.time - context.lastFlagUpdateTime;

            if (distanceMoved >= updateThreshold || timeSinceUpdate >= updateInterval)
            {
                context.setTarget(hiveLastKnownPos);
                context.lastFlagPosition = hiveLastKnownPos;
                context.lastFlagUpdateTime = Time.time;
            }
        }
    }
}