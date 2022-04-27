using Gameboard.TUIO;
using UnityEngine;

namespace Gameboard
{
    public class TrackedBoardObject
    {
        public TrackedBoardObject(uint inObjectId)
        {
            sessionId = inObjectId;
        }

        public void UpdateFromEventArgs(ObjectUpdateEventArgs inUpdateArgs)
        {
            if (inUpdateArgs.s_id != sessionId)
            {
                Debug.LogError("TUIO Session ID Mismatch!");
                return;
            }

            lastUpdateArgs = inUpdateArgs;
        }

        #region Properties
        /// <summary>
        /// Each object receives a 'SessionID', which refers to the Session of that individual object, and is unique to that session for that object.
        /// Therefore, if a user touches the Gameboard, this will create a Session with the pointer information for that Touch. If they lift their finger
        /// then place it down again, this will destroy the first session and create a new session when the touch resumes.
        /// </summary>
        public uint sessionId { get; private set; }

        /// <summary>
        /// For Pointer objects, this is the kind of Pointer we received (IE: Finger, Blade of Hand, etc.)
        /// </summary>
        public DataTypes.PointerTypes PointerType
        {
            get
            {
                if(TrackedObjectType == DataTypes.TrackedBoardObjectTypes.Pointer)
                {
                    return (DataTypes.PointerTypes)lastUpdateArgs.obj.ptr.tu_id;
                }
                else
                {
                    return DataTypes.PointerTypes.NONE;
                }
            }
        }

        /// <summary>
        /// The full TUIO object data received from the Gameboard.
        /// </summary>
        public TUIOObject tuio { get { return lastUpdateArgs.obj; } }

        /// <summary>
        /// If a Token was updated.
        /// </summary>
        public bool TokenUpdated { get { return lastUpdateArgs.UpdatedTok; } }

        /// <summary>
        /// If a Pointer (or finger touch) was updated.
        /// </summary>
        public bool PointerUpdated { get { return lastUpdateArgs.UpdatedPtr; } }

        /// <summary>
        /// If any Convex Hull Geometry was updated
        /// </summary>
        public bool ConvexHullGeometryUpdated { get { return lastUpdateArgs.UpdatedChg; } }

        /// <summary>
        /// If any Bounds were updated
        /// </summary>
        public bool BoundsUpdated { get { return lastUpdateArgs.UpdatedBnd; } }

        /// <summary>
        /// If a Data object was updated.
        /// </summary>
        public bool DataUpdated { get { return lastUpdateArgs.UpdatedDat; } }

        /// <summary>
        /// Checks if anything has been updated.
        /// </summary>
        public bool AnyUpdatesAvailable
        {
            get { return TokenUpdated || PointerUpdated || ConvexHullGeometryUpdated || BoundsUpdated || DataUpdated; }
        }

        public DataTypes.TrackedBoardObjectTypes TrackedObjectType
        {
            get
            {
                if (PointerUpdated)
                {
                    return DataTypes.TrackedBoardObjectTypes.Pointer;
                }
                else if (TokenUpdated)
                {
                    return DataTypes.TrackedBoardObjectTypes.Token;
                }
                else
                {
                    return DataTypes.TrackedBoardObjectTypes.None;
                }
            }
        }
        #endregion

        /// <summary>
        /// Where on the Gameboard screen this object exists.
        /// </summary>
        public Vector2 gameboardScreenPosition = new Vector2();

        /// <summary>
        /// Where in the Unity Scene this object exists, based on the position on the Gamebord screen
        /// </summary>
        public Vector3 sceneWorldPosition = new Vector3();

        /// <summary>
        /// Positions of the contour for this board object as represented in 2D, in World Space.
        /// </summary>
        public Vector2[] contourWorldVectors2D;
        
        /// <summary>
        /// Positions of the contour for this board object in World Space.
        /// </summary>
        public Vector3[] contourWorldVectors3D;

        /// <summary>
        /// Positions of the countour for this board object in Local Space in relation to the sceneWorldPosition of this trackedBoardObject.
        /// </summary>
        public Vector3[] contourLocalVectors3D;

        private ObjectUpdateEventArgs lastUpdateArgs;
    }
}