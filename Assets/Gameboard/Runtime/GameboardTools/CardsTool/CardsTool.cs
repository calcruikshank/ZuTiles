using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Gameboard.EventArgs;

namespace Gameboard.Tools
{
    public class CardsTool : GameboardToolFoundation
    {
        public delegate void CardToolDelegate(string userId, string cardId);
            public event CardToolDelegate CardPlayedOnCompanion;

        public static CardsTool singleton;

        /// <summary>
        /// Dictionary of all CardHands for the companions. Key is UserID, Value is a List of CardHandIDs for that UserID.
        /// </summary>
        private Dictionary<string, List<string>> CardHandIdDict = new Dictionary<string, List<string>>();

        /// <summary>
        /// Dictionary of what CardHand is currently displayed for each Player.
        /// </summary>
        private Dictionary<string, string> CurrentDisplayedHandIDForPlayers = new Dictionary<string, string>();

        /// <summary>
        /// A list of every card that has been created in this game.
        /// </summary>
        private List<CardDefinition> FullCardLibrary = new List<CardDefinition>();

        private bool setupCompleted;

        protected override void PerformCleanup()
        {
            if (setupCompleted && Gameboard.Instance != null && Gameboard.Instance.companionController != null)
            {
                Gameboard.Instance.companionController.CardPlayed -= CompanionController_CardPlayed;
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
                if (Application.isEditor || Gameboard.Instance.companionController.isConnected)
                {
                    if (!Application.isEditor)
                    {
                        Gameboard.Instance.companionController.CardPlayed += CompanionController_CardPlayed;
                    }

                    singleton = this;
                    setupCompleted = true;

                    GameboardLogging.LogMessage("--- Gameboard Card Tool is ready!", GameboardLogging.MessageTypes.Log);
                }
            }
        }

        private void CompanionController_CardPlayed(object sender, CompanionCardPlayedEventArgs e)
        {
            CardPlayedOnCompanion?.Invoke(e.userId, e.cardId);
        }

        /// <summary>
        /// Call this between game sessions to entirely reset the state of all cards and data.
        /// </summary>
        public void WipeAllCardsData()
        {
            CardHandIdDict.Clear();
            CurrentDisplayedHandIDForPlayers.Clear();
            FullCardLibrary.ForEach(s =>
            {
                s.ClearCardLocation();
                s.CardWasUnLoadedFromAllCompanions();
            });
        }

        #region Hand Display Management
        /// <summary>
        /// Returns the GUID of the CardHandDisplay currently shown for a specific Companion, or a blank string if none are currently displayed.
        /// </summary>
        /// <param name="inPlayerId"></param>
        /// <returns></returns>
        public string GetCardHandDisplayedForPlayer(string inPlayerId)
        {
            if (!CurrentDisplayedHandIDForPlayers.ContainsKey(inPlayerId))
            {
                CurrentDisplayedHandIDForPlayers.Add(inPlayerId, "");
            }

            return CurrentDisplayedHandIDForPlayers[inPlayerId];
        }

        public async Task<string> CreateCardHandOnPlayer(string playerId)
        {
            if (!CardHandIdDict.ContainsKey(playerId))
            {
                CardHandIdDict.Add(playerId, new List<string>());
            }

            CompanionCreateObjectEventArgs eventArgs = await Gameboard.Instance.companionController.CreateCompanionHandDisplay(playerId);
            if (eventArgs != null && eventArgs.wasSuccessful)
            {
                CardHandIdDict[playerId].Add(eventArgs.newObjectUid);
                return eventArgs.newObjectUid;
            }
            else
            {
                GameboardLogging.LogMessage($"--- Failed to create Card Hand on companion {playerId} due to error {eventArgs.errorResponse.ErrorValue}: {eventArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return null;
            }
        }

        /// <summary>
        /// Returns a list of CardHandIDs for all hands that this player has.
        /// </summary>
        /// <param name="inPlayerId"></param>
        /// <returns></returns>
        public List<string> GetAllCardHandsForPlayer(string inPlayerId)
        {
            return CardHandIdDict.ContainsKey(inPlayerId) ? CardHandIdDict[inPlayerId] : new List<string>();
        }

        /// <summary>
        /// Deletes all Card Hands for this player, and removes the cards in those hands.
        /// </summary>
        /// <param name="playerId"></param>
        public async void DeleteAllCardHandsForPlayer(string playerId, bool alsoUnloadCardsFromCompanion = false)
        {
            if(!CardHandIdDict.ContainsKey(playerId))
            {
                return;
            }

            for(int i = CardHandIdDict[playerId].Count - 1; i >= 0; i--)
            {
                await DeleteCardHandForPlayer(playerId, CardHandIdDict[playerId][i], alsoUnloadCardsFromCompanion);
            }

            CardHandIdDict.Remove(playerId);
        }

        /// <summary>
        /// Entirely deletes a Card Hand Display on a Player's Companion.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inCardHandId"></param>
        /// <param name="alsoUnloadCardsFromCompanion"></param>
        /// <returns></returns>
        public async Task DeleteCardHandForPlayer(string playerId, string inCardHandId, bool alsoUnloadCardsFromCompanion = false)
        {
            // NOTE: Currently this only deletes the CardHand locally, and removes all cards that are in this CardHand on the player. We don't have a call currently to full-out delete a CardHand.
            if(!CardHandIdDict.ContainsKey(playerId))
            {
                return;
            }
            CardHandIdDict[playerId].Remove(inCardHandId);

            if (CurrentDisplayedHandIDForPlayers.ContainsKey(playerId))
            {
                // If this is the displayed card hand, set the displayed hand as 'nothing'.
                if (CurrentDisplayedHandIDForPlayers[playerId] == inCardHandId)
                {
                    CurrentDisplayedHandIDForPlayers[playerId] = "";
                }
            }

            List<CardDefinition> cardsInHandList = GetAllCardsInCardHand(inCardHandId);
            foreach(CardDefinition thisCardDef in cardsInHandList)
            {
                await RemoveCardFromPlayerHand_Async(playerId, inCardHandId, thisCardDef);

                if(alsoUnloadCardsFromCompanion)
                {
                    await TakeCardFromPlayer(playerId, thisCardDef);
                }
            }
        }

        /// <summary>
        /// Changes the Card Hand Display currently in-view on a Player's Companion.
        /// </summary>
        /// <param name="inPlayerId"></param>
        /// <param name="inCardHandId"></param>
        /// <returns>bool wasSuccessful</returns>
        public async Task<bool> ShowHandDisplay(string inPlayerId, string inCardHandId)
        {
            if (!CurrentDisplayedHandIDForPlayers.ContainsKey(inPlayerId))
            {
                CurrentDisplayedHandIDForPlayers.Add(inPlayerId, "");
            }

            CompanionMessageResponseArgs responseArgs = await Gameboard.Instance.companionController.ShowCompanionHandDisplay(inPlayerId, inCardHandId);
            if (responseArgs.wasSuccessful)
            {
                CurrentDisplayedHandIDForPlayers[inPlayerId] = inCardHandId;
                return true;
            }
            else
            {
                GameboardLogging.LogMessage($"--- Failed to change the shown CompanionHandDIsplay on companion {inPlayerId} to CompanionHandDisplay {inCardHandId} due to error {responseArgs.errorResponse.ErrorValue}: {responseArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return false;
            }
        }
        #endregion

        #region Placing Cards in Hand Displays
        public void PlaceCardInPlayerHand(string playerId, string handId, CardDefinition inCardDef)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            PlaceCardInPlayerHand_Async(playerId, handId, inCardDef);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task<bool> PlaceCardInPlayerHand_Async(string playerId, string handId, CardDefinition inCardDef)
        {
            if(inCardDef.CurrentCardLocation == handId)
            {
                // Card is already placed in this player's hand, so don't place it again or else it refreshes the hand display.
                return true;
            }

            // Make sure the player has this card before we give it to them.
            await GiveCardToPlayer(playerId, inCardDef);

            // Tell the CompanionController to place this card in the player hand
            CompanionMessageResponseArgs responseArgs = await Gameboard.Instance.companionController.AddCardToHandDisplay(playerId, handId, inCardDef.cardGuid);
            if (responseArgs.wasSuccessful)
            {
                inCardDef.SetCardLocation(handId);
                return true;
            }
            else
            {
                GameboardLogging.LogMessage($"--- Failed to place Card {inCardDef.cardGuid} in Card Hand {handId} on companion {playerId} due to error {responseArgs.errorResponse.ErrorValue}: {responseArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return false;
            }
        }
        #endregion

        #region Removing Cards from Hand Displays
        public void RemoveCardFromPlayerHand(string playerId, string handId, CardDefinition cardDef)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            RemoveCardFromPlayerHand_Async(playerId, handId, cardDef);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task RemoveCardFromPlayerHand_Async(string playerId, string handId, CardDefinition cardDef)
        {
            CompanionMessageResponseArgs responseArgs = await Gameboard.Instance.companionController.RemoveCardFromHandDisplay(playerId, handId, cardDef.cardGuid);
            if (responseArgs.wasSuccessful)
            {
                cardDef.ClearCardLocation();
                return;
            }
            else
            {
                GameboardLogging.LogMessage($"--- Failed to remove Card {cardDef.cardGuid} from Card Hand {handId} on companion {playerId} due to error {responseArgs.errorResponse.ErrorValue}: {responseArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return;
            }
        }

        public async Task RemoveAllCardsFromPlayerHand(string playerId, string handId)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            await RemoveAllCardsFromPlayerHand_Async(playerId, handId);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public async Task RemoveAllCardsFromPlayerHand_Async(string playerId, string handId)
        {
            // NOTE: Need to also update cardDef.SetCardLocation(""); with this!!

            CompanionMessageResponseArgs responseArgs = await Gameboard.Instance.companionController.RemoveAllCardsFromHandDisplay(playerId, handId);
            if (responseArgs.wasSuccessful)
            {
                // Companion removal was successful, therefore remove all cards from the player's hand here in the CardsTool.
                List<CardDefinition> cardsInPlayerHand = GetAllCardsInCardHand(handId);
                cardsInPlayerHand.ForEach(s => s.ClearCardLocation());

                return;
            }
            else
            {
                GameboardLogging.LogMessage($"--- Failed to remove all cards from Card Hand {handId} on companion {playerId} due to error {responseArgs.errorResponse.ErrorValue}: {responseArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return;
            }
        }
        #endregion

        #region Giving Cards to Player Companion
        /// <summary>
        /// Creates this card on the players' Companion. Note that since Cards need to have their decks maintained on the Gameboard, it is the responsibility
        /// of the card manager in the Game to create and handle CardIDs.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="cardFrontTexture"></param>
        /// <param name="cardBackTexture"></param>
        /// <returns>GUID of the card object.</returns>
        public async Task<string> GiveCardToPlayer(string playerId, CardDefinition inCardDef)
        {
            if(inCardDef.IsCardLoadedIntoCompanion(playerId))
            {
                return inCardDef.cardGuid;
            }

            // Check if the card textures were loaded when we got here. If they were, do nothing, as the Creator is handling this. If they weren't, load them, and flag that we'll unload them as well.
            bool didTextureLoad = false;
            if (inCardDef.cardFrontBytes == null || inCardDef.cardFrontBytes.Length == 0)
            {
                inCardDef.LoadCardBytesFromPaths();
                didTextureLoad = true;
            }

            LoadedCompanionAsset frontLoadedAsset = null;
            if (inCardDef.cardFrontBytes != null && inCardDef.cardFrontBytes.Length > 0)
            {
                frontLoadedAsset = await CompanionAssetsTool.singleton.VerifyTextureLoadedToCompanion(playerId, inCardDef.cardFrontTexturePath, inCardDef.cardFrontBytes);
                if (frontLoadedAsset == null)
                {
                    GameboardLogging.LogMessage($"--- Card {inCardDef.cardGuid} for player {playerId} is missing the Front Texture because the texture failed to load to the companion.", GameboardLogging.MessageTypes.Error);
                }
                else
                {
                    if(string.IsNullOrEmpty(inCardDef.cardFrontTextureGUID))
                    {
                        inCardDef.CardFrontTextureLoadedIntoCompanion(frontLoadedAsset.assetUID);
                    }
                }
            }
            else
            {
                GameboardLogging.LogMessage("--- Card front texture null! Continuing.", GameboardLogging.MessageTypes.Log);
            }

            if (didTextureLoad)
            {
                inCardDef.UnloadCardTextureBytes();
            }

            LoadedCompanionAsset backLoadedAsset = null;
            // NOTE: Not even using back textures currently.
            /*if (inCardDef.cardBackTexture != null)
            {
                backLoadedAsset = await CompanionAssetsTool.singleton.VerifyTextureLoadedToCompanion(playerId, inCardDef.cardBackTexture);
                if (backLoadedAsset == null)
                {
                    GameboardLogging.LogMessage($"--- Card {inCardDef.cardGuid} for player {playerId} is missing the Back Texture because the texture failed to load to the companion.", GameboardLogging.MessageTypes.Error);
                }
                else
                {
                    if (string.IsNullOrEmpty(inCardDef.cardBackTextureGUID))
                    {
                        inCardDef.CardBackTextureLoadedIntoCompanion(backLoadedAsset.assetUID);
                    }
                }
            }*/

            CompanionCreateObjectEventArgs createCardEventArgs = await Gameboard.Instance.companionController.CreateCompanionCard(playerId, inCardDef.cardGuid, frontLoadedAsset?.assetUID, backLoadedAsset?.assetUID, inCardDef.cardTextureWidth, inCardDef.cardTextureHeight);
            if(createCardEventArgs.wasSuccessful)
            {
                inCardDef.CardWasLoadedToCompanion(playerId);
                return createCardEventArgs.newObjectUid;
            }
            else
            {
                GameboardLogging.LogMessage($"--- Failed to create card on companion {playerId} due to error {createCardEventArgs.errorResponse.ErrorValue}: {createCardEventArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return null;
            }
        }
        #endregion

        #region Removing Cards From Companion
        /// <summary>
        /// Removes a card from a player's Companion entirely.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inCardDef"></param>
        /// <returns></returns>
        public async Task DeleteCardFromPlayer_Async(string playerId, CardDefinition inCardDef)
        {
            // TODO: This call is not yet integrated in the SDK or Companion.
            await Task.Delay(100);
            //inCardDef.CardWasUnLoadedFromCompanion(); Uncomment this when this method is integrated. Currently cards are never unloaded from companions.
        }

        /// <summary>
        /// Removes a card from a player's Companion.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inCardDef"></param>
        /// <returns></returns>
        public async Task TakeCardFromPlayer(string playerId, CardDefinition inCardDef)
        {
            // Abort if this card has not been loaded into this companion.
            if(!inCardDef.IsCardLoadedIntoCompanion(playerId))
            {
                GameboardLogging.LogMessage($"--- CardsTool:TakeCardFromPlayer - Card with GUID {inCardDef.cardGuid} does not exist on Companion for PlayerID {playerId}", GameboardLogging.MessageTypes.Warning);
                return;
            }

            // Abort if this card is in a Card Hand owned by a different player.
            if(!string.IsNullOrEmpty(inCardDef.CurrentCardLocation))
            {
                string playerOwningCardLocation = "";
                foreach(KeyValuePair<string, List<string>> valuePair in CardHandIdDict)
                {
                    if(valuePair.Value.Contains(inCardDef.CurrentCardLocation))
                    {
                        playerOwningCardLocation = valuePair.Key;
                        break;
                    }
                }

                if (playerOwningCardLocation != playerId)
                {
                    GameboardLogging.LogMessage($"--- CardsTool:TakeCardFromPlayer - Card with GUID {inCardDef.cardGuid} was requested to be removed from Companion for player {playerId}, however this card is currently in the Player Hand with GUID {inCardDef.CurrentCardLocation} which is not owned by this player! It is owned by the player {playerOwningCardLocation}", GameboardLogging.MessageTypes.Warning);
                    return;
                }
            }

            // Delete the assets for this card.
            if(!string.IsNullOrEmpty(inCardDef.cardFrontTextureGUID))
            {
                await CompanionAssetsTool.singleton.DeleteAssetFromCompanion(playerId, inCardDef.cardFrontTextureGUID);
                inCardDef.CardFrontTextureDeletedFromCompanion();
            }

            // TODO: Need to verify if other cards are using this back or not, and only delete if nothing else is.
            if (!string.IsNullOrEmpty(inCardDef.cardBackTextureGUID))
            {
                await CompanionAssetsTool.singleton.DeleteAssetFromCompanion(playerId, inCardDef.cardBackTextureGUID);
                inCardDef.CardFrontTextureDeletedFromCompanion();
            }

            // If this card is in a CardHand for this player, remove it from their hand.
            if(!string.IsNullOrEmpty(inCardDef.CurrentCardLocation))
            {
                await RemoveCardFromPlayerHand_Async(playerId, inCardDef.CurrentCardLocation, inCardDef);
                inCardDef.ClearCardLocation();
            }

            // Remove this card from this companion.
            await DeleteCardFromPlayer_Async(playerId, inCardDef);
        }

        /// <summary>
        /// Accepts a list of CardDefinition objects, ensure their assets and data are all loaded to the player's Companion, and then populates the hand of cards. If the card hand does not exist, it will be created.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="handId"></param>
        /// <param name="inCardDefList"></param>
        /// <returns></returns>
        public async Task<bool> AddCardsToPlayerHand(string playerId, string handId, List<CardDefinition> inCardDefList)
        {
            // Get a list of Textures for these cards, and verify they have all been loaded to the Companion.
            Dictionary<string, byte[]> loadDict = new Dictionary<string, byte[]>();
            foreach(CardDefinition thisCardDef in inCardDefList)
            {
                thisCardDef.LoadCardBytesFromPaths();

                if (thisCardDef.cardFrontBytes != null && thisCardDef.cardFrontBytes.Length > 0)
                {
                    if (thisCardDef.cardFrontTextureGUID != null && !loadDict.ContainsKey(thisCardDef.cardFrontTexturePath))
                    {
                        loadDict.Add(thisCardDef.cardFrontTexturePath, thisCardDef.cardFrontBytes);
                    }
                }

                if (thisCardDef.cardBackBytes != null && thisCardDef.cardBackBytes.Length > 0)
                {
                    if (thisCardDef.cardBackTexturePath != null && !loadDict.ContainsKey(thisCardDef.cardBackTexturePath))
                    {
                        loadDict.Add(thisCardDef.cardBackTexturePath, thisCardDef.cardBackBytes);
                    }
                }
            }

            // Ensure those textures are loaded into this companion.
            await CompanionAssetsTool.singleton.LoadTextureListToCompanion(playerId, loadDict);

            // Now add these cards to the companion
            foreach(CardDefinition thisCardDef in inCardDefList)
            {
                await GiveCardToPlayer(playerId, thisCardDef);
            }

            // Add these cards to the player's hand.
            foreach (CardDefinition thisCardDef in inCardDefList)
            {
                await PlaceCardInPlayerHand_Async(playerId, handId, thisCardDef);
            }

            // Now unload the card textures
            foreach(CardDefinition thisCardDef in inCardDefList)
            {
                thisCardDef.UnloadCardTextureBytes();
            }

            return true;
        }
        #endregion

        #region General Card Management
        /// <summary>
        /// Adds a CardDefinition object to the Card Library. This is a runtime action.
        /// </summary>
        /// <param name="inCardDef"></param>
        public void AddCardToLibrary(CardDefinition inCardDef)
        {
            if(GetCardDefinitionByGUID(inCardDef.cardGuid) != null)
            {
                return;
            }

            FullCardLibrary.Add(inCardDef);
        }

        /// <summary>
        /// Returns a CardDefinition based on its internal GUID value.
        /// </summary>
        /// <param name="inCardGuid"></param>
        /// <returns>CardDefinition</returns>
        public CardDefinition GetCardDefinitionByGUID(string inCardGuid)
        {
            return FullCardLibrary.Find(s => s.cardGuid == inCardGuid);
        }

        /// <summary>
        /// Returns the current CardHandID GUID for the card that matches the entered GUID. If this card is not currently located anywhere, it will return a blank string.
        /// </summary>
        /// <param name="inCardGuid"></param>
        /// <returns>string</returns>
        public string GetCurrentLocationOfCardByGUID(string inCardGuid)
        {
            CardDefinition cardDef = GetCardDefinitionByGUID(inCardGuid);
            if (cardDef == null)
            {
                GameboardLogging.LogMessage($"--- CardsTool:GetCurrentLocationOfCardByGUID - No card exists with GUID {inCardGuid}", GameboardLogging.MessageTypes.Warning);
                return "";
            }

            return cardDef.CurrentCardLocation;
        }

        /// <summary>
        /// Returns a list of CardDefinition objects for all cards that are currently located in the requested CardHandID.
        /// </summary>
        /// <param name="inCardHandId"></param>
        /// <returns>List<CardDefinition></returns>
        public List<CardDefinition> GetAllCardsInCardHand(string inCardHandId)
        {
            return FullCardLibrary.FindAll(s => s.CurrentCardLocation == inCardHandId);
        }
        #endregion
    }
}
