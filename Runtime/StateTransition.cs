using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class TransitionData : ISerializationCallbackReceiver, IEquatable<TransitionData> {
        public WorldStateGraph StateGraph;

        public string OutputStateGUID;
        public string InputStateGUID;
        public SceneStateData OutputState;
        public SceneStateData InputState;

        public TransitionData(WorldStateGraph graph, SceneStateData output, SceneStateData input) {
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

        public bool Equals(TransitionData other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(StateGraph, other.StateGraph) && OutputStateGUID == other.OutputStateGUID &&
                   InputStateGUID == other.InputStateGUID && Equals(OutputState, other.OutputState) &&
                   Equals(InputState, other.InputState);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((TransitionData) obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(StateGraph, OutputStateGUID, InputStateGUID, OutputState, InputState);
        }
    }

    public class StateTransition : MonoBehaviour {
        public TransitionData TransitionData;

        public bool Active = true;

        private string m_Label;
        public string Label {
            get {
                if (m_Label != null)
                    return m_Label;

                m_Label = TransitionData.ToString();
                return TransitionData.ToString();
            }
            set => m_Label = value;
        }

        [SerializeReference] public List<StateCondition> conditions;
    }

}