using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public class StateTransition : ScriptableObject {
        public static StateTransition CreateInstance(WorldStateGraph graph, SceneStateData output, SceneStateData input) {
            var obj = ScriptableObject.CreateInstance<StateTransition>();

            obj.StateGraph = graph;
            obj.GUID = Guid.NewGuid().ToString();
            obj.OutputStateGUID = output.GUID;
            obj.OutputState = output;
            obj.InputStateGUID = input.GUID;
            obj.InputState = input;
            
            obj.name = $"{obj.OutputState.SceneName} ---> {obj.InputState.SceneName}";

            return obj;
        }

        [SerializeReference] public List<StateCondition> conditions = new List<StateCondition>();

        [SerializeField] private WorldStateGraph m_StateGraph;
        public WorldStateGraph StateGraph {
            get => m_StateGraph;
            private set => m_StateGraph = value;
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

        [SerializeField] private bool m_Active = true;
        public bool Active {
            get => m_Active;
            set => m_Active = value;
        }

        [SerializeField] private string m_Label;
        public string Label {
            get {
                if (m_Label != null)
                    return m_Label;

                m_Label = $"{OutputState.SceneName} ---> {InputState.SceneName}";
                return m_Label;
            }
            set => m_Label = value;
        }

        public override string ToString() {
            return $"{OutputState.SceneName} ---> {InputState.SceneName}";
        }
    }

}