using Gameboard.EventArgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static Gameboard.DataTypes;

namespace Gameboard.Utilities
{
    public delegate void IncomingEventDelegate<T>(T eventArgs);
    public interface ICompanionHandlerUtility
    {
        ICompanionCommunicationsUtility communicationsUtility { get; }
        IGameboardConfig gameboardConfig { get; }

        void InjectDependencies(ICompanionCommunicationsUtility inCommunications, IJsonUtility inJsonUtility, IGameboardConfig inGameboardConfig);


        // Gameboard Originating Events
        Task<CompanionUserPresenceEventArgs> GetUserPresenceList(int versionTag);
        void AddUser(int versionTag);
        void ClearQueueForPlayer(string inUserId);

#if UNITY_EDITOR
        Task<CompanionUserPresenceEventArgs> UserLeft(int versionTag, string userId);
#endif

        Task<CompanionMessageResponseArgs> DisplaySystemPopup(int versionTag, string userId, string textToDisplay, float timeInSecondsToDisplay);
        Task<CompanionCreateObjectEventArgs> LoadAsset(int versionTag, string userId, byte[] byteArray, string guid = "");
        Task<CompanionMessageResponseArgs> DeleteAsset(int versionTag, string userId, string assetId);
        Task<CompanionMessageResponseArgs> TintModelWithRGBColor(int versionTag, string userId, string objectId, Color rgbColor);
        Task<CompanionCreateObjectEventArgs> CreateContainer(int versionTag, string userId, CompanionContainerSortingTypes sortingType);
        Task<CompanionMessageResponseArgs> PlaceObjectInContainer(int versionTag, string userId, string containerId, string objectToPlaceId);
        Task<CompanionMessageResponseArgs> RemoveObjectFromContainer(int versionTag, string userId, string containerId, string objectToRemoveId);
        Task<CompanionMessageResponseArgs> changeObjectDisplayState(int versionTag, string userId, string objectId, ObjectDisplayStates newDisplayState);
        Task<CompanionCreateObjectEventArgs> CreateCompanionCard(int versionTag, string userId, string cardId, string frontTextureId, string backTextureId, int inTextureWidth, int inTextureHeight);
        Task<CompanionMessageResponseArgs> SetFacingDirectionOfCard(int versionTag, string userId, string cardId, CardFacingDirections facingDirection);
        Task<CompanionMessageResponseArgs> RollDice(int versionTag, string userId, int[] diceSizesToRoll, int inAddedModifier, Color diceTintColor, string inDiceNotation);
        Task<CompanionCreateObjectEventArgs> CreateCompanionMat(int versionTag, string userId, string backgroundTextureUid);
        Task<CompanionCreateObjectEventArgs> AddZoneToMat(int versionTag, string userId, string companionMatId, Vector2 zoneCenter, Vector2 sizeOfZone);
        Task<CompanionMessageResponseArgs> AddObjectToCompanionMat(int versionTag, string userId, string companionMatId, string objectToAddId, Vector2 positionOfObject);
        Task<CompanionMessageResponseArgs> RemoveObjectFromCompanionMat(int versionTag, string userId, string companionMatId, string objectToRemoveId);
        Task<CompanionCreateObjectEventArgs> CreateCompanionHandDisplay(int versionTag, string userId);
        Task<CompanionMessageResponseArgs> AddCardToHand(int versionTag, string userId, string cardHandDisplayId, string cardId);
        Task<CompanionMessageResponseArgs> RemoveCardFromHand(int versionTag, string userId, string cardHandDisplayId, string cardId);
        Task<CompanionMessageResponseArgs> RemoveAllCardsFromHand(int versionTag, string userId, string cardHandDisplayId);
        Task<CompanionMessageResponseArgs> ShowCompanionHandDisplay(int versionTag, string userId, string cardHandDisplayId);
        Task<CompanionCreateObjectEventArgs> CreateCompanionDialog(int versionTag, string userId);
        Task<CompanionMessageResponseArgs> AddObjectToDialog(int versionTag, string userId, string dialogId, string objectToAddId, Vector2 attachPosition);
        Task<CompanionCreateObjectEventArgs> CreateCompanionButton(int versionTag, string userId, string buttonText, string inButtonDownTextureId, string inButtonUpTextureId);
        Task<CompanionCreateObjectEventArgs> CreateCompanionTickbox(int versionTag, string userId, string onTextureId, string offTextureId, TickboxStates startingState, bool isInputLocked);
        Task<CompanionCreateObjectEventArgs> CreateCompanionLabel(int versionTag, string userId, string textToDisplay);
        Task<CompanionCreateObjectEventArgs> CreateCompanionDropdown(int versionTag, string userId, List<string> orderedStringList, int defaultIndex);
        Task<CompanionMessageResponseArgs> SetCompanionButtonValues(int versionTag, string userId, string buttonId, string inButtonLabelText, string inButtonCallback);
        Task<CompanionMessageResponseArgs> ResetPlayPanel(int versionTag, string userId);


        // Companion Originating Events
        void StartGame(int versionTag);
        void EndGame(int versionTag);


        IncomingEventDelegate<GameboardDropdownChangedEventArgs> DropdownSelectionChangedEvent { get; }
        IncomingEventDelegate<GameboardMatZoneDropOccuredEventArgs> MatZoneDropOccuredEvent { get; }
        IncomingEventDelegate<GameboardTickBoxStateChangedEventArgs> TickboxStateChangedEvent { get; }
        IncomingEventDelegate<GameboardDiceRolledEventArgs> DiceRolledEvent { get; set; }
        IncomingEventDelegate<GameboardCardPlayedEventArgs> CardPlayedEvent { get; set; }
        IncomingEventDelegate<GameboardUserPresenceEventArgs> UserPresenceChangedEvent { get; set; }
        IncomingEventDelegate<GameboardCompanionButtonPressedEventArgs> CompanionButtonPressedEvent { get; set; }
        IncomingEventDelegate<GameboardCompanionCardsButtonPressedEventArgs> CompanionCardsButtonPressedEvent { get; set; }

        Dictionary<string, Queue<CompanionQueuedEvent>> companionEventQueueDict { get; set; }
        Dictionary<string, CompanionQueuedEvent> companionProcessingDict { get; set; }

        Dictionary<string, Queue<GameboardQueuedEvent>> gameboardEventQueueDict { get; set; }
        Dictionary<string, GameboardQueuedEvent> gameboardProcessingDict { get; set; }

        List<SendMessageToCompanionServiceResponse> eventResponseList { get; set; }

        List<QueuedEvent> CurrentEvents { get; set; }
        List<QueuedEvent> EndedEvents { get; set; }        
    }
}