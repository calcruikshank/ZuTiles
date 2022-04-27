using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard.Tools
{
    public class BoardSpace : MonoBehaviour
    {
        public Collider spaceCollider;

        public Guid spaceId { get; private set; }
        public Vector3 spaceWorldPosition { get { return transform.position; } }
        public List<BoardSpace> connectedSpaceList { get; private set; }

        private bool spaceInitialized;

        #region Initialization / DeInitialization
        public void InitializeSpace()
        {
            spaceId = Guid.NewGuid();
            connectedSpaceList = new List<BoardSpace>();

            if (spaceCollider != null)
            {
                spaceCollider.isTrigger = true;
            }

            spaceInitialized = true;

            SpaceWasInitialized();
        }

        public void DeInitializeSpace()
        {
            SpaceDeinitializationBegun();

            connectedSpaceList.Clear();
            spaceInitialized = false;
        }
        #endregion

        #region Connected Space Management
        public void ConnectSpace(BoardSpace inSpace)
        {
            if(!spaceInitialized)
            {
                Debug.LogWarning("Space cannot be connected to because it is not initalized!");
                return;
            }

            if(connectedSpaceList.Contains(inSpace))
            {
                Debug.Log($"Board space {spaceId} already has {inSpace.spaceId} connected to it!");
                return;
            }

            connectedSpaceList.Add(inSpace);
        }

        public void DisconnectSpace(BoardSpace inSpace)
        {
            if (!spaceInitialized)
            {
                Debug.LogWarning("Space cannot be disconnected from because it is not initalized!");
                return;
            }

            if (!connectedSpaceList.Contains(inSpace))
            {
                Debug.LogWarning($"Board space {spaceId} cannot remove space {inSpace.spaceId} because it is not connected!");
                return;
            }

            connectedSpaceList.Remove(inSpace);
        }
        #endregion

        #region Space Querying
        public bool IsPointOnSpace(Vector3 inPoint)
        {
            if (!spaceInitialized)
            {
                Debug.LogWarning("Space cannot check if points are on it because because it is not initalized!");
                return false;
            }

            if (spaceCollider == null)
            {
                Debug.Log($"Space {spaceId} has no collider. IsPointOnSpace will always return false!");
                return false;
            }

            return spaceCollider.bounds.Contains(inPoint);
        }
        #endregion

        #region Editor Methods
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (connectedSpaceList != null)
            {
                Gizmos.color = Color.blue;
                    foreach (BoardSpace connectedSpace in connectedSpaceList)
                    {
                        Gizmos.DrawLine(transform.position, connectedSpace.transform.position);
                    }
                Gizmos.color = Color.white;
            }

            DrawEditorGizmos();
        }
#endif
        #endregion

        #region Trigger Handling
        public void OnTriggerEnter(Collider other)
        {
            if(!spaceInitialized)
            {
                return;
            }

            BoardSpaceInteractor interactor = other.gameObject.GetComponent<BoardSpaceInteractor>();
            if (interactor != null)
            {
                interactor.InteractorEnteredBoardSpace(this);
                ObjectEnteredSpace(interactor);
            }
        }

        public void OnTriggerStay(Collider other)
        {
            if (!spaceInitialized)
            {
                return;
            }

            BoardSpaceInteractor interactor = other.gameObject.GetComponent<BoardSpaceInteractor>();
            if (interactor != null)
            {
                ObjectStayingOnSpace(interactor);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (!spaceInitialized)
            {
                return;
            }

            BoardSpaceInteractor interactor = other.gameObject.GetComponent<BoardSpaceInteractor>();
            if (interactor != null)
            {
                interactor.InteractorExitedBoardSpace(this);
                ObjectExitedSpace(interactor);
            }
        }
        #endregion

        #region Protected Methods
        protected virtual void SpaceWasInitialized() { }
        protected virtual void SpaceDeinitializationBegun() { }
        protected virtual void ObjectEnteredSpace(BoardSpaceInteractor spaceInteractor) { }
        protected virtual void ObjectStayingOnSpace(BoardSpaceInteractor spaceInteractor) { }
        protected virtual void ObjectExitedSpace(BoardSpaceInteractor spaceInteractor) { }

        #if UNITY_EDITOR
        protected virtual void DrawEditorGizmos() { }
        #endif
#endregion
    }
}