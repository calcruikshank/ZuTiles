using Gameboard;
using System.Collections.Generic;
using UnityEngine;

public class HandBladeTest : MonoBehaviour
{
    public GameObject displayPrefab;

    private bool setupFinished;
    private List<TrackedBoardObject> trackingBoardObjects = new List<TrackedBoardObject>();
    private Dictionary<uint, GameObject> displayObjectDict = new Dictionary<uint, GameObject>();

    void Update()
    {
        if (!setupFinished)
        {
            if (Gameboard.Gameboard.Instance != null)
            {
                setupFinished = true;
                Gameboard.Gameboard.Instance.boardTouchController.boardTouchHandler.BoardObjectsUpdated += BoardObjectsUpdated;
                Gameboard.Gameboard.Instance.boardTouchController.boardTouchHandler.BoardObjectSessionsDeleted += BoardObjectsDeleted;
            }
        }
    }

    void BoardObjectsUpdated(object origin, List<TrackedBoardObject> newBoardObjectList)
    {
        lock (trackingBoardObjects)
        {
            lock (displayObjectDict)
            {
                foreach (TrackedBoardObject newBoardObject in newBoardObjectList)
                {
                    if (newBoardObject.TrackedObjectType == DataTypes.TrackedBoardObjectTypes.Pointer && newBoardObject.PointerType == DataTypes.PointerTypes.Blade)
                    {
                        if (trackingBoardObjects.Find(s => s.sessionId == newBoardObject.sessionId) == null)
                        {
                            Debug.Log("--- Blade Added with Session ID " + newBoardObject.sessionId);
                            trackingBoardObjects.Add(newBoardObject);

                            GameObject spawnedObject = Instantiate(displayPrefab, newBoardObject.sceneWorldPosition, displayPrefab.transform.rotation);
                            displayObjectDict.Add(newBoardObject.sessionId, spawnedObject);
                        }
                        else
                        {
                            if (!displayObjectDict.ContainsKey(newBoardObject.sessionId))
                            {
                                Debug.Log("--- Blade updated but does not exist in dictionary! ID: " + newBoardObject.sessionId);
                            }
                            else
                            {
                                displayObjectDict[newBoardObject.sessionId].transform.position = newBoardObject.sceneWorldPosition;
                            }
                        }
                    }
                }
            }
        }
    }

    void BoardObjectsDeleted(object origin, List<uint> deletedSessionIdList)
    {
        lock (trackingBoardObjects)
        {
            lock (displayObjectDict)
            {
                foreach (uint thisId in deletedSessionIdList)
                {
                    TrackedBoardObject boardObject = trackingBoardObjects.Find(s => s.sessionId == thisId);
                    if (boardObject != null)
                    {
                        Debug.Log("--- Blade Removed with Session ID " + boardObject.sessionId);
                        trackingBoardObjects.Remove(boardObject);
                        
                        DestroyImmediate(displayObjectDict[thisId]);
                        displayObjectDict.Remove(thisId);
                    }
                }
            }
        }
    }
}
