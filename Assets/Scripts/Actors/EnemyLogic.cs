/*******************************************************************************
File:      EnemyLogic.cs
Author:    Benjamin Ellinger
DP Email:  bellinge@digipen.edu
Date:      11/11/2022
Course:    DES 214

Description:
    Handles enemy stats, movement, and aggro behavior.

*******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityAI;
using static PCG;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyLogic : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////////////
    // DESIGNER ADJUSTABLE VALUES
    //////////////////////////////////////////////////////////////////////////
    
    // bool to check if this is a Leader
    public bool isLeader = false;
    // bool to check if this is a Grunt
    public bool isGrunt = false;

    public bool doChasePlayer = true;

    // tile to move to
    public Vector2 movementTargetTile;
    // Maximum Movement speed
    private float maxSpeed = 0.0f;
    // Current Movement speed
    public float Speed = 100.0f;
    //Starting health
    public int StartingHealth = 1;
    //Distance at which the enemy will attack
    public float AggroRange = 1.0f;
    //Time between checks for wandering a random direction
    public float WanderInterval = 4.0f;
    //Time between checks for wandering a random direction
    public float WanderChance = 0.3f; //30% chance
    //Chance of dropping a heart on death
    public float DropChance = 0.35f; //35% chance

    //////////////////////////////////////////////////////////////////////////
    [Header("Vision-Based Aggro:")]
    [Tooltip("Use vision layer for aggro detection instead of simple distance")]
    public bool useVisionBasedAggro = true;
    [Tooltip("Vision threshold - player must be at least this visible to trigger aggro (0-1)")]
    [Range(0f, 1f)]
    public float visionAggroThreshold = 0.1f;

    //////////////////////////////////////////////////////////////////////////
    [Header("Variables for flocking:")]
    [SerializeField]
    private float echoRadius = 0;
    // ref to this enemies leader
    private LeaderLogic leader;
    // ref to the flock controller
    private FlockController flockController;

    //Current health
    [HideInInspector]
    public int Health
    {
        get { return _Health; }
        set { EnemyHealthBar.Health = value; _Health = value;}
    }
    private int _Health;
    //Reference to the health bar
    private HealthBar EnemyHealthBar;

    //Current aggro state
    [HideInInspector]
    public bool Aggroed = false;
    //Minimum deaggro range, which is calculated based on
    //enemy aggro range and player range
    private float MinDeaggroRange = 0.0f; //Should always be more than the aggro range

    //Current wander state
    [HideInInspector]
    public bool Wander = false;

    //Timers
    private float Timer = 0.0f;
    private float MoveVerticalTimer = 0.0f; //Keeps enemies from jittering against walls
    private float MoveHorizontalTimer = 0.0f; //Keeps enemies from jittering against walls

    //Track the player for aggro and targeting purposes
    [HideInInspector]
    public Transform Player = null;
    public float repathInterval = 0.2f;

    //Astar stuff
    List<Vector2> path;
    int waypointIndex;
    Vector2 currTarget;
    [Header("Variables for Astar:")]
    public bool doAstar = true;
    public Transform Flag = null;
    public Transform AstarTarget = null;

    //Brain
    private Brain thisBrain;

    //Don't do anything because a cinematic occuring
    [HideInInspector]
    public bool CinematicMode = false;

    bool advancedThisFrame = false;

    // Start is called before the first frame update

    Transform lastTarget;
    Vector2 lastTargetPos;
    Vector2 lastMoveDirection = Vector2.zero;
    float directionBlendSpeed = 10.0f; // How fast to blend between old and new direction
    float repathTimer = 0.0f;

    public static event Action<EnemyLogic> OnEnemyDied;

    void Start()
    {
        //Set the minimum range at which aggro will be dropped to 150% of the aggro range
        MinDeaggroRange = AggroRange * 1.5f;

        //Initialize enemy health and health bar
        EnemyHealthBar = transform.Find("EnemyHealthBar").GetComponent<HealthBar>();
        EnemyHealthBar.MaxHealth = StartingHealth;
        EnemyHealthBar.Health = StartingHealth;
        Health = StartingHealth;

        GetPlayerReference();
        GetFlagReference();

        // Register this enemy with TerrainAnalysis for vision tracking
        if (TerrainAnalysis.Instance != null)
        {
            TerrainAnalysis.Instance.RegisterEnemy(transform);
        }

        // get a ref to our flock controller
        flockController = FindObjectOfType<FlockController>();

        thisBrain = GetComponent<Brain>();
        thisBrain.thisEnemy = this;

        // set our max speed
        maxSpeed = Speed;
    }

    void OnDestroy()
    {
        // Unregister this enemy when it's destroyed
        if (TerrainAnalysis.Instance != null)
        {
            TerrainAnalysis.Instance.UnregisterEnemy(transform);
        }
    }


    // Update is called once per frame
    void Update()
    {


        //Don't do anything if in cinematic mode
        if (CinematicMode)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            return;
        }

        // Check to see if we have a reference to the player and flag
        GetFlagReference();
        GetPlayerReference();

        if (thisBrain)
        {
            // Determine the desired target based on aggro state
            thisBrain.context.target = (Aggroed == true && Wander == false) ? Player : Flag;
            Transform desiredTarget = thisBrain.context.target;

            // Only update target if it actually changed
            if (AstarTarget != desiredTarget)
            {
                Debug.Log(gameObject.name + " switching to " + desiredTarget.name);
                AstarTarget = desiredTarget;
            }

            // Pathfinding logic - check if we need to repath (AI HELPED HERE)
            repathTimer += Time.deltaTime;

            bool targetChanged = lastTarget != AstarTarget;
            bool targetMoved = AstarTarget != null && Vector2.Distance(AstarTarget.position, lastTargetPos) > 0.3f;

            // Check if we've reached the current waypoint (or path is invalid)
            bool atWaypoint = path == null || waypointIndex >= path.Count ||
                              Vector2.Distance(transform.position, path[waypointIndex]) < 0.16f;

            // Check if we're at the end of the path completely
            bool atEndOfPath = path != null && waypointIndex >= path.Count;

            // START AI HELP

            // Repath if:
            // - Target changed (immediate aggro response)
            // - At end of path and timer elapsed (keep following)
            // - Timer elapsed AND target moved AND at a waypoint (smooth tracking)
            if (AstarTarget != null &&
                (targetChanged ||
                 (atEndOfPath && repathTimer >= repathInterval) ||
                 (repathTimer >= repathInterval && targetMoved && atWaypoint)))
            {
                lastTarget = AstarTarget;
                lastTargetPos = AstarTarget.position;

                List<Vector2> newPath = Pathfinder.Instance.FindPath(transform.position, AstarTarget.position);
                if (newPath != null && newPath.Count > 1)
                {
                    newPath.RemoveAt(0);
                    path = newPath;
                    waypointIndex = 0;
                }

                repathTimer = 0.0f;
            }

            // END AI HELP


            // if the path is empty or there we are at the end of it, stop moving (ASTAR)
            if (doAstar == true && (path == null || waypointIndex >= path.Count))
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                return;
            }

            // set the advanced flag to false and update current move target (ASTAR)
            advancedThisFrame = false;

            if (doAstar == false)
            {
                advancedThisFrame = true;
            }

            if (path != null && waypointIndex < path.Count)
            {
                currTarget = path[waypointIndex];
            }
        }


        if (doChasePlayer == true && (Player == null || !Player.gameObject.activeInHierarchy))
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            return;
        }

        if (path == null || waypointIndex >= path.Count)
        {
            // fall back to wandering
            WanderingUpdate();
            if (Wander)
            {
                GetComponent<Rigidbody2D>().velocity = transform.up * Speed;
            }
            else
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
            return;
        }

        //No reference to an active player, nothing to chase
        if ((Player == null || !Player.gameObject.activeInHierarchy) && doChasePlayer == true)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            SetAggroState(false);
            return;
        }

        //      //If player is within aggro range, chase it!
        //var playerDir = (Player.position - transform.position);

        // Use vision-based aggro if enabled

        if (CanSeePlayerVision())
        {
            if (Aggroed == false)
            {
                Wander = false;
                Timer = 0;
                Debug.Log($"{gameObject.name} detected player via VISION!");
            }
            SetAggroState(true);
        }
        else
        {
            SetAggroState(false);
        }

        // if this is not a leader it will be affected by the flocking logic
        if (isLeader == false)
        {
            Vector3 movementTargetLocation = currTarget;

            Vector3 flockDir = GetComponent<FlockingLogic>().GetDirection();
            Vector2 astarDir = currTarget - (Vector2)transform.position;
            // if the distance between us and our Astar target goes bellow this threshhold
            if (astarDir.magnitude < flockController.GetFlockingAstarPathThreshHoldDistance())
            {
                //disable certain parts of flocking so that the enemy can follow the Astar path correctly
                GetComponent<FlockingLogic>().DoCohesion(false);
                GetComponent<FlockingLogic>().DoAlignment(false);
                GetComponent<FlockingLogic>().DoReducedSeparationRadius(true);
            }
            //else 
            //{
            //    //
            //    GetComponent<FlockingLogic>().DoCohesion(true);
            //    GetComponent<FlockingLogic>().DoAlignment(true);
            //}

                // normalize the direction to our Astar target to add it to our blended direction
                astarDir = astarDir.normalized;


            Vector2 blendedDir = (astarDir + (Vector2)flockDir * 1.5f).normalized; // Increase from 0.3 to 1.5

            if (HasReachedMovementTarget(movementTargetLocation) == false)
            {
                if (blendedDir.magnitude > 0.01f)
                    transform.up = SnapVectorToGrid(blendedDir, MoveVerticalTimer > 0, MoveHorizontalTimer > 0);

                GetComponent<Rigidbody2D>().velocity = transform.up * Speed;
            }
            else
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                if (!advancedThisFrame)
                {
                    waypointIndex++;
                    advancedThisFrame = true;
                }
            }
        }
        // if this is a leader, then it will only move around with A*
        else
        {
            Vector3 movementTargetLocation = currTarget;
            Vector2 astarDir = (currTarget - (Vector2)transform.position).normalized;

            // Smoothly blend from last direction to new direction to prevent flipping
            if (lastMoveDirection != Vector2.zero)
            {
                astarDir = Vector2.Lerp(lastMoveDirection, astarDir, Time.deltaTime * directionBlendSpeed);
            }
            lastMoveDirection = astarDir;

            if (HasReachedMovementTarget(movementTargetLocation) == false)
            {
                if (astarDir.magnitude > 0.01f)
                    transform.up = SnapVectorToGrid(astarDir, MoveVerticalTimer > 0, MoveHorizontalTimer > 0);

                GetComponent<Rigidbody2D>().velocity = transform.up * Speed;
            }
            else
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                if (!advancedThisFrame)
                {
                    waypointIndex++;
                    advancedThisFrame = true;
                }
            }
        }
    }


    //Not using a normal getter and setter so that these calls are more explicit
    public bool IsAggroed()
    {
        return Aggroed;
    }

    //May need to update the deaggro range...
    public void SetAggroState(bool active)
    {
        //If we are just detecting the player, recalculate deaggro ranges
        if (active == true && Aggroed == false)
            RecalculateDeaggroRange();

        //Set the aggro state
        Aggroed = active;
    }

    //Get a reference to the player
    void GetFlagReference()
    {
        //Already tracking the player
        if (Flag != null)
            return;

        //Find the player
        var flag = GameObject.Find("Flag");
        if (flag == null)
            return;
        Flag = flag.transform;
    }

    void GetPlayerReference()
    {
        //Already tracking the player
        if (Player != null)
            return;

        //Find the player
        var player = GameObject.Find("Player(Clone)");
        if (player == null)
            return;
        Player = player.transform;
    }

    //Increase the deaggro range as the player's weapons get longer ranges
    void RecalculateDeaggroRange()
    {
        if (Player == null)
            return;

        //Find the maximum range of all player weapons
        var maxBulletRange = 0.0f;
        for (int i = 0; i < Player.childCount; i++)
        {
            Transform child = Player.GetChild(i);
            WeaponLogic weapon = child.GetComponent<WeaponLogic>();
            if (weapon != null && weapon.BulletRange > maxBulletRange)
                maxBulletRange = weapon.BulletRange;
        }

        //If this range is less than 150% of the max weapon range, use that instead
        if (MinDeaggroRange < maxBulletRange * 1.5f)
            MinDeaggroRange = maxBulletRange * 1.5f;
    }

    // Check if this enemy can see the player using vision layer
    bool CanSeePlayerVision()
    {
        // Safety checks
        if (Player == null || TerrainAnalysis.Instance == null || AStarGrid.Instance == null)
            return false;

        // Get this enemy's vision layer
        LayerVisualization visionLayer = TerrainAnalysis.Instance.GetEnemyVisionLayer(transform);
        if (visionLayer == null || visionLayer.layer == null)
            return false;

        // Convert player world position to grid coordinates
        Vector2 playerWorldPos = new Vector2(Player.position.x, Player.position.y);
        Node playerNode = AStarGrid.Instance.NodeFromWorldPoint(playerWorldPos);

        // Check if player position is valid on the grid
        if (!AStarGrid.Instance.IsValidGridPos(new Vector2(playerNode.gridX, playerNode.gridY)))
            return false;

        // Get the vision value at the player's grid position
        // Note: layer.GetValue takes (row, col) which is (gridY, gridX)
        float visionValue = visionLayer.layer.GetValue(playerNode.gridY, playerNode.gridX);

        // Player is visible if vision value exceeds threshold
        return visionValue >= visionAggroThreshold;
    }

    //Update the wandering state
    void WanderingUpdate()
    {
        //Check to see if we should wander
        if (Wander == false && Timer >= WanderInterval)
        {
            if (UnityEngine.Random.Range(0.0f, 1.0f) <= WanderChance)
            {
                Wander = true;
                //Pick a random direction, but account for whether the enemy is up against a wall
                transform.up = SnapVectorToGrid(UnityEngine.Random.insideUnitCircle, MoveVerticalTimer > 0, MoveHorizontalTimer > 0);
            }
            Timer = 0.0f;
        }

        //Check to see if it is time to stop wandering
        if (Wander == true && Timer >= WanderInterval / 4.0f)
        {
            //Stop wandering at one quarter the wander interval if aggroed, half if not
            if (Aggroed == true || Timer >= WanderInterval / 2.0f)
            {
                Wander = false;
                Timer = 0.0f;
            }
        }
    }

    //Snap this vector to only going vertical and/or horizontal
    //This allows and enemy to move along a wall instead of getting stuck
    private Vector3 SnapVectorToGrid(Vector3 v, bool vert, bool horiz)
    {
        var snappedVector = v;
        if (vert == true && horiz != true)
            snappedVector.x = 0;
        if (horiz == true && vert != true)
            snappedVector.y = 0;
        if (snappedVector.magnitude <= 0.05f)
            return v.normalized;
        return snappedVector.normalized;
    }

    //Check to see if the enemy has hit another enemy, the player, or is up against a wall
    private void OnCollisionStay2D(Collision2D col)
    {
        //Aggro on friendly collision if the other enemy is already aggroed
        var enemy = col.gameObject.GetComponent<EnemyLogic>();
        if (enemy != null && enemy.IsAggroed() == true)
        {
            GetComponent<EnemyLogic>().SetAggroState(true);
            return;
        }

        //Aggro on collision with player
        var player = col.gameObject.GetComponent<PlayerLogic>();
        if (player != null)
        {
            GetComponent<EnemyLogic>().SetAggroState(true);
            return;
        }

        if (col.gameObject.ToString().StartsWith("Wall") == false)
            return;
        //This a wall, so figure out whether it is horizontal or vertical
        var wallTransform = col.collider.transform;
        var xdist = Math.Abs(transform.position.x - wallTransform.position.x);
        var ydist = Math.Abs(transform.position.y - wallTransform.position.y);
        //If it is horizontal, reset the horizontal move timer so we only move horizontal for a bit
        if (xdist < ydist &&
            xdist <= wallTransform.localScale.x / 2.0f + transform.localScale.x / 2.0f &&
            MoveHorizontalTimer < -0.25f)
            MoveHorizontalTimer = 0.5f;
        //If it is vertical, reset the vertical move timer so we only move vertical for a bit
        if (ydist < xdist &&
            ydist <= wallTransform.localScale.y / 2.0f + transform.localScale.x / 2.0f &&
            MoveVerticalTimer < -0.25f)
            MoveVerticalTimer = 0.5f;
        //These delays on checking prevent the enemy from jittering back and forth on a wall
    }

    //Check to see if we are hit by a bullet
    private void OnTriggerEnter2D(Collider2D col)
    {
        var bullet = col.GetComponent<BulletLogic>();
        //Check for an enemy bullet
        if (bullet != null && bullet.Team == Teams.Player)
        {
            Health -= 1;
            GetComponent<EnemyLogic>().SetAggroState(true); //Aggro when hit
            if (Health <= 0) //We're dead, so destroy ourself
            {
                //fire an event when an enemy dies:
                OnEnemyDied?.Invoke(this);

                if (UnityEngine.Random.Range(0.0f, 1.0f) <= DropChance)
                    Instantiate(PCGObject.Prefabs["heart"], transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
        //Aggro but no damage on friendly fire
        if (bullet != null && bullet.Team != Teams.Player)
            GetComponent<EnemyLogic>().SetAggroState(true);
    }

    private bool HasReachedMovementTarget(Vector3 movementTargetLocation)
    {
        if (Vector3.Distance(movementTargetLocation, transform.position) < 0.15f)
        {
            return true;
        }

        return false;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public float GetCurrentSpeed()
    {
        return Speed;
    }

    public void SetCurrentSpeed(float newSpeed)
    {
        Speed = newSpeed;
    }

    public float GetEchoRadius()
    {
        return echoRadius;
    }

    // returns true if this enemy has a ref to a leader, false if not 
    public bool HasLeader()
    {
        if (leader != null)
            return true;
        return false;
    }

    // set leader ref to a given leader
    public void SetLeader(LeaderLogic gLeader)
    {
        // the given leader should never be null
        Debug.Assert(gLeader != null);
       
        leader = gLeader;
    }

    // return a ref to this enemies leader
    public LeaderLogic GetLeader() 
    { 
        return leader; 
    }


    private void OnDrawGizmos()
    {
        if (path == null || path.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < path.Count - 1; i++)
            Gizmos.DrawLine(path[i], path[i + 1]);

        Gizmos.color = Color.red;
        if (waypointIndex < path.Count)
            Gizmos.DrawSphere(path[waypointIndex], 0.15f);


        if (path == null || path.Count == 0) return;

        for (int i = 0; i < path.Count; i++)
        {
            // color goes from red at start to green at end
            Gizmos.color = Color.Lerp(Color.red, Color.green, (float)i / path.Count);
            Gizmos.DrawSphere(path[i], 0.1f);

            if (i < path.Count - 1)
                Gizmos.DrawLine(path[i], path[i + 1]);

            // draw the index number
            UnityEditor.Handles.Label(path[i], i.ToString());
        }
    }


}


