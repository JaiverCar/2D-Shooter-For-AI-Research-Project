using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_ScoreFlag")]
    public class A_ScoreFlag : ActionAI
    {
        public override void Init(Context context)
        {
            // TODO: Add INIT logic for A_ScoreFlag
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