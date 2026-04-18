using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UtilityAI
{
    public class Context
    {
        public Brain brain;
        private Vector2 target;

        readonly Dictionary<string, object> data = new();

        public Vector2 wanderTarget = new Vector2(-1, -1);
        public bool hasWanderTarget = false;

        public bool searchingForPlayer = false;
        public bool trackingPlayer = false;

        // In Context.cs
        public Transform playerRef = null;
        public Vector3 lastPlayerPosition;
        public float lastUpdateTime;

        public Vector3 lastFlagPosition;
        public float lastFlagUpdateTime;

        public bool retreating;
        public Vector2 retreatPos;
        public Context(Brain brain)
        {
            this.brain = brain;
        }

        public T GetData<T>(string key)
        {
            //AI HELPED WITH TYPE CONVERSION
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

        public void setTarget(Vector2 newTarget)
        {
            target = newTarget;
        }

        public void setTarget(Transform newTarget)
        {
            target = newTarget.position;
        }

        public Vector2 getTarget() => target;
    }

    public class LeaderContext
    {

    }
}
