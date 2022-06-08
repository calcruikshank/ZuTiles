using Gameboard.EventArgs;
using System.Threading.Tasks;
using static Gameboard.EventArgs.CompanionAssetErrorResponse;

namespace Gameboard.Objects
{
    public class CompanionTextureAsset : CompanionAsset
    {
        public override CompanionAssetType CompanionAssetType => CompanionAssetType.Texture;

        public byte[] textureBytes;

        public CompanionTextureAsset(byte[] textureBytes, AssetController assetController, string name = "")
        {
            this.Name = name;
            this.textureBytes = textureBytes;
            this.AssetController = assetController;
            assetController.CompanionAssets.Add(this.AssetGuid, this);
        }

        public override async Task<CompanionCreateObjectEventArgs> LoadAssetToCompanion(UserPresenceController UserPresenceController, string userId)
        {
            if (!UserPresenceController.Users.TryGetValue(userId, out var userPresence) || userPresence.presenceTypeValue != DataTypes.PresenceType.COMPANION)
            {
                var errorMessage = $"Failed to get a companion user before loading companion texture asset for userId={userId}";
                GameboardLogging.Warning(errorMessage);
                return new CompanionCreateObjectEventArgs() { errorResponse = new CompanionAssetErrorResponse() { ErrorValue = AssetErrorTypes.NoCompanionAssetFoundAtUID, } };
            }

            var response = await AssetController.LoadAsset(userId, textureBytes, AssetGuid.ToString());
            ProcessLoadAssetResponse(response, userId);
            return response;
        }
    }

}
