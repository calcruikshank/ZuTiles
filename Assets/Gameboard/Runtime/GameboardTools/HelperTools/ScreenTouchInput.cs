using System.Collections.Generic;
using UnityEngine;

namespace Gameboard.Tools
{
    public class ScreenTouchInput : MonoBehaviour
    {
        public Collider inputCollider;
        public AxisAngles downAxis = AxisAngles.Y;

        /// <summary>
        /// If true, then the input is locked to the Instance ID of a specific object.
        /// </summary>
        protected static int touchInputLockedToInstanceID = -1;

        private bool IsTouchInputLockedToID { get { return touchInputLockedToInstanceID != -1; } }

        /// <summary>
        /// If this is True, then input for this object will only initiate when first touched. This means that if a finger is
        /// on the screen, and this object appears beneath that finger, this object will not get toggled until the finger is lifted
        /// and then lowered again. This is useful for things like Button objects.
        /// </summary>
        protected virtual bool BeginOnFirstTouchOnly { get { return false; } }

        protected float lastInputTime;
        public TouchStates activeTouchState { get; private set; } = TouchStates.Idle;
        private List<int> touchedByFingerList = new List<int>();
        private Dictionary<int, Vector3> storedTouchOffsetByFingerDict = new Dictionary<int, Vector3>();

        private Vector3 storedTouchOffset;

        public enum TouchStates
        {
            NONE = 0,
            Idle = 1,
            Down = 2,
            Up = 3,
            Disabled = 4,
        }

        public enum AxisAngles
        {
            X = 0,
            Y = 1,
            Z = 2,
        }

        void LateUpdate()
        {
            if(IsTouchInputLockedToID && gameObject.GetInstanceID() != touchInputLockedToInstanceID || activeTouchState == TouchStates.Disabled)
            {
                return;
            }
            
#if UNITY_EDITOR
            if(Input.GetMouseButton(0) && TryInputFromScreenPosition(Input.mousePosition))
            {
                if(activeTouchState != TouchStates.Down)
                {
                    storedTouchOffset = AcquirePositionOffsetFromTouchPosition(Input.mousePosition);
                }

                ChangeInputState(TouchStates.Down);
            }
            else if(activeTouchState == TouchStates.Down)
            {
                ChangeInputState(TouchStates.Up);
            }

            if (activeTouchState == TouchStates.Down)
            {
                UpdateMovementPosition(Input.mousePosition, storedTouchOffset);
            }

#elif UNITY_ANDROID

            Touch[] touchArray = Input.touches;
            UpdateInRangeFingerIdList(touchArray, ref touchedByFingerList);
            UpdateStateForTouchCount(touchedByFingerList.Count);

            if(activeTouchState == TouchStates.Down)
            {
                for(int i = 0; i < touchArray.Length; i++)
                {
                    Touch thisTouch = touchArray[i];
                    if (thisTouch.phase == TouchPhase.Moved && touchedByFingerList.Contains(thisTouch.fingerId))
                    {
                        UpdateMovementPosition(thisTouch.position, storedTouchOffsetByFingerDict[thisTouch.fingerId]);
                    }
                }
            }
#endif

            LateUpdateOccured();
        }

        private void UpdateInRangeFingerIdList(Touch[] inTouchArray, ref List<int> inExistingFingerTouchList)
        {
            // If there are currently no touches, just clear everything out.
            if(inTouchArray.Length == 0)
            {
                inExistingFingerTouchList.Clear();
                return;
            }

            List<int> updatedTouchList = new List<int>();
            for(int i = 0; i < inTouchArray.Length; i++)
            {
                Touch touch = inTouchArray[i];
                if (touch.phase != TouchPhase.Began &&
                    touch.phase != TouchPhase.Stationary && 
                    touch.phase != TouchPhase.Moved)
                {
                    // Ignore this touch
                    continue;
                }

                if (BeginOnFirstTouchOnly)
                {
                    if (!inExistingFingerTouchList.Contains(touch.fingerId))
                    {
                        if (touch.phase != TouchPhase.Began)
                        {
                            // Only listen to Began actions if not already in the list.
                            continue;
                        }
                    }
                }

                updatedTouchList.Add(touch.fingerId);

                bool touchIsInRange = TryInputFromScreenPosition(touch.position);
                if (inExistingFingerTouchList.Contains(touch.fingerId))
                {
                    // This finger is already touching this object. Let's make sure it's still touching it.
                    if(!touchIsInRange)
                    {
                        // Is no longer in range, so remove it from the list.
                        inExistingFingerTouchList.Remove(touch.fingerId);
                        storedTouchOffsetByFingerDict.Remove(touch.fingerId);
                    }
                }
                else if (touchIsInRange)
                {
                    // Finger list does not contain this touch. If it's in range, add it to the list.
                    inExistingFingerTouchList.Add(touch.fingerId);

                    // Also store the initial positional offset of this touch related to this object
                    storedTouchOffsetByFingerDict.Add(touch.fingerId, AcquirePositionOffsetFromTouchPosition(touch.position));
                }
            }

            // Now check if anything in the inExistingFingerTouchList is no longer present in the inTouchArray, and if not, remove them.
            for (int i = inExistingFingerTouchList.Count - 1; i >= 0; i--)
            {
                int thisFingerId = inExistingFingerTouchList[i];
                if(!updatedTouchList.Contains(thisFingerId))
                {
                    inExistingFingerTouchList.RemoveAt(i);

                    if (storedTouchOffsetByFingerDict.ContainsKey(thisFingerId))
                    {
                        storedTouchOffsetByFingerDict.Remove(thisFingerId);
                    }
                }
            }
        }

        private void UpdateStateForTouchCount(int inTouchingCount)
        {
            switch (activeTouchState)
            {
                case TouchStates.NONE:
                    if (inTouchingCount > 0)
                    {
                        ChangeInputState(TouchStates.Down);
                    }
                    else
                    {
                        ChangeInputState(TouchStates.Idle);
                    }
                    break;

                case TouchStates.Idle:
                    if (inTouchingCount > 0)
                    {
                        ChangeInputState(TouchStates.Down);
                    }
                    break;

                case TouchStates.Down:
                    if (inTouchingCount == 0)
                    {
                        ChangeInputState(TouchStates.Idle);
                    }
                break;

                case TouchStates.Up:
                    ChangeInputState(TouchStates.Idle);
                break;

                case TouchStates.Disabled:
                    // Do nothing!
                break;
            }
        }

        private bool TryInputFromScreenPosition(Vector3 inPosition)
        {
            if(inputCollider == null || activeTouchState == TouchStates.Disabled)
            {
                return false;
            }

            Vector3 scenePosition = CalculateCameraScreenToWorldPoint(inPosition);
            switch (downAxis)
            {
                case AxisAngles.X: scenePosition.x = transform.position.x; break;
                case AxisAngles.Y: scenePosition.y = transform.position.y; break;
                case AxisAngles.Z: scenePosition.z = transform.position.z; break;
            }

            // The touch could be moving fast, therefore use allowedTouchOffset to see if it's within an allowed range of 
            // the object position. If not, ignore it, as it could also be another touch somewhere else on the screen.
            float allowedTouchOffset = 1f;

            if(inputCollider != null)
            {
                // Has an input collider.
                bool contains = inputCollider.bounds.Contains(scenePosition);
                float distance = Vector3.Distance(transform.position, scenePosition);
                return contains || distance < allowedTouchOffset;
            }
            else
            {
                // Does not have an input collider.
                return Vector3.Distance(transform.position, scenePosition) > allowedTouchOffset;
            }
        }

        private Vector3 AcquirePositionOffsetFromTouchPosition(Vector3 inCurrentPosition)
        {
            Vector3 scenePosition = CalculateCameraScreenToWorldPoint(inCurrentPosition);
            switch (downAxis)
            {
                case AxisAngles.X: scenePosition.x = transform.position.x; break;
                case AxisAngles.Y: scenePosition.y = transform.position.y; break;
                case AxisAngles.Z: scenePosition.z = transform.position.z; break;
            }

            return transform.position - scenePosition;
        }

        private void UpdateMovementPosition(Vector3 inCurrentPosition, Vector3 inTouchOffset)
        {
            Vector3 scenePosition = CalculateCameraScreenToWorldPoint(inCurrentPosition);
            switch (downAxis)
            {
                case AxisAngles.X: scenePosition.x = transform.position.x; break;
                case AxisAngles.Y: scenePosition.y = transform.position.y; break;
                case AxisAngles.Z: scenePosition.z = transform.position.z; break;
            }

            TouchMoved(scenePosition + inTouchOffset);
        }

        private Vector3 CalculateCameraScreenToWorldPoint(Vector3 inPoint)
        {
            // Use the camera defined for the Gameboard if there is one, otherwise fallback to the default Camera.
            return Gameboard.singleton.gameCamera != null ? Gameboard.singleton.gameCamera.ScreenToWorldPoint(inPoint) : Camera.main.ScreenToWorldPoint(inPoint);
        }

        public void DisableInput()
        {
            ChangeInputState(TouchStates.Disabled);
        }

        public void EnableInput()
        {
            ChangeInputState(TouchStates.Up);
        }

        protected void ChangeInputState(TouchStates inState)
        {
            if (activeTouchState == inState)
            {
                return;
            }

            activeTouchState = inState;

            switch (activeTouchState)
            {
                case TouchStates.Idle:
                    UnlockInputFromObject();
                break;

                case TouchStates.Down:
                    SetState_Down();
                break;

                case TouchStates.Up:
                    SetState_Up();
                    ChangeInputState(TouchStates.Idle);
                break;

                case TouchStates.Disabled:
                    SetState_Disabled();
                break;
            }
        }

#region Protected Virtuals
        protected virtual void SetState_Down() {}
        protected virtual void SetState_Up() {}
        protected virtual void SetState_Disabled() { }
        protected virtual void TouchMoved(Vector3 touchPosition) {}
        protected virtual void LateUpdateOccured() { }
#endregion

        public void LockInputToThisObject()
        {
            touchInputLockedToInstanceID = gameObject.GetInstanceID();
        }

        public void UnlockInputFromObject()
        {
            touchInputLockedToInstanceID = -1;
        }
    }
}