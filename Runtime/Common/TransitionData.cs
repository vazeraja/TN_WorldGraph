using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class TransitionData : ISerializationCallbackReceiver {
        [SerializeField] private WorldStateGraph m_StateGraph;
        public WorldStateGraph StateGraph {
            get => m_StateGraph;
            set => m_StateGraph = value;
        }

        [SerializeField] private string m_GUID;
        public string GUID {
            get => m_GUID;
            set => m_GUID = value;
        }

        [SerializeField] private string m_OutputStateGUID;
        public string OutputStateGUID {
            get => m_OutputStateGUID;
            set => m_OutputStateGUID = value;
        }

        [SerializeField] private string m_InputStateGUID;
        public string InputStateGUID {
            get => m_InputStateGUID;
            set => m_InputStateGUID = value;
        }

        [SerializeField] private SceneStateData m_OutputState;
        public SceneStateData OutputState {
            get => m_OutputState;
            set => m_OutputState = value;
        }

        [SerializeField] private SceneStateData m_InputState;
        public SceneStateData InputState {
            get => m_InputState;
            set => m_InputState = value;
        }

        public TransitionData(WorldStateGraph graph, SceneStateData output, SceneStateData input) {
            StateGraph = graph;

            GUID = Guid.NewGuid().ToString();
            OutputStateGUID = output.GUID;
            OutputState = output;
            InputStateGUID = input.GUID;
            InputState = input;
        }

        public override string ToString() {
            return $"{OutputState.SceneName} ---> {InputState.SceneName}";
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }
    }

}