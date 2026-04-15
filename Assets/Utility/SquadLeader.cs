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

            GetComponent<Brain>().hiveMind = mainHiveMind;
        }
        
        // Connect all subordinates
        foreach(var brain in subordinates)
        {
            ConnectSubordinate(brain);
        }
    }

    public void UpdateAggro(bool knowPlayerLocation, Vector2 lastKnownFlagPosition)
    {
        //update our own brain
        GetComponent<Brain>().context.SetData("aggroed", knowPlayerLocation);
        GetComponent<Brain>().context.lastPlayerPosition = lastKnownFlagPosition;


        //update all our subordinates
        foreach (var brain in subordinates)
        {
            brain.context.SetData("aggroed", knowPlayerLocation);
            brain.context.lastPlayerPosition = lastKnownFlagPosition;
        }
    }
    
    public void ConnectSubordinate(Brain brain)
    {
        brain.squadLeader = this; // Give them reference to me
        brain.squad = squadType;
        
        if(mainHiveMind != null)
        {
            brain.hiveMind = mainHiveMind; // Give them access to main hive
        }
    }

    public void RemoveSubordinate(Brain brain)
    {
        subordinates.Remove(brain);
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


