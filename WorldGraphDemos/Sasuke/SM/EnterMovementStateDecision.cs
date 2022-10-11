using ThunderNut.StateMachine;

namespace ThunderNut.WorldGraph.Demos {

    public class EnterMovementStateDecision : Decision {
        public PlayerController player => agent as PlayerController;

        public override bool Decide() {
            return player.State == PlayerController.AnimState.Movement;
        }
    }

}