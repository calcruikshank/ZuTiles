using System;
using System.Collections.Generic;

namespace Gameboard.Utilities
{
    public interface IGameboardTouchHandlerUtility
    {
        event EventHandler<List<TrackedBoardObject>> NewBoardObjectsCreated;
        event EventHandler<List<uint>> BoardObjectSessionsDeleted;
        event EventHandler<List<TrackedBoardObject>> BoardObjectsUpdated;
    }
}