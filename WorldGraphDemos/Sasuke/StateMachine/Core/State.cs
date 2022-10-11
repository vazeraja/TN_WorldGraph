using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ThunderNut.WorldGraph.Demos {
    public interface IState {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }

    [Serializable]
    public abstract class State : IState {
        [HideInInspector] public string stateName;
        [HideInInspector] public string guid;
        [HideInInspector] public Vector2 position;

        public object agent;

        [Space(25)] public List<State> children = new List<State>();
        public List<Transition> transitions = new List<Transition>();

        public event Action<State> CheckTransitions;

        public abstract void Enter();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void Exit();

        public void BindAgent<T>(object type) where T : class {
            agent = type as T;
        }
        
        public void AddTransition(Transition transition) {
            transitions.Add(transition);
        }

        public void RemoveTransition(Transition transition) {
            transitions.Add(transition);
        }

        public void CheckStateTransitions() {
            foreach (var t in transitions) {
                bool decisionSucceeded = t.GetDecision().Decide();
                CheckTransitions?.Invoke(decisionSucceeded ? t.GetTrueState() : t.GetFalseState());
            }
        }
    }
}