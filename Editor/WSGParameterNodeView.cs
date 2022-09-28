using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGParameterNodeView : TokenNode {
        
        public WSGParameterNodeView(ExposedParameterViewData data, WSGPortView output) : base(null, output) {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PropertyNodeView"));
            userData = data;
            viewDataKey = data.Parameter.GUID;

            output.portName = data.Parameter.Name;
            output.portColor = output.PortData.PortColor;
            icon = data.Parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null;
            style.left = data.Position.x;
            style.top = data.Position.y;

            this.Q("title-label").RemoveFromHierarchy();
            Add(new VisualElement() {name = "disabledOverlay", pickingMode = PickingMode.Ignore});
            
        }
        
        public ExposedParameterViewData GetViewData() => userData as ExposedParameterViewData;

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            ((ExposedParameterViewData) userData).Position = new Vector2(newPos.xMin, newPos.yMin);
        }
    }

}