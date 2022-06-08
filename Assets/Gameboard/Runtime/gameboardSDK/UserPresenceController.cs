using System.Collections.Generic;
using UnityEngine;

using Gameboard.EventArgs;
using System.Threading.Tasks;
using System;
using System.Collections;

namespace Gameboard
{
    [RequireComponent(typeof(Gameboard))]
    public class UserPresenceController: MonoBehaviour
    {
        public delegate void OnUserPresenceHandler(GameboardUserPresenceEventArgs userPresence);
        public event OnUserPresenceHandler OnUserPresence;
        public Dictionary<string, GameboardUserPresenceEventArgs> Users = new Dictionary<string, GameboardUserPresenceEventArgs>();
        public event Action UserPresenceControllerInitialized;

        private Gameboard gameboard => Gameboard.Instance;
        private Queue<GameboardUserPresenceEventArgs> presenceUpdates = new Queue<GameboardUserPresenceEventArgs>(20);

        private void Awake()
        {
            gameboard.GameboardInitializationCompleted += OnGameboardInitialization;
        }

        private void Update()
        {
            if (presenceUpdates.Count == 0)
                return;

            GameboardLogging.Verbose($"--- processing presenceUpdates queue Count: {presenceUpdates.Count}");

            // Clone the current queue of presence updates 
            var clonedPresenceUpdates = new Queue<GameboardUserPresenceEventArgs>(presenceUpdates);
            presenceUpdates.Clear();

            // Process presence updates
            while (clonedPresenceUpdates.Count > 0)
            {
                GameboardUserPresenceEventArgs presenceEvent = clonedPresenceUpdates.Dequeue();
                GameboardLogging.Verbose($"--- processing event: {presenceEvent}");

                switch (presenceEvent.changeValue)
                {
                    case DataTypes.UserPresenceChangeTypes.UNKNOWN:
                        GameboardLogging.Error($"--- USER PRESENCE UNKNOWN - {presenceEvent}");
                        throw new ArgumentOutOfRangeException($"USER PRESENCE UNKNOWN - {presenceEvent}");

                    case DataTypes.UserPresenceChangeTypes.ADD:
                        if (!Users.ContainsKey(presenceEvent.userId))
                        {
                            // User was just added. Clear their PlayPanel to make sure it's fresh.
                            if (presenceEvent.presence == DataTypes.PresenceType.COMPANION.ToString())
                                StartCoroutine(ResetUserPlayPanel(presenceEvent.userId));
                            Users[presenceEvent.userId] = presenceEvent;
                        }
                        else
                        {
                            GameboardLogging.Error($"--- USER PRESENCE ADD TRIED ADD USER ALREADY PRESENT IN DICTIONARY ---- USER: {presenceEvent}");
                            throw new InvalidOperationException($"attempted to add a user that was already added for presence update: {presenceEvent}");
                        }
                        break;

                    case DataTypes.UserPresenceChangeTypes.REMOVE:
                        if (Users.ContainsKey(presenceEvent.userId))
                        {
                            // User was removed, so clear any events the game was waiting on for this player.
                            if (presenceEvent.type == DataTypes.PresenceType.COMPANION.ToString())
                                StartCoroutine(ClearQueueForUser(presenceEvent.userId));
                            Users.Remove(presenceEvent.userId);
                        }
                        else
                        {
                            GameboardLogging.Error($"--- USER PRESENCE REMOVE TRIED TO EXECUTE FOR USER NOT IN DICTIONARY ---- USER: {presenceEvent}");
                            throw new InvalidOperationException($"attempted to remove a user that was already removed for presence update: {presenceEvent}");
                        }
                        break;

                    case DataTypes.UserPresenceChangeTypes.CHANGE:
                        if (Users.ContainsKey(presenceEvent.userId))
                            Users[presenceEvent.userId] = presenceEvent;
                        else
                        {
                            GameboardLogging.Error($"--- USER PRESENCE CHANGE TRIED TO EXECUTE FOR USER NOT IN DICTIONARY ---- USER: {presenceEvent}");
                            throw new InvalidOperationException($"attempted to change a user was not present for presence update: {presenceEvent}");
                        }
                        break;

                    case DataTypes.UserPresenceChangeTypes.CHANGE_POSITION:
                        if (Users.ContainsKey(presenceEvent.userId))
                            Users[presenceEvent.userId] = presenceEvent;
                        else
                        {
                            GameboardLogging.Error($"--- USER PRESENCE CHANGE_POSITION TRIED TO EXECUTE FOR USER NOT IN DICTIONARY ---- USER: {presenceEvent}");
                            throw new InvalidOperationException($"attempted to change position of a user that did not exist for presence update: {presenceEvent}");
                        }
                        break;
                }
                OnUserPresence?.Invoke(presenceEvent);
            }
            clonedPresenceUpdates.Clear();
        }
        
        private void OnDestroy()
        {
            gameboard.GameboardInitializationCompleted -= OnGameboardInitialization;
            gameboard.services.companionHandler.UserPresenceChangedEvent -= OnUserPresenceReceived;
        }

        private IEnumerator ResetUserPlayPanel(string userId)
        {
            var resetTask = Gameboard.Instance.companionController.ResetPlayPanel(userId);
            yield return new WaitUntil(() => resetTask.IsCompleted);
        }

        private IEnumerator ClearQueueForUser(string userId)
        {
            Gameboard.Instance.companionController.ClearQueueForPlayer(userId);
            yield break;
        }

        /// <summary>
        /// Handle the 
        /// </summary>
        /// <param name="userPresence"></param>
        /// <remarks>
        /// This is called on a background thread.
        /// We'll add the event and process it on update so it can be easily accessed by the main thread.
        /// </remarks>>
        private void OnUserPresenceReceived(GameboardUserPresenceEventArgs userPresence)
        {
            GameboardLogging.Verbose("--- DIGESTING USER PRESENCE CHANGE EVENT");
            
            // Add to event list to notify Unity on the next update.
            presenceUpdates.Enqueue(userPresence);
        }

        /// <summary>
        /// Update the initial list of users when the gameboard is initialized and register for event handling.
        /// </summary>
        private async void OnGameboardInitialization()
        {
            GameboardLogging.Verbose("--- FETCHING INITIAL USER PRESENCE");
            gameboard.services.companionHandler.UserPresenceChangedEvent += OnUserPresenceReceived;
            var initialPresence = await GetCompanionUserPresence();
            initialPresence.playerPresenceList.ForEach(user => Users.Add(user.userId, user));
            GameboardLogging.Verbose("--- FINISHED FETCHING INITIAL USER PRESENCE");
            UserPresenceControllerInitialized?.Invoke();
        }

        /// <summary>
        /// Retrieves the user presence args from the gameboard for the specified userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>GameboardUserPresenceEventArgs</returns>
        /// <remarks>
        /// Returns the presence for the user matching the specified ID.
        /// Returns null if no presence is available for the specified user.
        /// </remarks>
        public GameboardUserPresenceEventArgs GetCompanionUserPresence(string userId)
        {
            GameboardLogging.Verbose($"--- FETCHING USER COMPANION PRESENCE for {userId}");
            if (Users.ContainsKey(userId))
            {
                return Users[userId];
            }

            GameboardLogging.Verbose($"--- FAILED WHEN FETCHING USER COMPANION PRESENCE for {userId}");
            return null;
        }

        /// <summary>
        /// Retrieves the user presence args from the companion service.
        /// </summary>
        /// <returns>CompanionUserPresenceEventArgs</returns>
        public async Task<CompanionUserPresenceEventArgs> GetCompanionUserPresence()
        {
            GameboardLogging.Verbose("--- FETCHING USER COMPANION PRESENCE");
            CompanionUserPresenceEventArgs responseArgs = await gameboard.services.companionHandler.GetUserPresenceList(Gameboard.COMPANION_VERSION);
            GameboardLogging.Verbose("--- FINISHED FETCHING USER COMPANION PRESENCE");
            return responseArgs;
        }
    }
}