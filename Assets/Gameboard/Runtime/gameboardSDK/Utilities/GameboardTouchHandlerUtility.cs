using Gameboard.TUIO;
using System.Collections.Generic;
using UnityEngine;
using System;
using Gameboard.Helpers;

namespace Gameboard.Utilities
{
    public class GameboardTouchHandlerUtility : GameboardUtility, IGameboardTouchHandlerUtility
    {
        public event EventHandler<List<TrackedBoardObject>> NewBoardObjectsCreated;
        public event EventHandler<List<uint>> BoardObjectSessionsDeleted;
        public event EventHandler<List<TrackedBoardObject>> BoardObjectsUpdated;

        private GameboardCommunicationUtility boardCommunications { get; }
        private GameboardConfig boardConfig;
        private Dictionary<uint, TrackedBoardObject> trackedObjects = new Dictionary<uint, TrackedBoardObject>();
        private List<TrackedBoardObject> objectsUpdatedSinceLastUpdate = new List<TrackedBoardObject>();
        private List<TrackedBoardObject> newBoardObjectsSinceLastUpdate = new List<TrackedBoardObject>();
        private List<TrackedBoardObject> updatedBoardObjects = new List<TrackedBoardObject>();
        private List<uint> deletedSessionIdList = new List<uint>();
        private Stack<uint> deleteOnNextUpdateStack = new Stack<uint>();

        public GameboardTouchHandlerUtility(GameboardCommunicationUtility inBoardCommunicationsUtility, GameboardConfig inBoardConfig)
        {
            boardCommunications = inBoardCommunicationsUtility;
            boardConfig = inBoardConfig;

            boardCommunications.TrackedBoardObjectCreated += TrackedBoardObjectCreated;
            boardCommunications.TrackedBoardObjectDeleted += TrackedBoardObjectDeleted;
            boardCommunications.TrackedBoardObjectUpdated += TrackedBoardObjectUpdated;
        }

        ~GameboardTouchHandlerUtility()
        {
            boardCommunications.TrackedBoardObjectCreated -= TrackedBoardObjectCreated;
            boardCommunications.TrackedBoardObjectDeleted -= TrackedBoardObjectDeleted;
            boardCommunications.TrackedBoardObjectUpdated -= TrackedBoardObjectUpdated;
        }

        public override void ProcessUpdate()
        {
            HandleObjectDeletions();

            lock (objectsUpdatedSinceLastUpdate)
            {
                lock (newBoardObjectsSinceLastUpdate)
                {
                    lock (deleteOnNextUpdateStack)
                    {
                        updatedBoardObjects.Clear();

                        NewBoardObjectsCreated?.Invoke(this, newBoardObjectsSinceLastUpdate);
                        newBoardObjectsSinceLastUpdate.Clear();

                        for (int i = 0; i < objectsUpdatedSinceLastUpdate.Count; i++)
                        {
                            TrackedBoardObject trackedObject = objectsUpdatedSinceLastUpdate[i];

                            if(trackedObject == null)
                            {
                                GameboardLogging.LogMessage($"TrackedObject value was null in TouchHandler process update.", GameboardLogging.MessageTypes.Error);
                                continue;
                            }

                            HandleGameboardScreenPositionUpdates(trackedObject);
                            HandleUnityScenePositionUpdates(trackedObject);
                            //HandleObjectCountourUpdates(trackedObject);

                            updatedBoardObjects.Add(trackedObject);
                        }

                        BoardObjectsUpdated?.Invoke(this, updatedBoardObjects);

                        objectsUpdatedSinceLastUpdate.Clear();
                    }
                }
            }
        }

        private void HandleObjectDeletions()
        {
            deletedSessionIdList.Clear();

            lock (deleteOnNextUpdateStack)
            {
                lock (objectsUpdatedSinceLastUpdate)
                {
                    lock (newBoardObjectsSinceLastUpdate)
                    {
                        lock (trackedObjects)
                        {
                            int stackCount = deleteOnNextUpdateStack.Count;
                            for (int i = 0; i < stackCount; i++)
                            {
                                uint deleteId = deleteOnNextUpdateStack.Pop();
                                if (!trackedObjects.ContainsKey(deleteId))
                                {
                                    GameboardLogging.LogMessage($"HandleObjectDeletions attempted to delete object with Session ID {deleteId} however this object does not exist.", GameboardLogging.MessageTypes.Error);
                                }
                                else
                                {
                                    if (objectsUpdatedSinceLastUpdate.Contains(trackedObjects[deleteId]))
                                    {
                                        objectsUpdatedSinceLastUpdate.Remove(trackedObjects[deleteId]);
                                    }

                                    if (newBoardObjectsSinceLastUpdate.Contains(trackedObjects[deleteId]))
                                    {
                                        newBoardObjectsSinceLastUpdate.Remove(trackedObjects[deleteId]);
                                    }

                                    trackedObjects.Remove(deleteId);
                                    GameboardLogging.LogMessage("-- REMOVING from trackedObjects: " + deleteId, GameboardLogging.MessageTypes.Verbose);

                                    if (!deletedSessionIdList.Contains(deleteId))
                                    {
                                        deletedSessionIdList.Add(deleteId);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            BoardObjectSessionsDeleted?.Invoke(this, deletedSessionIdList);
        }

        private void HandleGameboardScreenPositionUpdates(TrackedBoardObject inBoardObject)
        {
            if (inBoardObject?.tuio != null)
            {
                //Get the x/y pos of the tracked object, regardless if it's a PTR or TOK.
                if (inBoardObject.tuio.HasTokMessage())
                {
                    TokMessage tok = inBoardObject.tuio.tok;
                    inBoardObject.gameboardScreenPosition.x = tok.x_pos;
                    inBoardObject.gameboardScreenPosition.y = tok.y_pos;
                }
                else if (inBoardObject.tuio.HasPtrMessage())
                {
                    PtrMessage ptr = inBoardObject.tuio.ptr;
                    inBoardObject.gameboardScreenPosition.x = ptr.x_pos;
                    inBoardObject.gameboardScreenPosition.y = ptr.y_pos;
                }
            }
        }

        private void HandleUnityScenePositionUpdates(TrackedBoardObject inBoardObject)
        {
            try
            {
                inBoardObject.sceneWorldPosition = GameboardHelperMethods.GameboardScreenPointToScenePoint(Camera.main, inBoardObject.gameboardScreenPosition);
            }
            catch(Exception e)
            {
                GameboardLogging.LogMessage($"HandleUnityScenePositionUpdates failed with exception {e.Message}", GameboardLogging.MessageTypes.Error);
            }
        }

        private void HandleObjectCountourUpdates(TrackedBoardObject inBoardObject)
        {
            if(!inBoardObject.tuio.HasChgMessage())
            {
                return;
            }    
            
            if (inBoardObject.tuio.chg.GetType() == typeof(ChgMessage))
            {
                ChgMessage chg = inBoardObject.tuio.chg;

                var contour = chg.contour;
                if(inBoardObject.contourWorldVectors2D?.Length != contour.Length)
                {
                    inBoardObject.contourWorldVectors2D = new Vector2[contour.Length];
                }

                if (inBoardObject.contourWorldVectors3D?.Length != contour.Length)
                {
                    inBoardObject.contourWorldVectors3D = new Vector3[contour.Length];
                }

                if (inBoardObject.contourLocalVectors3D?.Length != contour.Length)
                {
                    inBoardObject.contourLocalVectors3D = new Vector3[contour.Length];
                }

               /* for (int j = 0; j < contour.Length; j++)
                {
                    //TODO: dynamically figure out resolution, but in our case the GB-1 is always 1920x1920
                    try
                    {
                        if(contour[j] == null)
                        {
                            Debug.LogError("Contour index " + j + " was null!");
                            return;
                        }

                        inBoardObject.contourWorldVectors3D[j] = Camera.main.ScreenToWorldPoint(new Vector3(contour[j].Item1 * boardConfig.deviceResolution.x,
                                                                                                           (1 - contour[j].Item2) * boardConfig.deviceResolution.y,
                                                                                                           Camera.main.transform.position.y));

                        inBoardObject.contourLocalVectors3D[j] = InverseTransformPoint(inBoardObject.sceneWorldPosition, Quaternion.identity, inBoardObject.contourWorldVectors3D[j]);

                        inBoardObject.contourWorldVectors2D[j] = new Vector2(inBoardObject.contourWorldVectors3D[j].x, inBoardObject.contourWorldVectors3D[j].z);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Camera.Main is null: " + Camera.main);
                        Debug.LogError("inBoardObject is null: " + inBoardObject);
                        Debug.LogError("inBoardObject.sceneWorldPosition is null: " + inBoardObject.sceneWorldPosition);

                        GameboardLogging.LogMessage($"HandleObjectCountourUpdates failed to acquire contours with exception {e.Message} / {e.InnerException}", GameboardLogging.MessageTypes.Error);
                    }
                }*/
            }
        }

        private Vector3 InverseTransformPoint(Vector3 transformPosition, Quaternion inRotation, Vector3 worldPosition)
        {
            Matrix4x4 matrix = Matrix4x4.TRS(transformPosition, inRotation, Vector3.one);
            Matrix4x4 inverse = matrix.inverse;
            return inverse.MultiplyPoint3x4(worldPosition);
        }

        private void TrackedBoardObjectCreated(object sender, ObjectCreateEventArgs createEventArg)
        {
            lock (newBoardObjectsSinceLastUpdate)
            {
                lock (deleteOnNextUpdateStack)
                {
                    if (deleteOnNextUpdateStack.Contains(createEventArg.s_id))
                    {
                        Stack<uint> cloneDeleteStack = new Stack<uint>(deleteOnNextUpdateStack);
                        deleteOnNextUpdateStack.Clear();
                        foreach (uint thisUint in cloneDeleteStack)
                        {
                            if (thisUint != createEventArg.s_id)
                            {
                                deleteOnNextUpdateStack.Push(thisUint);
                            }
                        }
                    }
                }

                if (!trackedObjects.ContainsKey(createEventArg.s_id))
                {
                    trackedObjects.Add(createEventArg.s_id, new TrackedBoardObject(createEventArg.s_id));
                    newBoardObjectsSinceLastUpdate.Add(trackedObjects[createEventArg.s_id]);
                }
            }
        }

        private void TrackedBoardObjectDeleted(object sender, ObjectDeleteEventArgs deleteEventArg)
        {
            deleteOnNextUpdateStack.Push(deleteEventArg.s_id);
        }

        private void TrackedBoardObjectUpdated(object sender, ObjectUpdateEventArgs updateEventArg)
        {
            lock (objectsUpdatedSinceLastUpdate)
            {
                if (trackedObjects.ContainsKey(updateEventArg.s_id))
                {
                    TrackedBoardObject boardObject = trackedObjects[updateEventArg.s_id];

                    if (!objectsUpdatedSinceLastUpdate.Contains(boardObject))
                    {
                        boardObject.UpdateFromEventArgs(updateEventArg);
                        if (boardObject.AnyUpdatesAvailable)
                        {
                            if (updateEventArg.UpdatedTok)
                            {
                                GameboardLogging.LogMessage($"Token Object Updated: ID {updateEventArg.s_id} / Token Id {updateEventArg.obj.tok.tu_id}", GameboardLogging.MessageTypes.Verbose);
                            }
                            else if (updateEventArg.UpdatedPtr)
                            {
                                GameboardLogging.LogMessage($"Pointer Object Updated: ID {updateEventArg.s_id} / Pointer Type {(DataTypes.PointerTypes)updateEventArg.obj.ptr.tu_id}", GameboardLogging.MessageTypes.Verbose);
                            }

                            objectsUpdatedSinceLastUpdate.Add(boardObject);
                        }
                    }
                }
                else
                {
                    GameboardLogging.LogMessage($"TrackedBoardObjectUpdated attempted to update object with Session ID {updateEventArg.s_id} however this object does not exist.", GameboardLogging.MessageTypes.Error);
                }
            }
        }
    }
}