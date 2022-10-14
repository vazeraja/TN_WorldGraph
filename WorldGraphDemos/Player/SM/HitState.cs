using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {

    public class HitState : State {
        public PlayerController player => agent as PlayerController;

        public override void Enter() {
            var relativePosition =
                (Vector2) player.transform.InverseTransformPoint(player.collisionData.transform.position);
            var direction = (player.collisionHandler.m_Rigidbody2D.centerOfMass - relativePosition).normalized;

            player.hitStopwatch.Split();
            player.collisionHandler.m_Rigidbody2D.AddForce(
                direction * player.hitForce - player.collisionHandler.Velocity,
                ForceMode2D.Impulse
            );
        }

        public override void Update() { }

        public override void FixedUpdate() {
            player.FacingDirection = player.collisionHandler.m_Rigidbody2D.velocity.x < 0 ? -1 : 1;

            player.collisionHandler.m_Rigidbody2D.AddForce(Physics2D.gravity * 4);
            if (player.hitStopwatch.IsFinished &&
                (player.collisionHandler.IsGrounded || player.collisionHandler.IsTouchingWall)) {
                player.hitStopwatch.Split();
                player.EnterMovementState();
            }
        }

        public override void Exit() { }
    }

}