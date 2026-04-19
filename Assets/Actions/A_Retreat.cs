using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    [CreateAssetMenu(menuName = "AI/Actions/A_Retreat")]
    public class A_Retreat : ActionAI
    {
        public int searchRadius = 15;

        public float maxAcceptableVisibility = 0.3f;

        public float visibilityWeight = 0.6f;

        public float arrivalThreshold = 0.3f;
        public float healDelay = 3f;
        public int healAmount = 1;

        private LayerVisualization visibilityLayer;
        private readonly HashSet<EnemyLogic> healingEnemies = new HashSet<EnemyLogic>();
        public override void Init(Context context)
        {
            if (TerrainAnalysis.Instance == null)
            {
                return;
            }

            foreach (var layer in TerrainAnalysis.Instance.layers)
            {
                if (layer.layerType == LayerType.Visibility)
                {
                    visibilityLayer = layer;
                    break;
                }
            }
        }

        public override void Execute(Context context)
        {
            if (!context.retreating)
            {

                context.retreating = true;
                if (visibilityLayer == null || visibilityLayer.layer == null)
                {
                    Init(context);
                    return;
                }

                EnemyLogic enemy = context.brain.thisEnemy;

                if (enemy == null)
                {
                    return;
                }

                Vector2 currentPos = enemy.transform.position;
                AStarGrid grid = AStarGrid.Instance;

                if (grid == null)
                {
                    return;
                }

                Node currentNode = grid.NodeFromWorldPoint(currentPos);
                int currentRow = currentNode.gridY;
                int currentCol = currentNode.gridX;

                context.retreatPos = FindBestRetreatPosition(currentRow, currentCol, grid);

                if (context.retreatPos != Vector2.zero)
                {
                    context.setTarget(context.retreatPos);
                }
            }
            else
            {
                EnemyLogic enemy = context.brain.thisEnemy;

                if (enemy != null && context.retreatPos != Vector2.zero)
                {
                    float dist = Vector2.Distance((Vector2)enemy.transform.position, context.retreatPos);

                    if (dist <= arrivalThreshold && enemy.Health < enemy.StartingHealth && !healingEnemies.Contains(enemy))
                    {
                        healingEnemies.Add(enemy);
                        enemy.StartCoroutine(HealAfterDelay(enemy));
                    }
                }
            }

        }

        private IEnumerator HealAfterDelay(EnemyLogic enemy)
        {
            yield return new WaitForSeconds(healDelay);
            enemy.Health = Mathf.Min(enemy.Health + healAmount, enemy.StartingHealth);
            healingEnemies.Remove(enemy);
        }

        private Vector2 FindBestRetreatPosition(int currentRow, int currentCol, AStarGrid grid)
        {
            Vector2 bestPosition = Vector2.zero;
            float bestScore = float.MaxValue;

            // Search in a radius around the current position
            for (int r = -searchRadius; r <= searchRadius; r++)
            {
                for (int c = -searchRadius; c <= searchRadius; c++)
                {
                    int checkRow = currentRow + r;
                    int checkCol = currentCol + c;

                    // Skip if out of bounds or current position
                    if (checkRow < 0 || checkRow >= grid.gridSizeY ||
                        checkCol < 0 || checkCol >= grid.gridSizeX ||
                        (r == 0 && c == 0))
                        continue;

                    // Skip unwalkable cells
                    if (!grid.IsWalkable(checkCol, checkRow))
                        continue;

                    // Get visibility value
                    float visibility = visibilityLayer.layer.GetValue(checkRow, checkCol);

                    // Skip if too visible
                    if (visibility > maxAcceptableVisibility)
                        continue;

                    // Calculate distance from current position
                    float distance = Mathf.Sqrt(r * r + c * c);

                    // Calculate score
                    float distanceScore = distance;
                    float visibilityScore = visibility * searchRadius; // Normalize to similar scale
                    float combinedScore = (1f - visibilityWeight) * distanceScore + visibilityWeight * visibilityScore;

                    // Update best position if this is better
                    if (combinedScore < bestScore)
                    {
                        bestScore = combinedScore;
                        Node node = grid.GridGet(checkRow, checkCol);
                        bestPosition = node.worldPosition;
                    }
                }
            }

            return bestPosition;
        }

        public override void OnExit(Context context)
        {
            context.retreating = false;
        }
    }
}