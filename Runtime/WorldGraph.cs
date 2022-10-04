using System;
using System.Collections.Generic;
using System.Linq;
using ThunderNut.WorldGraph.Handles;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [AddComponentMenu("ThunderNut/WorldGraph/World Graph")]
    [DisallowMultipleComponent]
    public class WorldGraph : MonoBehaviour {
        [SerializeField] private WorldStateGraph stateGraph;
        public WorldStateGraph StateGraph => stateGraph; 

        public List<SceneHandle> SceneHandles = new List<SceneHandle>();
        public List<StateTransition> StateTransitions = new List<StateTransition>();

        public SceneHandle activeSceneHandle;

        public List<StateTransition> currentTransitions;
        private List<List<Func<bool>>> currentConditions;

        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        private void Awake() {
            activeSceneHandle = SceneHandles.First();

            SetCurrentTransitions();
        }

        private void Update() {
            SetFloat("_FloatParameter", 7);
            SetBool("_BoolParameter", true);

            CheckTransitions();
        }

        private void SetCurrentTransitions() {
            currentTransitions = activeSceneHandle.StateTransitions;

            currentConditions = new List<List<Func<bool>>>();
            foreach (var transition in currentTransitions) {
                Debug.Log(transition.conditions.Count);
                var conditionsToMeet = new List<Func<bool>>();
                foreach (var condition in transition.conditions) {
                    switch (condition.value) {
                        case StringCondition stringCondition:
                            switch (stringCondition.stringOptions) {
                                case StringParamOptions.Equals:
                                    conditionsToMeet.Add(condition.StringIsEqual());
                                    break;
                                case StringParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.StringNotEqual());
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case FloatCondition floatCondition:
                            switch (floatCondition.floatOptions) {
                                case FloatParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.FloatIsGreaterThan());
                                    break;
                                case FloatParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.FloatIsLessThan());
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case IntCondition intCondition:
                            switch (intCondition.intOptions) {
                                case IntParamOptions.Equals:
                                    conditionsToMeet.Add(condition.IntIsEqual());
                                    break;
                                case IntParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.IntNotEqual());
                                    break;
                                case IntParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.IntIsGreaterThan());
                                    break;
                                case IntParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.IntIsLessThan());
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case BoolCondition boolCondition:
                            switch (boolCondition.boolOptions) {
                                case BoolParamOptions.True:
                                    conditionsToMeet.Add(condition.BoolIsTrue());
                                    break;
                                case BoolParamOptions.False:
                                    conditionsToMeet.Add(condition.BoolIsFalse());
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                    }
                }

                currentConditions.Add(conditionsToMeet);
            }
        }

        private void CheckTransitions() {
            for (var i = 0; i < currentConditions.Count; i++) {
                var conditionsPerTransition = currentConditions[i];
                var conditionsMet = new bool[conditionsPerTransition.Count];

                for (var index = 0; index < conditionsPerTransition.Count; index++) {
                    Func<bool> condition = conditionsPerTransition[index];
                    conditionsMet[index] = condition();
                }

                if (conditionsMet.All(x => x)) {
                    Debug.Log($"All Conditions Met for Transition: {currentTransitions[i].Label}");
                }
            }
        }

        public void SetString(string name, string value) {
            var match = (StringParameterField) stateGraph.ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetFloat(string name, float value) {
            var match = (FloatParameterField) stateGraph.ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetInt(string name, int value) {
            var match = (IntParameterField) stateGraph.ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetBool(string name, bool value) {
            var match = (BoolParameterField) stateGraph.ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }
    }

}