using System.Collections.Generic;
using UnityEngine;

namespace Gameboard.Tools
{
    public class BoardTool : MonoBehaviour
    {
        private List<BoardSpace> boardSpaceList = new List<BoardSpace>();

        /// <summary>
        /// Creates a new board based on the rules of the entered BoardBuilder object.
        /// </summary>
        /// <param name="inBuilderObject"></param>
        public void BuildBoardWithBuilder(BoardBuilder inBuilderObject)
        {
            inBuilderObject.BuildBoard(this);
        }

        /// <summary>
        /// Adds a space to this board.
        /// </summary>
        /// <param name="inSpace"></param>
        public void AddSpaceToBoard(BoardSpace inSpace)
        {
            if(boardSpaceList.Contains(inSpace))
            {
                Debug.LogWarning($"Space {inSpace.spaceId} is already part of this Board!");
                return;
            }

            inSpace.InitializeSpace();
            boardSpaceList.Add(inSpace);
        }

        /// <summary>
        /// Removes a space from this board.
        /// </summary>
        /// <param name="inSpace"></param>
        public void RemoveSpaceFromBoard(BoardSpace inSpace)
        {
            if(!boardSpaceList.Contains(inSpace))
            {
                Debug.LogWarning($"Board cannot remove space {inSpace.spaceId} because it is not part of this board!");
                return;
            }

            foreach(BoardSpace thisSpace in inSpace.connectedSpaceList)
            {
                thisSpace.DisconnectSpace(inSpace);
            }

            inSpace.DeInitializeSpace();
            boardSpaceList.Remove(inSpace);
        }

        /// <summary>
        /// Connects to spaces together so that BoardTool can see that they can be accessed from one another.
        /// </summary>
        /// <param name="inSpaceA"></param>
        /// <param name="inSpaceB"></param>
        public void ConnectSpaces(BoardSpace inSpaceA, BoardSpace inSpaceB)
        {
            inSpaceA.ConnectSpace(inSpaceB);
            inSpaceB.ConnectSpace(inSpaceA);
        }

        /// <summary>
        /// Cycles through all spaces on the board and determines which space the inPoint is closest to the center.
        /// </summary>
        /// <param name="inWorldPoint"></param>
        /// <returns></returns>
        public BoardSpace GetClosestSpaceToPoint(Vector3 inWorldPoint)
        {
            float closestDistance = float.PositiveInfinity;
            BoardSpace closestSpace = null;
            foreach(BoardSpace space in boardSpaceList)
            {
                float thisDistance = Vector3.Distance(inWorldPoint, space.transform.position);
                if(thisDistance < closestDistance)
                {
                    closestDistance = thisDistance;
                    closestSpace = space;
                }
            }

            return closestSpace;
        }
    }
}