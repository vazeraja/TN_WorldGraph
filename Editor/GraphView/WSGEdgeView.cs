using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGEdgeView : Edge {
        public WSGGraphView owner => GetFirstAncestorOfType<WSGGraphView>();

        public WSGEdgeView() {
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public override void OnSelected() {
            base.OnSelected();
            
            
            owner.DrawPropertiesInInspector((StateTransition) userData);
            
        }
        
        void OnMouseDown(MouseDownEvent e)
        {
            if (e.clickCount == 2)
            {
                // Empirical offset:
                var position = e.mousePosition;
                position += new Vector2(-10f, -28);
                Vector2 mousePos = owner.ChangeCoordinatesTo(owner.contentViewContainer, position);

                owner.AddRelayNode((WSGPortView) input, (WSGPortView) output, mousePos);
                Debug.Log("double clicked edge");
                
                input.Disconnect(this);
                output.Disconnect(this);
                owner.RemoveElement(this);
            }
        }
    }

}