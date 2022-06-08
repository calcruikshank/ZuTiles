using System.Linq;

namespace Gameboard.Examples
{
    public static class Utils
    {
        public static string GetFirstCompanionUserId(UserPresenceController userPresenceController)
        {
            string firstUserId = string.Empty;
            var companionUsers = userPresenceController.Users
                .Where(u => u.Value.presenceTypeValue == DataTypes.PresenceType.COMPANION);
            if (companionUsers.Count() > 0)
            {
                var companionUSer = companionUsers.First();
                firstUserId = companionUsers.First().Key;
            }

            return firstUserId;
        }
    }
}
