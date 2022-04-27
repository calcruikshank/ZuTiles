using Gameboard.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    public class GameboardTouchController : IGameboardTouchController
    {
        public IBoardCommunicationsUtility boardCommunications { get; private set; }

        public IGameboardTouchHandlerUtility boardTouchHandler { get; private set; }

        public void InjectDependencies(IBoardCommunicationsUtility inBoardComs, IGameboardTouchHandlerUtility inBoardTouchHandler)
        {
            boardCommunications = inBoardComs;
            boardTouchHandler = inBoardTouchHandler;
        }

        public void EnableGameboard()
        {
            boardCommunications.ConnectToGameboard();
        }

        public void DisableGameboard()
        {
            boardCommunications.DisconnectFromGameboard();
        }
    }
}