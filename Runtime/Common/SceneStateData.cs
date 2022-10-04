using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public enum SceneType {
        Default,
        Cutscene,
        Battle
    }

    [Serializable]
    public class SceneStateData : ISerializationCallbackReceiver, IEquatable<SceneStateData> {
        [SerializeField] private string m_GUID;
        public string GUID => m_GUID;

        [SerializeField] private string m_SceneName;
        public string SceneName {
            get => m_SceneName;
            set => m_SceneName = value;
        }

        [SerializeField] private SceneType m_SceneType;
        public SceneType SceneType {
            get => m_SceneType;
            set => m_SceneType = value;
        }

        [SerializeField] private Vector2 m_Position;
        public Vector2 Position {
            get => m_Position;
            set => m_Position = value;
        }

        [SerializeField] private List<PortData> m_Ports;
        public List<PortData> Ports {
            get => m_Ports;
            set => m_Ports = value;
        }

        public SceneStateData(string name, SceneType type, Vector2 pos) {
            m_GUID = Guid.NewGuid().ToString();
            m_SceneName = name;
            m_SceneType = type;
            m_Position = pos;
            m_Ports = new List<PortData>();
        }

        public PortData CreatePort(string ownerGUID, bool isOutput, bool isMulti, bool isParameter, Color portColor) {
            var portData = new PortData {
                OwnerNodeGUID = ownerGUID,
                GUID = Guid.NewGuid().ToString(),

                PortDirection = isOutput ? "Output" : "Input",
                PortCapacity = isMulti ? "Multi" : "Single",
                PortType = isParameter ? PortType.Parameter : PortType.Default,
                PortColor = portColor,
            };
            m_Ports.Add(portData);
            return portData;
        }

        public void RemovePort(PortData portData) {
            m_Ports.Remove(portData);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }

        public bool Equals(SceneStateData other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GUID == other.GUID;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SceneStateData) obj);
        }

        public override int GetHashCode() {
            return (GUID != null ? GUID.GetHashCode() : 0);
        }
    }

}