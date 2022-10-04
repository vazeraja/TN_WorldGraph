using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public class StateTransition : MonoBehaviour {
        [SerializeField] private TransitionData transitionData;
        public TransitionData TransitionData {
            get => transitionData;
            set => transitionData = value;
        }

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

        [SerializeReference] public List<StateCondition> conditions = new List<StateCondition>();
    }

}