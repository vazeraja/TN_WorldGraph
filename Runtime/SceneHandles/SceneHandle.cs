﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderNut.WorldGraph.Handles {

    [AddComponentMenu("")]
    [Serializable]
    public abstract class SceneHandle : MonoBehaviour {
        [Tooltip("Whether or not this SceneHandle is active")]
        public bool Active = true;

        [Tooltip("The name of this SceneHandle to display in the inspector")]
        public string Label = "SceneHandle";

        [Tooltip("The color of this SceneHandle to display in the inspector")]
        public virtual Color HandleColor => Color.white;

        public virtual SceneType SceneType => SceneType.Default;

        [SerializeField, HideInInspector]
        public SceneStateData StateData;

        public WorldGraph WorldGraph => GetComponent<WorldGraph>();

        public SceneReference SceneReference;

        [SerializeField] public List<StateTransition> StateTransitions = new List<StateTransition>();

        public virtual void Awake() {
            var allTransitionForHandle = WorldGraph.GetAllTransitionForHandle(this);
            StateTransitions.AddRange(allTransitionForHandle);
        }
    }
 
}