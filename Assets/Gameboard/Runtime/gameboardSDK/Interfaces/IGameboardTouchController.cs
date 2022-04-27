using Gameboard.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    public interface IGameboardTouchController
    {
        IBoardCommunicationsUtility boardCommunications { get; }
        IGameboardTouchHandlerUtility boardTouchHandler { get;  }
    }
}