using System.Collections;
using System.Collections.Generic;
using Gameboard.Utilities;

namespace Gameboard
{
    public interface IGameboardServiceManager
    {
        GameboardTouchHandlerUtility gameBoardHandler { get; }
        CompanionCommunicationsUtility companionCommunications { get; }
        CompanionHandlerUtility companionHandler { get; }        
        TouchUtility touchUtility { get; }
        CompanionHandlerUtility companionUtility { get; }
        GameboardCommunicationUtility gameBoardCommunicationsUtility { get; }
        JsonUtility jsonUtility { get; }

        void PerformUtilityUpdate();
        void PerformUtilityLateUpdate();
    }
}