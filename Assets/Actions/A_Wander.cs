using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_Wander")]
    public class A_Wander : ActionAI
    {
        // NO instance fields here — ScriptableObjects are shared assets!

        public override void Init(Context context)
        {
            // Find a fresh target
            Vector2 newTarget = FindRandomWalkablePosition(context);
            if (newTarget.x >= -999)
            {
                context.wanderTarget = newTarget;
                context.hasWanderTarget = true;
                context.setTarget(newTarget);
            }
        }

        public override void Execute(Context context)
        {
            Vector2 currentPos = context.brain.thisEnemy.transform.position;
            Vector2 currentTarget = context.getTarget();

            // Check if we've reached the target or stopped moving
            bool reachedByDistance = Vector2.Distance(currentPos, currentTarget) < 0.5f;
            bool stoppedMoving = context.brain.thisEnemy.GetComponent<Rigidbody2D>().velocity.magnitude < 0.01f;
            
            // Note: This line was written by copilot
            // Need new target if: no target, reached by distance, OR stopped moving at current target
            bool needsNewTarget = !context.hasWanderTarget || 
                                  reachedByDistance || 
                                  (stoppedMoving && context.hasWanderTarget && Vector2.Distance(currentPos, currentTarget) < 1.5f);

            if (needsNewTarget)
            {
                Vector2 newTarget = FindRandomWalkablePosition(context);
                // set new target if one was found
                if (newTarget.x >= -999)
                {
                    context.wanderTarget = newTarget;
                    context.hasWanderTarget = true;
                    context.setTarget(newTarget);
                }
            }
        }

        private Vector2 FindRandomWalkablePosition(Context context)
        {
            // Return ridiculous val if the grid isnt active
            if (AStarGrid.Instance == null)
            {
                return new Vector2(-1000, -1000);
            }

            // Get current grid position
            Node currentNode = AStarGrid.Instance.NodeFromWorldPoint(context.brain.thisEnemy.transform.position);
            int startGridX = currentNode.gridX;
            int startGridY = currentNode.gridY;

            int maxAttempts = 50;
            int searchRadius = 30;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                int offsetX = Random.Range(-searchRadius, searchRadius + 1);
                int offsetY = Random.Range(-searchRadius, searchRadius + 1);

                int targetGridX = startGridX + offsetX;
                int targetGridY = startGridY + offsetY;
                Vector2 gridPos = new Vector2(targetGridX, targetGridY);

                // Check if valid grid position and walkable
                if (AStarGrid.Instance.IsValidGridPos(gridPos) && AStarGrid.Instance.IsWalkable(targetGridX, targetGridY))
                {
                    // Get the node and return its world position
                    Node targetNode = AStarGrid.Instance.GridGet(gridPos);
                    return targetNode.worldPosition;
                }

                // set higher radius if there wasnt one at the search radius
                if (attempt == 15)
                {
                    searchRadius = 40;
                }
                if (attempt == 30)
                {
                    searchRadius = 50;
                }
            }

            Debug.LogWarning($"[Wander] No walkable position found near grid ({startGridX}, {startGridY})");

            //return something ridiculous if theres not a valid position
            return new Vector2(-1000, -1000);
        }
    }
}