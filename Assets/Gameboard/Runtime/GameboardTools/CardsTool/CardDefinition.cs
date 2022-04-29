using Gameboard.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard.Tools
{
    public class CardDefinition
    {
        public string cardName;
        public byte[] cardFrontBytes { get; private set; }
        public byte[] cardBackBytes { get; private set;  }
        public string cardGuid { get; }
        public int cardTextureWidth { get; private set; }
        public int cardTextureHeight { get; private set; }

        /// <summary>
        /// A file path for the Front Texture for this card. Can be optionally used if not loading textures into memory for this card.
        /// </summary>
        public string cardFrontTexturePath { get; private set; }

        /// <summary>
        /// The GUID for the Front Texture of this card. This will only be populated if the texture and this card have both been loaded into a companion.
        /// </summary>
        public string cardFrontTextureGUID { get; private set; }

        /// <summary>
        /// A file path for the Back Texture for this card. Can be optionally used if not loading textures into memory for this card.
        /// </summary>
        public string cardBackTexturePath { get; private set; }

        /// <summary>
        /// The GUID for the Back Texture of this card. This will only be populated if the texture and this card have both been loaded into a companion.
        /// </summary>
        public string cardBackTextureGUID { get; private set; }

        /// <summary>
        /// Where this card is currently located. If null or empty, then it is not held by any CardHand. Otherwise, if the value is populated, then it is the ID of a specific CardHand.
        /// </summary>
        public string CurrentCardLocation { get; private set; }

        /// <summary>
        /// The actual front texture used on this card, for Unity-level access. Only exists after calling LoadCardTextures.
        /// </summary>
        public Texture2D cardFrontTexture { get; private set; }

        /// <summary>
        /// The actual back texture used on this card, for Unity-level access. Only exists after calling LoadCardTextures.
        /// </summary>
        public Texture2D cardBackTexture { get; private set; }

        /// <summary>
        /// A List for the ID values of all User companions that this card has been loaded into.
        /// </summary>
        private List<string> UserIDsLoadedInto = new List<string>();

        private const int defaultTextureSize = 2;

        public CardDefinition(string inCardFrontTexturePath, byte[] inFrontTextureBytes, string inCardBackTexturePath, byte[] inBackTextureBytes, int inCardWidth, int inCardHeight)
        {
            if(string.IsNullOrEmpty(inCardFrontTexturePath) && inFrontTextureBytes != null)
            {
                Debug.LogWarning("--- When FrontTextureBytes are used, the CardFrontTexturePath must be populated so the code has a reference to find these bytes!");
            }

            if (string.IsNullOrEmpty(inCardBackTexturePath) && inBackTextureBytes != null)
            {
                Debug.LogWarning("--- When BackTextureBytes are used, the CardBackTexturePath must be populated so the code has a reference to find these bytes!");
            }

            cardGuid = Guid.NewGuid().ToString();
            
            cardFrontBytes = inFrontTextureBytes;
            cardBackBytes = inBackTextureBytes;

            cardFrontTexturePath = inCardFrontTexturePath;
            cardBackTexturePath = inCardBackTexturePath;

            cardTextureWidth = inCardWidth;
            cardTextureHeight = inCardHeight;
        }

        public CardDefinition()
        {
            cardGuid = Guid.NewGuid().ToString();
            cardTextureWidth = defaultTextureSize;
            cardTextureHeight = defaultTextureSize;
        }

        #region Card Texture Management
        public void UpdateCardTextureSizing(int inWidth, int inHeight)
        {
        	cardTextureWidth = inWidth;
        	cardTextureHeight = inHeight;
        }

        /// <summary>
        /// Changes the texture used for the Front Texture.
        /// </summary>
        /// <param name="inFrontTexture"></param>
        public void UpdateFrontTextureBytes(byte[] inFrontTextureBytes)
        {
            cardFrontBytes = inFrontTextureBytes;
        }

        /// <summary>
        /// Changes the Texture used for the Back Texture.
        /// </summary>
        /// <param name="inBackTexture"></param>
        public void UpdateBackTextureBytes(byte[] inBackTextureBytes)
        {
            cardBackBytes = inBackTextureBytes;
        }

        /// <summary>
        /// Updates the current location of this card. Should either be the GUID for a CardHand where this card has been placed, or a blank value if currently not in any CardHand.
        /// </summary>
        /// <param name="inTargetLocation"></param>
        public void SetCardLocation(string inTargetLocation)
        {
            Debug.Log("Current card location = "  + inTargetLocation);
            CurrentCardLocation = inTargetLocation;
        }

        /// <summary>
        /// Updates the device file path for the Front Texture of this card. Use this if the Card Textures should not always be loaded into memory;
        /// </summary>
        /// <param name="inPath"></param>
        public void SetFrontTexturePath(string inPath)
        {
            cardFrontTexturePath = inPath;
        }

        /// <summary>
        /// Updates the device file path for the Back Texture of this card. Use this if the Card Textures should not always be loaded into memory;
        /// </summary>
        /// <param name="inPath"></param>
        public void SetBackTexturePath(string inPath)
        {
            cardBackTexturePath = inPath;
        }

        /// <summary>
        /// Loads the Front and Back textures from paths. The paths should preivously have been set using SetFrontTexturePath and SetBackTexturePath. If a path is not populated, then nothing will be loaded for that image.
        /// </summary>
        public void LoadCardBytesFromPaths()
        {
            if(cardFrontBytes == null || cardFrontBytes.Length == 0 && !string.IsNullOrEmpty(cardFrontTexturePath))
            {
                cardFrontBytes = GameboardHelperMethods.LoadByteArrayFromPath(cardFrontTexturePath);
            }

            if (cardBackBytes == null || cardBackBytes.Length == 0 && !string.IsNullOrEmpty(cardBackTexturePath))
            {
                cardBackBytes = GameboardHelperMethods.LoadByteArrayFromPath(cardBackTexturePath);
            }
        }

        /// <summary>
        /// Deletes the images used for the Front and Back textures for this card from Gameboard memory. Does not remove them from Companion memory.
        /// </summary>
        public void UnloadCardTextureBytes()
        {
            cardFrontBytes = null;
            cardBackBytes = null;
        }

        /// <summary>
        /// Loads the textures for this card into memory.
        /// </summary>
        public void LoadCardTextures()
        {
            if(cardFrontTexture == null && !string.IsNullOrEmpty(cardFrontTexturePath))
            {
                cardFrontTexture = GameboardHelperMethods.LoadTextureFromPath(cardFrontTexturePath);
            }

            if (cardBackTexture == null && !string.IsNullOrEmpty(cardBackTexturePath))
            {
                cardBackTexture = GameboardHelperMethods.LoadTextureFromPath(cardBackTexturePath);
            }
        }

        /// <summary>
        /// Unloads the textures for this card from memory.
        /// </summary>
        public void UnloadCardTextures()
        {
            cardFrontTexture = null;
            cardBackTexture = null;
        }
        #endregion

        /// <summary>
        /// Sets this card as not being located in any Card Hand.
        /// </summary>
        public void ClearCardLocation()
        {
            CurrentCardLocation = "";
        }

        public void CardFrontTextureLoadedIntoCompanion(string inTextureGuid)
        {
            cardFrontTextureGUID = inTextureGuid;
        }

        public void CardFrontTextureDeletedFromCompanion()
        {
            cardFrontTextureGUID = "";
        }

        public void CardBackTextureLoadedIntoCompanion(string inTextureGuid)
        {
            cardBackTextureGUID = inTextureGuid;
        }

        public void CardBackTextureDeletedFromCompanion()
        {
            cardBackTextureGUID = "";
        }

        /// <summary>
        /// This card just finished loading into a specific Companion.
        /// </summary>
        /// <param name="inUserId"></param>
        public void CardWasLoadedToCompanion(string inUserId)
        {
            UserIDsLoadedInto.Add(inUserId);
        }

        /// <summary>
        /// This card just finished being unloaded from a specific Companion.
        /// </summary>
        /// <param name="inUserId"></param>
        public void CardWasUnLoadedFromCompanion(string inUserId)
        {
            UserIDsLoadedInto.Remove(inUserId);
        }

        /// <summary>
        /// This card has been unloaded from all companions.
        /// </summary>
        /// <param name="inUserId"></param>
        public void CardWasUnLoadedFromAllCompanions()
        {
            UserIDsLoadedInto.Clear();
        }

        /// <summary>
        /// Verifies if this card has been loaded into a specific Companion or not.
        /// </summary>
        /// <param name="inUserId"></param>
        /// <returns></returns>
        public bool IsCardLoadedIntoCompanion(string inUserId)
        {
            return UserIDsLoadedInto.Contains(inUserId);
        }
    }
}
