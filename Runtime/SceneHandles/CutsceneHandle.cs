using ThunderNut.WorldGraph.Attributes;
using UnityEngine;

namespace ThunderNut.WorldGraph.Handles {
    
    [AddComponentMenu("")]
    [Path("Basic/CutsceneHandle", "Cutscene")]
    public class CutsceneHandle : SceneHandle {
        public override Color HandleColor => Color.red;
        
        public override SceneType SceneType => SceneType.Cutscene;
    }

}