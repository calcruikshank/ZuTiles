using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard.Tools
{
    public class ChessBoardBuilder : BoardBuilder
    {
        // Const hashtable keys ensure they don't get confused.
        public const string HashKey_boardXSize = "boardXSize";
        public const string HashKey_boardYSize = "boardYSize";
        public const string HashKey_spaceWorldSize = "spaceWorldSize";

        private int boardXSize;
        private int boardYSize;
        private float spaceWorldSize;
        private BoardSpace lightSpacePrefab;
        private BoardSpace darkSpacePrefab;

        ChessBoardSpace[,] boardSpaceArray;

        public ChessBoardBuilder(List<BoardSpace> spacePrefabList, Hashtable inTable) : base(spacePrefabList, inTable)
        {
            boardXSize = (int)inTable[HashKey_boardXSize];
            boardYSize = (int)inTable[HashKey_boardYSize];
            spaceWorldSize = (float)inTable[HashKey_spaceWorldSize];
            lightSpacePrefab = spacePrefabList[0];
            darkSpacePrefab = spacePrefabList[1];
        }

        public ChessBoardSpace GetSpaceAtPosition(int inX, int inY)
        {
            return boardSpaceArray[inX, inY];
        }

        public override void BuildBoard(BoardTool inBoardTool)
        {
            bool startWithLightSpace = true;
            bool generateLightSpace = true;
            Vector3 currentWorldPosition = inBoardTool.transform.position;

            boardSpaceArray = new ChessBoardSpace[boardXSize, boardYSize];

            // First, build the board
            for (int y = 0; y < boardYSize; y++)
            {
                for (int x = 0; x < boardXSize; x++)
                {
                    GameObject prefabObject = generateLightSpace ? lightSpacePrefab.gameObject : darkSpacePrefab.gameObject;
                    GameObject generatedSpace = GameObject.Instantiate(prefabObject, currentWorldPosition, prefabObject.transform.rotation);

                    ChessBoardSpace chessSpace = generatedSpace.GetComponent<ChessBoardSpace>();
                    chessSpace.SetBoardPosition(x, y);

                    inBoardTool.AddSpaceToBoard(chessSpace);

                    generateLightSpace = !generateLightSpace;
                    currentWorldPosition.x += spaceWorldSize;

                    boardSpaceArray[x, y] = chessSpace;
                }

                startWithLightSpace = !startWithLightSpace;
                generateLightSpace = startWithLightSpace;

                currentWorldPosition.x = inBoardTool.transform.position.x;
                currentWorldPosition.z -= spaceWorldSize;
            }

            // Now attach the spaces to each other
            for (int y = 0; y < boardYSize; y++)
            {
                for (int x = 0; x < boardXSize; x++)
                {
                    BoardSpace centerSpace = boardSpaceArray[x, y];

                    if (y > 0)
                    {
                        BoardSpace aboveSpace = boardSpaceArray[x, y - 1];
                        inBoardTool.ConnectSpaces(centerSpace, aboveSpace);
                    }

                    if (x > 0)
                    {
                        BoardSpace leftSpace = boardSpaceArray[x - 1, y];
                        inBoardTool.ConnectSpaces(centerSpace, leftSpace);
                    }                   
                }
            }
        }
    }
}