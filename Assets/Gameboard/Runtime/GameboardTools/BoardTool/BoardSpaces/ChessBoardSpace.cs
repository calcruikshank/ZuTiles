using UnityEngine;

namespace Gameboard.Tools
{
    public class ChessBoardSpace : BoardSpace
    {
        public int xPosition { get; private set; }
        public int yPosition { get; private set; }
        
        public void SetBoardPosition(int inX, int inY)
        {
            xPosition = inX;
            yPosition = inY;
        }

        protected override void ObjectEnteredSpace(BoardSpaceInteractor spaceInteractor)
        {
            //Debug.Log($"Piece Entered Space {xPosition}/{yPosition}: {spaceInteractor.gameObject.name}");
        }

        protected override void ObjectExitedSpace(BoardSpaceInteractor spaceInteractor)
        {
            //Debug.Log($"Piece Exited Space {xPosition}/{yPosition}: {spaceInteractor.gameObject.name}");
        }
    }
}