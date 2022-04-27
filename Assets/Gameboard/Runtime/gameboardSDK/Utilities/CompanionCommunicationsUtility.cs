using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Gameboard.Utilities
{
    public class CompanionCommunicationsUtility : GameboardUtility, ICompanionCommunicationsUtility
    {
        public event EventHandler<string> CompanionMessageReceived;
        public event EventHandler<ServerMessageCompletionObject> CompanionAckReceived;

        private IGameboardConfig gameboardConfig;

        public bool isConnected { get { return sioClientWrapper == null ? false : sioClientWrapper.wasConnected; } }
        public bool isReady { get; }

        private bool isWaitingToConnect;

        private GameboardSioClient sioClientWrapper;

        ~CompanionCommunicationsUtility()
        {
            sioClientWrapper = null;
        }

        public CompanionCommunicationsUtility(IGameboardConfig inConfig)
        {
            gameboardConfig = inConfig;

            sioClientWrapper = new GameboardSioClient();
            sioClientWrapper.SetupSioClient(inConfig.companionServerUriNamespace);

            sioClientWrapper.messageReceived += CompanionServerMessageReceived;
            sioClientWrapper.ackResponseReceived += AckResponseReceived;

            Debug.Log("Opening Socket.IO with address " + gameboardConfig.companionServerUri);
            
            isReady = true; 
        }

        #region Interface Methods
        TaskCompletionSource<string> messageTaskCompletion = null;
        public async Task<bool> AsyncConnectToCompanionServer()
        {
            if(isWaitingToConnect)
            {
                return false;
            }

            if(!isReady)
            {
                Debug.LogError("Unable to connect to CompanionServer because the socket is not ready!");
                return false;
            }

            isWaitingToConnect = true;

            messageTaskCompletion = new TaskCompletionSource<string>();
            
            sioClientWrapper.connectedToServer += ConnectedToServer;
            sioClientWrapper.ConnectToServer(gameboardConfig.companionServerUri, gameboardConfig.gameboardId);

            await messageTaskCompletion.Task;
            sioClientWrapper.connectedToServer -= ConnectedToServer;  

            messageTaskCompletion = null;
            
            return true;
        }

        void ConnectedToServer(object origin, object unused)
        {
            GameboardLogging.LogMessage("Connected to Companion Service.", GameboardLogging.MessageTypes.Log);

            isWaitingToConnect = false;

            messageTaskCompletion.SetResult("Finished");
        }

        public void DisconnectFromCompanionServer()
        {
            sioClientWrapper?.DisconnectFromServer();
        }

        void CompanionServerMessageReceived(object sender, string inMessage)
        {
            //Debug.Log("--- CompanionServerMessageReceived: " + inMessage);
            CompanionMessageReceived?.Invoke(this, inMessage);
        }

        public void SendMessageToCompanionServer(string inEvent, string inEventId, string inJson = null, byte[] inByteArray = null)
        {
            sioClientWrapper.SendMessage(inEvent, inEventId, inJson ?? "", inByteArray);
        }

        void AckResponseReceived(object origin, ServerMessageCompletionObject completionObject)
        {
            CompanionAckReceived?.Invoke(this, completionObject);
        }
        #endregion
    }
}