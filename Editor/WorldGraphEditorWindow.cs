using System;
using System.IO;
using ThunderNut.WorldGraph.Handles;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public class WorldGraphEditorWindow : EditorWindow {
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset;
        [SerializeField] private StyleSheet m_StyleSheet;

        [NonSerialized] private bool m_HasError;
        [NonSerialized] private bool m_FrameAllAfterLayout;
        [NonSerialized] private bool m_ProTheme;

        [SerializeField] private bool m_AssetMaybeChangedOnDisk;
        [SerializeField] private bool m_AssetMaybeDeleted;

        private const string k_SaveDataKey = "TN.WorldGraph.WSGSaveData";
        [SerializeField] private string m_LastSerializedFileContents;

        private VisualElement m_Content;
        private Toolbar m_Toolbar;
        private ToolbarButton m_ShowInProjectButton;
        private ToolbarButton m_RefreshButton;
        private ToolbarButton m_SettingsButton;
        private ToolbarButton m_SaveButton;
        public ToolbarToggle m_BlackboardButton;
        public ToolbarToggle m_GraphInspectorButton;
        private Label m_MessageLabel;

        [SerializeField] private string m_Selected;
        public string selectedGuid {
            get => m_Selected;
            private set => m_Selected = value;
        }

        private WorldGraph m_WorldGraph;

        private WorldStateGraph loadedGraph;

        [SerializeField] private WorldStateGraph m_StateGraph;
        private WorldStateGraph stateGraph {
            get => m_StateGraph;
            set {
                if (m_StateGraph != null)
                    DestroyImmediate(m_StateGraph);
                m_StateGraph = value;
            }
        }

        private WSGGraphView m_GraphView;
        private WSGGraphView graphView {
            get => m_GraphView;
            set {
                if (m_GraphView != null) {
                    m_GraphView.RemoveFromHierarchy();
                    m_GraphView.Dispose();

                    Selection.selectionChanged -= SelectionChanged;
                    m_SaveButton.clicked -= SaveAsset;
                    m_ShowInProjectButton.clicked -= PingAsset;
                    m_RefreshButton.clicked -= Refresh;
                }

                m_GraphView = value;

                // ReSharper disable once InvertIf
                if (m_GraphView != null) {
                    Selection.selectionChanged += SelectionChanged;

                    m_SaveButton.clicked += SaveAsset;
                    m_ShowInProjectButton.clicked += PingAsset;
                    m_RefreshButton.clicked += Refresh;

                    m_GraphView.Initialize();

                    m_GraphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    m_FrameAllAfterLayout = true;
                    
                    stateGraph.isDirty = false;
                    hasUnsavedChanges = false;
                    SaveAsset();

                    m_Content.Add(m_GraphView);
                }
            }
        }

        private void SelectionChanged() {
            if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent(out m_WorldGraph)) {
                graphView.worldGraph = null;
                m_MessageLabel.text = "WorldGraph not selected";
                return;
            }

            if (Selection.count > 1) {
                m_MessageLabel.text = "Cannot edit multiple WorldGraph's at once";
                return;
            }
            
            if (m_WorldGraph.StateGraph == null) {
                m_WorldGraph.StateGraph = loadedGraph;
            }

            if (GraphHasChangedSinceLastSerialization()) {
                if (EditorUtility.DisplayDialog("StateGraph's are different", "Do you want to assign the new State Graph?",
                    "Save and Assign Changes", "Don't Assign")) {
                    SaveAsset();
                    m_WorldGraph.StateGraph = loadedGraph;
                }
            }
            m_MessageLabel.text = $"{m_WorldGraph.name} selected";
            graphView.worldGraph = m_WorldGraph;
        }

        private static readonly ProfilerMarker GraphLoadMarker = new ProfilerMarker("GraphLoad");
        private static readonly ProfilerMarker CreateGraphEditorViewMarker = new ProfilerMarker("CreateGraphEditorView");

        protected void OnEnable() {
            this.SetAntiAliasing(4);
        }

        private void OnDisable() {
            graphView = null;
        }

        private void OnDestroy() {
            HandleWindowClosed();
        }

        public void CreateGUI() {
            VisualElement root = rootVisualElement;
            root.styleSheets.Add(m_StyleSheet);
            m_VisualTreeAsset.CloneTree(root);

            m_Content = root.Q<VisualElement>("content");
            m_Toolbar = root.Q<Toolbar>();

            m_ShowInProjectButton = m_Toolbar.Q<ToolbarButton>("show-in-project-button");
            m_RefreshButton = m_Toolbar.Q<ToolbarButton>("refresh-button");
            m_SettingsButton = m_Toolbar.Q<ToolbarButton>("settings-button");
            m_SaveButton = m_Toolbar.Q<ToolbarButton>("save-button");

            m_BlackboardButton = m_Toolbar.Q<ToolbarToggle>("blackboard-toggle");
            m_GraphInspectorButton = m_Toolbar.Q<ToolbarToggle>("graph-inspector-toggle");

            m_MessageLabel = m_Toolbar.Q<Label>("message-label");
        }

        private void Update() {
            if (m_HasError) return;

            var updateTitle = false;

            if (m_AssetMaybeDeleted) {
                m_AssetMaybeDeleted = false;
                if (File.Exists(AssetDatabase.GUIDToAssetPath(selectedGuid))) {
                    m_AssetMaybeChangedOnDisk = true;
                }
                else {
                    DisplayDeletedFromDiskDialog();
                }

                updateTitle = true;
            }

            if (m_AssetMaybeChangedOnDisk) {
                m_AssetMaybeChangedOnDisk = false;

                if (stateGraph != null) {
                    if (FileOnDiskHasChanged()) {
                        bool graphChanged = GraphHasChangedSinceLastSerialization();

                        if (EditorUtility.DisplayDialog(
                            "Graph has changed on disk",
                            AssetDatabase.GUIDToAssetPath(selectedGuid) + "\n\n" +
                            (graphChanged
                                ? "Do you want to reload it and lose the changes made in the graph?"
                                : "Do you want to reload it?"),
                            graphChanged ? "Discard Changes And Reload" : "Reload",
                            "Don't Reload")) {
                            // clear graph, trigger reload
                            graphView = null;
                        }
                    }
                }

                updateTitle = true;
            }

            try {
                if (stateGraph == null && selectedGuid != null) {
                    string guid = selectedGuid;
                    selectedGuid = null;
                    Initialize(guid);
                }

                if (stateGraph == null) {
                    Close();
                    return;
                }

                if (graphView == null) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);
                    string graphName = Path.GetFileNameWithoutExtension(assetPath);
                    // var asset = AssetDatabase.LoadAssetAtPath<WorldStateGraph>(assetPath);
                    // loadedGraph = asset;

                    graphView = new WSGGraphView(this, stateGraph, graphName) {
                        name = "GraphView",
                        viewDataKey = selectedGuid,
                    };

                    updateTitle = true;
                }

                if (stateGraph.isDirty) {
                    updateTitle = true;
                    stateGraph.isDirty = false;
                    hasUnsavedChanges = false;
                }

                if (updateTitle)
                    UpdateTitle();
            }
            catch (Exception e) {
                m_HasError = true;
                m_GraphView = null;
                stateGraph = null;
                Debug.LogException(e);
                throw;
            }
        }

        public void Initialize(string assetGuid) {
            try {
                string path = AssetDatabase.GUIDToAssetPath(assetGuid);
                WorldStateGraph asset = AssetDatabase.LoadAssetAtPath<WorldStateGraph>(path);

                if (asset == null || !EditorUtility.IsPersistent(asset) || selectedGuid == assetGuid)
                    return;

                selectedGuid = assetGuid;
                string graphName = Path.GetFileNameWithoutExtension(path);

                loadedGraph = asset;
                loadedGraph.name = graphName;
                loadedGraph.hideFlags = HideFlags.HideAndDontSave;

                using (GraphLoadMarker.Auto()) {
                    string serializedData = EditorUserSettings.GetConfigValue(k_SaveDataKey);

                    stateGraph = CreateInstance<WorldStateGraph>();
                    stateGraph.name = graphName;
                    stateGraph.hideFlags = HideFlags.HideAndDontSave;

                    if (!string.IsNullOrEmpty(serializedData)) {
                        EditorJsonUtility.FromJsonOverwrite(serializedData, stateGraph);
                    }
                }

                graphView = new WSGGraphView(this, stateGraph, graphName) {name = "GraphView", viewDataKey = assetGuid};
                
                UpdateTitle();
                Repaint();
            }
            catch (Exception e) {
                m_HasError = true;
                stateGraph = null;
                m_GraphView = null;
                Debug.LogException(e);
                throw;
            }
        }

        private void Initialize(WorldGraphEditorWindow other) {
            try {
                selectedGuid = other.selectedGuid;

                stateGraph = CreateInstance<WorldStateGraph>();
                stateGraph.hideFlags = HideFlags.HideAndDontSave;
                stateGraph = other.stateGraph;

                UpdateTitle();

                Repaint();
            }
            catch (Exception e) {
                Debug.LogException(e);
                m_HasError = true;
                graphView = null;
                stateGraph = null;
                throw;
            }
        }

        public void UpdateTitle() {
            string assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);
            string graphName = Path.GetFileNameWithoutExtension(assetPath);

            // update blackboard title (before we add suffixes)
            if (graphView != null)
                graphView.AssetName = graphName;

            string newTitle = graphName;
            if (stateGraph == null)
                newTitle += " (nothing loaded)";
            else {
                if (GraphHasChangedSinceLastSerialization()) {
                    hasUnsavedChanges = true;
                    saveChangesMessage = GetSaveChangesMessage();
                }
                else {
                    hasUnsavedChanges = false;
                    saveChangesMessage = "";
                }

                if (!File.Exists(AssetDatabase.GUIDToAssetPath(selectedGuid)))
                    newTitle += " (deleted)";
            }

            Texture2D icon;
            {
                icon = Resources.Load<Texture2D>("Sprite-0002");
            }
            titleContent = new GUIContent(newTitle, icon);
        }

        public void SaveAsset() {
            // -------------------- Serialize StateGraph data for GraphView -------------------- 
            m_LastSerializedFileContents = EditorJsonUtility.ToJson(stateGraph, true);
            EditorUserSettings.SetConfigValue(k_SaveDataKey, m_LastSerializedFileContents);

            // -------------------- Deserialize StateGraph data into StateGraph Asset -------------------- 
            EditorJsonUtility.FromJsonOverwrite(m_LastSerializedFileContents, loadedGraph);
            loadedGraph.name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(loadedGraph));

            hasUnsavedChanges = false;
            stateGraph.isDirty = false;
            UpdateTitle();
        }

        private string GetSaveChangesMessage() {
            return "Do you want to save the changes you made in the World Graph?\n\n" + AssetDatabase.GUIDToAssetPath(selectedGuid) +
                   "\n\nYour changes will be lost if you don't save them.";
        }

        public override void SaveChanges() {
            base.SaveChanges();
            SaveAsset();
        }

        private void HandleWindowClosed() {
            WorldGraphEditorWindow newWindow = null;
            if (!PromptSaveIfDirtyOnQuit()) {
                newWindow = CreateWindow<WorldGraphEditorWindow>(typeof(WorldGraphEditorWindow), typeof(SceneView));
                newWindow.Initialize(this);
            }
            else {
                Undo.ClearUndo(stateGraph);
            }

            stateGraph = null;
            graphView = null;

            // show new window if we have one
            if (newWindow != null) {
                newWindow.Show();
                newWindow.Focus();
            }
        }

        private bool PromptSaveIfDirtyOnQuit() {
            if (stateGraph == null) return true;
            if (!File.Exists(AssetDatabase.GUIDToAssetPath(selectedGuid))) return DisplayDeletedFromDiskDialog(false);

            if (!hasUnsavedChanges) return true;
            int option = EditorUtility.DisplayDialogComplex("Shader Graph Has Been Modified",
                GetSaveChangesMessage(), "SaveAsset", "Cancel", "Discard Changes");

            switch (option) {
                case 0:
                    SaveAsset();
                    return true;
                case 1:
                    return false;
                case 2:
                    return true;
            }

            return true;
        }

        private bool DisplayDeletedFromDiskDialog(bool reopen = true) {
            bool saved = false;
            bool okToClose = false;
            string originalAssetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);

            while (true) {
                int option = EditorUtility.DisplayDialogComplex(
                    "Graph removed from project",
                    "The file has been deleted or removed from the project folder.\n\n" +
                    originalAssetPath +
                    "\n\nWould you like to save your Graph Asset?",
                    "SaveAsset As...", "Cancel", "Discard Graph and Close Window");

                if (option == 0) {
                    SaveAsset();
                    saved = true;
                    // string savedPath = SaveAsImplementation(false);
                    // if (savedPath != null)
                    // {
                    //     saved = true;
                    //
                    //     // either close or reopen the local window editor
                    //     stateGraph = null;
                    //     selectedGuid = (reopen ? AssetDatabase.AssetPathToGUID(savedPath) : null);
                    //
                    //     break;
                    // }
                }
                else if (option == 2) {
                    okToClose = true;
                    stateGraph = null;
                    selectedGuid = null;

                    m_LastSerializedFileContents = null;
                    EditorUserSettings.SetConfigValue(k_SaveDataKey, m_LastSerializedFileContents);
                    break;
                }
                else if (option == 1) {
                    // continue in deleted state...
                    break;
                }
            }

            return (saved || okToClose);
        }

        public void CheckForChanges() {
            if (m_AssetMaybeDeleted || stateGraph == null) return;
            m_AssetMaybeChangedOnDisk = true;
            UpdateTitle();
        }

        public void AssetWasDeleted() {
            m_AssetMaybeDeleted = true;
            UpdateTitle();
        }

        private string ReadAssetFile() {
            string filePath = AssetDatabase.GUIDToAssetPath(selectedGuid);
            return WSGHelpers.SafeReadAllText(filePath);
        }

        private bool FileOnDiskHasChanged() {
            string currentFileJson = ReadAssetFile();
            return !string.Equals(currentFileJson, m_LastSerializedFileContents, StringComparison.Ordinal);
        }

        private bool GraphsAreDifferent(WorldStateGraph a, WorldStateGraph b) {
            string aGraphJson = EditorJsonUtility.ToJson(a, true);
            string bGraphJson = EditorJsonUtility.ToJson(b, true);
            return !string.Equals(aGraphJson, bGraphJson, StringComparison.Ordinal);
        }

        private bool GraphHasChangedSinceLastSerialization() {
            string currentGraphJson = EditorJsonUtility.ToJson(stateGraph, true);
            return !string.Equals(currentGraphJson, m_LastSerializedFileContents, StringComparison.Ordinal);
        }

        private void Refresh() => OnDisable();

        private void PingAsset() => WSGHelpers.PingAsset(selectedGuid);

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            if (m_GraphView == null) return;

            m_GraphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            if (m_FrameAllAfterLayout) m_GraphView.FrameAll();

            m_FrameAllAfterLayout = false;
        }
    }

}