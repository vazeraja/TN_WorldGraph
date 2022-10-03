using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : UnityEditor.Editor {
        private SceneHandle targetSceneHandle;

        private SerializedProperty _StateTransitions;
        private Dictionary<StateTransition, SerializedObject> m_SerializedObjects;
        private Dictionary<StateTransition, ReorderableList> listsPerTransition;

        private void OnEnable() {

            targetSceneHandle = target as SceneHandle;

            _StateTransitions = serializedObject.FindProperty("StateTransitions");

            m_SerializedObjects = new Dictionary<StateTransition, SerializedObject>();
            listsPerTransition = new Dictionary<StateTransition, ReorderableList>();
            for (var i = 0; i < _StateTransitions.arraySize; i++) {
                AddEditor(_StateTransitions.GetArrayElementAtIndex(i).objectReferenceValue as StateTransition);
            }
        }

        private void CheckDeletedTransitions() {
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            foreach ((StateTransition key, SerializedObject value) in m_SerializedObjects) {
                value.Update();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Active"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Label"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneReference"));

            for (var i = 0; i < _StateTransitions.arraySize; i++) {
                SerializedProperty property = _StateTransitions.GetArrayElementAtIndex(i);

                if (property.objectReferenceValue == null) continue; // Should not happen ...

                StateTransition stateTransition = property.objectReferenceValue as StateTransition;
                stateTransition!.hideFlags = HideFlags.HideInInspector;

                Undo.RecordObject(stateTransition, "Modified StateTransition");

                EditorGUI.BeginDisabledGroup(!stateTransition.Active);
                EditorGUILayout.Space();

                if (!listsPerTransition.ContainsKey(stateTransition)) AddEditor(stateTransition);
                var list = listsPerTransition[stateTransition];
                var obj = m_SerializedObjects[stateTransition];

                list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, stateTransition.Label);
                list.elementHeightCallback = index => EditorGUIUtility.singleLineHeight;
                list.drawElementCallback = (rect, index, active, focused) => {
                    var conditionProp = obj.FindProperty("conditions").GetArrayElementAtIndex(index);

                    conditionProp.managedReferenceValue ??= new StateCondition();
                    StateCondition condition = conditionProp.managedReferenceValue as StateCondition;

                    float width = rect.width / 2;

                    WorldStateGraph stateGraph = stateTransition.TransitionData.StateGraph;
                    var allParameters = stateGraph.ExposedParameters;

                    if (allParameters.Any()) {
                        rect.width = width;

                        if (EditorGUI.DropdownButton(rect, condition!.parameter != null
                            ? new GUIContent(condition.parameter.Name)
                            : new GUIContent("Select a Parameter"), FocusType.Passive)) {
                            PopupWindow.Show(rect, new ConditionOptionsPopupWindow(stateGraph, condition) {
                                Width = rect.width
                            });
                        }

                        rect.x += width + 5;
                        rect.width = width / 2 - 5;
                        switch (condition.parameter) {
                            case StringParameterField:
                                if (condition.value is StringCondition stringCondition) {
                                    stringCondition.stringOptions =
                                        (StringParamOptions) EditorGUI.EnumPopup(rect, stringCondition.stringOptions);
                                    rect.x += width / 2;
                                    stringCondition.Value =
                                        EditorGUI.TextField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5), GUIContent.none,
                                            stringCondition.Value);
                                }

                                break;
                            case FloatParameterField:
                                if (condition.value is FloatCondition floatCondition) {
                                    floatCondition.floatOptions =
                                        (FloatParamOptions) EditorGUI.EnumPopup(rect, floatCondition.floatOptions);
                                    rect.x += width / 2;
                                    floatCondition.Value =
                                        EditorGUI.FloatField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5),
                                            GUIContent.none, floatCondition.Value);
                                }

                                break;
                            case IntParameterField:
                                if (condition.value is IntCondition intCondition) {
                                    intCondition.intOptions = (IntParamOptions) EditorGUI.EnumPopup(rect, intCondition.intOptions);
                                    rect.x += width / 2;
                                    intCondition.Value =
                                        EditorGUI.IntField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5),
                                            GUIContent.none, intCondition.Value);
                                }

                                break;
                            case BoolParameterField:
                                if (condition.value is BoolCondition boolCondition) {
                                    boolCondition.boolOptions =
                                        (BoolParamOptions) EditorGUI.EnumPopup(rect, boolCondition.boolOptions);
                                    boolCondition.Value = boolCondition.boolOptions switch {
                                        BoolParamOptions.True => true,
                                        BoolParamOptions.False => false,
                                        _ => boolCondition.Value
                                    };
                                }

                                break;
                        }
                    }
                    else {
                        EditorGUI.HelpBox(rect, "No Parameters Available", MessageType.Warning);
                    }
                };


                list.DoLayoutList();

                EditorGUI.EndDisabledGroup();
            }

            if (_StateTransitions.arraySize > 0) {
                WGEditorGUI.DrawSplitter();
            }

            serializedObject.ApplyModifiedProperties();
            foreach ((StateTransition key, SerializedObject value) in m_SerializedObjects) {
                value.ApplyModifiedProperties();
            }
        }


        public void AddEditor(StateTransition transition) {
            if (transition == null) return;

            var serializedTransition = new SerializedObject(transition);

            if (!m_SerializedObjects.ContainsKey(transition)) {
                m_SerializedObjects.Add(transition, serializedTransition);
            }

            if (!listsPerTransition.ContainsKey(transition)) {
                var list = new ReorderableList(serializedTransition, serializedTransition.FindProperty("conditions"));
                listsPerTransition.Add(transition, list);
            }
        }
    }

}