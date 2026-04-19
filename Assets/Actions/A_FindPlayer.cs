using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_FindPlayer")]
    public class A_FindPlayer : ActionAI
    {

        public override void Init(Context context)
        {
        }

        public override void Execute(Context context)
        {
            // Get the hives value for the last known player position
            Vector2 hiveLastKnownPos = context.GetData<Vector2>("hiveLastKnownPlayerPos");

            // Set the players last known position as the enemies target
            context.setTarget(hiveLastKnownPos);
            context.lastPlayerPosition = hiveLastKnownPos;
            context.lastUpdateTime = Time.time;
        }
    }
}