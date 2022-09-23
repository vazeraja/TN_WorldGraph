using System;
using System.IO;
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

        private VisualElement m_Content;
        private Toolbar m_Toolbar;
        private ToolbarButton m_ShowInProjectButton;
        private ToolbarButton m_RefreshButton;
        private ToolbarButton m_SettingsButton;

        [SerializeField] private string m_Selected;
        public string selectedGuid {
            get => m_Selected;
            private set => m_Selected = value;
        }

        private WorldStateGraph graph { get; set; }

        private WSGGraphView m_GraphView;
        private WSGGraphView graphView {
            get => m_GraphView;
            set {
                if (m_GraphView != null) {
                    m_GraphView.RemoveFromHierarchy();
                    m_GraphView.Dispose();

                    m_ShowInProjectButton.clicked -= PingAsset;
                    m_RefreshButton.clicked -= Refresh;
                }

                m_GraphView = value;

                // ReSharper disable once InvertIf
                if (m_GraphView != null) {
                    m_ShowInProjectButton.clicked += PingAsset;
                    m_RefreshButton.clicked += Refresh;

                    m_GraphView.AddManipulator(new ContentDragger());
                    m_GraphView.AddManipulator(new SelectionDragger());
                    m_GraphView.AddManipulator(new RectangleSelector());
                    m_GraphView.AddManipulator(new ClickSelector());
                    m_GraphView.SetupZoom(0.05f, 8);
                    m_GraphView.AddToClassList("drop-area");

                    m_GraphView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    m_FrameAllAfterLayout = true;

                    m_Content.Add(m_GraphView);
                }
            }
        }

        private static readonly ProfilerMarker GraphLoadMarker = new ProfilerMarker("GraphLoad");
        private static readonly ProfilerMarker CreateGraphEditorViewMarker = new ProfilerMarker("CreateGraphEditorView");

        protected void OnEnable() {
            this.SetAntiAliasing(4);
        }

        private void OnDisable() {
            graph = null;
            graphView = null;
        }

        public void CreateGUI() {
            VisualElement root = rootVisualElement;
            root.styleSheets.Add(m_StyleSheet);
            m_VisualTreeAsset.CloneTree(root);

            m_Toolbar = root.Q<Toolbar>();
            m_Content = root.Q<VisualElement>("content");
            m_ShowInProjectButton = m_Toolbar.Q<ToolbarButton>("show-in-project-button");
            m_RefreshButton = m_Toolbar.Q<ToolbarButton>("refresh-button");
            m_SettingsButton = m_Toolbar.Q<ToolbarButton>("settings-button");
        }

        private void Update() {
            if (m_HasError) return;

            var updateTitle = false;
            try {
                if (graph == null && selectedGuid != null) {
                    string guid = selectedGuid;
                    selectedGuid = null;
                    Initialize(guid);
                }

                if (graph == null) {
                    Close();
                    return;
                }

                if (graphView == null) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);

                    string graphName = Path.GetFileNameWithoutExtension(assetPath);
                    var asset = AssetDatabase.LoadAssetAtPath<WorldStateGraph>(assetPath);

                    graphView = new WSGGraphView(this, asset, graphName) {
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
                graph = null;
                Debug.LogException(e);
                throw;
            }
        }

        public void Initialize(string assetGuid) {
            try {
                WorldStateGraph asset = AssetDatabase.LoadAssetAtPath<WorldStateGraph>(AssetDatabase.GUIDToAssetPath(assetGuid));

                if (asset == null || !EditorUtility.IsPersistent(asset) || selectedGuid == assetGuid)
                    return;

                string path = AssetDatabase.GetAssetPath(asset);

                selectedGuid = assetGuid;
                string graphName = Path.GetFileNameWithoutExtension(path);

                graph = asset;
                graphView = new WSGGraphView(this, graph, graphName) {name = "GraphView", viewDataKey = assetGuid,};

                UpdateTitle();
                Repaint();
            }
            catch (Exception e) {
                m_HasError = true;
                graph = null;
                m_GraphView = null;
                Debug.LogException(e);
                throw;
            }
        }

        private void UpdateTitle() {
            string assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);
            string graphName = Path.GetFileNameWithoutExtension(assetPath);

            // update blackboard title (before we add suffixes)
            if (graphView != null)
                graphView.AssetName = graphName;

            string newTitle = graphName;
            if (graph == null)
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

        private void Refresh() {
            OnDisable();
            OnEnable();
        }

        private void PingAsset() => WSGHelpers.PingAsset(selectedGuid);

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            if (m_GraphView == null)
                return;

            // this callback is only so we can run post-layout behaviors after the graph loads for the first time
            // we immediately unregister it so it doesn't get called again
            m_GraphView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            if (m_FrameAllAfterLayout) {
                m_GraphView.FrameAll();
            }

            m_FrameAllAfterLayout = false;
        }
    }

}