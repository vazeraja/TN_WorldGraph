using System;
using System.Collections;
using System.Collections.Generic;
using ThunderNut.WorldGraph.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ThunderNut.WorldGraph.Demos {

    public class PlayerController : MonoBehaviour {
        protected BoxCollider2D m_BoxCollider2D;
        protected SpriteRenderer m_SpriteRenderer;
        protected Animator m_Animator;
        protected CollisionHandler m_CollisionHandler;
        protected Rigidbody2D m_Rigidbody2D => m_CollisionHandler.m_Rigidbody2D;

        [SerializeField] private float walkingSpeed = 8f;
        [SerializeField] private float crouchingSpeed = 4f;

        private enum AnimationState {
            Idle,
            Walk,
            Run,
            Crouch,
            CrouchWalk,
            CrouchAttack,
            Jump,
        }

        [InspectorGroup("Jumping Properties", true, 12)] [SerializeField]
        private float firstJumpSpeed = 8;
        [SerializeField] private float jumpSpeed = 3;
        [SerializeField] private float fallSpeed = 12;
        [SerializeField] private int numberOfJumps = 2;
        [SerializeField] private AnimationCurve jumpFallOff = AnimationCurve.Linear(0, 1, 1, 0);
        [SerializeField] private FixedStopwatch jumpStopwatch = new FixedStopwatch();

        private AnimationState m_AnimationState { get; set; } = AnimationState.Walk;
        private Vector2 m_MovementDirection;
        private bool m_IsCrouching;
        private bool m_IsRunning;
        private int FacingDirection;

        public Vector2 Velocity => m_CollisionHandler.m_Rigidbody2D.velocity;
        public float JumpCompletion => jumpStopwatch.Completion;
        public bool IsJumping => !jumpStopwatch.IsFinished;
        public bool IsFirstJump => _jumpsLeft == numberOfJumps - 1;

        private bool _wantsToJump;
        private bool _wasOnTheGround;
        private int _jumpsLeft;
        

        private void Awake() {
            m_BoxCollider2D = GetComponent<BoxCollider2D>();
            m_SpriteRenderer = GetComponent<SpriteRenderer>();
            m_Animator = GetComponent<Animator>();
            m_CollisionHandler = GetComponent<CollisionHandler>();
        }
        
        private void Update() {
            var previousVelocity = m_Rigidbody2D.velocity;
            var velocityChange = Vector2.zero;

            switch (m_MovementDirection.x) {
                case > 0:
                    m_SpriteRenderer.flipX = false;
                    FacingDirection = 1;
                    break;
                case < 0:
                    m_SpriteRenderer.flipX = true;
                    FacingDirection = -1;
                    break;
            }
            
            if (_wantsToJump && IsJumping)
            {
                _wasOnTheGround = false;
                float currentJumpSpeed = IsFirstJump ? firstJumpSpeed : jumpSpeed;
                currentJumpSpeed *= jumpFallOff.Evaluate(JumpCompletion);
                velocityChange.y = currentJumpSpeed - previousVelocity.y;

                if (m_CollisionHandler.IsTouchingCeiling)
                    jumpStopwatch.Reset();
            }
            else if (m_CollisionHandler.IsGrounded)
            {
                _jumpsLeft = numberOfJumps;
                _wasOnTheGround = true;
            }
            else
            {
                if (_wasOnTheGround)
                {
                    _jumpsLeft -= 1;
                    _wasOnTheGround = false;
                }

                velocityChange.y = (-fallSpeed - previousVelocity.y) / 8;
            }

            velocityChange.x = (m_MovementDirection.x * walkingSpeed - previousVelocity.x);

            if (m_CollisionHandler.wallContact.HasValue)
            {
                var wallDirection = (int) Mathf.Sign(m_CollisionHandler.wallContact.Value.point.x - transform.position.x);
                var walkDirection = (int) Mathf.Sign(m_MovementDirection.x);

                if (walkDirection == wallDirection)
                    velocityChange.x = 0;
            }

            m_Rigidbody2D.AddForce(velocityChange, ForceMode2D.Impulse);

            HandleAnimation();
        }

        private void HandleAnimation() {

            if (Velocity.x != 0 && m_IsRunning) {
                m_AnimationState = AnimationState.Run;
            }

            if (Velocity.x != 0) {
                m_AnimationState = AnimationState.Walk;
            }

            if (Velocity.x == 0) {
                m_AnimationState = AnimationState.Idle;
            }

            switch (m_AnimationState) {
                case AnimationState.Idle:
                    m_Animator.Play("Player_Idle");
                    break;
                case AnimationState.Walk:
                    m_Animator.Play("Player_Walk");
                    break;
                case AnimationState.Run:
                    m_Animator.Play("Player_Run");
                    break;
                case AnimationState.Crouch:
                    break;
                case AnimationState.CrouchWalk:
                    break;
                case AnimationState.CrouchAttack:
                    break;
                case AnimationState.Jump:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void OnMovement(InputValue value) {
            m_MovementDirection = value.Get<Vector2>();
        }

        private void OnCrouch(InputValue value) {
            m_IsCrouching = value.Get<float>() > 0.5;
        }

        private void OnRun(InputValue value) {
            m_IsRunning = value.Get<float>() > 0.5;
        }

        private void OnJump(InputValue value) {
            _wantsToJump = value.Get<float>() > 0.5;
            Debug.Log("Pressing Jump: " + _wantsToJump);

            if (_wantsToJump)
                RequestJump();
            else
                jumpStopwatch.Reset();
        }

        private void RequestJump() {
            if (_jumpsLeft <= 0)
                return;

            _jumpsLeft--;
            jumpStopwatch.Split();
        }
    }

}