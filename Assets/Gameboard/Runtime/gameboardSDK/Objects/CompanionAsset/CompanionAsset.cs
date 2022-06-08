using Gameboard.EventArgs;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Gameboard.EventArgs.CompanionAssetErrorResponse;
using System;

namespace Gameboard.Objects
{
    public abstract class CompanionAsset : ICompanionAsset
    {
        // TODO: there were some additional things in CompanionAssetsTool.cs that have not been implemented yet
        //      maybe retain using the path as the asset id??
        //      could possibly also store a hash of the images here instead of using guid and doing a lot of the roundabout checks

        public abstract CompanionAssetType CompanionAssetType { get; }
        private Guid assetGuid;
        
        /// <summary>
        /// Guid associated with the asset used to identify
        /// </summary>
        public Guid AssetGuid
        { 
            get 
            {
                if (assetGuid == Guid.Empty)
                    assetGuid = Guid.NewGuid();
                return assetGuid;
            } 
        }
        protected AssetController AssetController { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Load this asset to the companion app with the specified userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public abstract Task<CompanionCreateObjectEventArgs> LoadAssetToCompanion(UserPresenceController UserPresenceController, string userId);

        /// <summary>
        /// Delete this asset from the companion app with the specified userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> DeleteAssetFromCompanion(UserPresenceController UserPresenceController, string userId)
        {
            if (!UserPresenceController.Users.TryGetValue(userId, out var userPresence) || userPresence.presenceTypeValue != DataTypes.PresenceType.COMPANION)
            {
                var errorMessage = $"Failed to get a companion user before loading companion assets for userId={userId}";
                GameboardLogging.Warning(errorMessage);
                return new CompanionMessageResponseArgs() { errorResponse = new CompanionAssetErrorResponse() { ErrorValue = AssetErrorTypes.NoCompanionAssetFoundAtUID, } };
            }

            return await DeleteAssetFromCompanionUser(userId);
        }

        /// <summary>
        /// Delete this asset from all companion users except the users specified in the allowlist.
        /// </summary>
        /// <param name="userIdAllowList">A list of users to skip when deleting the asset from all companions.</param>
        /// <returns></returns>
        public async Task<List<CompanionMessageResponseArgs>> DeleteAssetFromAllCompanions(UserPresenceController UserPresenceController, List<string> userIdAllowList = null)
        {
            List<CompanionMessageResponseArgs> responses = new List<CompanionMessageResponseArgs>();
            foreach(var user in UserPresenceController.Users)
            {
                if ((bool)(userIdAllowList?.Contains(user.Key)))
                    continue;

                if (user.Value.presenceTypeValue != DataTypes.PresenceType.COMPANION)
                    continue;

                responses.Add(await DeleteAssetFromCompanionUser(user.Key));
            }

            return responses;
        }

        /// <summary>
        /// Run the DeleteAsset method in the companion app to remove the asset from the companion user with the specified userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        protected async Task<CompanionMessageResponseArgs> DeleteAssetFromCompanionUser(string userId)
        {
            var response = await AssetController.DeleteAsset(userId, AssetGuid.ToString());
            if (response.wasSuccessful)
            {
                if (AssetController.LoadedAssets.TryGetValue(userId, out var assets))
                    if (!assets.Remove(this))
                        GameboardLogging.Error($"Response was successful but failed to remove from LoadedAssets with assetGuid={this.AssetGuid} from user with userId={userId}");
            }

            return response;
        }

        /// <summary>
        /// Process the response from the companion create event, and add the card to the loaded assets dictionary if it was successful.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="userId"></param>
        protected void ProcessLoadAssetResponse(CompanionCreateObjectEventArgs response, string userId)
        {
            if (response.wasSuccessful)
            {
                if (AssetController.LoadedAssets.TryGetValue(userId, out var assets))
                    assets.Add(this);
                else
                    AssetController.LoadedAssets.Add(userId, new List<ICompanionAsset>() { this });
            }
        }
    }
}
