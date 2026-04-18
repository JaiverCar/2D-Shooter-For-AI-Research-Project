using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    [Serializable]
    public class HiveMind : MonoBehaviour
    {
        public static HiveMind Instance;

        public List<Brain> subordinates;
        public List<Brain> scouts;

        public bool hiveSeesFlag = false;
        public Vector2 lastKnownFlagPosition;
        public List<Brain> flagAttackers;
        public bool hasFlagSquad;

        public bool hiveSeesPlayer = false;
        public Vector2 lastKnownPlayerPosition;
        public List<Brain> playerAttackers;
        private bool hasPlayerSquad;

        public float timeSincePlayerSeen = 0f;
        public float playerMemoryDuration = 3f;

        public float timeSinceFlagSeen = 0f;
        public float flagMemoryDuration = 3f;

        public float globalSignalStrength = 1.0f;


        

        void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            scouts = subordinates;
        }

        private void Update()
        {
            timeSincePlayerSeen += Time.deltaTime;
            if (timeSincePlayerSeen >= playerMemoryDuration)
            {
                ReportLostPlayer();
            }

            timeSinceFlagSeen += Time.deltaTime;
            if (timeSinceFlagSeen >= flagMemoryDuration)
            {
                ReportLostFlag();
            }
        }

        public void ReportSeeingPlayer(Vector2 position)
        {
            if (!hiveSeesPlayer)
            {
                AssignPlayerAttackers();
            }
            hiveSeesPlayer = true;
            lastKnownPlayerPosition = position;
            timeSincePlayerSeen = 0f;
        }

        private void ReportLostPlayer()
        {
            if (hiveSeesPlayer)
            {
                UnassignPlayerAttackers();
            }
            hiveSeesPlayer = false;
        }

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

        private void ReportLostFlag()
        {
            if (hiveSeesFlag)
            {
                UnassignFlagAttackers();
            }
            hiveSeesFlag = false;
        }

        public void GetKnowledge(Context context)
        {
            context.SetData("hiveSeesPlayer", hiveSeesPlayer);
            context.SetData("hiveLastKnownPlayerPos", lastKnownPlayerPosition);
            context.SetData("hiveSeesFlag", hiveSeesFlag);
            context.SetData("hiveLastKnownFlagPos", lastKnownFlagPosition);
        }

        [SerializeField]
        public enum squads
        {
            s_FlagDefenders,
            s_FlagAttackers,
            s_Scouts,
            s_PlayerAttackers,
            s_NoSquad
        }

        public void AssignPlayerAttackers()
        {
            if (!hasPlayerSquad)
            {
                hasPlayerSquad = true;

                int count = Mathf.Max(1, scouts.Count / 3);

                playerAttackers.Clear();

                List<Brain> sorted = new List<Brain>(scouts);
                sorted.Sort((a, b) =>
                    Vector2.Distance(a.transform.position, lastKnownPlayerPosition)
                    .CompareTo(Vector2.Distance(b.transform.position, lastKnownPlayerPosition)));

                for (int i = 0; i < count && i < sorted.Count; i++)
                {
                    Brain sub = sorted[i];
                    sub.squad = squads.s_PlayerAttackers;
                    playerAttackers.Add(sub);
                    scouts.Remove(sub);
                }
            }
        }

        public void UnassignPlayerAttackers()
        {
            foreach (Brain sub in playerAttackers)
            {
                sub.squad = squads.s_Scouts;
                scouts.Add(sub);
            }
            playerAttackers.Clear();
            hasPlayerSquad = false;
        }

        public void AssignFlagAttackers()
        {
            if (!hasFlagSquad)
            {
                hasFlagSquad = true;

                int count = Mathf.Max(1, scouts.Count / 3);

                flagAttackers.Clear();

                List<Brain> sorted = new List<Brain>(scouts);
                sorted.Sort((a, b) =>
                    Vector2.Distance(a.transform.position, lastKnownFlagPosition)
                    .CompareTo(Vector2.Distance(b.transform.position, lastKnownFlagPosition)));

                for (int i = 0; i < count && i < sorted.Count; i++)
                {
                    Brain sub = sorted[i];
                    sub.squad = squads.s_FlagAttackers;
                    flagAttackers.Add(sub);
                    scouts.Remove(sub);
                }
            }
        }

        public void UnassignFlagAttackers()
        {
            foreach (Brain sub in flagAttackers)
            {
                sub.squad = squads.s_Scouts;
                scouts.Add(sub);
            }
            flagAttackers.Clear();
            hasFlagSquad = false;
        }

        public bool RecievesSignal(float connection)
        {
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

        public void SetSignalStrength(float newSignalStrength)
        {
            globalSignalStrength = newSignalStrength;
        }
    }
}
