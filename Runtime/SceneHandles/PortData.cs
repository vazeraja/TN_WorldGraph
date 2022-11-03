using System;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public enum PortType {
        Default,
        Parameter,
        Relay
    }

    [Serializable]
    public class PortData {
        public string OwnerNodeGUID;
        public string GUID;

        public string PortDirection;
        public string PortCapacity;
        public PortType PortType;
        public Color PortColor;
    }

}