using UnityEngine;

namespace Gameboard.Tools
{
    public class ScreenTouchPullTab : ScreenTouchInput
    {
        public float pullDistanceToToggle = 1f;
        public float positionResetSpeed = 500f;
        public AxisAngles pullDirection = AxisAngles.Z;
        public Transform pullTabDisplayObject;
        public string methodOnPull;
        public GameObject targetForPullMethod;
        public SpriteRenderer offSprite;
        public SpriteRenderer onSprite;
        public bool lockToThisWhenPulling;

        public bool canToggle { get; private set; }

        private Vector3 targetPoint;

        void OnEnable()
        {
            ResetPullTab();
        }

        private void ResetPullTab()
        {
            pullTabDisplayObject.localPosition = Vector3.zero;
            canToggle = true;

            if (offSprite != null)
            {
                offSprite.color = new Color(1f, 1f, 1f, 1f);
            }

            if (onSprite != null)
            {
                onSprite.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        protected override void SetState_Down()
        {
            if(lockToThisWhenPulling)
            {
                if (touchInputLockedToInstanceID == -1)
                {
                    LockInputToThisObject();
                }
                else
                {
                    return;
                }
            }

            targetPoint = transform.localPosition;

            switch (pullDirection)
            {
                case AxisAngles.X: targetPoint.x += pullDistanceToToggle; break;
                case AxisAngles.Y: targetPoint.y += pullDistanceToToggle; break;
                case AxisAngles.Z: targetPoint.z += pullDistanceToToggle; break;
            }
        }

        protected override void SetState_Up()
        {
            if(lockToThisWhenPulling)
            {
                UnlockInputFromObject();
            }
        }

        protected override void TouchMoved(Vector3 touchPosition)
        {
            if(!canToggle)
            {
                return;
            }

            if(activeTouchState == TouchStates.Down)
            {
                Vector3 localTouchPosition = transform.InverseTransformPoint(touchPosition);

                bool isDragging = false;
                switch (pullDirection)
                {
                    case AxisAngles.X: isDragging = localTouchPosition.x > 0f; break;
                    case AxisAngles.Y: isDragging = localTouchPosition.y > 0f; break;
                    case AxisAngles.Z: isDragging = localTouchPosition.z > 0f; break;
                }

                if (isDragging)
                {
                    float progress = 0f;
                    switch (pullDirection)
                    {
                        case AxisAngles.X: progress = localTouchPosition.x / pullDistanceToToggle; break;
                        case AxisAngles.Y: progress = localTouchPosition.y / pullDistanceToToggle; break;
                        case AxisAngles.Z: progress = localTouchPosition.z / pullDistanceToToggle; break;
                    }

                    if (offSprite != null)
                    {
                        offSprite.color = new Color(1f, 1f, 1f, 1f - progress);
                    }

                    if (onSprite != null)
                    {
                        onSprite.color = new Color(1f, 1f, 1f, progress);
                    }

                    float distance = pullDistanceToToggle * progress;
                    Vector3 targPosition = Vector3.zero;
                    switch (pullDirection)
                    {
                        case AxisAngles.X: targPosition.x = distance; break;
                        case AxisAngles.Y: targPosition.y = distance; break;
                        case AxisAngles.Z: targPosition.z = distance; break;
                    }

                    pullTabDisplayObject.localPosition = targPosition;

                    if(progress >= 1f)
                    {
                        TogglePullTab();
                    }
                }
            }
        }

        private void TogglePullTab()
        {
            canToggle = false;
            targetForPullMethod?.SendMessage(methodOnPull);
        }

        void FixedUpdate()
        {
            if(activeTouchState == TouchStates.Up || activeTouchState == TouchStates.Idle || activeTouchState == TouchStates.Disabled)
            {
                pullTabDisplayObject.localPosition = Vector3.MoveTowards(pullTabDisplayObject.localPosition, Vector3.zero, Time.deltaTime * positionResetSpeed);
            }

            FixedUpdateOccured();
        }

        protected virtual void FixedUpdateOccured() { }
    }
}