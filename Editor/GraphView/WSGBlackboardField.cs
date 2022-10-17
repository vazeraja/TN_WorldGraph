using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGBlackboardField : BlackboardField {
        public WSGGraphView graphView => GetFirstAncestorOfType<WSGGraphView>();
        public WSGBlackboardField(ExposedParameter parameter) {
            userData = parameter;
            text = $"{parameter.Name}";
            typeText = parameter.ParameterType;
            icon = parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null;
        }

        public override void OnSelected() {
            base.OnSelected();
            graphView.DrawPropertiesInInspector((ExposedParameter) userData);
        }
    }

}