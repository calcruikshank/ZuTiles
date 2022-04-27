using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Gameboard.Utilities;
using static Gameboard.DataTypes;
using Gameboard.EventArgs;

namespace Gameboard.Companion
{
    public interface ICompanionController
    {
        event EventHandler ConnectedToCompanionServer;
        event EventHandler DisconnectedFromCompanionServer;

        ICompanionCommunicationsUtility companionCommunications { get; }
        ICompanionHandlerUtility companionHandler { get; }

        void InjectDependencies(ICompanionCommunicationsUtility companionComUtility, ICompanionHandlerUtility inHandler, IJsonUtility inJsonUtility, IGameboardConfig inConfig);

        Task<bool> EnableAndConnect();

        Task<CompanionMessageResponseArgs> DisplaySystemPopup(string userId, string textToDisplay, float timeInSecondsToDisplay);

        #region Asset Management
        Task<CompanionCreateObjectEventArgs> LoadAsset(string userId, byte[] textureBytes, string guid = "");
        Task<CompanionCreateObjectEventArgs> LoadAsset(string userId, Mesh meshAsset);
        Task<CompanionMessageResponseArgs> DeleteAsset(string userId, string assetUID);
        #endregion

        #region User Presence
        event EventHandler<GameboardUserPresenceEventArgs> UserPresenceUpdated;

        Task<CompanionUserPresenceEventArgs> FetchUserPresence();
        #endregion

        #region Model Objects
        Task<CompanionMessageResponseArgs> TintObject(string userId, string objectUid, Color tintColor);
        #endregion

        #region Object Management
        Task<CompanionMessageResponseArgs> ChangeObjectDisplayState(string userId, string objectId, ObjectDisplayStates newDisplayState);
        #endregion

        #region Container Objects
        Task<CompanionCreateObjectEventArgs> CreateContainer(string userId, CompanionContainerSortingTypes sortingType);
        Task<CompanionMessageResponseArgs> PlaceObjectInContainer(string userId, string containerId, string objectToPlaceId);
        Task<CompanionMessageResponseArgs> RemoveObjectFromContainer(string userId, string containerId, string objectToRemoveId);
        #endregion

        #region Card Objects
        Task<CompanionCreateObjectEventArgs> CreateCompanionCard(string userId, string cardId, string frontCardTextureUid, string backCardTextureUid, int inTextureWidth, int inTextureHeight);
        Task<CompanionMessageResponseArgs> SetFacingDirectionOfCard(string userId, string cardId, CardFacingDirections newFacingDirection);
        #endregion

        #region Companion Button Objects
        event EventHandler<GameboardCompanionButtonPressedEventArgs> CompanionButtonPressed;
        event EventHandler<GameboardCompanionCardsButtonPressedEventArgs> CompanionCardsButtonPressed;
        Task<CompanionCreateObjectEventArgs> CreateCompanionButton(string userId, string buttonText, string buttonDownTextureId, string buttonUpTextureId);
        Task<CompanionMessageResponseArgs> SetCompanionButtonValues(string userId, string buttonId, string inButtonLabelText, string inButtonCallback);
        #endregion

        #region Dice
        // Nothing Dice Related is in Phase 1
        #endregion

        #region Dice Roll Manager
        event EventHandler<CompanionDiceRollEventArgs> DiceRolled;
        Task<CompanionMessageResponseArgs> RollDice(string userId, int[] diceSizeArray, int addedModifier, Color diceTintColor, string inDiceNotation);
        #endregion

        #region Companion Mat
        event EventHandler<CompanionMatZoneEventArgs> CompanionMatZoneTriggered;

        Task<CompanionCreateObjectEventArgs> CreateCompanionMat(string userId, string backgroundTextureUid);
        Task<CompanionCreateObjectEventArgs> AddZoneToCompanionMat(string userId, string matId, Vector2 percentileCenterOfZone, Vector2 percentileSizeOfZone);
        Task<CompanionMessageResponseArgs> AddObjectToCompanionMat(string userId, string matId, string objectToAddId, Vector2 percentilePosition);
        Task<CompanionMessageResponseArgs> RemoveObjectFromCompanionMat(string userId, string matId, string objectToRemoveId);
        #endregion

        #region Companion Hand Display
        Task<CompanionCreateObjectEventArgs> CreateCompanionHandDisplay(string userId);
        Task<CompanionMessageResponseArgs> AddCardToHandDisplay(string userId, string handDisplayId, string cardId);
        Task<CompanionMessageResponseArgs> RemoveCardFromHandDisplay(string userId, string handDisplayId, string cardId);
        Task<CompanionMessageResponseArgs> RemoveAllCardsFromHandDisplay(string userId, string handDisplayId);
        Task<CompanionMessageResponseArgs> ShowCompanionHandDisplay(string userId, string handDisplayId);
        #endregion

        #region Companion Dialogs
        Task<CompanionCreateObjectEventArgs> CreateCompanionDialog(string userId);
        Task<CompanionMessageResponseArgs> AddObjectToDialog(string userId, string dialogId, string objectId, Vector2 attachPercentilePosition);
        #endregion

        #region Tickbox Objects
        event EventHandler<CompanionTickboxChangedEventArgs> CompanionTickboxStateChanged;
        Task<CompanionCreateObjectEventArgs> CreateCompanionTickbox(string userId, string onTextureId, string offTextureId, TickboxStates initialState, bool isInputLocked);
        #endregion

        #region Label Objects
        Task<CompanionCreateObjectEventArgs> CreateCompanionLabel(string userId, string labelString);
        #endregion

        #region Dropdown List Objects
        event EventHandler<CompanionDropdownSelectionChangedEventArgs> CompanionDropdownSelectionChanged;
        Task<CompanionCreateObjectEventArgs> CreateCompanionDropdown(string userId, List<string> orderedStringList, int defaultSelectedIndex);
        #endregion

        #region Drawer Actions
        void SetDrawersHidden();
        void SetDrawersVisible();
        #endregion

        #region Editor Only Actions
#if UNITY_EDITOR
        Task<CompanionMessageResponseArgs> UserLeft(string userId);
#endif
        #endregion
    }
}

