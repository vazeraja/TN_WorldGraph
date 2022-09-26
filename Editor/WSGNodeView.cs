using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public sealed class WSGNodeView : Node {

        public readonly SceneStateData stateData;
        
        public WSGPortView input;
        public WSGPortView output;
        public Color portColor;
        
        public WSGGraphView graphView => GetFirstAncestorOfType<WSGGraphView>();
        private IEdgeConnectorListener connectorListener;
        
        private TextField titleTextField;
        private Button addParameterButton;
        private Button playSceneButton;

        public WSGNodeView(SceneStateData stateData, IEdgeConnectorListener connectorListener) :
            base(AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/WGGraphNode"))) {
            this.stateData = stateData;
            this.connectorListener = connectorListener;
            
            if (string.IsNullOrEmpty(stateData.SceneName)) {
                stateData.SceneName = $"{stateData.SceneType.ToString()} Handle";
            }

            userData = stateData;
            name = stateData.SceneName;
            title = stateData.SceneName;
            viewDataKey = stateData.GUID;
            style.left = stateData.Position.x;
            style.top = stateData.Position.y;

            addParameterButton = this.Q<Button>("add-parameter-button");
            addParameterButton.style.backgroundImage = Resources.Load<Texture2D>("Sprite-0001");
            playSceneButton = this.Q<Button>("play-button");
            
            SetupTitleField();
            
            LoadDefaultPorts(stateData.Ports);
            LoadParameterPorts(stateData.Ports);
            
            addParameterButton.clicked += AddParameterPort;
            // playSceneButton.clicked += PlayScene;
        }

        private void LoadDefaultPorts(IEnumerable<PortData> portData) {
            PortData outputPortData = null;
            PortData inputPortData = null;

            var portDataList = portData.ToList();
            var loadedOutputPort = portDataList.Find(x => x.PortType == PortType.Default && x.PortDirection == "Output");
            var loadedInputPort = portDataList.Find(x => x.PortType == PortType.Default && x.PortDirection == "Input");

            switch (stateData.SceneType) {
                case SceneType.Default:
                    AddToClassList("defaultHandle");
                    portColor = new Color(0.12f, 0.44f, 0.81f);

                    if (loadedOutputPort == null) outputPortData = stateData.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WSGPortView(loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = stateData.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WSGPortView(loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
                case SceneType.Battle:
                    AddToClassList("battleHandle");
                    portColor = new Color(0.94f, 0.7f, 0.31f);

                    if (loadedOutputPort == null) outputPortData = stateData.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WSGPortView(loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = stateData.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WSGPortView(loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
                case SceneType.Cutscene:
                    AddToClassList("cutsceneHandle");
                    portColor = new Color(0.81f, 0.29f, 0.28f);

                    if (loadedOutputPort == null) outputPortData = stateData.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WSGPortView(loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = stateData.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WSGPortView(loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
            }
        }
        
        private void LoadParameterPorts(IEnumerable<PortData> portData) {
            foreach (var data in portData) {
                if (data.PortType == PortType.Parameter) {
                    var parameterPort = new WSGPortView(data, connectorListener, this);
                    inputContainer.Add(parameterPort);
                }
            }
        }
        
        private void AddParameterPort() {
            var portData = stateData.CreatePort(viewDataKey, false, false, true, portColor);
            var parameterPort = new WSGPortView(portData, connectorListener, this);

            inputContainer.Add(parameterPort);
            graphView.RegisterPortBehavior(parameterPort);
            ScreenCapture.CaptureScreenshot("Assets/TN_SceneManagement/Editor/Resources/temp.png", 1);
        }
        
        private void SetupTitleField() {
            Label titleLabel = this.Q<Label>("title-label");
            {
                // titleLabel.Bind(new SerializedObject(sceneHandle));
                // titleLabel.bindingPath = "HandleName";

                titleTextField = new TextField {isDelayed = true};
                titleTextField.style.display = DisplayStyle.None;
                titleLabel.parent.Insert(0, titleTextField);

                titleLabel.RegisterCallback<MouseDownEvent>(e => {
                    if (e.clickCount != 2 || e.button != (int) MouseButton.LeftMouse) return;

                    titleTextField.style.display = DisplayStyle.Flex;
                    titleLabel.style.display = DisplayStyle.None;
                    titleTextField.focusable = true;

                    titleTextField.SetValueWithoutNotify(title);
                    titleTextField.Focus();
                    titleTextField.SelectAll();
                });

                titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

                titleTextField.RegisterCallback<MouseDownEvent>(e => {
                    if (e.clickCount == 2 && e.button == (int) MouseButton.LeftMouse)
                        CloseAndSaveTitleEditor(titleTextField.value);
                });

                titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

                void CloseAndSaveTitleEditor(string newTitle) {
                    graphView.stateGraph.RegisterCompleteObjectUndo("Renamed node " + newTitle);
                    // sceneHandle.HandleName = newTitle;
                    stateData.SceneName = newTitle;
                    
                    // hide title TextBox
                    titleTextField.style.display = DisplayStyle.None;
                    titleLabel.style.display = DisplayStyle.Flex;
                    titleTextField.focusable = false;

                    UpdateTitle();
                }

                void UpdateTitle() {
                    // title = sceneHandle.HandleName ?? sceneHandle.GetType().Name;
                    title = stateData.SceneName ?? $"{stateData.SceneType.ToString()} Handle"; 
                }
            }
        }
        

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);
            
            stateData.Position.x = newPos.xMin;
            stateData.Position.y = newPos.yMin;
        }
    }
}