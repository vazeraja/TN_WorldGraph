using System;
using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {
    public interface IDecision {
        bool Decide();
    }

    [Serializable]
    public abstract class Decision : IDecision {

        public object agent;
        public abstract bool Decide();
        public void BindAgent<T>(object type) where T : class {
            agent = type as T;
        }
    }
}