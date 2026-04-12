using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UtilityAI
{
    public class Context
    {
        public Brain brain;
        public Transform target;

        readonly Dictionary<string, object> data = new();

        public Context(Brain brain)
        {
            this.brain = brain;
        }

        public T GetData<T>(string key) => data.TryGetValue(key, out var value) ? (T)value : default;
        public void SetData(string key, object value) => data[key] = value;
    }

    public class LeaderContext
    {

    }
}
