using Gameboard.TUIO;
using System;

namespace Gameboard.Utilities
{
    public interface IBoardCommunicationsUtility
    {
        bool IsConnected { get; }
        void ConnectToGameboard();
        void DisconnectFromGameboard();

        event EventHandler<DebugHeatmapMessage> DebugHeatMapUpdated;
    }
}