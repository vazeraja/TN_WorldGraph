using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ThunderNut.WorldGraph.Editor {

    public static class WSGHelpers {
        private static bool ShowWorldGraphEditorWindow(string path) {
            string guid = AssetDatabase.AssetPathToGUID(path);

            foreach (var w in Resources.FindObjectsOfTypeAll<WorldGraphEditorWindow>()) {
                if (w.selectedGuid != guid) continue;
                w.Focus();
                return true;
            }

            var window = EditorWindow.CreateWindow<WorldGraphEditorWindow>(typeof(WorldGraphEditorWindow), typeof(SceneView));
            window.minSize = new Vector2(1200, 600);
            window.Initialize(guid);
            window.Focus();

            return true;
        }

        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line) {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as WorldStateGraph;
            string path = AssetDatabase.GetAssetPath(instanceID);

            if (asset == null || !path.Contains("WorldGraph"))
                return false;

            return ShowWorldGraphEditorWindow(path);
        }
    }

}