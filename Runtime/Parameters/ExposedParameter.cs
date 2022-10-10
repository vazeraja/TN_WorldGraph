using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class ExposedParameterViewData {
        [SerializeField] private ExposedParameter parameter;
        public ExposedParameter Parameter {
            get => parameter;
            set => parameter = value;
        }

        [SerializeField] private SceneStateData connectedNode;
        public SceneStateData ConnectedNode {
            get => connectedNode;
            set => connectedNode = value;
        }

        [SerializeField] private string connectedPortGUID;
        public string ConnectedPortGUID {
            get => connectedPortGUID;
            set => connectedPortGUID = value;
        }

        [SerializeField] private Vector2 position;
        public Vector2 Position {
            get => position;
            set => position = value;
        }
    }

    public class ExposedParameter : ScriptableObject, IDisposable {
        public string GUID;
        public string Name;
        public string Reference;
        public bool Exposed;
        public string ParameterType;

        private void OnEnable() {
            name = Name;
        }

        public virtual void Dispose() { }
    }

}