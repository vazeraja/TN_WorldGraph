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
        private WorldGraph targetWorldGraph;
        private SerializedProperty sceneHandles;

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
            targetWorldGraph = target as WorldGraph;
            sceneHandles = serializedObject.FindProperty("SceneHandles");

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

                    // editor.OnInspectorGUI();
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
                        foreach (var f in targetWorldGraph.SceneHandles)
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

            _editors.Remove(sceneHandle!);
            Undo.DestroyObjectImmediate(sceneHandle);
        }

        public void AddEditor(SceneHandle handle) {
            if (handle == null) return;
            if (_editors.ContainsKey(handle)) return;

            UnityEditor.Editor editor = null;
            CreateCachedEditor(handle, null, ref editor);

            _editors.Add(handle, editor);
        }

        private void InitializeFromStateGraph() {
            var obj = targetWorldGraph.gameObject;
            var stateGraph = targetWorldGraph.StateGraph;
            if (stateGraph == null) {
                EditorUtility.DisplayDialog("No WorldStateGraph Assigned", "Please assign a WorldStateGraph to initialize", "OK");
                return;
            }
            
            // -------------------- Get Variables for SceneHandles and Transitions --------------------
            var SceneHandles = targetWorldGraph.SceneHandles;
            var StateTransitions = targetWorldGraph.StateTransitions;

            // -------------------- Get Variables for Old and New State/Transition Data's --------------------
            var stateData = stateGraph.SceneStateData;
            var oldStateData = SceneHandles.Select(sceneHandle => sceneHandle.StateData).ToList();
            var transitionData = stateGraph.TransitionData;
            var oldTransitionData = new List<TransitionData>();
            foreach (var sceneHandle in SceneHandles) {
                oldTransitionData.AddRange(sceneHandle.StateTransitions.Select(st => st.TransitionData));
            }


            List<SceneStateData> newElements = new List<SceneStateData>(); 
            
            // -------------------- Find Elements which were newly added + elements which were removed --------------------
            var elementsNotInGraph = oldStateData.Except(stateData).ToList();
            var newElementsInGraph = stateData.Except(oldStateData).ToList();

            var transitionNotInGraph = oldTransitionData.Except(transitionData).ToList();
            var newTransitionInGraph = transitionData.Except(oldTransitionData).ToList();

            elementsNotInGraph.ForEach(s => Debug.Log("States Not in Graph: " + s.SceneName));
            newElementsInGraph.ForEach(s => Debug.Log("New States in Graph: " + s.SceneName));

            transitionNotInGraph.ForEach(t => Debug.Log("Transitions Not in Graph: " + t));
            newTransitionInGraph.ForEach(t => Debug.Log("New Transitions in Graph: " + t));
            
            // -------------------- First remove the ones which were removed from the graph --------------------
            foreach (var elem in elementsNotInGraph) {
                RemoveHandle(elem);
            }
            
            // -------------------- Then add the ones which were newly added to graph --------------------
            foreach (var data in newElementsInGraph) {
                switch (data.SceneType) {
                    case SceneType.Default:
                        AddHandle(data, typeof(DefaultHandle));
                        break;
                    case SceneType.Cutscene:
                        AddHandle(data, typeof(CutsceneHandle));
                        break;
                    case SceneType.Battle:
                        AddHandle(data, typeof(BattleHandle));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            // -------------------- Repeat again with the state transitions --------------------
            foreach (var data in transitionNotInGraph) {
                RemoveTransition(data);
            }

            foreach (var newData in newTransitionInGraph) {
                CreateTransition(newData, typeof(StateTransition));
            }

            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            OnEnable();

            #region Inline Functions
            // -------------------- Add transition to the corresponding SceneHandle, and AddComponent --------------------
            void CreateTransition(TransitionData data, Type type) {
                StateTransition stateTransition = Undo.AddComponent(obj, type) as StateTransition;
                stateTransition!.Label = type.Name;
                stateTransition.TransitionData = data;
 
                (target as WorldGraph)?.StateTransitions.Add(stateTransition);
                
                var match = SceneHandles.Find(handle => handle.StateData.GUID == data.OutputStateGUID);
                match.StateTransitions.Add(stateTransition);
            }
            
            // -------------------- Remove transition from the SceneHandle which owns it, then destroy it --------------------
            void RemoveTransition(TransitionData data) {
                StateTransition toRemove = null;
                SceneHandle owner = null;

                foreach (SceneHandle sceneHandle in SceneHandles) {
                    foreach (StateTransition stateTransition in sceneHandle.StateTransitions) {
                        if (stateTransition.TransitionData.OutputStateGUID != data.OutputStateGUID) continue;
                        toRemove = stateTransition;
                        owner = sceneHandle;
                    }
                }
                
                (target as WorldGraph)?.StateTransitions.Remove(toRemove);
                if(owner != null) owner.StateTransitions.Remove(toRemove);
                
                Undo.DestroyObjectImmediate(toRemove);
            }
            
            // -------------------- Add SceneHandle and add as Component --------------------
            void AddHandle(SceneStateData data, Type type) {
                SceneHandle sceneHandle = Undo.AddComponent(obj, type) as SceneHandle;
                sceneHandle!.Label = data.SceneType + "Handle";
                sceneHandle.StateData = data;
                
                AddEditor(sceneHandle);

                (target as WorldGraph)?.SceneHandles.Add(sceneHandle);
            }
            
            // -------------------- Remove SceneHandle and Destroy Component --------------------
            void RemoveHandle(SceneStateData data) {
                var toRemove = SceneHandles.Find(x => x.StateData.Equals(data));
                
                (target as WorldGraph)?.SceneHandles.Remove(toRemove);
                Undo.DestroyObjectImmediate(toRemove);
            }

            #endregion
        }
    }

}