using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    [Serializable]
    public class UserViewSettings {
        public bool isBlackboardVisible = true;
        public bool isInspectorVisible = true;
        public bool isPreviewVisible = true;
    }

    public class WSGGraphView : GraphView, IDisposable {
        public WorldGraph worldGraph;
        public readonly WorldStateGraph stateGraph;
        public readonly WorldGraphEditorWindow window;
        private string assetName;

        private SerializedObject serializedGraph;

        private EdgeConnectorListener m_EdgeConnectorListener;
        private WSGSearcherProvider m_SearchWindowProvider;

        public string AssetName {
            get => assetName;
            set {
                assetName = value;
                exposedParametersBlackboard.title = assetName;
            }
        }

        private const string k_UserViewSettings = "TN.WorldGraph.ToggleSettings";
        private readonly UserViewSettings m_UserViewSettings;
        private Action changeCheck { get; set; }

        private GraphInspectorView inspectorView;
        private const string k_InspectorWindowLayoutKey = "TN.WorldGraph.InspectorWindowLayout";
        private WindowDockingLayout inspectorDockingLayout => m_InspectorDockingLayout;
        private readonly WindowDockingLayout m_InspectorDockingLayout = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        private Blackboard exposedParametersBlackboard;
        private GenericMenu exposedPropertiesItemMenu;
        private const string k_BlackboardWindowLayoutKey = "TN.WorldGraph.ExposedPropertiesWindowLayout";
        private WindowDockingLayout blackboardDockingLayout => m_BlackboardDockingLayout;
        private readonly WindowDockingLayout m_BlackboardDockingLayout = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = true,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        public WSGGraphView(WorldGraphEditorWindow window, WorldStateGraph graph, string assetName) {
            this.window = window;
            stateGraph = graph;

            window.m_BlackboardButton.RegisterValueChangedCallback(UpdateUserViewBlackboardSettings);
            window.m_GraphInspectorButton.RegisterValueChangedCallback(UpdateUserViewInspectorSettings);

            string serializedSettings = EditorUserSettings.GetConfigValue(k_UserViewSettings);
            m_UserViewSettings = JsonUtility.FromJson<UserViewSettings>(serializedSettings) ?? new UserViewSettings();
            window.m_BlackboardButton.value = m_UserViewSettings.isBlackboardVisible;
            window.m_GraphInspectorButton.value = m_UserViewSettings.isInspectorVisible;

            DeserializeWindowLayout(ref m_InspectorDockingLayout, k_InspectorWindowLayoutKey);
            DeserializeWindowLayout(ref m_BlackboardDockingLayout, k_BlackboardWindowLayoutKey);

            CreateInspectorView();
            CreateBlackboard();

            UpdateSubWindowsVisibility();
            RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);

            changeCheck = UpdateSubWindowsVisibility;
            graphViewChanged = OnGraphViewChanged;

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WSGSearcherProvider>();
            m_SearchWindowProvider.Initialize(window, this, CreateNode);
            nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != window) return;
                var displayPosition = c.screenMousePosition - window.position.position;

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(window, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };

            m_EdgeConnectorListener = new EdgeConnectorListener(window, m_SearchWindowProvider);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
                    ((WSGPortView) endPort).PortData.OwnerNodeGUID != ((WSGPortView) startPort).PortData.OwnerNodeGUID &&
                    ((WSGPortView) endPort).PortData.PortDirection != ((WSGPortView) startPort).PortData.PortDirection &&
                    ((WSGPortView) endPort).PortData.PortType == ((WSGPortView) startPort).PortData.PortType)
                .ToList();
        }

        public void Initialize() {
            foreach (var stateData in stateGraph.SceneStateData) {
                var element = new WSGNodeView(stateData, m_EdgeConnectorListener);
                AddElement(element);
            }

            foreach (var edge in stateGraph.StateTransitions) {
                WSGNodeView outputView = (WSGNodeView) GetNodeByGuid(edge.OutputStateGUID);
                WSGNodeView inputView = (WSGNodeView) GetNodeByGuid(edge.InputStateGUID);
                WSGEdgeView graphEdge = outputView.output.ConnectTo<WSGEdgeView>(inputView.input);
                AddElement(graphEdge);
            }
            
            // ------------------ Create Parameters ------------------
            foreach (var exposedParam in stateGraph.ExposedParameters) {
                CreateBlackboardField(exposedParam);
            }

            ports.OfType<WSGPortView>().ToList().ForEach(RegisterPortBehavior);
        }

        private void CreateInspectorView() {
            inspectorView = new GraphInspectorView {name = "GraphInspectorView"};

            var masterPreviewViewDraggable = new WindowDraggable(null, this);
            inspectorView.AddManipulator(masterPreviewViewDraggable);
            Add(inspectorView);

            masterPreviewViewDraggable.OnDragFinished += () => {
                ApplySerializedLayout(inspectorView, inspectorDockingLayout, k_InspectorWindowLayoutKey);
            };
            inspectorView.previewResizeBorderFrame.OnResizeFinished += () => {
                ApplySerializedLayout(inspectorView, inspectorDockingLayout, k_InspectorWindowLayoutKey);
            };
        }

        private void CreateBlackboard() {
            exposedParametersBlackboard = new Blackboard(this) {title = "Exposed Parameters", subTitle = "WorldGraph"};
            {
                exposedParametersBlackboard.Add(new BlackboardSection {title = "Exposed Variables"});
                exposedParametersBlackboard.editTextRequested = (_blackboard, element, newValue) => {
                    // var param = (ExposedParameter) ((BlackboardField) element).userData;
                    // var paramNode = graphElements.OfType<ExposedParameterNodeView>().ToList()
                    //     .Find(view => (ExposedParameter) view.userData == param);
                    //
                    // param.Name = newValue;
                    // if (paramNode != null) paramNode.output.portName = newValue;

                    ((BlackboardField) element).text = newValue;
                };

                exposedPropertiesItemMenu = new GenericMenu();

                exposedPropertiesItemMenu.AddItem(new GUIContent("String"), false, () => {
                    var exposedParameter = stateGraph.CreateParameter("String");
                    CreateBlackboardField(exposedParameter);
                });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Float"), false, () => {
                    var exposedParameter = stateGraph.CreateParameter("Float");
                    CreateBlackboardField(exposedParameter);
                });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Int"), false, () => {
                    var exposedParameter = stateGraph.CreateParameter("Int");
                    CreateBlackboardField(exposedParameter);
                });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Bool"), false, () => {
                    var exposedParameter = stateGraph.CreateParameter("Bool");
                    CreateBlackboardField(exposedParameter);
                });
                exposedPropertiesItemMenu.AddSeparator($"/");

                exposedParametersBlackboard.addItemRequested += _ => exposedPropertiesItemMenu.ShowAsContext();
            }

            Add(exposedParametersBlackboard);
        }
        
        public void CreateBlackboardField(ExposedParameter parameter) {
            stateGraph.RegisterCompleteObjectUndo("Created BlackboardField");

            var blackboardField = new WSGBlackboardField(parameter);
            exposedParametersBlackboard.Add(blackboardField);

            // UpdateSerializedProperties();
        }

        public void RegisterPortBehavior(WSGPortView worldGraphPort) {
            // ------------ Dragging an edge disconnects both ports ------------
            worldGraphPort.OnConnected += (node, port, edge) => {
                WSGPortView outputPort = (WSGPortView) edge.output;
                WSGPortView inputPort = (WSGPortView) edge.input;

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (port.PortData.PortType) {
                    case PortType.Default when port.direction == Direction.Output: {
                        stateGraph.RegisterCompleteObjectUndo("Created Transition");

                        var output = (WSGNodeView) outputPort.node;
                        var input = (WSGNodeView) inputPort.node;

                        // output.stateData.Children.Add(input.stateData);
                        stateGraph.CreateTransition(output.stateData, input.stateData);

                        break;
                    }
                    case PortType.Parameter when node is WSGNodeView nodeView: {
                        break;
                    }
                    case PortType.Parameter when node is WSGParameterNodeView propertyNodeView:
                        // var param = propertyNodeView.GetViewData();
                        // var inputNodeView = (WSGNodeView) inputPort.node;
                        //
                        // param.ConnectedNode = inputNodeView.sceneHandle;
                        // param.ConnectedPortGUID = inputPort.PortData.GUID;
                        break;
                }

                // UpdateSerializedProperties();
            };
            worldGraphPort.OnDisconnected += (node, port, edge) => {
                WSGPortView outputPort = (WSGPortView) edge.output;
                WSGPortView inputPort = (WSGPortView) edge.input;

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (port.PortData.PortType) {
                    case PortType.Default when port.direction == Direction.Output: {
                        stateGraph.RegisterCompleteObjectUndo("Removed Transition");

                        var output = (WSGNodeView) outputPort.node;
                        var input = (WSGNodeView) inputPort.node;

                        // output.stateData.Children.Remove(input.stateData);

                        var edgeToRemove = stateGraph.StateTransitions.Find(
                            transition => transition.OutputStateGUID == output.stateData.GUID &&
                                          transition.InputStateGUID == input.stateData.GUID);
                        stateGraph.RemoveTransition(edgeToRemove);

                        break;
                    }
                    case PortType.Parameter when node is WSGNodeView nodeView:
                        break;
                    case PortType.Parameter when node is WSGParameterNodeView propertyNodeView:
                        // var param = propertyNodeView.GetViewData();
                        //
                        // param.ConnectedNode = null;
                        // param.ConnectedPortGUID = null;
                        break;
                }

                // UpdateSerializedProperties();
            };
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.elementsToRemove?.ForEach(elem => {
                stateGraph.RegisterCompleteObjectUndo("Removed Elements");
                switch (elem) {
                    case WSGNodeView nodeView:
                        stateGraph.SceneStateData.Remove(nodeView.stateData);
                        break;
                    // case WorldGraphEdge edge:
                    //     break;
                    case BlackboardField blackboardField:
                        ExposedParameter exposedParameter = (ExposedParameter) blackboardField.userData;
                    
                        // if (GetNodeByGuid(exposedParameter.GUID) is WSGParameterNodeView paramNode) {
                        //     RemoveParameterGraphNode(paramNode);
                        // }
                        
                        stateGraph.RemoveParameter(exposedParameter);
                        break;
                    // case WSGParameterNodeView parameterNodeView:
                    //     graph.RemoveExposedParameterViewData(parameterNodeView.GetViewData());
                    //     break;
                }
            });
            graphViewChange.movedElements?.ForEach(elem => { stateGraph.RegisterCompleteObjectUndo("Moved Elements"); });
            graphViewChange.edgesToCreate?.ForEach(elem => { stateGraph.RegisterCompleteObjectUndo("Edge Created"); });


            return graphViewChange;
        }

        private void CreateNode(Type type, Vector2 pos) {
            stateGraph.RegisterCompleteObjectUndo("Node Created");
            SceneStateData stateData = null;

            if (type == typeof(DefaultHandle)) {
                stateData = new SceneStateData("", SceneType.Default, pos);
            }
            else if (type == typeof(CutsceneHandle)) {
                stateData = new SceneStateData("", SceneType.Cutscene, pos);
            }
            else if (type == typeof(BattleHandle)) {
                stateData = new SceneStateData("", SceneType.Battle, pos);
            }

            stateGraph.SceneStateData.Add(stateData);
            var element = new WSGNodeView(stateData, m_EdgeConnectorListener);

            AddElement(element);
        }
        
        // public void RemoveParameterGraphNode(WSGParameterNodeView view) {
        //     string portGuid = view.GetViewData().ConnectedPortGUID;
        //     if (portGuid != null) {
        //         Edge connectedEdge = edges.ToList().Find(edge => ((WorldGraphPort) edge.input).PortData.GUID == portGuid);
        //         connectedEdge.input.Disconnect(connectedEdge);
        //         connectedEdge.output.Disconnect(connectedEdge);
        //     }
        //
        //     graph.RemoveExposedParameterViewData(view.GetViewData());
        //     RemoveElement(view);
        //
        //     UpdateSerializedProperties();
        // }

        public void UpdateSerializedProperties() {
            serializedGraph = new SerializedObject(stateGraph);
        }

        #region Serialize Window Layouts

        private static void DeserializeWindowLayout(ref WindowDockingLayout layout, string layoutKey) {
            string serializedLayout = EditorUserSettings.GetConfigValue(layoutKey);
            if (!string.IsNullOrEmpty(serializedLayout)) {
                layout = JsonUtility.FromJson<WindowDockingLayout>(serializedLayout) ?? new WindowDockingLayout();
            }
        }

        private void ApplySerializedWindowLayouts(GeometryChangedEvent evt) {
            UnregisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);

            ApplySerializedLayout(inspectorView, inspectorDockingLayout, k_InspectorWindowLayoutKey);
            ApplySerializedLayout(exposedParametersBlackboard, blackboardDockingLayout, k_BlackboardWindowLayoutKey);
            // ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
        }

        private void ApplySerializedLayout(VisualElement target, WindowDockingLayout layout, string layoutKey) {
            layout.ApplySize(target);
            layout.ApplyPosition(target);

            target.RegisterCallback<GeometryChangedEvent>((evt) => {
                layout.CalculateDockingCornerAndOffset(target.layout, this.layout);
                layout.ClampToParentWindow();

                string serializedWindowLayout = JsonUtility.ToJson(layout);
                EditorUserSettings.SetConfigValue(layoutKey, serializedWindowLayout);
            });
        }

        private void UpdateSubWindowsVisibility() {
            exposedParametersBlackboard.visible = m_UserViewSettings.isBlackboardVisible;
            inspectorView.visible = m_UserViewSettings.isInspectorVisible;
            // masterPreviewView.visible = toolbar.m_UserViewSettings.isPreviewVisible;
        }

        private void UserViewSettingsChangeCheck() {
            string serializedUserViewSettings = JsonUtility.ToJson(m_UserViewSettings);
            EditorUserSettings.SetConfigValue(k_UserViewSettings, serializedUserViewSettings);
        }

        private void UpdateUserViewInspectorSettings(ChangeEvent<bool> evt) {
            m_UserViewSettings.isInspectorVisible = evt.newValue;

            if (evt.previousValue == evt.newValue) return;
            UserViewSettingsChangeCheck();
            changeCheck?.Invoke();
        }

        private void UpdateUserViewBlackboardSettings(ChangeEvent<bool> evt) {
            m_UserViewSettings.isBlackboardVisible = evt.newValue;

            if (evt.previousValue == evt.newValue) return;
            UserViewSettingsChangeCheck();
            changeCheck?.Invoke();
        }

        #endregion

        public void Dispose() {
            window.m_BlackboardButton.UnregisterValueChangedCallback(UpdateUserViewBlackboardSettings);
            window.m_GraphInspectorButton.UnregisterValueChangedCallback(UpdateUserViewInspectorSettings);

            nodeCreationRequest = null;

            m_SearchWindowProvider = null;
            UnityEngine.Object.DestroyImmediate(m_SearchWindowProvider);
        }
    }

}