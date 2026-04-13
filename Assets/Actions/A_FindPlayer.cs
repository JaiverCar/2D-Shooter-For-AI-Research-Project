using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_FindPlayer")]
    public class A_FindPlayer : ActionAI
    {

        private Transform playerRef = null;
        public override void Init(Context context)
        {
            if (playerRef == null)
            {
                GetPlayerReference();
            }
        }

        public override void Execute(Context context)
        {
            if(playerRef == null)
            {
                GetPlayerReference();
                return;
            }

            context.setTarget(playerRef.position);
        }

        void GetPlayerReference()
        {
            //Already tracking the player
            if (playerRef != null)
                return;

            //Find the player
            var player = GameObject.Find("Player(Clone)");
            if (player == null)
                return;
            playerRef = player.transform;
        }
    }
}