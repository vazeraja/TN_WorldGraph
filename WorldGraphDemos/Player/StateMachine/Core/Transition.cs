using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.WorldGraph.Demos {
    
    public interface ITransition {
        void SetDecision(Decision decision);
        void SetStates(State a, State b);
        Decision GetDecision();
        List<State> GetStates();
    }

    [Serializable]
    public class Transition : ITransition {
        [SerializeField] private Decision decision;
        [SerializeField] private State trueState;
        [SerializeField] private State falseState;

        public void SetDecision(Decision d) {
            decision = d;
        }

        public void SetStates(State t, State f) {
            trueState = t;
            falseState = f;
        }

        public Decision GetDecision() => decision;
        public List<State> GetStates() => new List<State> {trueState, falseState};
        public State GetTrueState() => trueState;
        public State GetFalseState() => falseState;
    }
}