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

        [SerializeField] private bool m_AssetMaybeDeleted;

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

        [SerializeField] private WorldGraph m_WorldGraph;

        [SerializeField] private WorldStateGraph m_StateGraph;
        private WorldStateGraph stateGraph {
            get => m_StateGraph;
            set => m_StateGraph = value;
        }

        private WSGGraphView m_GraphView;
        private WSGGraphView graphView {
            get => m_GraphView;
            set {
                if (m_GraphView != null) {
                    m_GraphView.RemoveFromHierarchy();
                    m_GraphView.Dispose();

                    Selection.selectionChanged -= SelectionChanged;

                    // m_SaveButton.clicked -= saveAction;
                    m_ShowInProjectButton.clicked -= PingAsset;
                    m_RefreshButton.clicked -= Refresh;
                }

                m_GraphView = value;

                // ReSharper disable once InvertIf
                if (m_GraphView != null) {
                    Selection.selectionChanged += SelectionChanged;

                    // m_SaveButton.clicked += saveAction;
                    m_ShowInProjectButton.clicked += PingAsset;
                    m_RefreshButton.clicked += Refresh;

                    m_GraphView.Initialize();

                    m_GraphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    m_FrameAllAfterLayout = true;

                    m_Content.Add(m_GraphView);
                }
            }
        }

        private static readonly ProfilerMarker GraphLoadMarker = new ProfilerMarker("GraphLoad");
        private static readonly ProfilerMarker SelectionChangedMarker = new ProfilerMarker("CreateGraphEditorView");

        protected void OnEnable() {
            this.SetAntiAliasing(4);
        }

        private void OnDisable() {
            graphView = null;
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

                Close();

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
                    var asset = AssetDatabase.LoadAssetAtPath<WorldStateGraph>(assetPath);
                    stateGraph = asset;

                    graphView = new WSGGraphView(this, stateGraph, graphName) {
                        name = "GraphView",
                        viewDataKey = selectedGuid,
                    };

                    updateTitle = true;
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

        private void SelectionChanged() {
            using (SelectionChangedMarker.Auto()) {
                if (Selection.activeGameObject == null || !Selection.activeGameObject.TryGetComponent(out m_WorldGraph)) {
                    // graphView.worldGraph = null;
                    m_MessageLabel.text = "WorldGraph not selected";
                    return;
                }

                if (Selection.count > 1) {
                    m_MessageLabel.text = "Cannot edit multiple WorldGraph's at once";
                    return;
                }

                m_MessageLabel.text = $"{m_WorldGraph.name} selected";
                graphView.worldGraph = m_WorldGraph;
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

                using (GraphLoadMarker.Auto()) {
                    stateGraph = asset;
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
                if (!File.Exists(AssetDatabase.GUIDToAssetPath(selectedGuid)))
                    newTitle += " (deleted)";
            }

            Texture2D icon;
            {
                icon = Resources.Load<Texture2D>("Sprite-0002");
            }
            titleContent = new GUIContent(newTitle, icon);
        }

        public void CheckForChanges() {
            if (m_AssetMaybeDeleted || stateGraph == null) return;
            UpdateTitle();
        }

        public void AssetWasDeleted() {
            m_AssetMaybeDeleted = true;
            UpdateTitle();
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