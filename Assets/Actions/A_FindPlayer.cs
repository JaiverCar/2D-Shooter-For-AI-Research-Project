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
            if (context.playerRef == null)
                GetPlayerReference(context);

            if (context.playerRef != null)
            {
                context.lastPlayerPosition = context.playerRef.position;
                context.lastUpdateTime = Time.time;
                context.setTarget(context.playerRef.position);
            }
        }

        public override void Execute(Context context)
        {
            if (context.playerRef == null)
            {
                GetPlayerReference(context);
                return;
            }

            float distanceMoved = Vector3.Distance(context.playerRef.position, context.lastPlayerPosition);
            float timeSinceUpdate = Time.time - context.lastUpdateTime;

            if (distanceMoved >= updateThreshold || timeSinceUpdate >= updateInterval)
            {
                context.setTarget(context.playerRef.position);
                context.lastPlayerPosition = context.playerRef.position;
                context.lastUpdateTime = Time.time;
            }
        }

        private void GetPlayerReference(Context context)
        {
            var player = GameObject.Find("Player(Clone)");
            if (player != null)
                context.playerRef = player.transform;
        }
    }
}