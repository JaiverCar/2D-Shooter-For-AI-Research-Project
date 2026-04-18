using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_FindPlayer")]
    public class A_FindPlayer : ActionAI
    {
        [SerializeField] private float updateThreshold = 1.0f;
        [SerializeField] private float updateInterval = 0.5f;

        // NO instance fields — this is a shared ScriptableObject asset

        public override void Init(Context context)
        {
        }

        public override void Execute(Context context)
        {

            Vector2 hiveLastKnownPos = context.GetData<Vector2>("hiveLastKnownPlayerPos");

            // If we have a direct reference to the player, track them normally
            float distanceMoved = Vector3.Distance(hiveLastKnownPos, context.lastPlayerPosition);
            float timeSinceUpdate = Time.time - context.lastUpdateTime;

            if (distanceMoved >= updateThreshold || timeSinceUpdate >= updateInterval)
            {
                context.setTarget(hiveLastKnownPos);
                context.lastPlayerPosition = hiveLastKnownPos;
                context.lastUpdateTime = Time.time;
            }
        }
    }
}