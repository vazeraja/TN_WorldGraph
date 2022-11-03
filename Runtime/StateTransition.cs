using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.WorldGraph {
    
    [Serializable]
    public class RelayNodeData {
        public string GUID;
        public string OldOutputGUID;
        public string OldInputGUID;
        public Vector2 Position;

        public PortData outputPortData;
        public PortData inputPortData;
        
        public PortData CreatePort(string ownerGUID, bool isOutput, bool isMulti, Color portColor) {
            var portData = new PortData {
                OwnerNodeGUID = ownerGUID,
                GUID = Guid.NewGuid().ToString(),

                PortDirection = isOutput ? "Output" : "Input",
                PortCapacity = isMulti ? "Multi" : "Single",
                PortType = PortType.Relay,
                PortColor = portColor,
            };
            
            switch (isOutput) {
                case true:
                    outputPortData = portData;
                    break;
                case false:
                    inputPortData = portData;
                    break;
            }

            return portData;
        }
    }
    
    [AddComponentMenu("")]
    [Serializable]
    public class StateTransition : MonoBehaviour {

        public RelayNodeData Relay;
        [SerializeReference] public List<StateCondition> conditions = new List<StateCondition>();
        
        public WorldGraph Controller => GetComponent<WorldGraph>();

        public string GUID;
        
        public string OutputStateGUID;
        public string InputStateGUID;

        private SceneHandle m_OutputState;
        public SceneHandle OutputState {
            get {
                if (m_OutputState != null) return m_OutputState;
                
                m_OutputState = Controller.SceneHandles.Find(sh => sh.GUID == OutputStateGUID);

                return m_OutputState;
            }
            set => m_OutputState = value;
        }

        private SceneHandle m_InputState;
        public SceneHandle InputState {
            get {
                if (m_InputState != null) return m_InputState;
                
                m_InputState = Controller.SceneHandles.Find(sh => sh.GUID == InputStateGUID);
                
                return m_InputState;
            }
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
                m_Label = $"{OutputState.Label} ---> {InputState.Label}";
                return m_Label;
            }
        }
        
        private void OnEnable() {
            if (InputState == null || OutputState == null) {
                Active = false;
            }
        }
        
        public override string ToString() {
            return $"{OutputState.Label} ---> {InputState.Label}";
        }
    }

}