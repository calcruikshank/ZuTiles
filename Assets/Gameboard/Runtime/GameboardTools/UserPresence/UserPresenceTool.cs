using Gameboard.EventArgs;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// This is an example of how to integrate User Presence into your game. This tool can be used as is, or can be used as the foundation
/// for your own User Presence integration. Simply add this component to a GameObject in your game scene and call DrainQueue
/// to fetch the most recent presence updates.
/// </summary>

namespace Gameboard.Tools
{
    public class UserPresenceTool : GameboardToolFoundation
    {
        [Tooltip("If enabled, will verbosely write details on every received user presence update to the log.")]
        public bool writePresenceUpdatesToLog;

        public static UserPresenceTool singleton;

        /// <summary>
        /// The Queue of received presence updates.
        /// </summary>
        private Queue<GameboardUserPresenceEventArgs> presenceUpdates = new Queue<GameboardUserPresenceEventArgs>(20);

        /// <summary>
        /// Queue used for draining the presenceUpdates and sending to other scripts.
        /// </summary>
        private Queue<GameboardUserPresenceEventArgs> drainedQueue = new Queue<GameboardUserPresenceEventArgs>(20);

        /// <summary>
        /// How many seconds to wait after receiving a getUserPresenceList update before requesting another.
        /// </summary>
        private const float presenceUpdateRequestDelay = 5f;

        /// <summary>
        /// If true, then we are waiting for a getUserPresenceList response.
        /// </summary>
        public bool playerPresenceRequestActive;

        private bool setupCompleted;

        protected override void PerformCleanup()
        {
            if (Gameboard.Instance != null && setupCompleted)
            {
                Gameboard.Instance.companionController.UserPresenceUpdated -= CompanionController_UserPresenceUpdated;
            }
        }

        void Update()
        {
            if (Gameboard.Instance == null)
            {
                return;
            }

            // Do the setup here in Update so we can just do a Singleton lookup on Gameboard, and not worry about race-conditions in using Start.
            if(!setupCompleted)
            {
                if (Gameboard.Instance.companionController.isConnected)
                {
                    Gameboard.Instance.companionController.UserPresenceUpdated += CompanionController_UserPresenceUpdated;
                    singleton = this;
                    setupCompleted = true;

                    Debug.Log("--- Gameboard User Presence tool is ready!");

                    RequestUserPresenceUpdate();
                }
            }
        }

        /// <summary>
        /// Initiates a user presence update to occur, acquiring the current state of everyone connected. Will result in the DrainQueue have contents to drain.
        /// </summary>
        public void RequestUserPresenceUpdate()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            UserPresenceRequestUpdate();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Async task which manuaually requests the current status of UserPresence users, then stores them in the presenceUpdates queue.
        /// </summary>
        /// <returns></returns>
        async Task UserPresenceRequestUpdate()
        {
            // NOTE: We handle all UserPresenceRequesting logic in this one method to keep things contained.

            if (playerPresenceRequestActive)
            {
                // Already actively fetching user presence
                return;
            }

            // We're ready! Let's fetch that presence!
            playerPresenceRequestActive = true;

            Task<CompanionUserPresenceEventArgs> task = Gameboard.Instance.companionController.FetchUserPresence();
            if (await Task.WhenAny(task, Task.Delay(2500)) == task)
            {
                CompanionUserPresenceEventArgs userPresence = task.Result;
                if (!userPresence.wasSuccessful)
                {
                    Debug.LogError($"PRESENCE: FetchUserPresence failed with error {userPresence.errorResponse.Message}");
                    playerPresenceRequestActive = false;
                    return;
                }

                foreach (GameboardUserPresenceEventArgs playerObject in userPresence.playerPresenceList)
                {
                    if (writePresenceUpdatesToLog)
                    {
                        string updateFeatures = "";
                        if(playerObject.boardUserPosition != null)
                        {
                            updateFeatures += "Position: " + playerObject.boardUserPosition.screenPosition + ". ";
                        }

                        if (playerObject.tokenColor != null)
                        {
                            updateFeatures += "Color: " + playerObject.tokenColor + ". ";
                        }

                        if(!string.IsNullOrEmpty(playerObject.userName))
                        {
                            updateFeatures += "Name: " + playerObject.userName + ". ";
                        }

                        Debug.Log($"USER PRESENCE - ENQUEUING PRESENCE UPDATE. UserID: {playerObject.userId}, ChangeValue: {playerObject.changeValue}. {updateFeatures}");
                    }

                    switch (playerObject.changeValue)
                    {
                        case DataTypes.UserPresenceChangeTypes.UNKNOWN:
                        break;
                            
                        case DataTypes.UserPresenceChangeTypes.ADD:
                            // User was just added. Clear their PlayPanel to make sure it's fresh.
                            await Gameboard.Instance.companionController.ResetPlayPanel(playerObject.userId);
                        break;
                            
                        case DataTypes.UserPresenceChangeTypes.REMOVE:
                            // User was removed, so clear any events the game was waiting on for this player.
                            Gameboard.Instance.companionController.ClearQueueForPlayer(playerObject.userId);
                        break;
                            
                        case DataTypes.UserPresenceChangeTypes.CHANGE:
                        break;
                            
                        case DataTypes.UserPresenceChangeTypes.CHANGE_POSITION:
                        break;
                    }

                    lock (presenceUpdates)
                    {
                        presenceUpdates.Enqueue(playerObject);
                    }
                }
            }
            else
            {
                // Since this timed out, let's request it again.
				playerPresenceRequestActive = false;
                RequestUserPresenceUpdate();
                return;
            }

            playerPresenceRequestActive = false;
        }

        /// <summary>
        /// Event receiver for UserPresence Updates that originate on the Companions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void CompanionController_UserPresenceUpdated(object sender, GameboardUserPresenceEventArgs eventArgs)
        {
            Debug.Log("--- USER PRESENCE TOOL RECEIVED USER PRESENCE UPDATE " + eventArgs.companionId);
            presenceUpdates.Enqueue(eventArgs);
        }

        /// <summary>
        /// Returns a Queue of GameboardUserPresenceEventArgs in the order they were received by the game. This also empties the Queue in the UserPresenceTool,
        /// so the next time you call DrainQueue it will be the next batch of received GameboardUserPresenceEventArgs.
        /// </summary>
        /// <returns></returns>
        public Queue<GameboardUserPresenceEventArgs> DrainQueue()
        {
            lock (presenceUpdates)
            {
                lock (drainedQueue)
                {
                    drainedQueue.Clear();
                    while (presenceUpdates.Count > 0)
                    {
                        drainedQueue.Enqueue(presenceUpdates.Dequeue());
                    }

                    presenceUpdates.Clear();
                }
            }

            return drainedQueue;
        }

        public DataTypes.ScreenSides GetScreenEdgeFromUserScreenPosition(Vector2 inScreenPosition)
        {
            if (inScreenPosition.y == 0f)
            {
                // At top of screen
                return DataTypes.ScreenSides.Forward;
            }
            else if (inScreenPosition.y == 1f)
            {
                // At bottom of screen
                return DataTypes.ScreenSides.Back;
            }
            else if (inScreenPosition.x == 0f)
            {
                // At left of screen
                return DataTypes.ScreenSides.Left;
            }
            else if (inScreenPosition.x == 1f)
            {
                // At right of screen
                return DataTypes.ScreenSides.Right;
            }
            else
            {
                // Unknown position. Really this should never happen.
                return DataTypes.ScreenSides.UNKNOWN;
            }
        }
    }
}