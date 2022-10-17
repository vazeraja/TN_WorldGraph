using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.WorldGraph {
    
    [AddComponentMenu("")]
    [Serializable]
    public class StateTransition : MonoBehaviour {

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

        public string GetOutputStateSceneName() {
            return WorldGraph.GetSceneName(OutputState.Scene.ScenePath);
        }
        
        public string GetInputStateSceneName() {
            return WorldGraph.GetSceneName(InputState.Scene.ScenePath);
        }

        public override string ToString() {
            return $"{OutputState.Label} ---> {InputState.Label}";
        }
    }

}