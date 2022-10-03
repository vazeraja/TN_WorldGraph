using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
    public class SceneStateData : IEquatable<SceneStateData> {
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

        public bool Equals(SceneStateData other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return m_GUID == other.m_GUID;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((SceneStateData) obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(GUID, SceneName, (int) SceneType, Position, Ports);
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(WorldStateGraph), true)]
    public class WorldStateGraphEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneStateData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TransitionData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExposedParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExposedParameterViewData"));

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif

    [CreateAssetMenu(fileName = "New WorldStateGraph", menuName = "WorldGraph/StateGraph", order = 0)]
    public class WorldStateGraph : ScriptableObject {
        public List<SceneStateData> SceneStateData = new List<SceneStateData>();
        public List<TransitionData> TransitionData = new List<TransitionData>();

        public List<ExposedParameter> ExposedParameters = new List<ExposedParameter>();
        public List<ExposedParameterViewData> ExposedParameterViewData = new List<ExposedParameterViewData>();

        public TransitionData CreateTransition(SceneStateData output, SceneStateData input) {
            TransitionData transitionData = new TransitionData(this, output, input);
            TransitionData.Add(transitionData);
            return transitionData;
        }


        public void RemoveTransition(TransitionData edge) {
            TransitionData.Remove(edge);
        }

        public ExposedParameter CreateParameter(string type) {
            switch (type) {
                case "String":
                    Undo.RecordObject(this, $"CreateParameter() :: String");

                    var stringParameter = CreateInstance<StringParameterField>();
                    ExposedParameters.Add(stringParameter);
                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(stringParameter, this);

                    Undo.RegisterCreatedObjectUndo(stringParameter, "StringParameterField SO Created");
                    AssetDatabase.SaveAssets();

                    return stringParameter;
                case "Float":
                    Undo.RecordObject(this, $"CreateParameter() :: Float");

                    var floatParameter = CreateInstance<FloatParameterField>();
                    ExposedParameters.Add(floatParameter);
                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(floatParameter, this);

                    Undo.RegisterCreatedObjectUndo(floatParameter, "FloatParameterField SO Created");
                    AssetDatabase.SaveAssets();

                    return floatParameter;
                case "Int":
                    Undo.RecordObject(this, $"CreateParameter() :: Int");

                    var intParameter = CreateInstance<IntParameterField>();
                    ExposedParameters.Add(intParameter);
                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(intParameter, this);

                    Undo.RegisterCreatedObjectUndo(intParameter, "IntParameterField SO Created");
                    AssetDatabase.SaveAssets();

                    return intParameter;
                case "Bool":
                    Undo.RecordObject(this, $"CreateParameter() :: Bool");

                    var boolParameter = CreateInstance<BoolParameterField>();
                    ExposedParameters.Add(boolParameter);
                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(boolParameter, this);

                    Undo.RegisterCreatedObjectUndo(boolParameter, "IntParameterField SO Created");
                    AssetDatabase.SaveAssets();

                    return boolParameter;
                default:
                    return null;
            }
        }

        public void RemoveParameter(ExposedParameter parameter) {
            Undo.RecordObject(this, $"RemoveParameter() :: {parameter.Name}");

            ExposedParameters.Remove(parameter);

            Undo.DestroyObjectImmediate(parameter);
            AssetDatabase.SaveAssets();
        }

        public void AddExposedParameterViewData(ExposedParameterViewData viewData) {
            ExposedParameterViewData.Add(viewData);
        }

        public void RemoveExposedParameterViewData(ExposedParameterViewData viewData) {
            ExposedParameterViewData.Remove(viewData);
        }
    }

}