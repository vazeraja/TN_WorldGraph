using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph {
    
    [AddComponentMenu("")]
    [Serializable]
    public class ExposedParameter : MonoBehaviour, IDisposable {
        public string GUID;
        public string Name;
        public string Reference;
        public bool Exposed;
        public string ParameterType;

        public Vector2 Position;

        private void OnEnable() {
            gameObject.name = Name;
        }

        public virtual void Dispose() { }
    }

}