﻿using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    [CustomPropertyDrawer(typeof(StateTransition), true)]
    public class StateTransitionDrawer : PropertyDrawer {
        private readonly Dictionary<string, ReorderableList> _listsPerProp = new Dictionary<string, ReorderableList>();

        ReorderableList GetReorderableList(SerializedProperty prop) {
            SerializedProperty listProperty = prop.FindPropertyRelative("conditions");

            if (_listsPerProp.TryGetValue(listProperty.propertyPath, out var list)) return list;

            list = new ReorderableList(listProperty.serializedObject, listProperty, true, true, true, true);
            _listsPerProp[listProperty.propertyPath] = list;
            
            list.onAddCallback = list1 => {
                list1.serializedProperty.arraySize++;

                var elem = list1.serializedProperty.GetArrayElementAtIndex(list1.serializedProperty.arraySize - 1);
                elem.FindPropertyRelative("parameter").managedReferenceValue = null;
                elem.FindPropertyRelative("value").managedReferenceValue = null;
            }; 
            list.onRemoveCallback = reorderableList => {
                reorderableList.serializedProperty.arraySize--;
            };
            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, listProperty.displayName);
            list.elementHeightCallback = index => EditorGUIUtility.singleLineHeight;
            list.drawElementCallback = (rect, index, active, focused) => {
                var element = listProperty.GetArrayElementAtIndex(index);
                var parameterProp = element.FindPropertyRelative("parameter");
                var valueProp = element.FindPropertyRelative("value");

                float width = rect.width / 2;

                // SceneHandle sceneHandle = prop.FindPropertyRelative("OutputNode").objectReferenceValue as SceneHandle;
                // Debug.Assert(sceneHandle != null, nameof(sceneHandle) + " != null");

                WorldStateGraph stateGraph = prop.FindPropertyRelative("StateGraph").objectReferenceValue as WorldStateGraph;
                Debug.Assert(stateGraph != null, nameof(stateGraph) + " != null");
                var allParameters = stateGraph.ExposedParameters;
                
                if (allParameters.Any()) {
                    rect.width = width;
                
                    if (EditorGUI.DropdownButton(rect, parameterProp.managedReferenceValue != null
                        ? new GUIContent(((ExposedParameter) parameterProp.managedReferenceValue).Name)
                        : new GUIContent("Select a Parameter"), FocusType.Passive)) {
                        PopupWindow.Show(rect,
                            new ConditionOptionsPopupWindow(allParameters, parameterProp, valueProp) {Width = rect.width});
                    }
                
                    rect.x += width + 5;
                    rect.width = width / 2 - 5;
                
                    switch (parameterProp.managedReferenceValue) {
                        case StringParameterField:
                
                            if (valueProp.managedReferenceValue is StringCondition stringCondition) {
                                stringCondition.stringOptions =
                                    (StringParamOptions) EditorGUI.EnumPopup(rect, stringCondition.stringOptions);
                
                                rect.x += width / 2;
                                stringCondition.Value =
                                    EditorGUI.TextField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5), GUIContent.none,
                                        stringCondition.Value);
                            }
                
                            break;
                        case FloatParameterField:
                            if (valueProp.managedReferenceValue is FloatCondition floatCondition) {
                                floatCondition.floatOptions =
                                    (FloatParamOptions) EditorGUI.EnumPopup(rect, floatCondition.floatOptions);
                
                                rect.x += width / 2;
                                floatCondition.Value =
                                    EditorGUI.FloatField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5),
                                        GUIContent.none, floatCondition.Value);
                            }
                
                            break;
                        case IntParameterField:
                            if (valueProp.managedReferenceValue is IntCondition intCondition) {
                                intCondition.intOptions = (IntParamOptions) EditorGUI.EnumPopup(rect, intCondition.intOptions);
                
                                rect.x += width / 2;
                                intCondition.Value =
                                    EditorGUI.IntField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5),
                                        GUIContent.none, intCondition.Value);
                            }
                
                            break;
                        case BoolParameterField:
                            if (valueProp.managedReferenceValue is BoolCondition boolCondition) {
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
                    EditorGUI.HelpBox(rect, "This SceneHandle has no connected parameters", MessageType.Warning);
                    parameterProp.managedReferenceValue = null;
                    valueProp.managedReferenceValue = null;
                }
            };

            return list;
        }

        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label) {
            ReorderableList list = GetReorderableList(serializedProperty);

            list.DoList(rect);
        }

        public override float GetPropertyHeight(SerializedProperty serializedProperty, GUIContent label) {
            return GetReorderableList(serializedProperty).GetHeight();
        }
    }

}