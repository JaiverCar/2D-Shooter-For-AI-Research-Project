using System;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    [Serializable]
    public class HiveMind : MonoBehaviour
    {
        // Instance for singleton 
        public static HiveMind Instance;

        // Other squad lists
        public List<Brain> subordinates;
        public List<Brain> scouts;
        public List<Brain> disconnected;

        // Variables for hive knowing where flag is
        public List<Brain> flagAttackers;
        public bool hiveSeesFlag = false;
        public Vector2 lastKnownFlagPosition;
        public bool hasFlagSquad;
        public float timeSinceFlagSeen = 0f;
        public float flagMemoryDuration = 5f;

        // Variables for hive knowing where player is
        public List<Brain> playerAttackers;
        public bool hiveSeesPlayer = false;
        public Vector2 lastKnownPlayerPosition;
        private bool hasPlayerSquad;
        public float timeSincePlayerSeen = 0f;
        public float playerMemoryDuration = 3f;
        public int playerSquadCount = 0;

        // Global signal strength (used in determining connection status of enemies)
        public float globalSignalStrength = 1.0f;


        

        void Awake()
        {
            // create singleton
            Instance = this;
        }

        private void Start()
        {
            // Assign all subordinates to scouts 
            scouts = subordinates;
        }

        private void Update()
        {
            // Update time since hive has last reported seeing the player
            timeSincePlayerSeen += Time.deltaTime;
            if (timeSincePlayerSeen >= playerMemoryDuration)
            {
                // Stop attempting to get player if it hasnt been seen
                // in a given amount of time (playerMemoryDuration)
                ReportLostPlayer();
            }
            else
            {
                if(playerAttackers.Count < playerSquadCount)
                {
                    hasFlagSquad = false;
                    AssignFlagAttackers();
                }
            }

            // Update time since hive has last reported seeing the flag
            timeSinceFlagSeen += Time.deltaTime;
            if (timeSinceFlagSeen >= flagMemoryDuration)
            {
                // Stop attempting to get flag if it hasnt been seen
                // in a given amount of time (flagMemoryDuration)
                ReportLostFlag();
            }

        }

        // Report to the hivemind that enemy has seen the player
        // Params: position - last seen player position
        public void ReportSeeingPlayer(Vector2 position)
        {
            // if the hive previously had not seen the player, assign new playerAttacker squad
            if (!hiveSeesPlayer)
            {
                AssignPlayerAttackers();
            }

            hiveSeesPlayer = true;
            lastKnownPlayerPosition = position;
            timeSincePlayerSeen = 0f;
        }

        // Stop chasing player and unassign the playerAttacker squad
        private void ReportLostPlayer()
        {
            if (hiveSeesPlayer)
            {
                UnassignPlayerAttackers();
            }
            hiveSeesPlayer = false;
        }

        // Report to the hivemind that enemy has seen the flag
        // Params: position - last seen flag position
        public void ReportSeeingFlag(Vector2 position)
        {
            if (!hiveSeesFlag)
            {
                AssignFlagAttackers();
            }
            hiveSeesFlag = true;
            lastKnownFlagPosition = position;
            timeSinceFlagSeen = 0f;
        }

        // Stop chasing flag and unassign the flagAttacker squad
        private void ReportLostFlag()
        {
            if (hiveSeesFlag)
            {
                UnassignFlagAttackers();
            }
            hiveSeesFlag = false;
        }

        // Update a context with what the hive knows
        // Params: context - the enemies presonal context for updating
        public void GetKnowledge(Context context)
        {
            context.SetData("hiveSeesPlayer", hiveSeesPlayer);
            context.SetData("hiveLastKnownPlayerPos", lastKnownPlayerPosition);
            context.SetData("hiveSeesFlag", hiveSeesFlag);
            context.SetData("hiveLastKnownFlagPos", lastKnownFlagPosition);
        }

        // Squad enums for assigning enemies
        [SerializeField]
        public enum squads
        {
            s_FlagDefenders,
            s_FlagAttackers,
            s_Scouts,
            s_PlayerAttackers,
            s_NoSquad
        }

        // Assigns the closest enemies to the player attacker squad
        public void AssignPlayerAttackers()
        {
            // Only does this if there are isnt already a player squad
            if (!hasPlayerSquad)
            {
                hasPlayerSquad = true;

                // max amount is 1/3 of current scouts, min is 1 scout
                int count = Mathf.Max(1, scouts.Count / 3);
                playerSquadCount = count;

                // clear current playerAttacker squad
                playerAttackers.Clear();

                // Find nearest enemies to the player
                List<Brain> sorted = closestToPlayer();

                // Assign n of the closest enemies from scouts to the playerAttacker squad
                for (int i = 0; i < count && i < sorted.Count; i++)
                {
                    Brain sub = sorted[i];
                    sub.squad = squads.s_PlayerAttackers;
                    playerAttackers.Add(sub);
                    scouts.Remove(sub);
                }
            }
        }

        // Unassign the enemies from the playerAttacker squad
        public void UnassignPlayerAttackers()
        {
            foreach (Brain sub in playerAttackers)
            {
                // Remove from attacker squad and back into scouts
                sub.squad = squads.s_Scouts;
                scouts.Add(sub);
            }
            playerSquadCount = 0;
            playerAttackers.Clear();
            hasPlayerSquad = false;
        }

        // Assigns the closest enemies to the flag attacker squad
        public void AssignFlagAttackers()
        {
            // Only does this if there are isnt already a flag squad
            if (!hasFlagSquad)
            {
                hasFlagSquad = true;

                // max amount is 1/3 of current scouts, min is 1 scout
                int count = Mathf.Max(1, scouts.Count / 3);

                // clear current flagAttacker squad
                flagAttackers.Clear();

                // Find nearest enemies to the flag
                List<Brain> sorted = closestToFlag();

                // Assign n of the closest enemies from scouts to the flagAttacker squad
                for (int i = 0; i < count && i < sorted.Count; i++)
                {
                    Brain sub = sorted[i];
                    sub.squad = squads.s_FlagAttackers;
                    flagAttackers.Add(sub);
                    scouts.Remove(sub);
                }
            }
        }

        // Unassign the enemies from the flagAttacker squad
        public void UnassignFlagAttackers()
        {
            foreach (Brain sub in flagAttackers)
            {
                // Remove from attacker squad and back into scouts
                sub.squad = squads.s_Scouts;
                scouts.Add(sub);
            }
            flagAttackers.Clear();
            hasFlagSquad = false;
        }

        // Checks if an enemy is able to recieve the signal from the hive mind
        // Params: connection - enemies personal connection strength
        // Returns: if enemy should be connected or not
        public bool RecievesSignal(float connection)
        {
            // Both globalSignalStrength and enemies personal connection must clear 
            // for the enemy to be connected
            if(UnityEngine.Random.value < globalSignalStrength)
            {
                if(UnityEngine.Random.value - 0.2f <= connection)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public List<Brain> closestToPlayer()
        {
            List<Brain> sorted = new List<Brain>(scouts);
            sorted.Sort((a, b) =>
                Vector2.Distance(a.transform.position, lastKnownPlayerPosition)
                .CompareTo(Vector2.Distance(b.transform.position, lastKnownPlayerPosition)));

            return sorted;
        }

        public List<Brain> closestToFlag()
        {
            List<Brain> sorted = new List<Brain>(scouts);
            sorted.Sort((a, b) =>
                Vector2.Distance(a.transform.position, lastKnownFlagPosition)
                .CompareTo(Vector2.Distance(b.transform.position, lastKnownFlagPosition)));

            return sorted;
        }


        // Switches a specific enemy to a target squad
        // Params:
        // brain - the enemy to move
        // targetSquad - the squad to move them to
        public void SwitchSquad(Brain brain, squads targetSquad)
        {
            // Remove brain from its current squad list
            if (scouts.Remove(brain)) 
            { }
            else if (playerAttackers.Remove(brain)) 
            {
                if (hasPlayerSquad && scouts.Count > 0)
                {
                    List<Brain> closest = closestToPlayer();
                    Brain sub = closest[0];
                    sub.squad = squads.s_PlayerAttackers;
                    playerAttackers.Add(sub);
                    scouts.Remove(sub);
                }
            }
            else if (flagAttackers.Remove(brain)) 
            {
                if (hasFlagSquad && scouts.Count > 0)
                {
                    List<Brain> closest = closestToFlag();
                    Brain sub = closest[0];
                    sub.squad = squads.s_FlagAttackers;
                    playerAttackers.Add(sub);
                    scouts.Remove(sub);
                }
            }
            else if (disconnected.Remove(brain))
            {
                if(brain.squad == squads.s_NoSquad)
                {
                    disconnected.Add(brain);
                    return;
                }
            }

            // Add brain to the target squad list and update its squad tag
            switch (targetSquad)
            {
                case squads.s_Scouts:
                    brain.squad = squads.s_Scouts;
                    scouts.Add(brain);
                    break;
                case squads.s_PlayerAttackers:
                    brain.squad = squads.s_PlayerAttackers;
                    playerAttackers.Add(brain);
                    break;
                case squads.s_FlagAttackers:
                    brain.squad = squads.s_FlagAttackers;
                    flagAttackers.Add(brain);
                    break;
                case squads.s_NoSquad:
                    brain.squad = squads.s_NoSquad;
                    disconnected.Add(brain);
                    break;
            }
        }

        // Sets the global signal strength
        // Params: new global signal strength
        public void SetSignalStrength(float newSignalStrength)
        {
            globalSignalStrength = newSignalStrength;
        }
    }
}
