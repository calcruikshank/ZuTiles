using Gameboard.EventArgs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameboard
{
    public interface ICompanionCommunicationsUtility
    {
        event EventHandler<string> CompanionMessageReceived;
        event EventHandler<ServerMessageCompletionObject> CompanionAckReceived;

        bool isConnected { get; }

        Task<bool> AsyncConnectToCompanionServer();
        void DisconnectFromCompanionServer();
        void SendMessageToCompanionServer(string inEvent, string inEventId, string inJson = null, byte[] inByteArray = null);
    }
}