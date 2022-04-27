using System.Collections.Generic;

namespace Gameboard.EventArgs
{
    public class CompanionUserPresenceEventArgs : CompanionMessageResponseArgs
    {
        public List<GameboardUserPresenceEventArgs> playerPresenceList = new List<GameboardUserPresenceEventArgs>();
    }
}