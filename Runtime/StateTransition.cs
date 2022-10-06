using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph {
    
    [Serializable]
    public class StateTransitionData {
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

        public StateTransitionData(WorldStateGraph graph, SceneStateData output, SceneStateData input) {
            m_StateGraph = graph;
            m_GUID = Guid.NewGuid().ToString();
            
            OutputStateGUID = output.GUID;
            InputStateGUID = input.GUID;
            OutputState = output;
            InputState = input;
        }
    }

    public class StateTransition : MonoBehaviour {

        [SerializeReference] public List<StateCondition> conditions = new List<StateCondition>();

        [SerializeField] public StateTransitionData data;

        public SceneHandle OutputState {
            get {
                var controller = GetComponent<WorldGraph>();
                var outputState = controller.SceneHandles.Find(sh => sh.StateData.GUID == data.OutputStateGUID);
                return outputState;
            }
        }
        public SceneHandle InputState {
            get {
                var controller = GetComponent<WorldGraph>();
                var outputState = controller.SceneHandles.Find(sh => sh.StateData.GUID == data.InputStateGUID);
                return outputState;
            }
        }
        
        [SerializeField] private bool m_Active = true;
        public bool Active {
            get => m_Active;
            set => m_Active = value;
        }

        [SerializeField] private string m_Label;
        public string Label {
            get {
                m_Label = $"{data.OutputState.SceneName} ---> {data.InputState.SceneName}";
                return m_Label;
            }
        }

        public override string ToString() {
            return $"{data.OutputState.SceneName} ---> {data.InputState.SceneName}";
        }
    }

}