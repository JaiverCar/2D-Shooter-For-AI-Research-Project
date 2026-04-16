using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_DefendFlag")]
    public class A_DefendFlag : ActionAI
    {
        private Transform flagRef = null;

        public int defendingDistance = 1;
        public LayerMask obstacleLayer;


        public override void Init(Context context)
        {
            GetFlagReference();
        }

        public override void Execute(Context context)
        {
            if (flagRef == null)
            {
                GetFlagReference();
                return;
            }

            // get the node position of the flag
            Node flagNode = AStarGrid.Instance.NodeFromWorldPoint(flagRef.transform.position);
            int flagRow = flagNode.gridY;
            int flagCol = flagNode.gridX;

            // get our grid position
            EnemyLogic enemy = context.brain.thisEnemy;
            Node ourNode = AStarGrid.Instance.NodeFromWorldPoint(enemy.transform.position);
            Vector2 ourGridPos = new Vector2(ourNode.gridX, ourNode.gridY);


            Vector2 defenseGridPos = GetDefendingGridPos(flagRow, flagCol, ourGridPos, context.brain, AStarGrid.Instance);

            Node node = AStarGrid.Instance.GridGet(defenseGridPos);

            context.setTarget(node.worldPosition);
        }


        private Vector2 GetDefendingGridPos(int flagRow, int flagCol, Vector2 ourGridPos, Brain ourBrain, AStarGrid grid)
        {
            // find the closest point on the defence perimeter
            Vector2 closestNode = Vector2.zero;
            float closestDist = float.MaxValue;

            for (int i = -defendingDistance; i <= defendingDistance; i++)
            {
                Vector2 checkPos = new Vector2(flagCol + i, flagRow);

                for (int j = -defendingDistance; j <= defendingDistance; j++)
                {
                    checkPos.y = flagRow + j;

                    int flagDist = (int)Vector2.Distance(checkPos, new Vector2(flagCol, flagRow));

                    // check if this is a valid node to defend on
                    if (AStarGrid.Instance.IsWalkable(flagCol + i, flagRow + j) && flagDist == defendingDistance)
                    {
                        Node checkNode = grid.GridGet(checkPos);
                        Collider2D enemyCollider = Physics2D.OverlapBox(checkNode.worldPosition, Vector2.one * 0.5f, 0f, obstacleLayer);

                        bool occupied = false;

                        if (enemyCollider != null)
                        {
                            Brain otherBrain = enemyCollider.GetComponent<Brain>();

                            if (otherBrain != ourBrain)
                            {
                                occupied = true;
                            }
                        }
                        
                        if (occupied == false)
                        {
                            float ourDist = Vector2.Distance(checkPos, ourGridPos);

                            // check if this node is closer than our closest node
                            if (ourDist < closestDist)
                            {
                                closestDist = ourDist;
                                closestNode = checkPos;
                            }
                        }

                    }
                }
            }

            return closestNode;
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
