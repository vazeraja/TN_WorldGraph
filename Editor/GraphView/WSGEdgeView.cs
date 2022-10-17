using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGEdgeView : Edge {
        public WSGGraphView graphView => GetFirstAncestorOfType<WSGGraphView>();

        public override void OnSelected() {
            base.OnSelected();
            
            
            graphView.DrawPropertiesInInspector((StateTransition) userData);
            
        }
    }

}