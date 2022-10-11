using MoreMountains.Feedbacks;
using ThunderNut.WorldGraph.Attributes;
using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {

    public class PlayerController : TN_MonoBehaviour {
        public InputProvider provider;
        public MMFeedbacks dashFeedback;
        
        private RuntimeStateMachine stateMachine;
        [HideInInspector] public CollisionHandler collisionHandler;

        [InspectorGroup("Walking", true, 12)]
        public float walkSpeed = 7;
        public float crouchSpeed = 3;

        [InspectorGroup("Jumping", true, 12)]
        public float firstJumpSpeed;
        public float secondJumpSpeed;
        public float fallSpeed;
        public int numberOfJumps = 2;
        public AnimationCurve jumpFallOff = AnimationCurve.Linear(0, 1, 1, 0);
        public FixedStopwatch jumpStopwatch = new FixedStopwatch();

        [InspectorGroup("Dashing", true, 12)]
        public float dashSpeed = 12;
        public FixedStopwatch dashStopwatch = new FixedStopwatch();

        [InspectorGroup("Damage", groupAllFieldsUntilNextGroupAttribute: true, 12)]
        public Vector2 hitForce;
        public FixedStopwatch hitStopwatch = new FixedStopwatch();
        public Collision2D collisionData;
        

        private InputState inputState => provider;

        public enum AnimState {
            Movement = 0,
            Dash = 1,
            Hit = 2,
        }

        public AnimState State { get; set; } = AnimState.Movement;
        public Vector2 MovementDirection => inputState.movementDirection;
        public bool IsCrouching => inputState.isCrouching;
        public int FacingDirection { get; set; } = 1;

        public float DashCompletion => dashStopwatch.Completion;
        public float JumpCompletion => jumpStopwatch.Completion;
        public bool IsJumping => !jumpStopwatch.IsFinished;
        public bool IsFirstJump => jumpsLeft == numberOfJumps - 1;

        [HideInInspector] public bool wantsToJump;
        [HideInInspector] public bool wasOnTheGround;
        [HideInInspector] public int jumpsLeft;
        [HideInInspector] public bool canDash;

        private void Awake() {
            collisionHandler = GetComponent<CollisionHandler>();

            stateMachine = Builder.RuntimeStateMachine
                .WithState(new MovementState(), out var movementState)
                .WithState(new DashState(), out var dashState)
                .WithState(new HitState(), out var hitState)
                .WithState(new RemainState(), out var remainState)
                .WithTransition(new EnterMovementStateDecision(), movementState, remainState, new[] {dashState, hitState})
                .WithTransition(new EnterDashStateDecision(), dashState, remainState, new[] {movementState})
                .WithTransition(new EnterHitStateDecision(), hitState, remainState, new[] {movementState, dashState})
                .SetCurrentState(movementState)
                .SetRemainState(remainState)
                .Build<PlayerController>(this);
        }

        private void OnEnable() {
            provider.onJump += OnJump;
            provider.onDash += OnDash;
        }

        private void OnDisable() {
            provider.onJump -= OnJump;
            provider.onDash -= OnDash;

            DestroyImmediate(stateMachine);
        }

        private void Update() => stateMachine.Update();
        private void FixedUpdate() => stateMachine.FixedUpdate();

        private void OnJump(float value) {
            wantsToJump = value > 0.5f;

            if (wantsToJump) {
                if (State != AnimState.Movement || jumpsLeft <= 0)
                    return;

                jumpsLeft--;
                jumpStopwatch.Split();
            }
            else {
                jumpStopwatch.Reset();
            }
        }

        private void OnDash(float value) => EnterDashState();

        private void OnCollisionEnter2D(Collision2D other) {
            if (other.gameObject.layer != collisionHandler.enemyMask) return;
            collisionData = other;
            EnterHitState();
        }

        private void OnCollisionStay2D(Collision2D other) {
            if (other.gameObject.layer != collisionHandler.enemyMask || State == AnimState.Hit) return;
            collisionData = other;
            EnterHitState();
        }

        private void EnterHitState() {
            if (State != AnimState.Hit && !hitStopwatch.IsReady) return;
            State = AnimState.Hit;
        }

        private void EnterDashState() {
            if (State != AnimState.Movement || !dashStopwatch.IsReady || !canDash) return;
            State = AnimState.Dash;
        }

        public void EnterMovementState() {
            State = AnimState.Movement;
        }
    }

}