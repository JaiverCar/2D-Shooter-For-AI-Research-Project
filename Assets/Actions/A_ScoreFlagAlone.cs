using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_ScoreFlagAlone")]
    public class A_ScoreFlagAlone : ActionAI
    {
        public override void Init(Context context)
        {
            // TODO: Add INIT logic for A_ScoreFlagAlone
        }

        public override void Execute(Context context)
        {
            var enemyGoal = GameObject.Find("EnemyGoal");
            if (enemyGoal != null)
            {
                context.setTarget(enemyGoal.transform);
            }
        }
    }
}