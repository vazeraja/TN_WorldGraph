using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class Transition : ISerializationCallbackReceiver {
        public WorldStateGraph StateGraph;

        public string OutputStateGUID;
        public string InputStateGUID;
        public SceneStateData OutputState;
        public SceneStateData InputState;

        public Transition(WorldStateGraph graph, SceneStateData output, SceneStateData input) {
            StateGraph = graph;
            OutputStateGUID = output.GUID;
            OutputState = output;
            InputStateGUID = input.GUID;
            InputState = input;
        }

        public override string ToString() {
            return $"{OutputState.SceneName} ---> {InputState.SceneName}";
        }

        public void OnBeforeSerialize() {
            OutputStateGUID = OutputState.GUID;
            InputStateGUID = InputState.GUID;
        }

        public void OnAfterDeserialize() { }
    }

    [Serializable]
    public class StateTransition : Transition {
        
        public StateTransition(WorldStateGraph graph, SceneStateData output, SceneStateData input) : base(graph, output, input) {
            conditions = new List<Condition>();
        }

        [SerializeReference] public List<Condition> conditions;
    }

}