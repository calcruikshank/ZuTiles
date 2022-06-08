using Gameboard.EventArgs;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an example of how to integrate Dice Rolling into your game. This tool can be used as is, or can be used as the foundation
/// for your own User Presence integration. Simply add this component to a GameObject in your game scene and call DrainQueue
/// to fetch the most recent received Dice Rolls that ocurred on Companions.
/// </summary>

namespace Gameboard.Tools
{
    public class DiceRollTool : GameboardToolFoundation
    {
        private Queue<CompanionDiceRollEventArgs> diceRollUpdates = new Queue<CompanionDiceRollEventArgs>();
        private bool setupCompleted;

        protected override void PerformCleanup()
        {
            if (Gameboard.Instance != null && setupCompleted)
            {
                Gameboard.Instance.companionController.DiceRolled -= CompanionController_DiceRolled;
            }
        }

        void Update()
        {
            if (Gameboard.Instance == null)
            {
                return;
            }

            // Do the setup here in Update so we can just do a Singleton lookup on Gameboard, and not worry about race-conditions in using Start.
            if (!setupCompleted)
            {
                if (Gameboard.Instance.companionController.isConnected)
                {
                    Gameboard.Instance.companionController.DiceRolled += CompanionController_DiceRolled;
                    setupCompleted = true;

                    Debug.Log("Gameboard Dice Roll tool is ready!");
                }
            }
        }

        /// <summary>
        /// Event receiver for Dice Roll events from the Companions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void CompanionController_DiceRolled(object sender, CompanionDiceRollEventArgs eventArgs)
        {
            diceRollUpdates.Enqueue(eventArgs);
        }

        /// <summary>
        /// Returns a Queue of CompanionDiceRollEventArgs in the order they were received by the game. This also empties the Queue in the DiceRollToll,
        /// so the next time you call DrainQueue it will be the next batch of received CompanionDiceRollEventArgs.
        /// </summary>
        /// <returns></returns>
        public Queue<CompanionDiceRollEventArgs> DrainQueue()
        {
            lock (diceRollUpdates)
            {
                Queue<CompanionDiceRollEventArgs> drainedQueue = new Queue<CompanionDiceRollEventArgs>(diceRollUpdates);
                diceRollUpdates.Clear();
                return drainedQueue;
            }
        }
    }
}