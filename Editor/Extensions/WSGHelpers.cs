using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThunderNut.WorldGraph.Editor {

    public static class WSGHelpers {
        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line) {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject == null || !gameObject.TryGetComponent(out WorldGraph controller)) {
                return false;
            }
            
            string path = AssetDatabase.GetAssetPath(instanceID);

            return ShowWorldGraphEditorWindow(path);
        }
        
        [MenuItem("Tools/ThunderNut/WorldGraph/Graph")]
        private static void ShowWorldGraphEditorWindow() {
            var gameObject = Resources.Load<GameObject>("WorldGraph");
            if (gameObject == null || !gameObject.TryGetComponent(out WorldGraph controller)) {
                return;
            }
            
            string path = AssetDatabase.GetAssetPath(gameObject.GetInstanceID());
            ShowWorldGraphEditorWindow(path);
        }
        
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

        public static Dictionary<int, List<FieldInfo>> FieldInfoList = new Dictionary<int, List<FieldInfo>>();

        public static int GetFieldInfo(Object target, out List<FieldInfo> fieldInfoList) {
            Type targetType = target.GetType();
            int targetTypeHashCode = targetType.GetHashCode();

            if (!FieldInfoList.TryGetValue(targetTypeHashCode, out fieldInfoList)) {
                IList<Type> typeTree = targetType.GetBaseTypes();
                fieldInfoList = target.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
                               BindingFlags.NonPublic)
                    .OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
                    .ToList();
                FieldInfoList.Add(targetTypeHashCode, fieldInfoList);
            }

            return fieldInfoList.Count;
        }

        public static IList<Type> GetBaseTypes(this Type t) {
            var types = new List<Type>();
            while (t.BaseType != null) {
                types.Add(t);
                t = t.BaseType;
            }

            return types;
        }

        public static string SafeReadAllText(string assetPath) {
            string result = null;
            try {
                result = File.ReadAllText(assetPath, Encoding.UTF8);
            }
            catch {
                result = null;
            }

            return result;
        }

        private static IEnumerable<TSource> ExceptImpl<TSource>(
            IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            IEqualityComparer<TSource> comparer) {
            HashSet<TSource> bannedElements = new HashSet<TSource>(second, comparer);
            foreach (TSource item in first) {
                if (bannedElements.Add(item)) {
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
    }

}