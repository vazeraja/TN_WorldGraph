using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using ThunderNut.WorldGraph.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.WorldGraph.Handles {

    [AddComponentMenu("")]
    [Serializable]
    public abstract class SceneHandle : TN_MonoBehaviour {
        [Tooltip("The color of this SceneHandle to display in the inspector")]
        public virtual Color HandleColor => Color.white;

        public virtual SceneType SceneType => SceneType.Default;

        public WorldGraphController worldGraphController => GetComponent<WorldGraphController>();

        [SerializeField, HideInInspector]
        public SceneStateData StateData;

        [SerializeField]
        public List<StateTransition> StateTransitions = new List<StateTransition>();

        [Tooltip("Whether or not this SceneHandle is active. Scene Handle will not work if false")]
        public bool Active = true;

        [Tooltip("The name of this SceneHandle to display in the inspector")]
        public string Label = "SceneHandle";

        [Tooltip("The scene that this SceneHandle represents")]
        public SceneReference Scene;

        /// the possible ways to load a new scene :
        /// - Direct : uses Unity's SceneManager API
        /// - SceneLoadingManager : the simple, original MM way of loading scenes
        /// - AdditiveSceneLoadingManager : a more advanced way of loading scenes, with (way) more options
        public enum LoadingModes {
            Direct,
            DefaultLoadingManager,
            AdditiveSceneLoadingManager
        }

        [InspectorGroup("Default Settings", true, 12)]
        [Header("--------------- Loading Settings ---------------")]
        [Tooltip("The loading screen scene to use ------- HAS TO BE ADDED TO YOUR BUILD SETTINGS")]
        public SceneReference LoadingScene;

        [Tooltip("the loading mode to use to load the destination scene : " +
                 "- Direct : uses Unity's SceneManager API" +
                 "- DefaultLoadingManager : the simple, original MM way of loading scenes" +
                 "- AdditiveSceneLoadingManager : a more advanced way of loading scenes, with (way) more options")]
        public LoadingModes LoadingMode = LoadingModes.AdditiveSceneLoadingManager;

        [Tooltip("the priority to use when loading the new scenes")]
        public ThreadPriority Priority = ThreadPriority.High;

        [Tooltip("whether or not to interpolate progress (slower, but usually looks better and smoother)")]
        public bool InterpolateProgress = true;

        [Tooltip("Perform extra checks to make sure the loading screen and destination scene are in the build settings")]
        public bool SecureLoad = true;

        [Header("--------------- Loading Scene Delays ---------------")]
        [Tooltip("A delay (in seconds) to apply before the first fade plays")]
        public float BeforeEntryFadeDelay = 0f;

        [Tooltip("the duration (in seconds) of the entry fade")]
        public float EntryFadeDuration = 0.2f;

        [Tooltip("a delay (in seconds) to apply after the first fade plays")]
        public float AfterEntryFadeDelay = 0f;

        [Tooltip("a delay (in seconds) to apply before the exit fade plays")]
        public float BeforeExitFadeDelay = 0f;

        [Tooltip("the duration (in seconds) of the exit fade")]
        public float ExitFadeDuration = 0.2f;

        [Header("--------------- Transitions ---------------")]
        [Tooltip("the speed at which the progress bar should move if interpolated")]
        public float ProgressInterpolationSpeed = 5f;

        [Tooltip("the order in which to play fades (really depends on the type of fader you have in your loading screen")]
        public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;

        [Tooltip("the tween to use on the entry fade")]
        public MMTweenType EntryFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));

        [Tooltip("the tween to use on the exit fade")]
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
                    MMAdditiveSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName,
                        Priority, SecureLoad, InterpolateProgress,
                        BeforeEntryFadeDelay, EntryFadeDuration, AfterEntryFadeDelay,
                        BeforeExitFadeDelay, ExitFadeDuration,
                        EntryFadeTween, ExitFadeTween,
                        ProgressInterpolationSpeed, FadeMode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}