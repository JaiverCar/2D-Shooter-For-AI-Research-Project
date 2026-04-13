using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    // Start is called before the first frame update
    public float visionAggroThreshold = 0.1f;

    Transform Player;
    Transform Flag;

    public bool canSeePlayer = false;
    public bool canSeeFlag = false;
    void Start()
    {
        var player = GameObject.Find("Player(Clone)");
        if (player != null)
        {
            Player = player.transform;
        }

        var flag = GameObject.Find("Flag(Clone)");
        if (flag != null)
        {
            Flag = flag.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {

        canSeeFlag = false;
        canSeePlayer = false;

        // Safety checks
        if (TerrainAnalysis.Instance == null || AStarGrid.Instance == null)
        {
            return;
        }

        // Get this enemy's vision layer
        LayerVisualization visionLayer = TerrainAnalysis.Instance.GetEnemyVisionLayer(transform);
        if (visionLayer == null || visionLayer.layer == null)
        {
            return;
        }


        if (Player == null)
        {
            var player = GameObject.Find("Player(Clone)");
            if (player != null)
            {
                Player = player.transform;
            }
            else
            {
                canSeePlayer = false;
            }
        }

        if (Flag == null)
        {
            var flag = GameObject.Find("Flag(Clone)");
            if (flag != null)
            {
                Flag = flag.transform;
            }
            else
            {
                canSeeFlag = false;
            }
        }


        // PLAYER VISIBILITY CHECK
        if (Player != null)
        {
            Vector2 playerWorldPos = new Vector2(Player.position.x, Player.position.y);
            Node playerNode = AStarGrid.Instance.NodeFromWorldPoint(playerWorldPos);

            // Check if player position is valid on the grid
            if (!AStarGrid.Instance.IsValidGridPos(new Vector2(playerNode.gridX, playerNode.gridY)))
            {
                canSeePlayer = false;
            }

            // Get the vision value at the player's grid position
            float visionValue = visionLayer.layer.GetValue(playerNode.gridY, playerNode.gridX);

            // Player is visible if vision value exceeds threshold
            if (visionValue >= visionAggroThreshold)
            {
                canSeePlayer = true;
            }
        }

        // FLAG VISIBILITY CHECK
        if (Flag != null)
        {
            Vector2 flagWorldPos = new Vector2(Flag.position.x, Flag.position.y);
            Node flagNode = AStarGrid.Instance.NodeFromWorldPoint(flagWorldPos);

            if (!AStarGrid.Instance.IsValidGridPos(new Vector2(flagNode.gridX, flagNode.gridY)))
            {
                canSeeFlag = false;
            }

            float flagVisionValue = visionLayer.layer.GetValue(flagNode.gridY, flagNode.gridX);

            if (flagVisionValue >= visionAggroThreshold)
            {
                canSeeFlag = true;
            }
        }
    }

    public Vector2 GetPlayerPosition()
    {
        if (Player != null)
        {
            return new Vector2(Player.position.x, Player.position.y);
        }
        return Vector2.zero;
    }

    public Vector2 GetFlagPosition()
    {
        if (Flag != null)
        {
            return new Vector2(Flag.position.x, Flag.position.y);
        }
        return Vector2.zero;
    }
}
