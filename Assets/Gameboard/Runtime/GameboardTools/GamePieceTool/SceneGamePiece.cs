using UnityEditor;
using UnityEngine;

namespace Gameboard.Tools
{
    public class SceneGamePiece : ScreenTouchInput
    {
        [Header("Object References")]
        [Tooltip("Place the visual objects for this piece inside a container in the GamePiece object. This is the container that will be hidden with hideOnPieceRemoval.")]
        public GameObject visualDisplayContainer;

        [Header("Token Lock Options")]
        [Tooltip("If this piece is Token Locked, then it will always match the position of the first TokenID it is given in its initial Source Reference, even if the piece is removed from the board and placed again in a different position.")]
        public bool tokenIdLocked;

        [Tooltip("If this piece is Token Locked, this will determine if the physical object is disabled when the piece is removed. If true, it will disable. If false, it will stay active.")]
        public bool hideOnPieceRemoval;

        [Header("Standard Options")]
        [Tooltip("If this piece is not Token Locked, then this will determine if this game piece is destroyed or not when the Tracked Board Object Reference is removed.")]
        public bool destroyOnObjectReferenceRemoval;

        [Tooltip("The radius size of this piece. This is used for checking if a physical piece on the Gameboard is on top of this digital piece in the scene.")]
        public float pieceRadius;

        [Header("System Touch")]
        [Tooltip("This allows a gamepiece to be moved via system touch inputs. Helps for in-editor testing especially.")]
        public bool isSystemTouchMoveable;

        [Tooltip("If true, and if isSystemTouchMoveable true, then this piece will only accept system touch if in the editor.")]
        public bool systemTouchInEditorOnly;

        [Tooltip("If true, whenever this object is interacted with, it will lock input to this object until the player stops touching it.")]
        public bool lockSystemTouchInputToThisObject;

        /// <summary>
        /// 999999999 is a unused token value for our system, therefore this is 'null'. We don't use 0 because 0 is a valid TokenID, and what
        /// the TokenID always starts as before it is read from the physical piece.
        /// </summary>
        private const uint nullTokenId = 999999999;

        public uint SessionId { get { return HasBoardObjectReference ? activeBoardObjectReference.SessionId : 0; } }
        public uint TokenID { get { return HasBoardObjectReference && activeBoardObjectReference.HasTokenId ? activeBoardObjectReference.TokenId : 0; } }
        public DataTypes.TrackedBoardObjectTypes ObjectType { get { return HasBoardObjectReference ? activeBoardObjectReference.ObjectType : DataTypes.TrackedBoardObjectTypes.None; } }

        public bool HasBoardObjectReference { get { return activeBoardObjectReference != null; } }

        private uint lockedTokenId = nullTokenId; 
        public TrackedBoardObjectReference activeBoardObjectReference { get; private set; }
        protected Vector3 currentTargetWorldPosition;
        private Vector3 DoesObjectReferenceOverlapPiece_Worker = Vector3.zero;

        #region Unity MonoBehavior Methods
        void OnEnable()
        {
            currentTargetWorldPosition = transform.position;    
        }

        void OnDestroy()
        {
            if (BoardInputTool.singleton != null)
            {
                BoardInputTool.singleton.ObjectReferenceUpdated -= ObjectReferenceUpdated;
                BoardInputTool.singleton.ObjectReferenceDeleted -= ObjectReferenceDeleted;
            }
        }

        void Update()
        {
            UpdateWorldPosition();

            UpdateOccured();
        }
        #endregion

        #region Event Listeners
        void ObjectReferenceUpdated(object origin, TrackedBoardObjectReference inObjectReference)
        {
            // If this GamePiece is Token Locked, and we have no BoardObjectReference, then this means the physical GamePiece was removed
            // from the board at some point, and we are waiting to get a new physical piece with a new SessionID, but with a matching TokenID.
            if(!HasBoardObjectReference)
            {
                if (tokenIdLocked && inObjectReference.HasTokenId && inObjectReference.TokenId == lockedTokenId)
                {
                    activeBoardObjectReference = inObjectReference;
                }
                else
                {
                    return;
                }
            }
            
            if(inObjectReference.SessionId != activeBoardObjectReference.SessionId)
            {
                return;
            }

            // Store the updated position target.
            currentTargetWorldPosition.x = activeBoardObjectReference.ObjectWorldPosition.x;
            currentTargetWorldPosition.y = transform.position.y;
            currentTargetWorldPosition.z = activeBoardObjectReference.ObjectWorldPosition.z;

            // TokenID is never in the initial reference object, so check for it in updates.
            if (activeBoardObjectReference.HasTokenId)
            {
                if (tokenIdLocked && lockedTokenId == nullTokenId)
                {
                    lockedTokenId = activeBoardObjectReference.TokenId;
                }
            }
        }

        void ObjectReferenceDeleted(object origin, TrackedBoardObjectReference inObjectReference)
        {
            if(!HasBoardObjectReference || inObjectReference.SessionId != activeBoardObjectReference.SessionId)
            {
                return;
            }

            activeBoardObjectReference = null;

            if (tokenIdLocked)
            {
                if(lockedTokenId == nullTokenId)
                {
                    Debug.LogWarning($"GamePiece {gameObject.name} is Token Locked, however it never received a TokenID. The TrackedBoardObject for this GamePiece was deleted. This GamePiece can no longer be tracked!");
                }

                if(hideOnPieceRemoval)
                {
                    visualDisplayContainer.SetActive(false);
                }

                return;
            }
            else
            {
                if (destroyOnObjectReferenceRemoval)
                {
                    Destroy(gameObject);
                }
            }    
        }
        #endregion

        #region Editor Only Methods
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Color cachedHandleColor = Handles.color;
                Handles.color = Color.red;
                Handles.DrawWireDisc(transform.position, Vector3.up, pieceRadius);
            Handles.color = cachedHandleColor;
        }
#endif
        #endregion

        #region Protected Virtuals
        protected virtual void UpdateOccured() { }
        #endregion

        /// <summary>
        /// Applies a new TrackedBoardObjectReference to this GamePiece. Use this when first setting up the GamePiece, and if you ever
        /// need to change the currently applied TrackedBoardObjectReference (like if this piece does not have a session id, and the piece is
        /// moved and placed somewhere else).
        /// </summary>
        /// <param name="sourceReference"></param>
        public void InjectBoardObjectReference(TrackedBoardObjectReference sourceReference)
        {
            BoardInputTool.singleton.ObjectReferenceUpdated += ObjectReferenceUpdated;
            BoardInputTool.singleton.ObjectReferenceDeleted += ObjectReferenceDeleted;

            if(activeBoardObjectReference != null &&
               activeBoardObjectReference.HasTokenId && 
               activeBoardObjectReference.TokenId != sourceReference.TokenId)
            {
                Debug.Log("Gamepiece " + gameObject.name + " now has TokenID " + sourceReference.TokenId);
            }

            activeBoardObjectReference = sourceReference;
        }

        private void UpdateWorldPosition()
        {
            // We do a per-value check like this to prevent additional GC, as perforing a Vector3 against a Vector3 compare
            // creates objects in memory which spikes the GC. We don't bother with the Y coordinate because the touch input is 2D.

            bool isAtTouchPosition = false;
            switch (downAxis)
            {
                case AxisAngles.X:
                    isAtTouchPosition = transform.position.y == currentTargetWorldPosition.y && transform.position.z == currentTargetWorldPosition.z;
                break;
                
                case AxisAngles.Y:
                    isAtTouchPosition = transform.position.x == currentTargetWorldPosition.x && transform.position.z == currentTargetWorldPosition.z;
                break;
                
                case AxisAngles.Z:
                    isAtTouchPosition = transform.position.x == currentTargetWorldPosition.x && transform.position.y == currentTargetWorldPosition.y;
                break;
            }

            if (isAtTouchPosition)
            {
                // At the intended target position, so just abort now.
                return;
            }

            // Do a MoveTowards so we don't get jerky movements.
            transform.position = Vector3.MoveTowards(transform.position, currentTargetWorldPosition, Time.deltaTime * 3000f);
        }

        /// <summary>
        /// Used for system level screen touch input if isTouchMoveable is true.
        /// </summary>
        /// <param name="touchPosition"></param>
        protected override void TouchMoved(Vector3 touchPosition)
        {
            if(!isSystemTouchMoveable ||
                systemTouchInEditorOnly && !Application.isEditor ||
                activeTouchState != TouchStates.Down || 
                activeBoardObjectReference != null)
            {
                return;
            }
                       
            currentTargetWorldPosition = touchPosition;
        }

        protected override void SetState_Down()
        {
            if(lockSystemTouchInputToThisObject)
            {
                LockInputToThisObject();
            }
        }

        protected override void SetState_Up()
        {
            if(lockSystemTouchInputToThisObject)
            {
                UnlockInputFromObject();
            }
        }

        public bool DoesObjectReferenceOverlapPiece(TrackedBoardObjectReference inObjectReference)
        {
            DoesObjectReferenceOverlapPiece_Worker.x = inObjectReference.ObjectWorldPosition.x;
            DoesObjectReferenceOverlapPiece_Worker.y = transform.position.y;
            DoesObjectReferenceOverlapPiece_Worker.z = inObjectReference.ObjectWorldPosition.z;

            return Vector3.Distance(DoesObjectReferenceOverlapPiece_Worker, transform.position) <= pieceRadius;
        }
    }
}