using System;
using System.Collections.Generic;
using System.Linq;
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

    #if UNITY_EDITOR
    [CustomEditor(typeof(WorldStateGraph), true)]
    public class WorldStateGraphEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneStateData"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("StateTransitions"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExposedParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ExposedParameterViewData"));

            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif

    [CreateAssetMenu(fileName = "New WorldStateGraph", menuName = "WorldGraph/StateGraph", order = 0)]
    public class WorldStateGraph : ScriptableObject {
        public List<SceneStateData> SceneStateData = new List<SceneStateData>();

        [SerializeReference] public List<Transition> StateTransitions = new List<Transition>();

        public List<ExposedParameter> ExposedParameters = new List<ExposedParameter>();
        public List<ExposedParameterViewData> ExposedParameterViewData = new List<ExposedParameterViewData>();

        public Transition CreateTransition(SceneStateData output, SceneStateData input) {
            StateTransition edge = new StateTransition(this, output, input);
            StateTransitions.Add(edge);
            return edge;
        }


        public void RemoveTransition(Transition edge) {
            StateTransitions.Remove(edge);
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