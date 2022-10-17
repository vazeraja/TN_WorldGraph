using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Action = System.Action;

namespace ThunderNut.WorldGraph.Editor {

    public class ExposedParameterTreeView : TreeView {
        public Action onDoubleClicked;
        private readonly List<ExposedParameter> data;

        private WorldGraph controller;
        private StateCondition condition;

        private ExposedParameterTreeView(WorldGraph controller, StateCondition condition) : base(
            new TreeViewState()) {
            this.controller = controller;
            this.condition = condition;

            data = controller.ExposedParameters;

            Reload();
        }

        protected override void DoubleClickedItem(int id) {
            var exposedParam = data[id];
            condition.parameter = exposedParam;

            condition.value = condition.parameter switch {
                StringParameter => new StringCondition(),
                FloatParameter => new FloatCondition(),
                IntParameter => new IntCondition(),
                BoolParameter => new BoolCondition(),
                _ => condition.value
            };

            onDoubleClicked?.Invoke();
            
            base.DoubleClickedItem(id);
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            var allItems = data.Select((param, index) => {
                var item = new TreeViewItem(index, 0, param.Name);
                return item;
            }).ToList();

            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }

        public static ExposedParameterTreeView Create(WorldGraph controller, StateCondition condition) {
            return new ExposedParameterTreeView(controller, condition);
        }
    }

}