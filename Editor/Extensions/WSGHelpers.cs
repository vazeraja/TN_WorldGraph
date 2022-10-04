using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public static string SafeReadAllText(string assetPath)
        {
            string result = null;
            try
            {
                result = File.ReadAllText(assetPath, Encoding.UTF8);
            }
            catch
            {
                result = null;
            }
            return result;
        }
        
        private static IEnumerable<TSource> ExceptImpl<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer)
        {
            HashSet<TSource> bannedElements = new HashSet<TSource>(second, comparer);
            foreach (TSource item in first)
            {
                if (bannedElements.Add(item))
                {
                    yield return item;
                }
            }
        }
        
        public static void PingAsset(string selectedGuid) {
            string path = AssetDatabase.GUIDToAssetPath(selectedGuid);
            if (selectedGuid == null) return;
            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
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