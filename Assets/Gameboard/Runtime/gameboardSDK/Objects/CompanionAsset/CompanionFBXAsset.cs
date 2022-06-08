using UnityEngine;
using Gameboard.EventArgs;
using System.Threading.Tasks;
using static Gameboard.EventArgs.CompanionAssetErrorResponse;

namespace Gameboard.Objects
{
    public class CompanionFBXAsset : CompanionAsset
    {
        public override CompanionAssetType CompanionAssetType => CompanionAssetType.FBX;

        public Mesh meshObject;

        public CompanionFBXAsset(Mesh meshObject, AssetController assetController, string name = "")
        {
            this.Name = name;
            this.meshObject = meshObject;
            this.AssetController = assetController;
            assetController.CompanionAssets.Add(this.AssetGuid, this);
        }

        public override async Task<CompanionCreateObjectEventArgs> LoadAssetToCompanion(UserPresenceController UserPresenceController, string userId)
        {
            if (!UserPresenceController.Users.TryGetValue(userId, out var userPresence) || userPresence.presenceTypeValue != DataTypes.PresenceType.COMPANION)
            {
                var errorMessage = $"Failed to get a companion user before loading companion FBX asset for userId={userId}";
                GameboardLogging.Warning(errorMessage);
                return new CompanionCreateObjectEventArgs() { errorResponse = new CompanionAssetErrorResponse() { ErrorValue = AssetErrorTypes.NoCompanionAssetFoundAtUID, } };
            }

            var response = await AssetController.LoadAsset(userId, meshObject);
            ProcessLoadAssetResponse(response, userId);
            return response;
        }
    }

}
