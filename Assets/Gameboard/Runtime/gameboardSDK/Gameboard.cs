using Gameboard.Companion;
using Gameboard.Utilities;
using System;
using System.Threading;
using UnityEngine;

namespace Gameboard
{
    public class Gameboard : Singleton<Gameboard>
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

        /// <summary>
        /// Event emitted when the gameboard is fully initialized and ready to be used.
        /// </summary>
        public event Action GameboardInitializationCompleted;

        private bool gameboardInitialized = false;
        
        /// <summary>
        /// Bool for if the gameboard fully initialized and ready to use
        /// </summary>
        public bool IsInitialized => gameboardInitialized;

        [Tooltip("The Gameboard ID to be used while in the Unity editor. When running on a Gameboard, this value is acquired from the board itself.")]
        public string mockGameboardId;

        /// <summary>
        /// Invoked by OnApplicationQuit when the Gameboard SDK and services are being shut down.
        /// </summary>
        public event Action GameboardShutdownBegun;

        private bool isPerformingShutdown;
        
        // We should probably move this to a json config file, or use the package version
        public const int COMPANION_VERSION = 1;

        // TODO: we can likely just remove this, nothing should be using it, but I left it in case people made games referencing it.
        public static Gameboard singleton => Instance;

        protected override void OnAwake()
        {
            base.OnAwake();
            GameboardLogging.Verbose("Initializing Gameboard");
            config = new GameboardConfig(mockGameboardId);
            services = new GameboardServiceManager(config);
            boardTouchController = new GameboardTouchController();
            companionController = new CompanionController();

#if UNITY_EDITOR
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationStarted += CompilationPipeline_assemblyCompilationStarted;
            UnityEditor.Compilation.CompilationPipeline.assemblyCompilationFinished += CompilationPipeline_assemblyCompilationFinished;
#endif

            ThreadStart injectDependencies = InjectGameboardDependencies;
            Thread dependendiesThread = new Thread(injectDependencies);
            dependendiesThread.Start();

            GameboardLogging.Verbose("Gameboard awake Completed");
        }

        private void InjectGameboardDependencies()
        {
            GameboardLogging.Verbose("injecting Gameboard dependencies");
            boardTouchController.InjectDependencies(services.gameBoardCommunicationsUtility, services.gameBoardHandler);
            companionController.InjectDependencies(services.companionCommunications, services.companionHandler, services.jsonUtility, config);
            boardTouchController?.EnableGameboard();
            companionController?.EnableAndConnect();
            gameboardInitialized = true;
            GameboardInitializationCompleted?.Invoke();
            GameboardLogging.Verbose("Gameboard initialization Completed");
        }

#if UNITY_EDITOR
        private void CompilationPipeline_assemblyCompilationStarted(string obj)
        {
            if (Instance.companionController?.isConnected ?? false)
            {
                Instance.companionController.ShutDownCompanionController();
            }
        }

        private void CompilationPipeline_assemblyCompilationFinished(string arg1, UnityEditor.Compilation.CompilerMessage[] arg2)
        {
            if (!Instance.companionController?.isConnected ?? false)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Instance.companionController.EnableAndConnect();
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