using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    [CustomEditor(typeof(WorldGraph))]
    public class WorldGraphEditor : UnityEditor.Editor {
        private SerializedProperty sceneHandles;
        private SerializedProperty stateTransitions;

        private Dictionary<SceneHandle, UnityEditor.Editor> _editors;

        private readonly List<string> typeDisplays = new List<string>(); 
        private bool _settingsMenuDropdown;
        private GUIStyle _playingStyle;
        private static bool debugView = false;
        private int _draggedStartID = -1;
        private int _draggedEndID = -1;
        private Color _originalBackgroundColor;
        private readonly Color _playButtonColor = new Color32(193, 255, 2, 255);

        private void OnEnable() {
            sceneHandles = serializedObject.FindProperty("SceneHandles");
            stateTransitions = serializedObject.FindProperty("StateTransitions");

            // store GUI bg color
            _originalBackgroundColor = GUI.backgroundColor;

            _editors = new Dictionary<SceneHandle, UnityEditor.Editor>();
            for (var i = 0; i < sceneHandles.arraySize; i++) {
                AddEditor(sceneHandles.GetArrayElementAtIndex(i).objectReferenceValue as SceneHandle);
            }

            typeDisplays.Add("Add new SceneHandle...");
            typeDisplays.AddRange(WSGAttributeCache.knownNodeTypes.Select(type => type.Name));

            _playingStyle = new GUIStyle {normal = {textColor = Color.yellow}};
        }

        public override void OnInspectorGUI() {
            var e = Event.current;

            serializedObject.Update();
            Undo.RecordObject(target, "Modified Feedback Manager");

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox($"Select SceneHandles using the **Add new SceneHandle** button", MessageType.None);

            Rect helpBoxRect = GUILayoutUtility.GetLastRect();

            // -------------------------------------------- Settings dropdown --------------------------------------------
            EditorGUILayout.PropertyField(serializedObject.FindProperty("stateGraph"));

            _settingsMenuDropdown = EditorGUILayout.Foldout(_settingsMenuDropdown, "Settings", true, EditorStyles.foldout);
            if (_settingsMenuDropdown) {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Initialization", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("activeSceneHandle"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingB"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingC"));

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingD"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingE"));
            }

            // -------------------------------------------- Duration --------------------------------------------

            float durationRectWidth = 70f;
            Rect durationRect = new Rect(helpBoxRect.xMax - durationRectWidth, helpBoxRect.yMax + 6, durationRectWidth, 17f);
            durationRect.xMin = helpBoxRect.xMax - durationRectWidth;
            durationRect.xMax = helpBoxRect.xMax;

            float playingRectWidth = 70f;
            Rect playingRect = new Rect(helpBoxRect.xMax - playingRectWidth - durationRectWidth, helpBoxRect.yMax + 6,
                playingRectWidth, 17f);
            playingRect.xMin = helpBoxRect.xMax - durationRectWidth - playingRectWidth;
            playingRect.xMax = helpBoxRect.xMax;

            // -------------------------------------------- Direction --------------------------------------------

            float directionRectWidth = 16f;
            Rect directionRect = new Rect(helpBoxRect.xMax - directionRectWidth, helpBoxRect.yMax + 5, directionRectWidth, 17f);
            directionRect.xMin = helpBoxRect.xMax - directionRectWidth;
            directionRect.xMax = helpBoxRect.xMax;

            if (Application.isPlaying) {
                GUI.Label(playingRect, "[PLAYING] ", _playingStyle);
            }

            // -------------------------------------------- Draw list --------------------------------------------

            for (int i = 0; i < stateTransitions.arraySize; i++) {
                SerializedProperty property = stateTransitions.GetArrayElementAtIndex(i);

                if (property.objectReferenceValue == null) continue; // Should not happen ...

                StateTransition stateTransition = property.objectReferenceValue as StateTransition;
                stateTransition!.hideFlags = debugView ? HideFlags.None : HideFlags.HideInInspector;
            }

            WGEditorGUI.DrawSection("Scene Handles");

            for (int i = 0; i < sceneHandles.arraySize; i++) {
                WGEditorGUI.DrawSplitter();

                SerializedProperty property = sceneHandles.GetArrayElementAtIndex(i);

                if (property.objectReferenceValue == null) continue; // Should not happen ...

                SceneHandle handle = property.objectReferenceValue as SceneHandle;
                handle!.hideFlags = debugView ? HideFlags.None : HideFlags.HideInInspector;

                Undo.RecordObject(handle, "Modified SceneHandle");

                int id = i;
                bool isExpanded = property.isExpanded;
                string label = handle.Label;

                Rect headerRect = WGEditorGUI.DrawSimpleHeader(ref isExpanded, ref handle.Active, label, handle.HandleColor, menu => {
                    if (Application.isPlaying)
                        menu.AddItem(new GUIContent("Play"), false, () => Debug.Log("Play"));
                    else
                        menu.AddDisabledItem(new GUIContent("Play"));
                    menu.AddSeparator(null);
                    menu.AddItem(new GUIContent("Remove"), false, () => RemoveSceneHandle(id));
                });

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                switch (e.type) {
                    case EventType.MouseDown:
                        if (headerRect.Contains(e.mousePosition)) {
                            _draggedStartID = i;
                            e.Use();
                        }

                        break;
                }

                // Draw blue rect if handle is being dragged
                if (_draggedStartID == i && headerRect != Rect.zero) {
                    Color color = new Color(0, 1, 1, 0.2f);
                    EditorGUI.DrawRect(headerRect, color);
                }

                // If hovering at the top of the handle while dragging one, check where the feedback should be dropped : top or bottom
                if (headerRect.Contains(e.mousePosition)) {
                    if (_draggedStartID >= 0) {
                        _draggedEndID = i;

                        Rect headerSplit = headerRect;
                        headerSplit.height *= 0.5f;
                        headerSplit.y += headerSplit.height;
                        if (headerSplit.Contains(e.mousePosition))
                            _draggedEndID = i + 1;
                    }
                }

                property.isExpanded = isExpanded;
                // ReSharper disable once InvertIf
                if (isExpanded) {
                    EditorGUI.BeginDisabledGroup(!handle.Active);
                    EditorGUILayout.Space();

                    if (!_editors.ContainsKey(handle)) AddEditor(handle);

                    UnityEditor.Editor editor = _editors[handle];
                    CreateCachedEditor(handle, handle.GetType(), ref editor);

                    ((SceneHandleEditor) editor).drawScriptField = false;
                    ((SceneHandleEditor) editor).OnInspectorGUI();

                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Play", EditorStyles.miniButtonMid)) {
                            Debug.Log("Play");
                        }

                        if (GUILayout.Button("Stop", EditorStyles.miniButtonMid)) {
                            Debug.Log("Stop");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
            }

            if (sceneHandles.arraySize > 0) {
                WGEditorGUI.DrawSplitter();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                int newItem = EditorGUILayout.Popup(0, typeDisplays.ToArray());
                if (newItem >= 1) {
                    Debug.Log(typeDisplays[newItem]);

                    var type = WSGAttributeCache.knownNodeTypes.ToList().Find(x => x.Name == typeDisplays[newItem]);
                    AddSceneHandle(type);
                }
            }
            EditorGUILayout.EndHorizontal();

            // ---------- Reorder -------------
            if (_draggedStartID >= 0 && _draggedEndID >= 0) {
                if (_draggedEndID != _draggedStartID) {
                    if (_draggedEndID > _draggedStartID)
                        _draggedEndID--;
                    sceneHandles.MoveArrayElement(_draggedStartID, _draggedEndID);
                    _draggedStartID = _draggedEndID;
                }
            }

            if (_draggedStartID >= 0 || _draggedEndID >= 0) {
                switch (e.type) {
                    case EventType.MouseUp:
                        _draggedStartID = -1;
                        _draggedEndID = -1;
                        e.Use();
                        break;
                }
            }

            // -------------------------------------------- Clean up --------------------------------------------
            var wasRemoved = false;
            for (int i = sceneHandles.arraySize - 1; i >= 0; i--) {
                // ReSharper disable once InvertIf
                if (sceneHandles.GetArrayElementAtIndex(i).objectReferenceValue == null) {
                    wasRemoved = true;
                    sceneHandles.DeleteArrayElementAtIndex(i);
                }
            }

            if (wasRemoved) {
                GameObject gameObject = (target as WorldGraph)?.gameObject;
                foreach (var c in gameObject!.GetComponents<Component>()) {
                    if (c != null) {
                        c.hideFlags = HideFlags.None;
                    }
                }
            }

            // -------------------------------------------- Apply Changes --------------------------------------------
            serializedObject.ApplyModifiedProperties();

            // -------------------------------------------- Debug Area --------------------------------------------
            WGEditorGUI.DrawSection("All Scenes Debug");
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            EditorGUILayout.BeginHorizontal();
            {
                //  -------------------------- Play button --------------------------
                _originalBackgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = _playButtonColor;
                if (GUILayout.Button("Play", EditorStyles.miniButtonMid)) {
                    Debug.Log("Play");
                }

                GUI.backgroundColor = _originalBackgroundColor;

                // -------------------------- Stop button --------------------------
                if (GUILayout.Button("Stop", EditorStyles.miniButtonMid)) {
                    Debug.Log("Stop");
                }

                EditorGUI.EndDisabledGroup();

                // -------------------------- Debug button --------------------------
                EditorGUI.BeginChangeCheck();
                {
                    debugView = GUILayout.Toggle(debugView, "Debug View", EditorStyles.miniButtonRight);

                    if (EditorGUI.EndChangeCheck()) {
                        foreach (var f in (target as WorldGraph)?.SceneHandles)
                            f.hideFlags = debugView ? HideFlags.HideInInspector : HideFlags.None;
                        foreach (var f in (target as WorldGraph)?.StateTransitions)
                            f.hideFlags = debugView ? HideFlags.HideInInspector : HideFlags.None;
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }

                if (GUILayout.Button("Initialize State Graph", EditorStyles.miniButtonRight)) {
                    InitializeFromStateGraph();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void InitializeFromStateGraph() {
            var obj = (target as WorldGraph)?.gameObject;

            // -------------------- Remove All Scene Handles --------------------
            for (int i = 0; i < sceneHandles.arraySize; i++) {
                SerializedProperty property = sceneHandles.GetArrayElementAtIndex(i);
                SceneHandle sceneHandle = property.objectReferenceValue as SceneHandle;

                if (sceneHandle == null) break;

                var match = (target as WorldGraph)?.stateGraph.SceneStateData.Find(data => data.GUID == sceneHandle.StateData.GUID);
                if (match != null) continue;

                Debug.Log($"destroying {sceneHandle.Label}");
                (target as WorldGraph)?.SceneHandles.Remove(sceneHandle);
                Undo.DestroyObjectImmediate(sceneHandle);
            }

            // -------------------- Remove all StateTransition's that no longer have data fields in the StateGraph --------------------
            for (int i = 0; i < stateTransitions.arraySize; i++) {
                SerializedProperty property = stateTransitions.GetArrayElementAtIndex(i);
                StateTransition stateTransition = property.objectReferenceValue as StateTransition;

                if (stateTransition == null) break;

                var match = (target as WorldGraph)?.stateGraph.StateTransitionData.Find(data =>
                    data.GUID == stateTransition.data.GUID);
                if (match != null) continue;

                Debug.Log($"destroying {stateTransition.Label}");
                (target as WorldGraph)?.StateTransitions.Remove(stateTransition);
                Undo.DestroyObjectImmediate(stateTransition);
            }

            // -------------------- Add All Scene Handles --------------------
            foreach (var data in (target as WorldGraph)?.stateGraph.SceneStateData) {
                var match = (target as WorldGraph)?.SceneHandles.Find(sceneHandle => sceneHandle.StateData.GUID == data.GUID);
                if (match != null) continue;

                switch (data.SceneType) {
                    case SceneType.Default:
                        AddHandle(typeof(DefaultHandle), data);
                        break;
                    case SceneType.Cutscene:
                        AddHandle(typeof(CutsceneHandle), data);
                        break;
                    case SceneType.Battle:
                        AddHandle(typeof(BattleHandle), data);
                        break;
                }
            }

            // ------------ Add all StateTransition's with their data fields skipping ones that already exist ------------
            foreach (var data in (target as WorldGraph)?.stateGraph.StateTransitionData) {
                var match = (target as WorldGraph)?.StateTransitions.Find(transition => transition.data.GUID == data.GUID);
                if (match != null) continue;

                AddTransition(data);
            }

            // ------------ Add all StateTransition's to their SceneHandle, if the SceneHandle doesnt already contain ------------
            // foreach (var transition in (target as WorldGraph)?.StateTransitions) {
            //     var match = (target as WorldGraph)?.SceneHandles.Find(handle =>
            //         handle.StateData.GUID == transition.data.OutputStateGUID);
            //
            //     if (match == null || match.StateTransitions.Any(x => x.data.GUID == transition.data.GUID)) continue;
            //     match.StateTransitions.Add(transition);
            // }

            void AddHandle(Type type, SceneStateData data) {
                SceneHandle sceneHandle = Undo.AddComponent(obj, type) as SceneHandle;
                sceneHandle!.hideFlags = debugView ? HideFlags.None : HideFlags.HideInInspector;
                sceneHandle.Label = type.Name;
                sceneHandle.StateData = data;

                AddEditor(sceneHandle);

                (target as WorldGraph)?.SceneHandles.Add(sceneHandle);
            }

            void AddTransition(StateTransitionData data) {
                StateTransition stateTransition = Undo.AddComponent(obj, typeof(StateTransition)) as StateTransition;
                stateTransition!.hideFlags = debugView ? HideFlags.None : HideFlags.HideInInspector;
                stateTransition.data = data;

                (target as WorldGraph)?.StateTransitions.Add(stateTransition);
            }
        }

        public SceneHandle AddSceneHandle(Type type) { 
            GameObject gameObject = (target as WorldGraph)?.gameObject;

            SceneHandle sceneHandle = Undo.AddComponent(gameObject, type) as SceneHandle;
            sceneHandle!.hideFlags = debugView ? HideFlags.None : HideFlags.HideInInspector;
            sceneHandle.Label = type.Name;

            AddEditor(sceneHandle);

            sceneHandles.arraySize++;
            sceneHandles.GetArrayElementAtIndex(sceneHandles.arraySize - 1).objectReferenceValue = sceneHandle;

            return sceneHandle;
        }

        private void RemoveSceneHandle(int id) {
            SerializedProperty property = sceneHandles.GetArrayElementAtIndex(id);
            SceneHandle sceneHandle = property.objectReferenceValue as SceneHandle;

            (target as WorldGraph)?.SceneHandles.Remove(sceneHandle);

            _editors.Remove(sceneHandle);
            Undo.DestroyObjectImmediate(sceneHandle);
        }

        public void AddEditor(SceneHandle handle) {
            if (handle == null) return;
            if (_editors.ContainsKey(handle)) return;

            UnityEditor.Editor editor = null;
            CreateCachedEditor(handle, null, ref editor);

            _editors.Add(handle, editor);
        }
    }

}