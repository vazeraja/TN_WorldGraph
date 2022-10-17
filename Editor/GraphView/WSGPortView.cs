using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGPortView : Port {
        public readonly PortData PortData;
        public Node nodeView;
        
        public Button deleteParameterButton;
        private WSGGraphView graphView;

        public event Action<Node, WSGPortView, Edge> OnConnected;
        public event Action<Node, WSGPortView, Edge> OnDisconnected;

        public WSGPortView(WSGGraphView graphView, PortData portData, IEdgeConnectorListener connectorListener, Node nodeView = null) : base(Orientation.Horizontal,
            portData.PortDirection == "Output" ? Direction.Output : Direction.Input,
            portData.PortCapacity == "Multi" ? Capacity.Multi : Capacity.Single, typeof(bool)) {
            m_EdgeConnector = new EdgeConnector<WSGEdgeView>(connectorListener);
            this.AddManipulator(m_EdgeConnector);

            this.graphView = graphView;
            this.nodeView = nodeView;
            PortData = portData;
            portColor = portData.PortColor;
            portName = portData.PortDirection;

            Initialize(portData, nodeView);
        }
        
        private void Initialize(PortData portData, Node nodeView) {
            if (portData.PortType == PortType.Parameter && nodeView != null) {
                int outputPortCount = nodeView.inputContainer.Query("connector").ToList().Count;
                portName = $"{portData.PortType.ToString()}({outputPortCount})";

                deleteParameterButton = new Button(() => { RemoveParameterPort(portData); });
                deleteParameterButton.style.backgroundImage = Resources.Load<Texture2D>("Sprite-0003");
                deleteParameterButton.style.width = 15;
                deleteParameterButton.style.height = 15;
                deleteParameterButton.style.marginLeft = 3;
                deleteParameterButton.style.marginRight = 3;
                deleteParameterButton.style.marginTop = 6;
                deleteParameterButton.style.marginBottom = 5;

                contentContainer.Add(deleteParameterButton);
            }
        }
        public void RemoveParameterPort(PortData portData) {
            var Edges = graphView.edges.ToList();
            Edge connectedEdge = Edges.Find(edge => ((WSGPortView) edge.input).PortData.GUID == portData.GUID);

            if (connectedEdge != null) {
                connectedEdge.input.Disconnect(connectedEdge);
                connectedEdge.output.Disconnect(connectedEdge);

                m_GraphView.RemoveElement(connectedEdge);
            }

            ((WSGNodeView) nodeView).sceneHandle.RemovePort(portData);
            nodeView.inputContainer.Remove(this);
        }

        public override void Connect(Edge edge) {
            OnConnected?.Invoke(node, this, edge);
            base.Connect(edge);
        }

        public override void Disconnect(Edge edge) {
            OnDisconnected?.Invoke(node, this, edge);
            base.Disconnect(edge);
        }
    }

}