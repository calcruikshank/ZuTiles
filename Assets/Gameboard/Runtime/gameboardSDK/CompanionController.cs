using Gameboard.EventArgs;
using Gameboard.Helpers;
using Gameboard.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Gameboard.DataTypes;

namespace Gameboard.Companion
{
    public class CompanionController : ICompanionController
    {
        public ICompanionCommunicationsUtility companionCommunications { get; private set; }
        public ICompanionHandlerUtility companionHandler { get; private set; }

        public event EventHandler ConnectedToCompanionServer;
        public event EventHandler DisconnectedFromCompanionServer;

        public bool isConnected { get { return companionCommunications == null ? false : companionCommunications.isConnected; } }

        #region Setup & Management
        public void InjectDependencies(ICompanionCommunicationsUtility companionComUtility, ICompanionHandlerUtility inHandler, IJsonUtility inJsonUtility, IGameboardConfig inConfig)
        {
            companionHandler = inHandler;
            companionHandler.InjectDependencies(companionComUtility, inJsonUtility, inConfig);
            SetupHandlerListeners();

            companionCommunications = companionComUtility;
        }

        public async Task<bool> EnableAndConnect()
        {
            bool connectionSuccessful = await companionCommunications.AsyncConnectToCompanionServer();
            return connectionSuccessful;
        }

        public void ShutDownCompanionController() 
        {
            companionCommunications?.DisconnectFromCompanionServer();
        }

        public void ClearQueueForPlayer(string inUserId)
        {
            companionHandler?.ClearQueueForPlayer(inUserId);
        }

        void SetupHandlerListeners()
        {
            companionHandler.UserPresenceChangedEvent += DigestEvent_UserPresenceUpdated;
            companionHandler.DiceRolledEvent += DigestEvent_DiceRollReceived;
            companionHandler.CardPlayedEvent += DigestEvent_CardPlayedReceived;
            companionHandler.CompanionButtonPressedEvent += DigestEvent_CompanionButtonPressed;
            companionHandler.CompanionCardsButtonPressedEvent += DigestEvent_CompanionCardsButtonPressed;
            Debug.Log("--- CompanionCardsButtonPressedEvent ASSIGNED!");
        }

        void RemoveHandlerListeners()
        {
            companionHandler.UserPresenceChangedEvent -= DigestEvent_UserPresenceUpdated;
            companionHandler.DiceRolledEvent -= DigestEvent_DiceRollReceived;
            companionHandler.CardPlayedEvent -= DigestEvent_CardPlayedReceived;
            companionHandler.CompanionButtonPressedEvent -= DigestEvent_CompanionButtonPressed;
            companionHandler.CompanionCardsButtonPressedEvent -= DigestEvent_CompanionCardsButtonPressed;
        }

        void CompanionServerConnected(object sender, CompanionCommunicationsEventArg eventArgs)
        {
            ConnectedToCompanionServer?.Invoke(this, null);
        }

        void CompanionServerDisconnected(object sender, CompanionCommunicationsEventArg eventArgs)
        {
            DisconnectedFromCompanionServer?.Invoke(this, null);
        }
        #endregion

        #region Device Events
        /// <summary>
        /// Displays a system-level popup on the target user's Companion device for the requested number of seconds.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="textToDisplay"></param>
        /// <param name="timeInSecondsToDisplay"></param>
        public async Task<CompanionMessageResponseArgs> DisplaySystemPopup(string userId, string textToDisplay, float timeInSecondsToDisplay)
        {
            return await companionHandler.DisplaySystemPopup(1, userId, textToDisplay, timeInSecondsToDisplay);
        }

        /// <summary>
        /// Entirely wipes the PlayPanel of a companion clean, removing all data.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> ResetPlayPanel(string userId)
        {
            return await companionHandler.ResetPlayPanel(1, userId);
        }
        #endregion

        #region User Presence
        public event EventHandler<GameboardUserPresenceEventArgs> UserPresenceUpdated;

        public async Task<CompanionUserPresenceEventArgs> FetchUserPresence()
        {
            CompanionUserPresenceEventArgs responseArgs = await companionHandler.GetUserPresenceList(1);
            return responseArgs;
        }

        void DigestEvent_UserPresenceUpdated(GameboardUserPresenceEventArgs inEventArgs)
        {
            Debug.Log("--- DIGESTING USER PRESENCE CHANGE EVENT");
            UserPresenceUpdated?.Invoke(this, inEventArgs);
        }
        #endregion

        #region Asset Loading
        /// <summary>
        /// Async method that loads a Texture asset onto a Companion.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="textureAsset"></param>
        /// <returns>CompanionCreateObjectEventArgs</returns>
        public async Task<CompanionCreateObjectEventArgs> LoadAsset(string userId, byte[] textureBytes, string guid = "")
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.LoadAsset(1, userId, textureBytes, guid);

            return responseArgs;
        }

        /// <summary>
        /// Async method that loads an FBX asset onto a Companion.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="meshObject"></param>
        /// <returns>CompanionCreateObjectEventArgs</returns>
        public async Task<CompanionCreateObjectEventArgs> LoadAsset(string userId, Mesh meshObject)
        {
            byte[] meshByteArray = GameboardHelperMethods.ObjectToByteArray(meshObject);
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.LoadAsset(1, userId, meshByteArray, meshObject.name);

            return responseArgs;
        }

        /// <summary>
        /// Unloads an asset from memory on a Companion.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assetUID"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> DeleteAsset(string userId, string assetUID)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.DeleteAsset(1, userId, assetUID);
            return responseArgs;
        }
        #endregion

        // TODO: move to controllers
        #region Model Objects
        /// <summary>
        /// Async method that adjusts the color tint applied to the texture on the model.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="objectUid"></param>
        /// <param name="tintColor"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> TintObject(string userId, string objectUid, Color tintColor)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.TintModelWithRGBColor(1, userId, objectUid, tintColor);
            return responseArgs;
        }
        #endregion

        // TODO: move to controllers
        #region Object Management
        /// <summary>
        /// Changes the display state of an object between Hidden and Displayed.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="objectId"></param>
        /// <param name="newDisplayState"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> ChangeObjectDisplayState(string userId, string objectId, ObjectDisplayStates newDisplayState)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.changeObjectDisplayState(1, userId, objectId, newDisplayState);
            return responseArgs;
        }
        #endregion

        // TODO: move to controllers
        #region Containers
        public async Task<CompanionCreateObjectEventArgs> CreateContainer(string userId, CompanionContainerSortingTypes sortingType)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateContainer(1, userId, sortingType);
            return responseArgs;
        }

        public async Task<CompanionMessageResponseArgs> PlaceObjectInContainer(string userId, string containerId, string objectToPlaceId)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.PlaceObjectInContainer(1, userId, containerId, objectToPlaceId);
            return responseArgs;
        }

        public async Task<CompanionMessageResponseArgs> RemoveObjectFromContainer(string userId, string containerId, string objectToRemoveId)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.RemoveObjectFromContainer(1, userId, containerId, objectToRemoveId);
            return responseArgs;
        }
        #endregion

        #region Companion Buttons
        public async Task<CompanionMessageResponseArgs> SetCompanionButtonValues(string userId, string buttonId, string inButtonLabelText, string inButtonCallback)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.SetCompanionButtonValues(1, userId, buttonId, inButtonLabelText, inButtonCallback, null);
            return responseArgs;
        }
        #endregion

        #region Cards
        public event EventHandler<CompanionCardPlayedEventArgs> CardPlayed;

        /// <summary>
        /// Creates a card object on a Companion, using the requested Card Texture.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="uidOfTextureForCard"></param>
        /// <returns>CompanionCreateObjectEventArgs</returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionCard(string userId, string cardId, string frontCardTextureUid, string backCardTextureUid, int inTextureWidth, int inTextureHeight)
        {
            CompanionCreateObjectEventArgs cardResponse = await companionHandler.CreateCompanionCard(1, userId, cardId, frontCardTextureUid, backCardTextureUid, inTextureWidth, inTextureHeight);
            return cardResponse;
        }

        /// <summary>
        /// Sets the facing direction of the requested card on a Companion
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cardId"></param>
        /// <param name="newFacingDirection"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> SetFacingDirectionOfCard(string userId, string cardId, CardFacingDirections newFacingDirection)
        {
            CompanionMessageResponseArgs cardResponse = await companionHandler.SetFacingDirectionOfCard(1, userId, cardId, newFacingDirection);
            return cardResponse;
        }

        private void DigestEvent_CardPlayedReceived(GameboardCardPlayedEventArgs cardPlayedEventArgs)
        {
            if (CardPlayed != null)
            {
                CompanionCardPlayedEventArgs eventArgs = new CompanionCardPlayedEventArgs()
                {
                    userId = cardPlayedEventArgs.userId,
                    cardId = cardPlayedEventArgs.cardId
                };
                CardPlayed(this, eventArgs);
            }
        }
        #endregion

        #region Dice Roll Manager
        /// <summary>
        /// Event handler for when a Dice Roll occurs on a Companion.
        /// </summary>
        public event EventHandler<CompanionDiceRollEventArgs> DiceRolled;

        /// <summary>
        /// Initiates a Dice Roll on a Companion.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="diceSizeArray"></param>
        /// <param name="addedModifier"></param>
        /// <param name="diceTintColor"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> RollDice(string userId, int[] diceSizeArray, int addedModifier, Color diceTintColor, string inDiceNotation)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.RollDice(1, userId, diceSizeArray, addedModifier, diceTintColor, inDiceNotation);
            return responseArgs;
        }

        /// <summary>
        /// Digests a Dice Roll. Internal method for the CompanionController class.
        /// </summary>
        /// <param name="userIdWhoRolled"></param>
        /// <param name="diceSizeArray"></param>
        /// <param name="addedModifier"></param>
        private void DigestEvent_DiceRollReceived(GameboardDiceRolledEventArgs diceRolledEventArgs)
        {
            if(DiceRolled != null)
            {
                CompanionDiceRollEventArgs eventArgs = new CompanionDiceRollEventArgs()
                {
                    diceSizesRolledList = diceRolledEventArgs.diceSizesRolledList,
                    addedModifier = diceRolledEventArgs.addedModifier,
                    diceNotation = diceRolledEventArgs.diceNotation,
                    ownerId = diceRolledEventArgs.ownerId,
                };

                DiceRolled(this, eventArgs);
            }
        }
        #endregion

        // TODO: move to controllers
        #region Companion Mats
        /// <summary>
        /// Event handler for when a Mat Zone is triggered by dropping a Companion Object in it.
        /// </summary>
        public event EventHandler<CompanionMatZoneEventArgs> CompanionMatZoneTriggered;

        /// <summary>
        /// Creates a Companion Mat on a Companion
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="backgroundTextureUid"></param>
        /// <returns>CompanionObjectResponse</returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionMat(string userId, string backgroundTextureUid)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateCompanionMat(1, userId, backgroundTextureUid);
            return responseArgs;
        }

        /// <summary>
        /// Adds a Zone to a Companion Mat on a Companion device.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="matId"></param>
        /// <param name="percentileCenterOfZone"></param>
        /// <param name="percentileSizeOfZone"></param>
        /// <returns>CompanionCreateObjectEventArgs</returns>
        public async Task<CompanionCreateObjectEventArgs> AddZoneToCompanionMat(string userId, string matId, Vector2 percentileCenterOfZone, Vector2 percentileSizeOfZone)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.AddZoneToMat(1, userId, matId, percentileCenterOfZone, percentileSizeOfZone);
            return responseArgs;
        }

        /// <summary>
        /// Adds the requested object to the requested Companion Mat.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="matId"></param>
        /// <param name="objectToAddId"></param>
        /// <param name="percentilePosition"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> AddObjectToCompanionMat(string userId, string matId, string objectToAddId, Vector2 percentilePosition)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.AddObjectToCompanionMat(1, userId, matId, objectToAddId, percentilePosition);
            return responseArgs;
        }

        /// <summary>
        /// Removes the requested object from the requested companion mat
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="matId"></param>
        /// <param name="objectToRemoveId"></param>
        /// <returns>CompanionMessageResponseArgs</returns>
        public async Task<CompanionMessageResponseArgs> RemoveObjectFromCompanionMat(string userId, string matId, string objectToRemoveId)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.RemoveObjectFromCompanionMat(1, userId, matId, objectToRemoveId);
            return responseArgs;
        }

        /// <summary>
        /// Digests a Mat Zone trigger.
        /// </summary>
        /// <param name="userIdWhoTriggered"></param>
        /// <param name="uidOfMatZone"></param>
        /// <param name="uidOfObjectDropped"></param>
        private void HandleCompanionMatZoneTriggered(string userIdWhoTriggered, string uidOfMatZone, string uidOfObjectDropped)
        {
            if(CompanionMatZoneTriggered != null)
            {
                CompanionMatZoneEventArgs eventArgs = new CompanionMatZoneEventArgs()
                {
                    userIdWhoTriggeredZone = userIdWhoTriggered,
                    matZoneIdTriggered = uidOfMatZone,
                    objectIdDroppedInZone = uidOfObjectDropped
                };

                CompanionMatZoneTriggered(this, eventArgs);
            }
        }
        #endregion

        
        #region Companion Hand Display
        /// <summary>
        /// Creates a Hand Display on the companion for the requested user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>CompanionCreateObjectEventArgs</returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionHandDisplay(string userId)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateCompanionHandDisplay(1, userId);
            return responseArgs;
        }

        /// <summary>
        /// Adds the requested Card to the requested Companion Hand for a specific User.
        /// </summary>
        /// <param name="usedId"></param>
        /// <param name="handDisplayId"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> AddCardToHandDisplay(string userId, string handDisplayId, string cardId)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.AddCardToHand(1, userId, handDisplayId, cardId);
            return responseArgs;
        }

        /// <summary>
        /// Removes the requested card from the requested Hand Display for a specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handDisplayId"></param>
        /// <param name="cardId"></param>
        /// <returns>CompanionResponse</returns>
        public async Task<CompanionMessageResponseArgs> RemoveCardFromHandDisplay(string userId, string handDisplayId, string cardId)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.RemoveCardFromHand(1, userId, handDisplayId, cardId);
            return responseArgs;
        }

        /// <summary>
        /// Removes all cards from the requested Hand Display for a specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handDisplayId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> RemoveAllCardsFromHandDisplay(string userId, string handDisplayId)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.RemoveAllCardsFromHand(1, userId, handDisplayId);
            return responseArgs;
        }

        /// <summary>
        /// Changes the Hand Display shown for a specific user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handDisplayId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> ShowCompanionHandDisplay(string userId, string handDisplayId)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.ShowCompanionHandDisplay(1, userId, handDisplayId);
            return responseArgs;
        }
        #endregion

        // TODO: move to controllers
        #region Companion Dialogs
        /// <summary>
        /// Creates a new Dialog on a specific Companion.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>CompanionObjectResponse</returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionDialog(string userId)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateCompanionDialog(1, userId);
            return responseArgs;
        }

        /// <summary>
        /// Adds an object to a specific Companion Dialog.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dialogId"></param>
        /// <param name="objectId"></param>
        /// <param name="attachPercentilePosition"></param>
        /// <returns>CompanionResponse</returns>
        public async Task<CompanionMessageResponseArgs> AddObjectToDialog(string userId, string dialogId, string objectId, Vector2 attachPercentilePosition)
        {
            CompanionMessageResponseArgs responseArgs = await companionHandler.AddObjectToDialog(1, userId, dialogId, objectId, attachPercentilePosition);
            return responseArgs;
        }
        #endregion

        #region Companion Button Objects
        /// <summary>
        /// Event handler to listen for when a Companion Button is pressed.
        /// </summary>
        public event EventHandler<GameboardCompanionButtonPressedEventArgs> CompanionButtonPressed;

        /// <summary>
        /// Event handler to listen for when a Companion Cards Button is pressed.
        /// </summary>
        public event EventHandler<GameboardCompanionCardsButtonPressedEventArgs> CompanionCardsButtonPressed;

        /// <summary>
        /// Creates a new Companion Button on a Companion device using the requested textures for the Down and Up visual states.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="buttonText"></param>
        /// <param name="buttonDownTextureId"></param>
        /// <param name="buttonUpTextureId"></param>
        /// <returns></returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionButton(string userId, string buttonText, string buttonDownTextureId, string buttonUpTextureId)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateCompanionButton(1, userId, buttonText, buttonDownTextureId, buttonUpTextureId);
            return responseArgs;
        }

        void DigestEvent_CompanionButtonPressed(GameboardCompanionButtonPressedEventArgs inEventArgs)
        {
            Debug.Log("Companion button pressed");
            CompanionButtonPressed?.Invoke(this, inEventArgs);
        }

        void DigestEvent_CompanionCardsButtonPressed(GameboardCompanionCardsButtonPressedEventArgs inEventArgs)
        {
            Debug.Log("Companion button pressed");
            Debug.Log("--- DigestEvent_CompanionCardsButtonPressed");
            CompanionCardsButtonPressed?.Invoke(this, inEventArgs);
        }
        #endregion

        // TODO: move to controllers
        #region Tickbox Objects
        /// <summary>
        /// Event handler for when a Tickbox value changes.
        /// </summary>
        public event EventHandler<CompanionTickboxChangedEventArgs> CompanionTickboxStateChanged;

        /// <summary>
        /// Creates a Tickbox Object on a Companion device, and applies the initial state.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="onTextureId"></param>
        /// <param name="offTextureId"></param>
        /// <param name="initialState"></param>
        /// <param name="isInputLocked"></param>
        /// <returns>CompanionCreateObjectEventArgs</returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionTickbox(string userId, string onTextureId, string offTextureId, TickboxStates initialState, bool isInputLocked)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateCompanionTickbox(1, userId, onTextureId, offTextureId, initialState, isInputLocked);
            return responseArgs;
        }

        /// <summary>
        /// Internal method to digest the Tickbox Changed event.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="changedTickboxId"></param>
        /// <param name="tickboxState"></param>
        private void DigestCompanionTickboxChangedEvent(string userId, string changedTickboxId, TickboxStates tickboxState)
        {
            if(CompanionTickboxStateChanged != null)
            {
                CompanionTickboxChangedEventArgs eventArgs = new CompanionTickboxChangedEventArgs()
                {
                    userIdOfChangedTickbox = userId,
                    changedTickboxId = changedTickboxId,
                    newTickboxState = tickboxState
                };

                CompanionTickboxStateChanged(this, eventArgs);
            }
        }
        #endregion

        // TODO: move to controllers
        #region Label Objects
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionLabel(string userId, string labelString)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateCompanionLabel(1, userId, labelString);
            return responseArgs;
        }
        #endregion

        // TODO: move to controllers
        #region Dropdown List Objects
        /// <summary>
        /// Event Handler for when the selected index in a Dropdown changes
        /// </summary>
        public event EventHandler<CompanionDropdownSelectionChangedEventArgs> CompanionDropdownSelectionChanged;

        /// <summary>
        /// Creates a Dropdown object on a Companion device
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderedStringList"></param>
        /// <param name="defaultSelectedIndex"></param>
        /// <returns>CompanionCreateObjectEventArgs</returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionDropdown(string userId, List<string> orderedStringList, int defaultSelectedIndex)
        {
            CompanionCreateObjectEventArgs responseArgs = await companionHandler.CreateCompanionDropdown(1, userId, orderedStringList, defaultSelectedIndex);
            return responseArgs;
        }

        /// <summary>
        /// Internal method for handling the digestion of Dropdown selection changes
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="dropdownId"></param>
        /// <param name="selectionIndex"></param>
        void DigestDropdownSelectionChangedEvent(string userId, string dropdownId, int selectionIndex)
        {
            if(CompanionDropdownSelectionChanged != null)
            {
                CompanionDropdownSelectionChangedEventArgs eventArgs = new CompanionDropdownSelectionChangedEventArgs()
                {
                    userIdOfChangedDropdown = userId,
                    changedDropdownId = dropdownId,
                    newSelectionIndex = selectionIndex
                };

                CompanionDropdownSelectionChanged(this, eventArgs);
            }
        }
        #endregion

        #region Drawer Actions
        /// <summary>
        /// Hides the Gameboard System Drawers.
        /// </summary>
        public void SetDrawersHidden()
        {
            Drawers_SetVisibility(false);
        }

        /// <summary>
        /// Displays the Gameboard System Drawers.
        /// </summary>
        public void SetDrawersVisible()
        {
            Drawers_SetVisibility(true);
        }

        private void Drawers_SetVisibility(bool inVisibleState)
        {
            AndroidJavaClass drawerHelper = Gameboard.Instance.config.drawerHelper;
            if (drawerHelper == null)
            {
                GameboardLogging.LogMessage("Drawer Helper is not available. Unable to set drawers as " + (inVisibleState ? "Visible" : "Hidden") + ".", GameboardLogging.MessageTypes.Warning);
                return;
            }

            AndroidApplicationContext context = Gameboard.Instance.config.androidApplicationContext;
            if (drawerHelper == null)
            {
                GameboardLogging.LogMessage("Android Application Context is not available. Unable to set drawers as " + (inVisibleState ? "Visible" : "Hidden") + ".", GameboardLogging.MessageTypes.Warning);
                return;
            }

            drawerHelper.CallStatic("setDrawerVisibility", context.GetNativeContext(), inVisibleState);
        }
        #endregion

        #region Editor Only Actions
#if UNITY_EDITOR
        public async Task<CompanionMessageResponseArgs> UserLeft(string userId)
        {
            return await companionHandler.UserLeft(1, userId);
        }
#endif
        #endregion
    }
}