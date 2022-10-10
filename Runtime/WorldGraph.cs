using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.WorldGraph {
    
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class WorldGraph : MonoBehaviour {
        private static WorldGraph _instance;
        public static WorldGraph Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<WorldGraph>();
                    if (_instance == null) {
                        var go = new GameObject("WorldGraph");
                        _instance = go.AddComponent<WorldGraph>();
                    }
                }

                return _instance;
            }
        }

        private void Awake() {
            if (Instance != this) {
                // Debug.Log($"WorldGraph Duplicate: deleting {name}", this);
                DestroyImmediate(gameObject);
            }
            else {
                // Debug.Log($"WorldGraph: {name}", this);
                #if UNITY_EDITOR
                EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
                #endif
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnDestroy() {
            if (_instance == this) {
                // Debug.Log("WorldGraph OnDestroy: set instance to null");
                _instance = null;
            }
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            #endif
        }

        #if UNITY_EDITOR
        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj) {
            switch (obj) {
                case PlayModeStateChange.ExitingEditMode:
                    // Debug.Log("WorldGraph PlayModeStateChange ExitingEditMode: set instance to null");
                    _instance = null;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj), obj, null);
            }
        }
        #endif
        
        public static string GetSceneName(string scenePath) {
            int slash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
            string sceneName = scenePath.Substring(slash + 1, scenePath.LastIndexOf(".", StringComparison.Ordinal) - slash - 1);
            return sceneName;
        }

        public static List<string> GetScenesInBuild() {
            List<string> scenesInBuild = new List<string>();

            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                scenesInBuild.Add(GetSceneName(SceneUtility.GetScenePathByBuildIndex(i)));
            }

            return scenesInBuild;
        }
    }

}