using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameboard.EventArgs;
using Gameboard.Helpers;
using UnityEngine;
namespace Gameboard
{

    public class UserPresenceSceneObject : MonoBehaviour
    {
        public string userId { get; private set; }
        public string seatKey { get; private set; }
        public string userName { get; private set; }
        public Vector3 playerSceneDrawerPosition { get; private set; }
        public Vector2 playerScreenPosition { get; private set; } = Vector2.zero;
        public Color playerTintColor { get; private set; }
        private bool userWasAdded { get; set; }

        void Start()
        {
#if UNITY_EDITOR
            Gameboard.Instance.GameboardShutdownBegun += EditorOnly_HaveUserLeaveGame;
#endif
        }

#if UNITY_EDITOR
        public void EditorOnly_HaveUserLeaveGame()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Gameboard.Instance.companionController.UserLeft(userId);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
#endif

        public void InjectDependencies(GameboardUserPresenceEventArgs inSourcePresence)
        {
            userId = inSourcePresence.userId;

            if (string.IsNullOrEmpty(inSourcePresence.seatKey))
            {
                GameboardLogging.LogMessage($"User at seat {inSourcePresence.seatKey} has no UserID assigned! If this is in the editor, be sure to call InjectUserId once the UserID is available!", GameboardLogging.MessageTypes.Warning);
            }
            else
            {
                seatKey = inSourcePresence.seatKey;
            }

            playerTintColor = inSourcePresence.unityColor;

            UpdateUserPresence(inSourcePresence);
        }

        public void InjectUserId(string inUserId)
        {
            userId = inUserId;
        }

        public void UpdateUserPresence(GameboardUserPresenceEventArgs inEventArgs)
        {
            // In case the player was created via a seat key, they may not have a userid assigned yet. Assign that now. Note that this
            // only really ever happens if you create a user in the Unity Editor, as you would create them by making a virtual Seat Key
            // and then tell the Gameboard about them. In any other instance, the first Player Presence data that come through would 
            // contain the UserID.
            if (userId != inEventArgs.userId)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    GameboardLogging.LogMessage($"User presence updated with a mismatched UserID! Seat Key {seatKey} already has UserID {userId} however the Player Presence update supplied it as {inEventArgs.userId}. It has been changed to match the new UserID.", GameboardLogging.MessageTypes.Warning);
                }

                userId = inEventArgs.userId;

                // PlayerAdded only gets called when this player is fully added, with a UserId. So call that now as well.
                PlayerAdded();
                userWasAdded = true;
            }

            switch (inEventArgs.changeValue)
            {
                case DataTypes.UserPresenceChangeTypes.UNKNOWN:
                    // Unknown! Flag it as an error.
                    GameboardLogging.LogMessage($"Unknown user presence change occured for user {userId}", GameboardLogging.MessageTypes.Warning);
                    break;

                case DataTypes.UserPresenceChangeTypes.ADD:
                    if (userId != null && !userWasAdded)
                    {
                        // New user was added
                        PlayerAdded();
                        playerScreenPosition = inEventArgs.boardUserPosition.screenPosition;
                        UpdatePlayerPositionIfNeeded();

                        userWasAdded = true;
                    }
                    break;

                case DataTypes.UserPresenceChangeTypes.CHANGE_POSITION:
                    // The drawer position was changed.
                    playerScreenPosition = inEventArgs.boardUserPosition.screenPosition;
                    UpdatePlayerPositionIfNeeded();
                    break;

                case DataTypes.UserPresenceChangeTypes.REMOVE:
                    // User was removed.
                    PlayerRemoved();
                    break;

                case DataTypes.UserPresenceChangeTypes.CHANGE:
                    // Something like Username or Color may have changed, so allow updates for those.

                    if (inEventArgs.unityColor != playerTintColor)
                    {
                        playerTintColor = inEventArgs.unityColor;
                        PlayerColorChanged();
                    }

                    if (inEventArgs.userName != userName)
                    {
                        userName = inEventArgs.userName;
                        PlayerNameChanged();
                    }

                    break;
            }
        }

        private void UpdatePlayerPositionIfNeeded()
        {
            playerSceneDrawerPosition = GameboardHelperMethods.GameboardScreenPointToScenePoint(Camera.main, playerScreenPosition);

            ScenePositionUpdated(new Vector3(playerSceneDrawerPosition.x, 0.2f, playerSceneDrawerPosition.z));

            Vector3 eulerRotation = Vector3.zero;
            if (playerScreenPosition.y == 0f)
            {
                // At top of screen
                eulerRotation.y = 180f;
            }
            else if (playerScreenPosition.y == 1f)
            {
                // At bottom of screen
                eulerRotation.y = 0f;
            }
            else if (playerScreenPosition.x == 0f)
            {
                // At left of screen
                eulerRotation.y = 90f;
            }
            else if (playerScreenPosition.x == 1f)
            {
                // At right of screen
                eulerRotation.y = -90f;
            }

            LocalEulerAnglesUpdated(eulerRotation);
        }

        protected virtual void ScenePositionUpdated(Vector3 inNewPosition) { }
        protected virtual void LocalEulerAnglesUpdated(Vector3 inNewEulers) { }
        protected virtual void PlayerAdded() { }
        protected virtual void PlayerRemoved() { }
        protected virtual void PlayerNameChanged() { }
        protected virtual void PlayerColorChanged() { }
    }

}