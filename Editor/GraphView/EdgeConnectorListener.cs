﻿using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    public class EdgeConnectorListener : IEdgeConnectorListener {
        private GraphViewChange m_GraphViewChange;
        private List<Edge> m_EdgesToCreate;
        private List<GraphElement> m_EdgesToDelete;

        private WSGSearcherProvider searchWindowProvider;
        private EditorWindow editorWindow;

        public EdgeConnectorListener(EditorWindow editorWindow, WSGSearcherProvider searchWindowProvider) {
            this.editorWindow = editorWindow;
            this.searchWindowProvider = searchWindowProvider;

            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();

            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            searchWindowProvider.target = null;
            SearcherWindow.Show(editorWindow, searchWindowProvider.LoadSearchWindow(),
                item => searchWindowProvider.OnSearcherSelectEntry(item, position), position, null);
        }

        public void OnDrop(GraphView graphView, Edge edge) {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);

            m_EdgesToDelete.Clear();
            if (edge.input.capacity == Port.Capacity.Single)
                foreach (var edge1 in edge.input.connections) {
                    var edgeToDelete = edge1;
                    if (edgeToDelete != edge)
                        m_EdgesToDelete.Add(edgeToDelete);
                }

            if (edge.output.capacity == Port.Capacity.Single)
                foreach (var edge1 in edge.output.connections) {
                    var edgeToDelete = edge1;
                    if (edgeToDelete != edge)
                        m_EdgesToDelete.Add(edgeToDelete);
                }

            // Delete duplicate edges (two edges stacked on top of each other)
            foreach (var edgeToDelete in from existingEdge in graphView.edges
                let edgeToDelete = existingEdge
                where existingEdge.output.node == edge.output.node && existingEdge.input.node == edge.input.node
                select edgeToDelete) {
                m_EdgesToDelete.Add(edgeToDelete);
                Debug.Log("Duplicate Port being deleted ...");
            }
            
            if (m_EdgesToDelete.Count > 0)
                graphView.DeleteElements(m_EdgesToDelete);
            
            var edgesToCreate = m_EdgesToCreate;
            
            if (graphView.graphViewChanged != null) {
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            }

            foreach (var e in edgesToCreate) {
                graphView.AddElement(e);
                edge.input.Connect(e);
                edge.output.Connect(e);
            }
        }
    }

}