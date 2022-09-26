using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common;
using ThunderNut.WorldGraph.Handles;
using UnityEngine;

namespace ThunderNut.WorldGraph {
    
    [AddComponentMenu("ThunderNut/WorldGraph/World Graph")]
    [DisallowMultipleComponent]
    public class WorldGraph : MonoBehaviour {

        [SerializeField] private WorldStateGraph stateGraph;
        public WorldStateGraph StateGraph {
            get => stateGraph;
            set => stateGraph = value;
        }
        
        public List<SceneHandle> SceneHandles = new List<SceneHandle>();

        public SceneHandle activeSceneHandle;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        private void Awake() {
            activeSceneHandle = GetComponentByName<BattleHandle>("Battle Handle");
        }

        private T GetComponentByName<T>(string name) where T : SceneHandle {
            var components = GetComponents<T>().ToList();
            return components.Find(component => component.Label == name);
        }
    }

}