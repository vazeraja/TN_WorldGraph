using System;
using System.Collections.Generic;
using System.Reflection;
using ThunderNut.WorldGraph.Attributes;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace ThunderNut.WorldGraph.Editor {

    public class InspectorGroupData {
        public bool GroupIsOpen;
        public InspectorGroupAttribute GroupAttribute;
        public List<SerializedProperty> PropertiesList = new List<SerializedProperty>();
        public HashSet<string> GroupHashSet = new HashSet<string>();
        public Color GroupColor;

        public void ClearGroup() {
            GroupAttribute = null;
            GroupHashSet.Clear();
            PropertiesList.Clear();
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(TN_MonoBehaviour), true, isFallback = true)]
    public class TN_MonoBehaviourEditor : UnityEditor.Editor {
        public bool DrawerInitialized;
        private bool _requiresConstantRepaint;
        protected bool _shouldDrawBase = true;
        public bool drawScriptField = true;

        public List<SerializedProperty> PropertiesList = new List<SerializedProperty>();
        public Dictionary<string, InspectorGroupData> GroupData = new Dictionary<string, InspectorGroupData>();

        public override bool RequiresConstantRepaint() {
            return _requiresConstantRepaint;
        }

        protected virtual void OnEnable() {
            DrawerInitialized = false;
            if (!target || !serializedObject.targetObject) return;

            _requiresConstantRepaint =
                serializedObject.targetObject.GetType().GetCustomAttribute<RequiresConstantRepaintAttribute>() != null;
        }

        protected virtual void OnDisable() {
            if (target == null) return;

            foreach (KeyValuePair<string, InspectorGroupData> groupData in GroupData) {
                EditorPrefs.SetBool(
                    string.Format(
                        $"{groupData.Value.GroupAttribute.GroupName}{groupData.Value.PropertiesList[0].name}{target.GetInstanceID()}"
                    ),
                    groupData.Value.GroupIsOpen);
                groupData.Value.ClearGroup();
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            Initialization();
            DrawBase();
            DrawScriptBox();
            DrawContainer();
            DrawContents();

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void Initialization() {
            if (DrawerInitialized) {
                return;
            }

            List<FieldInfo> fieldInfoList;
            InspectorGroupAttribute previousGroupAttribute = default;
            int fieldInfoLength = WSGHelpers.GetFieldInfo(target, out fieldInfoList);

            for (int i = 0; i < fieldInfoLength; i++) {
                InspectorGroupAttribute group =
                    Attribute.GetCustomAttribute(fieldInfoList[i], typeof(InspectorGroupAttribute)) as InspectorGroupAttribute;
                InspectorGroupData groupData;
                if (group == null) {
                    if (previousGroupAttribute != null && previousGroupAttribute.GroupAllFieldsUntilNextGroupAttribute) {
                        _shouldDrawBase = false;
                        if (!GroupData.TryGetValue(previousGroupAttribute.GroupName, out groupData)) {
                            GroupData.Add(previousGroupAttribute.GroupName, new InspectorGroupData {
                                GroupAttribute = previousGroupAttribute,
                                GroupHashSet = new HashSet<string> {fieldInfoList[i].Name},
                                GroupColor = WSGColors.GetColorAt(previousGroupAttribute.GroupColorIndex)
                            });
                        }
                        else {
                            groupData.GroupColor = WSGColors.GetColorAt(previousGroupAttribute.GroupColorIndex);
                            groupData.GroupHashSet.Add(fieldInfoList[i].Name);
                        }
                    }

                    continue;
                }

                previousGroupAttribute = group;

                if (!GroupData.TryGetValue(group.GroupName, out groupData)) {
                    bool groupIsOpen =
                        EditorPrefs.GetBool(string.Format($"{group.GroupName}{fieldInfoList[i].Name}{target.GetInstanceID()}"), false);
                    GroupData.Add(group.GroupName, new InspectorGroupData {
                        GroupAttribute = group,
                        GroupColor = WSGColors.GetColorAt(previousGroupAttribute.GroupColorIndex),
                        GroupHashSet = new HashSet<string> {fieldInfoList[i].Name}, GroupIsOpen = groupIsOpen
                    });
                }
                else {
                    groupData.GroupHashSet.Add(fieldInfoList[i].Name);
                    groupData.GroupColor = WSGColors.GetColorAt(previousGroupAttribute.GroupColorIndex);
                }
            }

            SerializedProperty iterator = serializedObject.GetIterator();

            if (iterator.NextVisible(true)) {
                do {
                    FillPropertiesList(iterator);
                } while (iterator.NextVisible(false));
            }

            DrawerInitialized = true;
        }

        protected virtual void DrawBase() {
            if (_shouldDrawBase) {
                DrawDefaultInspector();
                return;
            }
        }

        protected virtual void DrawScriptBox() {
            if (PropertiesList.Count == 0) return;
            if (!drawScriptField) return;

            using (new EditorGUI.DisabledScope("m_Script" == PropertiesList[0].propertyPath)) {
                EditorGUILayout.PropertyField(PropertiesList[0], true);
            }
        }

        protected virtual void DrawContainer() {
            if (PropertiesList.Count == 0) return;

            var ContainerStyle = new GUIStyle(GUI.skin.box);
            ContainerStyle.padding = new RectOffset(20, 0, 10, 10);

            foreach (KeyValuePair<string, InspectorGroupData> pair in GroupData) {
                this.DrawVerticalLayout(() => DrawGroup(pair.Value), ContainerStyle);
                EditorGUI.indentLevel = 0;
            }
        }


        protected virtual void DrawContents() {
            if (PropertiesList.Count == 0) {
                return;
            }

            EditorGUILayout.Space();
            for (int i = 1; i < PropertiesList.Count; i++) {
                EditorGUILayout.PropertyField(PropertiesList[i], true);
            }
        }

        protected virtual void DrawGroup(InspectorGroupData groupData) {
            Rect verticalGroup = EditorGUILayout.BeginVertical();

            var leftBorderRect = new Rect(verticalGroup.xMin + 5, verticalGroup.yMin - 10, 3f, verticalGroup.height + 20);
            leftBorderRect.xMin = 15f;
            leftBorderRect.xMax = 18f;
            EditorGUI.DrawRect(leftBorderRect, groupData.GroupColor);

            var GroupStyle = new GUIStyle(EditorStyles.foldout);
            GroupStyle.active.background = Resources.Load<Texture2D>("IN foldout focus-6510");
            GroupStyle.focused.background = Resources.Load<Texture2D>("IN foldout focus-6510");
            GroupStyle.hover.background = Resources.Load<Texture2D>("IN foldout focus-6510");
            GroupStyle.onActive.background = Resources.Load<Texture2D>("IN foldout focus on-5718");
            GroupStyle.onFocused.background = Resources.Load<Texture2D>("IN foldout focus on-5718");
            GroupStyle.onHover.background = Resources.Load<Texture2D>("IN foldout focus on-5718");
            GroupStyle.fontStyle = FontStyle.Bold;
            GroupStyle.overflow = new RectOffset(100, 0, 0, 0);
            GroupStyle.padding = new RectOffset(20, 0, 0, 0);

            groupData.GroupIsOpen =
                EditorGUILayout.Foldout(groupData.GroupIsOpen, groupData.GroupAttribute.GroupName, true, GroupStyle);

            if (groupData.GroupIsOpen) {
                EditorGUI.indentLevel = 0;

                for (int i = 0; i < groupData.PropertiesList.Count; i++) {
                    var BoxChildStyle = new GUIStyle(GUI.skin.box) {
                        padding = new RectOffset(0, 0, 0, 0),
                        margin = new RectOffset(0, 0, 0, 0),
                        normal = {background = new Texture2D(0, 0)}
                    };
                    this.DrawVerticalLayout(() => DrawChild(i), BoxChildStyle);
                }
            }

            EditorGUILayout.EndVertical();

            void DrawChild(int i) {
                EditorGUILayout.PropertyField(groupData.PropertiesList[i],
                    new GUIContent(ObjectNames.NicifyVariableName(groupData.PropertiesList[i].name),
                        tooltip: groupData.PropertiesList[i].tooltip), true);
            }
        }

        public void FillPropertiesList(SerializedProperty serializedProperty) {
            bool shouldClose = false;

            foreach (KeyValuePair<string, InspectorGroupData> pair in GroupData) {
                if (pair.Value.GroupHashSet.Contains(serializedProperty.name)) {
                    SerializedProperty property = serializedProperty.Copy();
                    shouldClose = true;
                    pair.Value.PropertiesList.Add(property);
                    break;
                }
            }

            if (!shouldClose) {
                SerializedProperty property = serializedProperty.Copy();
                PropertiesList.Add(property);
            }
        }
    }

}