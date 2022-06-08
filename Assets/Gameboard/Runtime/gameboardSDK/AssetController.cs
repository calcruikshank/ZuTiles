using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Gameboard.EventArgs;
using Gameboard.Objects;
using Gameboard.Helpers;

namespace Gameboard
{
    [RequireComponent(typeof(Gameboard))]
    [RequireComponent(typeof(UserPresenceController))]
    public class AssetController : MonoBehaviour
    {
        /// <summary>
        /// A dictionary of all companion assets that may be used by the companion. Key is the Guid of the asset, value is the companion asset.
        /// </summary>
        public Dictionary<Guid, ICompanionAsset> CompanionAssets { get; set; } = new Dictionary<Guid, ICompanionAsset>();

        /// <summary>
        /// A dictionary of all assets that have been loaded to each Companion. Key is UserID, Value is a List of LoadedCompanionAsset objects.
        /// </summary>
        public Dictionary<string, List<ICompanionAsset>> LoadedAssets { get; set; } = new Dictionary<string, List<ICompanionAsset>>();

        private UserPresenceController userPresenceController;
        private Gameboard gameboard => Gameboard.Instance;

        public delegate CompanionAssetType AddAsset<CompanionAssetType, UnityObjectType>(UnityObjectType resource)
            where CompanionAssetType : ICompanionAsset
            where UnityObjectType : UnityEngine.Object;

        private void Awake()
        {
            userPresenceController = gameboard.GetComponent<UserPresenceController>();
        }
        private void Start()
        {
            
        }

        public CompanionTextureAsset CreateTextureAsset(Texture2D resource)
        {
            return new CompanionTextureAsset(resource.EncodeToPNG(), this, resource.name);
        }

        public CompanionFBXAsset CreateFBXAsset(Mesh resource)
        {
            return new CompanionFBXAsset(resource, this, resource.name);
        }

        /// <summary>
        /// Loads a Texture asset onto a Companion.
        /// </summary>
        /// <param name="userId">the userId associated with the companion to send the asset to</param>
        /// <param name="textureAsset">a byte array of a png</param>
        /// <returns>Task<CompanionCreateObjectEventArgs></returns>
        public async Task<CompanionCreateObjectEventArgs> LoadAsset(string userId, byte[] textureBytes, string AssetGuid = "")
        {
            CompanionCreateObjectEventArgs response = await gameboard.services.companionHandler.LoadAsset(Gameboard.COMPANION_VERSION, userId, textureBytes, AssetGuid);
            return response;
        }

        /// <summary>
        /// Loads an FBX asset onto a Companion.
        /// </summary>
        /// <param name="userId">the userId associated with the companion to send the asset to</param>
        /// <param name="meshObject"></param>
        /// <returns>Task<CompanionCreateObjectEventArgs></returns>
        public async Task<CompanionCreateObjectEventArgs> LoadAsset(string userId, Mesh meshObject, string AssetGuid = "")
        {
            byte[] meshByteArray = GameboardHelperMethods.ObjectToByteArray(meshObject);
            var response = await gameboard.services.companionHandler.LoadAsset(Gameboard.COMPANION_VERSION, userId, meshByteArray, AssetGuid.ToString());

            return response;
        }

        /// <summary>
        /// Unloads an asset from memory on a Companion.
        /// </summary>
        /// <param name="userId">the userId associated with the companion to delete the asset from</param>
        /// <param name="assetUID"></param>
        /// <returns>Task<CompanionMessageResponseArgs></returns>
        public async Task<CompanionMessageResponseArgs> DeleteAsset(string userId, string AssetGuid)
        {
            CompanionMessageResponseArgs response = await gameboard.services.companionHandler.DeleteAsset(Gameboard.COMPANION_VERSION, userId, AssetGuid);
            return response;
        }


        /// <summary>
        /// Create companion assets that are loaded from a given local resource path.
        /// </summary>
        /// <typeparam name="CompanionAssetType">type of companion asset to create from the resources directory</typeparam>
        /// <typeparam name="UnityObjectType">type of the resource being loaded from the directory</typeparam>
        /// <param name="resourcePath">resource path used in Resources.LoadAll to load the assets from disk</param>
        /// <param name="addAsset">delegate for creating the companion asset</param>
        /// <returns>List<CompanionAssetType></returns>
        public List<CompanionAssetType> CreateCompanionAssetsFromPath<CompanionAssetType, UnityObjectType>(string resourcePath, AddAsset<CompanionAssetType, UnityObjectType> addAsset) 
            where CompanionAssetType: ICompanionAsset 
            where UnityObjectType: UnityEngine.Object
        {
            List<CompanionAssetType> assets = new List<CompanionAssetType>();
            var resources = Resources.LoadAll<UnityObjectType>(resourcePath);
            foreach (var resource in resources)
            {
                var newAsset = addAsset(resource);
                assets.Add(newAsset);
            }

            return assets;
        }


        /// <summary>
        /// Load all assets objects that were previously instantiated with CreateCompanionAssets 
        /// onto the companion app cor each connected user presence via the AssetController.
        /// </summary>
        /// <returns>Task<List<CompanionCreateObjectEventArgs>></returns>
        public async Task<List<CompanionCreateObjectEventArgs>> LoadAllAssetsOntoAllCompanions()
        {
            List<CompanionCreateObjectEventArgs> responses = new List<CompanionCreateObjectEventArgs>();
            foreach (var user in userPresenceController.Users.Values)
            {
                if (user.presenceTypeValue != DataTypes.PresenceType.COMPANION)
                    continue;

                foreach (var asset in CompanionAssets)
                {
                    CompanionCreateObjectEventArgs result = await asset.Value.LoadAssetToCompanion(userPresenceController, user.userId);
                    responses.Add(result);
                    GameboardLogging.Verbose($"Loaded assetId={asset.Key} for userId={user.userId}, result={result}");
                }
            }

            return responses;
        }
    }
}
