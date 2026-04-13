using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_GetFlag")]
    public class A_GetFlag : ActionAI
    {

        private Transform flagRef = null;
        public override void Init(Context context)
        {
            GetFlagReference();
        }

        public override void Execute(Context context)
        {
            if(flagRef == null)
            {
                GetFlagReference();
                return;
            }

            context.setTarget(flagRef.position);
        }

        void GetFlagReference()
        {
            //Already tracking the player
            if (flagRef != null)
                return;

            //Find the player
            var flag = GameObject.Find("Flag");
            if (flag == null)
                return;
            flagRef = flag.transform;
        }
    }
}