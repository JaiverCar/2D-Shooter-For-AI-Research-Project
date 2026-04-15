using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityAI;

public class SquadLeader : MonoBehaviour
{
    public HiveMind mainHiveMind; // Reference to main HiveMind
    public HiveMind.squads squadType;
    public List<Brain> subordinates = new List<Brain>();
    
    void Start()
    {
        // Find and connect to main HiveMind
        mainHiveMind = HiveMind.Instance;
        if(mainHiveMind != null)
        {
            mainHiveMind.RegisterSquadLeader(this);
        }
        
        // Connect all subordinates
        foreach(var brain in subordinates)
        {
            ConnectSubordinate(brain);
        }
    }
    
    public void ConnectSubordinate(Brain brain)
    {
        subordinates.Add(brain);
        brain.squadLeader = this; // Give them reference to me
        brain.squad = squadType;
        
        if(mainHiveMind != null)
        {
            brain.hiveMind = mainHiveMind; // Give them access to main hive
        }
    }
    
    void OnDestroy()
    {
        // Squad leader died - disconnect all subordinates
        if(mainHiveMind != null)
        {
            mainHiveMind.UnregisterSquadLeader(this);
        }
        
        foreach(var brain in subordinates)
        {
            brain.hiveMind = null;
            brain.squadLeader = null;
            // They go independent now
        }
    }
}


