using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {

    public class DashState : State {
        public PlayerController player => agent as PlayerController;

        public override void Enter() {
            player.dashStopwatch.Split();
            player.canDash = false;
            
            player.dashFeedback.PlayFeedbacks();
        }

        public override void Update() { }

        public override void FixedUpdate() {
            player.collisionHandler.m_Rigidbody2D.AddForce(
                new Vector2(player.FacingDirection * player.dashSpeed, 0) - player.collisionHandler.m_Rigidbody2D.velocity,
                ForceMode2D.Impulse
            );

            if (!player.dashStopwatch.IsFinished && !player.collisionHandler.IsTouchingWall) return;
            player.dashStopwatch.Split();
            player.EnterMovementState();
        }

        public override void Exit() { }
    }

}