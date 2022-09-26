using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph {
    
    [Serializable]
    public class ExposedParameter : ISerializationCallbackReceiver {
        public string GUID;
        public string Name;
        public string Reference;
        public bool Exposed;
        public string ParameterType;

        public void OnBeforeSerialize() {

        } 
 
        public void OnAfterDeserialize() {
        }
    }

}