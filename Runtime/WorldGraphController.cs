using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThunderNut.WorldGraph.Handles;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace ThunderNut.WorldGraph {

    public struct StateTransitionEvent {
        public StateTransition StateTransition;
        private static StateTransitionEvent e;

        public StateTransitionEvent(StateTransition t) {
            StateTransition = t;
        }

        public static void Trigger(StateTransition t) {
            e.StateTransition = t;

            EventManager.TriggerEvent(e);
        }
    }

    [AddComponentMenu("ThunderNut/WorldGraph/WorldGraphController")]
    [DisallowMultipleComponent]
    public class WorldGraphController : MonoBehaviour, IEventListener<StateTransitionEvent> {
        [SerializeField] public WorldStateGraph stateGraph;

        public List<SceneHandle> SceneHandles = new List<SceneHandle>();
        public List<StateTransition> StateTransitions = new List<StateTransition>();

        public List<StateTransition> currentTransitions;
        private List<List<Func<bool>>> currentConditions;
        private bool isInitiatingSceneTransition = false;

        public SceneHandle activeSceneHandle;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        private void Awake() {
            foreach (var handle in SceneHandles) {
                IEnumerable<StateTransition> stateTransitions = FindTransitions(handle);
                handle.StateTransitions.AddRange(stateTransitions);
            }

            activeSceneHandle = SceneHandles.First();
            if (activeSceneHandle.Scene.ScenePath != SceneManager.GetActiveScene().path) {
                SceneManager.LoadScene(activeSceneHandle.Scene.ScenePath);
            }

            SetCurrentTransitions();
        }

        private void OnEnable() {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            this.EventStartListening<StateTransitionEvent>();
        }

        private void OnActiveSceneChanged(Scene from, Scene to) {
            Debug.Log($"From: {from.name} --- To: {to.name}");
        }

        private void OnDisable() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            this.EventStopListening<StateTransitionEvent>();

            stateGraph.Dispose();
        }

        private void Update() {
            if (isInitiatingSceneTransition) return;

            CheckTransitions();
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
                    StateTransitionEvent.Trigger(currentTransitions[i]);
                }
            }
        }

        public void OnEventRaised(StateTransitionEvent stateTransitionEvent) {
            isInitiatingSceneTransition = true;
            activeSceneHandle.ExecuteStateTransition(stateTransitionEvent.StateTransition);
        }

        private void SetCurrentTransitions() {
            currentTransitions = activeSceneHandle.StateTransitions;

            currentConditions = new List<List<Func<bool>>>();
            foreach (var transition in currentTransitions) {
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

        private IEnumerable<StateTransition> FindTransitions(SceneHandle handle) {
            var matches = StateTransitions.FindAll(stateTransition => handle.StateData.GUID == stateTransition.data.OutputStateGUID);
            return matches;
        }

        public virtual SceneHandle AddSceneHandle(System.Type type) {
            SceneHandle sceneHandle;

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                sceneHandle = Undo.AddComponent(this.gameObject, type) as SceneHandle;
            }
            else {
                sceneHandle = this.gameObject.AddComponent(type) as SceneHandle;
            }
            #else
                sceneHandle = this.gameObject.AddComponent(feedbackType) as SceneHandle;
            #endif

            sceneHandle!.hideFlags = HideFlags.HideInInspector;
            sceneHandle.Label = type.Name;

            AutoRepair();

            return sceneHandle;
        }

        public virtual void RemoveSceneHandle(int id) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                Undo.DestroyObjectImmediate(SceneHandles[id]);
            }
            else {
                DestroyImmediate(SceneHandles[id]);
            }
            #else
                DestroyImmediate(SceneHandles[id]);
            #endif

            SceneHandles.RemoveAt(id);
            AutoRepair();
        }

        public virtual void AutoRepair() {
            var components = gameObject.GetComponents<Component>().ToList();
            foreach (Component component in components) {
                if (component is not SceneHandle handle) continue;
                bool found = SceneHandles.Any(t => t == handle);
                if (!found) {
                    SceneHandles.Add(handle);
                }
            }
        }

        protected virtual void OnDestroy() {
            #if UNITY_EDITOR
            if (Application.isPlaying) return;
            foreach (SceneHandle sceneHandle in SceneHandles) {
                EditorApplication.delayCall += () => { DestroyImmediate(sceneHandle); };
            }
            #endif
        }
    }

}