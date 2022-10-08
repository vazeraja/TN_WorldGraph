using System;
using System.Collections.Generic;
using ThunderNut.WorldGraph.Attributes;
using UnityEngine;

namespace ThunderNut.WorldGraph.Handles {

    [AddComponentMenu("")]
    [Serializable]
    public abstract class SceneHandle : MonoBehaviour {
        [InspectorGroup("Settings", true, 29)]
        [Tooltip("Whether or not this SceneHandle is active")]
        public bool Active = true;

        [Tooltip("The name of this SceneHandle to display in the inspector")]
        public string Label = "SceneHandle";

        [Tooltip("The color of this SceneHandle to display in the inspector")]
        public virtual Color HandleColor => Color.white;

        public virtual SceneType SceneType => SceneType.Default;

        [SerializeField, HideInInspector]
        public SceneStateData StateData;

        [SerializeField, HideInInspector]
        public List<StateTransition> StateTransitions = new List<StateTransition>();
        
        public WorldGraph WorldGraph => GetComponent<WorldGraph>();

        public SceneReference SceneReference;

        public virtual void Awake() {
            var allTransitionForHandle = WorldGraph.GetAllTransitionForHandle(this);
            StateTransitions.AddRange(allTransitionForHandle);
        }
    }

}