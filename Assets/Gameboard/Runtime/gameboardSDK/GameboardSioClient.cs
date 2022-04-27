using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Gameboard
{
    public class GameboardSioClient
    {
#if UNITY_IOS || UNITY_TVOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    private const string dllName = "__Internal";
#elif UNITY_ANDROID
    private const string dllName = "sioclient";
#else
    private const string dllName = "sioclient";
#endif

#region C++ Function Calls
        [DllImport(dllName)]
        private static extern void ConnectClient(string uri);

        [DllImport(dllName)]
        private static extern void ConnectClientWithBoardId(string uri, string boardId);

        [DllImport(dllName)]
        private static extern void DisconnectClient();

        [DllImport(dllName)]
        private static extern void InitClient(string uriNamespace);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterForConnectedCallback(SioConnectedCallback callback);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterForConnectionFailedCallback(SioConnectedCallback callback);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterForClosedCallback(SioConnectedCallback callback);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterForMessageCallback(SioCallback callback);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterForAckReceivedCallback(SioAckCallback callback);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void RegisterForPrintMessageCallback(SioCallback callback);

        [DllImport(dllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeregisterAllCallbacks();

        [DllImport(dllName)]
        private static extern void EmitMessage(string functionName, string inEventId, string jsonPayload, IntPtr arrayPtr, int length);
#endregion

        public delegate void SioCallback(string message);
        SioCallback sioMessageCallback; 
        SioCallback sioPrintMessageCallback;

        public delegate void SioAckCallback(string message, string eventId);
        SioAckCallback sioAckReceivedCallback;

        public delegate void SioConnectedCallback();
        SioConnectedCallback sioConnectedCallback;
        SioConnectedCallback sioConnectionFailedCallback;
        SioConnectedCallback sioConnectionClosedCallback;

        public EventHandler connectedToServer;
        public EventHandler<string> messageReceived;
        public EventHandler<string> printMessageReceived;
        public EventHandler<ServerMessageCompletionObject> ackResponseReceived;

        public bool wasConnected { get; private set; }
        private string lastUriTried;
        private string lastBoardIdTried;

        private static GameboardSioClient singleton;

        public void SetupSioClient(string inUriNamespace)
        {
            GameboardLogging.LogMessage("--- SettingUp SIO Client", GameboardLogging.MessageTypes.Log);

            if(singleton != null)
            {
                singleton = null;
            }

            singleton = this;

            RegisterAllCallbacks();
            InitClient(inUriNamespace);
        }

        public void ConnectToServer(string inUri, string inBoardId)
        {
            lastUriTried = inUri;
            lastBoardIdTried = inBoardId;

            GameboardLogging.LogMessage($"--- SIO Connect URI: {lastUriTried} / BoardID: {inBoardId}", GameboardLogging.MessageTypes.Log);

            ConnectClientWithBoardId(inUri, inBoardId);
        }

        private void RegisterAllCallbacks()
        {
            sioConnectedCallback += OnSioConnectedCallback;
            sioConnectionClosedCallback += OnSioConnectionClosedCallback;
            sioConnectionFailedCallback += OnSioConnectionFailedCallback;
            sioMessageCallback += OnSioMessageCallback;
            sioAckReceivedCallback += OnSioAckReceivedCallback;
            sioPrintMessageCallback += OnSioPrintMessageCallback;

            RegisterForConnectedCallback(sioConnectedCallback);
            RegisterForClosedCallback(sioConnectionClosedCallback);
            RegisterForConnectionFailedCallback(sioConnectionFailedCallback);
            RegisterForMessageCallback(sioMessageCallback);
            RegisterForAckReceivedCallback(sioAckReceivedCallback);
            RegisterForPrintMessageCallback(sioPrintMessageCallback);
        }

        private void UnregisterAllCallbacks()
        {
            DeregisterAllCallbacks();

            singleton.sioConnectedCallback -= OnSioConnectedCallback;
            singleton.sioConnectionClosedCallback -= OnSioConnectionClosedCallback;
            singleton.sioConnectionFailedCallback -= OnSioConnectionFailedCallback;
            singleton.sioMessageCallback -= OnSioMessageCallback;
            singleton.sioAckReceivedCallback -= OnSioAckReceivedCallback;
            singleton.sioPrintMessageCallback -= OnSioPrintMessageCallback;
        }

        public void DisconnectFromServer()
        {
            DisconnectClient();
        }

        public void SendMessage(string functionName, string inEventId, string jsonPayload, byte[] byteArray = null)
        {
#if SIODEBUG
            GameboardLogging.LogMessage($"--- SIO: Send Message - FunctionName: {functionName} / {jsonPayload}", GameboardLogging.MessageTypes.Log);
#endif

            unsafe
            {
                if (byteArray != null)
                {
                    fixed (byte* pointer = byteArray)
                    {
                        EmitMessage(functionName, inEventId, jsonPayload, (IntPtr)pointer, byteArray.Length);
                    }
                }
                else
                {
                    EmitMessage(functionName, inEventId, jsonPayload, IntPtr.Zero, 0);
                }
            }
        }

        [MonoPInvokeCallback(typeof(SioConnectedCallback))]
        private static void OnSioConnectedCallback()
        {
            GameboardLogging.LogMessage("--- SIO Client Connected!", GameboardLogging.MessageTypes.Log);
            if (singleton.wasConnected)
            {
                return;
            }

            singleton.wasConnected = true;

            singleton.connectedToServer?.Invoke(singleton, null);
        }

        [MonoPInvokeCallback(typeof(SioConnectedCallback))]
        private static void OnSioConnectionFailedCallback()
        {
            GameboardLogging.LogMessage("--- SIO CONNECTION FAILED", GameboardLogging.MessageTypes.Error);

            if (!string.IsNullOrEmpty(singleton.lastUriTried))
            {
                ConnectClientWithBoardId(singleton.lastUriTried, singleton.lastBoardIdTried);
            }
        }

        [MonoPInvokeCallback(typeof(SioConnectedCallback))]
        private static void OnSioConnectionClosedCallback()
        {
            singleton.wasConnected = false;

            singleton.UnregisterAllCallbacks();

            GameboardLogging.LogMessage("--- SIO CONNECTION CLOSED", GameboardLogging.MessageTypes.Log);
        }

        [MonoPInvokeCallback(typeof(SioCallback))] 
        public static void OnSioMessageCallback(string inMessage)
        {
#if SIODEBUG
            GameboardLogging.LogMessage("--- SIO MESSAGE RECEIVED: " + inMessage, GameboardLogging.MessageTypes.Log);
#endif

            singleton.messageReceived?.Invoke(singleton, inMessage);
        }

        [MonoPInvokeCallback(typeof(SioAckCallback))]
        private static void OnSioAckReceivedCallback(string inMessage, string inEventId)
        {
#if SIODEBUG
            GameboardLogging.LogMessage("=== --- ACK RESPONSE RECEIVED for eventId " + inEventId + ": " + inMessage, GameboardLogging.MessageTypes.Log);
#endif

            ServerMessageCompletionObject completionObject = new ServerMessageCompletionObject()
            {
                message = inMessage,
                eventId = inEventId
            };

            singleton.ackResponseReceived?.Invoke(singleton, completionObject);
        }

        [MonoPInvokeCallback(typeof(SioCallback))]
        private static void OnSioPrintMessageCallback(string inMessage)
        {
            GameboardLogging.LogMessage("SIO Print Message: " + inMessage, GameboardLogging.MessageTypes.Log);
        }
    }
}