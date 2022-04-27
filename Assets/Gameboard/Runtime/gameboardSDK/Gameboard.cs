using Gameboard.Companion;
using System;
using UnityEngine;

namespace Gameboard
{
    public class Gameboard : MonoBehaviour
    {
        /// <summary>
        /// Controller for managing Token and Pointer input from the Gameboard. (Note that this is separate from device touch input).
        /// </summary>
        public GameboardTouchController boardTouchController { get; private set; }

        /// <summary>
        /// Controller for communicating with the Companions.
        /// </summary>
        public CompanionController companionController { get; private set; }

        /// <summary>
        /// Configuration file housing IP addresses, SDK version, device settings, etc.
        /// </summary>
        public GameboardConfig config { get; private set; }

        /// <summary>
        /// Service Manager holding all of the various Utility objects used by the Gameboard SDK.
        /// </summary>
        public GameboardServiceManager services { get; private set; }

        [Tooltip("Camera to be used for the viewport displayed on the Gameboard, and used for input of any touches or tokens. If null, Camera.main will be used instead.")]
        public Camera gameCamera;

        [Tooltip("The Gameboard ID to be used while in the Unity editor. When running on a Gameboard, this value is acquired from the board itself.")]
        public string mockGameboardId;

        /// <summary>
        /// Invoked by OnApplicationQuit when the Gameboard SDK and services are being shut down.
        /// </summary>
        public event Action GameboardShutdownBegun;

        private bool isPerformingShutdown;

        public static Gameboard singleton;

        void Start()
        {
            if (singleton != null)
            {
                GameboardLogging.LogMessage("There are two Gameboard objects in your scene. Make sure there is only ever one! The duplicate will now be destroyed.", GameboardLogging.MessageTypes.Error);
                DestroyImmediate(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            GameboardLogging.LogMessage("Initializing Gameboard", GameboardLogging.MessageTypes.Log);

            singleton = this;

            config = new GameboardConfig(mockGameboardId);
            services = new GameboardServiceManager(config);
            boardTouchController = new GameboardTouchController();
            companionController = new CompanionController();

#if UNITY_EDITOR
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationStarted += CompilationPipeline_assemblyCompilationStarted;
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += CompilationPipeline_assemblyCompilationFinished;
#endif

            boardTouchController.InjectDependencies(services.gameBoardCommunicationsUtility, services.gameBoardHandler);
            companionController.InjectDependencies(services.companionCommunications, services.companionHandler, services.jsonUtility, config);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            boardTouchController?.EnableGameboard();
            companionController?.EnableAndConnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            GameboardLogging.LogMessage("Gameboard Initialization Completed", GameboardLogging.MessageTypes.Log);
        }

#if UNITY_EDITOR
        private void CompilationPipeline_assemblyCompilationStarted(string obj)
        {
            if (singleton?.companionController != null && singleton.companionController.isConnected)
            {
                singleton?.companionController.ShutDownCompanionController();
            }
        }

        private void CompilationPipeline_assemblyCompilationFinished(string arg1, UnityEditor.Compilation.CompilerMessage[] arg2)
        {
            if (singleton?.companionController != null && !singleton.companionController.isConnected)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                singleton?.companionController.EnableAndConnect();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
#endif 

        void OnDestroy()  
        {
            DestroyGameboard();
        }

        void OnApplicationQuit()
        {
            DestroyGameboard();
        }

        void Update()
        {
            if (isPerformingShutdown)
                return;

            services?.PerformUtilityUpdate();
        }

        void LateUpdate()
        {
            if (isPerformingShutdown)
                return;

            services?.PerformUtilityLateUpdate();
        }

        void DestroyGameboard()
        {
            if (isPerformingShutdown)
                return;

            GameboardLogging.LogMessage("Destroying Gameboard", GameboardLogging.MessageTypes.Log);

            GameboardShutdownBegun?.Invoke();

            isPerformingShutdown = true;
            boardTouchController?.DisableGameboard();
            companionController?.ShutDownCompanionController();
            singleton = null;
        }

        /// <summary>
        /// Manually aligns the camera assigned to the gameCamera slot on the Gameboard component to match a standard position and orientation that fits perfectly with the Gameboard screen.
        /// </summary>
        public void AlignCameraForGameboard()
        {
            if(gameCamera == null)
            {
                Debug.LogWarning("No camera assigned to align!");
                return;
            }

            gameCamera.transform.position = new Vector3(0f, 19f, 0f);
            gameCamera.transform.eulerAngles = new Vector3(90f, 0f, 0f);
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = 19.2f;
            gameCamera.nearClipPlane = 0f;
            gameCamera.farClipPlane = 20f;
        }

        #region Boardalytics Actions
        ///
        /// Sends a play event to be tracked
        /// <param name="extras">Json string with extra data to include in the event</param>
        ///
        public void Gameboardlytics_SendPlayEvent(string extras)
        {
            AndroidJavaClass gameboardlytics = new AndroidJavaClass("com.lastgameboard.gameboardlytics.GameboardlyticsHelper");
            gameboardlytics.CallStatic("sendPlayEvent", config.androidApplicationContext.GetNativeContext(), extras);
        }
        #endregion
    }
}