using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [AddComponentMenu("ThunderNut/WorldGraph/World Graph")]
    [DisallowMultipleComponent]
    public class WorldGraph : MonoBehaviour {
        [SerializeField] private WorldStateGraph stateGraph;
        public WorldStateGraph StateGraph {
            get => stateGraph;
            set {
                stateGraph = value;
                Initialize();
            }
        }

        public List<SceneHandle> SceneHandles = new List<SceneHandle>();

        public SceneHandle activeSceneHandle;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        private void Initialize() {
            SceneHandles.Clear();

            var stateData = stateGraph.SceneStateData;
            var obj = gameObject;

            foreach (var data in stateData) {
                switch (data.SceneType) {
                    case SceneType.Default:
                        CreateSceneHandle(data, typeof(DefaultHandle));
                        break;
                    case SceneType.Cutscene:
                        CreateSceneHandle(data, typeof(CutsceneHandle));
                        break;
                    case SceneType.Battle:
                        CreateSceneHandle(data, typeof(BattleHandle));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            void CreateSceneHandle(SceneStateData data, Type type) {
                if (obj.AddComponent(type) is not SceneHandle sceneHandle) return;
                sceneHandle.Label = data.SceneType + "Handle";
                sceneHandle.StateData = data;

                SceneHandles.Add(sceneHandle);
            }
        }

        private T GetComponentByName<T>(string name) where T : SceneHandle {
            var components = GetComponents<T>().ToList();
            return components.Find(component => component.Label == name);
        }
    }

}