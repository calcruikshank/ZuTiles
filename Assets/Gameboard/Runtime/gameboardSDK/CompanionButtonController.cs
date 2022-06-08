using System.Collections.Generic;
using UnityEngine;
using Gameboard.EventArgs;
using System.Threading.Tasks;
using Gameboard.Objects;

namespace Gameboard
{
    [RequireComponent(typeof(Gameboard))]
    [RequireComponent(typeof(UserPresenceController))]
    [RequireComponent(typeof(AssetController))]
    public class CompanionButtonController : MonoBehaviour
    {
        public delegate void OnCompanionButtonPressed(GameboardCompanionButtonPressedEventArgs companionButtonEvent);
        public event OnCompanionButtonPressed CompanionButtonPressed;

        private Queue<GameboardCompanionButtonPressedEventArgs> eventQueue = new Queue<GameboardCompanionButtonPressedEventArgs>(20);

        private Gameboard gameboard => Gameboard.Instance;

        private void Awake()
        {
            gameboard.GameboardInitializationCompleted += OnGameboardInit;
            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
        }

        void Update()
        {
            if (eventQueue.Count > 0)
            {
                Queue<GameboardCompanionButtonPressedEventArgs> clonedEventQueue = new Queue<GameboardCompanionButtonPressedEventArgs>(eventQueue);
                eventQueue.Clear();

                while (clonedEventQueue.Count > 0)
                {
                    GameboardCompanionButtonPressedEventArgs eventToProcess = clonedEventQueue.Dequeue();

                    CompanionButtonPressed?.Invoke(eventToProcess);
                }
                clonedEventQueue.Clear();
            }
        }

        void OnDestroy()
        {
            gameboard.GameboardInitializationCompleted -= OnGameboardInit;
            gameboard.GameboardShutdownBegun -= OnGameboardShutdown;
            gameboard.services.companionHandler.CompanionButtonPressedEvent -= CompanionButtonPressedEvent;
        }

        private void OnGameboardInit()
        {
            gameboard.services.companionHandler.CompanionButtonPressedEvent += CompanionButtonPressedEvent;
        }

        private void OnGameboardShutdown()
        {
            gameboard.services.companionHandler.CompanionButtonPressedEvent -= CompanionButtonPressedEvent;
        }

        /// <summary>
        /// Creates a new Companion Button on a Companion device
        /// </summary>
        /// <param name="userId">userId associated with the companion device</param>
        /// <param name="buttonId">buttonId associated with the Companion Template, custom position changes if 1-3 are set</param>
        /// <param name="inButtonLabelText">a label set on the button visible to the companion user</param>
        /// <param name="inButtonCallback">a callback string to be sent back when the user clicks the button</param>
        /// <param name="assetId">optional assetId to use for the button texture, a default texture will be used if one is not provided</param>
        /// <returns>Task<CompanionMessageResponseArgs></returns>
        /// <remarks>
        /// By default, the button is not visible on the companion app, it must be enabled with SetCompanionButtonVisiblity
        /// 
        /// There are currently 3 presets, buttonId = "1", "2", and "3"
        /// "1" will appear in the bottom center of the screen
        /// "2" will appear on the bottom left of the screen
        /// "3" will appear on the bottom right of the screen
        /// 
        /// If any of the preset ids are set, they will override other buttons.
        /// 
        /// If we set buttons with the ids { "testing0", "testing1", "testing2", "3", "1", "2", "testing3", "testing4", "testing5" }
        ///     only "3", "1", "2" will be displayed with "1" in the center, "2" on the left, and "3" on the right.
        ///     
        /// If we set buttons with the ids { "testing0", "testing1", "testing2" }
        ///     we will 3 see buttons with testing0 in the center, testing1 on the left, and testing2 on the right.
        /// 
        /// If we set buttons with the ids { "2", "3" }
        ///     we will see 2 buttons with "2" on the left, and "3" on the right.
        /// 
        /// The buttonId can be set to something other than 1, 2, and 3, but if 1, 2, or 3 are present they will override other buttons previously set.
        /// </remarks>
        public async Task<CompanionMessageResponseArgs> SetCompanionButtonValues(string userId, string buttonId, string inButtonLabelText, string inButtonCallback, string assetId = null)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetCompanionButtonValues(1, userId, buttonId, inButtonLabelText, inButtonCallback, assetId);
            return responseArgs;
        }

        /// <summary>
        /// Set the visibility of a Companion Button on a Companion device.
        /// </summary>
        /// <param name="userId">userId associated with the companion device</param>
        /// <param name="buttonId">buttonId associated with the Companion Template, custom position changes if 1-3 are set</param>
        /// <param name="visible">true if button should be shown on the companion app</param>
        /// <returns>Task<CompanionMessageResponseArgs></returns>
        public async Task<CompanionMessageResponseArgs> SetCompanionButtonVisiblity(string userId, string buttonId, bool visible)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.changeObjectDisplayState(1, userId, buttonId, visible ? DataTypes.ObjectDisplayStates.Displayed : DataTypes.ObjectDisplayStates.Hidden);
            return responseArgs;
        }

        /// <summary>
        /// Handle the companion button press events generated by game/companionButtonPressed
        /// </summary>
        /// <param name="inEventArgs"></param>
        private void CompanionButtonPressedEvent(GameboardCompanionButtonPressedEventArgs inEventArgs)
        {
            eventQueue.Enqueue(inEventArgs);
        }
    }

}
