using UnityEngine;

namespace Gameboard.Tools
{
    public class BoardSpaceInteractor : MonoBehaviour
    {
        public void InteractorEnteredBoardSpace(BoardSpace inSpace)
        {
            Handle_EnteredBoardSpace(inSpace);
        }

        public void InteractorExitedBoardSpace(BoardSpace inSpace)
        {
            Handle_ExitedBoardSpace(inSpace);
        }

        protected virtual void Handle_EnteredBoardSpace(BoardSpace inSpace) { }
        protected virtual void Handle_ExitedBoardSpace(BoardSpace inSpace) { }
    }
}