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
        private SerializedProperty _StateTransitions;
        private Dictionary<StateTransition, UnityEditor.Editor> editors;

        private void OnEnable() {
            _StateTransitions = serializedObject.FindProperty("StateTransitions");

            editors = new Dictionary<StateTransition, UnityEditor.Editor>();
            for (var i = 0; i < _StateTransitions.arraySize; i++) {
                AddEditor(_StateTransitions.GetArrayElementAtIndex(i).objectReferenceValue as StateTransition);
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Active"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Label"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneReference"));

            for (var i = 0; i < _StateTransitions.arraySize; i++) {
                SerializedProperty property = _StateTransitions.GetArrayElementAtIndex(i);

                if (property.objectReferenceValue == null) continue; // Should not happen ...

                StateTransition stateTransition = property.objectReferenceValue as StateTransition;
                // stateTransition!.hideFlags = HideFlags.HideInInspector;

                Undo.RecordObject(stateTransition, "Modified StateTransition");

                EditorGUI.BeginDisabledGroup(!stateTransition!.Active);
                EditorGUILayout.Space();
                
                if (!editors.ContainsKey(stateTransition)) AddEditor(stateTransition);
                
                UnityEditor.Editor editor = editors[stateTransition];
                CreateCachedEditor(stateTransition, stateTransition.GetType(), ref editor);

                ((StateTransitionEditor) editor).OnInspectorGUI();

                EditorGUI.EndDisabledGroup();
            }

            if (_StateTransitions.arraySize > 0) {
                WGEditorGUI.DrawSplitter();
            }

            serializedObject.ApplyModifiedProperties();
        }


        public void AddEditor(StateTransition transition) {
            if (transition == null) return;
            if (editors.ContainsKey(transition)) return;
            
            UnityEditor.Editor editor = null;
            CreateCachedEditor(transition, null, ref editor);

            editors.Add(transition, editor);
        }
    }

}