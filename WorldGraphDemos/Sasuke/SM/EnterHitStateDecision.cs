namespace ThunderNut.WorldGraph.Demos {

    public class EnterHitStateDecision : Decision {
        public PlayerController player => agent as PlayerController;

        public override bool Decide() {
            return player.State == PlayerController.AnimState.Hit;
        }
    }

}