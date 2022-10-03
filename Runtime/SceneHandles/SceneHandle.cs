using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderNut.WorldGraph.Handles {
    
    [AddComponentMenu("")]
    [Serializable]
    public abstract class SceneHandle : MonoBehaviour {
        [Tooltip("Whether or not this SceneReference transition is active")]
        public bool Active = true;

        [Tooltip("The name of this SceneReference transition to display in the inspector")]
        public string Label = "SceneHandle";

        [Tooltip("The color of this SceneReference transition to display in the inspector")]
        public virtual Color HandleColor => Color.white;

        public virtual SceneType SceneType => SceneType.Default;

        public SceneStateData StateData;

        public List<StateTransition> StateTransitions = new List<StateTransition>();
        
        public SceneReference SceneReference;
    }

}