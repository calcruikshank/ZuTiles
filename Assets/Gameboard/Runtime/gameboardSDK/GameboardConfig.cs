using System;
using UnityEngine;

namespace Gameboard
{
    public class GameboardConfig : IGameboardConfig
    {
        public Version SDKVersion { get; }
        public int gameboardConnectPort { get; }
        public string companionServerUri { get; }
        public string companionServerUriNamespace { get; }
        public DataTypes.OperatingMode operatingMode { get; }
        public Vector2 deviceResolution { get; }

        public string gameboardId { get; }
        public string accountEmail { get; private set; }

        /// <summary>
        /// How long events will wait before timing out, in Milliseconds
        /// </summary>
        public int eventTimeoutLength { get; }

        /// <summary>
        /// How many times an event will be retried after timing out from eventTimeoutLength before being considered a fatal failure.
        /// </summary>
        public int eventRetryCount { get; }

        public AndroidJavaClass mGameboardSettings { get; }
        public AndroidJavaClass drawerHelper { get; }
        
        public AndroidApplicationContext androidApplicationContext { get; }

        public GameboardConfig(string mockGameboardId = "")
        {
            SDKVersion = new Version(0, 0, 1067);
            gameboardConnectPort = 3333;
            deviceResolution = new Vector2(1920, 1920); //TODO: dynamically figure out resolution, but in our case the GB-1 is always 1920x1920         

            androidApplicationContext = new AndroidApplicationContext();

#if UNITY_ANDROID
            mGameboardSettings = new AndroidJavaClass("com.lastgameboard.gameboard_settings_sdk.GameboardSettings");
            drawerHelper = new AndroidJavaClass("com.lastgameboard.gameboardservice.client.drawer.DrawerHelper");
#endif

            companionServerUri = GetBoardServiceWebSocketUrl(mGameboardSettings);
            companionServerUriNamespace = "";
            operatingMode = DataTypes.OperatingMode.Production;

            gameboardId = (Application.platform == RuntimePlatform.Android) ? GetBoardId(mGameboardSettings) : mockGameboardId;
            GameboardLogging.Verbose("BOARD ID: " + gameboardId);

#if UNITY_ANDROID
            accountEmail = GetGameboardAccountEmail(mGameboardSettings);
#endif

            eventTimeoutLength = 3000;
            eventRetryCount = 5;
        }

        /// <summary>
        /// Retrives the board ID for the gameboard this game is running on. 
        /// This API is only available while running on the hardware so if 
        /// running on the editor no Board ID will be obtained.
        /// </summary>
        private string GetBoardId(AndroidJavaClass inSettings)
        {
#if UNITY_ANDROID
            return inSettings.CallStatic<string>("getBoardId", androidApplicationContext.GetNativeContext());
#else
            Debug.Log("Cannot acquire BoardID because you are not running on the Gameboard device.");
            return "NullID";
#endif
        }

        /// <summary>
        /// This method returns the WebSocket URL as configured in the board.
        /// This is a full URL including the protocol and port. i.e. wss://api.lastgameboard.com:443
        /// In the event the board is configured to use the service running local on the board 
        /// that address might be of the form: ws://localhost:8000
        /// </summary>
        private string GetBoardServiceWebSocketUrl(AndroidJavaClass inSettings)
        {
            string defaultURL = "ws://api.lastgameboard.com";

#if UNITY_EDITOR
            return defaultURL;
#elif UNITY_ANDROID
            try
            {
                return inSettings.CallStatic<string>("getBoardServiceWebSocketUrl", androidApplicationContext.GetNativeContext());
            }
            catch(Exception e)
            {
                Debug.Log($"Cannot acquire Board Service URL due to the exception {e.Message}. Using default URL of {defaultURL}.");
                return defaultURL;
            }
#else
            Debug.Log($"Cannot acquire Board Service URL because you are not running on the Gameboard device. Using default URL of {defaultURL}.");
            return defaultURL;
#endif
        }
        
        /// <summary>
        /// Sends a play event to be tracked
        /// </summary>
        /// <param name="extras">Json string with extra data to include in the event</param>
        public static void SendPlayEvent(AndroidApplicationContext context, string extras) {
            AndroidJavaClass gameboardlytics = new AndroidJavaClass("com.lastgameboard.gameboardlytics.GameboardlyticsHelper");
            gameboardlytics.CallStatic("sendPlayEvent", context.GetNativeContext(), extras);   
        }

        /// <summary>
        /// Get the email address for a linked account if one exists.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private string GetGameboardAccountEmail(AndroidJavaClass inSettings)
        {
#if UNITY_ANDROID
            try
            {
                AndroidJavaClass mGameboardSettings = new AndroidJavaClass("com.lastgameboard.gameboard_settings_sdk.GameboardSettings");
                string email = inSettings.CallStatic<string>("getLinkedAccountEmail", androidApplicationContext.GetNativeContext());
                return email;
            }
            catch (Exception e)
            {
                GameboardLogging.Error($"Failed to retrieve linked account email, exception: {e.Message}");
                return null;
            }
#else
            GameboardLogging.Verbose($"Not using android, skipping getting linked gameboard account email.");
            return null;
#endif
        }
    }
}
