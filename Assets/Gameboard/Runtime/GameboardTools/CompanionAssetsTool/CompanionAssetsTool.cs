using Gameboard.EventArgs;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gameboard.Tools
{
    public class CompanionAssetsTool : GameboardToolFoundation
    {
        public static CompanionAssetsTool singleton;

        private bool setupCompleted;

        /// <summary>
        /// A dictionary of all assets that have been loaded to each Companion. Key is UserID, Value is a List of LoadedCompanionAsset objects.
        /// </summary>
        private Dictionary<string, List<LoadedCompanionAsset>> LoadedCompanionAssetDict = new Dictionary<string, List<LoadedCompanionAsset>>();

        void Update()
        {
            if (Gameboard.Instance == null)
            {
                return;
            }

            // Do the setup here in Update so we can just do a Singleton lookup on Gameboard, and not worry about race-conditions in using Start.
            if (!setupCompleted)
            {
                if (Gameboard.Instance.companionController.isConnected || Application.isEditor)
                {
                    singleton = this;
                    setupCompleted = true;

                    GameboardLogging.LogMessage("--- Gameboard Companion Assets Tool is ready!", GameboardLogging.MessageTypes.Log);
                }
            }
        }

        /// <summary>
        /// Processes through a list of Texture2D objects and makes sure each one has already been loaded into the requested player's companion. Any unloaded textures will be loaded.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inTextureDict"><texturePath, textureByteArray></param>
        /// <returns></returns>
        public async Task LoadTextureListToCompanion(string playerId, Dictionary<string, byte[]> inTextureDict)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                GameboardLogging.LogMessage("LoadTextureListToCompanion requires playerId to not be null!", GameboardLogging.MessageTypes.Warning);
                return;
            }

            if (!LoadedCompanionAssetDict.ContainsKey(playerId))
            {
                LoadedCompanionAssetDict.Add(playerId, new List<LoadedCompanionAsset>());
            }

            if(inTextureDict == null || inTextureDict.Count == 0)
            {
                return;
            }

            foreach(KeyValuePair<string, byte[]> valuePair in inTextureDict)
            {
                await VerifyTextureLoadedToCompanion(playerId, valuePair.Key, valuePair.Value);
            }

            return;
        }

        /// <summary>
        /// Checks if this Texture2D has been loaded into this player's companion, and if not, loads it in.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inTexture"></param>
        /// <returns></returns>
        public async Task<LoadedCompanionAsset> VerifyTextureLoadedToCompanion(string playerId, string inTexturePath, byte[] textureByteArray)
        {
            if (!LoadedCompanionAssetDict.ContainsKey(playerId))
            {
                LoadedCompanionAssetDict.Add(playerId, new List<LoadedCompanionAsset>());
            }

            LoadedCompanionAsset loadedTextureAsset = LoadedCompanionAssetDict[playerId].Find(s => s.assetPath == inTexturePath);
            if (loadedTextureAsset != null)
            {
                //Debug.Log("--- Texture " + inTexturePath + " was already loaded to companion " + playerId + ". Finished!");
                return loadedTextureAsset;
            }
            else
            {
                loadedTextureAsset = await LoadTextureOnCompanion(playerId, inTexturePath, textureByteArray);
                if (loadedTextureAsset == null)
                {
                    GameboardLogging.LogMessage($"--- Failed to load texture asset {inTexturePath} to player {playerId}.", GameboardLogging.MessageTypes.Warning);
                    return null;
                }
                else
                {
                    LoadedCompanionAssetDict[playerId].Add(loadedTextureAsset);
                    return loadedTextureAsset;
                }
            }
        }

        /// <summary>
        /// Verifies is the requested Texture2D has been previously loaded into the Companion for the requested PlayerID.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inTexture"></param>
        /// <returns></returns>
        public bool WasTextureLoadedToCompanion(string playerId, string inAssetPath)
        {
            if(!LoadedCompanionAssetDict.ContainsKey(playerId))
            {
                return false;
            }

            return LoadedCompanionAssetDict[playerId].Find(s => s.assetPath == inAssetPath) != null;
        }

        /// <summary>
        /// Performs the process of loading a specifics Texture2D object into the requested player's Companion.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inTexture"></param>
        /// <returns></returns>
        private async Task<LoadedCompanionAsset> LoadTextureOnCompanion(string playerId, string inTexturePath, byte[] textureByteArray)
        {
            if(string.IsNullOrEmpty(inTexturePath))
            {
                GameboardLogging.LogMessage($"Failed to LoadTextureOnCompanion because the inTexturePath was blank!", GameboardLogging.MessageTypes.Error);
                return null;
            }

            if (textureByteArray == null || textureByteArray.Length == 0)
            {
                GameboardLogging.LogMessage($"Failed to LoadTextureOnCompanion because the textureByteArray was empty!", GameboardLogging.MessageTypes.Error);
                return null;
            }

            CompanionCreateObjectEventArgs objectEventArgs = await Gameboard.Instance.companionController.LoadAsset(playerId, textureByteArray);
            if (objectEventArgs.wasSuccessful)
            {
                LoadedCompanionAsset loadedAsset = new LoadedCompanionAsset()
                {
                    assetPath = inTexturePath,
                    assetUID = objectEventArgs.newObjectUid
                };

                return loadedAsset;
            }
            else
            {
                GameboardLogging.LogMessage($"Failed to load texture {inTexturePath} on companion {playerId} due to error {objectEventArgs.errorResponse.ErrorValue}: {objectEventArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return null;
            }
        }

        /// <summary>
        /// Finds all companions that has the requested textures loaded into them, and if that companion is not in the included Whitelist, then the textures get deleted.
        /// </summary>
        /// <param name="inPlayerWhitelist"></param>
        /// <param name="inTextureList"></param>
        public void DeleteTextureListFromAllCompanionsExceptWhitelist(List<string> inPlayerWhitelist, List<string> inTexturePaths)
        {
            foreach(string thisPath in inTexturePaths)
            {
                DeleteTextureFromAllCompanionsExceptWhitelist(inPlayerWhitelist, thisPath);
            }
        }

        /// <summary>
        /// Finds all companions that has the requested texture loaded into them, and if that companion is not in the included Whitelist, then the texture gets deleted.
        /// </summary>
        /// <param name="inPlayerWhitelist"></param>
        /// <param name="inTexture"></param>
        public void DeleteTextureFromAllCompanionsExceptWhitelist(List<string> inPlayerWhitelist, string inTexturePath)
        {
            foreach(KeyValuePair<string, List<LoadedCompanionAsset>> valuePair in LoadedCompanionAssetDict)
            {
                if(inPlayerWhitelist.Contains(valuePair.Key))
                {
                    continue;
                }

                LoadedCompanionAsset companionAsset = valuePair.Value.Find(s => s.assetPath == inTexturePath);
                if(companionAsset != null)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    DeleteTextureFromCompanion(valuePair.Key, inTexturePath);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        /// <summary>
        /// Processes through a list of Texture2D objects and makes sure each one has been deleted from a specific Companion.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="textureList"></param>
        /// <returns></returns>
        public async Task<bool> DeleteTextureListFromCompanion(string playerId, List<string> texturePathList)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                GameboardLogging.LogMessage("DeleteTextureListFromCompanion requires playerId to not be null!", GameboardLogging.MessageTypes.Error);
                return true;
            }

            if (!LoadedCompanionAssetDict.ContainsKey(playerId))
            {
                return true;
            }

            for (int i = 0; i < texturePathList.Count; i++)
            {
                await DeleteTextureFromCompanion(playerId, texturePathList[i]);
            }

            return true;
        }

        /// <summary>
        /// Tells the companion for the PlayerID to delete a specific Texture object from memory.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="inTexture"></param>
        /// <returns></returns>
        public async Task DeleteTextureFromCompanion(string playerId, string inPath)
        {
            LoadedCompanionAsset loadedTextureAsset = LoadedCompanionAssetDict[playerId].Find(s => s.assetPath == inPath);
            if(loadedTextureAsset == null)
            {
                GameboardLogging.LogMessage($"--- CompanionAssetsTool:DeleteTextureFromCompanion - Texture {inPath} has not been loaded to Companion for PlayerID {playerId}", GameboardLogging.MessageTypes.Warning);
                return;
            }

            CompanionMessageResponseArgs messageResponseArgs = await Gameboard.Instance.companionController.DeleteAsset(playerId, loadedTextureAsset.assetUID);
            if (messageResponseArgs.wasSuccessful)
            {
                LoadedCompanionAssetDict[playerId].Remove(loadedTextureAsset);
                return;
            }
            else
            {
                GameboardLogging.LogMessage($"Failed to delete texture {inPath} from companion {playerId} due to error {messageResponseArgs.errorResponse.ErrorValue}: {messageResponseArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return;
            }
        }

        public async Task DeleteAssetFromCompanion(string playerId, string inAssetGuid)
        {
            LoadedCompanionAsset loadedAsset = LoadedCompanionAssetDict[playerId].Find(s => s.assetUID == inAssetGuid);
            if (loadedAsset == null)
            {
                GameboardLogging.LogMessage($"--- CompanionAssetsTool:DeleteAssetFromCompanion - Asset with GUID {inAssetGuid} has not been loaded to Companion for PlayerID {playerId}", GameboardLogging.MessageTypes.Warning);
                return;
            }

            CompanionMessageResponseArgs messageResponseArgs = await Gameboard.Instance.companionController.DeleteAsset(playerId, inAssetGuid);
            if (messageResponseArgs.wasSuccessful)
            {
                LoadedCompanionAssetDict[playerId].Remove(loadedAsset);
                return;
            }
            else
            {
                GameboardLogging.LogMessage($"Failed to delete asset with GUID {inAssetGuid} from companion {playerId} due to error {messageResponseArgs.errorResponse.ErrorValue}: {messageResponseArgs.errorResponse.Message}", GameboardLogging.MessageTypes.Error);
                return;
            }
        }
    }
}
