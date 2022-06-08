using System.Collections.Generic;
using UnityEngine;
using Gameboard.EventArgs;
using Gameboard.Objects;
using System.Threading.Tasks;

namespace Gameboard
{
    [RequireComponent(typeof(Gameboard))]
    [RequireComponent(typeof(UserPresenceController))]
    [RequireComponent(typeof(AssetController))]
    public class CardController : MonoBehaviour
    {
        public delegate void OnCardPlayedHandler(CompanionCardPlayedEventArgs cardPlayedEvent);
        public event OnCardPlayedHandler CardPlayed;

        public delegate void OnCompanionCardButtonPressedHandler(GameboardCompanionCardsButtonPressedEventArgs cardButtonEvent);
        public event OnCompanionCardButtonPressedHandler CardButtonPressed;

        private Queue<GameboardCompanionCardsButtonPressedEventArgs> eventCardButtonQueue = new Queue<GameboardCompanionCardsButtonPressedEventArgs>(20);
        private Queue<CompanionCardPlayedEventArgs> eventCardQueue = new Queue<CompanionCardPlayedEventArgs>(20);

        private Gameboard gameboard => Gameboard.Instance;
        
        private void Awake()
        {
            gameboard.GameboardInitializationCompleted += OnGameboardInit;
            gameboard.GameboardShutdownBegun += OnGameboardShutdown;
        }

        private void Update()
        {
            // Process the card button events if there are any
            if (eventCardButtonQueue.Count > 0)
            {
                Queue<GameboardCompanionCardsButtonPressedEventArgs> clonedCardEventQueue = new Queue<GameboardCompanionCardsButtonPressedEventArgs>(eventCardButtonQueue);
                eventCardButtonQueue.Clear();

                while (clonedCardEventQueue.Count > 0)
                {
                    GameboardCompanionCardsButtonPressedEventArgs eventToProcess = clonedCardEventQueue.Dequeue();

                    CardButtonPressed?.Invoke(eventToProcess);
                }

                clonedCardEventQueue.Clear();
            }

            // Process the card events if there are any
            if (eventCardQueue.Count > 0)
            {
                Queue<CompanionCardPlayedEventArgs> clonedEventQueue = new Queue<CompanionCardPlayedEventArgs>(eventCardQueue);
                eventCardQueue.Clear();

                while (clonedEventQueue.Count > 0)
                {
                    CompanionCardPlayedEventArgs eventToProcess = clonedEventQueue.Dequeue();

                    CardPlayed?.Invoke(eventToProcess);
                }

                clonedEventQueue.Clear();
            }
        }

        private void OnGameboardInit()
        {
            gameboard.services.companionHandler.CardPlayedEvent += CardPlayedEvent;
            gameboard.services.companionHandler.CompanionCardsButtonPressedEvent += CompanionCardsButtonPressedEvent;
        }

        private void OnGameboardShutdown()
        {
            gameboard.services.companionHandler.CardPlayedEvent -= CardPlayedEvent;
            gameboard.services.companionHandler.CompanionCardsButtonPressedEvent -= CompanionCardsButtonPressedEvent;
        }

        private void OnDestroy()
        {
            gameboard.services.companionHandler.CardPlayedEvent -= CardPlayedEvent;
            gameboard.services.companionHandler.CompanionCardsButtonPressedEvent -= CompanionCardsButtonPressedEvent;
        }

        /// <summary>
        /// Set a card control asset for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="assetType"></param>
        /// <param name="assetGuid"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetCardControlAsset(string userId, ControlAssetType assetType, string assetGuid)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetControlAsset(Gameboard.COMPANION_VERSION, userId, assetType, assetGuid);
            return responseArgs;
        }

        /// <summary>
        /// Set the card template type for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="templateType"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetCardTemplateType(string userId, CompanionCardTemplateType templateType)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetCardTemplate(Gameboard.COMPANION_VERSION, userId, templateType);
            return responseArgs;
        }

        /// <summary>
        /// Creates a Hand Display for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Task<CompanionCreateObjectEventArgs></returns>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionHandDisplay(string userId)
        {
            CompanionCreateObjectEventArgs responseArgs = await gameboard.services.companionHandler.CreateCompanionHandDisplay(Gameboard.COMPANION_VERSION, userId);
            return responseArgs;
        }

        /// <summary>
        /// Changes the Hand Display shown for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handDisplayId"></param>
        /// <returns>Task<CompanionMessageResponseArgs></returns>
        public async Task<CompanionMessageResponseArgs> ShowCompanionHandDisplay(string userId, string handDisplayId)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.ShowCompanionHandDisplay(Gameboard.COMPANION_VERSION, userId, handDisplayId);
            return responseArgs;
        }

        /// <summary>
        /// Creates a card object on a Companion, using the requested Card Texture for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="uidOfTextureForCard"></param>
        /// <returns>Task<CompanionCreateObjectEventArgs></returns>
        /// <remarks>This requires the card asset to be previously loaded to the companion</remarks>
        public async Task<CompanionCreateObjectEventArgs> CreateCompanionCard(string userId, string cardId, string frontCardTextureUid, string backCardTextureUid, int inTextureWidth, int inTextureHeight)
        {
            CompanionCreateObjectEventArgs cardResponse = await gameboard.services.companionHandler.CreateCompanionCard(Gameboard.COMPANION_VERSION, userId, cardId, frontCardTextureUid, backCardTextureUid, inTextureWidth, inTextureHeight);
            return cardResponse;
        }

        /// <summary>
        /// Adds the requested Card to the requested Companion Hand for a specified companion user.
        /// </summary>
        /// <param name="usedId"></param>
        /// <param name="handDisplayId"></param>
        /// <param name="cardId"></param>
        /// <returns>Task<CompanionMessageResponseArgs></returns>
        public async Task<CompanionMessageResponseArgs> AddCardToHandDisplay(string userId, string handDisplayId, string cardId)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.AddCardToHand(Gameboard.COMPANION_VERSION, userId, handDisplayId, cardId);
            return responseArgs;
        }

        /// <summary>
        /// Removes the requested card from the requested Hand Display for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handDisplayId"></param>
        /// <param name="cardId"></param>
        /// <returns>Task<CompanionMessageResponseArgs></returns>
        public async Task<CompanionMessageResponseArgs> RemoveCardFromHandDisplay(string userId, string handDisplayId, string cardId)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.RemoveCardFromHand(Gameboard.COMPANION_VERSION, userId, handDisplayId, cardId);
            return responseArgs;
        }

        /// <summary>
        /// Removes all cards from the requested Hand Display for a specified companion user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="handDisplayId"></param>
        /// <returns>Task<CompanionMessageResponseArgs></returns>
        public async Task<CompanionMessageResponseArgs> RemoveAllCardsFromHandDisplay(string userId, string handDisplayId)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.RemoveAllCardsFromHand(Gameboard.COMPANION_VERSION, userId, handDisplayId);
            return responseArgs;
        }

        /// <summary>
        /// Set the card highlights for a specified user.
        /// </summary>
        /// <param name="cardHighlights"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetCardHighlights(string userId, CardHighlights cardHighlights)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetCardHighlight(Gameboard.COMPANION_VERSION, userId, cardHighlights);
            return responseArgs;
        }

        /// <summary>
        /// Set the card's front asset to the specified id for a specified user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cardAssetId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetCardFrontAssetId(string userId, CardAssetId cardAssetId)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetCardFrontAssetId(Gameboard.COMPANION_VERSION, userId, cardAssetId);
            return responseArgs;
        }

        /// <summary>
        /// Set the card's back asset to the specified id for a specified user.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cardAssetId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetCardBackAssetId(string userId, CardAssetId cardAssetId)
        {
            CompanionMessageResponseArgs responseArgs = await gameboard.services.companionHandler.SetCardBackAssetId(Gameboard.COMPANION_VERSION, userId, cardAssetId);
            return responseArgs;
        }

        /// <summary>
        /// Handle the card played event generated by game/cardPlayed
        /// </summary>
        /// <param name="cardPlayedEventArgs"></param>
        /// <remarks>This event is called from a background thread.</remarks>
        private void CardPlayedEvent(GameboardCardPlayedEventArgs cardPlayedEventArgs)
        {
            CompanionCardPlayedEventArgs eventArgs = new CompanionCardPlayedEventArgs()
            {
                userId = cardPlayedEventArgs.userId,
                cardId = cardPlayedEventArgs.cardId
            };

            eventCardQueue.Enqueue(eventArgs);
        }

        /// <summary>
        /// Handle the card button presses generated by game/cardsButtonPressed
        /// </summary>
        /// <param name="inEventArgs"></param>
        private void CompanionCardsButtonPressedEvent(GameboardCompanionCardsButtonPressedEventArgs inEventArgs)
        {
            eventCardButtonQueue.Enqueue(inEventArgs);
        }
    }
}
