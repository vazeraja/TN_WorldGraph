using ThunderNut.WorldGraph.Attributes;
using UnityEngine;

namespace ThunderNut.WorldGraph.Handles {

    [AddComponentMenu("")]
    [Path("Special/BattleHandle", "Battle")]
    public class BattleHandle : SceneHandle {
        public override Color HandleColor => Color.yellow;
        
        public override SceneType SceneType => SceneType.Cutscene;
    }

}