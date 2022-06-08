using System.Collections.Generic;
using UnityEngine;
using Gameboard.EventArgs;
using System;
using System.Threading.Tasks;

namespace Gameboard
{

    [RequireComponent(typeof(Gameboard))]
    [RequireComponent(typeof(UserPresenceController))]
    public class DiceRollController : MonoBehaviour
    {
        public delegate void OnDiceRolledHandler(CompanionDiceRollEventArgs diceRolledEvent);
        public event OnDiceRolledHandler OnDiceRolled;

        private Queue<CompanionDiceRollEventArgs> eventQueue = new Queue<CompanionDiceRollEventArgs>(20);

        private Gameboard gameboard => Gameboard.Instance;

        private void Awake()
        {
            gameboard.GameboardInitializationCompleted += OnGameboardInit;
            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
        }

        private void Update()
        {
            if (eventQueue.Count == 0)
                return;

            ProcessDiceRollEvents();
        }

        private void OnGameboardInit()
        {
            gameboard.services.companionHandler.DiceRolledEvent += OnDiceRolledEvent;
        }

        private void OnGameboardShutdown()
        {
            gameboard.services.companionHandler.DiceRolledEvent -= OnDiceRolledEvent;
        }

        private void ProcessDiceRollEvents()
        {
            // Get the events to process
            Queue<CompanionDiceRollEventArgs> clonedEventQueue = new Queue<CompanionDiceRollEventArgs>(eventQueue);
            eventQueue.Clear();
            while (clonedEventQueue.Count > 0)
            {
                // Clear the queue one at a time
                CompanionDiceRollEventArgs eventToProcess = clonedEventQueue.Dequeue();
                OnDiceRolled?.Invoke(eventToProcess);
            }
            clonedEventQueue.Clear();
        }

        /// <summary>
        /// Set the dice background asset for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assetGuid"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetDiceBackgroundAsset(string userId, string assetGuid)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetDiceBackground(Gameboard.COMPANION_VERSION, userId, assetGuid);
            return responseArgs;
        }

        /// <summary>
        /// Set the visibility of the dice selector for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="visible"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetDiceSelectorVisibility(string userId, bool visible)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetDiceSelectorVisibility(Gameboard.COMPANION_VERSION, userId, visible);
            return responseArgs;
        }

        /// <summary>
        /// Initiates a Dice Roll on a Companion.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="diceSizeArray">Array of ints matching the number of sides on the dice (IE: [6,6,12] would show 2x D6 and 1 D12)</param>
        /// <param name="addedModifier"></param>
        /// <param name="diceTintColor"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> RollDice(string userId, int[] diceSizeArray, int addedModifier, Color diceTintColor, string inDiceNotation)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.RollDice(Gameboard.COMPANION_VERSION, userId, diceSizeArray, addedModifier, diceTintColor, inDiceNotation);
            return responseArgs;
        }

        /// <summary>
        /// Digests a Dice Roll. Internal method for the CompanionController class.
        /// </summary>
        /// <param name="userIdWhoRolled"></param>
        /// <param name="diceSizeArray"></param>
        /// <param name="addedModifier"></param>
        private void OnDiceRolledEvent(GameboardDiceRolledEventArgs diceRolledEventArgs)
        {
            CompanionDiceRollEventArgs eventArgs = new CompanionDiceRollEventArgs()
            {
                diceSizesRolledList = diceRolledEventArgs.diceSizesRolledList,
                addedModifier = diceRolledEventArgs.addedModifier,
                diceNotation = diceRolledEventArgs.diceNotation,
                ownerId = diceRolledEventArgs.ownerId,
            };

            // This event comes in a worker thread. Add to event list to be handled in the main thread. 
            eventQueue.Enqueue(eventArgs);
        }
    }

}
