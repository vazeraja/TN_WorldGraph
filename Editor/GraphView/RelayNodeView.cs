using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public class RelayNodeView : Node {
        public WSGPortView input;
        public WSGPortView output;

        private WSGGraphView graphView;
        private IEdgeConnectorListener connectorListener;
        public readonly RelayNodeData RelayNodeData;

        private new VisualElement inputContainer => this.Q("input");
        private new VisualElement outputContainer => this.Q("output");

        public RelayNodeView(WSGGraphView graphView, IEdgeConnectorListener connectorListener, RelayNodeData RelayNodeData) {
            this.RelayNodeData = RelayNodeData;
            this.graphView = graphView;
            this.connectorListener = connectorListener;

            userData = RelayNodeData;
            viewDataKey = RelayNodeData.GUID;
            style.left = RelayNodeData.Position.x;
            style.top = RelayNodeData.Position.y;

            this.Q("title").RemoveFromHierarchy();
            this.Q("divider").RemoveFromHierarchy();

            LoadPorts();
        }

        private void LoadPorts() {
            PortData OutputPort = null;
            PortData InputPort = null;

            if (RelayNodeData.outputPortData == null) {
                OutputPort = RelayNodeData.CreatePort(viewDataKey, true, false, Color.white);
            }

            if (RelayNodeData.inputPortData == null) {
                InputPort = RelayNodeData.CreatePort(viewDataKey, false, false, Color.white);
            }

            output = new WSGPortView(graphView, RelayNodeData.outputPortData ?? OutputPort, connectorListener, this);
            output.portName = "";
            outputContainer.Add(output);

            input = new WSGPortView(graphView, RelayNodeData.inputPortData ?? InputPort, connectorListener, this);
            input.portName = "";
            inputContainer.Add(input);
            
            UpdateSize();
        }


        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            RelayNodeData.Position = new Vector2(newPos.xMin, newPos.yMin);
            UpdateSize();
        }

        void UpdateSize() {
            if (graphView != null)
            {
                int inputEdgeCount = 1;
                style.height = Mathf.Max(30, 24 * inputEdgeCount + 5);
                style.width = -1;
                if (input != null)
                    input.style.height = -1;
                if (output != null)
                    output.style.height = -1;
                RemoveFromClassList("hideLabels");
            }
            else
            {
                style.height = 20;
                style.width = 50;
                if (input != null)
                    input.style.height = 16;
                if (output != null)
                    output.style.height = 16;
                AddToClassList("hideLabels");
            }
        }
    }

}