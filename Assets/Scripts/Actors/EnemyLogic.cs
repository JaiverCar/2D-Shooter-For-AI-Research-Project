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
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public bool isHiveNode = false;

    public bool isTower = false;

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

    //Timers
    private float MoveVerticalTimer = 0.0f; //Keeps enemies from jittering against walls
    private float MoveHorizontalTimer = 0.0f; //Keeps enemies from jittering against walls

    //Track the player for aggro and targeting purposes
    [HideInInspector]
    public float repathInterval = 3.0f;

    //Astar stuff
    List<Vector2> path;
    int waypointIndex;
    Vector2 currTarget;
    [Header("Variables for Astar:")]
    public bool doAstar = true;
    public Vector2 AstarTarget = new Vector2(0, 0);
    public bool drawAStarPath = false;
    public bool actionChange = false;
    // used to draw the Astar path
    private LineRenderer debugAStarPath;
    // used to draw the Astar circles
    List<LineRenderer> debugDrawCircles = new List<LineRenderer>();

    // Stable path origin: pathfind from the last committed grid node, not the live transform
    Node lastPathNode = null;
    Node currentGridNode = null;
    Vector2 lastTargetGridPos = Vector2.zero;
    Vector2 targetGridPos = Vector2.zero;
    float repathThresholdSqr = 4.0f; // repath when target moves ~2 grid units

    //Brain
    public Brain thisBrain;

    private Scanner thisScanner;

    public bool seesFlag = false;
    public Vector2 flagLastKnownLocation = Vector2.zero;

    public bool seesPlayer = false;
    public bool hiveSeesPlayer = false;
    public Vector2 lastKnownPlayerLocation = Vector2.zero;

    //Don't do anything because a cinematic occuring
    [HideInInspector]
    public bool CinematicMode = false;

    bool advancedThisFrame = false;

    // Start is called before the first frame update
    Vector2 lastMoveDirection = Vector2.zero;
    float directionBlendSpeed = 10.0f; // How fast to blend between old and new direction
    float repathTimer = 0.0f;

    public bool hasFlag = false;

    float prevSignal;
    float signal;

    public static event Action<EnemyLogic> OnEnemyDied;

    private static float hiveNodeMaxTime = 15.0f;
    private float hiveNodeTimer = hiveNodeMaxTime;
    private bool damageLock = false;

    void Start()
    {
        //Initialize enemy health and health bar
        EnemyHealthBar = transform.Find("EnemyHealthBar").GetComponent<HealthBar>();
        EnemyHealthBar.MaxHealth = StartingHealth;
        EnemyHealthBar.Health = StartingHealth;
        Health = StartingHealth;

        // Register this enemy with TerrainAnalysis for vision tracking
        if (TerrainAnalysis.Instance != null)
        {
            TerrainAnalysis.Instance.RegisterEnemy(transform);
        }

        // get a ref to our flock controller
        flockController = FindObjectOfType<FlockController>();

        thisBrain = GetComponent<Brain>();
        thisBrain.thisEnemy = this;

        thisScanner = GetComponent<Scanner>();


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

        if (isHiveNode == true)
        {
            var hiveMind = FindObjectOfType<HiveMind>();

            if (hiveMind != null)
            {
                hiveMind.globalSignalStrength -= 0.5f;

                if (hiveMind.globalSignalStrength < 0.0f)
                {
                    hiveMind.globalSignalStrength = 0.0f;
                }
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (drawAStarPath == true && isHiveNode == false && isTower == false)
        {
            DebugDrawAStarPath();
        }
        else 
        {
            if (debugAStarPath != null)
            {
                Destroy(debugAStarPath.gameObject);
                debugAStarPath = null;
            }

            foreach (var debugCircle in debugDrawCircles)
            {
                Destroy(debugCircle.gameObject);
            }
            debugDrawCircles.Clear();
        }

        //Don't do anything if in cinematic mode
        if (CinematicMode)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            return;
        }

        if (thisBrain)
        {
            Vector2 desiredTarget = thisBrain.context.getTarget();

            // Only update target if it actually changed
            if (AstarTarget != desiredTarget)
            {
                Debug.Log(gameObject.name + " switching to " + desiredTarget);
                AstarTarget = desiredTarget;
            }

            // Pathfinding logic - check if we need to repath (AI HELPED HERE)
            repathTimer += Time.deltaTime;

            // Check if we've reached the current waypoint (or path is invalid)
            bool atWaypoint = path == null || waypointIndex >= path.Count ||
                              Vector2.Distance(transform.position, path[waypointIndex]) < 0.05f;

            // Check if we're at the end of the path completely
            bool atEndOfPath = path != null && waypointIndex >= path.Count;

            // START COPILOT HELP

            // Repath if:
            // - Target changed (immediate aggro response)
            // - At end of path and timer elapsed (keep following)
            // - Timer elapsed AND target moved AND at a waypoint (smooth tracking)
            // - Path is null (initial state) <- ADDED THIS CHECK
            // Update current grid node and target grid pos each frame
            currentGridNode = AStarGrid.Instance.NodeFromWorldPoint(transform.position);
            if (AstarTarget != Vector2.zero)
                targetGridPos = AStarGrid.Instance.NodeFromWorldPoint(AstarTarget).pos;

            bool cooldownReady = repathTimer >= repathInterval;
            bool targetMovedEnough =
                (targetGridPos - lastTargetGridPos).sqrMagnitude >= repathThresholdSqr;

            signal = Mathf.Clamp01(thisBrain.personalConnection * (HiveMind.Instance != null ? HiveMind.Instance.globalSignalStrength : 1f));

            bool shouldRepath =
                path == null ||
                (cooldownReady && atWaypoint && (targetMovedEnough || atEndOfPath) ||
                (signal != prevSignal));

            //repath if action changed
            if(actionChange)
            {
                actionChange = false;
                shouldRepath = true;
            }

            prevSignal = signal;

            //used for stupidity and weight

            if (AstarTarget != Vector2.zero && shouldRepath)
            {
                // Use last committed grid node as path origin to prevent jitter
                Node originNode = lastPathNode ?? currentGridNode;
                Vector2 pathOrigin = originNode != null ? originNode.worldPosition : transform.position;

                lastPathNode = currentGridNode;
                lastTargetGridPos = targetGridPos;
                repathTimer = 0f;

                float pathWeight = Mathf.Lerp(2.0f, 1.0f, signal);
                float pathStupidity = Mathf.Lerp(10.0f, 0.0f, signal);

                List<Vector2> newPath = Pathfinder.Instance.FindPath(pathOrigin, AstarTarget, pathWeight, pathStupidity, signal > 0.7 , true);
                if (newPath != null && newPath.Count > 1)
                {
                    path = newPath;
                    waypointIndex = 0;
                }
            }

            // END COPILOT HELP
        }



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

        // Add null check for thisScanner
        if (thisScanner != null)
        {
            if (seesPlayer = thisScanner.canSeePlayer)
            {
                lastKnownPlayerLocation = thisScanner.GetPlayerPosition();
                SetAggroState(true);

                if (thisBrain != null && thisBrain.isConnectedToHive)
                {
                    HiveMind.Instance.ReportSeeingPlayer(lastKnownPlayerLocation);
                }
            }
            else
            {
                SetAggroState(false);
            }

            if(thisScanner.canSeeFlag)
            {
                flagLastKnownLocation = thisScanner.GetFlagPosition();
                seesFlag = true;

                if (thisBrain != null && thisBrain.isConnectedToHive)
                {
                    HiveMind.Instance.ReportSeeingFlag(flagLastKnownLocation);
                }
            }
            else
            {
                seesFlag = false;
            }
        }
        else
        {
            thisScanner = GetComponent<Scanner>();
        }


        // if this is not a leader it will be affected by the flocking logic
        if (isLeader == false && isHiveNode == false && isTower == false)
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

            astarDir = astarDir.normalized;


            Vector2 blendedDir = (astarDir + (Vector2)flockDir * 1.5f).normalized;

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
                    // Commit the reached waypoint node as the new stable path origin
                    lastPathNode = AStarGrid.Instance.NodeFromWorldPoint(currTarget);
                    waypointIndex++;
                    advancedThisFrame = true;
                }
            }
        }
        // if this is a leader
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
                    // Commit the reached waypoint node as the new stable path origin
                    lastPathNode = AStarGrid.Instance.NodeFromWorldPoint(currTarget);
                    waypointIndex++;
                    advancedThisFrame = true;
                }
            }

            if(isHiveNode && damageLock)
            {
                hiveNodeTimer -= Time.deltaTime;
                if(hiveNodeTimer <= 0.0f)
                {
                    hiveNodeTimer = hiveNodeMaxTime;
                    damageLock = false;
                    Health = StartingHealth;
                    HiveMind.Instance.globalSignalStrength += 0.50f;
                    if(HiveMind.Instance.globalSignalStrength > 1.0f)
                    {
                        HiveMind.Instance.globalSignalStrength = 1.0f;
                    }
                    GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 109f / 255f, 13f / 255f);
                }
            }
        }
    }


    //Not using a normal getter
    public bool IsAggroed()
    {
        return Aggroed;
    }

    //May need to update the deaggro range...
    public void SetAggroState(bool active)
    {
        Aggroed = active;
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

    public void ResetAstar()
    {
        path = null;
        currentGridNode = null;
        lastPathNode = null;
    }

    //Check to see if we are hit by a bullet
    private void OnTriggerEnter2D(Collider2D col)
    {
        var bullet = col.GetComponent<BulletLogic>();
        //Check for an enemy bullet
        if (bullet != null && bullet.Team == Teams.Player)
        {
            if (!damageLock)
            {
                Health -= 1;
            }
            GetComponent<EnemyLogic>().SetAggroState(true); //Aggro when hit
            if (Health <= 0) //We're dead, so destroy ourself
            {
                if (isTower)
                {
                    Destroy(this.gameObject);
                }
                if (isHiveNode)
                {
                    HiveMind.Instance.globalSignalStrength -= 0.50f;
                    if(HiveMind.Instance.globalSignalStrength < 0.0f)
                    {
                        HiveMind.Instance.globalSignalStrength = 0.0f;
                    }
                    damageLock = true;
                    GetComponent<SpriteRenderer>().color = Color.gray;
                    return;
                }
                var enemyGoal = GameObject.Find("EnemyTower");
                if (enemyGoal != null)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) <= DropChance)
                        Instantiate(PCGObject.Prefabs["heart"], transform.position, Quaternion.identity);

                    Health = StartingHealth;
                    OnEnemyDied?.Invoke(this);
                    transform.position = enemyGoal.transform.position;
                    ResetAstar();

                    HiveMind.Instance.SwitchSquad(thisBrain, HiveMind.squads.s_Scouts);
                }
                else
                {
                    //fire an event when an enemy dies:
                    OnEnemyDied?.Invoke(this);
                    Destroy(gameObject);
                }
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
    
    private void DebugDrawAStarPath()
    {
        // Don't draw if the path is null
        if (path == null || path.Count == 0) return;

        // Delete the old Line Renderer
        if (debugAStarPath != null)
        {
            Destroy(debugAStarPath.gameObject);
            debugAStarPath = null;
        }

        // delete all the old circles
        foreach (var debugCircle in debugDrawCircles)
        {
            Destroy(debugCircle.gameObject);
        }
        debugDrawCircles.Clear();

        // Create a new Line Renderer
        debugAStarPath = CreateDebugLine(path.Count, Color.red, Color.green);

        for (int i = 0; i < path.Count; i++)
        {
            // color goes from red at start to green at end
            Color setColor = Color.Lerp(Color.red, Color.green, (float)i / path.Count);
            DebugDrawCircle(path[i], 0.1f, setColor);

            debugAStarPath.SetPosition(i, path[i]);

        }

        if (waypointIndex < path.Count)
            DebugDrawCircle(path[waypointIndex], 0.1f, Color.red, 9999);
    }

    private void DebugDrawCircle(Vector2 center, float radius, Color color, int renderOrder = 9998, float duration = 0f, int segments = 32)
    {
        LineRenderer debugCircle = CreateDebugLine(segments, color, color, renderOrder);

        debugDrawCircles.Add(debugCircle);

        float angleStep = 360f / segments;
        float angle = 0f;

        Vector2 prevPoint = center + new Vector2(Mathf.Cos(0), Mathf.Sin(0)) * radius;

        Vector2 startPos = prevPoint;

        for (int i = 0; i < segments; i++)
        {
            angle += angleStep * Mathf.Deg2Rad;

            debugCircle.SetPosition(i, prevPoint);

            Vector2 nextPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            prevPoint = nextPoint;
        }

        angle += angleStep * Mathf.Deg2Rad;

        debugCircle.SetPosition(31, prevPoint);
    }


    // start color = red, end color = green
    public LineRenderer CreateDebugLine(int segmentCount, Color startColor, Color endColor, int renderOrder = 9997, float width = 0.05f)
    {
        var go = new GameObject("DebugLine");
        var lr = go.AddComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.renderQueue = renderOrder;
        lr.startColor = startColor;
        lr.endColor = endColor;
        lr.startWidth = lr.endWidth = width;
        lr.positionCount = segmentCount;
        lr.useWorldSpace = true;

        return lr; 
    }
}


