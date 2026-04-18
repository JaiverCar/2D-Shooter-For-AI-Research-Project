using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_FindPlayerAlone")]
    public class A_FindPlayerAlone : ActionAI
    {
        [SerializeField] private float updateInterval = 0.5f;

        public override void Init(Context context)
        {
            // TODO: Add INIT logic for A_FindPlayerAlone
        }

        public override void Execute(Context context)
        {
            Vector2 playerlastKnownPos = context.brain.thisEnemy.lastKnownPlayerLocation;//context.lastPlayerPosition;

            // If we have a direct reference to the player, track them normally
            float timeSinceUpdate = Time.time - context.lastUpdateTime;

            if (timeSinceUpdate >= updateInterval)
            {
                context.setTarget(playerlastKnownPos);
                context.lastUpdateTime = Time.time;
            }
        }
    }
}