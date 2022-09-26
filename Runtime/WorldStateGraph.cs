using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    public enum PortType {
        Default,
        Parameter,
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

    public enum SceneType {
        Default,
        Cutscene,
        Battle
    }

    [Serializable]
    public class SceneStateData {
        public string GUID;
        public string SceneName;
        public SceneType SceneType;
        public Vector2 Position;
        public List<PortData> Ports;

        public SceneStateData(string name, SceneType type, Vector2 pos) {
            GUID = Guid.NewGuid().ToString();
            SceneName = name;
            SceneType = type;
            Position = pos;
            Ports = new List<PortData>();
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
            Ports.Add(portData);
            return portData;
        }

        public void RemovePort(PortData portData) {
            Ports.Remove(portData);
        }
    }


    [CreateAssetMenu(fileName = "New WorldStateGraph", menuName = "WorldGraph/StateGraph", order = 0)]
    public class WorldStateGraph : ScriptableObject {
        public List<SceneStateData> SceneStateData = new List<SceneStateData>();
        public List<StateTransition> StateTransitions = new List<StateTransition>();
        [SerializeReference] public List<ExposedParameter> ExposedParameters = new List<ExposedParameter>();

        [SerializeField] private bool m_IsDirty;
        public bool isDirty {
            get => m_IsDirty;
            set => m_IsDirty = value;
        }

        public void RegisterCompleteObjectUndo(string actionName) {
            Undo.RegisterCompleteObjectUndo(this, actionName);
            m_IsDirty = true;
        }

        public StateTransition CreateTransition(SceneStateData output, SceneStateData input) {
            StateTransition edge = new StateTransition {
                OutputStateGUID = output.GUID,
                OutputState = output,
                InputStateGUID = input.GUID,
                InputState = input
            };

            StateTransitions.Add(edge);
            return edge;
        }


        public void RemoveTransition(StateTransition edge) {
            StateTransitions.Remove(edge);
        }

        public ExposedParameter CreateParameter(string type) {
            switch (type) {
                case "String":
                    var stringParameter = new StringParameterField();

                    ExposedParameters.Add(stringParameter);

                    return stringParameter;
                case "Float":
                    var floatParameter = new FloatParameterField();

                    ExposedParameters.Add(floatParameter);

                    return floatParameter;
                case "Int":
                    var intParameter = new IntParameterField();

                    ExposedParameters.Add(intParameter);

                    return intParameter;
                case "Bool":
                    var boolParameter = new BoolParameterField();
                    ExposedParameters.Add(boolParameter);

                    return boolParameter;
                default:
                    return null;
            }
        }

        public void RemoveParameter(ExposedParameter parameter) {
            ExposedParameters.Remove(parameter);
        }
    }

}