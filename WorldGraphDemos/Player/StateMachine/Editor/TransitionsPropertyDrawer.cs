using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {
    
    // [CustomPropertyDrawer(typeof(Transition))]
    public class TransitionsPropertyDrawer : PropertyDrawer {
        private static class Styles {
            public static readonly GUIContent RemoveIcon = EditorGUIUtility.IconContent("d_tab_next");
            public static readonly GUIStyle IconButton = new GUIStyle("IconButton");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var trueState = property.FindPropertyRelative("trueState");
            return EditorGUI.GetPropertyHeight(trueState);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var decision = property.FindPropertyRelative("decision");
            
            var trueState = property.FindPropertyRelative("trueState");
            
            var falseState = property.FindPropertyRelative("falseState");

            float width = (position.width - 20) / 2;

            position.width = width;
            EditorGUI.PropertyField(position, decision, GUIContent.none);
            position.x += width + 2;
            position.width = 10;
            EditorGUI.LabelField(position, Styles.RemoveIcon, Styles.IconButton);
            position.x += 18;
            position.width = (width - 10) / 2;
            EditorGUI.PropertyField(position, trueState, GUIContent.none);
            position.x += position.width + 10;
            position.width = (width - 10) / 2;
            EditorGUI.PropertyField(position, falseState, GUIContent.none);


            EditorGUI.EndProperty();
        }
    }
}