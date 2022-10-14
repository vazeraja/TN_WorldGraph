using ThunderNut.StateMachine;

namespace ThunderNut.WorldGraph.Demos {

    public class EnterDashStateDecision : Decision {
        public PlayerController player => agent as PlayerController;

        public override bool Decide() {
            return player.State == PlayerController.AnimState.Dash;
        }
    }

}
