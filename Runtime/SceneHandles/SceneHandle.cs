using System;
using UnityEngine;

namespace ThunderNut.WorldGraph.Handles {

    [AddComponentMenu("")]
    [System.Serializable]
    public abstract class SceneHandle : MonoBehaviour {
        [Tooltip("Whether or not this scene handle is active")]
        public bool Active = true;

        [Tooltip("The name of this scene handle to display in the inspector")]
        public string Label = "SceneHandle";

        [Tooltip("The color of this scene handle to display in the inspector")]
        public virtual Color HandleColor => Color.white;

        public SceneReference scene;

    }

}