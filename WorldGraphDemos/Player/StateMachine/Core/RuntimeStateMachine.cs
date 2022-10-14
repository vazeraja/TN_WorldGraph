using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {
    public interface IStateMachine {
        State CurrentState { get; }
        State RemainState { get; }
        void TransitionToState(State nextState);
    }

    public delegate void StateMachineListener();

    [CreateAssetMenu]
    public class RuntimeStateMachine : ScriptableObject {
        public List<State> states = new List<State>();
        public List<Decision> decisions = new List<Decision>();

        public event StateMachineListener OnStateTransition;

        [Header("State Machine")] public State currentState;
        public State remainState;

        public void Update() {
            currentState.Update();
            currentState.CheckStateTransitions();
        }

        public void FixedUpdate() {
            currentState.FixedUpdate();
        }

        public void EnableTransitions() {
            states.ForEach(state => state.CheckTransitions -= TransitionToState);
            states.ForEach(state => state.CheckTransitions += TransitionToState);
        }

        public void DisableTransitions() {
            states.ForEach(state => state.CheckTransitions -= TransitionToState);
        }

        private void TransitionToState(State nextState) {
            if (nextState == remainState)
                return;

            currentState.Exit();
            currentState = nextState;
            currentState.Enter();
            OnStateTransition?.Invoke();
        }

        public void Bind<T>(object type) where T : class {
            states.ForEach(state => state.BindAgent<T>(type));
            decisions.ForEach(decision => decision.BindAgent<T>(type));
        }
    }
}