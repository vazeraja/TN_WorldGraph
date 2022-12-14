using ThunderNut.WorldGraph.Attributes;
using UnityEngine;

namespace ThunderNut.WorldGraph.Handles {
    
    [AddComponentMenu("")]
    [Path("Basic/DefaultHandle", "Default")]
    public class DefaultHandle : SceneHandle {
        public override Color HandleColor => WSGColors.Bisque;
    }
}