using Gameboard.EventArgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gameboard.Objects
{
    public interface ICompanionAsset
    {
        CompanionAssetType CompanionAssetType { get; }
        Guid AssetGuid { get; }
        string Name { get; set; }
        Task<CompanionCreateObjectEventArgs> LoadAssetToCompanion(UserPresenceController UserPresenceController, string userId);
        Task<CompanionMessageResponseArgs> DeleteAssetFromCompanion(UserPresenceController UserPresenceController, string userId);
        Task<List<CompanionMessageResponseArgs>> DeleteAssetFromAllCompanions(UserPresenceController UserPresenceController, List<string> userIdAllowList);
    }
}
