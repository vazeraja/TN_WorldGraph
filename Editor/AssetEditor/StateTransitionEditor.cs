using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    [CustomEditor(typeof(StateTransition), true)]
    public class StateTransitionEditor : UnityEditor.Editor {
        private StateTransition stateTransition;

        private SerializedProperty conditionsProp;
        
        private ReorderableList conditionsList;

        private void OnEnable() {
            stateTransition = target as StateTransition;
 
            conditionsProp = serializedObject.FindProperty("conditions");
            
            conditionsList = new ReorderableList(serializedObject, conditionsProp) {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Conditions"),
                elementHeightCallback = index => EditorGUIUtility.singleLineHeight,
                drawElementCallback = DrawElementCallback
            };
        }
        
        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.Separator();
            conditionsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawElementCallback(Rect rect, int index, bool active, bool focused) {
            var conditionProp = conditionsProp.GetArrayElementAtIndex(index);

            conditionProp.managedReferenceValue ??= new StateCondition();
            StateCondition condition = conditionProp.managedReferenceValue as StateCondition;

            float width = rect.width / 2;

            WorldGraph controller = stateTransition.Controller;
            var allParameters = controller.ExposedParameters;

            if (allParameters.Any()) {
                rect.width = width;

                if (EditorGUI.DropdownButton(rect, condition!.parameter != null
                    ? new GUIContent(condition.parameter.Name)
                    : new GUIContent("Select a Parameter"), FocusType.Passive)) {
                    PopupWindow.Show(rect, new ConditionOptionsPopupWindow(controller, condition) {Width = rect.width});
                }

                rect.x += width + 5;
                rect.width = width / 2 - 5;
                switch (condition.parameter) {
                    case StringParameter:
                        if (condition.value is StringCondition stringCondition) {
                            stringCondition.stringOptions = (StringParamOptions) EditorGUI.EnumPopup(rect, stringCondition.stringOptions);
                            rect.x += width / 2;
                            stringCondition.Value = EditorGUI.TextField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5), GUIContent.none, stringCondition.Value);
                        }

                        break;
                    case FloatParameter:
                        if (condition.value is FloatCondition floatCondition) {
                            floatCondition.floatOptions = (FloatParamOptions) EditorGUI.EnumPopup(rect, floatCondition.floatOptions);
                            rect.x += width / 2;
                            floatCondition.Value = EditorGUI.FloatField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5), GUIContent.none, floatCondition.Value);
                        }

                        break;
                    case IntParameter:
                        if (condition.value is IntCondition intCondition) {
                            intCondition.intOptions = (IntParamOptions) EditorGUI.EnumPopup(rect, intCondition.intOptions);
                            rect.x += width / 2;
                            intCondition.Value = EditorGUI.IntField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5), GUIContent.none, intCondition.Value);
                        }

                        break;
                    case BoolParameter:
                        if (condition.value is BoolCondition boolCondition) {
                            boolCondition.boolOptions = (BoolParamOptions) EditorGUI.EnumPopup(rect, boolCondition.boolOptions);
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
        }
    }

}