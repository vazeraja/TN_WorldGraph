using UnityEditor.Experimental.GraphView;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGParameterNodeView : TokenNode {
        
        
        public WSGParameterNodeView(Port input, Port output) : base(input, output) {
            
            
        }
        
        // public ExposedParameterViewData GetViewData() => userData as ExposedParameterViewData;
    }

}