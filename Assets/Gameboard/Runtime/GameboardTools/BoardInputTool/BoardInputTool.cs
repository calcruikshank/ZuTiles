using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameboard.Tools
{
    public class BoardInputTool : MonoBehaviour
    {
        public EventHandler<TrackedBoardObjectReference> ObjectReferenceCreated;
        public EventHandler<TrackedBoardObjectReference> ObjectReferenceUpdated;
        public EventHandler<TrackedBoardObjectReference> ObjectReferenceDeleted;

        public static BoardInputTool singleton;

        private bool setupCompleted;
        private List<TrackedBoardObjectReference> trackedObjectList = new List<TrackedBoardObjectReference>();
        private Queue<uint> deleteObjectQueue = new Queue<uint>();
        private Queue<TrackedBoardObject> createObjectQueue = new Queue<TrackedBoardObject>();
        private Queue<TrackedBoardObject> updatedObjectQueue = new Queue<TrackedBoardObject>();

        void Update()
        {
            if (!setupCompleted)
            {
                CheckForSetupReady();
                return;
            }

            ProcessCreateObjectQueue();
            ProcessUpdateObjectQueue();
            ProcessDeleteObjectQueue();
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            foreach(TrackedBoardObjectReference objectReference in trackedObjectList)
            {
                Gizmos.color = objectReference.ObjectType == DataTypes.TrackedBoardObjectTypes.Pointer ? Color.green : Color.blue;
                    Gizmos.DrawSphere(objectReference.ObjectWorldPosition, 0.75f);
                    
                    string displayString = $"    SessionID: {objectReference.SessionId}\n    Type: {objectReference.ObjectType}";
                    if(objectReference.HasTokenId)
                    { 
                        displayString += $"\n    TokenID: {objectReference.TokenId}";
                    }

                    Color cachedGuiColor = GUI.color;
                    GUI.color = Color.black;
                        Handles.Label(objectReference.ObjectWorldPosition, displayString);
                    GUI.color = cachedGuiColor;
                Gizmos.color = Color.white;

                if(objectReference.HasConvexGeometry)
                {
                    Vector3[] worldPoints = objectReference.ConvexGeometryWorldPositions;

                    Gizmos.color = Color.red;
                        for(int i = 1; i < worldPoints.Length; i++)
                        {
                            Gizmos.DrawLine(worldPoints[i - 1], worldPoints[i]);
                        }

                        Gizmos.DrawLine(worldPoints[0], worldPoints[worldPoints.Length - 1]);

                    Gizmos.color = Color.white;
                }
            }
        }
#endif

        void CheckForSetupReady()
        {
            if (Gameboard.singleton.boardTouchController != null && Gameboard.singleton.boardTouchController.boardTouchHandler != null)
            {
                Gameboard.singleton.boardTouchController.boardTouchHandler.NewBoardObjectsCreated += NewBoardObjectsCreated;
                Gameboard.singleton.boardTouchController.boardTouchHandler.BoardObjectSessionsDeleted += BoardObjectSessionsDeleted;
                Gameboard.singleton.boardTouchController.boardTouchHandler.BoardObjectsUpdated += BoardObjectsUpdated;

                singleton = this;
                setupCompleted = true;
                Debug.Log("Board Input Tool Ready!");
            }
        }

        void NewBoardObjectsCreated(object origin, List<TrackedBoardObject> newBoardObjectList)
        {
            lock (createObjectQueue)
            {
                foreach (TrackedBoardObject boardObject in newBoardObjectList)
                {
                    createObjectQueue.Enqueue(boardObject);
                }
            }
        }

        void BoardObjectsUpdated(object origin, List<TrackedBoardObject> boardObjectUpdates)
        {
            lock (updatedObjectQueue)
            {
                foreach (TrackedBoardObject boardObject in boardObjectUpdates)
                {
                    updatedObjectQueue.Enqueue(boardObject);
                }
            }
        }

        void BoardObjectSessionsDeleted(object origin, List<uint> deletedSessionIdList)
        {
            lock (deleteObjectQueue)
            {
                foreach (uint id in deletedSessionIdList)
                {
                    deleteObjectQueue.Enqueue(id);
                }
            }
        }

        private void ProcessDeleteObjectQueue()
        {
            lock(deleteObjectQueue)
            {
                lock(trackedObjectList)
                {
                    while (deleteObjectQueue.Count > 0)
                    {
                        uint deleteSessionId = deleteObjectQueue.Dequeue();
                        
                        TrackedBoardObjectReference trackedObject = trackedObjectList.Find(s => s.SessionId == deleteSessionId);

                        if (trackedObject != null)
                        {
                            ObjectReferenceDeleted?.Invoke(this, trackedObject);

                            trackedObjectList.Remove(trackedObject);
                        }
                    }
                }
            }
        }

        private void ProcessCreateObjectQueue()
        {
            lock (createObjectQueue)
            {
                lock (trackedObjectList)
                {
                    while (createObjectQueue.Count > 0)
                    {
                        TrackedBoardObject createObject = createObjectQueue.Dequeue();
                        GameboardLogging.LogMessage($"Creating Board Object with session ID {createObject.sessionId}.", GameboardLogging.MessageTypes.Verbose);

                        TrackedBoardObjectReference trackedObjectReference = new TrackedBoardObjectReference(createObject);
                        trackedObjectList.Add(trackedObjectReference);

                        ObjectReferenceCreated?.Invoke(this, trackedObjectReference);
                    }
                }
            }
        }

        private void ProcessUpdateObjectQueue()
        {
            lock (updatedObjectQueue)
            {
                lock (trackedObjectList)
                {
                    while(updatedObjectQueue.Count > 0)
                    {
                        TrackedBoardObject updatedObject = updatedObjectQueue.Dequeue();

                        TrackedBoardObjectReference trackedObject = trackedObjectList.Find(s => s.SessionId == updatedObject.sessionId);
                        if (trackedObject == null)
                        {
                            GameboardLogging.LogMessage($"Unknown session id {updatedObject.sessionId} reached BoardObjectsUpdated.", GameboardLogging.MessageTypes.Verbose);
                            continue;
                        }

                        trackedObject.UpdateWithTrackedBoardObject(updatedObject);

                        ObjectReferenceUpdated?.Invoke(this, trackedObject);
                    }
                }
            }
        }
    }
}