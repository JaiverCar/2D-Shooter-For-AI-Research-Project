using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityAI;

public class HiveMind : MonoBehaviour
{
    public static HiveMind Instance; // Singleton
    
    public Vector2 lastKnownPlayerPosition;
    public Vector2 lastKnownFlagPosition;
    
    public float globalSmartness = 1.0f;
    public float globalCoordination = 1.0f;
    
    public List<SquadLeader> activeSquadLeaders = new List<SquadLeader>();
    
    void Awake()
    {
        Instance = this;
    }
    
    public void RegisterSquadLeader(SquadLeader leader)
    {
        activeSquadLeaders.Add(leader);
        RecalculateStats();
    }
    
    public void UnregisterSquadLeader(SquadLeader leader)
    {
        activeSquadLeaders.Remove(leader);
        RecalculateStats();
    }
    
    void RecalculateStats()
    {
        // Stats based on how many active squads exist
        int totalSubordinates = 0;
        foreach(var leader in activeSquadLeaders)
            totalSubordinates += leader.subordinates.Count;
            
        globalCoordination = 1 - (1f / (totalSubordinates + 1));
    }

    public enum squads
    {
        s_FlagDefenders,
        s_FlagAttackers,
        s_Scouts,
        s_PlayerAttackers
    }


}
