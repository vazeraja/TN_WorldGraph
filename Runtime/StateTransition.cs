using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.WorldGraph {
    
    [Serializable]
    public class StateTransition {
        public string OutputStateGUID;
        public string InputStateGUID;
        public SceneStateData OutputState;
        public SceneStateData InputState;

        // public List<Condition> conditions;

        public override string ToString() {
            return $"{OutputState.SceneName} ---> {InputState.SceneName}";
        }
    }

}