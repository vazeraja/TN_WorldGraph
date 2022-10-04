using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph {

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