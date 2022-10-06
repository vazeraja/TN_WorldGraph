using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.WorldGraph.Editor {

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : UnityEditor.Editor {
        
        private VisualElement _RootElement;
        private VisualTreeAsset _VisualTree;

        public override VisualElement CreateInspectorGUI() {
            _RootElement = new VisualElement();
            
            _VisualTree = Resources.Load<VisualTreeAsset>($"UXML/SceneHandleInspector");
            _VisualTree.CloneTree(_RootElement);
            
            _RootElement.Q<ScrollView>("content-container").Add(new IMGUIContainer(base.OnInspectorGUI));

            return _RootElement;
        }
    }

}