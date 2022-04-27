using UnityEngine;
using Gameboard.TUIO;
using OSCsharp.Utils;
using System;

namespace Gameboard.Utilities
{
    public class GameboardCommunicationUtility : GameboardUtility, IBoardCommunicationsUtility
    {
        private TUIOClient client { get; }
        public bool IsConnected { get { return client.IsConnected; } }

        // NOTE: These should really be part of IBoardCommunicationsUtility, but that would require a minor refactor. Will come back
        //       to that later.
        public event EventHandler<ObjectCreateEventArgs> TrackedBoardObjectCreated;
        public event EventHandler<ObjectDeleteEventArgs> TrackedBoardObjectDeleted;
        public event EventHandler<ObjectUpdateEventArgs> TrackedBoardObjectUpdated;
        public event EventHandler<DebugHeatmapMessage> DebugHeatMapUpdated;

        public GameboardCommunicationUtility(IGameboardConfig gameboardConfig, IJsonUtility jsonUtility)
        {
            client = new TUIOClient(gameboardConfig.gameboardConnectPort, jsonUtility);
            
            client.OnErrorOccured += BoardCommunicationErrorOccured;
            client.OnObjectCreate += handleTrackedBoardObjectCreate;
            client.OnObjectDelete += handleTrackedBoardObjectDelete;
            client.OnObjectUpdate += handleTrackedBoardObjectUpdate;
            client.OnDebugHeatMapUpdated += handleDebugHeatMapUpdated;
        }

        ~GameboardCommunicationUtility()
        {
            client.OnErrorOccured -= BoardCommunicationErrorOccured;
            client.OnObjectCreate -= handleTrackedBoardObjectCreate;
            client.OnObjectDelete -= handleTrackedBoardObjectDelete;
            client.OnObjectUpdate -= handleTrackedBoardObjectUpdate;
            client.OnDebugHeatMapUpdated -= handleDebugHeatMapUpdated;
        }

        public void ConnectToGameboard()
        {
            client.Connect();
        }

        public void DisconnectFromGameboard()
        {
            client.Disconnect();
        }

        #region Event Listeners
        void BoardCommunicationErrorOccured(object sender, ExceptionEventArgs eventArgs)
        {
            Debug.LogError($"Gameboard Communications Error! Message: {eventArgs.Exception.Message} | InnerException: {eventArgs.Exception.InnerException} | Stacktrace: {eventArgs.Exception.StackTrace} | Source: {eventArgs.Exception.Source}");
        }

        private void handleTrackedBoardObjectCreate(object sender, ObjectCreateEventArgs eventArgs)
        {
            TrackedBoardObjectCreated?.Invoke(this, eventArgs);
        }

        private void handleTrackedBoardObjectDelete(object sender, ObjectDeleteEventArgs eventArgs)
        {
            TrackedBoardObjectDeleted?.Invoke(this, eventArgs);
        }

        private void handleTrackedBoardObjectUpdate(object sender, ObjectUpdateEventArgs eventArgs)
        {
            TrackedBoardObjectUpdated?.Invoke(this, eventArgs);
        }

        private void handleDebugHeatMapUpdated(object sender, DebugHeatmapMessage heatMapArgs)
        {
            DebugHeatMapUpdated?.Invoke(this, heatMapArgs);
        }
        #endregion
    }
}