using UnityEngine;

namespace Gameboard.Tools
{
    public class TrackedBoardObjectReference
    {
        public uint SessionId { get; private set; } = 0;
        public Vector3 ObjectWorldPosition { get { return lastBoardObject == null ? Vector3.zero : lastBoardObject.sceneWorldPosition; } }
        public DataTypes.TrackedBoardObjectTypes ObjectType { get { return lastBoardObject == null ? DataTypes.TrackedBoardObjectTypes.None : lastBoardObject.TrackedObjectType; } }
        
        public bool HasTokenId
        {
            get
            {
                if (lastBoardObject == null || lastBoardObject.TrackedObjectType != DataTypes.TrackedBoardObjectTypes.Token)
                {
                    return false;
                }
                else if (!lastBoardObject.TokenUpdated)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        
        public bool HasConvexGeometry { get { return lastBoardObject != null && lastBoardObject.ConvexHullGeometryUpdated && ConvexGeometryWorldPositions != null; } }

        public Vector3[] ConvexGeometryWorldPositions { get { return lastBoardObject.contourWorldVectors3D; } }

        /// <summary>
        /// The 2D top-down angle of this object on the Gameboard.
        /// </summary>
        public float ObjectAngle
        {
            get
            {
                switch (lastBoardObject.TrackedObjectType)
                {
                    case DataTypes.TrackedBoardObjectTypes.None: return 0f;
                    case DataTypes.TrackedBoardObjectTypes.Pointer: return lastBoardObject.tuio.ptr.angle;
                    case DataTypes.TrackedBoardObjectTypes.Token: return lastBoardObject.tuio.tok.angle;
                    default: return 0f;
                }
            }
        }

        public uint TokenId { get  { return lastBoardObject.tuio.tok.tu_id; } }
        public DataTypes.PointerTypes PointerType { get { return lastBoardObject.PointerType; } }

        private TrackedBoardObject lastBoardObject;

        public TrackedBoardObjectReference(TrackedBoardObject sourceBoardObject)
        {
            lastBoardObject = sourceBoardObject;

            // The sessionId for an individual object will never change.
            SessionId = sourceBoardObject.sessionId;
        }

        public void UpdateWithTrackedBoardObject(TrackedBoardObject inBoardObject)
        {
            if(SessionId != 0)
            {
                if(SessionId != inBoardObject.sessionId)
                {
                    Debug.LogWarning($"TrackedBoardObjectReference was updated with a mismatched SessionID! Existing ID is {SessionId} however was attempted to update with ID {inBoardObject.sessionId}");
                    return;
                }
            }

            if(inBoardObject.TrackedObjectType != ObjectType)
            {
                ObjectTypeChangeOccured(inBoardObject.TrackedObjectType);
            }

            lastBoardObject = inBoardObject;
        }

        protected virtual void ObjectTypeChangeOccured(DataTypes.TrackedBoardObjectTypes newObjectType) { }
    }
}