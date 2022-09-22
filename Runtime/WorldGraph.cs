using System.Collections;
using System.Collections.Generic;
using ThunderNut.WorldGraph.Handles;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [AddComponentMenu("ThunderNut/WorldGraph/World Graph")]
    [DisallowMultipleComponent]
    public class WorldGraph : MonoBehaviour {
        public List<SceneHandle> SceneHandles = new List<SceneHandle>();

        public string settingA;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;
    }

}