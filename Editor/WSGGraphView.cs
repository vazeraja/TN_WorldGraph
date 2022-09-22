using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    public class WSGGraphView : GraphView, IDisposable {
        private WorldStateGraph graph;
        private EditorWindow window;
        private string assetName;
        
        public string AssetName {
            get => assetName;
            set {
                assetName = value;
                Debug.Log("Updating Inspector Title");
            }
        }

        public WSGGraphView(EditorWindow window, WorldStateGraph graph, string assetName) {
            this.window = window;
            this.graph = graph;
            this.assetName = assetName;
            
            
        }
        
        public void Dispose() { }
    }

}