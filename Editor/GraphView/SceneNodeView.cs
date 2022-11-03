using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using ThunderNut.WorldGraph.Attributes;
using ThunderNut.WorldGraph.Handles;

namespace ThunderNut.WorldGraph.Editor {

    public interface ISceneNodeView {
        public SceneHandle sceneHandle { get; set; }
    }

    public class SceneNodeView : Node, ISceneNodeView {
        public SceneHandle sceneHandle { get; set; }

        public WSGPortView input;
        public WSGPortView output;
        public Color portColor;

        public WSGGraphView graphView;

        Image m_PreviewImage;
        VisualElement m_TitleContainer;
        new VisualElement m_ButtonContainer;

        private VisualElement m_PreviewContainer;
        private VisualElement m_PreviewFiller;
        private VisualElement m_ControlItems;
        private VisualElement m_ControlsDivider;
        private VisualElement m_DropdownItems;
        private VisualElement m_DropdownsDivider;
        private IEdgeConnectorListener connectorListener;

        private TextField titleTextField;
        private Button addParameterButton;
        private Button playSceneButton;

        public void Initialize(WSGGraphView graphView, SceneHandle sceneHandle, IEdgeConnectorListener connectorListener)
            /* : base(AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/WGGraphNode")))  */ {
            styleSheets.Add(Resources.Load<StyleSheet>("UXML/SceneNodeView"));
            AddToClassList("SceneNode");

            if (sceneHandle == null)
                return;

            var contents = this.Q("contents");

            this.graphView = graphView;
            this.sceneHandle = sceneHandle;
            this.connectorListener = connectorListener;
            mainContainer.style.overflow = StyleKeyword.None;

            if (string.IsNullOrEmpty(sceneHandle.Label)) sceneHandle.Label = $"{sceneHandle.GetType()}";
            userData = sceneHandle;
            viewDataKey = sceneHandle.GUID;
            style.left = sceneHandle.Position.x;
            style.top = sceneHandle.Position.y;

            SetupTitleField();
            UpdateTitle();

            // Add disabled overlay
            Add(new VisualElement() {name = "disabledOverlay", pickingMode = PickingMode.Ignore});

            // Add controls container
            var controlsContainer = new VisualElement {name = "controls"};
            {
                // controlsContainer.style.backgroundColor = new Color(0.12f, 0.44f, 0.81f);
                m_ControlsDivider = new VisualElement {name = "divider"};
                m_ControlsDivider.AddToClassList("horizontal");
                controlsContainer.Add(m_ControlsDivider);
                m_ControlItems = new VisualElement {name = "items"};
                controlsContainer.Add(m_ControlItems);

                // Instantiate control views from node
                foreach (var propertyInfo in sceneHandle.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                foreach (IControlAttribute attribute in propertyInfo.GetCustomAttributes(typeof(IControlAttribute), false))
                    m_ControlItems.Add(attribute.InstantiateControl(sceneHandle, propertyInfo));
            }
            if (m_ControlItems.childCount > 0)
                contents.Add(controlsContainer);

            LoadDefaultPorts(sceneHandle.Ports);
            LoadParameterPorts(sceneHandle.Ports);

            // addParameterButton = this.Q<Button>("add-parameter-button");
            // addParameterButton.style.backgroundImage = Resources.Load<Texture2D>("Sprite-0001");
            // playSceneButton = this.Q<Button>("play-button");

            // addParameterButton.clicked += AddParameterPort;
            // playSceneButton.clicked += PlayScene;
        }

        private void LoadDefaultPorts(IEnumerable<PortData> portData) {
            PortData outputPortData = null;
            PortData inputPortData = null;

            var portDataList = portData.ToList();
            var loadedOutputPort = portDataList.Find(x => x.PortType == PortType.Default && x.PortDirection == "Output");
            var loadedInputPort = portDataList.Find(x => x.PortType == PortType.Default && x.PortDirection == "Input");

            switch (sceneHandle) {
                case DefaultHandle:
                    AddToClassList("defaultHandle");
                    portColor = new Color(0.12f, 0.44f, 0.81f);

                    if (loadedOutputPort == null) outputPortData = sceneHandle.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WSGPortView(graphView, loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = sceneHandle.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WSGPortView(graphView, loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
                case BattleHandle:
                    AddToClassList("battleHandle");
                    portColor = new Color(0.94f, 0.7f, 0.31f);

                    if (loadedOutputPort == null) outputPortData = sceneHandle.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WSGPortView(graphView, loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = sceneHandle.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WSGPortView(graphView, loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
                case CutsceneHandle:
                    AddToClassList("cutsceneHandle");
                    portColor = new Color(0.81f, 0.29f, 0.28f);

                    if (loadedOutputPort == null) outputPortData = sceneHandle.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WSGPortView(graphView, loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = sceneHandle.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WSGPortView(graphView, loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
            }
        }

        private void LoadParameterPorts(IEnumerable<PortData> portData) {
            foreach (var data in portData) {
                if (data.PortType == PortType.Parameter) {
                    var parameterPort = new WSGPortView(graphView, data, connectorListener, this);
                    inputContainer.Add(parameterPort);
                }
            }
        }

        private void AddParameterPort() {
            var portData = sceneHandle.CreatePort(viewDataKey, false, false, true, portColor);
            var parameterPort = new WSGPortView(graphView, portData, connectorListener, this);
            graphView.RegisterPortBehavior(parameterPort);

            inputContainer.Add(parameterPort);
        }

        private void SetupTitleField() {
            Label titleLabel = this.Q<Label>("title-label");
            {
                titleLabel.Bind(new SerializedObject(sceneHandle));
                titleLabel.bindingPath = "Label";

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
                    // sceneHandle.HandleName = newTitle;
                    sceneHandle.Label = newTitle;

                    // hide title TextBox
                    titleTextField.style.display = DisplayStyle.None;
                    titleLabel.style.display = DisplayStyle.Flex;
                    titleTextField.focusable = false;

                    UpdateTitle();
                }
            }
        }

        private void UpdateTitle() {
            // title = sceneHandle.HandleName ?? sceneHandle.GetType().Name;
            title = sceneHandle.Label ?? $"{sceneHandle.GetType()} ";
        }


        public override void OnSelected() {
            base.OnSelected();
            graphView.DrawPropertiesInInspector(sceneHandle);
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            sceneHandle.Position = new Vector2(newPos.xMin, newPos.yMin);
        }
    }

}