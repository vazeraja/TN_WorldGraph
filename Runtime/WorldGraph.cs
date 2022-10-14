using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.WorldGraph {
    
    public class WorldGraph {
        
        private WorldGraphController m_Controller;
        public WorldGraphController Controller {
            get => m_Controller;
            set => m_Controller = value;
        }
        
        private WorldStateGraph m_StateGraph;
        public WorldStateGraph StateGraph {
            get => m_StateGraph;
            set => m_StateGraph = value;
        }


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap() {
            var app = UnityEngine.Object.Instantiate(Resources.Load("WorldGraph")) as GameObject;
            if (app == null) {
                throw new ApplicationException();
            }
            app.name = "WorldGraph";
            
            UnityEngine.Object.DontDestroyOnLoad(app);
        }

        public static WorldGraph GetWorldGraph() {
            var worldGraphPrefab = GameObject.FindGameObjectWithTag("WorldGraph");
            
            var controller = worldGraphPrefab.GetComponent<WorldGraphController>();
            
            WorldGraph app = new WorldGraph {
                m_Controller = controller,
                m_StateGraph = controller.stateGraph
            };
            
            return app;
        }
        
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