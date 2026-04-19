using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * NOTE: Part of this code was reconstructed from youtube tutorials on action curves
 * Video 1: https://www.youtube.com/watch?v=sISJdLO3JYM
 * Video 2: https://www.youtube.com/watch?v=S4oyqrsU2WU
 * 
 * The file was otherwise completely written by us (James Hardy, Javier Carballo Flor)
**************************************************************************************/

namespace UtilityAI
{
    public class Context
    {
        // Reference to associated brain
        public Brain brain;

        // Current Astar target
        private Vector2 target;

        // Dictionary for storing key value pairs of data
        readonly Dictionary<string, object> data = new();

        // values for wandering action
        public Vector2 wanderTarget = new Vector2(-1, -1);
        public bool hasWanderTarget = false;

        // enemy flags for finding player
        public bool searchingForPlayer = false;
        public bool trackingPlayer = false;

        // tracker for player ref and position
        public Transform playerRef = null;
        public Vector3 lastPlayerPosition;
        public float lastUpdateTime;

        // trackers for flag positions
        public Vector3 lastFlagPosition;
        public float lastFlagUpdateTime;

        // Values for retreating action
        public bool retreating;
        public Vector2 retreatPos;

        // Sets the reference to associated brain
        public Context(Brain brain)
        {
            this.brain = brain;
        }

        // Gets data from the dataset
        // Params: key - the key used to look up a value
        // Returns: the value at the key
        public T GetData<T>(string key)
        {
            //Note: copilot helped with type conversion
            if (!data.TryGetValue(key, out var value))
                return default;

            // Handle type conversions, especially for float (needed for AnimationCurves)
            if (typeof(T) == typeof(float))
            {
                if (value is float f) return (T)(object)f;
                if (value is int i) return (T)(object)(float)i;
                if (value is double d) return (T)(object)(float)d;
                if (value is bool b) return (T)(object)(b ? 1f : 0f);

                // Try generic conversion
                try { return (T)(object)System.Convert.ToSingle(value); }
                catch { return default; }
            }

            // Direct cast for same types
            if (value is T directCast)
                return directCast;

            // Fallback
            try { return (T)value; }
            catch { return default; }
        }

        // Set data in the dataset
        // Params:
        // key - the key to store the value with
        // value - the value to store
        // maxVal - the value to compare against when pulling its utility
        public void SetData(string key, object value, float maxVal = 1.0f)
        {
            if (value is float f)
            {
                value = f / maxVal;
            }
            else if (value is int i)
            {
                float floatVal = i;
                value = floatVal / maxVal;
            }
            else if (value is double d)
            {
                float floatVal = (float)d;
                value = floatVal / maxVal;
            }

            data[key] = value;
        }

        // Sets the target for Astar pathfinding
        // Params: newTarget (Vector2) - new target to path to
        public void setTarget(Vector2 newTarget)
        {
            target = newTarget;
        }

        // Sets the target for Astar pathfinding
        // Params: newTarget (Transform) - new target to path to
        public void setTarget(Transform newTarget)
        {
            target = newTarget.position;
        }

        // Gets the current set Astar target
        // Returns: the Astar target position
        public Vector2 getTarget()
        {
            return target;
        }
    }
}
