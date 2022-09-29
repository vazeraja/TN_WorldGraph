using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class Transition {
        public WorldStateGraph StateGraph;
        
        public string OutputStateGUID;
        public string InputStateGUID;
        public SceneStateData OutputState;
        public SceneStateData InputState;
        
        public override string ToString() {
            return $"{OutputState.SceneName} ---> {InputState.SceneName}";
        }
    }
    
    [Serializable]
    public class StateTransition : Transition {

        public StateTransition(WorldStateGraph graph, SceneStateData output, SceneStateData input) {
            StateGraph = graph;
            OutputStateGUID = output.GUID;
            OutputState = output;
            InputStateGUID = input.GUID;
            InputState = input;
        }

        public List<Condition> conditions;
    }

}