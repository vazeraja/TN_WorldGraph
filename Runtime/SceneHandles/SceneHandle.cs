using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using ThunderNut.WorldGraph.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.WorldGraph.Handles {

    public class Yeehaw { }

    [AddComponentMenu("")]
    [Serializable]
    public abstract class SceneHandle : MonoBehaviour {
        [SerializeField] private string m_GUID;
        public string GUID {
            get => m_GUID;
            set => m_GUID = value;
        }

        [SerializeField] private Vector2 m_Position;
        public Vector2 Position {
            get => m_Position;
            set => m_Position = value;
        }

        [SerializeField] private List<PortData> m_Ports = new List<PortData>();
        public List<PortData> Ports {
            get => m_Ports;
            set => m_Ports = value;
        }

        [Tooltip("The color of this SceneHandle to display in the inspector")]
        public virtual Color HandleColor => Color.white;

        public WorldGraph Controller => GetComponent<WorldGraph>();

        [Tooltip("Whether or not this SceneHandle is active. SceneHandle will not work if false")]
        public bool Active = true;

        [Tooltip("The name of this SceneHandle to display in the inspector")]
        public string Label = "SceneHandle";

        [Tooltip("The scene that this SceneHandle represents")]
        public SceneReference Scene;

        [SerializeField] public List<StateTransition> StateTransitions = new List<StateTransition>();

        /// the possible ways to load a new scene :
        /// - Direct : uses Unity's SceneManager API
        /// - SceneLoadingManager : the simple, original MM way of loading scenes
        /// - AdditiveSceneLoadingManager : a more advanced way of loading scenes, with (way) more options
        public enum LoadingModes {
            Direct,
            DefaultLoadingManager,
            AdditiveSceneLoadingManager
        }

        [Header("Loading Settings")] [Tooltip("The loading screen scene to use ------- HAS TO BE ADDED TO YOUR BUILD SETTINGS")]
        public SceneReference LoadingScene;


        [Tooltip("The loading mode to use to load the destination scene : " +
                 "- Direct : uses Unity's SceneManager API" +
                 "- DefaultLoadingManager : the simple way of loading scenes" +
                 "- AdditiveSceneLoadingManager : a more advanced way of loading scenes, with (way) more options")]
        [SerializeField]
        private LoadingModes m_LoadingModes = LoadingModes.AdditiveSceneLoadingManager;
        [EnumControl("LoadingMode")]
        public LoadingModes LoadingMode {
            get => m_LoadingModes;
            set => m_LoadingModes = value;
        }

        [Tooltip("The priority to use when loading the new scenes")]
        public ThreadPriority Priority = ThreadPriority.High;

        [Tooltip("Whether or not to interpolate progress (slower, but usually looks better and smoother)")]
        public bool InterpolateProgress = true;

        [Tooltip("Perform extra checks to make sure the loading screen and destination scene are in the build settings")]
        public bool SecureLoad = true;

        [Header("Loading Scene Delays")] [Tooltip("A delay (in seconds) to apply before the first fade plays")]
        public float BeforeEntryFadeDelay = 0f;

        [Tooltip("The duration (in seconds) of the entry fade")]
        public float EntryFadeDuration = 0.2f;

        [Tooltip("A delay (in seconds) to apply after the first fade plays")]
        public float AfterEntryFadeDelay = 0f;

        [Tooltip("A delay (in seconds) to apply before the exit fade plays")]
        public float BeforeExitFadeDelay = 0f;

        [Tooltip("the duration (in seconds) of the exit fade")]
        public float ExitFadeDuration = 0.2f;

        [Header("Transitions")] [Tooltip("the speed at which the progress bar should move if interpolated")]
        public float ProgressInterpolationSpeed = 5f;

        [Tooltip("The order in which to play fades (really depends on the type of fader you have in your loading screen")]
        public AdditiveSceneLoadingManager.FadeModes FadeMode = AdditiveSceneLoadingManager.FadeModes.FadeInThenOut;

        [Tooltip("The tween to use on the entry fade")]
        public MMTweenType EntryFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));

        [Tooltip("The tween to use on the exit fade")]
        public MMTweenType ExitFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));

        public virtual void ExecuteStateTransition(StateTransition stateTransition) {
            if (!Active) return;

            string DestinationSceneName = WorldGraph.GetSceneName(stateTransition.InputState.Scene.ScenePath);
            string LoadingSceneName = WorldGraph.GetSceneName(LoadingScene.ScenePath);

            switch (LoadingMode) {
                case LoadingModes.Direct:
                    SceneManager.LoadScene(DestinationSceneName);
                    break;
                case LoadingModes.DefaultLoadingManager:
                    MMSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName);
                    break;
                case LoadingModes.AdditiveSceneLoadingManager:
                    AdditiveSceneLoadingManager.LoadScene(stateTransition, DestinationSceneName, LoadingSceneName,
                        Priority, SecureLoad, InterpolateProgress,
                        BeforeEntryFadeDelay, EntryFadeDuration, AfterEntryFadeDelay,
                        BeforeExitFadeDelay, ExitFadeDuration,
                        EntryFadeTween, ExitFadeTween,
                        ProgressInterpolationSpeed, FadeMode);
                    break;
            }
        }

        public virtual void Enter() { }
        public virtual void Exit() { }

        public PortData CreatePort(string ownerGUID, bool isOutput, bool isMulti, bool isParameter, Color portColor) {
            var portData = new PortData {
                OwnerNodeGUID = ownerGUID,
                GUID = Guid.NewGuid().ToString(),

                PortDirection = isOutput ? "Output" : "Input",
                PortCapacity = isMulti ? "Multi" : "Single",
                PortType = isParameter ? PortType.Parameter : PortType.Default,
                PortColor = portColor,
            };
            Ports.Add(portData);
            return portData;
        }

        public void RemovePort(PortData portData) {
            Ports.Remove(portData);
        }
    }

}