using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ThunderNut.WorldGraph.Handles;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ThunderNut.WorldGraph {

    public class WorldGraphManager {
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Bootstrap() {
            var app = UnityEngine.Object.Instantiate(Resources.Load("WorldGraph")) as GameObject;
            if (app == null) throw new ApplicationException();
            app.name = "WorldGraph";

            UnityEngine.Object.DontDestroyOnLoad(app);
        }
        
        public static WorldGraph worldGraph {
            get {
                GameObject worldGraphPrefab = null;
                worldGraphPrefab = GameObject.FindGameObjectWithTag("WorldGraph");
                
                return worldGraphPrefab.GetComponent<WorldGraph>();
            }
        }
    }

    [AddComponentMenu("ThunderNut/WorldGraph/WorldGraph")]
    [DisallowMultipleComponent]
    public class WorldGraph : MonoBehaviour, IEventListener<LoadingSceneEvent> {
        public List<SceneHandle> SceneHandles = new List<SceneHandle>();
        public List<StateTransition> StateTransitions = new List<StateTransition>();
        public List<ExposedParameter> ExposedParameters = new List<ExposedParameter>();

        public List<RelayNodeData> RelayNodeData = new List<RelayNodeData>();

        public SceneHandle activeSceneHandle;
        public List<StateTransition> activeTransitions = new List<StateTransition>();
        private List<List<Func<bool>>> activeConditions = new List<List<Func<bool>>>();

        public bool Active;
        public bool enableDoubleSidedTransitions;
        public string settingD;
        public string settingE;

        public static bool LoadingInProgress;

        private void Awake() {
            InitializeSceneHandles();
            InitializeExposedParameters();

            activeSceneHandle = SceneHandles.First();
            if (activeSceneHandle.Scene.ScenePath != SceneManager.GetActiveScene().path) {
                SceneManager.LoadScene(activeSceneHandle.Scene.ScenePath);
            }
            
            SetTransitions(activeSceneHandle);
        }

        private void OnEnable() {
            this.EventStartListening<LoadingSceneEvent>();
        }

        private void OnDisable() {
            this.EventStopListening<LoadingSceneEvent>();
        }

        private void Update() {
            if (LoadingInProgress) {
                return;
            }

            CheckTransitions();

            Debug.Log("Active SceneHandle: " + activeSceneHandle.Label);
            Debug.Log("Current Transitions Count: " + activeTransitions.Count);
            Debug.Log("Current Conditions Count: " + activeConditions.Count);
            Debug.Log("Loading In Progress: " + LoadingInProgress);
        }

        public void OnEventRaised(LoadingSceneEvent loadingSceneEvent) {
            switch (loadingSceneEvent.Status) {
                case LoadingStatus.UnloadLoadingScene:
                    ChangeState(loadingSceneEvent.stateTransition.InputState);
                    break;
            }
        }

        private void ChangeState(SceneHandle sceneHandle) {
            InitializeExposedParameters();

            activeSceneHandle.Exit();
            activeSceneHandle = sceneHandle;
            activeSceneHandle.Enter();

            SetTransitions(activeSceneHandle);
        }


        private void CheckTransitions() {
            for (var i = 0; i < activeConditions.Count; i++) {
                var conditionsPerTransition = activeConditions[i];
                var conditionsMet = new bool[conditionsPerTransition.Count];

                for (var index = 0; index < conditionsPerTransition.Count; index++) {
                    Func<bool> condition = conditionsPerTransition[index];
                    conditionsMet[index] = condition();
                }

                if (conditionsMet.All(x => x)) {
                    activeSceneHandle.ExecuteStateTransition(activeTransitions[i]);
                    Debug.Log("All conditions met");
                }
            }
        }

        private void SetTransitions(SceneHandle sceneHandle) {
            activeTransitions = sceneHandle.StateTransitions;

            activeConditions = new List<List<Func<bool>>>();
            foreach (var transition in activeTransitions) {
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

                activeConditions.Add(conditionsToMeet);
            }
        }
    
        private void InitializeExposedParameters() {
            foreach (var exposedParameter in ExposedParameters) {
                exposedParameter.Dispose();
            }
        }

        private void InitializeSceneHandles() {
            foreach (var handle in SceneHandles) {
                IEnumerable<StateTransition> stateTransitions = FindTransitionsFor(handle);
                handle.StateTransitions.AddRange(stateTransitions);
            }
        }

        public void SetString(string name, string value) {
            var match = (StringParameter) ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetFloat(string name, float value) {
            var match = (FloatParameter) ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetInt(string name, int value) {
            var match = (IntParameter) ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetBool(string name, bool value) {
            var match = (BoolParameter) ExposedParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        private IEnumerable<StateTransition> FindTransitionsFor(SceneHandle handle) {
            var matches = StateTransitions.FindAll(stateTransition => handle.GUID == stateTransition.OutputStateGUID);
            return matches;
        }

        public static string GetSceneName(string scenePath) {
            int slash = scenePath.LastIndexOf("/", StringComparison.Ordinal);
            string sceneName = scenePath.Substring(slash + 1, scenePath.LastIndexOf(".", StringComparison.Ordinal) - slash - 1);
            return sceneName;
        }

        public static List<string> GetScenesInBuild() {
            List<string> scenesInBuild = new List<string>();

            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
                scenesInBuild.Add(GetSceneName(SceneUtility.GetScenePathByBuildIndex(i)));
            }

            return scenesInBuild;
        }

        public virtual ExposedParameter AddExposedParameter(Type type) {
            ExposedParameter exposedParameter;

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                exposedParameter = Undo.AddComponent(this.gameObject, type) as ExposedParameter;
            }
            else {
                exposedParameter = this.gameObject.AddComponent(type) as ExposedParameter;
            }
            #else
                exposedParameter = this.gameObject.AddComponent(type) as ExposedParameter;
            #endif

            exposedParameter!.hideFlags = HideFlags.HideInInspector;
            exposedParameter.GUID = Guid.NewGuid().ToString();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            AutoRepair();

            return exposedParameter;
        }

        public virtual void RemoveExposedParameter(ExposedParameter exposedParameter) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                Undo.DestroyObjectImmediate(exposedParameter);
            }
            else {
                DestroyImmediate(exposedParameter);
            }
            #else
                DestroyImmediate(exposedParameter);
            #endif

            ExposedParameters.Remove(exposedParameter);
            AutoRepair();
        }

        public virtual StateTransition AddStateTransition(SceneHandle outputSceneHandle, SceneHandle inputSceneHandle) {
            StateTransition stateTransition;

            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                stateTransition = Undo.AddComponent(this.gameObject, typeof(StateTransition)) as StateTransition;
            }
            else {
                stateTransition = this.gameObject.AddComponent(typeof(StateTransition)) as StateTransition;
            }
            #else
                stateTransition = this.gameObject.AddComponent(typeof(StateTransition)) as StateTransition;
            #endif

            stateTransition!.hideFlags = HideFlags.HideInInspector;
            stateTransition.GUID = Guid.NewGuid().ToString();
            stateTransition.OutputState = outputSceneHandle;
            stateTransition.InputState = inputSceneHandle;
            stateTransition.OutputStateGUID = outputSceneHandle.GUID;
            stateTransition.InputStateGUID = inputSceneHandle.GUID;

            AutoRepair();

            return stateTransition;
        }

        public virtual void RemoveStateTransition(StateTransition stateTransition) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                Undo.DestroyObjectImmediate(stateTransition);
            }
            else {
                DestroyImmediate(stateTransition);
            }
            #else
                DestroyImmediate(stateTransition);
            #endif

            StateTransitions.Remove(stateTransition);

            AutoRepair();
        }

        public virtual SceneHandle AddSceneHandle(Type type) {
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
            sceneHandle.GUID = Guid.NewGuid().ToString();
            sceneHandle.Label = type.Name;

            AutoRepair();

            return sceneHandle;
        }

        public virtual void RemoveSceneHandle(SceneHandle sceneHandle) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                Undo.DestroyObjectImmediate(sceneHandle);
            }
            else {
                DestroyImmediate(sceneHandle);
            }
            #else
                DestroyImmediate(sceneHandle);
            #endif

            SceneHandles.Remove(sceneHandle);

            AutoRepair();
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

            foreach (Component component in components) {
                if (component is not StateTransition stateTransition) continue;
                bool found = StateTransitions.Any(t => t == stateTransition);
                if (!found) {
                    StateTransitions.Add(stateTransition);
                }
            }

            foreach (Component component in components) {
                if (component is not ExposedParameter exposedParameter) continue;
                bool found = ExposedParameters.Any(t => t == exposedParameter);
                if (!found) {
                    ExposedParameters.Add(exposedParameter);
                }
            }
        }

        protected virtual void OnDestroy() {
            #if UNITY_EDITOR
            if (Application.isPlaying) return;
            foreach (Component component in gameObject.GetComponents<Component>().ToList()) {
                EditorApplication.delayCall += () => { DestroyImmediate(component); };
            }
            #endif
        }
    }

}