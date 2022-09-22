using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThunderNut.WorldGraph {

    [Serializable]
    public class SceneReference {
        [SerializeField] private Object sceneAsset;
        [SerializeField] public string ScenePath;
    }

}