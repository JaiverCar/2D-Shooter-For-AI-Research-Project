using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_FindPlayerAlone")]
    public class A_FindPlayerAlone : ActionAI
    {
        public override void Init(Context context)
        {
            // TODO: Add INIT logic for A_FindPlayerAlone
        }

        public override void Execute(Context context)
        {
            Vector2 playerlastKnownPos = context.brain.thisEnemy.lastKnownPlayerLocation;//context.lastPlayerPosition;

            context.setTarget(playerlastKnownPos);
        }
    }
}