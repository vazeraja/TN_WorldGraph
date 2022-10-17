﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    public class ConditionOptionsPopupWindow : PopupWindowContent {
        private readonly SearchField m_SearchField;

        private readonly ExposedParameterTreeView multiColumnTreeView;

        private bool m_ShouldClose;
        public float Width;

        public ConditionOptionsPopupWindow(WorldGraph controller, StateCondition condition) {
            m_SearchField = new SearchField();
            multiColumnTreeView = ExposedParameterTreeView.Create(controller, condition);
            multiColumnTreeView.onDoubleClicked = ForceClose;
        }

        public override void OnGUI(Rect rect) {
            if (m_ShouldClose || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int remainTop = topPadding + searchHeight + border;
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2,
                rect.height - remainTop - border);

            multiColumnTreeView.searchString = m_SearchField.OnGUI(searchRect, multiColumnTreeView.searchString);
            multiColumnTreeView.OnGUI(remainingRect);
        }

        public override Vector2 GetWindowSize() {
            var result = base.GetWindowSize();
            result.x = Width;
            return result;
        }

        public override void OnOpen() {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        private void ForceClose() => m_ShouldClose = true;
    }

}