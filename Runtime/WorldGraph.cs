using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Codice.Client.Common;
using ThunderNut.WorldGraph.Handles;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.WorldGraph {

    [AddComponentMenu("ThunderNut/WorldGraph/World Graph")]
    [DisallowMultipleComponent]
    public class WorldGraph : MonoBehaviour {
        [SerializeField] private WorldStateGraph stateGraph;
        public WorldStateGraph StateGraph {
            get => stateGraph;
            set {
                stateGraph = value;
                SceneHandles.Clear();

                var stateData = stateGraph.SceneStateData;
                var obj = gameObject;

                foreach (var data in stateData) {
                    switch (data.SceneType) {
                        case SceneType.Default:
                            CreateSceneHandle(data, typeof(DefaultHandle));
                            break;
                        case SceneType.Cutscene:
                            CreateSceneHandle(data, typeof(CutsceneHandle));
                            break;
                        case SceneType.Battle:
                            CreateSceneHandle(data, typeof(BattleHandle));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                void CreateSceneHandle(SceneStateData data, Type type) {
                    if (obj.AddComponent(type) is not SceneHandle sceneHandle) return;
                    sceneHandle.Label = data.SceneType + "Handle";
                    sceneHandle.StateData = data;

                    SceneHandles.Add(sceneHandle);
                }
            }
        }

        public List<SceneHandle> SceneHandles = new List<SceneHandle>();
        public SceneHandle activeSceneHandle;

        private Dictionary<Transition, List<Func<bool>>> allConditionsLookupTable = new Dictionary<Transition, List<Func<bool>>>();
        public List<Transition> currentTransitions = new List<Transition>();
        private List<List<Func<bool>>> currentConditions = new List<List<Func<bool>>>();

        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        private void Awake() {
            activeSceneHandle = SceneHandles.First();
            InitializeLookupTable();

            foreach (var (key, value) in allConditionsLookupTable) {
                Debug.Log($"Transition: {key} with {value.Count} conditions");
            }

            currentTransitions = stateGraph.StateTransitions.FindAll(t => t.OutputState == activeSceneHandle.StateData);
            foreach (var transition in currentTransitions) {
                foreach (var pair in allConditionsLookupTable) {
                    if (pair.Key == transition) {
                        currentConditions.Add(pair.Value);
                    }
                }
            }
        }

        private void Update() {
            CheckTransitions();

            SetFloat("_FloatParameter", 7);
        }

        public void CheckTransitions() {
            for (var i = 0; i < currentConditions.Count; i++) {
                var conditionsPerTransition = currentConditions[i];
                var conditionsMet = new bool[conditionsPerTransition.Count];

                for (var index = 0; index < conditionsPerTransition.Count; index++) {
                    Func<bool> condition = conditionsPerTransition[index];
                    conditionsMet[index] = condition();
                }

                if (conditionsMet.All(x => x)) {
                    Debug.Log($"All Conditions Met for Transition: {currentTransitions[i]}");
                }
            }
        }

        private void InitializeLookupTable() {
            allConditionsLookupTable = new Dictionary<Transition, List<Func<bool>>>();
            foreach (var transition in stateGraph.StateTransitions) {
                var conditionsToMeet = new List<Func<bool>>();
                foreach (var condition in ((StateTransition) transition).conditions) {
                    switch (condition.value) {
                        case StringCondition stringCondition:
                            switch (stringCondition.stringOptions) {
                                case StringParamOptions.Equals:
                                    conditionsToMeet.Add(condition.StringIsEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case StringParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.StringNotEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case FloatCondition floatCondition:
                            switch (floatCondition.floatOptions) {
                                case FloatParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.FloatIsGreaterThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case FloatParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.FloatIsLessThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case IntCondition intCondition:
                            switch (intCondition.intOptions) {
                                case IntParamOptions.Equals:
                                    conditionsToMeet.Add(condition.IntIsEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.IntNotEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.IntIsGreaterThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.IntIsLessThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case BoolCondition boolCondition:
                            switch (boolCondition.boolOptions) {
                                case BoolParamOptions.True:
                                    conditionsToMeet.Add(condition.BoolIsTrue());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case BoolParamOptions.False:
                                    conditionsToMeet.Add(condition.BoolIsFalse());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                    }
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

        private T GetComponentByName<T>(string name) where T : SceneHandle {
            var components = GetComponents<T>().ToList();
            return components.Find(component => component.Label == name);
        }
    }

}