﻿using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    public class BlackboardFieldManipulator : PointerManipulator {
        private WSGGraphView graphView;

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
        }

        protected override void UnregisterCallbacksFromTarget() {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
        }

        public BlackboardFieldManipulator(VisualElement root) {
            target = root.Q<VisualElement>(className: "drop-area");
            graphView = root as WSGGraphView;
        }

        private void OnPointerDown(PointerDownEvent evt) { }

        void OnDragEnter(DragEnterEvent _) { }

        void OnDragLeave(DragLeaveEvent _) { }

        private void OnDragUpdate(DragUpdatedEvent evt) {
            if (DragAndDrop.GetGenericData("DragSelection") is not List<ISelectable> selection) return;

            foreach (var selectedElement in selection.OfType<BlackboardField>()) {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            }
        }

        private void OnDragPerform(DragPerformEvent evt) {
            if (DragAndDrop.GetGenericData("DragSelection") is not List<ISelectable> selection)
                return;

            var windowRoot = graphView.window.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, evt.localMousePosition);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            foreach (var selectedElement in selection.OfType<BlackboardField>()) {
                // ExposedParameter parameter = (ExposedParameter) selectedElement.userData;
                // graphView.CreateParameterGraphNode(parameter, graphMousePosition);
            }
        }


        // private void Callback(DragExitedEvent evt) {
        //     if (target is not WorldGraphGraphView graphView) return;
        //     
        //     Vector2 nodePosition = graphView.ChangeCoordinatesTo(graphView.contentViewContainer, evt.localMousePosition);
        //
        //     if (UnityEditor.DragAndDrop.GetGenericData("DragSelection") is not List<ISelectable> selection) return;
        //
        //     foreach (var item in selection) {
        //         if (item is BlackboardField blackboardField) {
        //             Debug.Log(blackboardField.text);
        //         }
        //     }
        // }
    }

}