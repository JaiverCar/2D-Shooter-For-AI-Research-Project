using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_GetFlag")]
    public class A_GetFlag : ActionAI
    {
        public override void Init(Context context)
        {
        }

        public override void Execute(Context context)
        {
            // Get the hives value for the last known flag position
            Vector2 hiveLastKnownPos = context.GetData<Vector2>("hiveLastKnownFlagPos");

            // Set the flags last known position as the enemies target
            context.setTarget(hiveLastKnownPos);
            context.lastFlagPosition = hiveLastKnownPos;
            context.lastFlagUpdateTime = Time.time;
        }
    }
}